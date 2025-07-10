using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Global.GlbPerson;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbPersonNewController))]
	sealed class GlbPersonNewControllerTest : GlbPersonControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbPersonNew;
		}

		protected override IZForm GetEditFormToShow()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.GlbPersonNew);
			return controller.ShowEditForm(person);
		}

		public override void TestEditForm()
		{
			using (var form = GetEditFormToShow())
			{
				AssertType(typeof(GlbPersonNewForm), form);
			}
		}

		protected override void CloseAndDispose(IZForm form)
		{
			form?.Dispose();
		}

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert(true); // non Customs Controller
		}
	}
}
