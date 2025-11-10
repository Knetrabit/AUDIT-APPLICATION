namespace VrsAuditApplication.Models
{
    public class Person
    {
        public int PersonID { get; set; }
        public int PlazaID { get; set; }
        public string JobPositionID { get; set; }
        public int IsPhysicalPerson { get; set; }
        public string? Salutation { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public int IDType { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string? BirthPlace { get; set; }
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Phone2 { get; set; }
        public string? Fax { get; set; }
        public string? MobilePhone { get; set; }
        public string Email { get; set; } = string.Empty;
        public int PersonAddressID { get; set; }
        public bool Active { get; set; }
        public bool SmsAlert { get; set; }
        public bool EmailAlert { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string PasswordChangedBy { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public int Status { get; set; }
    }


}
