using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AviationFuelType : Customs.Business.CusSupportingInfo
	{
		public AviationFuelType(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(11)]
		[ResourceStringData("AviationFuelType|CSI_ReferenceNumber2", ShortCaption = "Tax ID", Caption = "Tax ID")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		[ResourceStringData("AviationFuelType|CSI_DateOfIssue", ShortCaption = "Inv. Date", Caption = "Invoice Date")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		[MaxLength(20)]
		[ResourceStringData("AviationFuelType|CSI_ReferenceNumber", ShortCaption = "Inv. No", Caption = "Invoice No")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[DecimalPlaces(2)]
		[ResourceStringData("AviationFuelType|CSI_Value", ShortCaption = "Tot.Amount", Caption = "Total Amount")]
		public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

		[ReadOnlyMember(nameof(CSI_RX_NKCurrency_ReadOnly))]
		[ResourceStringData("AviationFuelType|CSI_RX_NKCurrency", ShortCaption = "Currency", Caption = "Currency")]
		public override ZString CSI_RX_NKCurrency { get => base.CSI_RX_NKCurrency; set => base.CSI_RX_NKCurrency = value; }

		[ResourceStringData("AviationFuelType|CSI_Description", ShortCaption = "Description", Caption = "Description")]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		bool CSI_RX_NKCurrency_ReadOnly => true;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.AviationFuelType;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			CSI_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;
		}

		public new AviationFuelTypeValidation Validation => (AviationFuelTypeValidation)base.Validation;

		protected override Customs.Business.CusSupportingInfoValidation GetNewValidation()
		{
			return new AviationFuelTypeValidation(this);
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;
	}
}
