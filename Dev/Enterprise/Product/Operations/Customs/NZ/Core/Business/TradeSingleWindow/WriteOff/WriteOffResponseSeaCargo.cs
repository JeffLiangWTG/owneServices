using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	sealed class WriteOffResponseSeaCargo : WriteOffResponse, IWriteOffStatus
	{
		internal WriteOffResponseSeaCargo(BaseTSWResponse response)
			: base(response)
		{
			OceanBill = (CusSCAOceanBill)response.LinkedObject;
			this.response = response;
		}

		public readonly CusSCAOceanBill OceanBill;
		readonly BaseTSWResponse response;

		protected override IEnumerable<ZString> ConsignmentIDs
		{
			get
			{
				return ErrorCodes.Any() && !ConsignmentsWithResponse.Any()
					? OceanBill.HouseBills.Cast<CusSCAHouse>().Where(x => !x.CA_MessageStatus.IsEmpty && x.CA_MessageStatus != LowValueConsignmentStatusList.Codes.NotSentToCustoms).Select(x => x.CA_HouseBill)
					: ConsignmentsWithResponse.Select(x => (ZString)x.ID);
			}
		}

		protected override ZString CustomsDeliveryInstructionsCore { get; set; }

		protected override ZString MasterBillCore => OceanBill.CB_OceanBill;

		protected override Consignment CreateConsignment(string id, string status, string movementStatus, int msgSequence) => new ConsignmentSeaCargo(this, id, status, movementStatus);

		protected override string GetJobID() => OceanBill.CB_MessageReference;

		protected override string GetJobName() => Core.Constants.NZCustoms.ExpressSeaCargoName;

		protected override bool GetIsImportEntry() => OceanBill.IsImport;

		protected override void SetCustomsStatus(ZString newValue)
		{
			OceanBill.CB_MessageStatus = LowValueManifestStatusList.Codes.Acknowledgement;
			OceanBill.CB_CustomsStatus = newValue;
			LogResponseStatus();
			if (IsICR && OceanBill.CB_CustomsStatus == LowValueManifestStatusList.Codes.ManifestCancelled)
			{
				foreach (CusSCAHouse houseBill in OceanBill.HouseBills)
				{
					houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
					houseBill.CA_MessageStatus = LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
				}
			}
		}

		void LogResponseStatus()
		{
			var messageRejectedInError = MessageType == TransactionTypeList.Codes.Error || AllConsignments.Any(consignment => consignment != null && consignment.IsInError);
			var messageAccepted = AllConsignments.Any(consignment => consignment != null && (consignment.IsWrittenOff || consignment.IsFormalDeclarationRequired));

			OceanBill.LogResponseStatus(IsCancellation, messageRejectedInError, messageAccepted);
		}

		protected override void SetECINumber(ZString newValue) => OceanBill.EntryNumber = newValue;

		#region IWriteOffStatus Members

		Declaration.ECIWriteOff.CusEntryHeader IWriteOffStatus.EntryHeader => null;

		ZString IWriteOffStatus.CustomsStatus => response.Status;

		ZString IWriteOffStatus.GoodsClearanceStatus => response.GoodsClearanceStatus;

		ZString IWriteOffStatus.CombinedStatus => OceanBill.CB_CustomsStatus;

		Declaration.JobDeclaration ITSWStatus.Declaration => null;

		ZString ITSWStatus.ResponseStatus => ZString.Empty;

		ZString ITSWStatus.EnterpriseStatus => Status;

		ZString ITSWStatus.Agency => response.ResponsibleGovernmentAgency;

		#endregion
	}
}
