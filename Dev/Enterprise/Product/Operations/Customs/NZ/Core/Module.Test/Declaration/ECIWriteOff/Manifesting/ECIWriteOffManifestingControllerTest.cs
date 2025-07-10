using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.Testing
{
	[TestedType(typeof(ECIWriteOffManifestingController))]
	sealed class ECIWriteOffManifestingControllerTest : ZControllerBasherTest
	{
		public void TestSaveChangesMessageShouldNotBeShownWhenNothingHasBeenChanged()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.NZ.ECIWriteOffManifesting);
			using (var form = controller.ShowNewForm() as NewManifestSelectionForm)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Close();
				AssertEquals("There should be no message asking the user to save changes.", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.NewZealand;
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.NZ.ECIWriteOffManifesting;
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var declaration = JobDeclaration.New(Factory);
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			Factory.Save();
			return entryHeader;
		}
	}
}
