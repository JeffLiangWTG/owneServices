using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class ProductCode : CusCodeData
	{
		public ProductCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "+" + nameof(ProductCodeLookups.ProductCodes))]
		[ResourceStringData("1ED4C43F-7AA4-4FD1-813B-AE45CC258D2C", Caption = "Product Code")]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		public new ITariffData Parent
		{
			get { return (ITariffData)base.Parent; }
			set { base.Parent = (BusinessObject)value; }
		}

		public new ProductCodeLookups Lookups
		{
			get { return (ProductCodeLookups)base.Lookups; }
		}

		public new ProductCodeValidation Validation
		{
			get { return (ProductCodeValidation)base.Validation; }
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new ProductCodeLookups(this);
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new ProductCodeValidation(this);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(Classification), typeof(CusClassPartPivot)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ProductCode;
		}
	}
}
