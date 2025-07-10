using System;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class ControllingMessageUserControl : BaseCustomsEntryUserControl
	{
		public ControllingMessageUserControl()
		{
			InitializeComponent();
			AddContextMenuOptionsForAssignCMHeaderToInvoicesToGrid();

			ControllingMessageDetailsGrid.AfterBind -= ControllingMessageDetailsGrid_AfterBind;
			ControllingMessageDetailsGrid.AfterBind += ControllingMessageDetailsGrid_AfterBind;

			Splitter.AllowOverlap(ControllingMessageDetailsGrid);
		}

		#region Assign CM Header to Invoice Lines
		ZMenuItem assignCMHeaderToInvoicesActionMenuItem;

		void AddContextMenuOptionsForAssignCMHeaderToInvoicesToGrid()
		{
			var assignCMHeaderToInvoicesSeperatorMenuItem = new ZMenuItem("-");
			assignCMHeaderToInvoicesActionMenuItem = new ZMenuItem(ResString.GetMultilingualString("84BDBCCC-CFC9-4D0A-A6D5-4B6B5DD84998", "&Assign CM Header to Invoice Lines"), AssignCMHeaderToInvoices_Click);

			var contextMenu = ControllingMessageDetailsGrid.ContextMenu;
			contextMenu.MenuItems.Add(assignCMHeaderToInvoicesSeperatorMenuItem);
			contextMenu.MenuItems.Add(assignCMHeaderToInvoicesActionMenuItem);
			contextMenu.Popup += ContextMenu_Popup;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			assignCMHeaderToInvoicesActionMenuItem.Enabled = ControllingMessageDetailsGrid.DeleteMenuItem?.Enabled ?? true;
		}

		void AssignCMHeaderToInvoices_Click(object sender, EventArgs e)
		{
			AutoAssignCMHeaderToInvoices();
		}

		void AutoAssignCMHeaderToInvoices()
		{
			CusTWControllingMessageHeader currentMessageHeader = null;
			var listManager = ControllingMessageDetailsGrid.ListManager;
			if (listManager != null)
			{
				currentMessageHeader = (CusTWControllingMessageHeader)listManager.GetCurrent();
				if (currentMessageHeader?.IsDeleted ?? false)
				{
					currentMessageHeader = null;
				}
			}

			var message = currentMessageHeader?.AssignHeaderToInvoiceLines() ?? ZString.Empty;
			if (!message.IsEmpty)
			{
				Globals.Message.ShowWarning(message);
			}
		}
		#endregion

		readonly string[] controllingMessageDetailsGridNotDisplayedColumnsImport = new[]
		{
			CusTWControllingMessageHeader.Schema.TW1_CertificateType,
			CusTWControllingMessageHeader.Schema.TW1_IsEstimatedLoadingDate,
			CusTWControllingMessageHeader.Schema.TW1_IsSpecialApplication,
			CusTWControllingMessageHeader.Schema.TW1_SpecialApplicationId,
			CusTWControllingMessageHeader.Schema.TW1_OriginalQuantity,
			CusTWControllingMessageHeader.Schema.TW1_CopyQuantity,
			CusTWControllingMessageHeader.Schema.TW1_EUSteelProductNo,
			CusTWControllingMessageHeader.Schema.TW1_EUSteelProductPhase,
			CusTWControllingMessageHeader.Schema.TW1_ManufacturerPrintingCode,
			CusTWControllingMessageHeader.Schema.TW1_ReturnPreviousCOO,
			CusTWControllingMessageHeader.Schema.TW1_BeforeClearanceApplicationReason,
			CusTWControllingMessageHeader.Schema.TW1_PrintingCode,
			CusTWControllingMessageHeader.Schema.TW1_IsTriangularTrade
		};

		readonly string[] controllingMessageDetailsGridNotDisplayedColumnsExport = new[]
		{
			CusTWControllingMessageHeader.Schema.BulkApplicationID,
			CusTWControllingMessageHeader.Schema.TW1_PortOfBulkCommodity,
			CusTWControllingMessageHeader.Schema.BulkPaymentID,
			CusTWControllingMessageHeader.Schema.PermitNoExpirationDate,
			CusTWControllingMessageHeader.Schema.TW1_ElectronicReceipt,
			CusTWControllingMessageHeader.Schema.TW1_ApplyForSampleReturn,
			CusTWControllingMessageHeader.Schema.TW1_InspectionRegistrationNumber,
			CusTWControllingMessageHeader.Schema.TW1_PreWineInspectionStatus,
			CusTWControllingMessageHeader.Schema.TW1_Purpose,
			CusTWControllingMessageHeader.Schema.TW1_SampleReturnAddress,
			CusTWControllingMessageHeader.Schema.TW1_SamplingReductionReason
		};

		readonly string[] invoiceLinesGridImportColumns = new[]
		{
			ControllingMessageHeaderLinkInvoiceLine.Schema.CitesPermit,
			ControllingMessageHeaderLinkInvoiceLine.Schema.HighTechLicense,
			ControllingMessageHeaderLinkInvoiceLine.Schema.TariffAdditionalCode,
			ControllingMessageHeaderLinkInvoiceLine.Schema.AlcoholPercentage,
			ControllingMessageHeaderLinkInvoiceLine.Schema.CusValueConvRatio,
			ControllingMessageHeaderLinkInvoiceLine.Schema.CarType,
			ControllingMessageHeaderLinkInvoiceLine.Schema.Transmission,
			ControllingMessageHeaderLinkInvoiceLine.Schema.EngineType,
			ControllingMessageHeaderLinkInvoiceLine.Schema.LHD,
			ControllingMessageHeaderLinkInvoiceLine.Schema.HasCatalystConverter,
			ControllingMessageHeaderLinkInvoiceLine.Schema.CarCondition,
			ControllingMessageHeaderLinkInvoiceLine.Schema.PrimaryPreference,
			ControllingMessageHeaderLinkInvoiceLine.Schema.ModelYear,
			ControllingMessageHeaderLinkInvoiceLine.Schema.Displacement,
			ControllingMessageHeaderLinkInvoiceLine.Schema.NumberOfDoor,
			ControllingMessageHeaderLinkInvoiceLine.Schema.Cylinders,
			ControllingMessageHeaderLinkInvoiceLine.Schema.Seats,
			ControllingMessageHeaderLinkInvoiceLine.Schema.Gears,
			ControllingMessageHeaderLinkInvoiceLine.Schema.DtyPymntMthd,
			ControllingMessageHeaderLinkInvoiceLine.Schema.VatPymntMthd,
			ControllingMessageHeaderLinkInvoiceLine.Schema.FormattedAdValoremDutyRate,
			ControllingMessageHeaderLinkInvoiceLine.Schema.FormattedSpecificDutyRate
		};

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			var declaration = (JobDeclaration)CurrentDataItem;
			var isImport = declaration?.IsImport ?? false;
			var isExport = declaration?.IsExport ?? false;

			using (InvoiceLinesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var procedureColumnInfo = InvoiceLinesGrid.GetColumnStyle(ControllingMessageHeaderLinkInvoiceLine.Schema.Procedure);
				if (isImport)
				{
					InvoiceLinesGrid.RemoveFromAvailableColumns(ControllingMessageHeaderLinkInvoiceLine.Schema.BondedGoodsCode);
					InvoiceLinesGrid.AddToAvailableColumns(invoiceLinesGridImportColumns);
					procedureColumnInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("ec7e09f9-cdf0-4648-b620-3fb2a5277a44", "Duty Treatment");
				}
				else
				{
					InvoiceLinesGrid.RemoveFromAvailableColumns(invoiceLinesGridImportColumns);
					InvoiceLinesGrid.AddToAvailableColumns(ControllingMessageHeaderLinkInvoiceLine.Schema.BondedGoodsCode);
					procedureColumnInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("6be92307-ea5e-4e6d-96a3-4e55a016221a", "Mode of Statistics");
				}
			}

			using (ControllingMessageDetailsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				if (isImport)
				{
					ControllingMessageDetailsGrid.RemoveFromAvailableColumns(controllingMessageDetailsGridNotDisplayedColumnsImport);
					ControllingMessageDetailsGrid.AddToAvailableColumns(controllingMessageDetailsGridNotDisplayedColumnsExport);
				}
				else if (isExport)
				{
					ControllingMessageDetailsGrid.RemoveFromAvailableColumns(controllingMessageDetailsGridNotDisplayedColumnsExport);
					ControllingMessageDetailsGrid.AddToAvailableColumns(controllingMessageDetailsGridNotDisplayedColumnsImport);
				}
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			ResetGridDefaultOrder();
		}

		void ResetGridDefaultOrder()
		{
			if (ControllingMessageDetailsGrid.ListManager?.GetCurrent() is CusTWControllingMessageHeader currentMessageHeader)
			{
				using (ControllingMessageDetailsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					ControllingMessageDetailsGrid.ReOrderColumns(controllingMessageDetailsGridDefaultColumnsInSortOrder);
				}
			}
		}

		readonly string[] controllingMessageDetailsGridDefaultColumnsInSortOrder = new string[]
		{
			CusTWControllingMessageHeader.Schema.TW1_Sequence,
			CusTWControllingMessageHeader.Schema.TW1_FunctionalReferenceId,
			CusTWControllingMessageHeader.Schema.TW1_ControllingMessageType,
			CusTWControllingMessageHeader.Schema.ControllingMessageTypeDescription,
			CusTWControllingMessageHeader.Schema.TW1_ControllingAgency,
			CusTWControllingMessageHeader.Schema.TW1_CertificateType,
			CusTWControllingMessageHeader.Schema.TW1_BusinessType,
			CusTWControllingMessageHeader.Schema.TW1_ProcessingUnit,
			CusTWControllingMessageHeader.Schema.TW1_ManufacturerPrintingCode,
			CusTWControllingMessageHeader.Schema.TW1_PrintingCode,
			CusTWControllingMessageHeader.Schema.TW1_OriginalQuantity,
			CusTWControllingMessageHeader.Schema.TW1_CopyQuantity,
			CusTWControllingMessageHeader.Schema.TW1_BeforeClearanceApplicationReason,
			CusTWControllingMessageHeader.Schema.TW1_IsTriangularTrade,
			CusTWControllingMessageHeader.Schema.TW1_IsEstimatedLoadingDate,
			CusTWControllingMessageHeader.Schema.TW1_PaymentMethod,
			CusTWControllingMessageHeader.Schema.TW1_Purpose,
			CusTWControllingMessageHeader.Schema.BulkPaymentID,
			CusTWControllingMessageHeader.Schema.TW1_ElectronicReceipt,
			CusTWControllingMessageHeader.Schema.TW1_AppointmentDate,
			CusTWControllingMessageHeader.Schema.TW1_AppointmentPeriod,
			CusTWControllingMessageHeader.Schema.TW1_ProofOfPaper,
			CusTWControllingMessageHeader.Schema.TW1_InspectionRegistrationNumber,
			CusTWControllingMessageHeader.Schema.TW1_PreWineInspectionStatus,
			CusTWControllingMessageHeader.Schema.PermitNumber,
			CusTWControllingMessageHeader.Schema.PermitNoExpirationDate,
			CusTWControllingMessageHeader.Schema.TW1_PrePermitNumber,
			CusTWControllingMessageHeader.Schema.BulkApplicationID,
			CusTWControllingMessageHeader.Schema.TW1_PortOfBulkCommodity,
			CusTWControllingMessageHeader.Schema.TW1_SampleReturnAddress,
			CusTWControllingMessageHeader.Schema.TW1_ApplyForSampleReturn,
			CusTWControllingMessageHeader.Schema.TW1_SamplingReductionReason,
			CusTWControllingMessageHeader.Schema.TW1_RequestDescription,
			CusTWControllingMessageHeader.Schema.TW1_ReturnPreviousCOO,
			CusTWControllingMessageHeader.Schema.TW1_IsSpecialApplication,
			CusTWControllingMessageHeader.Schema.TW1_SpecialApplicationId,
			CusTWControllingMessageHeader.Schema.TW1_EUSteelProductPhase,
			CusTWControllingMessageHeader.Schema.TW1_EUSteelProductNo,
			CusTWControllingMessageHeader.Schema.ProcessingNumber,
			CusTWControllingMessageHeader.Schema.CustomsMessageIdentifier,
			CusTWControllingMessageHeader.Schema.TW1_Observations,
			CusTWControllingMessageHeader.Schema.TW1_Remarks
		};

		void ControllingMessageDetailsGrid_AfterBind(object sender, EventArgs e)
		{
			ControllingMessageDetailsGrid.ListManager.CurrentChanged -= ControllingMessageDetailsGrid_ListManager_CurrentChanged;
			ControllingMessageDetailsGrid.ListManager.CurrentChanged += ControllingMessageDetailsGrid_ListManager_CurrentChanged;
			ControllingMessageDetailsGrid_ListManager_CurrentChanged(null, null);
		}

		void ControllingMessageDetailsGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			var currentGridItem = ControllingMessageDetailsGrid?.ListManager?.GetCurrent() as CusTWControllingMessageHeader;
			if (currentGridItem == null || currentGridItem.IsDeleted)
			{
				currentControllingMessageHeader = null;
			}
			else if (currentControllingMessageHeader != currentGridItem)
			{
				UnHookControllingMessageHeaderEvents(currentControllingMessageHeader);
				currentControllingMessageHeader = currentGridItem;
				HookControllingMessageHeaderEvents(currentControllingMessageHeader);
			}
		}

		void HookControllingMessageHeaderEvents(CusTWControllingMessageHeader controllingMessageHeader)
		{
			if (controllingMessageHeader != null)
			{
				controllingMessageHeader.TW1_ControllingMessageTypeInfo.ValueChanged += ChangeCertificateOfOriginTabPageTabVisible;
				controllingMessageHeader.TW1_CertificateTypeInfo.ValueChanged += ChangeECFAPrintedRemarksTextBoxEnable;
				ChangeCertificateOfOriginTabPageTabVisible(this, EventArgs.Empty);
			}
		}

		void UnHookControllingMessageHeaderEvents(CusTWControllingMessageHeader controllingMessageHeader)
		{
			if (controllingMessageHeader != null)
			{
				controllingMessageHeader.TW1_ControllingMessageTypeInfo.ValueChanged -= ChangeCertificateOfOriginTabPageTabVisible;
				controllingMessageHeader.TW1_CertificateTypeInfo.ValueChanged -= ChangeECFAPrintedRemarksTextBoxEnable;
			}
		}

		void ChangeCertificateOfOriginTabPageTabVisible(object sender, EventArgs e)
		{
			CertificateOfOriginTabPage.TabVisible = CurrentControllingMessageHeader?.IsNX101 ?? false;
		}

		void ChangeECFAPrintedRemarksTextBoxEnable(object sender, EventArgs e)
		{
			ECFAPrintedRemarksTextBox.Enabled = CurrentControllingMessageHeader?.IsCertificate15 ?? false;
		}

		CusTWControllingMessageHeader currentControllingMessageHeader;
		CusTWControllingMessageHeader CurrentControllingMessageHeader => currentControllingMessageHeader;
	}
}
