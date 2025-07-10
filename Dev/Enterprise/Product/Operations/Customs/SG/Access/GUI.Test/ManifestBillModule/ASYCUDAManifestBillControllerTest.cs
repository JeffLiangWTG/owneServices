using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(ManifestBillController))]
	public class ASYCUDAManifestBillControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(AsycudaBill), new ManifestBillController().TypeOfTopLevelBusinessObject);
		}

		public override Type ControllerToBashType => typeof(ManifestBillController);
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.ASYCUDA.SGAccess.ManifestBill;
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Factory.Save();
			return bill;
		}
	}
}
