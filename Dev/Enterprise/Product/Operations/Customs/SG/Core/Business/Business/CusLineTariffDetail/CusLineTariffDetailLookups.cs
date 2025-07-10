using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusLineTariffDetailLookups : Customs.Business.CusLineTariffDetailLookups
	{
		public CusLineTariffDetailLookups(CusLineTariffDetail parent)
			: base(parent)
		{
		}

		#region Product Code UQ List

		public ProductCodeUQList ProductCodeUQList
		{
			get { return productCodeUQList ?? (productCodeUQList = new ProductCodeUQList()); }
		}
		ProductCodeUQList productCodeUQList;

		#endregion

		public CodeDescriptionPairList TariffCommodities
		{
			get
			{
				if (tariffCommodities == null)
				{
					var tariff = Parent.InvoiceLine?.UniversalTariff;
					if (tariff != null)
					{
						tariffCommodities = new CodeDescriptionPairList();
						tariffCommodities.AddRange(tariff.GetTariffCommodities(Parent.EffectiveAssessmentDate));
					}
				}
				return tariffCommodities;
			}
		}
		CodeDescriptionPairList tariffCommodities;
	}
}
