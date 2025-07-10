using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class FoodData : CusCodeData
	{
		public FoodData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string Content = "Content";
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

		[MaxLength(500)]
		[ResourceStringData("Enterprise.Customs.TW.Business.FoodData|CY_Data", Caption = "Ingredient", FullDescription = "The name of the ingredient or food additive of the product.")]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.FoodData|CY_Code", Caption = "Content")]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		[DecimalPlaces(4)]
		[DecimalPrecision(14)]
		[ResourceStringData("Enterprise.Customs.TW.Business.FoodData|Content", Caption = "Content", FullDescription = "The content of the ingredient or food additive of the product.")]
		public ZDecimal Content
		{
			get { return ZDecimal.ParseSafe(CY_Code, ZDecimal.Zero); }
			set
			{
				CY_Code = value.ToString();
				ContentInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateContent();
				}
			}
		}

		public ZPropertyInfo ContentInfo => GetZPropertyInfo(Schema.Content);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeList.Codes.Food;
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new FoodDataValidation(this);
		}

		public new FoodDataValidation Validation => (FoodDataValidation)base.Validation;

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;
	}
}
