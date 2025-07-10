using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business.Documents
{
	class PermittedRefDocTypeCollection : RefDocTypeCollection
	{
		readonly TrackingSiteUser user;

		public PermittedRefDocTypeCollection(BusinessObjectFactory factory, TrackingSiteUser user)
			: base(factory)
		{
			this.user = Argument.NotNull(user, nameof(user));
		}

		public PermittedRefDocTypeCollection(BusinessObjectFactory factory, ZQuery filter, TrackingSiteUser user)
			: base(factory, filter)
		{
			this.user = Argument.NotNull(user, nameof(user));
		}

		protected override bool MatchesFilterCore(RefDocType element, bool fetchOnlyFromLocalCache)
		{
			return user != null && user.CanViewDocument(Factory, element) && base.MatchesFilterCore(element, fetchOnlyFromLocalCache);
		}
	}
}
