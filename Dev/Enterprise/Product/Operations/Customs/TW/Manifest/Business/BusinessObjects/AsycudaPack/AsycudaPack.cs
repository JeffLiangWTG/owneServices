using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
		, Integration.Customs.ASYCUDA.TWManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override MasterFiles.Business.UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}

		UNDGDataItemCollection fUNDGs;
	}
}
