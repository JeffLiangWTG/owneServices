using System;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class JobDeclarationFilterControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestWarehouseDetailsColumns()
		{
			using (var form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControl(null, JobDeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_WHSEntryFilerCode]);
				AssertEquals("Warehouse Details", filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_WHSEntryFilerCode].GroupName.Caption);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_WHSEntryNumber]);
				AssertEquals("Warehouse Details", filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_WHSEntryNumber].GroupName.Caption);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_IsFinalWHS]);
				AssertEquals("Warehouse Details", filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_IsFinalWHS].GroupName.Caption);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_QtyInWHBeforeWithdrawal]);
				AssertEquals("Warehouse Details", filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_QtyInWHBeforeWithdrawal].GroupName.Caption);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_QtyBeingWithdrawn]);
				AssertEquals("Warehouse Details", filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_QtyBeingWithdrawn].GroupName.Caption);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_QtyInWHAfterWithdrawal]);
				AssertEquals("Warehouse Details", filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_QtyInWHAfterWithdrawal].GroupName.Caption);
			}
		}

		public void TestFilterGridColorContextKey()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var module = new JobDeclarationModule())
			using (var filterControl = new JobDeclarationFilterStripControl(module, declarations, module.FilterBusinessObject))
			{
				filterControl.OnLoad_Exposed();
				AssertEquals(ModuleIDs.Customs.JobDeclaration.Name, filterControl.FilteredGrid.ColorContextKey);
			}
		}

		public void TestInBondClosedDateColumn()
		{
			using (var form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControl(null, JobDeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var column = filterControl.FilteredGrid.Columns[JobDeclaration.Schema.InBondClosedDate];
				AssertNotNull(column);
				AssertEquals(column.ColumnStyle.GetType(), typeof(ZTextBoxColumnStyle));
			}
		}

		public void TestSetDefaultColumnsVisibility()
		{
			using (var form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControl(null, JobDeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();

				foreach (ZGridColumnInfo column in filterControl.FilteredGrid.ColumnStyles)
				{
					if (column.ColumnName == JobDeclarationSchema.Constants.JE_GB
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_MessageType
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_TransportMode
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_DeclarationReference
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_VesselName
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_VoyageFlightNo
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_RL_NKOrigin
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_RL_NKFinalDestination
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_MasterBill
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_HouseBill
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_ScreeningStatus
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_OH_Importer
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_OH_Supplier
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_AgentsReference
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_OwnerRef
							|| column.ColumnName == JobDeclaration.Schema.OrderNumbers
							|| column.ColumnName == JobDeclaration.Schema.DeclarationNumber
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_EntrySubmittedDate
							|| column.ColumnName == JobDeclaration.Schema.JE_EntryStatusDescription)
					{
						AssertEquals(column.ColumnName, true, column.IsVisible);
					}
					else if (column.ColumnName == JobDeclaration.Schema.US_EntryDate
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_TotalVolume
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_TotalVolumeUnit
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_TotalWeight
							|| column.ColumnName == JobDeclarationSchema.Constants.JE_TotalWeightUnit
							|| column.ColumnName == JobDeclaration.Constants.GenAddOnColumnFieldName.US_ConsolidatedJobNumber
							|| column.ColumnName == JobDeclaration.Schema.ElectronicInvoiceStatus
							|| column.ColumnName == JobDeclaration.Schema.FreightContainerMode
							|| column.ColumnName == JobDeclaration.Schema.ElectronicInvoiceStatusDescription
							|| column.ColumnName == JobDeclaration.Schema.ISFBillStatus
							|| column.ColumnName == JobDeclaration.Schema.ISFBillStatusDescription
							|| column.ColumnName == JobDeclaration.Schema.US_SchDExport)
					{
						AssertEquals(column.ColumnName, false, column.IsVisible);
					}
				}
			}
		}

		public void TestInitializeAdditionalColumns()
		{
			using (var form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControl(null, JobDeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertNotNull(filterControl.FilteredGrid.Columns[USAddInfoSchema.Constants.US_DateOfExport]);
				AssertNull(filterControl.FilteredGrid.Columns[JobDeclarationSchema.Constants.JE_ExportDate]);
				AssertNotNull(filterControl.FilteredGrid.Columns[USAddInfoSchema.Constants.US_EnableCRL]);
				AssertNotNull(filterControl.FilteredGrid.Columns[USAddInfoSchema.Constants.US_EnableAII]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.ConsigneeAddressOrgPK]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.IOROrgPK]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.ElectronicInvoiceStatus]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.ElectronicInvoiceStatusDescription]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_EntryFilerCode]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.BrokerName]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.ImporterName]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.SupplierName]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.ForwarderName]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.IORName]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.UltimateConsigneeName]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.JE_ScreeningStatus]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.FreightContainerMode]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_SchDExport]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.InBondClosedDate]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_InsuranceDisposition]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_PresentationDate]);
				AssertNotNull(filterControl.FilteredGrid.Columns["Job+JH_Status"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["Job+JH_HoldReason"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["Job+JH_ProfitLossReasonCode"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["Job+JH_TotalProfitRevenueMargin"]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.ImporterEIN]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.ImporterOfRecordEIN]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_PreparerDistrictPort]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.JE_ETAOfDischarge]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.JE_ETDOfLoading]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.JE_SystemCreateUser]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.JE_SystemCreateTimeUtc]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.JE_SystemLastEditUser]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.JE_SystemLastEditTimeUtc]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+DescriptionWithReference"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_ActualDate"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+Milestones+CurrentCompanyLastMilestone+DescriptionWithReference"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_ActualDate"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+DescriptionWithReference"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_ScheduledDate"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+Milestones+CurrentCompanyNextMilestone+DescriptionWithReference"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_ScheduledDate"]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.LocalClientCode]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.LocalClientName]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.LocalClientAddressAsString]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.LocalClientAddressShortCode]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.LocalClientAddress1]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.LocalClientAddress2]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.LocalClientCity]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.LocalClientState]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.LocalClientCountry]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.RelatedTransportBookingsJobNumbers]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.JE_FCLDeliveryOrPickupEquipmentNeeded]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.DeliveryOrPickupCartageCoPK]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.AESResponseCode]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.AESResponseCodeDescription]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.AESSeverity]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.AESSeverityDescription]);
			}
		}

		public void TestInitializedAdditionalColumnForACE()
		{
			using (var form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControl(null, JobDeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.JE_ApplicationCode]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.SoldToPartyOrgPK]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.JE_OH_NotifyParty]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_PSC]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_BondSuperseding]);
				AssertNotNull(filterControl.FilteredGrid.Columns[JobDeclaration.Schema.US_BondWaiverCode]);
			}
		}

		public void TestInitializeAdditionalColumns_TaxBranch()
		{
			using (var form = new ZForm())
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var filterControl = new JobDeclarationFilterStripControl(null, JobDeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNotNull(grid.Columns["BillingTaxBranch"]);
			}

			using (var form = new ZForm())
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var filterControl = new JobDeclarationFilterStripControl(null, JobDeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNull(grid.Columns["BillingTaxBranch"]);
			}
		}

		public new void TestCustomColumn()
		{
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
			using (var filterControl = new JobDeclarationFilterStripControl(null, JobDeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("custom", typeof(ZString))]);
			}
		}

		public void TestSetColumnCaption()
		{
			using (var form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControl(null, JobDeclarationCollection, FilterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				foreach (ZGridColumnInfo column in filterControl.FilteredGrid.ColumnStyles)
				{
					if (column.ColumnName == JobDeclarationSchema.Constants.JE_EntryAuthorisationDate)
					{
						AssertEquals(column.Caption, DeclarationFilterConstants.ColumnnCaptions.ReleaseDate);
					}

					if (column.ColumnName == JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival)
					{
						AssertEquals(column.Caption, DeclarationFilterConstants.ColumnnCaptions.Discharge);
					}
				}
			}
		}

		JobDeclarationCollection jobDeclarationCollection;
		JobDeclarationCollection JobDeclarationCollection => jobDeclarationCollection ?? (jobDeclarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK));

		JobDeclarationFilterBusinessObject filterBO;
		JobDeclarationFilterBusinessObject FilterBO => filterBO ?? (filterBO = new JobDeclarationFilterBusinessObject());
	}
}
