using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	public static class TestExtensions
	{
		public static JobHeader CreateConsolJob(this CommonConsol consol)
		{
			var jobHeaderParent = consol as IJobHeaderParent;
			return jobHeaderParent != null ? new JobHeader.Loader(jobHeaderParent).TryCreate() : null;
		}

		public static BusinessObject CreateConsolCost(this CommonConsol consol, ZString currencyCode, decimal exchangeRate = 1M, bool isPosted = false)
		{
			return CreateConsolCostCore(consol.Factory, consol.PK, currencyCode, exchangeRate, isPosted);
		}

		public static BusinessObject CreateUnrelatedConsolCost(this CommonConsol consol, ZString currencyCode, decimal exchangeRate = 1M, bool isPosted = false)
		{
			return CreateConsolCostCore(consol.Factory, ZGuid.NewZGuid(), currencyCode, exchangeRate, isPosted);
		}

		static BusinessObject CreateConsolCostCore(BusinessObjectFactory factory, ZGuid parentPK, ZString currencyCode, ZDecimal exchangeRate, bool isPosted)
		{
			var result = (BusinessObject)factory.New<IJobConsolCost>();
			result[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
			result[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			result.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				result[JobConsolCostSchema.E6_ParentID] = parentPK;
				result[JobConsolCostSchema.E6_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			}
			finally
			{
				result.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			result[JobConsolCostSchema.E6_RX_NKCurrency] = currencyCode;
			result[JobConsolCostSchema.E6_ExchangeRate] = exchangeRate;

			if (isPosted)
			{
				result[JobConsolCostSchema.E6_AH_APInvoice] = ZGuid.NewZGuid();
			}

			return result;
		}
	}
}
