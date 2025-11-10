namespace VrsAuditApplication.Models
{
    public class CashupRequest
    {
        public string UserName { get; set; }
        public int BagNumber { get; set; }
        public string Shift { get; set; }
        public DateTime OperationDay { get; set; }
        public CashDenomination Cash { get; set; }  // Nested model
        public int NEFT_CHEQUE_CARD_Amount { get; set; } 

    }

}
