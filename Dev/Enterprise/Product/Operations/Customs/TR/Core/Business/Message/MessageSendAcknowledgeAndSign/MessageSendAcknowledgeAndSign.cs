using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TR.Business
{
	public class MessageSendAcknowledgeAndSign : NonPersistentBusinessObject
	{
		public MessageSendAcknowledgeAndSign(ZString xmlMessage, GlbExternalPassword_TR user, ZString messageType = default, ZString nationalXMLText = default)
		{
			CurrentUserExternalPasswordInfo = user;
			MessageTextBeforeSign = xmlMessage;
			MessageType = messageType;
			// TODO: Remove the error suppression and add actual national xml text IN FUTURE WORK ITEM
#pragma warning disable CW1161
			NationalXMLTextBeforeSign = nationalXMLText.IsDefault ? "National XML" : nationalXMLText;
#pragma warning restore CW1161
		}

		[ResourceStringData("MessageSendAcknowledgeAndSign|MessageTextBeforeSign", Caption = "Message Context")]
		public ZString MessageTextBeforeSign { get; }

		[ResourceStringData("MessageSendAcknowledgeAndSign|NationalXMLTextBeforeSign", Caption = "National XML")]
		public ZString NationalXMLTextBeforeSign { get; }

		public GlbExternalPassword_TR CurrentUserExternalPasswordInfo { get; set; }

		[ResourceStringData("MessageSendAcknowledgeAndSign|CurentUserName", Caption = "Signature Owner")]
		public ZString CurrentUserName => CurrentUserExternalPasswordInfo?.Staff?.HumanReadableName ?? ZString.Empty;

		[ResourceStringData("MessageSendAcknowledgeAndSign|PINCode", Caption = "Pin Code")]
		public ZString PINCode
		{
			get { return CurrentUserExternalPasswordInfo?.PINCode ?? ZString.Empty; }
			set
			{
				if (CurrentUserExternalPasswordInfo != null)
				{
					CurrentUserExternalPasswordInfo.PINCode = value;
				}
			}
		}

		public ZString MessageType { get; }

		public string SigningCancelledMessage => Res.GetString("5E96252B-7977-4132-8019-1F027BBFB916", "Message signing canceled");
	}
}


