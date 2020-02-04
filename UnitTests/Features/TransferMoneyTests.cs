using System;
using Moq;
using NUnit.Framework;
using App.DataAccess;
using App.Domain;
using App.Domain.Services;
using App.Features;

namespace UnitTests.Features
{
    public class TransferMoneyTests
    {
        private const decimal PayInLimit = 4000m;
        private const decimal FundsLowNotificationThreshold = 500m;
        private const decimal ReachingPayInLimitNotificationThreshold = 500m;
        private Guid _fromAccountId;
        private Guid _toAccountId;
        private User _fromUser;
        private User _toUser;
        Account _fromAccount;
        Account _toAccount;
        private Mock<IAccountRepository> _accountRepository;
        private Mock<INotificationService> _notificationService;

        [SetUp]
        public void SetUp()
        {
            _fromAccountId = Guid.NewGuid();
            _toAccountId = Guid.NewGuid();
            _fromUser = new User {Email = "a@email.com"};
            _toUser = new User {Email = "b@email.com"};
            _fromAccount = new Account
            {
                Id = _fromAccountId,
                User = _fromUser
            };
            _toAccount = new Account
            {
                Id = _toAccountId,
                User = _toUser
            };
            _accountRepository = new Mock<IAccountRepository>();
            _notificationService = new Mock<INotificationService>();
            _accountRepository.Setup(a => a.GetAccountById(_fromAccountId)).Returns(_fromAccount);
            _accountRepository.Setup(a => a.GetAccountById(_toAccountId)).Returns(_toAccount);
            _notificationService.Setup(n => n.NotifyFundsLow(_fromAccount.User.Email));
            _notificationService.Setup(n => n.NotifyApproachingPayInLimit(_toAccount.User.Email));
        }

        [Test]
        public void Execute_BalanceIsLessThanAmountToWithdraw_ThrowInvalidOperationException()
        {
            _fromAccount.Balance = 1m;
            var sut = new TransferMoney(_accountRepository.Object, _notificationService.Object);

            Assert.Throws<InvalidOperationException>(() => sut.Execute(_fromAccountId, _toAccountId, 2m));
        }

        [Test]
        public void Execute_BalanceBecomesLowerThanThreshold_SendNotification()
        {
            _fromAccount.Balance = FundsLowNotificationThreshold;
            var sut = new TransferMoney(_accountRepository.Object, _notificationService.Object);

            sut.Execute(_fromAccountId, _toAccountId, 1m);

            _notificationService.Verify(n => n.NotifyFundsLow(_fromUser.Email));
        }

        [Test]
        public void Execute_ExceedPayInLimit_ThrowInvalidOperationException()
        {
            _toAccount.Balance = PayInLimit;
            var sut = new TransferMoney(_accountRepository.Object, _notificationService.Object);

            Assert.Throws<InvalidOperationException>(() => sut.Execute(_fromAccountId, _toAccountId, 1m));
        }

        [Test]
        public void Execute_ApproachPayInLimit4000_SendNotification()
        {
            _fromAccount.Balance = 1m;
            _toAccount.PaidIn = PayInLimit - ReachingPayInLimitNotificationThreshold;
            var sut = new TransferMoney(_accountRepository.Object, _notificationService.Object);

            sut.Execute(_fromAccountId, _toAccountId, 1m);

            _notificationService.Verify(n => n.NotifyApproachingPayInLimit(_toUser.Email));
        }

        [Test]
        public void Execute_WhenCalled_TransferMoneyCorrectly()
        {
            _fromAccount.Withdrawn = 10m;
            _fromAccount.PaidIn = 20m;
            _fromAccount.Balance = 10m;
            _toAccount.Withdrawn = 1m;
            _toAccount.PaidIn = 2m;
            _toAccount.Balance = 1m;
            var sut = new TransferMoney(_accountRepository.Object, _notificationService.Object);

            sut.Execute(_fromAccountId, _toAccountId, 1m);

            Assert.Multiple(() =>
            {
                Assert.That(_fromAccount.Withdrawn, Is.EqualTo(9m));
                Assert.That(_fromAccount.PaidIn, Is.EqualTo(20m));
                Assert.That(_fromAccount.Balance, Is.EqualTo(9m));
                Assert.That(_toAccount.Withdrawn, Is.EqualTo(1m));
                Assert.That(_toAccount.PaidIn, Is.EqualTo(3m));
                Assert.That(_toAccount.Balance, Is.EqualTo(2m));
            });
        }

        [Test]
        public void Execute_WhenCalled_UpdateAccountRepository()
        {
            _fromAccount.Balance = 1m;
            _accountRepository.Setup(a => a.Update(_fromAccount));
            _accountRepository.Setup(a => a.Update(_toAccount));
            var sut = new TransferMoney(_accountRepository.Object, _notificationService.Object);

            sut.Execute(_fromAccountId, _toAccountId, 1m);
            
            _accountRepository.Verify(a => a.Update(_fromAccount));
            _accountRepository.Verify(a => a.Update(_toAccount));
        }
    }
}
