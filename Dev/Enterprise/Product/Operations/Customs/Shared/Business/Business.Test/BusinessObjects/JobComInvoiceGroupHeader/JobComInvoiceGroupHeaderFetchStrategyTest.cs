using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class JobComInvoiceGroupHeaderFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var subGroup1 = Factory.New<BaseJobComInvoiceGroupHeader>();
			subGroup1.JZ_JE = Declaration.PK;
			subGroup1.JZ_JZ_GroupInvoiceFK = GroupHeader.PK;
			var charge1 = subGroup1.Charges.AddNew();
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge1.J7_Amount = 100M;

			var subGroup2 = Factory.New<BaseJobComInvoiceGroupHeader>();
			subGroup2.JZ_JE = Declaration.PK;
			subGroup2.JZ_JZ_GroupInvoiceFK = GroupHeader.PK;
			var charge2 = subGroup2.Charges.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge2.J7_Amount = 200M;

			Factory.Save();

			TestFetchForView(subGroup1, subGroup2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			var parent = factory.Load<BaseJobComInvoiceGroupHeader>(GroupHeader.PK);
			return new BaseJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader>(parent).CollectionToFilter;
		}

		BaseJobDeclaration Declaration
		{
			get
			{
				return fDeclaration ?? (fDeclaration = Factory.New<BaseJobDeclaration>());
			}
		}
		BaseJobDeclaration fDeclaration;

		BaseJobComInvoiceGroupHeader GroupHeader
		{
			get
			{
				return fGroupHeader ?? (fGroupHeader = Declaration.JobComInvoiceGroupHeaders[0]);
			}
		}
		BaseJobComInvoiceGroupHeader fGroupHeader;
	}
}
