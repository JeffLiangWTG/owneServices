using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class GovernmentUniformInvoiceData : CusCodeData
	{
		public GovernmentUniformInvoiceData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string Amount = "Amount";

			public new const int CY_CodeMaxLength = 12;
			public new const int CY_DataMaxLength = 12;
		}

		public new JobDeclaration Parent { get => (JobDeclaration)base.Parent; set => base.Parent = value; }

		[MaxLength(Schema.CY_CodeMaxLength)]
		[ResourceStringData("GovernmentUniformInvoiceData|CY_Code", Caption = "Government Uniform Invoice Number", ShortCaption = "Number", FullDescription = "The Uniform Invoice numbers of the bonded goods included in monthly reporting.")]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		[ResourceStringData("GovernmentUniformInvoiceData|CY_Data", Caption = "Amount")]
		[MaxLength(Schema.CY_DataMaxLength)]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		[ResourceStringData("GovernmentUniformInvoiceData|Amount", Caption = "Amount", FullDescription = "The Uniform Invoices amount of the bonded goods included in monthly reporting.")]
		public ZDecimal Amount { get => ZDecimal.ParseSafe(CY_Data, ZDecimal.Zero); set => CY_Data = value.ToString(); }

		public ZPropertyInfo AmountInfo => GetWrappedZPropertyInfo(Schema.Amount, x => CY_DataInfo);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeList.Codes.GOVUniformInvoice;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new GovernmentUniformInvoiceDataValidation(this);
		}

		public new GovernmentUniformInvoiceDataValidation Validation => (GovernmentUniformInvoiceDataValidation)base.Validation;
	}
}
