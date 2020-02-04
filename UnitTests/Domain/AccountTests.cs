using System;
using App.Domain;
using NUnit.Framework;

namespace UnitTests.Domain
{
    public class AccountTests
    {
        private const decimal PayInLimit = 4000m;
        private const decimal FundsLowNotificationThreshold = 500m;
        private const decimal ReachingPayInLimitNotificationThreshold = 500m;

        [Test]
        public void Withdraw_BalanceIsLessThanAmountToWithdraw_ThrowInvalidOperationException()
        {
            var sut = new Account
            {
                Balance = 1m
            };

            Assert.Throws<InvalidOperationException>(() => sut.Withdraw(2m));
        }

        [Test]
        public void Withdraw_BalanceBecomesLowerThanThreshold_RaiseOnFundsLowEvent()
        {
            var sut = new Account
            {
                Balance = FundsLowNotificationThreshold
            };
            bool isEventRaised = false;
            sut.OnFundsLow += (sender, args) => isEventRaised = true;

            sut.Withdraw(1);

            Assert.That(isEventRaised, Is.True);
        }

        [Test]
        public void Withdraw_BalanceIsEnough_DecreaseBalanceAndWithdraw()
        {
            var sut = new Account
            {
                Balance = 1m,
                Withdrawn = 0m
            };

            sut.Withdraw(1m);

            Assert.That(sut.Balance, Is.EqualTo(0));
            Assert.That(sut.Withdrawn, Is.EqualTo(-1));
        }

        [Test]
        public void PayIn_ExceedPayInLimit_ThrowInvalidOperationException()
        {
            var sut = new Account
            {
                PaidIn = PayInLimit
            };

            Assert.Throws<InvalidOperationException>(() => sut.PayIn(1m));
        }

        [Test]
        public void PayIn_PayInLimitReached_RaiseOnPayInLimitReachedEvent()
        {
            var sut = new Account
            {
                PaidIn = PayInLimit - ReachingPayInLimitNotificationThreshold
            };
            bool isEventRaised = false;
            sut.OnPayInLimitReached += (sender, args) => isEventRaised = true;

            sut.PayIn(1m);

            Assert.That(isEventRaised, Is.True);
        }

        [Test]
        public void PayIn_NotExceedPayInLimit_IncreaseBalanceAndPaidIn()
        {
            var sut = new Account
            {
                Balance = 1m,
                PaidIn = 0m
            };

            sut.PayIn(1m);

            Assert.That(sut.Balance, Is.EqualTo(2m));
            Assert.That(sut.PaidIn, Is.EqualTo(1m));
        }
    }
}
