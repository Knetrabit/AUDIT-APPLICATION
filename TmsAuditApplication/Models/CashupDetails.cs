namespace VrsAuditApplication.Models
{
    public class CashupDetails
    {
        public string UserID { get; set; }                // VARCHAR(30)
        public DateTime StartOfShiftDateTime { get; set; } // DATETIME
        public DateTime EndOfShiftDateTime { get; set; }   // DATETIME
        public int Bagnumber { get; set; }                 // INT
        public int Shiftnnumber { get; set; }              // INT

        public int CoinOf5Rupees { get; set; }             // INT
        public int CoinOf2Rupees { get; set; }             // INT
        public int CoinOf1Rupee { get; set; }              // INT

        public int NoteOf1Rupee { get; set; }              // INT
        public int NoteOf2Rupees { get; set; }             // INT
        public int NoteOf5Rupees { get; set; }             // INT
        public int NoteOf10Rupees { get; set; }            // INT
        public int NoteOf20Rupees { get; set; }            // INT
        public int NoteOf50Rupees { get; set; }            // INT
        public int NoteOf100Rupees { get; set; }           // INT
        public int NoteOf200Rupees { get; set; }           // INT
        public int NoteOf500Rupees { get; set; }           // INT
        public int NoteOf1000Rupees { get; set; }          // INT
        public int NoteOf2000Rupees { get; set; }          // INT

        public double CheckDraftAmount { get; set; }       // FLOAT
        public double WalletAmount { get; set; }           // FLOAT

        public decimal TCashupAmount { get; set; }         // DECIMAL(18,2)
        public decimal CashupDeclareAmount { get; set; }   // DECIMAL(18,2)
        public decimal FloatAmount { get; set; }           // DECIMAL(18,2)

        public decimal BeforValidationtionSystemAmount { get; set; } // DECIMAL(18,3)
        public decimal Shortage { get; set; }              // DECIMAL(18,3)
        public decimal Excess { get; set; }                // DECIMAL(18,3)
        public decimal AfterValidationSystemAmount { get; set; } // DECIMAL(18,3)
        public decimal ExcessShortage { get; set; }        // DECIMAL(18,3)
        public decimal BleedAmount { get; set; }           // DECIMAL(18,3)
        public decimal TSystemWIMamount { get; set; }      // DECIMAL(18,3)
        public decimal Paytm { get; set; }                 // DECIMAL(18,3)
        public decimal Wimonle { get; set; }               // DECIMAL(18,3)

        public int ShiftTypeID { get; set; }               // INT
    }

}
