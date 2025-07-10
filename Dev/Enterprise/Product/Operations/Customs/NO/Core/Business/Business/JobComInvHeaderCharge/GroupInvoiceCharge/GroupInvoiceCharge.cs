using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.NO.Business
{
	public class GroupInvoiceCharge : Customs.Business.BaseGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new GroupInvoiceCharge Clone() => (GroupInvoiceCharge)base.Clone();

		JobDeclaration JobDeclaration => GroupInvoice?.JobDeclaration;

		bool IsImport => JobDeclaration?.IsImport ?? ZBool.False;
		bool IsExport => JobDeclaration?.IsExport ?? ZBool.False;

		public new JobComInvoiceGroupHeader GroupInvoice => (JobComInvoiceGroupHeader)base.GroupInvoice;

		public new GroupInvoiceChargeLookups Lookups => (GroupInvoiceChargeLookups)base.Lookups;

		public new GroupInvoiceChargeValidation Validation => (GroupInvoiceChargeValidation)base.Validation;

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => this switch
		{
			{ IsImport: true } => new ImportGroupInvoiceChargeLookups(this),
			{ IsExport: true } => new ExportGroupInvoiceChargeLookups(this),
			_ => new GroupInvoiceChargeLookups(this),
		};

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => IsImport
			? new ImportGroupInvoiceChargeValidation(this)
			: new ExportGroupInvoiceChargeValidation(this);

		protected override bool GetJ7_IsDutiable_ReadOnly() => J7_ChargeType.ToString() switch
		{
			Common.CustomsChargeTypeList.Codes.OverseasFreight or
			Common.CustomsChargeTypeList.Codes.OverseasInsurance => true,
			_ => base.GetJ7_IsDutiable_ReadOnly()
		};

		[DecimalPlaces(3)]
		[ResourceStringData("NO.GroupInvoiceCharge.J7_Percentage", Caption = "% of Invoice Amount", ShortCaption = "% of Amount")]
		public override ZDecimal J7_Percentage
		{
			get => base.J7_Percentage;
			set => base.J7_Percentage = value;
		}
	}
}
