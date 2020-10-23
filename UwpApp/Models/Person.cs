namespace UwpApp.Models
{
    public class Person
    {
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

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Age { get; set; }
        public string StreetAddress { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
    }
}