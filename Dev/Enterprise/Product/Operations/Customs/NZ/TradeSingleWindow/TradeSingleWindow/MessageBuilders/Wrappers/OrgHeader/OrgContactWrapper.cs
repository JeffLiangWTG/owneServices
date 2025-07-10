using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class OrgContactWrapper : IContact
	{
		OrgContactWrapper(OrgContact organisationContact)
		{
			this.organisationContact = Argument.NotNull(organisationContact, "organisationContact cannot be null");
		}
		readonly OrgContact organisationContact;

		public static OrgContactWrapper New(OrgContact organisationContact)
		{
			return organisationContact == null ? null : new OrgContactWrapper(organisationContact);
		}

		public ZString ContactName => organisationContact.OC_ContactName;

		public IEnumerable<ICommunication> Communications
		{
			get
			{
				if (!organisationContact.OC_Email.IsEmpty)
				{
					yield return new Communication(organisationContact.OC_Email, CommunicationTypeList.Codes.EM);
				}

				if (!organisationContact.OC_Phone.IsEmpty)
				{
					yield return new Communication(organisationContact.OC_Phone, CommunicationTypeList.Codes.TE);
				}

				if (!organisationContact.OC_Mobile.IsEmpty)
				{
					yield return new Communication(organisationContact.OC_Mobile, CommunicationTypeList.Codes.AL);
				}

				if (!organisationContact.OC_Fax.IsEmpty)
				{
					yield return new Communication(organisationContact.OC_Fax, CommunicationTypeList.Codes.FX);
				}
			}
		}
	}
}
