using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UwpApp.Models;
using UwpApp.Services;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UwpApp
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Person> peopleToExport = new ObservableCollection<Person>();
        public ObservableCollection<Person> importedPeopleList = new ObservableCollection<Person>();
        public List<Person> fileContents = new List<Person>();

        private static readonly ContentDialog noFileSelectedDialog = new ContentDialog()
        {
            Title = "No file selected",
            Content = "No file selected. Make sure that the field contains a valid path and that you have permission to read it.",
            CloseButtonText = "Ok"
        };

        public static TextBlock tbSaveStatus;
        private static List<TextBox> textBoxes;
        private static StorageFile selectedFile;

        public MainPage()
        {
            InitializeComponent();

            textBoxes = new List<TextBox>()
            {
                tbFirstName, tbLastName, tbAge,
                tbStreetAddress, tbZipCode, tbCity
            };

            tbSaveStatus = tbFileSaveStatus;
        }

        // Funktion för att rensa input-fält
        private void clearInputFields()
            => textBoxes.ForEach(tb => tb.Text = "");

        // -- [IMPORT / LEFT SIDE] --

        // Välj fil m.h.a. FileService
        private async void btnBrowseFiles_Click(object sender, RoutedEventArgs e)
            => (tbPathToFile.Text, selectedFile) = await FileService.BrowseFilesAsync();

        // Läs in fil m.h.a. FileService
        private async void btnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            if (tbPathToFile.Text.Length > 1)
            {
                fileContents = await FileService.OpenFileAsync(selectedFile);

                if (fileContents != null)
                {
                    importedPeopleList.Clear();
                    fileContents.ForEach((person) => importedPeopleList.Add(person));
                }
            }
            else
                await noFileSelectedDialog.ShowAsync();
        }

        // Rensa lista med importerad person-data
        private void btnClearImportedList_Click(object sender, RoutedEventArgs e)
            => importedPeopleList.Clear();

        // Raderingsknapp vid varje ListItem
        private void btnRemoveImportItem_Click(object sender, RoutedEventArgs e)
            => importedPeopleList.Remove(((FrameworkElement)sender).DataContext as Person);
        /* ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
         * var buttonParent = sender as FrameworkElement;
         * var context = buttonParent.DataContext;
         * var person = context as Person;
         */

        // -- [EXPORT / RIGHT SIDE] --
        private void btnAddToList_Click(object sender, RoutedEventArgs e)
        {
            peopleToExport.Insert(0, new Person(tbFirstName.Text, tbLastName.Text, tbAge.Text, tbStreetAddress.Text, tbZipCode.Text, tbCity.Text));
            clearInputFields();
        }

        // Rensa input-fälten
        private void btnClearInput_Click(object sender, RoutedEventArgs e)
            => clearInputFields();

        // Rensa lista med sparad person-data
        private void btnClearExportList_Click(object sender, RoutedEventArgs e)
            => peopleToExport.Clear();

        // Exportera lista med sparade person-data till fil m.h.a. FileService
        private void btnExportList_Click(object sender, RoutedEventArgs e)
            => FileService.SaveToFileAsync(peopleToExport.ToList()).GetAwaiter();

        // Raderingsknapp vid varje ListItem
        private void btnRemoveExportItem_Click(object sender, RoutedEventArgs e)
            => peopleToExport.Remove(((FrameworkElement)sender).DataContext as Person);
        /* ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
* var buttonParent = sender as FrameworkElement;
* var context = buttonParent.DataContext;
* var person = context as Person;
*/
    }
}
