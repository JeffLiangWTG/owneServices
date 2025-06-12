using System;
using System.Data.Entity;
using System.Linq;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper
{
	public class SGCustomsDataModelAccessor
	{
		private readonly ContextFactory<eHubTransactionsContext> contextFactory;
		private const string SGCustomsAccountRegistrationTypeID = "SGCustomsAccount";

		public SGCustomsDataModelAccessor() : this(new ContextFactory<eHubTransactionsContext>()) { }

		internal SGCustomsDataModelAccessor(ContextFactory<eHubTransactionsContext> contextFactory)
		{
			this.contextFactory = contextFactory;
			Database.SetInitializer<eHubTransactionsContext>(null);
		}

		public virtual string GetSGCustomsAccount(string brokerID, string eHubClientID)
		{
			if (string.IsNullOrWhiteSpace(brokerID))
			{
				throw new ArgumentException("Argument is null or empty or whitespace", "brokerID");
			}
			if (string.IsNullOrWhiteSpace(eHubClientID))
			{
				throw new ArgumentException("Argument is null or empty or whitespace", "eHubClientID");
			}

			using (var context = contextFactory.CreateContext())
			{
				var client = (from c in context.eHubClients where c.CC_ID == eHubClientID select c.CC_PK).FirstOrDefault();
				if (client == Guid.Empty)
				{
					throw new ArgumentException("Argument is not valid, no matching CC_ID value in database", "eHubClientID");
				}
				var accounts = from r in context.eHubClientRegistrations
							   where r.CX_Qualifier == brokerID && r.eHubRegistrationType.RT_ID == SGCustomsAccountRegistrationTypeID
							&& r.CX_CC == client
							   select r.CX_Code;

				return accounts.FirstOrDefault() ?? string.Empty;
			}
		}

		public virtual bool IsAccountValid(string brokerID, string eHubClientID, string accountName)
		{
			using (var context = contextFactory.CreateContext())
			{
				var client = (from c in context.eHubClients where c.CC_ID == eHubClientID select c.CC_PK).FirstOrDefault();
				if (client == Guid.Empty)
				{
					throw new ArgumentException("Argument is not valid, no matching CC_ID value in database", "eHubClientID");
				}
				var flags = from r in context.eHubClientRegistrations
					where r.eHubRegistrationType.RT_ID == SGCustomsAccountRegistrationTypeID
						&& r.CX_CC == client && r.CX_Qualifier == brokerID && r.CX_Code == accountName
					select r.CX_Flag1;

				return Convert.ToBoolean(flags.FirstOrDefault());
			}
		}

		public virtual string GetSGCustomsSenderID(string accountName)
		{
			if (string.IsNullOrWhiteSpace(accountName))
			{
				throw new ArgumentException("Argument is null or empty or whitespace", "accountName");
			}
			if (accountName.Length < 4)
			{
				throw new ArgumentException("Argument must be at least 4 characters in length", "accountName");
			}

			return string.Format("{0}.{1}", accountName.Substring(0, 4), accountName);
		}
	}
}
