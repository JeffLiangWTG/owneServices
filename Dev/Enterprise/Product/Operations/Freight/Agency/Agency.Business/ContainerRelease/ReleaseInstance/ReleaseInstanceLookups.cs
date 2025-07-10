using CargoWise.EntityFramework;
using CargoWise.EventReference;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseInstanceLookups : ZLookups
	{
		public ReleaseInstanceLookups(ReleaseInstance parent)
			: base(parent) { }

		#region ReleaseType_List

		public CodeDescriptionPairList ReleaseType_List
		{
			get { return releaseType_List ?? (releaseType_List = NewReleaseTypes()); }
		}
		CodeDescriptionPairList releaseType_List;

		CodeDescriptionPairList NewReleaseTypes()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.EventReferenceReleaseTypes.Codes.Cancellation, Constants.EventReferenceReleaseTypes.Desc.Cancellation);
			result.AddPair(Constants.EventReferenceReleaseTypes.Codes.Original, Constants.EventReferenceReleaseTypes.Desc.Original);
			result.AddPair(Constants.EventReferenceReleaseTypes.Codes.Reprint, Constants.EventReferenceReleaseTypes.Desc.Reprint);
			result.AddPair(Constants.EventReferenceReleaseTypes.Codes.Revised, Constants.EventReferenceReleaseTypes.Desc.Revised);

			return result;
		}

		#endregion
	}
}
