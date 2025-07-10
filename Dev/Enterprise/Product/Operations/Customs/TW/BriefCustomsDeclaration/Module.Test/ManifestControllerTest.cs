using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.TW.BriefCustomsDeclaration.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Module.Testing
{
	[TestedType(typeof(ManifestController))]
	sealed class ManifestControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(Business.AsycudaManifestHeader), new ManifestController().TypeOfTopLevelBusinessObject);
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.TW.BriefCustomsDeclarations, new ManifestController().ModuleID);
		}

		public void TestGetForm()
		{
			AssertType<BriefDeclarationForm>(Controller.ShowNewForm());
		}

		public void TestCheckPointForView()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(Env.Security.AsycudaManifestReporting, new ManifestController().GetCheckPointForView(header));
		}

		public void TestCheckPointForEdit()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(Env.Security.AsycudaManifestReporting, new ManifestController().GetCheckPointForEdit(header));
		}

		public void TestCheckPointForNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(Env.Security.AsycudaManifestReporting, new ManifestController().GetCheckPointForNew(header));
		}

		public void TestCheckPointForDelete()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(Env.Security.AsycudaManifestReporting, new ManifestController().GetCheckPointForDelete(header));
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.TW.BriefCustomsDeclarations;

		protected override string CountryCode => Core.Constants.CountryCodes.Taiwan;
	}
}
