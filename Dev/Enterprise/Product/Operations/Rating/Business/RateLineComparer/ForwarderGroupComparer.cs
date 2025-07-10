using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class ForwarderGroupComparer : BaseRateLineComparer
	{
		public override int Compare(FastLine line1, FastLine line2)
		{
			var provider1 = line1.ParentRateEntry.Supplier;
			var provider2 = line2.ParentRateEntry.Supplier;

			if (provider1 != null && provider2 != null && provider1.PK != provider2.PK)
			{
				if (GetForwarderGroup(provider1) == provider2)
				{
					return 1;
				}

				if (GetForwarderGroup(provider2) == provider1)
				{
					return -1;
				}
			}

			return 0;
		}

		static OrgHeader GetForwarderGroup(OrgHeader org)
		{
			return org.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderGroup, RelatedPartyDirectionList.Codes.Forwarder);
		}

		protected override string GetName()
		{
			return (NoResString)"Forwarder Group"; // log message, subject to change, more for support people as of now
		}
	}
}
