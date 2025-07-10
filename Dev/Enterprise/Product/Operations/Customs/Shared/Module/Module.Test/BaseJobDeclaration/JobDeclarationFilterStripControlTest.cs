using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	public class JobDeclarationFilterStripControlTest : ZFilterStripControlTest
	{
		[RequiresSTA]
		public void TestJobStatus()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var userControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;
				var columnInfo = grid.GetColumnStyle("Job+JH_Status");
				AssertNotNull(columnInfo);
			}
		}

		[RequiresSTA]
		public void TestHoldReason()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var filterControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				var holdReason = nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_HoldReason);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(col => col.ColumnName == holdReason && !col.IsVisible);

				Assert("Hold Reason column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestProfitLossReason()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var filterControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				var profitLossReason = nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_ProfitLossReasonCode);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(col => col.ColumnName == profitLossReason && !col.IsVisible);

				Assert("Profit/Loss Reason column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestTotalProfitRevenueMargin()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var filterControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				var margin = nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_TotalProfitRevenueMargin);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(col => col.ColumnName == margin && !col.IsVisible);

				Assert("Margin% column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestJobTaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNotNull(GetTaxBranchColumn());
			}

			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNull(GetTaxBranchColumn());
			}
		}

		ZGridColumn GetTaxBranchColumn()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var form = new ZForm())
			{
				var userControl = new JobDeclarationFilterStripControl(null, declarations, filterBO);
				form.Controls.Add(userControl);
				form.Show();
				Application.DoEvents();
				return userControl.FilteredGrid.Columns["BillingTaxBranch"];
			}
		}

		[RequiresSTA]
		public void TestApplicationCode()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var module = new JobDeclarationModule())
			using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;
				var columnInfo = grid.GetColumnStyle("JE_ApplicationCode");
				AssertNotNull(columnInfo);
			}
		}

		#region Pickup or Delivery Drop Mode column

		[RequiresSTA]
		public void TestPickupOrDeliveryDropMode()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var module = new JobDeclarationModule())
			using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;
				var columnInfo = grid.GetColumnStyle("JE_FCLDeliveryOrPickupEquipmentNeeded");
				AssertNotNull(columnInfo);
			}
		}

		#endregion

		#region Pickup or Delivery Transport Company

		[RequiresSTA]
		public void TestPickupOrDeliveryTransportCompany()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var module = new JobDeclarationModule())
			using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;
				var columnInfo = grid.GetColumnStyle("DeliveryOrPickupCartageCoPK");
				AssertNotNull(columnInfo);
			}
		}

		#endregion

		[RequiresSTA]
		public void TestDeclarationSelection()
		{
			BaseJobDeclarationCollection newCollection = new BaseJobDeclarationCollection(Factory);
			BaseJobDeclaration declaration1 = newCollection.AddNew();
			declaration1.JE_JS = ZGuid.NewZGuid();
			BaseJobDeclaration declaration2 = newCollection.AddNew();
			declaration2.JE_JS = ZGuid.Empty;
			using (ZForm form = new ZForm())
			using (JobDeclarationModule module = new JobDeclarationModule())
			using (JobDeclarationFilterStripControlForTest control = new JobDeclarationFilterStripControlForTest(module, newCollection, new JobDeclarationFilterBusinessObject()))
			{
				form.Controls.Add(control);
				form.Show();
				control.Find();
				control.FilteredGrid.ContextMenu.Dispose();
				control.FilteredGrid.ContextMenu = ((ZDisplayGrid)module.DisplayGrid).ContextMenu;
				control.SetMenuVisibility();
				AssertEquals(false, control.FilteredGrid.ContextMenu.MenuItems.FindByText("Actions").MenuItems.FindByText(JobDeclarationModule.CopyDeclarationOnlyText).Visible);
				control.selectedElements = new BaseJobDeclaration[1];
				control.selectedElements[0] = declaration1;
				control.SetMenuVisibility();
				AssertEquals(true, control.FilteredGrid.ContextMenu.MenuItems.FindByText("Actions").MenuItems.FindByText(JobDeclarationModule.CopyDeclarationOnlyText).Visible);
				control.selectedElements[0] = declaration2;
				control.SetMenuVisibility();
				AssertEquals(false, control.FilteredGrid.ContextMenu.MenuItems.FindByText("Actions").MenuItems.FindByText(JobDeclarationModule.CopyDeclarationOnlyText).Visible);
			}
		}

		public void TestWorkflowFilterStripIsInherited()
		{
			AssertEquals("Must inherit ZFilterStripControl<WorkflowFilterStrip> so that workflow filter strips may be selected", true, typeof(JobDeclarationFilterStripControl).IsSubclassOf(typeof(ZFilterStripControl<MasterFiles.Module.WorkflowFilterStrip>)));
		}

		[RequiresSTA]
		public void TestCustomColumn()
		{
			BaseJobDeclarationCollection newCollection = new BaseJobDeclarationCollection(Factory);
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			template.P0_GC = GlbCompany.CurrentCompany.PK;
			template.P0_GB = GlbBranch.CurrentBranch.PK;
			template.P0_GE = GlbDepartment.CurrentDepartment.PK;
			var def = template.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "custom";
			def.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;
			Factory.Save();
			using (ZForm form = new ZForm())
			using (var filterControl = new JobDeclarationFilterStripControlForTest(null, newCollection, filterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("custom", typeof(ZString))]);
			}
		}

		[RequiresSTA]
		public void TestDeclarationGridColumns()
		{
			BaseJobDeclarationCollection declarations = new BaseJobDeclarationCollection(Factory);
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			using (ZForm form = new ZForm())
			{
				using (var userControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
				{
					form.Show();
					var grid = userControl.FilteredGrid;
					AssertNotNull(grid.GetColumnStyle(JobDeclarationSchema.Constants.JE_GB));
					AssertNotNull(grid.GetColumnStyle("BillingBranch"));
					AssertNotNull(grid.GetColumnStyle("BillingDepartment"));
					AssertNotNull(grid.GetColumnStyle("BillingOperator"));
					AssertNotNull(grid.GetColumnStyle("LocalClientCode"));
					AssertNotNull(grid.GetColumnStyle("LocalClientName"));
					AssertNotNull(grid.GetColumnStyle("LocalClientAddressAsString"));
					AssertNotNull(grid.GetColumnStyle("LocalClientAddressShortCode"));
					AssertNotNull(grid.GetColumnStyle("LocalClientAddress1"));
					AssertNotNull(grid.GetColumnStyle("LocalClientAddress2"));
					AssertNotNull(grid.GetColumnStyle("LocalClientCity"));
					AssertNotNull(grid.GetColumnStyle("LocalClientState"));
					AssertNotNull(grid.GetColumnStyle("LocalClientCountry"));
					AssertNotNull(grid.GetColumnStyle("RelatedTransportBookingsJobNumbers"));
					AssertEquals("Pack Type (Declaration)", grid.GetColumnStyle("JE_TotalNoOfPacksPackType").CaptionResourceString.Caption);
				}
			}
		}

		public void TestDeclarationComplianceGridColumns()
		{
			AssertDeclarationComplianceGridColumns(true);
			AssertDeclarationComplianceGridColumns(false);

			void AssertDeclarationComplianceGridColumns(bool enableCompliance)
			{
				BaseJobDeclarationCollection declarations = new BaseJobDeclarationCollection(Factory);
				JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();

				var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(enableCompliance);
				featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
				using (ObjectFactory.Substitute(featureControlMock.Object))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ZForm form = new ZForm())
				{
					using (var userControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
					{
						form.Show();
						var grid = userControl.FilteredGrid;
						AssertNotNull(grid.GetColumnStyle(JobDeclarationSchema.Constants.JE_ScreeningStatus));
						if (enableCompliance)
						{
							AssertEquals("Legacy Screening Status", grid.GetColumnStyle(JobDeclarationSchema.Constants.JE_ScreeningStatus).CaptionResourceString.Caption);
						}
						else
						{
							AssertEquals("Screening Status", grid.GetColumnStyle(JobDeclarationSchema.Constants.JE_ScreeningStatus).CaptionResourceString.Caption);
						}
					}
				}
			}
		}

		public void TestDeclarantColumns_Enabled()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();
			var mock = new Mock<JobDeclarationFilterStripControl>(null, declarations, filterBO) { CallBase = true };
			mock.Protected().SetupGet<bool>("ShouldAddDeclarantFieldsToGrid").Returns(true);
			using (var userControl = mock.Object)
			{
				var grid = userControl.FilteredGrid;

				AssertNotNull(grid.GetColumnStyle("DeclarantCode"));
				AssertNotNull(grid.GetColumnStyle("DeclarantName"));
			}
		}

		public void TestDeclarantColumns_Disabled()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();
			var mock = new Mock<JobDeclarationFilterStripControl>(null, declarations, filterBO) { CallBase = true };
			mock.Protected().SetupGet<bool>("ShouldAddDeclarantFieldsToGrid").Returns(false);
			using (var userControl = mock.Object)
			{
				var grid = userControl.FilteredGrid;

				AssertNull(grid.GetColumnStyle("DeclarantCode"));
				AssertNull(grid.GetColumnStyle("DeclarantName"));
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestLoadControl()
		{
			BaseJobDeclarationCollection declarations = new BaseJobDeclarationCollection(Factory);
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			using (ZForm form = new ZForm())
			{
				JobDeclarationFilterStripControl filterControl = new JobDeclarationFilterStripControl(null, declarations, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		[RequiresSTA]
		public void TestEntrySubmitDateFormat()
		{
			BaseJobDeclarationCollection declarations = new BaseJobDeclarationCollection(Factory);
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			using (JobDeclarationFilterStripControl userControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;
				var columnInfo = grid.GetColumnStyle(JobDeclarationSchema.Constants.JE_EntrySubmittedDate);
				AssertNotNull(columnInfo);
				var columnStyle = columnInfo as ZDateEditColumnStyleInfo;
				AssertNotNull(columnStyle);
				AssertEquals(ZDateTimePickerFormat.Long, columnStyle.DateTimeFormat);
			}
		}

		[RequiresSTA]
		public void TestEntryReleaseDateColumn()
		{
			BaseJobDeclarationCollection declarations = new BaseJobDeclarationCollection(Factory);
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			using (ZForm form = new ZForm())
			{
				using (var userControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
				{
					form.Show();
					var grid = userControl.FilteredGrid;
					var columnStyle = grid.GetColumnStyle("EntryReleaseDate");
					AssertNotNull(columnStyle);
					AssertEquals("Entry Release Date", columnStyle.CaptionResourceString.Caption);
					AssertEquals("Entry Release", columnStyle.CaptionResourceString.MediumCaption);
					AssertEquals("Entry Rel.", columnStyle.CaptionResourceString.ShortCaption);
					AssertType<ZTextBoxColumnStyleInfo>(columnStyle);
					AssertEquals("IsVisible", false, columnStyle.IsVisible);
				}
			}
		}

		public class JobDeclarationFilterStripControlForTest : JobDeclarationFilterStripControl
		{
			public JobDeclarationFilterStripControlForTest(JobDeclarationModule module, BusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(module, gridCollection, filterStripBusinessObject)
			{
			}

			public BusinessObject[] selectedElements;
			protected override BusinessObject[] GetSelectedElements()
			{
				return selectedElements;
			}

			public new void SetMenuVisibility()
			{
				base.SetMenuVisibility();
			}
		}
	}
}
