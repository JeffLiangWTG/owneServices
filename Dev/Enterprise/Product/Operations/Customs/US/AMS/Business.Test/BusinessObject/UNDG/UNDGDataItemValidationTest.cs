using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class UNDGDataItemValidationSailingSynchronisationTest : LinkedSailingBillsImportedTest
	{
		public void TestUNDGDataItemIsNotLinkedToSailing()
		{
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "SCAC";
			bill.B0_MasterBillNumber = "AAA";
			var container = bill.MovementDetail.Containers.AddNew();
			container.BC_ContainerNum = "ABCD1111";
			var undg = container.UNDGs.AddNew();
			undg.DI_DG = ZGuid.Empty;
			undg.Validation.ValidateAll();
			AssertHasRowWarning(undg, ValidationConstants.SailingSynchronisation.HazardousDetailMightBeIncorectlyAdded);
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "!!5%", "", "IMO").First().PK;
			undg.Validation.ValidateAll();
			AssertNoRowWarningContaining(undg, ValidationConstants.SailingSynchronisation.HazardousDetailMightBeIncorectlyAdded);
		}
	}
}
