using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public sealed class WriteOffResponseAsycuda : WriteOffResponse
	{
		public WriteOffResponseAsycuda(BaseTSWResponse response)
			: base(response)
		{
		}

		public AsycudaManifestHeader ManifestHeader
		{
			get { return (AsycudaManifestHeader)LinkedObject; }
		}

		public override bool ShouldShowSummaryOnReport => false;

		protected override IEnumerable<ZString> ConsignmentIDs
		{
			get
			{
				return ManifestHeader.Bills.Cast<AsycudaBill>().Select(x => x.ABL_BillNumber);
			}
		}

		protected override ZString CustomsDeliveryInstructionsCore { get; set; }

		protected override ZString MasterBillCore => ManifestHeader.MasterBill?.ABL_BillNumber ?? ZString.Empty;

		protected override Consignment CreateConsignment(string id, string status, string movementStatus, int msgSequence)
		{
			return new ConsignmentAsycuda(this, id, status);
		}

		protected override string GetJobID()
		{
			return ManifestHeader.AMA_JobReference;
		}

		protected override string GetJobName()
		{
			return "Manifest";
		}

		protected override bool GetIsImportEntry()
		{
			return ManifestHeader.AMA_ManifestType == NZManifestTypes.Codes.ICR;
		}

		protected override void SetCustomsStatus(ZString newValue)
		{
			ManifestHeader.AMA_MessageStatus = NZMessageStatusList.Codes.Acknowledged;
		}

		protected override void SetECINumber(ZString newValue)
		{
			ManifestHeader.RegistrationNumber = newValue;
		}
	}
}
