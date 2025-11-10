namespace VrsAuditApplication.Models
{
    public class ShiftDetails
    {
        public string ShiftNumber { get; set; }
        public string ShiftOrigin { get; set; }
        public string UserName { get; set; }
        public DateTime StartOfShiftTime { get; set; }
        public DateTime EndOfShiftTime { get; set; }
        public int BagNumber { get; set; }
        public DateTime OperationDay { get; set; }
    }

}
