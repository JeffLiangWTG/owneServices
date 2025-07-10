using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaTransferHeaderLookups : ASYCUDA.Business.AsycudaTransferHeaderLookups
	{
		public AsycudaTransferHeaderLookups(AsycudaTransferHeader parent) : base(parent)
		{
		}

		protected override IBusinessObjectCollection CarrierCollectionCore() => new USCarrierCombinedCollection(Factory);

		protected override IBusinessObjectCollection ShippingProvidersCore() => new ShippingProviderCollection(Factory);
	}
}
