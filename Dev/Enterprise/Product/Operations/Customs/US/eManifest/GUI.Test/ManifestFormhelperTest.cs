using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class ManifestFormhelperTest : TestCaseWithFactory
	{
		public void TestManifestMenuCreateJobDeclaration()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "t1";
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "Ts1";
			var trip1 = Factory.NewWithValidTestData<Trip>();
			trip1.BH_GB = branch.PK;
			var shipment1 = trip1.Shipments.AddNew();
			shipment1.B0_ReferenceID = "Shipment1";
			var shipment2 = trip1.Shipments.AddNew();
			shipment2.B0_ReferenceID = "Shipment2";
			trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertNoExceptionThrown(() => ManifestFormhelper.CreateANewDeclaration(trip1));
		}
	}
}
