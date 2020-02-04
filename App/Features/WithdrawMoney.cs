using System;
using App.DataAccess;
using App.Domain.Services;

namespace App.Features
{
    public class WithdrawMoney
    {
        private readonly IAccountRepository _accountRepository;
        private readonly INotificationService _notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            _accountRepository = accountRepository;
            _notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            var from = _accountRepository.GetAccountById(fromAccountId);

            from.OnFundsLow += (sender, args) => _notificationService.NotifyFundsLow(from.User.Email);

            from.Withdraw(amount);

            _accountRepository.Update(from);
        }
    }
}
