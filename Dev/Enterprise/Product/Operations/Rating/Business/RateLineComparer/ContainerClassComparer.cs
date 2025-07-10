using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	sealed class ContainerClassComparer : BaseRateLineComparer
	{
		public ContainerClassComparer(ZString containerCode)
		{
			this.containerCode = containerCode;
		}

		readonly ZString containerCode;

		// sort matching lines after non-matching lines;
		public override int Compare(FastLine line1, FastLine line2)
		{
			var entry1Container = line1.ParentRateEntry.Container;
			var entry2Container = line2.ParentRateEntry.Container;

			bool entry1Matches;
			bool entry2Matches;

			if (containerCode.IsEmpty)
			{
				entry1Matches = (entry1Container == null);
				entry2Matches = (entry2Container == null);
			}
			else
			{
				entry1Matches = (entry1Container != null && entry1Container.RC_Code == containerCode);
				entry2Matches = (entry2Container != null && entry2Container.RC_Code == containerCode);
			}

			return (entry1Matches ? 1 : 0) - (entry2Matches ? 1 : 0);
		}

		protected override string GetName()
		{
			return (NoResString)"Container Class"; // log message, subject to change, more for support people as of now
		}
	}
}
