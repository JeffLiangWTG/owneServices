using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class ProfileHeaderCollection : NonPersistentBusinessObjectCollection<ProfileHeader>
	{
		public ProfileHeaderCollection(DpsResponse response, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(response, nameof(response));
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(response.Profiles, nameof(response.Profiles));

			var filteredProfiles = response.Profiles
				.Select(p => new ProfileHeader(p, response, Factory))
				.Where(x => x.Level > ProfileHeader.RiskLevel.Low && x.IsValid && !x.IsExcluded)
				.OrderByDescending(x => x.Level);

			if (filteredProfiles.Any())
			{
				AddRange(filteredProfiles);
			}
		}

		public override bool ReadOnly => true;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
