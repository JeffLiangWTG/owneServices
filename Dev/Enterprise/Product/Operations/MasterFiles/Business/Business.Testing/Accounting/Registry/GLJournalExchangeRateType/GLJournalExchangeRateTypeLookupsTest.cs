using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GLJournalExchangeRateTypeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestExchangeRateTypes()
		{
			var itemsSystemLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value;
			itemsSystemLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					if (x.Code.Equals(Core.Constants.ExchangeRateTypes.Code.CustomsRate))
					{
						x.Bool = true;
					}
					else
					{
						x.Bool = false;
					}
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, itemsSystemLevel);

			var itemsCompanyLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.Value;
			itemsCompanyLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					if (x.Code.Equals(Core.Constants.ExchangeRateTypes.Code.L01Rate))
					{
						x.Bool = true;
					}
					else
					{
						x.Bool = false;
					}
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, itemsCompanyLevel);

			var expectedList = new string[]
			{
				Core.Constants.ExchangeRateTypes.Code.CustomsRate,
				Core.Constants.ExchangeRateTypes.Code.L01Rate,
			};

			AssertContainsExactElementsInAnyOrder(expectedList, Parent.Lookups.ExchangeRateTypes.ToArray().Select(p => p.Code));
		}

		GLJournalExchangeRateType Parent
		{
			get
			{
				if (parent == null)
				{
					parent = new GLJournalExchangeRateType();
					parent.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				}
				return parent;
			}
		}

		GLJournalExchangeRateType parent;
	}
}
