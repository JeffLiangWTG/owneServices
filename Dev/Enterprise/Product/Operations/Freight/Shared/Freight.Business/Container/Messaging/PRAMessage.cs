using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Business
{
	public class PRAMessage : EDIMessage
	{
		public static string SenderID
		{
			get
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				return registrationKey.EnterpriseCode +
					registrationKey.ServerCode;
			}
		}

		public PRAMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected string Sender
		{
			get
			{
				return "EDIAL";
			}
		}

		protected string Receiver
		{
			get
			{
				return "1STOP";
			}
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", Sender, Receiver).GetNextFormatted(Factory);
		}

		protected override string GetSendersReference()
		{
			return "";
		}

		protected CommonContainer Container
		{
			get
			{
				return Factory.Load<CommonContainer>(EM_LinkUniqueID);
			}
		}

		public const string MessageType = "PRA";
		public const string MessageSubType = "1ST";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.OneStop;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EM_MessageType = MessageType;
			EM_MessageSubType = MessageSubType;
			EM_Status = Status.Queued;
#if DEBUG
			EM_IsTestMessage = Env.Registry.Freight.PRAMessaging.PRATestMode;
#endif
		}
	}
}
