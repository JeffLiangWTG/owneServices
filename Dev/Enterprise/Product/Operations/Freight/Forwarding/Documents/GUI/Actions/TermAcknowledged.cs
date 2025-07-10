using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Actions
{
	public sealed class TermAcknowledged
	{
		public TermAcknowledged(bool hasBeenAcknowledged, int versionNo)
		{
			HasBeenAcknowledged = hasBeenAcknowledged;
			VersionNo = versionNo;
		}

		public ZInt VersionNo { get; set; }
		public ZBool HasBeenAcknowledged { get; set; }
	}
}
