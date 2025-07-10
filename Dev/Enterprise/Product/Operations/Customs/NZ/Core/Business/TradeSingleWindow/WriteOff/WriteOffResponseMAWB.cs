using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	sealed class WriteOffOResponseMAWB : WriteOffResponse, IWriteOffStatus
	{
		internal WriteOffOResponseMAWB(BaseTSWResponse response)
			: base(response)
		{
			this.response = response;
		}
		readonly BaseTSWResponse response;

		public CusMAWB Mawb
		{
			get { return (CusMAWB)LinkedObject; }
		}

		#region Overrides

		protected override IEnumerable<ZString> ConsignmentIDs => Mawb.ChildBills.Cast<CusHAWB>().Select(hawb => hawb.CS_HAWB);

		protected override ZString CustomsDeliveryInstructionsCore
		{
			get { return Mawb.CM_CustomsDeliveryInstructions; }
			set { Mawb.CM_CustomsDeliveryInstructions = value; }
		}

		protected override ZString MasterBillCore
		{
			get { return Mawb.FormattedMasterBill; }
		}

		protected override Consignment CreateConsignment(string id, string status, string movementStatus, int msgSequence)
		{
			return new ConsignmentHAWB(this, id, status, movementStatus, msgSequence);
		}

		protected override string GetJobID()
		{
			return Mawb.CM_MessageReference;
		}

		protected override string GetJobName()
		{
			return Constants.NZCustoms.ExpressECIName;
		}

		protected override bool GetIsImportEntry() => Mawb.IsImport;

		protected override void SetCustomsStatus(ZString newValue)
		{
			Mawb.CM_CustomsStatus = newValue;
			LogResponseStatus();
			Mawb.RequireLogManifestStatusIfNeeded();
			if (IsICR && Mawb.CM_CustomsStatus == LowValueManifestStatusList.Codes.ManifestCancelled)
			{
				foreach (CusHAWB houseBill in Mawb.ChildBills)
				{
					houseBill.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
				}
			}
		}

		void LogResponseStatus()
		{
			var messageRejectedInError = MessageType == TransactionTypeList.Codes.Error || AllConsignments.Any(consignment => consignment != null && consignment.IsInError);
			var messageAccepted = AllConsignments.Any(consignment => consignment != null && (consignment.IsWrittenOff || consignment.IsFormalDeclarationRequired));

			Mawb.LogResponseStatus(IsCancellation, messageRejectedInError, messageAccepted);
		}

		protected override void SetECINumber(ZString newValue)
		{
			Mawb.ECINumber = newValue;
		}

		#endregion // Overrides

		#region IWriteOffStatus Members

		Declaration.ECIWriteOff.CusEntryHeader IWriteOffStatus.EntryHeader => null;

		ZString IWriteOffStatus.CustomsStatus => response.Status;

		ZString IWriteOffStatus.GoodsClearanceStatus => response.GoodsClearanceStatus;

		ZString IWriteOffStatus.CombinedStatus => Mawb.CM_CustomsStatus;

		Declaration.JobDeclaration ITSWStatus.Declaration => null;

		ZString ITSWStatus.ResponseStatus => ZString.Empty;

		ZString ITSWStatus.EnterpriseStatus => Status;

		ZString ITSWStatus.Agency => response.ResponsibleGovernmentAgency;

		#endregion
	}
}

// Tested in CREMessageProcessorMAWBTest
