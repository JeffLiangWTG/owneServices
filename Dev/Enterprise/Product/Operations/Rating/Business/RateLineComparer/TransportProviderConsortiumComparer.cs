using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class TransportProviderConsortiumComparer : ConsortiumComparer
	{
		public override int Compare(FastLine line1, FastLine line2)
		{
			return Compare(line1.ParentRateEntry.TransportProvider, line2.ParentRateEntry.TransportProvider);
		}

		protected override string GetName()
		{
			return (NoResString)"Transport Provider Consortium"; // log message, subject to change, more for support people as of now
		}
	}
}
