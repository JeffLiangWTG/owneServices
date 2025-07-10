namespace Enterprise.Packing.Business
{
	public enum OrphanScanJobType
	{
		CSN, // Consignment

#if DEBUG
		DUM, // Dummy
#endif
	}
}
