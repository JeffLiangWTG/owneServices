using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class GlbReleaseNoteLookups : AutoGlbReleaseNoteLookups
	{
		public GlbReleaseNoteLookups(AutoGlbReleaseNote parent) : base(parent)
		{
		}

		#region Categories

		public CodeDescriptionPairList Categories
		{
			get
			{
				UntranslatableCodeDescriptionPairList result;

				if (Parent is AutoGlbReleaseNote parent && categoriesBlankSections.Contains(parent.GF_Section))
				{
					if (fCategoriesBlank == null)
					{
						fCategoriesBlank = CreateBaseUntranslatableCodeDescriptionPairList();
					}

					result = fCategoriesBlank;
				}
				else
				{
					if (fCategories == null)
					{
						fCategories = CreateBaseUntranslatableCodeDescriptionPairList();
						foreach (LicenceCheckpoint licence in Env.Licence.GetAllModuleCheckpoints().Where(x => !DataRegistry.Instance.ProductivityWiseModeEnabled || VisibleCategories.Contains(x.Name)))
						{
							fCategories.AddPair(licence.Name, licence.DisplayName);
						}

						fCategories.Sort();
					}

					result = fCategories;
				}

				return result;
			}
		}

		UntranslatableCodeDescriptionPairList fCategories;
		UntranslatableCodeDescriptionPairList fCategoriesBlank;
		readonly HashSet<string> categoriesBlankSections = new HashSet<string> { NewsSectionTypeList.Codes.BorderWise, NewsSectionTypeList.Codes.WiseTechAcademy };

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

					countries.Insert(0, new CodeDescriptionPair(AllCountriesCode, Res.GetString("521263ad-3228-4514-a54b-69db51199078", "All Countries/Regions")));
				}

				return countries;
			}
		}

		CodeDescriptionPairList countries;

		public const string AllCountriesCode = "";

		#endregion

		#region Sections

		public virtual ReadOnlyCodeDescriptionPairList SectionList
		{
			get { return sectionList ?? (sectionList = NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.WiseTechOnly)); }
		}
		ReadOnlyCodeDescriptionPairList sectionList;

		#endregion
	}
}
