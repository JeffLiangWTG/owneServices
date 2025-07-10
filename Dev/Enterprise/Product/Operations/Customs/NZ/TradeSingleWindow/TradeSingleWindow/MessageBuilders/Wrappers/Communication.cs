using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class Communication : ICommunication
	{
		public Communication(ZString detail, ZString type)
		{
			this.detail = detail;
			this.type = type;
		}
		readonly ZString detail;
		readonly ZString type;

		public ZString ContactDetail
		{
			get
			{
				var result = detail;
				if (ContactIsTelephoneTypeContact)
				{
					result = detail.KeepNumericCharacters();
				}
				return result;
			}
		}

		public ZString ContactType
		{
			get { return type; }
		}

		bool ContactIsTelephoneTypeContact
		{
			get
			{
				return ContactType == CommunicationTypeList.Codes.TE
					|| ContactType == CommunicationTypeList.Codes.AL
					|| ContactType == CommunicationTypeList.Codes.FX;
			}
		}
	}
}
