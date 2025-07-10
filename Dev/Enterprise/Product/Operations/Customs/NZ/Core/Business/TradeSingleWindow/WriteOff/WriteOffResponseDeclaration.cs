using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	class WriteOffResponseDeclaration : WriteOffResponse, IWriteOffStatus
	{
		internal WriteOffResponseDeclaration(BaseTSWResponse response)
			: base(response)
		{
			this.response = response;
		}
		readonly BaseTSWResponse response;

		public CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)LinkedObject; }
		}

		#region Overrides

		protected override IEnumerable<ZString> ConsignmentIDs
		{
			get { yield return EntryHeader.Declaration.JE_HouseBill; }
		}

		protected override ZString CustomsDeliveryInstructionsCore
		{
			get { return EntryHeader.CH_CustomsDeliveryInstructions; }
			set { EntryHeader.CH_CustomsDeliveryInstructions = value; }
		}

		protected override ZString MasterBillCore
		{
			get { return EntryHeader.Declaration.FormattedMasterBill; }
		}

		protected override Consignment CreateConsignment(string id, string status, string movementStatus, int msgSequence)
		{
			return string.IsNullOrEmpty(id) ? null : new ConsignmentDeclaration(this, id, status);
		}

		protected override string GetJobID()
		{
			return EntryHeader.Declaration.JE_DeclarationReference;
		}

		protected override string GetJobName()
		{
			return "ECI Write-Off";
		}

		protected override bool GetIsImportEntry() => EntryHeader.Declaration.IsImport;

		protected override void SetCustomsStatus(ZString newValue)
		{
			EntryHeader.CH_EntryStatus = newValue;
			var declaration = EntryHeader.Declaration;

			if (response.Status == StatusList.Codes.EciOutwardReportRejectedErrorReportAttached || response.Status == StatusList.Codes.EntryRejected || response.Status == StatusList.Codes.CustomsProcessingError)
			{
				declaration.JE_MessageStatus = response.Status;
				declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
				declaration.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
				var outgoingMessage = response.OutgoingMessage;
				if (outgoingMessage != null && outgoingMessage.IsAnOriginal)
				{
					declaration.JE_EDITransmitDate = EntryHeader.CH_EDITransmitDate = ZDateTime.Empty;  // if this rejection is in response to an original message send, reset transmit date
				}
			}
			else
			{
				var agency = response.ResponsibleGovernmentAgency;
				var responseTime = response.ResponseTime;
				var status = string.IsNullOrEmpty(response.GoodsClearanceStatus) ? string.IsNullOrEmpty(response.GoodsStatus) ? response.Status : response.GoodsStatus : response.GoodsClearanceStatus;
				if (agency == ResponsibleGovernmentAgencyList.Codes.MPIBIO)
				{
					UpdateBIOStatus(responseTime, status);
				}
				else if (agency == ResponsibleGovernmentAgencyList.Codes.NZCS)
				{
					UpdateNZCSStatus(responseTime, status);
				}

				if (response.Status != StatusList.Codes.Acknowledgement)
				{
					if (!EntryHeader.IsManifestEntry && declaration.IsImport)
					{
						declaration.JE_EntryStatus = TSWStatus.GetWriteOffEntryStatus;
						declaration.AgencyMessageBeingProcessed = response.ResponsibleGovernmentAgency;
						declaration.JE_TSWCombinedStatus = TSWStatus.GetCombinedWriteOffStatus;
					}

					if (declaration.JE_EntryStatus == LowValueConsignmentStatusList.Codes.ConsignmentCancelled && response.Status == StatusList.Codes.EntryCancelled)
					{
						EntryHeader.CH_IsActive = false;
						EntryHeader.CH_IsEntryCancelled = true;
					}
				}

				if (declaration.IsTSWICRWriteOff)
				{
					TranshipmentRequest decITR = EntryHeader.Declaration.TranshipmentRequest;
					if (decITR != null && (!decITR.C4_ModeOfMovement.IsEmpty || !decITR.C4_TranshipModeOfMovement.IsEmpty))
					{
						decITR.C4_Status = TSWStatus.CalculateCombinedMovementStatus();
					}
				}
			}
		}

		void UpdateBIOStatus(ZDateTime responseTime, ZString status)
		{
			if (EntryHeader.CH_MPIBioResponseTime.IsEmpty || responseTime >= EntryHeader.CH_MPIBioResponseTime)
			{
				EntryHeader.CH_MPIBioResponseTime = responseTime;
				EntryHeader.CH_MPIBioStatus = status;
			}

			if (EntryHeader.Declaration.IsTSWICRWriteOff)
			{
				if (!string.IsNullOrEmpty(response.GoodsMovementStatus))
				{
					if (EntryHeader.CH_MPIBioMovementStatusTime.IsEmpty || responseTime >= EntryHeader.CH_MPIBioMovementStatusTime)
					{
						EntryHeader.CH_MPIBioMovementStatus = new ZString(response.GoodsMovementStatus).Left(EntryHeader.CH_MPIBioMovementStatusInfo.MaxLength);
						EntryHeader.CH_MPIBioMovementStatusTime = responseTime;
					}
				}
			}
		}

		void UpdateNZCSStatus(ZDateTime responseTime, ZString status)
		{
			if (EntryHeader.CH_NZCSResponseTime.IsEmpty || responseTime >= EntryHeader.CH_NZCSResponseTime)
			{
				EntryHeader.CH_NZCSResponseTime = responseTime;
				EntryHeader.CH_NZCSStatus = status;
			}

			if (!string.IsNullOrEmpty(response.GoodsMovementStatus))
			{
				if (EntryHeader.CH_NZCSMovementStatusTime.IsEmpty || responseTime >= EntryHeader.CH_NZCSMovementStatusTime)
				{
					EntryHeader.CH_NZCSMovementStatus = new ZString(response.GoodsMovementStatus).Left(EntryHeader.CH_NZCSMovementStatusInfo.MaxLength);
					EntryHeader.CH_NZCSMovementStatusTime = responseTime;
				}
			}
		}

		TSWStatus TSWStatus
		{
			get { return new TSWStatus(this); }
		}

		#region IWriteOffStatus Members

		ZString IWriteOffStatus.CustomsStatus => response.Status;

		ZString IWriteOffStatus.GoodsClearanceStatus => response.GoodsClearanceStatus;

		ZString IWriteOffStatus.CombinedStatus => EntryHeader.Declaration.JE_TSWCombinedStatus;

		Declaration.JobDeclaration ITSWStatus.Declaration => EntryHeader.Declaration;

		ZString ITSWStatus.ResponseStatus => ZString.Empty;

		ZString ITSWStatus.EnterpriseStatus => EntryHeader.CH_EntryStatus;

		ZString ITSWStatus.Agency => response.ResponsibleGovernmentAgency;

		#endregion

		protected override void SetECINumber(ZString newValue)
		{
			EntryHeader.EntryNumber = newValue;
		}

		#endregion Overrides
	}
}

// Tested in CREMessageProcessorDeclarationTest
// Tested in ICRMessageProcessorDeclarationTest
