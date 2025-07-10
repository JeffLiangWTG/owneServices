using System.Diagnostics.CodeAnalysis;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public static class RateCategoriesList
	{
		public static CodeDescriptionPairList GetRateCategoriesList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(RatingConstants.RateCategory.ALL, ResString.GetMultilingualString("3ee4d3aa-35b1-4c78-900b-903b225c0b19", "All Charges"));
			result.AddPair(RatingConstants.RateCategory.AIR, ResString.GetMultilingualString("19d74acb-21b0-46e8-abac-f44db2d289dd", "Air Freight"));
			result.AddPair(RatingConstants.RateCategory.FCL, ResString.GetMultilingualString("89e2bb54-c0ce-462c-87de-45bc18d170ef", "FCL Freight"));
			result.AddPair(RatingConstants.RateCategory.LCL, ResString.GetMultilingualString("4d745c71-9ea3-4e4c-9c14-639b44dbc897", "LCL/FTL/LTL/COU Freight"));
			result.AddPair(RatingConstants.RateCategory.ORG, ResString.GetMultilingualString("77f9e313-1539-4af6-a1f6-e82c6da32d61", "Origin Charges"));
			result.AddPair(RatingConstants.RateCategory.DST, ResString.GetMultilingualString("a3a300da-7306-4350-8397-42a6417d9930", "Destination Charges"));

			result.AddPair(RatingConstants.RateCategory.CAI, ResString.GetMultilingualString("9986A4E1-77A0-4088-A3C0-2C341CBB2883", "Air Freight"));
			result.AddPair(RatingConstants.RateCategory.CFC, ResString.GetMultilingualString("E29A0FD9-2A2B-4EAF-9905-82EC88C8C682", "FCL Freight"));
			result.AddPair(RatingConstants.RateCategory.CLC, ResString.GetMultilingualString("C831EE84-2F4A-4011-B8FC-D09A359CF091", "LCL/FTL/LTL Freight"));
			result.AddPair(RatingConstants.RateCategory.COR, ResString.GetMultilingualString("4C2BC83B-D9E9-4810-851C-B8F81A17F542", "Origin Charges"));
			result.AddPair(RatingConstants.RateCategory.CDS, ResString.GetMultilingualString("1FA61347-D488-4E48-8CBE-DABF619712B3", "Destination Charges"));

			result.AddPair(RatingConstants.RateCategory.SOR, ResString.GetMultilingualString("3e796e64-20af-4e54-a571-8e4dfa21a94c", "Origin Charges"));
			result.AddPair(RatingConstants.RateCategory.SDE, ResString.GetMultilingualString("cd2f73a9-bbae-496a-85b4-c62d13e7ce1d", "Destination Charges"));
			result.AddPair(RatingConstants.RateCategory.SCO, ResString.GetMultilingualString("d03d8ed5-987c-48d3-b506-afb4406975c5", "Containerized Freight"));
			result.AddPair(RatingConstants.RateCategory.SNC, ResString.GetMultilingualString("2bfbae05-ed63-4a6a-800a-0d67f977420a", "Non-Containerized Freight"));
			result.AddPair(RatingConstants.RateCategory.SED, ResString.GetMultilingualString("42f8196c-cafc-4b26-9137-656a3f03b4e0", "Export Container Detention"));
			result.AddPair(RatingConstants.RateCategory.SID, ResString.GetMultilingualString("7879c57b-5603-4765-a741-5636544e89e6", "Import Container Detention"));

			result.AddPair(RatingConstants.RateCategory.PAC, ResString.GetMultilingualString("2b47e53f-5243-48bb-872c-54a57009a1ba", "Packing Charges"));
			result.AddPair(RatingConstants.RateCategory.UNP, ResString.GetMultilingualString("5eb5cc4c-013f-443e-bbbf-a9a529004b4f", "Unpacking Charges"));
			result.AddPair(RatingConstants.RateCategory.CST, ResString.GetMultilingualString("84b27a55-44a4-4ecd-aca8-19d628e7c23c", "Container Storage"));

			result.AddPair(RatingConstants.RateCategory.WHS, ResString.GetMultilingualString("1de0e7ea-705b-4a45-a373-eeb1158bb919", "Product Warehouse"));
			result.AddPair(RatingConstants.RateCategory.TRW, ResString.GetMultilingualString("1291676e-69c2-4db8-a059-b3a918a8ffa1", "Transit Warehouse"));
			result.AddPair(RatingConstants.RateCategory.TWU, ResString.GetMultilingualString("e0813a96-54bd-4682-b712-3761b323af74", "Transit Warehouse Transportation Unit"));

			result.AddPair(RatingConstants.RateCategory.CYD, ResString.GetMultilingualString("5b3ea9b5-2f2e-406a-bf17-294ac271663f", "Storage && Handling"));
			result.AddPair(RatingConstants.RateCategory.CYU, ResString.GetMultilingualString("cff13e33-2c44-4221-aee7-84bfbfdc7234", "Transportation Unit"));
			result.AddPair(RatingConstants.RateCategory.CYM, ResString.GetMultilingualString("7d9ba2b3-1cef-4971-81a9-c8ad130a77de", "Maintenance && Repair"));

			result.AddPair(RatingConstants.RateCategory.TRN, ResString.GetMultilingualString("09661ca6-39c5-478e-87c8-bacdb0c9720c", "Port Transport"));
			result.AddPair(RatingConstants.RateCategory.TBC, ResString.GetMultilingualString("96bebc0d-71d1-4857-bac5-dd28049643aa", "Land Transport"));
			return result;
		}

		public static CodeDescriptionPairList GetRateCategoriesAndGroupsList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (CodeDescriptionPair category in GetRateCategoriesList())
			{
				var groupCaption = Groups.GetGroupNameByCategory(category.Code).Replace("LinerAndAgency", "Liner & Agency");
				var categoryDescription = category.MultilingualDescription;
				var categoryFullName = groupCaption == (NoResString)"Empty" ? categoryDescription : ResString.GetMultilingualString("e7b4247b-bf02-4a50-9143-4f58bc229c35", "{0} | {1}", groupCaption, categoryDescription); // Comparison string

				result.AddPair(category.Code, categoryFullName);
			}
			return result;
		}
	}

	public static class Groups
	{
		#region SuppressResourceStringsCheckRegion

		public const string Forwarding = "Forwarding";
		public const string CFS = "CFS";
		public const string LinerAndAgency = "LinerAndAgency";
		public const string Transport = "Transport";
		public const string Warehouse = "Warehouse";
		public const string Yard = "Yard";
		public const string Customs = "Customs";
		public const string Empty = "Empty";

		#endregion

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It thinks the switch statement is too complex because of the fallthroughs becoming gotos")]
		public static string GetGroupNameByCategory(string category)
		{
			switch (category)
			{
				case RatingConstants.RateCategory.AIR:
				case RatingConstants.RateCategory.FCL:
				case RatingConstants.RateCategory.LCL:
				case RatingConstants.RateCategory.ORG:
				case RatingConstants.RateCategory.DST:
					return Forwarding;

				case RatingConstants.RateCategory.SCO:
				case RatingConstants.RateCategory.SNC:
				case RatingConstants.RateCategory.SOR:
				case RatingConstants.RateCategory.SDE:
				case RatingConstants.RateCategory.SED:
				case RatingConstants.RateCategory.SID:
					return LinerAndAgency;

				case RatingConstants.RateCategory.CST:
				case RatingConstants.RateCategory.PAC:
				case RatingConstants.RateCategory.UNP:
					return CFS;

				case RatingConstants.RateCategory.TRN:
				case RatingConstants.RateCategory.TBC:
					return Transport;

				case RatingConstants.RateCategory.TWU:
				case RatingConstants.RateCategory.TRW:
				case RatingConstants.RateCategory.WHS:
					return Warehouse;

				case RatingConstants.RateCategory.CYD:
				case RatingConstants.RateCategory.CYU:
				case RatingConstants.RateCategory.CYM:
					return Yard;

				case RatingConstants.RateCategory.CAI:
				case RatingConstants.RateCategory.CFC:
				case RatingConstants.RateCategory.CLC:
				case RatingConstants.RateCategory.COR:
				case RatingConstants.RateCategory.CDS:
					return Customs;

				default:
					return Empty;
			}
		}

		public static ResourceStringData GetGroupCaptionByCategory(string category)
		{
			switch (GetGroupNameByCategory(category))
			{
				case Forwarding:
					return Res.GetData("c2a378ca-5c0d-4ec0-80f4-8cf93587bf43", "Forwarding");
				case CFS:
					return Res.GetData("1059e29e-f86c-4a1c-9c7b-5314205ca333", "CFS");
				case LinerAndAgency:
					return Res.GetData("500E5737-FCA4-4AC7-8493-8F2D196B392E", "Liner && Agency");
				case Transport:
					return Res.GetData("5AD76639-57BE-4B70-B97E-05B348F5AA39", "Transport");
				case Warehouse:
					return Res.GetData("b5392fa1-3869-4395-92bf-f589833466c2", "Warehouse");
				case Yard:
					return Res.GetData("da0bd929-c511-4117-81e8-5d100b137f00", "Yard");
				case Customs:
					return Res.GetData("4E431DCE-43A2-43F8-9FA9-66AE66A868DF", "Customs");
				default:
					return null;
			}
		}
	}
}
