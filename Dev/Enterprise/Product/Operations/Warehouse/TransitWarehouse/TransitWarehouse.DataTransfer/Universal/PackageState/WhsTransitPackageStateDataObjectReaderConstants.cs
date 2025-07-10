namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public static class WhsTransitPackageStateDataObjectReaderConstants
	{
		public enum WhsTransitPackageStatePopulateStrategy
		{
			New,
			Update,
			Delete,
			Attach,
			Detach,
		}

		public enum WhsTransitPackageStateUpdateStrategy
		{
			CompleteUpdate,
			PartialUpdate
		}

		public enum ProcessType
		{
			RCN,
			DetachASNFromRCN,
			ASN,
			DLL,
			DCN
		}
	}
}
