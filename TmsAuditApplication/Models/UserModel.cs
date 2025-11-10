namespace VrsAuditApplication.Models
{
    public class UserModel
    {
        // General Information
        public string Saluation { get; set; } // Mr., Ms., Mrs., Dr., etc.
        public string LoginCardNumber { get; set; } // Unique identifier for login card
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; } // Male or Female
        public string ContactNumber { get; set; }
        public string DocumentType { get; set; } // Passport, Driver's License, Aadhar Card, etc.
        public string DocumentNumber { get; set; } // Unique identifier for the document
        public string JobPosition { get; set; } // TC Collector, Driver, Manager, etc.
        public string BlacklistStatus { get; set; } // Yes or No
        public string PdfDocument { get; set; } // Base64 encoded PDF document

        // Address Details

        public string City { get; set; }
        public string PinCode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string HouseApartmentNo { get; set; }
        public string Floor { get; set; }
    }

}
