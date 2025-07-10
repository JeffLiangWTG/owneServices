using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationForm))]
	sealed class ConsolidatedDeclarationFormBaseOnlyTest : ConsolidatedDeclarationFormTest<ConsolidatedDeclaration>
	{
		public void TestWorkflowTab()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var tabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTemplateTabControl;
				AssertNotNull("Main tab control exists", tabControl);
				var tabPage = form.Controls.Find("WorkflowTabPage", true).FirstOrDefault() as ZWorkflowTabPage;
				AssertNotNull("Workflow tab exists", tabPage);
				tabControl.SelectedTab = tabPage;
				CombineAssertions(() =>
				{
					AssertNotNull("Tasks exists", tabPage.Controls.Find("TasksTab", true).FirstOrDefault());
					AssertNotNull("Milestones exists", tabPage.Controls.Find("MilestonesTab", true).FirstOrDefault());
					AssertNotNull("Exception exists", tabPage.Controls.Find("ExceptionsTab", true).FirstOrDefault());
					AssertNotNull("Triggers exists", tabPage.Controls.Find("TriggersTab", true).FirstOrDefault());
					AssertNotNull("Events exists", tabPage.Controls.Find("EventsTab", true).FirstOrDefault());
				});
			}
		}

		public void TestPanelLayout()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				CombineAssertions(() =>
				{
					consolidatedDeclaration.LeadDeclaration.JE_TransportMode = TransportTypeGenericList.Codes.Sea;
					form.Show();
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.JobNumberTextBox), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.CRD_JobReferenceNumber))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.EntryNumberTextBox), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.DeclarationNumber))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.EntryStatusTextBox), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_EntryStatusDescription))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.MessageTypeDropEdit), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_MessageType))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.MessageSubTypeDropEdit), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_MessageSubType))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.TransportModeDropEdit), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_TransportMode))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.ImporterGuidFindBox), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_OH_Importer))));
					var vesselNameControl = form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_VesselName))) as ZUserControl;
					AssertEquals($"{nameof(ConsolidatedDeclarationControlBag.VesselCodeFindBox)} should be visible", true, vesselNameControl?.Visible);
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.VoyageFlightNoTextBox), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_VoyageFlightNo))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.DischargeETADateEdit), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_DateOfArrival))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.PortOfLoadingCodeFindBox), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_RL_NKPortOfLoading))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.PortOfDischargeCodeFindBox), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_RL_NKPortOfArrival))));
					AssertNotNull(nameof(ConsolidatedDeclarationControlBag.EntryPeriodDateEdit), form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.CRD_PeriodTo))));
				});
			}
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				CombineAssertions(() =>
				{
					consolidatedDeclaration.LeadDeclaration.JE_TransportMode = TransportTypeGenericList.Codes.Air;
					form.Show();
					var vesselNameControl = form.FindSingleOrDefault<IBindTo>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_VesselName))) as ZUserControl;
					AssertEquals($"{nameof(ConsolidatedDeclarationControlBag.VesselCodeFindBox)} should be invisible", false, vesselNameControl?.Visible);
				});
			}
		}

		public void TestFormCaption()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				AssertEquals($"Consolidated Entry {ConsolidatedDeclaration.CRD_JobReferenceNumber}", form.FormCaption);
			}
		}

		public void TestEDIMenu()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				CombineAssertions(() =>
				{
					AssertNotNull("Brokerage Menu Item", form.Menu.MenuItems.FindByText("______"));
				});
			}
		}

		public void TestGridColumnsWithSeaTransportMode()
		{
			consolidatedDeclaration.LeadDeclaration.JE_TransportMode = TransportTypeGenericList.Codes.Sea;

			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				CombineAssertions(() =>
				{
					var grid = form.JobDeclarationsGrid;
					AssertType<ZTextBoxColumnStyleInfo>(AutoJobDeclaration.Schema.JE_DeclarationReference, grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_DeclarationReference));
					AssertType<ZTextBoxColumnStyleInfo>(AutoJobDeclaration.Schema.JE_MasterBill, grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_MasterBill));
					AssertType<ZTextBoxColumnStyleInfo>(AutoJobDeclaration.Schema.JE_HouseBill, grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_HouseBill));
					AssertType<ZGuidFindBoxColumnStyleInfo>(AutoJobDeclaration.Schema.JE_OH_Supplier, grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_OH_Supplier));
					AssertType<ZTextBoxColumnStyleInfo>(AutoJobDeclaration.Schema.JE_GoodsDescription, grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_GoodsDescription));
					Assert("JE_HouseBill should have correct caption", grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_HouseBill).CaptionResourceString.Caption.Equals("House Bill"));
					Assert("Master bill should be available when lead declaration transport mode is not Mail", !grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_HouseBill).IsUnavailable);
				});
			}
		}

		public void TestGridColumnsWithAUMailTransportMode()
		{
			consolidatedDeclaration.LeadDeclaration.JE_TransportMode = TransportTypeGenericList.Codes.PostMail;

			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				var grid = form.JobDeclarationsGrid;
				var houseBillColumn = grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_HouseBill);

				CombineAssertions(() =>
				{
					AssertType<ZTextBoxColumnStyleInfo>(AutoJobDeclaration.Schema.JE_HouseBill, houseBillColumn);
					Assert("JE_HouseBill should be renamed to Parcel Post Number when Transport Mode is Mail", houseBillColumn.CaptionResourceString.Caption.Equals("Parcel Post Number"));
					Assert("Master bill should be unavailable when AU lead declaration transport mode is Mail", grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_MasterBill).IsUnavailable);
				});
			}
		}

		public void TestGridColumnsWithNZMailTransportMode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				consolidatedDeclaration.LeadDeclaration.JE_TransportMode = "PST";

				using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
				{
					var grid = form.JobDeclarationsGrid;
					var houseBillColumn = grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_HouseBill);

					CombineAssertions(() =>
					{
						AssertType<ZTextBoxColumnStyleInfo>(AutoJobDeclaration.Schema.JE_HouseBill, houseBillColumn);
						Assert("JE_HouseBill should be renamed to Parcel Post Number when Transport Mode is Post", houseBillColumn.CaptionResourceString.Caption.Equals("Parcel Post Number"));
						Assert("Master bill should be unavailable when NZ lead declaration transport mode is Post", grid.GetColumnStyle(AutoJobDeclaration.Schema.JE_MasterBill).IsUnavailable);
					});
				}
			}
		}

		public void TestPostingButtons()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				AssertNotNull(form.FindSingleOrDefault<ZPostingButtonsUserControl>());
			}
		}

		public void TestLeadDeclaration()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertNotNull(nameof(BaseJobDeclaration.JE_DeclarationReference), form.FindSingleOrDefault<ZTextBox>(c => c.BindTo.EndsWith(nameof(BaseJobDeclaration.JE_DeclarationReference))));
					form.FindSingle<ZButton>("OpenJobDeclarationButton").PerformClick();
					var openedForm = Application.OpenForms[Application.OpenForms.Count - 1] as ZForm;
					AssertEquals("Lead declaration should be opened", ConsolidatedDeclaration.LeadDeclaration.PK, (openedForm.BusinessEntity as BusinessObject).PK);
					openedForm.Close();
					form.Close();
				});
			}
		}

		public void TestOpenDeclaration()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				form.Show();
				CombineAssertions(() =>
				{
					var grid = form.FindSingle<ZGrid>(c => c.BindTo.EndsWith(nameof(ConsolidatedDeclaration.JobDeclarations)));
					grid.PerformMouseDownForTest(0, 2);
					var openedForm = Application.OpenForms[Application.OpenForms.Count - 1] as ZForm;
					AssertEquals("First declaration should be opened", ConsolidatedDeclaration.JobDeclarations[0].PK, (openedForm.BusinessEntity as BusinessObject).PK);
					openedForm.Close();
					grid.PerformMouseDownForTest(1, 2);
					openedForm = Application.OpenForms[Application.OpenForms.Count - 1] as ZForm;
					AssertEquals("Second declaration should be opened", ConsolidatedDeclaration.JobDeclarations[1].PK, (openedForm.BusinessEntity as BusinessObject).PK);
					openedForm.Close();
					form.Close();
				});
			}
		}

		public void TestMessagesTab()
		{
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				var messagesTabPage = form.FindSingleOrDefault<ZTabPage>("MessagesTabPage");
				AssertType<BaseMessagesTabUserControl>(messagesTabPage.Controls[0]);
			}
		}

		public void TestGetListOfBizObjectsToCheckEditing()
		{
			using (var form = new ConsolidatedDeclarationFormForTest(ConsolidatedDeclaration, new DefaultConsolidatedDeclarationFormAdaptationsProvider()))
			{
				var declarations = ConsolidatedDeclaration.JobDeclarations;
				var dec0 = declarations[0];
				var dec1 = declarations[1];

				var shipment1 = Factory.New<ForwardingShipment>();
				shipment1.JS_UniqueConsignRef = "S9988776655";
				dec1.JE_JS = shipment1.PK;
				AssertEquals("Declaration is linked to Shipment", shipment1, dec1.Shipment);

				var listOfBizObjects = form.ListOfBizObjectsToCheckEditingExposed;
				AssertEquals("Checking ConsolidatedDeclaration", form.FormCaption, listOfBizObjects[ConsolidatedDeclaration]);
				AssertEquals("Checking Member Declaration", "Customs Declaration - " + dec0.JE_DeclarationReference, listOfBizObjects[dec0]);
				AssertEquals("Checking Shipment linked to Member Declaration", "Shipment S9988776655", listOfBizObjects[shipment1]);
				AssertEquals("Not Checking for Member Declaration linked to Shipment", false, listOfBizObjects.ContainsKey(dec1));
			}
		}

		public void TestShowOtherUsersCurrentlyAccessingThisEntity()
		{
			var user1 = Factory.New<IGlbStaff>();
			user1.GS_LoginName = "SemaphoreUser";

			var declarations = ConsolidatedDeclaration.JobDeclarations;
			var dec0 = declarations[0];
			var dec1 = declarations[1];

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S9988776655";
			dec1.JE_JS = shipment1.PK;
			AssertEquals("Declaration is linked to Shipment", shipment1, dec1.Shipment);

			Factory.Save();

			using (var user1Provider = new SemaphoreProviderWithMockUserIdForTesting(user1.PK.ToGuid()))
			using (var semaphoreHandle1 = ((ISemaphoreProvider)user1Provider).CreateSemaphoreHandle(new PendingUserActionSemaphore(ConsolidatedDeclaration.PK.ToString())))
			using (var semaphoreHandle2 = ((ISemaphoreProvider)user1Provider).CreateSemaphoreHandle(new PendingUserActionSemaphore(dec0.PK.ToString())))
			using (var semaphoreHandle3 = ((ISemaphoreProvider)user1Provider).CreateSemaphoreHandle(new PendingUserActionSemaphore(shipment1.PK.ToString())))
			using (var form = GetFormToBash() as ConsolidatedDeclarationForm)
			{
				form.Show();
				Application.DoEvents();
			}

			var consolidatedEntryReference = ConsolidatedDeclaration.CRD_JobReferenceNumber;
			var declarationReference = dec0.JE_DeclarationReference;

			var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text.Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");
			AssertStartsWith("", $"These users are currently modifying Consolidated Entry {consolidatedEntryReference} or one of its dependent objects:", lastMessage);
			AssertContains("Consolidated Entry", $"Consolidated Entry {consolidatedEntryReference}: - HeartbeatInfoFactoryWithMockUserForTesting_TestHost since", lastMessage);
			AssertContains("Member Declaration", $"Customs Declaration - {declarationReference}: - HeartbeatInfoFactoryWithMockUserForTesting_TestHost since", lastMessage);
			AssertContains("Declaration on Shipment", "Shipment S9988776655: - HeartbeatInfoFactoryWithMockUserForTesting_TestHost since", lastMessage);
		}

		public new void TestCorrectModuleIdAndMenuSection()
		{
			Assert("This test can only be tested in countries which implement Consolidated Declaration module.", true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			consolidatedDeclaration.Factory.Save();
		}

		protected override ConsolidatedDeclaration ConsolidatedDeclaration => consolidatedDeclaration;

		ConsolidatedDeclaration consolidatedDeclaration;
	}

	sealed class ConsolidatedDeclarationFormForTest : ConsolidatedDeclarationForm
	{
		public ConsolidatedDeclarationFormForTest(ConsolidatedDeclaration consolidatedDeclaration, IConsolidatedDeclarationFormAdaptationsProvider adaptionsProvider)
			: base(consolidatedDeclaration, adaptionsProvider)
		{
		}

		public Dictionary<IBusiness, ZString> ListOfBizObjectsToCheckEditingExposed => GetListOfBizObjectsToCheckEditing();
	}

	sealed class DefaultConsolidatedDeclarationFormAdaptationsProvider : IConsolidatedDeclarationFormAdaptationsProvider
	{
		public IPanelLayoutProvider HeaderDetailsLayout => new DefaultConsolidatedDeclarationLayoutProvider();

		public IEnumerable<ZGridColumnInfo> DeclarationGridExtraColumnInfos => null;

		public ZUserControl MessagesTabUserControl => new BaseMessagesTabUserControl();

		public IConsolidatedDeclarationMenuBuilder EDIMenuBuilder => new TestEDIMenuProvider();

		public bool EnableDocumentMenuItem => false;
	}

	sealed class DefaultConsolidatedDeclarationLayoutProvider : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new ConsolidatedDeclarationLayoutBuilder<ConsolidatedDeclaration>();
				var commonBag = builder.CommonBag;

				builder.AddColumn();
				builder.Add(commonBag.JobNumberTextBox, ControlWidthClass.Auto);
				builder.Add(commonBag.EntryNumberTextBox, ControlWidthClass.Auto);
				builder.Add(commonBag.EntryStatusTextBox, ControlWidthClass.Auto);
				builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.MessageSubTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.ImporterGuidFindBox, ControlWidthClass.Auto);
				builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonBag.VoyageFlightNoTextBox, ControlWidthClass.Auto);
				builder.Add(commonBag.DischargeETADateEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.PortOfLoadingCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonBag.PortOfDischargeCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonBag.EntryPeriodDateEdit, ControlWidthClass.Auto);

				return builder.Build();
			}
		}
	}

	sealed class TestEDIMenuProvider : IConsolidatedDeclarationMenuBuilder
	{
		public ConsolidatedDeclaration ConsolidatedDeclaration { get; set; }

		public ZMenuItem BuildMenu()
		{
			var result = new ZMenuItem("______");
			return result;
		}
	}
}
