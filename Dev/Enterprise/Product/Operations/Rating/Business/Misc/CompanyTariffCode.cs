using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public sealed class CompanyTariffCode : CodeDescriptionPair
	{
		public CompanyTariffCode(string code, MultilingualString discountDescription, string categoryCode, MultilingualString categoryDescription, bool noServiceDirection = false)
			: this(code, discountDescription, new CodeDescriptionPair(categoryCode, categoryDescription))
		{
			this.NoServiceDirection = noServiceDirection;
		}

		public CompanyTariffCode(string code, MultilingualString discountDescription, CodeDescriptionPair categoryDescription, bool noServiceDirection = false)
			: base(code, discountDescription)
		{
			this.categoryDescription = categoryDescription;
			this.NoServiceDirection = noServiceDirection;
		}

		public readonly bool NoServiceDirection;

		public CodeDescriptionPair CategoryDescription
		{
			get { return categoryDescription; }
		}
		readonly CodeDescriptionPair categoryDescription;
	}

	public sealed class CompanyTariffCodes : CodeDescriptionPairList
	{
		public CompanyTariffCodes()
		{
			Add(new CompanyTariffCode(RatingConstants.RateCategory.AIR, ResString.GetMultilingualString("31aeff42-a7c8-4fad-b119-6c9fabf5a402", "Air Freight"), "FRT", ResString.GetMultilingualString("6dfa93eb-5c23-462f-8a6f-0e6fb25e846f", "Freight")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.FCL, ResString.GetMultilingualString("ab48cc97-1c65-4429-bc2a-97d709ad25db", "FCL Freight"), "FRT", ResString.GetMultilingualString("6dfa93eb-5c23-462f-8a6f-0e6fb25e846f", "Freight")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.LCL, ResString.GetMultilingualString("66fd6ae7-1d25-4550-97fd-27aa4c9d30f1", "LCL/FTL/LTL Freight"), "FRT", ResString.GetMultilingualString("6dfa93eb-5c23-462f-8a6f-0e6fb25e846f", "Freight")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.ORG, ResString.GetMultilingualString("10b26300-167f-4a2a-9422-fec757e6337a", "Origin Charges"), "ORG", ResString.GetMultilingualString("3124c329-01cd-4104-a55b-e30191c08463", "Origin")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.DST, ResString.GetMultilingualString("0c9d7288-f12d-403b-a96c-5ef1c186eb4f", "Destination Charges"), "DST", ResString.GetMultilingualString("6811ede4-61af-498b-bd20-46b9ded96c6e", "Destination")));

			// Customs
			Add(new CompanyTariffCode(RatingConstants.RateCategory.CAI, ResString.GetMultilingualString("EEF76463-C374-4CBA-ACDC-E84415B6DC16", "Customs Air Freight"), "FRT", ResString.GetMultilingualString("0AD84CDA-C239-466D-82D4-2ED2E8E4174D", "Customs Freight")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.CFC, ResString.GetMultilingualString("6258CC9C-DD4F-45A6-B264-CF62A58B54C8", "Customs FCL Freight"), "FRT", ResString.GetMultilingualString("0AD84CDA-C239-466D-82D4-2ED2E8E4174D", "Customs Freight")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.CLC, ResString.GetMultilingualString("5A35B604-5B9C-4C6E-BE20-7008295B2D62", "Customs LCL/FTL/LTL Freight"), "FRT", ResString.GetMultilingualString("0AD84CDA-C239-466D-82D4-2ED2E8E4174D", "Customs Freight")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.COR, ResString.GetMultilingualString("8D2D832F-8B33-4761-B26C-4C745CE62D15", "Customs Origin Charges"), "ORG", ResString.GetMultilingualString("C783FFC3-C54E-49B1-97E1-8CA67E6F0006", "Customs Origin")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.CDS, ResString.GetMultilingualString("F1AC344C-6863-4D23-A39F-7786755C5582", "Customs Destination Charges"), "DST", ResString.GetMultilingualString("55EA6300-579B-4FA3-86A8-FC46BFC81FC6", "Customs Destination")));

			Add(new CompanyTariffCode(RatingConstants.RateCategory.SOR, ResString.GetMultilingualString("6bfc0dec-ae2a-4d26-bb9d-4827c334e96b", "Shipping Origin Charges"), "SOR", ResString.GetMultilingualString("bd3eb88b-6187-4395-a086-f2b025709bd7", "Shipping Origin")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.SDE, ResString.GetMultilingualString("80ea2809-52ce-4756-833a-6b5570f03200", "Shipping Destination Charges"), "SDE", ResString.GetMultilingualString("d57f46b8-267d-4297-aca0-6a5649230647", "Shipping Destination")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.SCO, ResString.GetMultilingualString("67ca759f-fae2-428b-9d98-ceb30c848e8b", "Shipping Containerized Freight Charges"), "SFR", ResString.GetMultilingualString("5cc6f7b4-563d-426d-87b3-b32a9c83d105", "Shipping Freight")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.SNC, ResString.GetMultilingualString("f7121f8e-8518-4b15-8931-4891e036752e", "Shipping Non-Containerized Freight Charges"), "SFR", ResString.GetMultilingualString("5cc6f7b4-563d-426d-87b3-b32a9c83d105", "Shipping Freight")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.SED, ResString.GetMultilingualString("3694108e-59c7-4f5a-98e2-8a210f8d439e", "Shipping Export Container Detention"), "SCD", ResString.GetMultilingualString("75659e80-ad6c-4ed2-98e6-cd3559fac9d2", "Shipping Container Detention")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.SID, ResString.GetMultilingualString("ca70aca1-ffa5-44f3-b390-ecc310577d27", "Shipping Import Container Detention"), "SCD", ResString.GetMultilingualString("75659e80-ad6c-4ed2-98e6-cd3559fac9d2", "Shipping Container Detention")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.PAC, ResString.GetMultilingualString("7593229e-5e7b-4a5c-be49-de17d6fae5a9", "CFS Packing Charges"), "CFS", ResString.GetMultilingualString("4fa7f04c-6861-4506-96b8-8349f423deaa", "CFS")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.UNP, ResString.GetMultilingualString("507b6dbe-0d02-4a3c-a991-eadfbc28ca94", "CFS Unpacking Charges"), "CFS", ResString.GetMultilingualString("4fa7f04c-6861-4506-96b8-8349f423deaa", "CFS")));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.CST, ResString.GetMultilingualString("6e138628-5370-4e32-a456-f5a3d1f6a23e", "Container Storage"), "CFS", ResString.GetMultilingualString("4fa7f04c-6861-4506-96b8-8349f423deaa", "CFS"), true));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.CYM, ResString.GetMultilingualString("9e722396-7689-4e87-979c-3cec4c94cd77", "Maintenance and Repair"), "CYM", ResString.GetMultilingualString("1bc94888-dda2-413d-a467-3e4fd3da23b2", "Yard Maintenance and Repair Charges"), true));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.CYD, ResString.GetMultilingualString("19ff136f-31f6-45d3-ba1f-af590bfc9860", "Container Yard Charges"), "CYD", ResString.GetMultilingualString("5cb16648-77e7-4242-a3c4-0ac7173a589a", "Container Yard"), true));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.CYU, ResString.GetMultilingualString("ac175f0f-04bf-45be-a4a9-7f8801838213", "Container Yard Transportation Unit Charges"), "CYU", ResString.GetMultilingualString("1cd05b84-c7e8-465b-8361-6cc84e807348", "Container Yard Transportation Unit"), true));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.TRN, ResString.GetMultilingualString("a14d193d-28e2-4657-94da-ce4f0c758a8f", "Port Transport Charges"), "TRN", ResString.GetMultilingualString("4f5c2b9a-6421-4216-b39e-084cfe5d0b0d", "Transport"), true));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.TBC, ResString.GetMultilingualString("ade9dc5c-91b4-437e-b14d-0c3c353779b5", "Land Transport Charges"), "TBC", ResString.GetMultilingualString("7c0a76f2-c178-4eff-b871-96902452140b", "Land Transport"), true));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.WHS, ResString.GetMultilingualString("2acc6a1b-fbd4-41dc-9953-d99adefdad84", "Product Warehouse Charges"), "WHS", ResString.GetMultilingualString("77d3a2f0-d97e-4633-82d7-c32571179e23", "Product Warehouse"), true));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.TRW, ResString.GetMultilingualString("787e6a50-c917-44c2-9f8f-afde75889ff5", "Transit Warehouse Charges"), "TRW", ResString.GetMultilingualString("7989ede4-c02c-4149-95a4-a3d7e3cd1e81", "Transit Warehouse"), true));
			Add(new CompanyTariffCode(RatingConstants.RateCategory.TWU, ResString.GetMultilingualString("43f8765a-9ab4-4196-9aaf-0a1eb0459667", "Transit Warehouse Transportation Unit Charges"), "TWU", ResString.GetMultilingualString("3ad6f8fc-0c3b-4612-a6f9-e481af406690", "Transit Warehouse Transportation Unit"), true));
		}

		public new CompanyTariffCode this[string code]
		{
			get { return (CompanyTariffCode)base[code]; }
		}

		public bool HasNoServiceDirection(string rateCategory)
		{
			var item = this[rateCategory];
			if (item != null)
			{
				return item.NoServiceDirection;
			}
			return false;
		}

		public CodeDescriptionPair GetCompanyTariffCodeDescription(string rateCategory)
		{
			var item = this[rateCategory];
			if (item != null)
			{
				return item.CategoryDescription;
			}
			else
			{
				return null;
			}
		}

		public string GetCompanyTariffDiscountDescription(string rateCategory)
		{
			var item = this[rateCategory];
			if (item != null)
			{
				return item.Description;
			}
			else
			{
				return null;
			}
		}

		public IEnumerable<string> GetRateCategories(ZString companyTariffCode)
		{
			foreach (CompanyTariffCode item in this)
			{
				if (item.CategoryDescription.Code == companyTariffCode)
				{
					yield return item.Code;
				}
			}
		}
	}
}
