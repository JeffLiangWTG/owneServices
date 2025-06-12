using System;
using System.Data;
using System.Linq;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ITCustoms.Configuration;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Common.Logging;

namespace CargoWise.eHub.Products.ITCustoms.Helpers
{
	public static class AccountProfileHelper
	{
		public static byte GetSenderProfileStatus(string systemId, string node, ILog logger)
		{
			return GetClientSystemRegistration(systemId, node, logger).CD_Flag1 ?? 0;
		}

		public static Account GetSenderProfile(string systemId, string node, ILog logger)
		{
            var sys = GetClientSystemRegistration(systemId, node, logger);
            return new Account { AccountNumber = sys.CD_Attr1, AccountPassword = EhubServerDecryptor.Decrypt(sys.CD_Attr2) };
        }

		static eHubClientSystemRegistration GetClientSystemRegistration(string systemId, string node, ILog logger)
		{
            using (var context = GetDbContext())
            {
                var reg = context.eHubClientSystemRegistrations.SingleOrDefault(
                    s => s.eHubRegistrationType.RT_ID == RegistrationType && s.eHubClientSystem.EH_ID == systemId && s.CD_Code == node);
                if (reg == null)
                    throw new FatalMessageProcessingException("You are not registered with this service. Please contact WTG to register.");
                return reg;
            }
        }

		public static void MarkAccountsInvalid(Account account)
		{
			using (var context = GetDbContext())
			{
				var regs = context.eHubClientSystemRegistrations.Where(s => s.CD_Attr1 == account.AccountNumber && s.eHubRegistrationType.RT_ID == RegistrationType && s.CD_Flag1 != (byte)ConfigurationStatus.INV);
				foreach (var eHubClientSystemRegistration in regs)
				{
					if (EhubServerDecryptor.Decrypt(eHubClientSystemRegistration.CD_Attr2) == account.AccountPassword)
						eHubClientSystemRegistration.CD_Flag1 = (byte)ConfigurationStatus.INV;
				}
				context.SaveChanges();
			}
		}

		public static void MarkAccountsValid(Account account)
		{
			using (var context = GetDbContext())
			{
				var regs = context.eHubClientSystemRegistrations.Where(s => s.CD_Attr1 == account.AccountNumber && s.eHubRegistrationType.RT_ID == RegistrationType && s.CD_Flag1 == (byte)ConfigurationStatus.UNK);
				foreach (var eHubClientSystemRegistration in regs)
				{
					if (EhubServerDecryptor.Decrypt(eHubClientSystemRegistration.CD_Attr2) == account.AccountPassword)
						eHubClientSystemRegistration.CD_Flag1 = (byte)ConfigurationStatus.VAL;
				}
				context.SaveChanges();
			}
		}

		internal static Func<eHubTransactionsContext> GetDbContext = () => new eHubTransactionsContext();
		internal const string RegistrationType = "ITCustomsAccount";
	}
}
