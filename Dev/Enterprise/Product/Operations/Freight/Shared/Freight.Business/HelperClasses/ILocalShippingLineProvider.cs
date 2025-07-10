using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface ILocalShippingLineProvider
	{
		OrgHeader ShippingLine { get; }
	}
}
