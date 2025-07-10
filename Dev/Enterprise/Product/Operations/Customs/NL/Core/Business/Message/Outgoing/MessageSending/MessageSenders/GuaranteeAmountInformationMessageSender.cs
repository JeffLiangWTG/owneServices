using CargoWise.Customs.NL.MessageContracts.CI;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class GuaranteeAmountInformationMessageSender
{
	public GuaranteeAmountInformationMessageSender(JobDeclarationMessageSendingObjectParent declarationWrapper)
	{
		declaration = declarationWrapper.ParentDeclaration;
		var wrapper = new RequestForGuaranteeInformationWrapper(declaration);
		messageBuilder = new GuaranteeAmountInformationMessageBuilder(wrapper);
	}
	protected readonly JobDeclaration declaration;
	protected readonly IXmlMessageBuilder messageBuilder;

	public ZString SendMessage()
	{
		var continueSend = true;
		var result = new ZStringBuilder();
		var newMessage = CreateEDIMessage();
		if (newMessage.EM_MessageText.IsEmpty)
		{
			continueSend = false;
			result.AppendLine(Res.GetString("1E7C6020-9B12-4582-B703-675A20DF2865", "Unable to send empty message"));
		}

		if (continueSend)
		{
			declaration.Messages.Add(newMessage);
			try
			{
				declaration.Factory.Save();
				result.AppendLine(Res.GetString("E25D4682-4CD7-467C-800A-EBA548813C3B", "Message sent successfully"));
			}
			catch (ZSaveException ex)
			{
				result.AppendLine(Res.GetString("9ABCA420-AF68-4BC3-9E6D-A737F3FCCF14", "The following error was encountered while saving the changes:"));
				result.AppendLine(ex.Message);
				newMessage.Delete();
			}
		}
		else
		{
			newMessage.Delete();
		}

		return result.ToString();
	}

	protected virtual NLEDIMessage CreateEDIMessage()
	{
		var message = declaration.Factory.New<NLEDIMessage>();
		message.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		message.EM_MessageSubType = NLConstants.EdiMessageSubTypes.GuaranteeAmountInformation;
		message.EM_IsTestMessage = NLCustomsRegistry.Instance.IsNLTestingSystem.Value;
		message.SetEM_MessageTextOrDataSource(messageBuilder.GenerateXmlMessage().GetSerializedStream());
		message.EM_LinkedObject = declaration;
		message.EM_Status = NLEDIMessage.Status.Pending;

		return message;
	}
}
