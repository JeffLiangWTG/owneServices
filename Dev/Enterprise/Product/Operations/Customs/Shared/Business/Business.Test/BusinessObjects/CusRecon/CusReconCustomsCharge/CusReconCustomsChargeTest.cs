using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReconCustomsCharge))]
	class CusReconCustomsChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<CusReconCustomsChargeTypeDecider>(CusReconCustomsCharge.TypeDecider);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetCusReconCustomsCharge(factory);

		CusReconCustomsCharge GetCusReconCustomsCharge(BusinessObjectFactory factory)
		{
			var address = factory.NewWithValidTestData<OrgAddress>();
			factory.Save();

			var declaration = factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var reconEntry = factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = address.PK;
			var entryLine = factory.New<CusReconEntryLine>();
			entryLine.CRL_LineNumber = 1;
			entryLine.CRL_CustomsStatus = "AA";
			entryLine.CRL_Description = "AA";
			entryLine.CRL_OriginalEntryLineNumber = 1;
			entryLine.CRL_CRE = reconEntry.PK;
			var charge = factory.New<CusReconCustomsCharge>();
			charge.CRC_CRL_Line = entryLine.PK;
			charge.CRC_Amount = 1;
			charge.CRC_ChargeType = "AA";
			return charge;
		}
	}
}
