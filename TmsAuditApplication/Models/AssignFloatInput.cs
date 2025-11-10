namespace VrsAuditApplication.Models
{
    public class AssignFloatInput
    {
        public string UserID { get; set; }
        public int BagNumber { get; set; }
        public double FloatAmount { get; set; }
        public string AssignerName { get; set; }
        public DateTime OperationDay { get; set; }
        public string Shift { get; set; }

    }
}
