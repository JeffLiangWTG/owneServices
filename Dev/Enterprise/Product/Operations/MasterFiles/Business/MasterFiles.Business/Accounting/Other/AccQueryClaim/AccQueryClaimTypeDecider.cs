using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccQueryClaimTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if (row[AccQueryClaim.Schema.AY_AH] != null)
			{
				var transactionPK = new ZGuid(row[AccQueryClaim.Schema.AY_AH]);
				var transactionHeader = factory.Load<AccTransactionHeader>(transactionPK);
				switch (transactionHeader.AH_Ledger)
				{
					case LedgerTypes.AccountsPayable:
						return ObjectFactory.GetType<IAPAccQueryClaim>();
					case LedgerTypes.AccountsReceivable:
						return ObjectFactory.GetType<IARAccQueryClaim>();
					default:
						throw new ArgumentException("Claim/Query type cannot be determined");
				}
			}
			else
			{
				throw new ArgumentException("Claim/Query type cannot be determined");
			}
		}

		public override Type GetTypeForNew()
		{
			throw new ArgumentException("Claim/Query type cannot be determined");
		}

		public override Type GetTypeForBinding()
		{
			throw new ArgumentException("Claim/Query type cannot be determined");
		}
	}
}
