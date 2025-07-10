//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewUNDGAttributeLookups
//
//    This class should be used for overriding collections in AutoViewUNDGAttributeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ViewUNDGAttributeLookups : AutoViewUNDGAttributeLookups
	{
		public ViewUNDGAttributeLookups(AutoViewUNDGAttribute parent) : base(parent)
		{
		}

		public static class TypeConstants
		{
			public const string ProperShippingName = "PSN";
			public const string Observations = "OBS";
			public const string Properties = "PRP";
			public const string QualifyingDescriptiveText = "QDT";
			public const string SpecialProvisions = "SPP";
			public const string StowageSegmentation_DangerousGoods = "DLG";
			public const string StowageSegmentation_Cargo = "CPV";
			public const string CrossReferences = "OTN";
			public const string SegregationGroups = "SGG";
			public const string UsrUSDOTShippingNames = "USN";
		}

		public CodeDescriptionPairList Types
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(TypeConstants.ProperShippingName, Res.GetString("75e50b35-710d-443c-834e-852bd9f89ce4", "Names"));
				list.AddPair(TypeConstants.Observations, Res.GetString("4b5cc40f-a687-4f73-9575-ca355b265b04", "Observations"));
				list.AddPair(TypeConstants.Properties, Res.GetString("ac7383f3-6986-48c4-ae8c-4f525fe0e94b", "Properties"));
				list.AddPair(TypeConstants.QualifyingDescriptiveText, Res.GetString("3aa8d014-303c-4752-b712-48e57567845d", "Qualifying Descriptive Text"));
				list.AddPair(TypeConstants.CrossReferences, Res.GetString("DDC97520-F1D2-4782-9DDE-34E07B6FE6FA", "Cross References"));
				return list;
			}
		}

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}

		public virtual UNDGSubstanceCollection UNDGSubstances
		{
			get { return new UNDGSubstanceCollection(Factory); }
		}
	}
}
