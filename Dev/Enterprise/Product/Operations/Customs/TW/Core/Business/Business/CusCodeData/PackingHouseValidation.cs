using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class PackingHouseValidation : CusCodeDataValidation
	{
		public PackingHouseValidation(AutoCusCodeData parent) : base(parent)
		{
		}

		protected new PackingHouse Parent => (PackingHouse)base.Parent;

		protected override void CheckCY_CodeList()
		{
			base.CheckCY_CodeList();
			var packingHouse = Parent;
			var invoice = packingHouse.Parent;
			var info = packingHouse.CY_CodeInfo;
			var packingHouses = invoice.PackingHouseCollection;
			if (packingHouses.Count > 1 && packingHouses.Where(x => x.CY_Code == packingHouse.CY_Code).Count() > 1)
			{
				info.AddMessageError(Res.GetString("D449A5A8-A94C-4E86-A61A-C1005F54F718", "This packing house already exists."));
			}

			var cusCode = packingHouse.CusCode;
			if (cusCode != null)
			{
				var country = cusCode.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Country);
				var goodsOrigin = invoice.JI_CountryOfOrigin;
				if (country != goodsOrigin)
				{
					info.AddMessageError(Res.GetString("275B2528-0F63-4D06-A21A-9DAAAC62AEE2", "Packing House Country {0} and County of Origin {1} should match.", country, goodsOrigin));
				}

				var tariff = cusCode.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Tariff);
				var invoiceTariff = invoice.JI_Tariff;
				if (tariff != invoiceTariff)
				{
					info.AddMessageError(Res.GetString("DBD9C076-2881-4174-A7F2-9E9D06B320D5", "Packing House Tariff {0} and Tariff {1} should match.", tariff, invoiceTariff));
				}
			}
		}
	}
}
