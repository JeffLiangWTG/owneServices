using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.US.MessageDefinitions.ExportManifest;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class EMRMessageTypeProcessor : UEMMessageTypeProcessor<UEMEDIMessage>
	{
		const string ManifestAcceptResponseCode = "000";
		const string ManifestRejectResponseCode = "970";

		protected override bool ProcessMessageCore(UEMEDIMessage message)
		{
			var manifestFiling = UEMMessageHelper.DeSerializeManifestFiling(message.EM_MessageText);

			ResolveLinkedObjectByMessageReferenceNumber(manifestFiling, message);
			if (message.EM_LinkedObject == null)
			{
				ResolveLinkedObjectByMessageControlNumber(manifestFiling, message);
			}

			if (message.EM_LinkedObject != null)
			{
				var bill = message.EM_LinkedObject as AsycudaBill;
				var header = bill?.Header ?? message.EM_LinkedObject as AsycudaManifestHeader;

				if (header != null)
				{
					var isFailure = HasResponseCode(manifestFiling, ManifestRejectResponseCode);
					if (isFailure)
					{
						header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
					}
					else if (HasResponseCode(manifestFiling, ManifestAcceptResponseCode))
					{
						header.AMA_MessageStatus = MessageStatusCodeList.Codes.Registered;
					}

					GenerateHtmlEmailAndSendToUser(header, bill, message, isFailure);
				}
				return true;
			}
			return false;
		}

		void GenerateHtmlEmailAndSendToUser(AsycudaManifestHeader header, AsycudaBill bill, UEMEDIMessage message, bool isFailure)
		{
			var uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest, header.PK.ToGuid());
			var jobNumber = header.AMA_JobReference;
			var messageNum = message.EM_MessageNum;
			var additionalSubject = messageNum.IsEmpty ? string.Empty : "- " + messageNum;
			var body = message.EM_MessageInterpretation;
			var branch = header.Branch ?? GlbBranch.CurrentBranch;
			var emailAddressToSendTo = GetRecipient(message, header, bill);

			if (!emailAddressToSendTo.IsEmpty
				&& new Enterprise.Messaging.Business.HtmlResponseEmailGenerator().TryGenerateEmail(uri, jobNumber, "Export Manifest", body, isFailure, out var email, branch))
			{
				email.Subject = email.Subject + " " + additionalSubject;
				if (email != null)
				{
					using (branch.SetAsTemporaryContext())
					{
						email.AddRecipientForSystemCommunication(emailAddressToSendTo);
						if (email.Recipients.Count > 0)
						{
							Env.OutgoingCustomsMailManager.CreateAndSave(email);
						}
					}
				}
			}
			else
			{
				var failureMessage = string.Format(@"Email was fail to send. Job Number: {0}, Recipient: {1}", jobNumber, emailAddressToSendTo);
				message.Logs.AddNew(ZArchitecture.Business.Events.EmailSent, failureMessage);
			}
		}

		ZString GetRecipient(UEMEDIMessage message, AsycudaManifestHeader header, AsycudaBill bill)
		{
			var result = ZString.Empty;
			var messageNum = message.EM_MessageNum;
			BusinessObjectCollection messageCollection = null;

			if (bill != null)
			{
				messageCollection = bill.Messages;
			}
			else if (header != null)
			{
				messageCollection = header.Messages;
			}
			else
			{
				return result;
			}

			var originalMessage = messageCollection.Cast<UEMEDIMessage>()
					.Where(x => x.EM_MessageNum == messageNum &&
					x.EM_ApplicationCode == ApplicationCodeList.Codes.USExportManifest &&
					x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit &&
					x.EM_MessageType == MessageTypeList.Codes.ExportManifestSubmission &&
					x.EM_Status == EDIMessage.Status.Sent &&
					x.EM_SystemCreateTimeUtc <= ZDateTime.UtcNow)
					.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault()
					?? messageCollection.Cast<UEMEDIMessage>()
					.Where(x => x.EM_ApplicationCode == ApplicationCodeList.Codes.USExportManifest &&
					x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit &&
					x.EM_MessageType == MessageTypeList.Codes.ExportManifestSubmission &&
					x.EM_Status == EDIMessage.Status.Sent &&
					x.EM_SystemCreateTimeUtc <= ZDateTime.UtcNow)
					.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();

			var staff = originalMessage?.UserWhoQueuedThisRecord;
			if (staff != null && staff.GS_IsActive && !staff.GS_IsSystemAccount)
			{
				result = staff.GS_EmailAddress;
			}
			return result;
		}

		void ResolveLinkedObjectByMessageReferenceNumber(ManifestFiling manifestFiling, UEMEDIMessage message)
		{
			var messageReferenceNumber = manifestFiling?.Filing?.MessageReferenceNumber?.Value;
			if (!string.IsNullOrEmpty(messageReferenceNumber))
			{
				var query = new ZQuery(EDIMessageSchema.EM_MessageNum, messageReferenceNumber);
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.USExportManifest);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
				query.AddToFilter(EDIMessageSchema.EM_LinkTable, AsycudaBillSchema.Constants.TableName);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.ExportManifestSubmission);
				query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
				query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
				var foundMessage = message.Factory.LoadTop1<UEMEDIMessage>(query);
				if (foundMessage != null)
				{
					message.EM_MessageNum = messageReferenceNumber;
					message.EM_LinkedObject = foundMessage.EM_LinkedObject;
				}
			}
		}

		void ResolveLinkedObjectByMessageControlNumber(ManifestFiling manifestFiling, UEMEDIMessage message)
		{
			var messageControlNumber = manifestFiling?.Filing?.MessageControlNumber?.Value;
			if (!string.IsNullOrEmpty(messageControlNumber))
			{
				var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_JobReference, messageControlNumber);
				query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, ManifestBase.ApplicationCodeTypeList.Codes.Consolidator);
				query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestType, USExportManifestTypes.Codes.EFM);
				query.AddToFilter(AsycudaManifestHeaderSchema.AMA_Nature, ShipmentTypeList.Codes.Export22);
				query.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);
				var foundHeader = message.Factory.LoadTop1<AsycudaManifestHeader>(query);
				if (foundHeader != null)
				{
					message.EM_LinkedObject = foundHeader;
				}
			}
		}

		bool HasResponseCode(ManifestFiling manifestFiling, string code)
		{
			var result = manifestFiling.ResponseMessage.Any(x => x.ResponseCode == code);
			if (!result)
			{
				result = manifestFiling.Conveyance.Any(x => x.ResponseMessage.Any(z => z.ResponseCode == code));
			}
			if (!result)
			{
				result = manifestFiling.Conveyance.Any(x => x.BolInfoList.Any(z => z.ResponseMessage.Any(y => y.ResponseCode == code)));
			}
			return result;
		}
	}
}
