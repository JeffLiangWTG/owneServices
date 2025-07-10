using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.SGAccess.IAsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
	}
}
