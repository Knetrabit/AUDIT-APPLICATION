namespace VrsAuditApplication.Models
{
    public class PendingCashupDetail
    {
        public string UserName { get; set; }       // Example: "38000060"
        public string BagNo { get; set; }          // Example: "41809"
        public string Shift { get; set; }          // Example: "A"
        public decimal FloatAmount { get; set; }   // Example: 100
        public DateTime OperationDay { get; set; } // Example: 11-Sep-2025
        public DateTime FloatDateTime { get; set; }// Example: 09-Sep-2025 16:43:20
        public string UserID { get; set; }         // Example: "38000060"

    }
}
