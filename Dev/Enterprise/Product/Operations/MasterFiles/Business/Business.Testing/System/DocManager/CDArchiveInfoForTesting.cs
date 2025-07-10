using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CDArchiveInfoForTesting : CDArchiveInfo
	{
		public CDArchiveInfoForTesting(BusinessObject parent)
			: base(parent)
		{
		}

		public override ZString ConsigneeCode
		{
			get { return "CONSIGNEE"; }
		}

		public override ZString ConsignorCode
		{
			get { return "CONSIGNOR"; }
		}

		public override ZString[] ContainerNumbersList
		{
			get { return new ZString[] { "11111", "22222", "33333", "44444", "55555" }; }
		}

		public override ZString Destination
		{
			get { return "USLAX"; }
		}

		public override ZString Origin
		{
			get { return "AUSYD"; }
		}

		public override ZString[] EntryNumbersList
		{
			get { return new ZString[] { "B00001000", "B00001001" }; }
		}

		public override ZDateTime ETA
		{
			get { return new ZDateTime(2004, 12, 24); }
		}

		public override ZDateTime ETD
		{
			get { return new ZDateTime(2004, 12, 12); }
		}

		public override ZString HouseBill
		{
			get { return "H12345"; }
		}

		public override ZString[] InvoiceNumbersList
		{
			get { return new ZString[] { "111", "222", "333", "444", "555" }; }
		}

		public override ZString JobNumber
		{
			get { return "S00001000"; }
		}

		public override ZString MasterBill
		{
			get { return "M12345"; }
		}

		public override ZString[] OrderNumbersList
		{
			get { return new ZString[] { "1111", "2222", "3333", "4444", "5555" }; }
		}

		public override ZString Vessel
		{
			get { return "VESSEL ABC"; }
		}

		public override ZString VoyageFlight
		{
			get { return "VOYAGE 12345"; }
		}
	}
}
