using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(ManifestController))]
	public class ManifestControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(AsycudaManifestHeader), new ManifestController().TypeOfTopLevelBusinessObject);
		}

		public override Type ControllerToBashType => typeof(ManifestController);
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.ASYCUDA.SGAccess.Manifest;
		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Factory.Save();
			return header;
		}
	}
}
