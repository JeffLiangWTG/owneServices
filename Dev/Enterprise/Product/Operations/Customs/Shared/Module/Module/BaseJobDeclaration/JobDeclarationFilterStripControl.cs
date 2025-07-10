using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	/// <summary>
	/// Filter control for JobDeclaration.
	/// </summary>
	public partial class JobDeclarationFilterStripControl : ZFilterStripControl<MasterFiles.Module.WorkflowFilterStrip>
	{
		public JobDeclarationFilterStripControl()
		{
			InitializeComponent();
		}

		public JobDeclarationFilterStripControl(JobDeclarationModuleBase module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.module = module;
			this.filterStripBusinessObject = filterBusinessObject as JobDeclarationFilterBusinessObject;
			InitializeComponent();
			AddCustomFieldsToGrid();
			AddWHSStatusFieldToGrid();
			AddApplicationCodeToGrid();
			AddLocalClientToGrid();
			AddRelatedTransportBookingsColumnToGrid();
			AddDeclarantFieldsToGrid();
			SetScreeningStatusCaption();

			SetDateTimeFormat();
			SetStatusVisibility();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new JobDeclarationModuleStrip();
		}

		void SetScreeningStatusCaption()
		{
			var screeningStatusTextBoxColumnStyle = FilteredGrid.GetColumnStyle(BaseJobDeclaration.Schema.JE_ScreeningStatus);
			if (screeningStatusTextBoxColumnStyle != null)
			{
				var screeningStatusLabel = (!ComplianceRiskHelper.CheckIfComplianceRiskEnabled(typeof(BaseJobDeclaration), allowViewType: false)) ? Res.GetData("B8DBC236-5512-4529-98E5-9136DF13C6F4", "Screening Status") : Res.GetData("4315E42A-D7FB-435C-ADF4-4EF8C80260E9", "Legacy Screening Status");
				screeningStatusTextBoxColumnStyle.CaptionResourceString = screeningStatusLabel;
			}
		}

		void SetDateTimeFormat()
		{
			var submittedDateColumnStyle = FilteredGrid.GetColumnStyle(JobDeclarationSchema.Constants.JE_EntrySubmittedDate);

			if (IsEntrySubmitDateShort && submittedDateColumnStyle is ZDateEditColumnStyleInfo)
			{
				var columnStyleInfo = (ZDateEditColumnStyleInfo)submittedDateColumnStyle;
				columnStyleInfo.DateTimeFormat = ZDateTimePickerFormat.Short;
			}
		}

		void SetStatusVisibility()
		{
			if (DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(GlbCompany.CurrentCompany.Country.RN_Code, GlbCompany.CurrentCompany.PK))
			{
				var messageStatusTextBoxColumnStyle = FilteredGrid.GetColumnStyle(BaseJobDeclaration.Schema.JE_MessageStatus);
				messageStatusTextBoxColumnStyle.IsVisible = false;

				var messageStatusDescriptionTextBoxColumnStyle = FilteredGrid.GetColumnStyle(BaseJobDeclaration.Schema.JE_MessageStatusDescription);
				messageStatusDescriptionTextBoxColumnStyle.IsVisible = false;
			}
		}

		protected virtual bool ShouldAddDeclarantFieldsToGrid => false;

		void AddDeclarantFieldsToGrid()
		{
			if (ShouldAddDeclarantFieldsToGrid) 
			{
				var declarantCodeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				declarantCodeColumnStyleInfo.CaptionResourceString = Res.GetData("1EE7F45F-66F1-4012-8154-9E62EED6950E", "Declarant");
				declarantCodeColumnStyleInfo.ColumnName = nameof(BaseJobDeclaration.DeclarantCode);
				declarantCodeColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
				this.grid.ColumnStyles.Add(declarantCodeColumnStyleInfo);

				var declarantNameColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				declarantNameColumnStyleInfo.CaptionResourceString = Res.GetData("958383D2-FFCE-42DE-AE0D-2C2ED28D7EB2", "Declarant Name");
				declarantNameColumnStyleInfo.ColumnName = nameof(BaseJobDeclaration.DeclarantName);
				declarantNameColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
				declarantNameColumnStyleInfo.IsVisible = false;
				this.grid.ColumnStyles.Add(declarantNameColumnStyleInfo);
			}
		}

		void AddWHSStatusFieldToGrid()
		{
			if (ShouldAddWHSStatusFieldToGrid)
			{
				var whsStatusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				whsStatusTextBoxColumnStyleInfo.Caption = Res.GetString("1a6a477f-729a-4c7a-841b-868525c2f1c6", "WHS Status");
				whsStatusTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				whsStatusTextBoxColumnStyleInfo.ColumnName = "WarehouseTransactionStatusDescription";
				whsStatusTextBoxColumnStyleInfo.IsVisible = false;
				ControlDpiScalingHelper.SetWidth(ref whsStatusTextBoxColumnStyleInfo, 150, true);
				this.FilteredGrid.ColumnStyles.Add(whsStatusTextBoxColumnStyleInfo);
			}
		}

		protected virtual ZBool ShouldAddWHSStatusFieldToGrid
		{
			get { return false; }
		}

		protected virtual ZBool ShouldAddCustomFieldsToGrid
		{
			get { return true; }
		}

		void AddCustomFieldsToGrid()
		{
			if (ShouldAddCustomFieldsToGrid)
			{
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, GridCollection, JobInvoicingConsumerTypes.Brokerage.Code, false, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			}
		}

		void AddApplicationCodeToGrid()
		{
			var filterBusinessObject = (module?.FilterBusinessObject ?? this.filterStripBusinessObject) as JobDeclarationFilterBusinessObject;
			if (filterBusinessObject != null)
			{
				var applicationCodeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo
				{
					Caption = filterBusinessObject.ApplicationCodeFilterCaption,
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = "JE_ApplicationCode",
					IsVisible = true
				};
				ControlDpiScalingHelper.SetWidth(ref applicationCodeTextBoxColumnStyleInfo, 90, true);
				FilteredGrid.ColumnStyles.Add(applicationCodeTextBoxColumnStyleInfo);
			}
		}

		void AddLocalClientToGrid()
		{
			var columnStyleInfos = new ZGridColumnInfo[] {
										new ZTextBoxColumnStyleInfo
										{
											CharacterCasing = CharacterCasing.Upper,
											ColumnName = BaseJobDeclaration.Schema.LocalClientCode,
											Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
											IsVisible = false
										},
										new ZTextBoxColumnStyleInfo
										{
											CharacterCasing = CharacterCasing.Upper,
											ColumnName = BaseJobDeclaration.Schema.LocalClientName,
											Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
											IsVisible = false
										},
										new ZTextBoxColumnStyleInfo
										{
											CharacterCasing = CharacterCasing.Upper,
											ColumnName = BaseJobDeclaration.Schema.LocalClientAddressAsString,
											Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
											IsVisible = false
										},
										new ZTextBoxColumnStyleInfo
										{
											CharacterCasing = CharacterCasing.Upper,
											ColumnName = BaseJobDeclaration.Schema.LocalClientAddressShortCode,
											Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
											IsVisible = false
										},
										new ZTextBoxColumnStyleInfo
										{
											CharacterCasing = CharacterCasing.Upper,
											ColumnName = BaseJobDeclaration.Schema.LocalClientAddress1,
											GroupName = LocalClientAddressDetails,
											Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
											IsVisible = false
										},
										new ZTextBoxColumnStyleInfo
										{
											CharacterCasing = CharacterCasing.Upper,
											ColumnName = BaseJobDeclaration.Schema.LocalClientAddress2,
											GroupName = LocalClientAddressDetails,
											Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
											IsVisible = false
										},
										new ZTextBoxColumnStyleInfo
										{
											CharacterCasing = CharacterCasing.Upper,
											ColumnName = BaseJobDeclaration.Schema.LocalClientCity,
											GroupName = LocalClientAddressDetails,
											Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
											IsVisible = false
										},
										new ZTextBoxColumnStyleInfo
										{
											CharacterCasing = CharacterCasing.Upper,
											ColumnName = BaseJobDeclaration.Schema.LocalClientState,
											GroupName = LocalClientAddressDetails,
											Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
											IsVisible = false
										},
										new ZTextBoxColumnStyleInfo
										{
											CharacterCasing = CharacterCasing.Upper,
											ColumnName = BaseJobDeclaration.Schema.LocalClientCountry,
											GroupName = LocalClientAddressDetails,
											Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
											IsVisible = false
										},
									};

			FilteredGrid.ColumnStyles.AddRange(columnStyleInfos);
		}

		ResourceStringData LocalClientAddressDetails => Res.GetData("7438F939-3857-4BB8-9DA9-DCD3201F7668", "Local Client Address Details");

		void AddRelatedTransportBookingsColumnToGrid()
		{
			var columnStyle = new ZTextBoxColumnStyleInfo
			{
				CharacterCasing = CharacterCasing.Upper,
				ColumnName = BaseJobDeclaration.Schema.RelatedTransportBookingsJobNumbers,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
				IsVisible = false
			};
			FilteredGrid.ColumnStyles.Add(columnStyle);
		}

		protected void SetMenuVisibility()
		{
			if (FilteredGrid.ContextMenu != null)
			{
				if (module != null && module is JobDeclarationModule derivedModule && derivedModule.CopyDeclarationOnlyMenuItem != null)
				{
					BusinessObject[] selectedElements = GetSelectedElements();
					if (selectedElements != null && selectedElements.Length == 1)
					{
						BaseJobDeclaration declaration = (BaseJobDeclaration)selectedElements[0];
						derivedModule.CopyDeclarationOnlyMenuItem.Visible = !declaration.JE_JS.IsEmpty;
					}
					else
					{
						derivedModule.CopyDeclarationOnlyMenuItem.Visible = false;
					}
				}
			}
		}

		protected virtual BusinessObject[] GetSelectedElements() => FilteredGrid.SelectedElements;

		protected virtual bool IsEntrySubmitDateShort => false;

		void JobDeclarationFilterControl_Load(object sender, EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				new CustomFieldColumnCreator().Set(FilteredGrid, new JobDocsAndCartageCustomFieldsDescriptor(), BaseJobDeclaration.Schema.DocsAndCartage, true, true);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				FilteredGrid.SetAvailability(AccountingMasterFilesUtils.IsTaxBranchApplicable, "BillingTaxBranch");
			}
		}

		readonly JobDeclarationModuleBase module;
		readonly JobDeclarationFilterBusinessObject filterStripBusinessObject;
	}
}
