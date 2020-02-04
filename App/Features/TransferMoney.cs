using System;
using App.DataAccess;
using App.Domain.Services;

namespace App.Features
{
    public class TransferMoney
    {
        private readonly IAccountRepository _accountRepository;
        private readonly INotificationService _notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            _accountRepository = accountRepository;
            _notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = _accountRepository.GetAccountById(fromAccountId);
            var to = _accountRepository.GetAccountById(toAccountId);

            from.OnFundsLow += (sender, args) => _notificationService.NotifyFundsLow(from.User.Email);
            to.OnPayInLimitReached += (sender, args) => _notificationService.NotifyApproachingPayInLimit(to.User.Email);

            from.Withdraw(amount);
            to.PayIn(amount);

            _accountRepository.Update(from);
            _accountRepository.Update(to);
        }
    }
}
