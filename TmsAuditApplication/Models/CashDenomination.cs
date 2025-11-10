namespace VrsAuditApplication.Models
{
    public class CashDenomination
    {
        public int INR2000 { get; set; }
        public int INR500 { get; set; }
        public int INR200 { get; set; }
        public int INR100 { get; set; }
        public int INR50 { get; set; }
        public int INR20 { get; set; }
        public int INR10 { get; set; }
        public int Coin5 { get; set; }
        public int Coin2 { get; set; }
        public int Coin1 { get; set; }


        // Calculates total amount
        public int TotalAmount()
        {
            return INR2000 * 2000 +
                   INR500 * 500 +
                   INR200 * 200 +
                   INR100 * 100 +
                   INR50 * 50 +
                   INR20 * 20 +
                   INR10 * 10 +
                   Coin5 * 5 +
                   Coin2 * 2 +
                   Coin1 * 1;
        }

        public int TotalCoinsAmount()
        {
            return Coin5 * 5 +
                   Coin2 * 2 +
                   Coin1 * 1;
        }


    }

}
