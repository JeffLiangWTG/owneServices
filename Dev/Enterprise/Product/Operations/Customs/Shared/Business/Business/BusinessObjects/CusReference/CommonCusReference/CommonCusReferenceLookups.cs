using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CommonCusReferenceLookups : CusReferenceLookups
	{
		public CommonCusReferenceLookups(CommonCusReference parent)
			: base(parent)
		{
		}

		public OrganisationsFindBoxCollection OwnersList => new OrganisationsFindBoxCollection(Factory);

		public virtual CodeDescriptionPairList CodeList => Factory.GetCachedValue<CodeDescriptionPairList>();
	}
}
