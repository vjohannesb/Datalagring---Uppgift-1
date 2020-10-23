using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UwpApp.Models;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml.Controls;

namespace UwpApp.Services
{
    public class FileService
    {
        private static readonly string _delim = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        private static readonly ContentDialog incorrectFileExtensionDialog = new ContentDialog()
        {
            Title = "File could not be saved",
            Content = "Either access to the file is limited or the file format is unavailable. "
                    + "Accepted export formats are json, csv, xml and plain text (.txt).",
            CloseButtonText = "Ok"
        };

        private static readonly ContentDialog fileNotProcessedDialog = new ContentDialog()
        {
            Title = "File could not be processed",
            Content = "Either access to the file is limited or the formatting is incorrect. \n"
                    + "JSON files should follow proper json formatting rules. \n"
                    + "XML files should be structured like an exported file. \n"
                    + "CSV fields should be separated by either ',' or ';'. \n"
                    + "Text files should be structured similarly to CSV files.",
            CloseButtonText = "Ok"
        };

        private static readonly FileSavePicker savePicker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = "exported-list",
            FileTypeChoices =
                {
                    { "Any", new List<string>() {".json", ".csv", ".xml", ".txt"} },
                    { "JSON", new List<string>() {".json"} },
                    { "CSV", new List<string>() {".csv"} },
                    { "XML", new List<string>() {".xml"} },
                    { "Text", new List<string>() {".txt"} }
                }
        };

        private static readonly FileOpenPicker openPicker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            ViewMode = PickerViewMode.List,
            FileTypeFilter = { ".json", ".csv", ".xml", ".txt" }
        };

        // -- [IMPORT / OPEN + READ FILES] --
        public static async Task<(string, StorageFile)> BrowseFilesAsync()
        {
            var file = await openPicker.PickSingleFileAsync();
            return (file?.Path ?? string.Empty, file);
        }

        public static async Task<List<Person>> OpenFileAsync(StorageFile file)
        {
            var persons = await ConvertTextToData(file);

            if (persons == null)
                await fileNotProcessedDialog.ShowAsync();

            return persons;
        }

        private static async Task<List<Person>> ConvertTextToData(StorageFile file)
        {
            var contentList = await FileIO.ReadLinesAsync(file);
            string contentString = string.Concat(contentList);

            switch (file.FileType)
            {
                case ".json":
                    return ConvertFromJson(contentString);
                case ".csv":
                    return ConvertFromCsv(contentList);
                case ".xml":
                    return ConvertFromXml(contentString);
                case ".txt":
                    return ConvertFromTxt(contentList);
                default:
                    await incorrectFileExtensionDialog.ShowAsync();
                    return null;
            }
        }

        // Konverteringsfunktioner
        private static List<Person> ConvertFromJson(string data)
        {
            try { return JsonConvert.DeserializeObject<List<Person>>(data); }
            catch { return null; }
        }

        private static List<Person> ConvertFromCsv(IList<string> lines)
        {
            try
            {
                var persons = new List<Person>();
                var _csvDelimiter = lines[0].Contains(",") ? "," : ";";

                foreach (var line in lines)
                {
                    var data = line.Split(_csvDelimiter);
                    persons.Add(new Person(data[0], data[1], data[2], data[3], data[4], data[5]));
                }
                return persons;
            }
            catch { return null; }
        }

        private static List<Person> ConvertFromXml(string data)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Person>));
                return serializer.Deserialize(new StringReader(data)) as List<Person>;
            }
            catch { return null; }
        }

        private static List<Person> ConvertFromTxt(IList<string> lines)
            => ConvertFromCsv(lines);


        // -- [EXPORT / SAVE FILES] --
        public static async Task SaveToFileAsync(List<Person> exportData)
        {
            StorageFile file = await savePicker.PickSaveFileAsync();

            // Avbryt om fil är null
            if (file == null)
                return;

            // Förhindra uppdateringar av filen
            CachedFileManager.DeferUpdates(file);

            // Konvertera data till text m.h.a. funktioner nedan
            string data = await ConvertDataToText(exportData, file);

            await FileIO.WriteTextAsync(file, data, Windows.Storage.Streams.UnicodeEncoding.Utf8);

            // Slutför filändringar och kolla om filen existerar
            var status = await CachedFileManager.CompleteUpdatesAsync(file);

            // Uppdatera informationsruta i UI
            if (status == FileUpdateStatus.Complete)
                MainPage.tbSaveStatus.Text = $"File {file.Name} has been saved.";
            else
                MainPage.tbSaveStatus.Text = $"File {file.Name} couldn't be saved.";
        }

        private static async Task<string> ConvertDataToText(List<Person> exportData, StorageFile file)
        {
            switch (file.FileType)
            {
                case ".json":
                    return ConvertToJson(exportData);
                case ".csv":
                    return ConvertToCsv(exportData);
                case ".xml":
                    return ConvertToXml(exportData);
                case ".txt":
                    return ConvertToText(exportData);
                default:
                    await incorrectFileExtensionDialog.ShowAsync();
                    return null;
            }
        }

        // Konverteringsfunktioner
        private static string ConvertToJson(List<Person> data)
            => JsonConvert.SerializeObject(data);

        private static string ConvertToCsv(List<Person> data)
        {
            string csvString = "First name" + _delim
                             + "Last name" + _delim
                             + "Age" + _delim
                             + "Street address" + _delim
                             + "Zip code" + _delim
                             + "City" + "\n";

            foreach (var person in data)
            {
                csvString += person.FirstName + _delim
                           + person.LastName + _delim
                           + person.Age + _delim
                           + person.StreetAddress + _delim
                           + person.ZipCode + _delim
                           + person.City + "\n";
            }

            return csvString;
        }

        private static string ConvertToXml(List<Person> data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = ("  "),
                CloseOutput = true
            };

            using XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings);
            XmlSerializer serializer = new XmlSerializer(typeof(List<Person>));

            serializer.Serialize(xmlWriter, data);

            return stringBuilder.ToString();
        }

        private static string ConvertToText(List<Person> data)
        {
            string text = string.Empty;

            foreach (var person in data)
            {
                text += person.FirstName + ","
                      + person.LastName + ","
                      + person.Age + ","
                      + person.StreetAddress + ","
                      + person.ZipCode + ","
                      + person.City
                      + "\n";
            }
            return text;
        }
    }
}