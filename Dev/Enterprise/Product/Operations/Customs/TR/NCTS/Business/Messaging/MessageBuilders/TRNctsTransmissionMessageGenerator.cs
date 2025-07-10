using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.TR.NCTS.Business.Messaging
{
	public class TRNctsTransmissionMessageGenerator : NctsTransmissionMessageGenerator
	{
		public TRNctsTransmissionMessageGenerator(NctsMessageFunctionSet messageFunction) : base(messageFunction)
		{
		}

		protected override IMessageNumberStrategy GetMessageNumberStrategy(BuilderResult builderResult)
		{
			return new NctsMessageNumberStrategy(builderResult.Message.Factory, ApplicationCodes.TRCustoms);
		}

		protected override (ZString MessageText, ZString MessageType, ZString MessageSubType) GetMessageTextAndMessageType(EU.NCTS.Business.NctsHeader header, NctsMessageFunctionSet how, ErrorCollector errorCollector)
		{
			var result = base.GetMessageTextAndMessageType(header, how, errorCollector);
			result.MessageType = TRMessageTypes.Codes.TRN;
			result.MessageSubType = ZString.Empty;

			if (!Globals.IsTest)
			{
				var trbPassword = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
				var signIn = new MessageSendAcknowledgeAndSign(result.MessageText, trbPassword);

				if (ObjectFactory.Get<IESignatureHelper>().CanSignMessage(signIn))
				{
					result.MessageText = processMessage(TRMessageTypes.Codes.TRN, result.MessageText, trbPassword, signIn.PINCode);
				}
				else
				{
					errorCollector.AddError(signIn.SigningCancelledMessage);
				}
			}

			return result;
		}

		protected override ZString GetApplicationCode()
		{
			return ApplicationCodes.TRCustoms;
		}

		protected override ZString GetMessageOwner(EU.NCTS.Business.NctsHeader header, ErrorCollector errorCollector)
		{
			return GlbStaff.CurrentUser.GS_Code;
		}

		protected override ZString GetMessageCodeFromMessage(EDIMessage message) => message.EM_MessageType;

		ZString processMessage(ZString messageType, ZString messageText, GlbExternalPassword_TR trbPassword, ZString pinCode)
		{
			var signer = TRMessageSigner.New();
			var processedMessageText = signer.ProcessSignedXMLMessage(messageType, signer.SignXMLMessage(messageText, trbPassword, pinCode), trbPassword.GP_UserID);
			return Encoding.UTF8.GetString(processedMessageText);
		}
	}
}
