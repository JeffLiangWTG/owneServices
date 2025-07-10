using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class DpsEDIMessage : EDIMessage
	{
		public DpsEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EM_ApplicationCode = ApplicationCodeList.Codes.DPSRequestMessage;
			EM_MessageType = EDIMessageTypeList.Codes.JDC;
			EM_MessageSubType = EDIMessageSubTypeList.Codes.ScreeningRequest;
		}

		protected override string GetMessageReferenceNumber() => Env.NumberFountains.DpsEDIMessageNumber.GetNextFormatted(Factory);
	}
}
