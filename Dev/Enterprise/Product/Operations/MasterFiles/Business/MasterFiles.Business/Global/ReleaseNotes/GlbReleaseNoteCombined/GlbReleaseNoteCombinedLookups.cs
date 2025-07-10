using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business;

public class GlbReleaseNoteCombinedLookups : AutoGlbReleaseNoteCombinedLookups
{
	public GlbReleaseNoteCombinedLookups(AutoGlbReleaseNoteCombined parent) : base(parent)
	{
	}

	#region Categories

	public CodeDescriptionPairList Categories
	{
		get
		{
			UntranslatableCodeDescriptionPairList result;

			if (Parent is AutoGlbReleaseNoteCombined parent && categoriesBlankSections.Contains(parent.GF_Section))
			{
				categoriesBlank ??= CreateBaseUntranslatableCodeDescriptionPairList();

				result = categoriesBlank;
			}
			else
			{
				if (categories == null)
				{
					categories = CreateBaseUntranslatableCodeDescriptionPairList();

					foreach (var licence in Env.Licence.GetAllModuleCheckpoints().Where(x => !DataRegistry.Instance.ProductivityWiseModeEnabled || VisibleCategories.Contains(x.Name)))
					{
						categories.AddPair(licence.Name, licence.DisplayName);
					}

					categories.Sort();
				}

				result = categories;
			}

			return result;
		}
	}

	UntranslatableCodeDescriptionPairList categories;

	UntranslatableCodeDescriptionPairList categoriesBlank;

	readonly HashSet<string> categoriesBlankSections = [NewsSectionTypeList.Codes.BorderWise, NewsSectionTypeList.Codes.WiseTechAcademy];

	public HashSet<string> VisibleCategories
	{
		get
		{
			if (visibleCategories == null)
			{
				visibleCategories = new HashSet<string>();

				foreach (ModuleCategory category in ModuleTree.Tree.Categories.Values)
				{
					foreach (ModuleSection section in category.Sections.Values)
					{
						foreach (IMainFormModule module in section.Modules.Values)
						{
							var checkpoint = module.LicenceCheckpoint;

							while (checkpoint != null && !visibleCategories.Contains(checkpoint.Name))
							{
								visibleCategories.Add(checkpoint.Name);
								checkpoint = checkpoint.ParentCheckpoint;
							}
						}
					}
				}
			}

			return visibleCategories;
		}
	}

	HashSet<string> visibleCategories;

	UntranslatableCodeDescriptionPairList CreateBaseUntranslatableCodeDescriptionPairList()
	{
		var result = new UntranslatableCodeDescriptionPairList((NoResString)"Module names cannot be translated");
		result.AddPair("", "");

		return result;
	}

	#endregion

	#region Countries

	public CodeDescriptionPairList Countries
	{
		get
		{
			if (countries == null)
			{
				countries = new CodeDescriptionPairList();

				var collection = new RefCountryCollection(Factory);

				foreach (var country in collection)
				{
					countries.AddPair(country.Code, country.Description);
				}

				countries.Sort();

				countries.Insert(0, new CodeDescriptionPair(AllCountriesCode, Res.GetString("BB8DDB16-A12A-4840-BC7E-005DB0F89986", "All Countries/Regions")));
			}

			return countries;
		}
	}

	CodeDescriptionPairList countries;

	public const string AllCountriesCode = "";

	#endregion

	#region Sections

	public ReadOnlyCodeDescriptionPairList SectionList => sectionList ??= NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.WiseTechOnly);

	ReadOnlyCodeDescriptionPairList sectionList;

	#endregion
}
