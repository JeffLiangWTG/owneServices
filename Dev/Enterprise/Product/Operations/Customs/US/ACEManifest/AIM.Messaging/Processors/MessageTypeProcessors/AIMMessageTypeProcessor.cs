using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMMessageTypeProcessor
	{
		bool Process(AIMEDIMessage message);
	}

	public abstract class AIMMessageTypeProcessor<T> : IAIMMessageTypeProcessor
		where T : AIMInboundMessage
	{
		protected AIMMessageTypeProcessor(LoggingInformation logger)
		{
			Logger = logger;
		}
		protected readonly LoggingInformation Logger;

		#region Process

		public bool Process(AIMEDIMessage aimMessage)
		{
			var succeeded = false;

			if (aimMessage is T message)
			{
				var bill = FindBill(message);
				var manifest = bill?.Header;
				if (manifest != null)
				{
					var branch = manifest.AMA_GB;
					var mostRecentSentMessage = FindMostRecentSentMessage(manifest.Messages, message, manifest, bill);
					if (mostRecentSentMessage != null && mostRecentSentMessage.EM_GB.IsValid)
					{
						branch = mostRecentSentMessage.EM_GB;
					}

					using (DisposableEnvironment.ForBranch(branch.ToGuid()))
					{
						message.EM_GB = branch;
						message.EM_ApplicationReference = bill.ABL_BillNumber;
						SetLinkedObject(message, mostRecentSentMessage, manifest, bill);

						succeeded = ProcessMessageCore(message, manifest, bill, mostRecentSentMessage);

						var email = CreateEmail(message, bill);
						SendEmail(mostRecentSentMessage, email, manifest);
					}
				}
				else
				{
					var msg = $"A Manifest record was not found for MasterBill = {message.MAWBNumberFormatted}, HouseBill = {message.HAWBNumber}.";
					message.Notes.AddNew(true, "AIM Message Processing", msg);
					Logger.LogError(msg);
				}
			}

			return succeeded;
		}

		protected abstract bool ProcessMessageCore(T message, AsycudaManifestHeader manifest, AsycudaBill bill, EDIMessage mostRecentSentMessage);

		#endregion

		#region FindBill

		AsycudaBill FindBill(T message)
		{
			AsycudaBill bill = null;

			var headerSubQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaBillSchema.ABL_AMA);
			headerSubQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.UsaAndTerritoriesList);
			headerSubQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_TransportMode, Core.Constants.TransportModes.Air);
			headerSubQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcToday.AddYears(-1));

			var bills = FindBillsByHAWBNumber(message, headerSubQuery);

			if (bills == null || (bills.Length == 0))
			{
				bills = FindBillsByMAWBNumber(message, headerSubQuery);
			}

			if (bills?.Any() ?? false)
			{
				bill = SelectBillOnMostRecentManifest(message, bills);
			}

			return bill;
		}

		AsycudaBill[] FindBillsByHAWBNumber(T message, ZDBOnlySubQuery headerSubQuery)
		{
			var hawb = message.HAWBNumber;
			if (!hawb.IsEmpty)
			{
				var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
				billQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, hawb);
				billQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
				billQuery.AddSubQuery(headerSubQuery, JoinCondition.And);

				return message.Factory.Load<AsycudaBill>(billQuery);
			}
			return null;
		}

		AsycudaBill[] FindBillsByMAWBNumber(T message, ZDBOnlySubQuery headerSubQuery)
		{
			var mawb = message.MAWBNumber;
			if (!mawb.IsEmpty)
			{
				var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
				billQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, new[] { mawb, message.MAWBNumberFormatted });
				billQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
				billQuery.AddToFilter(AsycudaBillSchema.ABL_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, message.EM_SystemCreateTimeUtc.AddYears(-1));

				billQuery.AddSubQuery(headerSubQuery, JoinCondition.And);

				return message.Factory.Load<AsycudaBill>(billQuery);
			}
			return null;
		}

		AsycudaBill SelectBillOnMostRecentManifest(T message, AsycudaBill[] bills)
		{
			var mawb = message.MAWBNumber;
			var mawbFormatted = message.MAWBNumberFormatted;
			var filteredBills = mawb.IsEmpty ? bills : bills.Where(b => b.Header.AMA_MasterBill == mawb || b.Header.AMA_MasterBill == mawbFormatted).ToArray();
			return filteredBills.OrderByDescending(b => b.Header.AMA_SystemCreateTimeUtc).FirstOrDefault();
		}

		#endregion

		protected virtual EDIMessage FindMostRecentSentMessage(EDIMessageCollectionNonDependent messages, T message, AsycudaManifestHeader manifest, AsycudaBill bill)
		{
			var outgoingMessageQuery = new ZQuery(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
			outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);

			if (bill.IsChildMasterBill)
			{
				outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, AsycudaManifestHeaderSchema.Constants.TableName);
				outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, manifest.PK);
			}
			else
			{
				outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, AsycudaBillSchema.Constants.TableName);
				outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, bill.PK);
			}

			outgoingMessageQuery.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + OrderByClause.Descending;

			return messages.Find(outgoingMessageQuery).FirstOrDefault() as EDIMessage;
		}

		protected AsycudaArrivalLine CreateUpdateArrivalLine(AsycudaManifestHeader manifest, AsycudaBill bill, AIMArrivalWrapper arrival)
		{
			var arrivalHeader = CreateUpdateArrivalHeader(manifest, bill, arrival);

			var arrivalDetails = arrivalHeader.ArrivalDetails;
			var arrivalLine = arrivalDetails.FirstOrDefault(x => x.ATL_ABL_AsycudaBill == bill.PK);
			if (arrivalLine == null)
			{
				arrivalLine = arrivalDetails.AddNew();
				arrivalLine.ATL_ABL_AsycudaBill = bill.PK;
			}

			if (!arrival.PartArrivalReference.IsEmpty)  // indicates a split shipment
			{
				arrivalLine.ATL_Reference = arrival.PartArrivalReference;
				arrivalLine.ATL_Quantity = (ZInt)arrival.BoardedPieces;
			}

			return arrivalLine;
		}

		protected void UpdateTransferBill(AsycudaBill bill, AsycudaArrivalLine arrivalLine, EDIMessage mostRecentSentMessage)
		{
			var transferBill = FindActiveTransferBill(bill, arrivalLine.ArrivalHeader);
			if (transferBill != null && mostRecentSentMessage != null)
			{
				if (mostRecentSentMessage.EM_MessageSubType == Constants.AIMMessageSubTypes.FSN)
				{
					transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.Arrived;
				}
				else
				{
					transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferAccepted;
				}
			}
		}

		protected AsycudaTransferBill FindActiveTransferBill(AsycudaBill bill, ManifestBase.AsycudaArrivalHeader arrivalHeader)
		{
			var query = new ZQuery(AsycudaTransferBillSchema.ATB_ABL_Bill, bill.PK);
			if (arrivalHeader != null)
			{
				query.AddToFilter(AsycudaTransferBillSchema.ATB_ATF_TransferHeader, arrivalHeader.TransferHeaders.Select(ah => ah.PK));
			}
			query.AddToFilter(AsycudaTransferBillSchema.ATB_MessageStatus, SQLComparisonOperator.NotEqual, AIMTransferStatusCodes.Codes.TransferCancelled);
			query.AddToFilter(AsycudaTransferBillSchema.ATB_MessageStatus, SQLComparisonOperator.NotEqual, AIMTransferStatusCodes.Codes.Arrived);
			query.OrderBy = $"{AsycudaTransferBillSchema.ATB_SystemCreateTimeUtc.Name} {OrderByClause.Descending}";
			return bill.Factory.LoadTop1<AsycudaTransferBill>(query);
		}

		AsycudaArrivalHeader CreateUpdateArrivalHeader(AsycudaManifestHeader manifest, AsycudaBill bill, AIMArrivalWrapper arrival)
		{
			var arrivalHeaders = manifest.ArrivalHeaders;

			var calulatedScheduledArrivalDate = CalculateScheduledArrivalDate(arrival.EstimateScheduledArrivalDate, manifest.AMA_E_ARV);

			var arrivalHeader = arrivalHeaders.FirstOrDefault(x => x.ATH_VoyageFlightNo.EqualsIgnoringCase(arrival.FlightNumber)
																&& x.ETAAtDischargePortForShortFormat.Date == calulatedScheduledArrivalDate);

			if (arrivalHeader == null)
			{
				arrivalHeader = arrivalHeaders.AddNew();
				arrivalHeader.ATH_VoyageFlightNo = arrival.FlightNumber;
				arrivalHeader.ATH_ETAAtDischargePort = calulatedScheduledArrivalDate;
			}

			if (bill.IsChildMasterBill)
			{
				arrivalHeader.ATH_Reference = arrival.PartArrivalReference;
			}

			return arrivalHeader;
		}

		protected AsycudaArrivalHeader FindArrivalHeader(AsycudaManifestHeader manifest, ZString flightNumber, ZDate arrivalDate)
		{
			var arrivalHeaders = manifest.ArrivalHeaders;

			var calulatedScheduledArrivalDate = CalculateScheduledArrivalDate(arrivalDate, manifest.AMA_E_ARV);

			var arrivalHeader = arrivalHeaders.FirstOrDefault(x => x.ATH_VoyageFlightNo.EqualsIgnoringCase(flightNumber)
																&& x.ETAAtDischargePortForShortFormat.Date == calulatedScheduledArrivalDate);

			return arrivalHeader;
		}

		/// <summary>
		/// CBP mentioned a 90 day enforcement period for this regulatory change.
		/// If the time in the message is outside the range of 90 days before and after the reference time, we will assume that this time is one year later.
		/// </summary>
		protected ZDate CalculateScheduledArrivalDate(ZDate estimateDate, ZDateTime referenceDate)
		{
			var result = estimateDate;

			if (estimateDate.IsValid && referenceDate.IsValid)
			{
				var start = referenceDate.AddMonths(-3);
				var end = referenceDate.AddMonths(3);

				var date = new ZDate(referenceDate.Year, estimateDate.Month, estimateDate.Day);

				var dates = new[]
				{
					date,
					date.AddYears(1),
					date.AddYears(-1)
				};

				result = dates.FirstOrDefault(c => c >= start && c <= end);

				if (!result.IsValid)
				{
					result = date.AddYears(1);
				}
			}

			return result;
		}

		void SetLinkedObject(T message, EDIMessage mostRecentSentMessage, AsycudaManifestHeader manifest, AsycudaBill bill)
		{
			if (mostRecentSentMessage != null)
			{
				message.EM_LinkUniqueID = mostRecentSentMessage.EM_LinkUniqueID;
				message.EM_LinkTable = mostRecentSentMessage.EM_LinkTable;
			}
			else if (bill.IsChildMasterBill)
			{
				message.EM_LinkedObject = manifest;
			}
			else
			{
				message.EM_LinkedObject = bill;
			}
		}

		#region CreateEmail

		const string MessageSender = " from CBP";

		EmailDef CreateEmail(T message, AsycudaBill bill)
		{
			var messageTypeDescription = GetMessageTypeDescription(message);
			var emailBuilder = new EmailDefBuilder(messageTypeDescription + GetJobDetails(message), EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			emailBuilder.AddArgReplacementRange(messageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.HeaderSectionDetails, GetEmailHeader(bill));
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, BuildPropertiesTable(message));

			var htmlPart2 = GetEmailContentPart2(message, emailBuilder, bill);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, htmlPart2);

			return emailBuilder.ToEmail();
		}

		protected ZString GetMessageTypeDescription(T message)
		{
			return ZString.Format("{0} ({1})", message.MessageTypeDescription, message.MessageTypeCode);
		}

		ZString GetJobDetails(T message)
		{
			ZString jobDetails = ZString.Empty;
			var manifestNumber = message.MAWBNumber;
			if (!manifestNumber.IsEmpty)
			{
				jobDetails += " - " + manifestNumber;
				var billNumber = message.HAWBNumber;
				if (!billNumber.IsEmpty)
				{
					jobDetails += " - " + billNumber;
				}
			}
			return jobDetails;
		}

		ZString GetEmailHeader(AsycudaBill bill)
		{
			var manifest = bill.Header;
			return ZString.Format(@"Job Number : {0}", manifest.AMA_MasterBill);
		}

		public ZString BuildPropertiesTable(T message)
		{
			var propertiesTable = new HtmlTableCreator(new string[] { "Property", "Value" });
			AddRowsToPropertiesTable(propertiesTable, message);

			return propertiesTable.ToHtml();
		}

		protected abstract void AddRowsToPropertiesTable(HtmlTableCreator propertiesTable, T message);

		protected virtual ZString GetEmailContentPart2(T message, EmailDefBuilder emailBuilder, AsycudaBill bill)
		{
			return ZString.Empty;
		}

		#endregion

		#region Send Email

		protected void SendEmail(EDIMessage mostRecentSentMessage, EmailDef emailDef, AsycudaManifestHeader manifest)
		{
			var recipient = GetSenderFromSentMessage(mostRecentSentMessage);
			var notificationGroupRegistryItem = GetNotificationGroupRegistryItem(manifest);
			var emailNotificationOptions = notificationGroupRegistryItem.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Env.CurrentBranchPK, Guid.Empty);

			var sendErrorOnly = emailNotificationOptions.SendErrorOnly;
			var emailSendMode = emailNotificationOptions.SendMode;
			var emailGroup = emailNotificationOptions.SendGroupPK;

			if (!sendErrorOnly)
			{
				var emailRepCal = new ACEManifestEmailRecipientCalculator(emailSendMode, emailGroup, recipient, ZGuid.Empty);
				try
				{
					emailRepCal.SendNotifications(manifest.Factory, emailDef, notificationGroupRegistryItem);
				}
				catch (EmailSendFailedException e)
				{
					Logger.LogError("Couldn't send email: " + e.Message + ".  Here are the contents of the email that couldn't be sent:\r\n\r\n" +
					"SUBJECT: " + emailDef.Subject + "\r\n" +
					"BODY: " + emailDef.Body + "\r\n");
				}
			}
		}

		GlbStaff GetSenderFromSentMessage(EDIMessage sentMessage)
		{
			GlbStaff result = null;

			var userCode = sentMessage?.EM_SystemCreateUser ?? ZString.Empty;
			if (!userCode.IsEmpty)
			{
				result = sentMessage.Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, userCode)).FirstOrDefault();
			}

			return result;
		}

		ManifestGroupNotificationRegistryItem GetNotificationGroupRegistryItem(AsycudaManifestHeader manifest)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, manifest.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.TransferredCode);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "TYP=HVL");

			if (manifest.Factory.Exists(typeof(StmALog), query))
			{
				return ManifestCustomsDataRegistry.Instance.USHVLVAirAMSGroupNotification;
			}
			else
			{
				return ManifestCustomsDataRegistry.Instance.USAirAMSGroupNotification;
			}
		}

		#endregion
	}
}
