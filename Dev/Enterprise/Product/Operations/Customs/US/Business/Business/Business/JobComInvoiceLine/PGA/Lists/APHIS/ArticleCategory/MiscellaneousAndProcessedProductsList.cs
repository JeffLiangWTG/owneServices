using CargoWise.Common.Collections;

namespace Enterprise.Customs.US.Business.APHIS.ArticleCategory
{
	public class MiscellaneousAndProcessedProductsList : MiscellaneousAndProcessedProductsListBase
	{
		public MiscellaneousAndProcessedProductsList()
		{
			if (ZZCustomsFunctionality.IsAPHIS2024Effective)
			{
				RemoveCode(Codes.BeesBeeEquipmentAndBeeProducts);
				RemoveCodeByDescription("Soil, rocks, and garbage");
			}
			else
			{
				RemoveCode(Codes.Bees);
				RemoveCode(Codes.BeeEquipment);
				RemoveCode(Codes.BeeProducts);
				RemoveCodeByDescription("Soil and rocks");
				RemoveCode(Codes.Garbage);
			}
		}

		void RemoveCodeByDescription(string description)
		{
			Elements.RemoveAll(x => x.Description == description);
		}
	}
}
