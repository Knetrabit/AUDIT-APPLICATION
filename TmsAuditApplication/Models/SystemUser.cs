using System;

namespace VrsAuditApplication.Models
{
    public class SystemUser
    {
        public string UserID { get; set; }           // varchar(32)
        public byte PlazaID { get; set; }            // tinyint
        public int? PersonID { get; set; }           // int (nullable)
        public string JobPositionID { get; set; }    // varchar(32), nullable
        public string PasswordX { get; set; }        // varchar(60)
        public string Password { get; set; }         // varchar(60)
        public bool Active { get; set; }             // bit
        public bool? RequestPasswordChange { get; set; } // bit (nullable)
        public int? PaymentMeansID { get; set; }     // int (nullable)
        public DateTime? PasswordChangeDate { get; set; } // datetime (nullable)
        public bool Login { get; set; }              // bit
        public string SecurityKey { get; set; }      // varchar(250), nullable
        public DateTime? PasswordExpiryDateTime { get; set; } // datetime (nullable)
        public DateTime CreatedDate { get; set; }    // datetime
        public DateTime ModifiedDate { get; set; }   // datetime
        public bool Status { get; set; }             // bit
    }

}
