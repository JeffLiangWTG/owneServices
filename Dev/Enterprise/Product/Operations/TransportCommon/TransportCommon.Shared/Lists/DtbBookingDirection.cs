using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.TransportCommon.Shared
{
	public enum DtbBookingDirection
	{
		None,

		LOC,
		PIC,
		DLV
	}

	public static class DocumentDirectionExtension
	{
		public static DtbBookingDirection ToDtbBookingDirection(this DocumentDirection docDirection)
		{
			switch (docDirection)
			{
				case DocumentDirection.DEP:
					return DtbBookingDirection.PIC;
				case DocumentDirection.ARV:
					return DtbBookingDirection.DLV;
				default:
					return DtbBookingDirection.LOC;
			}
		}
	}
}
