using System.Data;

using CargoWise.EntityFramework;

using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Freight.Business
{
	public class ComTracMessage : EDIMessage
	{
		public ComTracMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "EDIAL", "ComTrac").GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.ComTrac;
			EM_MessageType = ApplicationCodeList.Codes.ComTrac;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EM_Status = EDIMessage.Status.Queued;
		}

		public override string CollationKey
		{
			get { return EM_ApplicationReference; }
		}
	}
}
