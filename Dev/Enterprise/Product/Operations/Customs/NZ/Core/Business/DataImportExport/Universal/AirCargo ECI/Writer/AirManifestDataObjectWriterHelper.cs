using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Express;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class AirManifestDataObjectWriterHelper : DataTransfer.Universal.AirManifest.AirManifestDataObjectWriterHelper
	{
		public AirManifestDataObjectWriterHelper(DataTransfer.Universal.AirManifest.AirManifestDataObjectWriterHelper list)
			: base(list)
		{
		}

		public AirManifestDataObjectWriterHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AirManifestDataObjectWriterHelper(CusMAWB mawb)
			: base(mawb)
		{
		}

		public AirManifestDataObjectWriterHelper(CusHAWB hawb, DataTransfer.Universal.AirManifest.AirManifestDataObjectWriterHelper list)
			: base(hawb, list)
		{
		}

		public AirManifestDataObjectWriterHelper(CusHAWB hawb)
			: base(hawb)
		{
		}

		protected new CusMAWB MAWB
		{
			get { return (CusMAWB)base.MAWB; }
		}

		protected new CusHAWB HAWB
		{
			get { return (CusHAWB)base.HAWB; }
		}
	}
}
