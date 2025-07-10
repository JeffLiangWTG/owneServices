using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGCommonDataLookups : AutoUNDGCommonDataLookups
	{
		public UNDGCommonDataLookups(AutoUNDGCommonData parent) : base(parent)
		{
		}

		public static class TypeConstants
		{
			public const string SpecialProvisions = "SPP";
			public const string StowageSegmentationRequirements = "STS";
		}

		public CodeDescriptionPairList Types
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(TypeConstants.SpecialProvisions, Res.GetString("af46a00e-3240-4bfc-b5b5-b6d532888c44", "Special Provision"));
				list.AddPair(TypeConstants.StowageSegmentationRequirements, Res.GetString("375d19d7-cb9e-4b0b-be32-00b4717acbe3", "Stowage/Segmentation Requirement"));

				return list;
			}
		}

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}
	}
}
