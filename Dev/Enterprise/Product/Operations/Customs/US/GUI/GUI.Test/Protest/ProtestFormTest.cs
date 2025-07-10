using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI.Protest.Testing
{
	using Enterprise.Customs.US.Business.Protest;
	using NUnit.Framework;

	[TestedType(typeof(ProtestForm))]
	sealed class ProtestFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (ProtestForm form = (ProtestForm)GetFormToBashCore())
			{
				Assert(form.FormCaption.Contains("Protest"));
				form.BusinessEntity.Factory.Save();
				Assert(form.FormCaption.Contains("Protest - "));
			}
		}

		public void TestSupportsEDocs()
		{
			using (ProtestForm form = (ProtestForm)GetFormToBashCore())
			{
				AssertNull(form.Controls["eDocs"]);
			}
		}

		public void TestPlugInsAdded()
		{
			using (var form = (ProtestForm)GetFormToBashCore())
			{
				form.Show();
				AssertPlugIn(ControllerIDs.JobInvoicing, form);
				AssertPlugIn(ControllerIDs.DocDataPlugIn, form);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var protest = Protest;
			Factory.Save();
			var result = new ProtestForm(Protest);
			result.ControllerID = ControllerIDs.Customs.US.Protest;
			return result;
		}

		Protest protest;
		Protest Protest => protest ?? (protest = new Protest(Factory.New<JobDeclaration>()));

		void AssertPlugIn(ControllerID id, ProtestForm form)
		{
			AssertNotNull(id.Name + " is added", form.PlugIns.GetPlugIn(id));
			AssertNoExceptionThrown(delegate
			{
				form.PlugIns.GetPlugIn(id).SelectTabPage();
			});
		}
	}
}
