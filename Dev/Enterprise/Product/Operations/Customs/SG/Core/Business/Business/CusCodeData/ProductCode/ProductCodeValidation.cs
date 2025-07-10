using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class ProductCodeValidation : CusCodeDataValidation
	{
		public ProductCodeValidation(ProductCode parent)
			: base(parent)
		{
		}

		new ProductCode Parent
		{
			get { return (ProductCode)base.Parent; }
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (Parent.Lookups.ProductCodes != null)
			{
				ListValidation.WarnIfInvalidCode(Parent.CY_DataInfo, Parent.Lookups.ProductCodes);
			}

			if (Parent.Parent != null)
			{
				foreach (ProductCode productCode in Parent.Parent.ProductCodes)
				{
					if (productCode != Parent && productCode.CY_Data == Parent.CY_Data)
					{
						Parent.CY_DataInfo.AddError("The Product Code already exists.");
						break;
					}
				}
			}
		}

		protected override void CheckCY_Code()
		{
			//base.CheckCY_Code not used for the product code
		}
	}
}
