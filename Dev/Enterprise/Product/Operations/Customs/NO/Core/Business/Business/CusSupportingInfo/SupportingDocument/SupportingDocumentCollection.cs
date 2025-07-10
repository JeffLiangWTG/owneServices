using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business
{
	public class SupportingDocumentCollection : Customs.Business.CusSupportingInfoCollection<SupportingDocument>
	{
		public SupportingDocumentCollection(BusinessObject parent) : base(parent, Customs.Common.NO.CusSupportingInfoTypeList.Codes.SupportingDocument)
		{
		}

		public bool HasAnyWithCode(ZString csi_code)
		{
			return this.Cast<SupportingDocument>().Any(x => x.CSI_Code == csi_code);
		}
	}
}
