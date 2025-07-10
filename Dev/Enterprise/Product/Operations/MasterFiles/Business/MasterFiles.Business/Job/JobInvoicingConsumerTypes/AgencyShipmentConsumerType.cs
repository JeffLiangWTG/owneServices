using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public abstract class AgencyShipmentConsumerType : JobInvoicingConsumerType
	{
		protected AgencyShipmentConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				var list = base.InvoiceTypeList;
				list.RemoveCode(InvoiceTypesList.Codes.DestinationChargesInvoice_Batching);
				list.RemoveCode(InvoiceTypesList.Codes.FreightInvoice_Batching);

				return list;
			}
		}
	}
}
