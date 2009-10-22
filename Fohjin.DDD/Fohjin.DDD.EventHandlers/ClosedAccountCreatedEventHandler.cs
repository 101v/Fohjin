using System;
using Fohjin.DDD.Events.ClosedAccount;
using Fohjin.DDD.Reporting.Dto;
using Fohjin.DDD.Reporting.Infrastructure;

namespace Fohjin.DDD.EventHandlers
{
    public class ClosedAccountCreatedEventHandler : IEventHandler<ClosedAccountCreatedEvent>
    {
        private readonly IReportingRepository _reportingRepository;

        public ClosedAccountCreatedEventHandler(IReportingRepository reportingRepository)
        {
            _reportingRepository = reportingRepository;
        }

        public void Execute(ClosedAccountCreatedEvent theEvent)
        {
            var closedAccount = new ClosedAccount(theEvent.AccountId, theEvent.ClientId, theEvent.AccountName, theEvent.AccountNumber);
            var closedAccountDetails = new ClosedAccountDetails(theEvent.AccountId, theEvent.ClientId, theEvent.AccountName, 0, theEvent.AccountNumber);

            foreach (var ledger in theEvent.Ledgers)
            {
                var split = ledger.Value.Split(new[] { '|' });
                var amount = Convert.ToDecimal(split[0]);
                var account = split[1];
                _reportingRepository.Save(new Ledger(Guid.NewGuid(), theEvent.AccountId, GetDescription(ledger.Key, account), amount));
            }

            _reportingRepository.Save(closedAccount);
            _reportingRepository.Save(closedAccountDetails);
            _reportingRepository.Delete<Account>(new { Id = theEvent.AccountId });
            _reportingRepository.Delete<AccountDetails>(new { Id = theEvent.AccountId });
        }

        private static string GetDescription(string transferType, string accountNumber)
        {
            if (transferType == "CreditMutation")
                return "Deposite";

            if (transferType == "DebitMutation")
                return "Withdrawl";

            if (transferType == "CreditTransfer")
                return string.Format("Transfer to {0}", accountNumber);

            if (transferType == "DebitTransfer")
                return string.Format("Transfer from {0}", accountNumber);

            return string.Empty;
        }
    }
}