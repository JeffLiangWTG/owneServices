using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.VN.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.VNManifest.IAsycudaBill
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.VietNam;
	}
}
