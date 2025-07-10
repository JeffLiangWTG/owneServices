using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class TSWStatus
	{
		public TSWStatus(ITSWStatus declarationEntity)
		{
			this.declarationEntity = declarationEntity;
			declaration = this.declarationEntity.Declaration;
			agency = this.declarationEntity.Agency;
		}

		public TSWStatus(IWriteOffStatus businessEntity)
		{
			this.businessEntity = businessEntity;
			declaration = businessEntity.Declaration;
			entryHeader = businessEntity.EntryHeader;
			agency = businessEntity.Agency;
		}

		public TSWStatus(IMovementStatus businessEntity)
		{
			agency = businessEntity.Agency;
		}

		readonly ITSWStatus declarationEntity;
		readonly JobDeclaration declaration;
		readonly CusEntryHeader entryHeader;
		readonly string agency;
		readonly IWriteOffStatus businessEntity;

		#region Code Constants

		public static class StatusCodes
		{
			public const string Cleared = "0";
			public const string Inspection = "1";
			public const string AdjustmentAccepted = "2";
			public const string CreditAdvice = "3";
			public const string ResponseReceipt = "4";
			public const string Cancelled = "5";
			public const string PendingPayment = "6";
			public const string Error = "7";
			public const string ResponsePending = "9";
			public const string NZCSStatusNotRelevant = "N";
		}

		public static class WriteOffStatusCodes
		{
			public const string WrittenOffCleared = "0";
			public const string Held = "1";
			public const string IDR = "2";
			public const string MDR = "3";
			public const string CDR = "4";
			public const string Rescind = "5";
			public const string Error = "7";
			public const string UnknownStatus = "8";
			public const string ResponsePending = "9";
			public const string ITAApproved = "A";
			public const string DTAApproved = "D";
			public const string TranshipmentDeclined = "T";
		}

		public static class MovementStatusCodes
		{
			public const string Held = "1";
			public const string ITRApproved = "2";
			public const string DTRApproved = "3";
			public const string Rescind = "5";
			public const string ITRDeclined = "6";
			public const string DTRDeclined = "7";
			public const string UnknownStatus = "8";
			public const string ResponsePending = "9";
		}

		#endregion

		public ZString GetCombinedStatus
		{
			get
			{
				var newStatus = ZString.Empty;

				if (declarationEntity.ResponseStatus == StatusList.Codes.EntryRestored)
				{
					newStatus = TSWEntryStatusList.Codes.RES;
				}
				else if (IsCancellationEntry)
				{
					newStatus = GetCancelledStatus;
				}
				else if (declaration.IsExport)
				{
					newStatus = declarationEntity.EnterpriseStatus;
				}
				else if (declaration.IsPrimaryIndustriesImportDeclaration)
				{
					newStatus = GetIPICombinedStatus;
				}
				else
				{
					newStatus = GetImportStatus;
				}

				return newStatus;
			}
		}

		public ZString CalculateEntryStatus
		{
			get
			{
				var result = ZString.Empty;
				var combinedStatus = GetCombinedStatus;

				if ((IsMPIBiosecurityResponse || IsMPIFoodResponse) && (declaration.JE_EntryStatus == FormalEntryStatusList.Codes.DeliveryOrderReceived || declaration.JE_EntryStatus == FormalEntryStatusList.Codes.DOSentToRecipient))
				{
					result = declaration.JE_EntryStatus;    // NZCS have now advised we should allow DO to be shown regardless of other agency statuses - so we will not override with other status. Also, once a Job has a DeliveryOrder status we will never override.
				}
				else if (HasDeliveryOrderResponse)
				{
					result = FormalEntryStatusList.Codes.DeliveryOrderReceived;
				}
				else if (DeliveryOrderSentToRecipient)
				{
					result = FormalEntryStatusList.Codes.DOSentToRecipient;
				}
				else if (declaration.IsExport || declaration.IsPrimaryIndustriesImportDeclaration)
				{
					result = CalculateEntryStatusFromCombinedStatus(combinedStatus);
				}
				else
				{
					if (declaration.IsNZCSEntryRestored)
					{
						result = FormalEntryStatusList.Codes.EntryRestored;
					}
					else if (combinedStatus.StartsWith("DC"))
					{
						if (combinedStatus == TSWEntryStatusList.Codes.DCC)
						{
							result = FormalEntryStatusList.Codes.EntryCancelled;
						}
						else if (combinedStatus == TSWEntryStatusList.Codes.DCI)
						{
							result = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
						}
						else if (combinedStatus == TSWEntryStatusList.Codes.DCE)
						{
							result = FormalEntryStatusList.Codes.EntryInError;
						}
						else
						{
							result = FormalEntryStatusList.Codes.ResponseReceived;
						}
					}
					else
					{
						result = CalculateEntryStatusFromCombinedStatus(combinedStatus);
					}
				}

				return result;
			}
		}

		public ZString GetCombinedWriteOffStatus
		{
			get { return GetICRWriteOffStatus; }
		}

		public ZString GetWriteOffEntryStatus
		{
			get { return GetWriteOffJobStatus; }
		}

		#region Transhipment Movement Status

		public ZString CalculateCombinedMovementStatus()
		{
			return CombinedMovementStatus(entryHeader.CH_NZCSMovementStatus, entryHeader.CH_MPIBioMovementStatus);
		}

		public ZString CalculateCombinedMovementStatus(CusHAWB houseBill)
		{
			return CombinedMovementStatus(houseBill.CS_CustomsMovementStatus, houseBill.CS_BioMovementStatus);
		}

		public ZString CalculateCombinedMovementStatus(CusSCAHouse houseBill)
		{
			return CombinedMovementStatus(houseBill.CA_CustomsMovementStatus, houseBill.CA_BioMovementStatus);
		}

		public ZString CalculateCombinedMovementStatus(TranshipmentRequest underbondMovement)
		{
			if (underbondMovement.C4_Status.Length > 2)
			{
				return underbondMovement.C4_Status;
			}

			var customsStatus = underbondMovement.C4_Status.SubstringSafe(0, 1);
			var bioStatus = underbondMovement.C4_Status.SubstringSafe(1, 1);
			return CombinedMovementStatus(customsStatus, bioStatus);
		}

		public ZString CalculateCombinedMovementStatus(ZString customsMovementStatus, ZString bioMovementStatus)
		{
			return CombinedMovementStatusFromIndividualStatus(customsMovementStatus, bioMovementStatus);
		}

		ZString CombinedMovementStatus(ZString customsMovementStatus, ZString bioMovementStatus)
		{
			var nzcsMovementStatusCode = customsMovementStatus.IsEmpty ? "9" : ConvertMovementCode(customsMovementStatus);
			var mpiBioMovementStatusCode = bioMovementStatus.IsEmpty ? "9" : ConvertMovementCode(bioMovementStatus);
			return nzcsMovementStatusCode + mpiBioMovementStatusCode;
		}

		ZString CombinedMovementStatusFromIndividualStatus(ZString customsMovementStatus, ZString bioMovementStatus)
		{
			var nzcsMovementStatusCode = customsMovementStatus.IsEmpty ? "9" : (string)customsMovementStatus;
			if (nzcsMovementStatusCode.Length > 2)
			{
				nzcsMovementStatusCode = ConvertMovementCode(customsMovementStatus);
			}

			var mpiBioMovementStatusCode = bioMovementStatus.IsEmpty ? "9" : (string)bioMovementStatus;
			if (mpiBioMovementStatusCode.Length > 2)
			{
				mpiBioMovementStatusCode = ConvertMovementCode(bioMovementStatus);
			}

			return nzcsMovementStatusCode + mpiBioMovementStatusCode;
		}

		#endregion

		#region Implementation

		ZString GetCancelledStatus
		{
			get
			{
				var cancelStatus = ZString.Empty;
				if (declarationEntity.ResponseStatus == StatusList.Codes.EntryCancelled)
				{
					cancelStatus = declaration.IsExport ? TSWStatus.StatusCodes.Cancelled : TSWEntryStatusList.Codes.DCC;
				}
				else
				{
					switch (declarationEntity.EnterpriseStatus)
					{
						case StatusCodes.Inspection:
							cancelStatus = TSWEntryStatusList.Codes.DCI;
							break;
						case StatusCodes.Error:
							cancelStatus = TSWEntryStatusList.Codes.DCE;
							break;
						default:
							cancelStatus = TSWEntryStatusList.Codes.DCA;
							break;
					}
				}

				return cancelStatus;
			}
		}

		ZString GetIPICombinedStatus
		{
			get
			{
				ZString currentStatus = (declaration.JE_TSWCombinedStatus.IsEmpty || declaration.JE_TSWCombinedStatus == TSWEntryStatusList.Codes.STC) ? "N99" : declaration.JE_TSWCombinedStatus.ToString();
				if (currentStatus == "N00" && declarationEntity.EnterpriseStatus == "0")
				{
					return currentStatus;
				}
				else
				{
					var mpiFoodStatus = currentStatus.SubstringSafe(2, 1);
					var mpiBioStatus = currentStatus.SubstringSafe(1, 1);

					if (IsMPIFoodResponse)
					{
						mpiFoodStatus = declarationEntity.EnterpriseStatus;
					}
					else if (IsMPIBiosecurityResponse)
					{
						mpiBioStatus = declarationEntity.EnterpriseStatus;
					}

					return StatusCodes.NZCSStatusNotRelevant + mpiBioStatus + mpiFoodStatus;
				}
			}
		}

		ZString GetImportStatus
		{
			get
			{
				ZString currentStatus = (declaration.JE_TSWCombinedStatus.IsEmpty || declaration.JE_TSWCombinedStatus == TSWEntryStatusList.Codes.STC) ? "999" : declaration.JE_TSWCombinedStatus.ToString();
				if (currentStatus == "000" && declarationEntity.EnterpriseStatus == "0")
				{
					return currentStatus;
				}
				else
				{
					var mpiFoodStatus = currentStatus.SubstringSafe(2, 1);
					var mpiBioStatus = currentStatus.SubstringSafe(1, 1);
					var nzcsStatus = currentStatus.SubstringSafe(0, 1);

					if (IsMPIFoodResponse)
					{
						mpiFoodStatus = declarationEntity.EnterpriseStatus;
					}
					else if (IsMPIBiosecurityResponse)
					{
						mpiBioStatus = declarationEntity.EnterpriseStatus;
					}
					else
					{
						nzcsStatus = declarationEntity.EnterpriseStatus;
					}

					return nzcsStatus + mpiBioStatus + mpiFoodStatus;
				}
			}
		}

		ZString CalculateEntryStatusFromCombinedStatus(string combinedStatus)
		{
			var result = ZString.Empty;
			if (combinedStatus.Contains(StatusCodes.Error))
			{
				result = FormalEntryStatusList.Codes.EntryInError;
			}
			else if (combinedStatus.Contains(StatusCodes.Inspection))
			{
				result = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			}
			else if (combinedStatus.Contains(StatusCodes.ResponsePending))
			{
				result = FormalEntryStatusList.Codes.AgencyResponsePending;
			}
			else if (combinedStatus.Contains(StatusCodes.AdjustmentAccepted))
			{
				result = FormalEntryStatusList.Codes.AdjustmentAccepted;
			}
			else if (combinedStatus.Contains(StatusCodes.CreditAdvice))
			{
				result = FormalEntryStatusList.Codes.CreditAdvice;
			}
			else if (combinedStatus.Contains(StatusCodes.ResponseReceipt))
			{
				result = FormalEntryStatusList.Codes.ResponseReceived;
			}
			else if (combinedStatus.Contains(StatusCodes.PendingPayment) || declaration.IsNZCSDeliveryOnPayment)
			{
				result = FormalEntryStatusList.Codes.DeliveryOnPayment;
			}
			else if (combinedStatus.Contains(StatusCodes.Cancelled) || combinedStatus == TSWEntryStatusList.Codes.DCC)
			{
				result = FormalEntryStatusList.Codes.EntryCancelled;
			}
			else if (combinedStatus.Contains(StatusCodes.Cleared))
			{
				result = FormalEntryStatusList.Codes.EntryCleared;
				if (declaration.JE_TSWCombinedStatus == TSWEntryStatusList.Codes.CCC ||
					declaration.JE_TSWCombinedStatus == TSWEntryStatusList.Codes.CC ||
					declaration.JE_TSWCombinedStatus == TSWEntryStatusList.Codes.CLR)
				{
					if (HasDeliveryOrderResponse)
					{
						result = FormalEntryStatusList.Codes.DeliveryOrderReceived;
					}
					else if (DeliveryOrderSentToRecipient)
					{
						result = FormalEntryStatusList.Codes.DOSentToRecipient;
					}
				}
			}

			return result;
		}

		bool HasDeliveryOrderResponse => StatusList.IsDeliveryOrderHerewithMethodOfPayment(declaration.CusEntryHeader.CH_NZCSStatus);

		bool DeliveryOrderSentToRecipient => StatusList.IsDeliveryOrderSentToRecipient(declaration.CusEntryHeader.CH_NZCSStatus);

		bool IsMPIFoodResponse
		{
			get { return agency == ResponsibleGovernmentAgencyList.Codes.MPIFOOD; }
		}

		bool IsMPIBiosecurityResponse
		{
			get { return agency == ResponsibleGovernmentAgencyList.Codes.MPIBIO; }
		}

		bool IsCancellationEntry
		{
			get { return declaration.IsTSWCancellation || declarationEntity.ResponseStatus == StatusList.Codes.EntryCancelled; }
		}

		#region Write-Off Status

		ZString GetICRWriteOffStatus
		{
			get
			{
				ZString currentCombinedStatus = ZString.Empty;
				if (businessEntity.CustomsStatus == StatusList.Codes.EntryCancelled)
				{
					return LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
				}
				else if (businessEntity.CustomsStatus == LowValueConsignmentStatusList.Codes.ConsignmentRejected || businessEntity.CustomsStatus == StatusList.Codes.EntryRejected)
				{
					return LowValueConsignmentStatusList.Codes.ConsignmentRejected;
				}

				if (declaration == null)
				{
					currentCombinedStatus = businessEntity.CustomsStatus.IsEmpty || businessEntity.CustomsStatus == LowValueConsignmentStatusList.Codes.SentToCustoms ? LowValueConsignmentStatusList.Codes.PP : businessEntity.CombinedStatus.ToString();
				}
				else
				{
					currentCombinedStatus = (declaration.JE_TSWCombinedStatus.IsEmpty || declaration.JE_TSWCombinedStatus == LowValueConsignmentStatusList.Codes.SentToCustoms) ? LowValueConsignmentStatusList.Codes.PP : declaration.JE_TSWCombinedStatus.ToString();
				}

				var customsStatus = currentCombinedStatus.SubstringSafe(0, 1);
				var bioStatus = currentCombinedStatus.SubstringSafe(1, 1);
				if (currentCombinedStatus == LowValueConsignmentStatusList.Codes.CC && businessEntity.EnterpriseStatus == LowValueConsignmentStatusList.Codes.Cleared)
				{
					return currentCombinedStatus;
				}

				var agencyStatus = IsImportManifest ? IsMPIBiosecurityResponse ? declaration.JE_ManifestBioStatus : declaration.JE_ManifestNZCSStatus : businessEntity.GoodsClearanceStatus;
				var agencyStatusCode = ConvertStatusCodeTo1CharInd(agencyStatus);
				if (IsMPIBiosecurityResponse)
				{
					bioStatus = agencyStatusCode;
				}
				else
				{
					customsStatus = agencyStatusCode;
				}

				return customsStatus + bioStatus;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "case statement")]
		string ConvertStatusCodeTo1CharInd(string agencyStatus)
		{
			string statusCode = WriteOffStatusCodes.UnknownStatus;
			switch (agencyStatus)
			{
				case LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff:
					statusCode = WriteOffStatusCodes.WrittenOffCleared;
					break;
				case LowValueConsignmentStatusList.Codes.ConsignmentHeld:
					statusCode = WriteOffStatusCodes.Held;
					break;
				case LowValueConsignmentStatusList.Codes.ConsignmentInError:
					statusCode = WriteOffStatusCodes.Error;
					break;
				case LowValueConsignmentStatusList.Codes.ImportDeclarationRequired:
					statusCode = WriteOffStatusCodes.IDR;
					break;
				case LowValueConsignmentStatusList.Codes.MpiImportDecRequired:
					statusCode = WriteOffStatusCodes.MDR;
					break;
				case LowValueConsignmentStatusList.Codes.RescindPreviousStatusNotification:
					statusCode = WriteOffStatusCodes.Rescind;
					break;
				case LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved:
					statusCode = WriteOffStatusCodes.ITAApproved;
					break;
				case LowValueConsignmentStatusList.Codes.DomesticTranshipmentApproved:
					statusCode = WriteOffStatusCodes.DTAApproved;
					break;
				case LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined:
				case LowValueConsignmentStatusList.Codes.DomesticTranshipmentDeclined:
					statusCode = WriteOffStatusCodes.TranshipmentDeclined;
					break;
				case LowValueConsignmentStatusList.Codes.ConsolidationIcrRequired:
					statusCode = WriteOffStatusCodes.CDR;
					break;
				case LowValueConsignmentStatusList.Codes.AgencyResponsePending:
					statusCode = WriteOffStatusCodes.ResponsePending;
					break;
			}

			return statusCode;
		}

		bool IsImportManifest => declaration != null && declaration.IsTSWICRWriteOff && declaration.IsECIManifestDeclarationReference;

		ZString GetWriteOffJobStatus => entryHeader.Declaration.CalculateCombinedJobStatus(entryHeader.CH_MPIBioStatus, entryHeader.CH_NZCSStatus);

		#endregion

		#region Movement Status

		string ConvertMovementCode(string movementStatus)
		{
			string statusCode = MovementStatusCodes.UnknownStatus;
			switch (movementStatus)
			{
				case LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved:
					statusCode = MovementStatusCodes.ITRApproved;
					break;
				case LowValueConsignmentStatusList.Codes.DomesticTranshipmentApproved:
					statusCode = MovementStatusCodes.DTRApproved;
					break;
				case LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined:
					statusCode = MovementStatusCodes.ITRDeclined;
					break;
				case LowValueConsignmentStatusList.Codes.DomesticTranshipmentDeclined:
					statusCode = MovementStatusCodes.DTRDeclined;
					break;
				case LowValueConsignmentStatusList.Codes.RescindPreviousStatusNotification:
					statusCode = MovementStatusCodes.Rescind;
					break;
				case LowValueConsignmentStatusList.Codes.ConsignmentHeld:
					statusCode = MovementStatusCodes.Held;
					break;
			}

			return statusCode;
		}

		#endregion

		#endregion
	}
}
