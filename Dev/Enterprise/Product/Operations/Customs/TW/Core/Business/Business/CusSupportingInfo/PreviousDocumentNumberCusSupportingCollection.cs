using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class PreviousDocumentNumberCusSupportingCollection : Customs.Business.CusSupportingInfoCollection<PreviousDocumentNumberCusSupporting>
	{
		public PreviousDocumentNumberCusSupportingCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.PreviousDocumentNumber)
		{
		}
	}
}
