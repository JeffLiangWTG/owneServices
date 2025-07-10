using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	partial class AutoDeliveryOrderHeader : DeclarationDocumentData<USDeliveryOrderHeaderAddInfo>
	{
		public USCarrierCombined CarriersLocalAgent
		{
			get { return Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, US_UI_NKCarriersLocalAgent)); }
		}
	}
}