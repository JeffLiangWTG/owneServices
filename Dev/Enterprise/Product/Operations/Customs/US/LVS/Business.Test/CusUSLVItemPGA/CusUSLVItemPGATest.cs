using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVItemPGA))]
	internal class CusUSLVItemPGATest : EnterpriseBusinessObjectTestCase
	{
		public void TestPGADisclaimReasonList()
		{
			var pga = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVItemPGA;
			AssertHasCustomAttribute<ListAttribute>(pga.GetType(), nameof(pga.ULP_DisclaimReason), false, a => a.ListDataSourceMember == nameof(pga.PGADisclaimReasonList));
			AssertHasCustomAttribute<MaxLengthAttribute>(pga.GetType(), nameof(pga.ULP_DisclaimReason), false, a => a.MaxLength == 1);
			pga.AgencyCode = "TST";
			var list = pga.PGADisclaimReasonList;
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode("T"));
			AssertEquals("TestReason", list["T"].Description);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusUSLVItemPGA = factory.NewWithValidTestData<CusUSLVClearance>()
				.CusUSLVConsignments
				.AddNew()
				.CusUSLVItems
				.AddNew()
				.CusUSLVItemPGAs
				.AddNew();
			cusUSLVItemPGA.ULP_Agency = "TES";
			cusUSLVItemPGA.ULP_DisclaimReason = "A";
			cusUSLVItemPGA.ULP_Indicator = "C";
			cusUSLVItemPGA.ULP_AgencyProgram = "TES";

			return cusUSLVItemPGA;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusUSLVItemPGA = Factory.NewWithValidTestData<CusUSLVClearance>()
				.CusUSLVConsignments
				.AddNew()
				.CusUSLVItems
				.AddNew()
				.CusUSLVItemPGAs
				.AddNew();
			cusUSLVItemPGA.ULP_Agency = "TES";
			cusUSLVItemPGA.ULP_DisclaimReason = "A";
			cusUSLVItemPGA.ULP_Indicator = "C";
			cusUSLVItemPGA.ULP_AgencyProgram = "TES";

			return cusUSLVItemPGA;
		}
	}
}
