using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class USFSISLotCollection : DependentCusAddInfoCollection<USFSISLot, USInvoiceLineFSISLine>
	{
		public USFSISLotCollection(USInvoiceLineFSISLine certificate)
			: base(certificate, CusAddInfoTypeAttribute.Codes.USFSISLot)
		{ }

		protected override bool AllowNewCore
		{
			get
			{
				var certificate = Master;
				return certificate != null && !certificate.IsElectronicallyCertificated;
			}
		}
	}
}
