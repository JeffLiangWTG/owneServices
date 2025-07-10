using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryPayInfoTypeDecider : Customs.Business.CusEntryPayInfoTypeDecider, Integration.Customs.ZA.ICusEntryPayInfoTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context)
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var result = typeof(CusEntryPayInfo);
			var transactionType = row[CusEntryPayInfoSchema.Constants.C9_TransactionType].ToString().Trim();
			var lineLevelPPTypeList = factory.GetCachedValue<LineLevelProvisionalPayments>();
			var headerLevelPPTypeList = factory.GetCachedValue<HeaderLevelProvisionalPayments>();
			if (lineLevelPPTypeList.ContainsCode(transactionType) || headerLevelPPTypeList.ContainsCode(transactionType))
			{
				result = typeof(ProvisionalPaymentCusEntryPayInfo);
			}
			return result;
		}
	}
}
