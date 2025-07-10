using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class DeliveryNotificationPartiesContactsWrapper : IContact
	{
		public DeliveryNotificationPartiesContactsWrapper(ZString email)
		{
			this.email = email;
		}

		readonly ZString email;

		ZString IContact.ContactName => ZString.Empty;

		IEnumerable<ICommunication> IContact.Communications
		{
			get
			{
				if (!email.IsEmpty)
				{
					yield return new Communication(email, CommunicationTypeList.Codes.EM);
				}
			}
		}
	}
}
