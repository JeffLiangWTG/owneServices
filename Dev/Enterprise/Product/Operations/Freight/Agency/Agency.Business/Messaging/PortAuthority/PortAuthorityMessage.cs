using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class PortAuthorityMessage : EDIMessage
	{
		static string UsKeyPart { get { return "EDI"; } }
		const string ThemKeyPart = "PortAuthority";

		public PortAuthorityMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region New

		public static PortAuthorityMessage New(ISailingEndPoint endPoint, IPortAuthorityMessagingData data, ZString version, ZString messageType, ZString principal)
		{
			if (endPoint == null)
			{
				throw new ArgumentNullException(nameof(endPoint));
			}

			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			PortAuthorityMessage message = (PortAuthorityMessage)endPoint.Messages.AddNew(typeof(PortAuthorityMessage));
			message.EM_MessageSubType = messageType;
			message.EM_MessageText = PortAuthorityMessageBuilderFactory.GetBuilder(version).GenerateMessageText(data);
			message.EM_ApplicationReference = principal;
			return message;
		}

		#endregion

		#region Overrides

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", UsKeyPart, ThemKeyPart).GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EM_ApplicationCode = ApplicationCodes.PortAuthority;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EM_MessageType = ApplicationCodes.PortAuthority;
			EM_MessageSubType = "";
			EM_Status = Status.Queued;
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return new PortMessageTypeList(); }
		}

		#endregion
	}
}




