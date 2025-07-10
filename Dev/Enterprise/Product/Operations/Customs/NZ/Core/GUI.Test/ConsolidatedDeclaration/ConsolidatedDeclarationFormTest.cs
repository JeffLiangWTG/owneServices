using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.GUI.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationForm))]
	sealed class ConsolidatedDeclarationFormTest : ConsolidatedDeclarationFormTest<ConsolidatedDeclaration>
	{
		public void TestPanelLayout()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				CombineAssertions(() =>
				{
					form.Show();
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.ConsolidatedDeclarationDetailsUserControl), form.FindSingleOrDefault<ZUserControl>(u => u.Name == "ConsolidatedDeclarationDetailsUserControl").FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.LeadDeclaration.DeclarationNumber))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.ConsolidatedDeclarationDetailsUserControl), form.FindSingleOrDefault<ZUserControl>(u => u.Name == "ConsolidatedDeclarationDetailsUserControl").FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.CustomsStatusDescription))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.ConsolidatedDeclarationDetailsUserControl), form.FindSingleOrDefault<ZUserControl>(u => u.Name == "ConsolidatedDeclarationDetailsUserControl").FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(JobDeclaration.JE_TSWCombinedStatusDesc))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.EntryStyleDropEdit), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.EntryStyle))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.VoyageFlightNoTextBox), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.VoyageFlightNo))));
				});
			}
		}

		public void TestPeriodToVisibility()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				consolidatedDeclaration.LeadDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				form.Show();
				Assert("Entry period should not be visible for non PER entry style", !form.FindSingleOrDefault<ZDateEdit>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.CRD_PeriodTo))).Visible);
			}
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				consolidatedDeclaration.LeadDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
				form.Show();
				Assert("Entry period should be visible for PER entry style", form.FindSingleOrDefault<ZDateEdit>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.CRD_PeriodTo))).Visible);
			}
		}

		public void TestVesselNameVisibility()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				CombineAssertions(() =>
				{
					consolidatedDeclaration.LeadDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
					form.Show();
					var vesselNameControl = form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.VesselName))) as ZUserControl;
					AssertEquals($"{nameof(ConsolidatedDeclarationControlBag.VesselCodeFindBox)} should be visible", true, vesselNameControl?.Visible);
				});
			}
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				CombineAssertions(() =>
				{
					consolidatedDeclaration.LeadDeclaration.JE_TransportMode = JobTransportModeList.Codes.Air;
					form.Show();
					var vesselNameControl = form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.VesselName))) as ZUserControl;
					AssertEquals($"{nameof(ConsolidatedDeclarationControlBag.VesselCodeFindBox)} should be invisible", false, vesselNameControl?.Visible);
				});
			}
		}

		public void TestVoyageFlightCaption()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				consolidatedDeclaration.LeadDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
				form.Show();
				AssertEquals("Caption for SEA", "Voyage", form.FindSingleOrDefault<ZTextBox>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.VoyageFlightNo))).CaptionResourceString.Caption);
			}
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				consolidatedDeclaration.LeadDeclaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				form.Show();
				AssertEquals("Caption for AIR", "Arrival Flight", form.FindSingleOrDefault<ZTextBox>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.VoyageFlightNo))).CaptionResourceString.Caption);
			}
		}

		public void TestGridColumns()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				CombineAssertions(() =>
				{
					var grid = form.FindSingleOrDefault<ZGrid>("JobDeclarationsGrid");
					AssertType<ZCalcEditColumnStyleInfo>("NoOfInvoices", grid.GetColumnStyle("NoOfInvoices"));
					AssertType<ZCalcEditColumnStyleInfo>("CusEntryHeader.VFDWholeNZD", grid.GetColumnStyle("CusEntryHeader+VFDWholeNZD"));
				});
			}
		}

		public void TestMessagesTab()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				var messagesTabPage = form.FindSingleOrDefault<ZTabPage>("MessagesTabPage");
				AssertType<CustomsMessagesTabUserControl>(messagesTabPage.Controls[0]);
			}
		}

		public void TestLinkedDeclarationsGroupBoxCaption()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				var linkedDeclarationsGroupBox = form.FindSingleOrDefault<ZGroupBox>("LinkedDeclarationsGroupBox");
				AssertEquals("Linked Declarations", linkedDeclarationsGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestEDIMenu()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				CombineAssertions(() =>
				{
					var brokerageMenuItem = form.Menu.MenuItems.FindByText("Brokerage");
					AssertNotNull("Brokerage Menu Item", brokerageMenuItem);
					AssertNotNull("Submit Menu Item", brokerageMenuItem.MenuItems.FindByText("Submit Job"));
					AssertNotNull("Cancel Menu Item", brokerageMenuItem.MenuItems.FindByText("Cancel Job"));
					AssertNotNull("Reset Menu Item", brokerageMenuItem.MenuItems.FindByText("Reset to Original"));
				});
			}
		}

		public void TestDocumentMenu()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				AssertNotNull("The form should contain the Documents PlugIn", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public new void TestCorrectModuleIdAndMenuSection()
		{
			Assert("This test should be revisited when the registry switch is removed", !RawDataRegistry.Instance.EnableConsolidatedEntries.Value);
		}

		protected override ConsolidatedDeclaration ConsolidatedDeclaration => consolidatedDeclaration;

		protected override void SetUp()
		{
			base.SetUp();
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			consolidatedDeclaration.Factory.Save();
		}

		protected override Form GetFormToBashCore()
		{
			var form = new ConsolidatedDeclarationForm(ConsolidatedDeclaration, new ConsolidatedDeclarationFormAdaptationsProvider());
			form.ControllerID = ControllerIDs.Customs.ConsolidatedDeclaration;
			return form;
		}

		ConsolidatedDeclaration consolidatedDeclaration;
	}
}
