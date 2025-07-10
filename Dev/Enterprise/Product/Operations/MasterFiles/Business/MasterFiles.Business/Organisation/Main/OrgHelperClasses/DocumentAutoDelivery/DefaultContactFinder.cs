using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DefaultContactFinder
	{
		/// <summary>
		/// Use this object to find the correct contact to receive certain document types on an organisation.
		/// If no contact is found, a brand new system generated contact will be returned.
		/// </summary>
		/// <param name="organisation">The org on which to search</param>
		public DefaultContactFinder(OrgHeader organisation)
			: this(organisation, true)
		{
		}

		/// <summary>
		/// Use this object to find the correct contact to receive certain document types on an organisation.
		/// </summary>
		/// <param name="organisation">The org on which to search</param>
		/// <param name="allowSystemGeneratedContact">If no contact is found, default behaviour is to return a brand new
		/// contact which is system generated. Specify false here if you do not want this behaviour.</param>
		public DefaultContactFinder(OrgHeader organisation, ZBool allowSystemGeneratedContact)
			: this(organisation, allowSystemGeneratedContact, null, null)
		{
		}

		/// <summary>
		/// Use this object to find the correct contact to receive certain document types on an organisation.
		/// </summary>
		/// <param name="organisation">The org on which to search</param>
		/// <param name="stmMenuItem">If exist stmMenuItem when create DefaultContactFinder, user it to set delivery method
		public DefaultContactFinder(OrgHeader organisation, IStmMenuItem stmMenuItem = null)
			: this(organisation, true, null, stmMenuItem)
		{
		}

		/// <summary>
		/// Use this object to find the correct contact to receive certain document types on an organisation.
		/// </summary>
		/// <param name="organisation">The org on which to search</param>
		/// <param name="allowSystemGeneratedContact">If no contact is found, default behaviour is to return a brand new
		/// contact which is system generated. Specify false here if you do not want this behaviour.</param>
		/// <param name="factory">The factory used when the organisation is null.</param>
		public DefaultContactFinder(OrgHeader organisation, ZBool allowSystemGeneratedContact, BusinessObjectFactory factory, IStmMenuItem stmMenuItem = null)
		{
			this.allowSystemGeneratedContact = allowSystemGeneratedContact;
			this.organisation = organisation;
			this.factory = factory;
			this.stmMenuItem = stmMenuItem;
		}

		readonly OrgHeader organisation;
		readonly bool allowSystemGeneratedContact;
		readonly BusinessObjectFactory factory;
		readonly IStmMenuItem stmMenuItem;

		#region Default Contact Retrieval

		public OrgContact DefaultContact(string type)
		{
			return DefaultContact(type, "");
		}

		public OrgContact DefaultContact(Guid menuItem, string type)
		{
			OrgContact result = null;

			var documentQuery = new ZQuery(OrgDocumentSchema.OD_SU_MenuItem, menuItem);

			foreach (OrgContact contact in organisation.Contacts)
			{
				var documents = (OrgDocument[])contact.Documents.Find(documentQuery);
				if (documents.Length > 0)
				{
					result = contact;
					break;
				}
			}

			if (result == null)
			{
				result = DefaultContact(type);
			}

			return result;
		}

		public OrgContact DefaultContact(string type, string transportMode)
		{
			return DefaultContact(ContactType.Find(type), transportMode);
		}

		public OrgContact DefaultContact(ContactType type)
		{
			return DefaultContact(type, "");
		}

		public OrgContact DefaultContact(ContactType type, string transportMode)
		{
			var contactType = type.CalculateInitialContactType(transportMode);

			OrgContact defaultContact = null;

			if (organisation != null)
			{
				if (contactType == ContactType.Consignee)
				{
					defaultContact = DefaultContactForConsignee(transportMode);
				}
				else if (contactType == ContactType.Consignor)
				{
					defaultContact = DefaultContactForConsignor(transportMode);
				}
				else
				{
					defaultContact = DefaultContactForType(contactType);
				}
			}

			if (defaultContact == null)
			{
				defaultContact = SystemDefaultContact(contactType);
			}

			return defaultContact;
		}

		public OrgContact DefaultContactAllocationType(string type) => organisation.Contacts.Cast<OrgContact>().FirstOrDefault(c => c.Allocations.Cast<OrgContactAllocation>().Any(ca => ca.PC_Type == type));

		#endregion

		#region System Default Contact

		internal OrgContact SystemDefaultContact(ContactType contactType)
		{
			if (!allowSystemGeneratedContact)
			{
				return null;
			}

			var contactTypeDefaultName = contactType.DefaultName;
			if (!DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.Value)
			{
				contactTypeDefaultName = (NoResString)"";
			}

			var orgPK = organisation != null ? organisation.PK : Guid.Empty;
			var contactQuery = new ZQuery(OrgContactSchema.OC_OH, orgPK);
			contactQuery.FetchOnlyFromLocalCache = true;

			var contacts = SystemDefaultContactFactory.Load<OrgContact>(contactQuery);
			var systemDefaultContact = contacts.FirstOrDefault(contact => contact.SystemDefaultContactName != null && contact.SystemDefaultContactName.ToString() == contactTypeDefaultName.ToString());

			if (systemDefaultContact != null)
			{
				systemDefaultContact.OC_ContactName = contactTypeDefaultName;
			}
			else
			{
				systemDefaultContact = SystemDefaultContactFactory.New<OrgContact>();
				systemDefaultContact.OC_ContactName = contactTypeDefaultName;
				systemDefaultContact.SystemDefaultContactName = contactTypeDefaultName;
				if (DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.Value)
				{
					systemDefaultContact.OC_Salutation = DefaultSalutationProvider.GetDefaultSalutation(Enterprise.Core.Constants.Languages.English, string.Empty).
						Replace(Core.Constants.SalutationMacros.Name, systemDefaultContact.OC_ContactName);
				}

				systemDefaultContact.IsSystemDefaultContact = true;

				if (organisation == null)
				{
					return systemDefaultContact;
				}
			}

			SetDeliveryMethodForSystemDefaultContact(stmMenuItem != null ?
				organisation.Addresses.AddressForDocument(stmMenuItem, ZString.Empty, false) : organisation?.MainAddress, systemDefaultContact);

			return systemDefaultContact;
		}

		internal void SetDeliveryMethodForSystemDefaultContact(OrgAddress address, OrgContact systemDefaultContact)
		{
			if (organisation != null)
			{
				foreach (var handled in GetDeliveryMethodHandlers())
				{
					if (address != null && handled(systemDefaultContact, address))
					{
						break;
					}
				}
				systemDefaultContact.OC_OH = organisation.PK;
			}
		}

		DeliveryMethodHandler[] GetDeliveryMethodHandlers()
		{
			switch (DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.Value)
			{
				case DeliveryMethodOptionList.Codes.EmailFaxThenPrint:
					return new DeliveryMethodHandler[] { HandleEmail, HandleFax, HandlePrint };

				case DeliveryMethodOptionList.Codes.FaxEmailThenPrint:
					return new DeliveryMethodHandler[] { HandleFax, HandleEmail, HandlePrint };

				case DeliveryMethodOptionList.Codes.EmailThenPrint:
					return new DeliveryMethodHandler[] { HandleEmail, HandlePrint };

				case DeliveryMethodOptionList.Codes.FaxThenPrint:
					return new DeliveryMethodHandler[] { HandleFax, HandlePrint };

				default:
					return new DeliveryMethodHandler[] { HandlePrint };
			}
		}

		delegate bool DeliveryMethodHandler(OrgContact contact, OrgAddress address);

		bool HandleEmail(OrgContact contact, OrgAddress address)
		{
			bool canHandle = !address.OA_Email.IsEmpty;
			if (canHandle)
			{
				contact.OC_NotifyMode = Constants.ContactNotifyModes.Email;
				contact.OC_AttachmentType = OrgConstants.AttachmentType.XLS;
			}
			return canHandle;
		}

		bool HandleFax(OrgContact contact, OrgAddress address)
		{
			bool canHandle = !address.OA_Fax.IsEmpty;
			if (canHandle)
			{
				contact.OC_NotifyMode = Constants.ContactNotifyModes.Fax;
				contact.OC_AttachmentType = ZString.Empty;
			}
			return canHandle;
		}

		bool HandlePrint(OrgContact contact, OrgAddress address)
		{
			contact.OC_NotifyMode = Constants.ContactNotifyModes.Print;
			contact.OC_AttachmentType = ZString.Empty;
			return true;
		}

		public ReadOnlyBusinessObjectFactory SystemDefaultContactFactory
		{
			get
			{
				if (systemDefaultContactFactory == null)
				{
					if (organisation != null)
					{
						systemDefaultContactFactory = organisation.Factory.GetCachedReadOnlyFactory();

						if (systemDefaultContactFactory != null)
						{
							systemDefaultContactFactory.ImportFromAnotherFactorySafe(organisation);
						}
					}

					if (systemDefaultContactFactory == null)
					{
						systemDefaultContactFactory = factory != null ? factory.GetCachedReadOnlyFactory() : new ReadOnlyBusinessObjectFactory();
						systemDefaultContactFactory.NameForDebugging = "DefaultContactFinder";
						systemDefaultContactFactory.RefreshEnabled = false;
					}
				}
				return systemDefaultContactFactory;
			}
		}

		ReadOnlyBusinessObjectFactory systemDefaultContactFactory;

		#endregion

		#region Default Consignee Contact

		OrgContact DefaultContactForConsignee(string transportMode)
		{
			OrgContact defaultContact = null;

			foreach (OrgContact contact in organisation.Contacts)
			{
				if (!contact.Documents.ContainsDefault(ContactType.Consignee.Code))
				{
					continue;
				}

				defaultContact = contact;
				break;
			}

			if (defaultContact != null)
			{
				return defaultContact;
			}

			if (organisation.OH_IsForwarder)
			{
				if (transportMode == Constants.TransportModes.Air)
				{
					defaultContact = DefaultContactForType(ContactType.ImportAirFreightAgent);
				}
				else
				{
					defaultContact = DefaultContactForType(ContactType.ImportSeaFreightAgent);
				}
			}
			else
			{
				defaultContact = DefaultContactForType(ContactType.Find(ContactType.Consignee.AggregateParentType));
			}

			return defaultContact;
		}

		#endregion

		#region Default Consignor Contact

		OrgContact DefaultContactForConsignor(string transportMode)
		{
			OrgContact defaultContact = null;

			foreach (OrgContact contact in organisation.Contacts)
			{
				if (contact.Documents.ContainsDefault(ContactType.Consignor.Code))
				{
					defaultContact = contact;
					break;
				}
			}

			if (defaultContact == null)
			{
				if (organisation.OH_IsForwarder)
				{
					if (transportMode == Constants.TransportModes.Air)
					{
						defaultContact = DefaultContactForType(ContactType.ExportAirFreightAgent);
					}
					else
					{
						defaultContact = DefaultContactForType(ContactType.ExportSeaFreightAgent);
					}
				}
				else
				{
					defaultContact = DefaultContactForType(ContactType.Find(ContactType.Consignor.AggregateParentType));
				}
			}

			return defaultContact;
		}

		#endregion

		#region Default Notify Party Contact

		public static ZGuid GetDefaultNotifyPartyContact(OrgHeader organisation)
		{
			if (organisation != null)
			{
				var contact = new DefaultContactFinder(organisation, false).DefaultContact(ContactType.NotifyParty);
				if (contact != null)
				{
					return contact.PK;
				}
			}

			return ZGuid.Empty;
		}

		#endregion

		#region Default Contact For Type

		OrgContact DefaultContactForType(ContactType type)
		{
			OrgContact defaultContact = null;
			foreach (OrgContact contact in organisation.Contacts)
			{
				if (contact.OC_IsActive && contact.Documents.ContainsDefault(type.Code))
				{
					defaultContact = contact;
					break;
				}
			}

			if (defaultContact == null)
			{
				if (!string.IsNullOrEmpty(type.AggregateParentType))
				{
					defaultContact = DefaultContactForType(ContactType.Find(type.AggregateParentType));
				}
			}

			return defaultContact;
		}

		#endregion
	}
}
