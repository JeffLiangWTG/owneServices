using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class TTBCOLAAndCertificateCollection : DependentCusAddInfoCollection<TTBCOLAAndCertificate, TTBLine>
	{
		public TTBCOLAAndCertificateCollection(TTBLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.USTTBCOLAAndCertificate)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				var result = base.AllowNewCore;
				if (result)
				{
					var ttbLine = Master;
					result = ttbLine != null && !ttbLine.IsTobaccoProgramType;
				}
				return result;
			}
		}
	}
}
