namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Core;

	public class OrgContactLookups : AutoOrgContactLookups
	{
		public OrgContactLookups(AutoOrgContact parent) : base(parent)
		{
		}

		#region Job Categories

		public ReadOnlyCodeDescriptionPairList JobCategory_List => Factory.GetCachedValue(
			"OrgContactLookups.JobCategory_List",
			() => CreateJobCategoryList());

		public static CodeDescriptionPairList CreateJobCategoryList()
		{
			return OrganisationsDataRegistry.Instance.ContactJobCategories.Value.GetCodeDescriptionPairList();
		}

		#endregion

		#region Contact Sources

		public ReadOnlyCodeDescriptionPairList ContactSourceList
		{
			get { return OrganisationsDataRegistry.Instance.ContactSourceTypes.Value; }
		}

		#endregion

		#region Gender

		public CodeDescriptionPairList Gender
		{
			get
			{
				return Factory.GetCachedValue("GendersList", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Core.Constants.Genders.Woman, Core.Constants.GenderDescriptions.Woman);
					list.AddPair(Core.Constants.Genders.Man, Core.Constants.GenderDescriptions.Man);
					list.AddPair(Core.Constants.Genders.NotSpecified, Core.Constants.GenderDescriptions.NotSpecified);
					list.AddPair(Core.Constants.Genders.Custom, Core.Constants.GenderDescriptions.Custom);
					list.AddPair(Core.Constants.Genders.Agender, Core.Constants.GenderDescriptions.Agender);
					list.AddPair(Core.Constants.Genders.NonBinary, Core.Constants.GenderDescriptions.NonBinary);
					return list;
				});
			}
		}

		#endregion

		#region Nationality Types

		public IBusinessObjectCollection NationalityTypes
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion
	}
}
