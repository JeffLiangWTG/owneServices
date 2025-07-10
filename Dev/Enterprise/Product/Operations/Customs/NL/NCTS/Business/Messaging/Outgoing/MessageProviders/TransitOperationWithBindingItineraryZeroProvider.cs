namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class TransitOperationWithBindingItineraryZeroProvider : TransitOperationProvider
{
	public TransitOperationWithBindingItineraryZeroProvider(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	public override bool BindingItinerary => false;
}
