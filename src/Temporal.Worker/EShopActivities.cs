using System;
using System.Collections.Generic;
using System.Text;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Temporal.Worker
{
    public class EShopActivities
    {

        [Activity]
        public static async Task<string> SetAwaitingValidationStatus(int orderNumber)
        {
            
            throw new NotImplementedException();
            //var bankService = new BankingService("bank1.example.com");
            //Console.WriteLine($"Withdrawing ${details.Amount} from account {details.SourceAccount}.");
            //try
            //{
            //    return await bankService.WithdrawAsync(details.SourceAccount, details.Amount, details.ReferenceId).ConfigureAwait(false);
            //}
            //catch (Exception ex)
            //{
            //    throw new ApplicationFailureException("Withdrawal failed", ex);
            //}
        }
    }
}
