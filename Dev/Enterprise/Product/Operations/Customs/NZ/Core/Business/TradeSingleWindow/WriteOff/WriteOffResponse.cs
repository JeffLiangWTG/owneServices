using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public abstract class WriteOffResponse : CargoReportResponse
	{
		protected WriteOffResponse(BaseTSWResponse response)
			: base(response)
		{
		}

		internal IEnumerable<Consignment> AllConsignments
		{
			get { return ConsignmentsWithResponse.Concat(ConsignmentsWithoutResponse); }
		}

		internal IEnumerable<Consignment> ConsignmentsWithoutResponse
		{
			get
			{
				if (consignmentsWithoutResponse == null)
				{
					var defaultStatus = ErrorCodes.IsNullOrEmpty() ? "" : ConsignmentGoodsStatusList.Codes.Error;
					consignmentsWithoutResponse =
						ConsignmentIDs.Where(id => !ConsignmentsWithResponse.Any(consignment => consignment.ID == id))
									  .Select(id => CreateConsignment(id, defaultStatus, "", 0))
									  .ToList();
				}

				return consignmentsWithoutResponse;
			}
		}

		internal IEnumerable<Consignment> ConsignmentsWithResponse
		{
			get
			{
				if (consignmentsWithResponse == null)
				{
					consignmentsWithResponse = new List<Consignment>();
					foreach (var consignment in GetElements("p:OverallDeclaration/p:Declaration/p:Consignment"))
					{
						var id = GetElementValue("p:TransportContractDocument/p:ID", consignment);
						if (!string.IsNullOrEmpty(id))
						{
							var sequenceValue = GetElementValue("p:SequenceNumeric", consignment);
							var msgSequence = ZInt.ParseSafe(sequenceValue, 0);
							ZString clearanceStatus = GetElementAttributeValue("p:GoodsStatusCode", "StatusType", "CLEARANCE", consignment);
							if (clearanceStatus.IsEmpty)
							{
								clearanceStatus = GetElementValue("p:GoodsStatusCode", consignment);
							}

							ZString movementStatus = GetElementAttributeValue("p:GoodsStatusCode", "StatusType", "MOVEMENT", consignment);
							consignmentsWithResponse.Add(CreateConsignment(id, clearanceStatus, movementStatus, msgSequence));
						}
					}
				}

				return consignmentsWithResponse;
			}
		}

		internal IEnumerable<Consignment> ConsignmentsWrittenOff
		{
			get { return ConsignmentsWithResponse.Where(consignment => consignment.IsWrittenOff); }
		}

		public ZString CustomsDeliveryInstructions
		{
			get { return CustomsDeliveryInstructionsCore; }
			set { CustomsDeliveryInstructionsCore = value; }
		}

		public ZString MasterBill
		{
			get { return MasterBillCore; }
		}

		public LowValueConsignmentStatusList LowValueConsignmentStatusList
		{
			get { return Factory.GetCachedValue<LowValueConsignmentStatusList>(); }
		}

		public void UpdateCustomsStatus()
		{
			SetCustomsStatus(EnterpriseStatus);
			if (EnterpriseStatus == LowValueConsignmentStatusList.Codes.ConsignmentInError || EnterpriseStatus == LowValueConsignmentStatusList.Codes.ConsignmentRejected)
			{
				foreach (var consignment in AllConsignments)
				{
					if (consignment != null)
					{
						if (EntryRejectedByTSWFrontEnd || (ConsignmentSentToCustoms(consignment)))
						{
							consignment.MessageStatus = LowValueConsignmentStatusList.Codes.Acknowledgement;
							consignment.CustomsStatus = EnterpriseStatus;
						}
					}
				}
			}
		}

		bool ConsignmentSentToCustoms(Consignment consignment)
		{
			return consignment.CustomsStatus == LowValueConsignmentStatusList.Codes.SentToCustoms ||
				   consignment.MessageStatus == LowValueConsignmentStatusList.Codes.SentToCustoms;
		}

		bool EntryRejectedByTSWFrontEnd => string.IsNullOrEmpty(DeclarationID) || DeclarationID == "00000000";

		public void UpdateECINumber()
		{
			SetECINumber(DeclarationID);
		}

		public virtual bool ShouldShowSummaryOnReport => true;

		#region Overrides

		protected override string GetEnterpriseStatus()
		{
			if (IsCancellation)
			{
				return LowValueManifestStatusList.Codes.ManifestCancelled;
			}
			else if (MessageType == TransactionTypeList.Codes.Error)
			{
				return LowValueManifestStatusList.Codes.ManifestRejected;
			}
			else if (MessageType == TransactionTypeList.Codes.Receipt)
			{
				return LowValueManifestStatusList.Codes.Acknowledgement;
			}
			else if (AllConsignments.Any(consignment => consignment != null && consignment.IsInError))
			{
				return LowValueManifestStatusList.Codes.ManifestInError;
			}
			else if ((MessageType == TransactionTypeList.Codes.Inspection) || (AllConsignments.Any(consignment => consignment != null && consignment.IsHeld)))
			{
				return LowValueManifestStatusList.Codes.InspectionsAuditRequirements;
			}
			else if ((AllConsignments.Any(consignment => consignment != null && (consignment.IsWrittenOff || consignment.IsFormalDeclarationRequired)))
					|| (MessageType == TransactionTypeList.Codes.ClearanceInstructions))
			{
				return LowValueManifestStatusList.Codes.ManifestAccepted;
			}
			else
			{
				return "";
			}
		}

		protected override string GetEnterpriseStatusDescription()
		{
			return LowValueManifestStatusList.GetDescriptionFromCode(EnterpriseStatus) ?? "Unknown";
		}

		protected override string GetStatusDescription(string code)
		{
			var result = base.GetStatusDescription(code);
			if (IsCancellation)
			{
				result += ". Entry is now CANCELLED.";
			}
			return result;
		}

		#endregion // Overrides

		#region Implementation

		List<Consignment> consignmentsWithResponse;
		List<Consignment> consignmentsWithoutResponse;

		LowValueManifestStatusList LowValueManifestStatusList
		{
			get { return Factory.GetCachedValue<LowValueManifestStatusList>(); }
		}

		protected abstract IEnumerable<ZString> ConsignmentIDs { get; }

		protected abstract ZString CustomsDeliveryInstructionsCore { get; set; }

		protected abstract ZString MasterBillCore { get; }

		protected abstract Consignment CreateConsignment(string id, string status, string movementStatus, int msgSequence);

		protected abstract void SetCustomsStatus(ZString newValue);

		protected abstract void SetECINumber(ZString newValue);

		#endregion // Implementation
	}
}

// Tested in
// - CREMessageProcessorMAWBTest
// - CREMessageProcessorDeclarationTest
// - CREMessageProcessorManifestingTest
// - ICRMessageProcessorConsolTest
