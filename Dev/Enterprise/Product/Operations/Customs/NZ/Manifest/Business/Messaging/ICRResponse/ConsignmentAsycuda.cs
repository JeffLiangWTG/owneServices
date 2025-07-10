using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	sealed class ConsignmentAsycuda : Consignment, IWriteOffStatus
	{
		public ConsignmentAsycuda(WriteOffResponseAsycuda response, string id, string status)
			: base(response, id, status, ZString.Empty)
		{
			manifestHeader = response.ManifestHeader;
			bill = manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == id);
		}
		readonly AsycudaManifestHeader manifestHeader;
		readonly AsycudaBill bill;

		protected override ZString CustomsStatusCore
		{
			get => bill?.ABL_BillStatus ?? ZString.Empty;
			set
			{
				if (bill != null)
				{
					bill.ABL_MessageStatus = LowValueManifestStatusList.Codes.Acknowledgement;
					if (value == LowValueConsignmentStatusList.Codes.ConsignmentInError || value == "REJ")
					{
						bill.ABL_BillStatus = LowValueConsignmentStatusList.Codes.EE;
					}
					else
					{
						bill.ABL_BillStatus = TSWStatus.GetCombinedWriteOffStatus;
					}
				}
			}
		}

		protected override ZString MessageStatusCore
		{
			get => bill?.ABL_MessageStatus ?? ZString.Empty;
			set
			{
				if (bill != null)
				{
					bill.ABL_MessageStatus = value;
				}
			}
		}

		protected override ZString HouseBillCore => bill?.ABL_BillNumber ?? ZString.Empty;

		public NZ.Business.Declaration.ECIWriteOff.CusEntryHeader EntryHeader => null;

		public ZString GoodsClearanceStatus => Status;

		public ZString CombinedStatus => bill?.ABL_BillStatus ?? ZString.Empty;

		public JobDeclaration Declaration => null;

		public ZString ResponseStatus => ZString.Empty;

		ZString ITSWStatus.EnterpriseStatus => Status;

		public ZString Agency => Response.ResponsibleGovernmentAgency;

		TSWStatus TSWStatus => tswStatus ?? (tswStatus = new TSWStatus(this));
		TSWStatus tswStatus;
	}
}
