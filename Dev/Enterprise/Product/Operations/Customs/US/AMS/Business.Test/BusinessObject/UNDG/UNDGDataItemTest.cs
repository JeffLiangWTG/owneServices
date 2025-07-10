using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class UNDGDataItemImportedFromSailingTest : LinkedSailingBillsImportedTest
	{
		public void TestImporting()
		{
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			var subs1 = UNDGSubstanceLoader.LoadSubstances(Factory, "!!5%", "", "IMO").First();
			var subs2 = UNDGSubstanceLoader.LoadSubstances(Factory, "!!6%", "", "IMO").First();
			header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "AAA").MovementDetail.Containers[0].UNDGs[0].SubstancePK = subs1.PK;
			var undg = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "AAA").MovementDetail.Containers[0].UNDGs.FirstOrDefault(x => x.SubstanceCode == subs1.DG_Code);
			AssertEquals(1.2m, undg.DI_DGFlashPoint);
			AssertEquals(contact.PK, undg.DI_OC_DGContact);
			header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "AAA").MovementDetail.Containers[0].UNDGs[0].SubstancePK = subs2.PK;
			undg = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "AAA").MovementDetail.Containers[0].UNDGs.FirstOrDefault(x => x.SubstanceCode == subs2.DG_Code);
			AssertNotNull(undg);
			header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "BBB").MovementDetail.Containers[0].UNDGs[0].SubstancePK = subs1.PK;
			undg = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "BBB").MovementDetail.Containers[0].UNDGs.FirstOrDefault(x => x.SubstanceCode == subs1.DG_Code);
			AssertNotNull(undg);
			header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "BBB").MovementDetail.Containers[0].UNDGs[0].SubstancePK = subs2.PK;
			undg = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "BBB").MovementDetail.Containers[0].UNDGs.FirstOrDefault(x => x.SubstanceCode == subs2.DG_Code);
			AssertNotNull(undg);
			header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "CCC").MovementDetail.Containers[0].UNDGs[0].SubstancePK = subs1.PK;
			undg = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "CCC").MovementDetail.Containers[0].UNDGs.FirstOrDefault(x => x.SubstanceCode == subs1.DG_Code);
			AssertNotNull(undg);
			header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "CCC").MovementDetail.Containers[0].UNDGs[0].SubstancePK = subs2.PK;
			undg = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "CCC").MovementDetail.Containers[0].UNDGs.FirstOrDefault(x => x.SubstanceCode == subs2.DG_Code);
			AssertNotNull(undg);
		}
	}

	[TestedType(typeof(UNDGDataItem))]
	class UNDGDataItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObjectForDeleteTest(factory);
			((UNDGDataItem)result).DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			return result;
		}
	}
}
