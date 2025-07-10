using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.ACEManifest.IAsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
	}
}
