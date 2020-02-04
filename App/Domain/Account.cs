using System;

namespace App.Domain
{
    public class Account
    {
        private const decimal PayInLimitThreshold = 500m;
        private const decimal FundsLowThreshold = 500m;

        public decimal PayInLimit => 4000m;
        public Guid Id { get; internal set; }
        public User User { get; internal set; }
        public decimal Balance { get; internal set; }
        public decimal Withdrawn { get; internal set; }
        public decimal PaidIn { get; internal set; }

        public EventHandler OnFundsLow;
        public EventHandler OnPayInLimitReached;

        public void Withdraw(decimal amount)
        {
            var estimatedBalance = Balance - amount;
            if (estimatedBalance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }

            if (estimatedBalance < FundsLowThreshold)
            {
                OnFundsLow?.Invoke(this, null);
            }

            Balance -= amount;
            Withdrawn -= amount;
        }

        public void PayIn(decimal amount)
        {
            var estimatedPaidIn = PaidIn + amount;
            if (estimatedPaidIn > PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }

            if (PayInLimit - estimatedPaidIn < PayInLimitThreshold)
            {
                OnPayInLimitReached?.Invoke(this, null);
            }

            PaidIn += amount;
            Balance += amount;
        }
    }
}
