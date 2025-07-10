using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class DeliveryNotificationPartiesWrapper : IOrganisationSimple
	{
		public DeliveryNotificationPartiesWrapper(ZString name, ZString customsClientCode, ZString email)
		{
			this.name = name;
			this.customsClientCode = customsClientCode;
			this.email = email;
		}
		readonly ZString name;
		readonly ZString customsClientCode;
		readonly ZString email;

		ZString IOrganisationSimple.Name => name;

		ZString IOrganisationSimple.CustomsClientCode => customsClientCode;

		ZString IOrganisationSimple.CustomsSupplierCode => ZString.Empty;

		IEnumerable<IContact> IOrganisationSimple.Contacts
		{
			get
			{
				if (!email.IsEmpty)
				{
					yield return new DeliveryNotificationPartiesContactsWrapper(email);
				}
			}
		}
	}
}
