//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbPersonLookups
//
//    This class should be used for overriding collections in AutoGlbPersonLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonLookups : AutoGlbPersonLookups
	{
		public GlbPersonLookups(AutoGlbPerson parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Genders
		{
			get
			{
				return Factory.GetCachedValue("GendersList", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Core.Constants.Genders.Woman, Core.Constants.GenderDescriptions.Woman);
					list.AddPair(Core.Constants.Genders.Man, Core.Constants.GenderDescriptions.Man);
					list.AddPair(Core.Constants.Genders.Agender, Core.Constants.GenderDescriptions.Agender);
					list.AddPair(Core.Constants.Genders.NonBinary, Core.Constants.GenderDescriptions.NonBinary);
					list.AddPair(Core.Constants.Genders.NotSpecified, Core.Constants.GenderDescriptions.NotSpecified);
					list.AddPair(Core.Constants.Genders.Custom, Core.Constants.GenderDescriptions.Custom);
					return list;
				});
			}
		}

		public static class TitleNames
		{
			public static ResourceString Mr
			{
				get { return ResString.GetMultilingualString("GlbPersonLookups|Mr.", "Mr."); }
			}

			public static ResourceString Ms
			{
				get { return ResString.GetMultilingualString("GlbPersonLookups|Ms.", "Ms."); }
			}

			public static ResourceString Mrs
			{
				get { return ResString.GetMultilingualString("GlbPersonLookups|Mrs", "Mrs"); }
			}

			public static ResourceString Miss
			{
				get { return ResString.GetMultilingualString("GlbPersonLookups|Miss", "Miss"); }
			}

			public static ResourceString Dr
			{
				get { return ResString.GetMultilingualString("GlbPersonLookups|Dr.", "Dr."); }
			}
		}

		public CodeDescriptionPairList Titles
		{
			get
			{
				return Factory.GetCachedValue("TitleList", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(TitleNames.Mr, TitleNames.Mr);
					list.AddPair(TitleNames.Ms, TitleNames.Ms);
					list.AddPair(TitleNames.Mrs, TitleNames.Mrs);
					list.AddPair(TitleNames.Miss, TitleNames.Miss);
					list.AddPair(TitleNames.Dr, TitleNames.Dr);
					return list;
				});
			}
		}

		public new GlbPerson Parent
		{
			get { return (GlbPerson)base.Parent; }
		}

		public CodeDescriptionPairList Languages
		{
			get { return Parent.PER_RN_NKCountry == Core.Constants.CountryCodes.SouthAfrica || Parent.PER_RN_NKNationalityCodeISO == Core.Constants.CountryCodes.SouthAfrica ? LanguagesOfSouthAfrica : EnglishIsTheStandardBusinessLanguge; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Well this one can get lost too I think, How can I ask for this in one language when clearly two languages are being displayed??")]
		public CodeDescriptionPairList LanguagesOfSouthAfrica
		{
			get
			{
				return Factory.GetCachedValue("LanguagesOfSouthAfrica", delegate
				{
					//https://en.wikipedia.org/wiki/Languages_of_South_Africa
					var list = new CodeDescriptionPairList();
					list.AddPair(Core.Constants.Languages.English, "English");
					list.AddPair("ZUL", "Zulu / isiZulu");
					list.AddPair("XHO", "Xhosa / isiXhosa");
					list.AddPair("AFR", "Afrikaans");
					list.AddPair("NSO", "Northern Sotho / Sesotho sa Leboa");
					list.AddPair("TSN", "Tswana / Setswana");
					list.AddPair("SOT", "Sesotho");
					list.AddPair("TSO", "Tsonga / Xitsonga");
					list.AddPair("SSW", "Swazi / siSwati");
					list.AddPair("VEN", "Venda / Tshivenḓa");
					list.AddPair("NBL", "Ndebele / isiNdebele");
					return list;
				});
			}
		}
		public CodeDescriptionPairList EnglishIsTheStandardBusinessLanguge
		{
			get
			{
				return Factory.GetCachedValue("EnglishLanguage", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Core.Constants.Languages.English, Res.GetString("GlbPersonLookups.English", "English"));
					return list;
				});
			}
		}

		public override RefCountryCollection Countries
		{
			get
			{
				if (countries == null)
				{
					countries = base.Countries;
					countries.ApplySort(RefCountrySchema.Constants.RN_Desc, System.ComponentModel.ListSortDirection.Ascending);
				}
				return countries;
			}
		}
		RefCountryCollection countries;

		public override RefCountryCollection NationalityCodeISOs => Countries;

		#region StateList

		public CodeDescriptionPairList StateList
		{
			get
			{
				var result = new UntranslatableCodeDescriptionPairList((NoResString)"States from RefCountryState table");
				var country = Parent.Country;
				var stateList = (country != null) ? new OrgCodeLists().State_List(country) : new CodeDescriptionPairList();
				result.AddRange(stateList);

				return result;
			}
		}

		#endregion
	}
}
