using Newtonsoft.Json;

namespace UwpApp.Models
{
    public class Person
    {
        // För XamlSerializer
        public Person() { }

        public Person(string firstName, string lastName, string age, string streetAddress, string zipCode, string city)
        {
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            StreetAddress = streetAddress;
            ZipCode = zipCode;
            City = city;
        }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "age")]
        public string Age { get; set; }

        [JsonProperty(PropertyName = "streetAddress")]
        public string StreetAddress { get; set; }

        [JsonProperty(PropertyName = "zipCode")]
        public string ZipCode { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
    }
}