using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	sealed class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestDeclarationSelectionSG()
		{
			var newCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var declaration1 = newCollection.AddNew();
			declaration1.JE_JS = ZGuid.NewZGuid();
			declaration1.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			var declaration2 = newCollection.AddNew();
			declaration2.JE_JS = ZGuid.Empty;
			declaration2.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			using (var form = new ZForm())
			using (var module = new JobDeclarationModule())
			using (var control = new JobDeclarationFilterStripControlForTest(module, newCollection, new JobDeclarationFilterBusinessObject()))
			{
				form.Controls.Add(control);
				form.Show();
				control.Find();
				control.FilteredGrid.ContextMenu.Dispose();
				control.FilteredGrid.ContextMenu = ((ZDisplayGrid)module.DisplayGrid).ContextMenu;
				control.SetMenuVisibility();
				AssertEquals(false, control.FilteredGrid.ContextMenu.MenuItems.FindByText("Actions").MenuItems.FindByText(JobDeclarationModule.CopyDeclarationOnlyText).Visible);
				control.selectedElements = new JobDeclaration[1];
				control.selectedElements[0] = declaration1;
				control.SetMenuVisibility();
				AssertEquals(true, control.FilteredGrid.ContextMenu.MenuItems.FindByText("Actions").MenuItems.FindByText(JobDeclarationModule.CopyDeclarationOnlyText).Visible);
				control.selectedElements[0] = declaration2;
				control.SetMenuVisibility();
				AssertEquals(false, control.FilteredGrid.ContextMenu.MenuItems.FindByText("Actions").MenuItems.FindByText(JobDeclarationModule.CopyDeclarationOnlyText).Visible);
			}
		}

		public void TestInitializeAdditionalColumns()
		{
			var newCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControl(null, newCollection, new JobDeclarationFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNotNull(grid.Columns[JobDeclaration.Schema.BrokerName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.ImporterName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.SupplierName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.ForwarderName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_OH_ControllingAgent]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_OH_ControllingCustomer]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_OH_ExternalBroker]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.AuditDate]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.AuditReference]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.AuditLogUser]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.AuditLogUserName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.BillingBranch]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.BillingDepartment]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.BillingOperator]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_ApplicationCode]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_ETAOfDischarge]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_ETDOfLoading]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_SystemCreateUser]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_SystemCreateTimeUtc]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_SystemLastEditUser]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.JE_SystemLastEditTimeUtc]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientCode]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientName]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientAddressAsString]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientAddressShortCode]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientAddress1]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientAddress2]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientCity]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientState]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.LocalClientCountry]);
				AssertNotNull(grid.Columns[JobDeclaration.Schema.RelatedTransportBookingsJobNumbers]);
				AssertNotNull(grid.Columns["Job+JH_Status"]);
			}
		}

		public void TestInitializeAdditionalColumns_TaxBranch()
		{
			var newCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var form = new ZForm())
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var filterControl = new JobDeclarationFilterStripControl(null, newCollection, new JobDeclarationFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNotNull(grid.Columns["BillingTaxBranch"]);
			}

			using (var form = new ZForm())
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var filterControl = new JobDeclarationFilterStripControl(null, newCollection, new JobDeclarationFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNull(grid.Columns["BillingTaxBranch"]);
			}
		}

		public void TestManifestDetailsColumnsVisibilitySwitchViaAccessRegistry()
		{
			var newCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControl(null, newCollection, new JobDeclarationFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNull(grid.Columns[JobDeclaration.Schema.ManifestStatus]);
				AssertNull(grid.Columns[JobDeclaration.Schema.ManifestStatusDescription]);
			}

			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControl(null, newCollection, new JobDeclarationFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				var manifetStatus = grid.Columns[JobDeclaration.Schema.ManifestStatus];
				var manifetStatusDescription = grid.Columns[JobDeclaration.Schema.ManifestStatusDescription];
				Assert(manifetStatus.IsVisible);
				AssertEquals("Manifest Details", manifetStatus.GroupName.Caption);
				AssertEquals("132D95D0-302D-49D0-BB2D-E5E59CC902EB", manifetStatus.GroupName.Key);
				Assert(manifetStatusDescription.IsVisible);
				AssertEquals("Manifest Details", manifetStatusDescription.GroupName.Caption);
				AssertEquals("132D95D0-302D-49D0-BB2D-E5E59CC902EB", manifetStatusDescription.GroupName.Key);
			}
		}

		[RequiresSTA]
		public new void TestCustomColumn()
		{
			var newCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			template.P0_GC = GlbCompany.CurrentCompany.PK;
			template.P0_GB = GlbBranch.CurrentBranch.PK;
			template.P0_GE = GlbDepartment.CurrentDepartment.PK;
			var def = template.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "custom";
			def.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;
			Factory.Save();
			using (var form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControl(null, newCollection, new JobDeclarationFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("custom", typeof(ZString))]);
			}
		}

		public void TestManageLayoutsFormOpen()
		{
			var newCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.Customs.JobDeclaration, "First", false, false, false);
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.Customs.JobDeclaration, "Second", false, false, false);
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.Customs.JobDeclaration, "Third", false, false, false);
			Factory.Save();
			using (var form = new ZForm())
			using (var module = new JobDeclarationModule())
			using (var filterControl = new JobDeclarationFilterStripControl(module, newCollection, new JobDeclarationFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();
				using (var manageLayoutForm = new ZArchitecture.GUI.Internal.ManageLayoutsForm(module.FilterBusinessObject, true, true, false))
				{
					AssertNoExceptionThrown(() =>
					{
						manageLayoutForm.Show();
						Application.DoEvents();
					});
				}
			}
		}
	}
}
