using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class SGInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		public SGInvoiceLineUserControl()
		{
			InitializeComponent();
			ContainersTabPage.TabVisible = false;
		}

		public override Customs.Business.ICommonInvoiceDataProvider JobDeclaration
		{
			get { return base.JobDeclaration; }
			set
			{
				UnHookValueChanged(JobDeclaration as ICommonInvoiceDataProvider);
				base.JobDeclaration = value;
				var declaration = JobDeclaration as ICommonInvoiceDataProvider;
				HookValueChanged(declaration);
				ShowOrHideCertificateOfOriginTabPage(declaration);
				ShowOrHideOtherTaxField(declaration);
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			InvoiceLineCharges.RemoveColumn(JobComInvHeaderChargeSchema.Constants.J7_IsDutiable);
			InvoiceLineCharges.RemoveColumn(JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable);

			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var brandNameColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|d99bd2a5-0ef3-41e7-a6eb-1818c3188a23", "Brand Name"),
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = "JI_BrandName",
					IsVisible = false,
				};
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(brandNameColumnStyleInfo);

				var modelColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|ac0251de-b729-4549-99de-66bb76cbd81b", "Model"),
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = "JI_Model",
					IsVisible = false,
				};
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(modelColumnStyleInfo);

				CustomsInvoiceLinesBoundGrid.ReOrderColumns(ColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(false, ColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, defaultColumnsForGrid);
				ZGridColumnInfo columnInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CC);
				if (columnInfo != null)
				{
					columnInfo.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|FEF97B54-586D-47D9-AEFD-6F75151AD1FC", "Classification");
					ControlDpiScalingHelper.SetWidth(ref columnInfo, 80, true);
				}

				columnInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.UnitPrice);
				if (columnInfo != null)
				{
					ControlDpiScalingHelper.SetWidth(ref columnInfo, 60, true);
				}

				columnInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_LineNo);
				if (columnInfo != null)
				{
					columnInfo.IsReadOnly = true;
				}
			}
		}

		protected override ZArchitecture.Modules.ModuleIdentifier ClassificationModuleID
		{
			get { return Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.SG.SG4Classification; }
		}

		void zStgcGovLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				WebUrlLauncher.Launch(SGConstants.STGCWebURL);
				zStgcGovLink.LinkVisited = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void HookValueChanged(ICommonInvoiceDataProvider provider)
		{
			if (provider != null)
			{
				provider.JE_MessageTypeInfo.ValueChanged += new EventHandler(JE_MessageTypeInfo_ValueChanged);
				if (provider is JobDeclaration declaration)
				{
					declaration.JE_MessageSubTypeInfo.ValueChanged += new EventHandler(JE_MessageSubTypeInfo_ValueChanged);
				}
			}
		}

		void UnHookValueChanged(ICommonInvoiceDataProvider provider)
		{
			if (provider != null)
			{
				provider.JE_MessageTypeInfo.ValueChanged -= new EventHandler(JE_MessageTypeInfo_ValueChanged);
				if (provider is JobDeclaration declaration)
				{
					declaration.JE_MessageSubTypeInfo.ValueChanged -= new EventHandler(JE_MessageSubTypeInfo_ValueChanged);
				}
			}
		}

		void JE_MessageSubTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideCertificateOfOriginTabPage(JobDeclaration as ICommonInvoiceDataProvider);
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideCertificateOfOriginTabPage(JobDeclaration as ICommonInvoiceDataProvider);
		}

		void ShowOrHideCertificateOfOriginTabPage(ICommonInvoiceDataProvider provider)
		{
			CertificateOfOriginTabPage.TabVisible = (provider.JE_MessageType == MessageTypeCodeList.Codes.COO || (provider.JE_MessageType == MessageTypeCodeList.Codes.OUT && provider is JobDeclaration declaration && declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.BKO));
		}

		void ShowOrHideOtherTaxField(ICommonInvoiceDataProvider declaration)
		{
			if (declaration != null)
			{
				OtherTaxAmountCurrencyControl.Visible = declaration.IsTradeNet4Point1;
			}
		}

		protected override Universal.GUI.TariffColumnStyleInfo CreateUniversalTariffColumnStyleInfo(string columnName)
		{
			return new TariffColumnStyleInfo()
			{
				GetCountryCode = GetCustomsCountryCode,
				GetDataGrouping = GetDataGroupingForUniversalTariff,
				GetTariffType = GetUniversalTariffType,
				ColumnName = columnName
			};
		}

		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Singapore;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.Singapore;

		#region Invoice Line Grid Columns

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					List<string> columns = new List<string>();
					columns.AddRange(DefaultColumnsForGrid);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Weight);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_WeightUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeight);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeightUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Volume);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_VolumeUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_OrderNumber);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib2);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib3);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_SerialNumber);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.UnitPrice);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib2);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib3);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib4);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib5);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib6);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomTextBlob1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_BrandName);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Model);
					columnNamesInSortOrder = columns.ToArray();
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;

		string[] DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					List<string> columns = new List<string>();
					columns.Add(JobComInvoiceLineSchema.Constants.JI_LineNo);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartNo);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CC);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Tariff);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_LinePrice);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Description);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_RH_NKCommodity_Code);

					defaultColumnsForGrid = columns.ToArray();
				}

				return defaultColumnsForGrid;
			}
		}
		string[] defaultColumnsForGrid;

		#endregion
	}
}
