using System;
using CargoWise.Common;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class DAEMessageSender
	{
		public DAEMessageSender(AsycudaManifestHeader header, IDaeDeclaration dataProvider, ZString recordType)
		{
			this.header = header;
			messageBuilder = new DaeMessageBuilder(dataProvider, recordType);
		}

		readonly AsycudaManifestHeader header;
		readonly IXmlMessageBuilder messageBuilder;

		public ZString SendDAEMessage(ZString messageSubType, AsycudaBill[] selectedBills)
		{
			var result = ZString.Empty;

			var message = CreateEDIMessage();
			if (message != null)
			{
				try
				{
					header.Messages.Add(message);

					foreach (AsycudaBill bill in selectedBills)
					{
						if (messageSubType == MessageSubTypeCodes.Codes.Original)
						{
							bill.ABL_BillStatus = MessageStatusCodeList.Codes.Sent;
						}
						bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
					}

					header.AMA_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
					header.Factory.Save();

					result = FormattableString.Invariant($"{MessageSendSuccessful}");
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
					result = FormattableString.Invariant($"{MessageCreateFailure}\n{e.Message}");
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					message.Delete();
					result = FormattableString.Invariant($"{MessageSendFailure}\n{e.Message}");
				}
			}
			return result;
		}

		EDIMessage CreateEDIMessage()
		{
			var credentialWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.GetGlbExternalPassword<GlbCompanyCredential>(PasswordTypesList.Codes.UTB);

			var message = header.Factory.New<UYMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UYCustoms;
			message.EM_ApplicationReference = header.AMA_JobReference;
			message.EM_IsTestMessage = UYCustomsDataRegistry.Instance.IsUYTestingSystem;
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;
			message.EM_MessageOwner = credentialWrapper?.GP_UserID.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength) ?? ZString.Empty;
			message.EM_MessageText = UYMessageFormatting.FormatWithXMLRepresentation(messageBuilder.GenerateXmlMessage().GetSerializedString());
			message.EM_MessageType = EDIInterchangeTypeList.Codes.UYCustoms;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GP = credentialWrapper?.PK ?? ZGuid.Empty;

			return message;
		}

		static MultilingualString MessageCreateFailure => ResString.GetMultilingualString("1FBEA883-E76F-4036-B794-7981F07A5370", "Failed to create message");

		static MultilingualString MessageSendFailure => ResString.GetMultilingualString("D6AEAD34-360C-4A2A-89B6-2F6200C28884", "Failed to send message");

		static MultilingualString MessageSendSuccessful => ResString.GetMultilingualString("1AD384F4-9A83-4E22-A4E4-556440511FC4", "Message sent successfully");
	}
}
