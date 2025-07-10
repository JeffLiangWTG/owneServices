using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	public class FTZAdmissionProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			emailReportThatHasBeenDelayed = null;
			var html = new StringBuilder();
			var emailReport = new HtmlTableCreator(new string[] { "Column", "Description" });
			HtmlTableCreator dispositionsTable = null;

			HtmlTableCreator lineErrors = null;
			HtmlTableCreator itNumberErrors = null;
			HtmlTableCreator irsErrors = null;
			HtmlTableCreator headerErrors = null;
			HtmlTableCreator nf96Table = null;
			var isFailed = messageBlocks.OfType<FTZNF95>().Any(x => ExtractFTZResponseResult.IsFailure(x.NarrativeMessageTypeCode));
			var isPaperless = false;
			var isUnauthorized = false;

			var billNumber = ZString.Empty;
			var itNumber = ZString.Empty;
			var irsNumber = ZString.Empty;
			var tariff = ZString.Empty;
			var isCommonError = false;
			var isBillLevelError = false;
			var isStatusUpdateMessage = true;
			var authorisationDate = ZDateTime.Empty;
			var isNF91ForBillOfLading = false;

			var declaration = OriginalMessageLinker.Link<JobDeclaration>(Message);
			var originalMessage = Message.OriginalMessage;

			foreach (MessageBlock block in messageBlocks)
			{
				if (block is IFTZNF90 nf90)
				{
					emailReport.WriteRow("Admission Type", Factory.GetCachedValue<FTZAdmissionTypeCodeList>().GetDescriptionFromCode(nf90.AdmissionType));
					emailReport.WriteRow("Zone ID", nf90.ZoneID);
					emailReport.WriteRow("Control Number", nf90.ControlNumber);
					emailReport.WriteRow("Year", nf90.CalendarYear);
					emailReport.WriteRow("Port", nf90.PortCode + " " + GetPortName(nf90.PortCode));
					emailReport.WriteRow("Direct Delivery", nf90.DirectDeliveryIndicator);
				}
				else if (block is FTZNF91)
				{
					if (dispositionsTable == null)
					{
						dispositionsTable = new HtmlTableCreator(new string[] { "Disposition Code", "Disposition Description", "Reference", "Action Date and Time", "Carrier Code", "Location of Goods", "Container No" });
					}

					var nf91 = (FTZNF91)block;
					var dispositionCode = nf91.DispositionCode;

					var actionTime = ZDateTime.Empty;
					var actionDate = nf91.ActionDate;
					if (actionDate.IsValid)
					{
						actionTime = new ZDateTime(actionDate.Year, actionDate.Month, actionDate.Day, new ZInt(nf91.ActionTime.SubstringSafe(0, 2)), new ZInt(nf91.ActionTime.SubstringSafe(2, 2)), 0);
					}
					dispositionsTable.WriteRow(dispositionCode, DispositionList.GetDescriptionFromCode(dispositionCode), GetReference(nf91.ReferenceQualifier), actionTime, nf91.InbondCarrierCode, nf91.FIRMSID, nf91.ContainerNumber);

					if (declaration != null && !dispositionCode.IsEmpty && DispositionList.IsNotableFTZDispositionCode(dispositionCode))
					{
						declaration.FTZDispositionCodes.AddNewIfNotExist(dispositionCode, actionTime);
					}

					if ((!authorisationDate.IsValid || actionTime > authorisationDate) && dispositionCode == DispositionList.Codes.BF)
					{
						authorisationDate = actionTime;
					}

					isNF91ForBillOfLading = nf91.ReferenceQualifier == BillNumberRefQualifer;
				}
				else if (block is IFTZNF10 || block is FTZNF20)
				{
					isCommonError = true;
				}
				else if (block is IFTZNF40 nf40)
				{
					billNumber = nf40.BillOfLadingOrAirWaybill;
					isCommonError = false;
					isBillLevelError = true;
				}
				else if (block is FTZFT41)
				{
					var block41 = (FTZFT41)block;
					itNumber = block41.ITNumber;
					irsNumber = ZString.Empty;
					tariff = ZString.Empty;
					isBillLevelError = false;
					isCommonError = false;
				}
				else if (block is FTZFT42)
				{
					var block42 = (FTZFT42)block;
					irsNumber = block42.IRSIdentifierBondedCarrier;
					itNumber = ZString.Empty;
					tariff = ZString.Empty;
					isCommonError = false;
					isBillLevelError = false;
				}
				else if (block is FTZFT50)
				{
					var block50 = (FTZFT50)block;
					tariff = block50.HarmonizedTariffScheduleNumber;
					itNumber = ZString.Empty;
					irsNumber = ZString.Empty;
					isBillLevelError = false;
					isCommonError = false;
				}
				else if (block is FTZNF92 nf92)
				{
					if (isNF91ForBillOfLading && nf92.ReferenceIdentifierQualifier == FTZPermitToTransferReferenceIdentifierList.Codes.PTTUniqueIdentifier && originalMessage != null && declaration != null)
					{
						var permitToTransferID = nf92.ReferenceIdentifier;
						var ftBillOfLadingBlocks = originalMessage.MessageBlock.MessageBlocks.OfType<IFTBillOfLading>();
						foreach (var ftBillOfLading in ftBillOfLadingBlocks)
						{
							var billOfLadingOrAirWayBill = ftBillOfLading.BillOfLadingOrAirWaybill;
							if (!billOfLadingOrAirWayBill.IsEmpty)
							{
								FindMatchedBillAndSetPID(billOfLadingOrAirWayBill, permitToTransferID);
							}

							var houseBill = ftBillOfLading.HouseBill;
							if (!houseBill.IsEmpty)
							{
								FindMatchedBillAndSetPID(houseBill, permitToTransferID);
							}
						}
					}
				}
				else if (block is FTZNF95)
				{
					var nf95 = (FTZNF95)block;
					var errorCode = nf95.ErrorCode.Length > 3 ? nf95.ErrorCode.SubstringSafe(2, 3) : nf95.ErrorCode;
					isPaperless |= ContainsPaperlessCode(nf95.Remarks);
					isUnauthorized |= ExtractFTZResponseResult.IsUnauthorized(errorCode);
					isStatusUpdateMessage &= errorCode.IsEmpty && nf95.NarrativeMessageTypeCode.IsEmpty;

					if (!billNumber.IsEmpty && !itNumber.IsEmpty)
					{
						if (itNumberErrors == null)
						{
							itNumberErrors = new HtmlTableCreator(new string[] { "Bill Number", "IT Number", "Notification" });
						}

						itNumberErrors.WriteRow(billNumber, itNumber, nf95.Remarks);
					}

					if (!billNumber.IsEmpty && !irsNumber.IsEmpty)
					{
						if (irsErrors == null)
						{
							irsErrors = new HtmlTableCreator(new string[] { "Bill Number", "IRS Identifier", "Notification" });
						}

						irsErrors.WriteRow(billNumber, irsNumber, nf95.Remarks);
					}

					if (!billNumber.IsEmpty && !tariff.IsEmpty)
					{
						if (lineErrors == null)
						{
							lineErrors = new HtmlTableCreator(new string[] { "Bill Number", "Line Tariff", "Notification" });
						}

						lineErrors.WriteRow(billNumber, tariff, nf95.Remarks);
					}

					if (!isCommonError && !messageBlocks.Exists(x => x is IFTZNF40 || x is FTZFT41 || x is FTZFT42 || x is FTZFT50))
					{
						isCommonError = true;
					}

					if (isCommonError || !isFailed || isBillLevelError || isUnauthorized)
					{
						if (headerErrors == null)
						{
							headerErrors = new HtmlTableCreator(new string[] { "Notification Code", "Description" });
						}

						headerErrors.WriteRow(errorCode, nf95.Remarks);
					}
				}
				else if (block is FTZNF96)
				{
					var nf96 = (FTZNF96)block;
					if (nf96Table == null)
					{
						nf96Table = new HtmlTableCreator(new string[] { "Carrier Code", "Flight No", "Arrival Date" });
					}
					nf96Table.WriteRow(nf96.CarrierCode, nf96.FlightNumber, nf96.ArrivalDate);
				}
			}

			if (!isUnauthorized)
			{
				html.Append(emailReport.ToHtml());
				html.Append("<br>");
			}

			if (dispositionsTable != null)
			{
				html.Append(dispositionsTable.ToHtml());
				html.Append("<br>");
			}

			if (headerErrors != null)
			{
				html.Append(headerErrors.ToHtml());
				html.Append("<br>");
			}

			if (itNumberErrors != null)
			{
				html.Append(itNumberErrors.ToHtml());
				html.Append("<br>");
			}

			if (irsErrors != null)
			{
				html.Append(irsErrors.ToHtml());
				html.Append("<br>");
			}

			if (lineErrors != null)
			{
				html.Append(lineErrors.ToHtml());
				html.Append("<br>");
			}

			if (nf96Table != null)
			{
				html.Append(nf96Table.ToHtml());
				html.Append("<br>");
			}

			var jobNumber = "Unknown";
			if (declaration != null)
			{
				if (!isStatusUpdateMessage)
				{
					var isAcceptedWithWarnings = Message.IsFTZAcceptedWithCensusWarning;
					var iFTZHeader = (IFTZCommonHeader)declaration;
					iFTZHeader.SetMessageStatus(originalMessage?.EM_MessageSubType ?? ZString.Empty, new FTZMessageStatusCalculator().Calculate(Message, isFailed, isAcceptedWithWarnings));
				}
				else
				{
					Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZStatusUpdate;
				}

				jobNumber = declaration.JE_DeclarationReference;

				if (isPaperless)
				{
					if (!declaration.IsReconMessageType)
					{
						declaration.US_PaperlessEntry = YesNoDefaultList.Codes.Yes;
					}
				}

				if (Message.IsFTZCleared
					&& authorisationDate.IsValid
					&& (!declaration.JE_EntryAuthorisationDate.IsValid || authorisationDate > declaration.JE_EntryAuthorisationDate)
					)
				{
					declaration.JE_EntryAuthorisationDate = authorisationDate;
				}
			}

			var uri = declaration == null ? "" : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.JobDeclaration, declaration.PK.ToGuid());
			var branch = declaration != null ? declaration.Branch : null;

			var isAdmissionDataOutgoingMsgType = originalMessage != null && originalMessage.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			var msgTypeInSubject = isAdmissionDataOutgoingMsgType ? "FTZ Admission" : "FZ Event Reporting";
			if (declaration != null && declaration.SupportsBondedWarehousing && IsWhsMessagingResponse() && ShouldUpdateInward(declaration))
			{
				Message.Factory.Saved -= Factory_Saved;
				Message.Factory.Saved += Factory_Saved;
				new HtmlResponseEmailGenerator().TryGenerateEmail(uri, jobNumber, msgTypeInSubject, html.ToString() + EmailDefBuilder.HtmlTemplates.DynamicHtml1 + EmailDefBuilder.HtmlTemplates.DynamicHtml2 + EmailDefBuilder.HtmlTemplates.DynamicHtml3, isFailed, out emailReportThatHasBeenDelayed, branch);
			}
			else
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, msgTypeInSubject, html.ToString(), isFailed, branch, declaration);
			}

			void FindMatchedBillAndSetPID(ZString billOfLading, ZString pID)
			{
				if (declaration != null && declaration.Bills.OfType<Bill>().FirstOrDefault(x => x.EffectiveBillNumber == billOfLading) is Bill matchedBill)
				{
					matchedBill.USB_PermitToTransferID = pID;
				}
			}
		}

		#region Inventory Management
		bool IsWhsMessagingResponse()
		{
			switch (Message.EM_MessageSubType)
			{
				case EM_MessageSubTypeList.Codes.FTZAdmissionAdd:
				case EM_MessageSubTypeList.Codes.FTZAdmissionReplace:
				case EM_MessageSubTypeList.Codes.FTZAdmissionDelete:
					return true;
				default:
					return false;
			}
		}

		bool ShouldUpdateInward(JobDeclaration declaration)
		{
			var whsStatus = declaration.WarehouseTransactionStatus;
			return WarehouseTransactionStatusList.IsPendingInward(whsStatus);
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= Factory_Saved;
			}
			savedSuccessfully &= Message.EM_LinkedObject is JobDeclaration declaration &&
				!declaration.WarehouseTransactionStatus.IsEmpty;
			new BondedWarehouseFTZMessageProcessor(Message.PK, emailReportThatHasBeenDelayed, SendEmailToOriginalSenderOrGroupIfSenderInvalid).ProcessAfterSaved(savedSuccessfully);
		}

		void SendEmailToOriginalSenderOrGroupIfSenderInvalid(JobDeclaration declaration, EmailDef email, bool isFailure)
		{
			SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, declaration.Branch, isFailure);
			if (email == emailReportThatHasBeenDelayed)
			{
				emailReportThatHasBeenDelayed = null;
			}
		}
		EmailDef emailReportThatHasBeenDelayed;
		#endregion

		bool ContainsPaperlessCode(ZString remarks)
		{
			return remarks.Contains(Paperless);
		}
		const string Paperless = "PAPERLESS";

		string GetPortName(ZString portCode)
		{
			string districtPortCode = portCode.PadLeft(4, '0');

			var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, portCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			return port != null ? port.ZZD_Description : districtPortCode;
		}

		ZString GetReference(ZString refQualifer)
		{
			switch (refQualifer)
			{
				case "1":
					return "FTZ Admission Number";
				case BillNumberRefQualifer:
					return "Bill Number";
				case "3":
					return "IT Number";
				case "4":
					return "Container Number";
				default:
					return ZString.Empty;
			}
		}

		const string BillNumberRefQualifer = "2";

		DispositionList DispositionList
		{
			get { return Factory.GetCachedValue<DispositionList>(); }
		}
	}
}
