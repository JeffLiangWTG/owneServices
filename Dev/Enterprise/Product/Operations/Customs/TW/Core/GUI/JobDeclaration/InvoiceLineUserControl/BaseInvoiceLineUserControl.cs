using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI
{
	public partial class BaseInvoiceLineUserControl : DeclarationInvoiceLineUserControl
	{
		public BaseInvoiceLineUserControl()
		{
			InitializeComponent();

			AddLineDetailsUserControl();
			ResetCustomsInvoiceLinesBoundGridColumns();
			LineChargesTabPage.TabVisible = false;
		}

		protected virtual void ResetCustomsInvoiceLinesBoundGridColumns()
		{
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_Description));
			CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.UnitPrice);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo
			{
				CaptionResourceString = Res.GetData("211cd3cf-573c-4060-845d-da5d9cfbcd0f", "English Description"),
				ColumnName = JobComInvoiceLineSchema.Constants.JI_Description,
				ToolTip = Res.GetString("4cba5400-2e53-4127-9607-717e6fbd03e5", "Invoice line description"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200)
			});

			#region Permit columns

			var groupName = Res.GetData("331bd009-618b-422b-ac39-cd4c79c95dff", "Permit 1");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("e258530a-dc64-4e39-8aff-a948c8b2893b", "Permit No 1"),
				ColumnName = JobComInvoiceLine.Schema.PermitCusSupportingNo1,
				ToolTip = Res.GetString("5ab4d524-9be3-4ca0-9d20-604a06f21fcc", "Permit No 1"),
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("f09570c9-6ad2-4069-b3e4-6d9ffd4ded52", "Permit Line No 1"),
				ColumnName = JobComInvoiceLine.Schema.PermitCusSupportingLineNo1,
				ToolTip = Res.GetString("1ec06a23-61ab-4f8b-8aa8-acb874913cfa", "Permit Line No 1"),
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true
			});

			groupName = Res.GetData("af026606-f6f9-4221-8467-cf9f8121226b", "Permit 2");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("676dab27-652a-44ea-a229-b9c064dfb77f", "Permit No 2"),
				ColumnName = JobComInvoiceLine.Schema.PermitCusSupportingNo2,
				ToolTip = Res.GetString("1f9119f9-a838-4ca6-97a8-977fcd90eb88", "Permit No 2"),
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("aa119bfa-27ed-4efe-9608-c66386e1e935", "Permit Line No 2"),
				ColumnName = JobComInvoiceLine.Schema.PermitCusSupportingLineNo2,
				ToolTip = Res.GetString("324e1503-c37f-4382-895d-5f5a750c3741", "Permit Line No 2"),
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true
			});

			groupName = Res.GetData("c54a8e2a-4429-4bc7-ac5a-314fb948d70a", "Permit 3");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("147c1e5e-7d58-4425-b1d2-adb95f1b3f6b", "Permit No 3"),
				ColumnName = JobComInvoiceLine.Schema.PermitCusSupportingNo3,
				ToolTip = Res.GetString("bda3ead4-9ce5-41c6-9b46-97c0816a5e18", "Permit No 3"),
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("11042614-b6ff-409a-9205-112d42ac97d8", "Permit Line No 3"),
				ColumnName = JobComInvoiceLine.Schema.PermitCusSupportingLineNo3,
				ToolTip = Res.GetString("ea58ecf8-fab0-4011-815a-0518a95be7c9", "Permit Line No 3"),
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true
			});

			groupName = Res.GetData("905d3e35-1cbd-4327-bdfa-4fdc75af7458", "Permit 4");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("73a45759-9c19-4f9f-ab10-5babbddb410b", "Permit No 4"),
				ColumnName = JobComInvoiceLine.Schema.PermitCusSupportingNo4,
				ToolTip = Res.GetString("c2bb7e80-d47f-48e4-92ff-046055e67c11", "Permit No 4"),
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("99f6c138-1d6b-4010-ae04-340788d608fc", "Permit Line No 4"),
				ColumnName = JobComInvoiceLine.Schema.PermitCusSupportingLineNo4,
				ToolTip = Res.GetString("f882c5fe-1123-4aa1-bd29-508160845d01", "Permit Line No 4"),
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true
			});

			groupName = Res.GetData("706a9cd7-0d60-45d6-ad89-b0c6be9e57f3", "Permit 5");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("ae52a602-dc5f-46b9-b6e8-c7e0d037336a", "Permit No 5"),
				ColumnName = JobComInvoiceLine.Schema.PermitCusSupportingNo5,
				ToolTip = Res.GetString("c3b287ae-cfd3-490a-a8e1-7ca959c288bc", "Permit No 5"),
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("61f3269e-eeb4-4841-ab90-4220e23d8f13", "Permit Line No 5"),
				ColumnName = JobComInvoiceLine.Schema.PermitCusSupportingLineNo5,
				ToolTip = Res.GetString("aa3670f7-ef72-434e-935f-62a930b120e9", "Permit Line No 5"),
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true
			});

			groupName = Res.GetData("F79CB54C-287E-480F-8780-4340FBBD035B", "Documentary Quantity/Unit");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.TWL_DocumentaryQty,
				GroupName = groupName,
				ShowGroupSeparators = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.TWL_DocumentaryUQ,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.TWL_DocumentaryUnitPrice,
				ShowGroupSeparators = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			#endregion

			#region Assigned Numbers

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = JobComInvoiceLine.Schema.AssignedNumber1,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = JobComInvoiceLine.Schema.AssignedNumber2,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			#endregion

			#region Permit Exemption Codes

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = JobComInvoiceLine.Schema.PermitExemptionCode1,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = JobComInvoiceLine.Schema.PermitExemptionCode2,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = JobComInvoiceLine.Schema.PermitExemptionCode3,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = JobComInvoiceLine.Schema.PermitExemptionCode4,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = JobComInvoiceLine.Schema.PermitExemptionCode5,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			#endregion

			#region Reserved Fields

			groupName = Res.GetData("BC3FF777-D4FF-4E10-84F7-76F03B9A4E11", "Reserved Field 1");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.ReservedFieldCode1,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.ReservedFieldValue1,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
			});

			groupName = Res.GetData("233FCBAE-8765-493B-AD35-C17B1251E998", "Reserved Field 2");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.ReservedFieldCode2,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.ReservedFieldValue2,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
			});

			#endregion

			#region TextileWidth

			groupName = Res.GetData("F058DA08-E837-4B34-8018-BCA57E563647", "Textile Width");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_TextileWidth,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = false,
				Decimals = 6
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_TextileWidthUQ,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40)
			});

			#endregion

			#region Licensing - Common

			groupName = Res.GetData("601BAEE3-6935-464F-80A5-088213338653", "Licensing Quantity");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("4D6C716A-4A1F-4295-9D39-06556191CE16", "Licensing Quantity"),
				ColumnName = JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = false,
				Decimals = 4
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("66860391-40E1-45C0-A340-F774360AC7C5", "Licensing UQ"),
				ColumnName = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("C564838C-FEA6-4E59-BBF2-476BBEC210D0", "Goods Type"),
				ColumnName = JobComInvoiceLine.Schema.JI_GoodsType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("01CB1722-BCE3-4777-9ED2-25F7FDDEB104", "Pre. Permit No", "Pre. Permit Number", "Previous Permit Number"),
				ColumnName = JobComInvoiceLine.Schema.PreviousPermitNo,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("A92F683A-033E-4C59-B641-7580701CCEA4", "Thickness"),
				ColumnName = JobComInvoiceLine.Schema.JI_ProductThickness,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("E79ED4D3-57B6-4568-AEC5-882071A258E0", "Grade"),
				ColumnName = JobComInvoiceLine.Schema.JI_ProductGrade,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("B23678CA-DA70-466A-A8EF-DFB98783CBFD", "Tariff Extension Code"),
				ColumnName = JobComInvoiceLine.Schema.JI_TariffExtensionCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("5867B922-F3AD-4B22-9430-2ACFB51D1331", "Packaging Type"),
				ColumnName = JobComInvoiceLine.Schema.JI_InnerPackType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("95785C4B-1BBF-43F9-B197-C1816336AA31", "Packaging Material"),
				ColumnName = JobComInvoiceLine.Schema.JI_InnerPackingMaterial,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo
			{
				CaptionResourceString = Res.GetData("30FF84E0-767F-4B44-BA17-2DF07CE8CFE3", "Packaging Description"),
				ColumnName = JobComInvoiceLine.Schema.JI_InnerPackDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			#endregion

			#region Licensing - Animal and Plant

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo
			{
				CaptionResourceString = Res.GetData("6F9177FC-24B3-421C-8C68-73BF684D35EB", "Quarantine Treatment"),
				ColumnName = JobComInvoiceLine.Schema.JI_QuarantineTreatment,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo
			{
				CaptionResourceString = Res.GetData("FBE987CD-54AE-43BD-8180-6A5A2507243A", "Color/Characteristics/Botanical Nomenclature"),
				ColumnName = JobComInvoiceLine.Schema.JI_QuarantineFeatures,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo
			{
				CaptionResourceString = Res.GetData("870FD5A4-9E86-47B3-8469-AFC1F6805FD0", "Vaccination Type/Date"),
				ColumnName = JobComInvoiceLine.Schema.JI_VaccinationTypeDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_MicrochipID,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("BBB14ADA-5B0B-4AB6-BD0E-D3B70B511DF0", "Age (Year)"),
				ColumnName = JobComInvoiceLine.Schema.JI_AnimalAgeYear,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true,
				Decimals = 0
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("BBD98BE9-626E-43A3-AC60-DCE23C7B30A3", "Age (Month)"),
				ColumnName = JobComInvoiceLine.Schema.JI_AnimalAgeMonth,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true,
				Decimals = 0
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("094F8B1A-4197-4521-9913-83DFCB6BC393", "Male Quantity"),
				ColumnName = JobComInvoiceLine.Schema.JI_AnimalMaleQty,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true,
				Decimals = 0
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("91A356AA-371B-4AE0-A36F-3F00DFB3E6D0", "Female Quantity"),
				ColumnName = JobComInvoiceLine.Schema.JI_AnimalFemaleQty,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true,
				Decimals = 0
			});

			#endregion

			#region Licensing - Food and Drug

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("82D2BE4E-37A1-4108-981F-8F4FB794B0A4", "Barcode"),
				ColumnName = JobComInvoiceLine.Schema.JI_BarCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			#endregion

			#region Licensing - Alcohol

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo
			{
				CaptionResourceString = Res.GetData("F8CCD0BC-9F52-43F6-881E-76F31BC712BA", "Geographical Indication"),
				ColumnName = JobComInvoiceLine.Schema.JI_AlcoholCountryRegion,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("AEEBE7DA-E8C4-40ED-8694-E4BC8BEC9BDE", "Without Original Lot Number"),
				ColumnName = JobComInvoiceLine.Schema.JI_NoOriginalLotNoAmt,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = false,
				Decimals = 4
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("A7B5ADF9-D5D3-4E0F-97AB-BC94F00B168F", "With Lot Number Removed"),
				ColumnName = JobComInvoiceLine.Schema.JI_RemovedLotNoAmt,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = false,
				Decimals = 4
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("6CD02B77-5672-403C-959F-D39FE63B276C", "With Lot Number Altered"),
				ColumnName = JobComInvoiceLine.Schema.JI_AlteredLotNoAmt,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = false,
				Decimals = 4
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("6804FFA2-B275-41C9-8FE8-D065A5FBB095", "Bottled Date"),
				ColumnName = JobComInvoiceLine.Schema.JI_BottledDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("F897FE56-747B-450F-A0C2-E90D85F8E90F", "Expiration Date"),
				ColumnName = JobComInvoiceLine.Schema.JI_ExpirationDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("A06ABF03-7A02-412D-BFCA-CCAD8A743AA9", "End of Shelf Life"),
				ColumnName = JobComInvoiceLine.Schema.JI_AlcoholEndOfShelfLife,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("E3573D02-C8F3-458E-8CA6-DC6F62FDF556", "Age (Alcohol)"),
				ColumnName = JobComInvoiceLine.Schema.JI_AlcoholAge,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true,
				Decimals = 0
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("D9D7A418-8955-4DD6-AFA2-C44CAD9BB076", "Year (Alcohol)"),
				ColumnName = JobComInvoiceLine.Schema.JI_AlcoholYear,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true,
				Decimals = 0
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("49308025-E516-4A6B-8736-C38C0EACC8C3", "ABV (%)"),
				ColumnName = JobComInvoiceLine.Schema.JI_AlcoholPercentage,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true,
				Decimals = 3
			});

			#endregion

			ResetCustomsQuantityGroupName();
		}

		void ResetCustomsQuantityGroupName()
		{
			var groupName = Res.GetData("7C21913B-D7B9-4F42-9824-9C166E231A7D", "Statistical Weight");
			var customsQuantityColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity);
			customsQuantityColumnStyle.GroupName = groupName;
			var customsUnitQtyColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty);
			customsUnitQtyColumnStyle.GroupName = groupName;
		}

		void AddLineDetailsUserControl()
		{
			LineDetailsUserControl = GetLineDetailsUserControl();

			LineDetailsTabPage.SuspendLayout();
			LineDetailsUserControl.SuspendLayout();
			SuspendLayout();

			TWLineDetailsTabPage.Controls.Add(LineDetailsUserControl);
			LineDetailsUserControl.AllowDrop = true;
			BindingSource.SetBindingMember(LineDetailsUserControl, ".");
			LineDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			LineDetailsUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			LineDetailsUserControl.Name = "LineDetailsUserControl";
			LineDetailsUserControl.TabIndex = 0;

			LineDetailTabControl.ResumeLayout(false);
			LineDetailTabControl.PerformLayout();
			LineDetailsUserControl.ResumeLayout(true);
			LineDetailsUserControl.PerformLayout();
			ResumeLayout(false);
			PerformLayout();

			if (!DesignModeFinder.IsDesigning)
			{
				LineDetailsUserControl.JI_TariffFindBox.GetCountryCode = GetCustomsCountryCode;
				LineDetailsUserControl.JI_TariffFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
				LineDetailsUserControl.JI_TariffFindBox.GetTariffType = GetUniversalTariffType;
				LineDetailsUserControl.JI_TariffFindBox.GetEffectiveDate = GetEffectiveAssessmentDateForUniversalTariff;
			}
		}

		protected virtual LineDetailsUserControl GetLineDetailsUserControl()
		{
			return new LineDetailsUserControl();
		}

		string[] DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					defaultColumnsForGrid = GetDefaultColumnsForGridCore().ToArray();
				}

				return defaultColumnsForGrid;
			}
		}

		string[] defaultColumnsForGrid;

		protected virtual List<string> GetDefaultColumnsForGridCore()
		{
			var columns = new List<string>();
			columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice);
			columns.Add(JobComInvoiceLine.Schema.InvoiceHeaderDisplaySequence);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_LineNo);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_PartNo);
			columns.Add(JobComInvoiceLine.Schema.JI_FormattedTariff);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_Procedure);
			columns.Add(JobComInvoiceLine.Schema.JI_Group);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_Description);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ);
			columns.Add(JobComInvoiceLine.Schema.JI_EnteredUnitPrice);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_LinePrice);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeight);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeightUQ);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsSecondQuantity);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsSecondUnitQty);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_BrandName);
			columns.Add(JobComInvoiceLine.Schema.TWL_DocumentaryQty);
			columns.Add(JobComInvoiceLine.Schema.TWL_DocumentaryUQ);
			columns.Add(JobComInvoiceLine.Schema.TWL_DocumentaryUnitPrice);

			return columns;
		}

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					List<string> columns = new List<string>();
					columns.AddRange(DefaultColumnsForGrid);

					columns.Add(JobComInvoiceLineSchema.Constants.JI_RH_NKCommodity_Code);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Model);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Weight);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_WeightUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Volume);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_VolumeUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CC);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_OrderNumber);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib2);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib3);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_SerialNumber);
					columns.Add(JobComInvoiceLine.Schema.JI_NewOwnerPartNo);
					columns.Add(JobComInvoiceLine.Schema.JI_NewPartAttribute1);
					columns.Add(JobComInvoiceLine.Schema.JI_NewPartAttribute2);
					columns.Add(JobComInvoiceLine.Schema.JI_NewPartAttribute3);
					columns.Add(JobComInvoiceLine.Schema.JI_NewSerialNumber);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib2);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib3);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib4);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib5);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib6);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomTextBlob1);
					columns.Add(JobComInvoiceLine.Schema.JI_CustomsSupplierPartNo);
					columns.Add(JobComInvoiceLine.Schema.JI_CustomsOwnerPartNo);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_NDescription);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty);

					columnNamesInSortOrder = columns.ToArray();
				}

				return columnNamesInSortOrder;
			}
		}

		string[] columnNamesInSortOrder;

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(ColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, DefaultColumnsForGrid);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(ColumnNamesInSortOrder);
				base.OnLoad(e);
			}
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			var lineDetailsTabPage = LineDetailTabControl.GetTabPage("LineDetailsTabPage");
			if (lineDetailsTabPage != null)
			{
				LineDetailTabControl.TabPages.Remove(TWLineDetailsTabPage);
				LineDetailTabControl.TabPages.Remove(lineDetailsTabPage);
				LineDetailTabControl.TabPages.Insert(TWLineDetailsTabPage, 0);
				LineDetailTabControl.TabPages.Remove(TWOtherDetailsTabPage);
				LineDetailTabControl.TabPages.Insert(TWOtherDetailsTabPage, 1);
				LineDetailTabControl.TabPages.Remove(DutiesTaxesAndFeesTabPage);
				LineDetailTabControl.TabPages.Insert(DutiesTaxesAndFeesTabPage, 2);
				LineDetailTabControl.TabPages.Remove(SupportingDocumentsTabPage);
				LineDetailTabControl.TabPages.Insert(SupportingDocumentsTabPage, 3);
				LineDetailTabControl.TabPages.Remove(CarInfoTabPage);
				LineDetailTabControl.TabPages.Insert(CarInfoTabPage, 4);
				LineDetailTabControl.TabPages.Remove(AircraftPartsTabPage);
				LineDetailTabControl.TabPages.Insert(AircraftPartsTabPage, 5);
				LineDetailTabControl.SelectedTab = (ZTabPage)LineDetailTabControl.TabPages[0];
			}
			var isImport = JobDeclaration.IsImport;
			DutiesTaxesAndFeesTabPage.TabVisible = isImport;
			AircraftPartsTabPage.TabVisible = isImport;
		}

		#region Extract From GeneralCountryInvoiceLineUserControl

		protected override void HookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.HookInvoiceLineEvents(invoiceLine);

			if (invoiceLine is JobComInvoiceLine twInvoiceLine)
			{
				twInvoiceLine.JI_TariffInfo.ValueChanged -= JI_TariffInfo_ValueChanged;
				twInvoiceLine.JI_TariffInfo.ValueChanged += JI_TariffInfo_ValueChanged;
				JI_TariffInfo_ValueChanged(this, EventArgs.Empty);
				SetControllingMsgTabPagesVisibilityForCAHeader(null, null);
				twInvoiceLine.HasLinkedCMHeaderInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.HasLinkedCMHeaderInfo.ValueChanged += SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.CertificateOfOriginSupportedInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.CertificateOfOriginSupportedInfo.ValueChanged += SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.AlcoholSupportedInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.AlcoholSupportedInfo.ValueChanged += SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.TypeApprovalSupportedInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.TypeApprovalSupportedInfo.ValueChanged += SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.AnimalAndPlantSupportedInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.AnimalAndPlantSupportedInfo.ValueChanged += SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.FoodAndDrugSupportedInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.FoodAndDrugSupportedInfo.ValueChanged += SetControllingMsgTabPagesVisibilityForCAHeader;
			}
		}

		protected override void UnHookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.UnHookInvoiceLineEvents(invoiceLine);

			if (invoiceLine is JobComInvoiceLine twInvoiceLine)
			{
				twInvoiceLine.JI_TariffInfo.ValueChanged -= JI_TariffInfo_ValueChanged;
				twInvoiceLine.HasLinkedCMHeaderInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.CertificateOfOriginSupportedInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.AlcoholSupportedInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.TypeApprovalSupportedInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.AnimalAndPlantSupportedInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
				twInvoiceLine.FoodAndDrugSupportedInfo.ValueChanged -= SetControllingMsgTabPagesVisibilityForCAHeader;
			}
		}

		protected new JobComInvoiceLine CurrentInvoiceLine => base.CurrentInvoiceLine as JobComInvoiceLine;

		void SetControllingMsgTabPagesVisibilityForCAHeader(object sender, EventArgs e)
		{
			var invoiceLine = CurrentInvoiceLine;
			LicensingUserControl.SetControllingMsgTabPagesVisibilityForCAHeader(invoiceLine);
		}
		#endregion

		void JI_TariffInfo_ValueChanged(object sender, EventArgs e)
		{
			var invoiceLine = CurrentInvoiceLine;
			CarInfoTabPage.TabVisible = invoiceLine?.IsCarRelatedTariff ?? false;
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Taiwan;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.Taiwan;

		protected override bool SupportNewOwnerPartDetails => true;
		protected override string NewOwnerPartNoColumnName => JobComInvoiceLine.Schema.JI_NewOwnerPartNo;
		protected override string NewOwnerPartAttrib1ColumnName => JobComInvoiceLine.Schema.JI_NewPartAttribute1;
		protected override string NewOwnerPartAttrib2ColumnName => JobComInvoiceLine.Schema.JI_NewPartAttribute2;
		protected override string NewOwnerPartAttrib3ColumnName => JobComInvoiceLine.Schema.JI_NewPartAttribute3;
		protected override string NewOwnerSerialNumberColumnName => JobComInvoiceLine.Schema.JI_NewSerialNumber;

		protected override void ChangeContainerTabVisibility()
		{
			ContainersTabPage.TabVisible = JobDeclaration.IsContainerInvoiceLinkRelevant;
		}

		public new Business.IInvoicesProvider CurrentDataItem
		{
			get { return (Business.IInvoicesProvider)base.CurrentDataItem; }
		}

		protected override Customs.GUI.InvoiceLineFilterBusinessObject CreateFilterBusinessObject(Func<Customs.Business.IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable)
		{
			return DesignModeFinder.IsDesigning ? null : new InvoiceLineFilterBusinessObject(getInvoicesProvider, isColumnAvailable);
		}

		protected override ZString TariffColumnNameCore => JobComInvoiceLine.Schema.JI_FormattedTariff;
	}
}
