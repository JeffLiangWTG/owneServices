using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class ProductCodeLookups : CusCodeDataLookups
	{
		public ProductCodeLookups(ProductCode parent)
			: base(parent)
		{
		}

		protected new ProductCode Parent
		{
			get { return (ProductCode)base.Parent; }
		}

		public CodeDescriptionPairList ProductCodes
		{
			get
			{
				if (productCodes == null)
				{
					productCodes = new CodeDescriptionPairList();
					var tariff = UniversalReferenceDataHelper.LoadBestMatch(Factory, Parent.Parent.TariffNum, ZDateTime.Today);
					var commodities = tariff?.GetTariffCommodities(ZDateTime.Today);
					if (commodities != null)
					{
						productCodes.AddRange(commodities);
					}
				}
				return productCodes;
			}
		}
		CodeDescriptionPairList productCodes;
	}
}
