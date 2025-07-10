using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(SPTSHeaderForm))]
	class SPTSHeaderFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<SPTSHeader>();
			_ = header.MovementHeader;
			Factory.Save();

			return new SPTSHeaderForm(header)
			{
				ControllerID = ControllerIDs.Customs.TR.SimplifiedProcedureTransitSystem
			};
		}

		#endregion

		public void TestMessagesTabPage()
		{
			var sptsHeader = Factory.New<SPTSHeader>();

			using (var form = new ZForm())
			using (var control = new SPTSHeaderForm(sptsHeader))
			{
				control.SetDataBinding(sptsHeader, ZString.Empty);
				form.Controls.Add(control);
				form.Show();

				var messagesTabPage = (ZTabPage)control.Controls.Find("MessagesTabPage", true).First();
				AssertEquals("Messages", messagesTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestTRSPTSHeaderFormTestFieldsVisibility()
		{
			var sptsHeader = Factory.New<SPTSHeader>();
			using (var form = new ZForm(sptsHeader))
			{
				using (var control = new SPTSHeaderUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var sptsHeaderUserControl = form.Controls.Find("SPTSHeaderUserControl", true).FirstOrDefault();

					CombineAssertions(() =>
					{
						var registrationNumberTextBox = sptsHeaderUserControl.FindSingle<ZTextBox>(c => c.Name == "RegistrationNumberTextBox");
						AssertEquals("RegistrationNumberTextBox", true, registrationNumberTextBox.Visible);
						var registrationDateEdit = sptsHeaderUserControl.FindSingle<ZDateEdit>(c => c.Name == "RegistrationDateEdit");
						AssertEquals("RegistrationDateEdit", true, registrationDateEdit.Visible);
						var voyageNumberTextBox = sptsHeaderUserControl.FindSingle<ZTextBox>(c => c.Name == "VoyageNumberTextBox");
						AssertEquals("VoyageNumberTextBox", true, voyageNumberTextBox.Visible);
						var sailingDateEdit = sptsHeaderUserControl.FindSingle<ZDateEdit>(c => c.Name == "SailingDateEdit");
						AssertEquals("SailingDateEdit", true, sailingDateEdit.Visible);
					});
				}
			}
		}
	}
}
