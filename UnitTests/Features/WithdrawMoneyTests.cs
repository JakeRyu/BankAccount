using System;
using App.DataAccess;
using App.Domain;
using App.Domain.Services;
using App.Features;
using Moq;
using NUnit.Framework;

namespace UnitTests.Features
{
    public class WithdrawMoneyTests
    {
        private Guid _fromAccountId;
        private User _fromUser;
        Account _fromAccount;
        private Mock<IAccountRepository> _accountRepository;
        private Mock<INotificationService> _notificationService;

        [SetUp]
        public void SetUp()
        {
            _fromAccountId = Guid.NewGuid();
            _fromUser = new User { Email = "a@email.com" };
            _fromAccount = new Account
            {
                Id = _fromAccountId,
                User = _fromUser
            };
            _accountRepository = new Mock<IAccountRepository>();
            _notificationService = new Mock<INotificationService>();
            _accountRepository.Setup(a => a.GetAccountById(_fromAccountId)).Returns(_fromAccount);
            _notificationService.Setup(n => n.NotifyFundsLow(_fromAccount.User.Email));
        }

        [Test]
        public void Execute_BalanceIsLessThanAmountToWithdraw_ThrowInvalidOperationException()
        {
            _fromAccount.Balance = 1m;
            var sut = new WithdrawMoney(_accountRepository.Object, _notificationService.Object);

            Assert.Throws<InvalidOperationException>(() => sut.Execute(_fromAccountId, 2m));
        }

        [Test]
        public void Execute_BalanceBecomesLowerThanThreshold500_SendNotification()
        {
            _fromAccount.Balance = 500m;
            var sut = new WithdrawMoney(_accountRepository.Object, _notificationService.Object);

            sut.Execute(_fromAccountId, 1m);

            _notificationService.Verify(n => n.NotifyFundsLow(_fromUser.Email));
        }

        [Test]
        public void Execute_WhenCalled_WithdrawMoneyCorrectly()
        {
            _fromAccount.Withdrawn = 10m;
            _fromAccount.PaidIn = 20m;
            _fromAccount.Balance = 10m;
            var sut = new WithdrawMoney(_accountRepository.Object, _notificationService.Object);

            sut.Execute(_fromAccountId, 1m);

            Assert.Multiple(() =>
            {
                Assert.That(_fromAccount.Withdrawn, Is.EqualTo(9m));
                Assert.That(_fromAccount.PaidIn, Is.EqualTo(20m));
                Assert.That(_fromAccount.Balance, Is.EqualTo(9m));
            });
        }

        [Test]
        public void Execute_WhenCalled_UpdateAccountRepository()
        {
            _fromAccount.Balance = 1m;
            _accountRepository.Setup(a => a.Update(_fromAccount));
            var sut = new WithdrawMoney(_accountRepository.Object, _notificationService.Object);

            sut.Execute(_fromAccountId, 1m);

            _accountRepository.Verify(a => a.Update(_fromAccount));
        }
    }
}
