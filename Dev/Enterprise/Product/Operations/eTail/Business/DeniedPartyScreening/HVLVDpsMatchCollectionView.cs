using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class HVLVDpsMatchCollectionView : NonPersistentBusinessObjectCollectionView<HVLVDpsMatch>
	{
		public HVLVDpsMatchCollectionView(HVLVDpsMatchCollection collection) : base(collection)
		{
		}

		public bool ShowHasChangesOnly
		{
			get => showHasChangesOnly;
			set
			{
				if (value != showHasChangesOnly)
				{
					showHasChangesOnly = value;
					Rebuild();
				}
			}
		}
		bool showHasChangesOnly = true;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override HVLVDpsMatch CreateNonPersistentBusinessObject()
		{
			var header = Factory.New<OrgHeader>();
			var party = new ScreeningParty(header, "", header);
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = new List<ProfileHeaderInfo>() }, new DpsRequestHeaderWithAddressMatching());
			return new HVLVDpsMatch(responseWithParty, Factory);
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var result = true;
			if (element is HVLVDpsMatch match && ShowHasChangesOnly)
			{
				result = match.PotentialMatchesCount > 0;
			}

			return result;
		}
	}
}
