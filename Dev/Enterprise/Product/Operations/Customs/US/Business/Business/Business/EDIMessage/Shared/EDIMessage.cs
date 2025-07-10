using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class EDIMessage : Messaging.Business.CBPEDIMessage
	{
		public new static readonly EDIMessageTypeDecider TypeDecider = new EDIMessageTypeDecider();

		public EDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString MessageTypeDescriptionCore
		{
			get
			{
				ZString messageDesc = EM_MessageSubTypeDescription;

				if (EM_MessageSubType == EM_MessageSubTypeList.Codes.EntrySummaryAdd && EM_ReceiveTransmit == "RCV" && EM_User.ToUpper() == "CUSTOMS")
				{
					messageDesc = "Entry Summary Response";
				}

				return messageDesc;
			}
		}

		public override ZString EM_MessageSubTypeDescription
		{
			get
			{
				string result = MessageSubTypeList.GetDescriptionFromCode(EM_MessageSubType);

				if (string.IsNullOrEmpty(result))
				{
					result = "Unknown Message Sub Type";
				}
				return result;
			}
		}

		protected override bool AllowExceeding9999Limit
		{
			get { return true; }
		}

		protected override Messaging.Business.BlockControlGenerator GetMessageBlock()
		{
			throw new NotSupportedException();
		}
	}
}
