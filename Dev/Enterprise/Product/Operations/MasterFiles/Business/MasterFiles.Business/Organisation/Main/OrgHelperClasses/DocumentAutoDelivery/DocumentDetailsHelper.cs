using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class DocumentDetailsHelper
	{
		public static void PopulateAutoDeliveryContactsForJob(this DocDeliveryContactCollection contacts, IStmMenuItem menuItem, BusinessObject job)
		{
			var jobDocumentDeliveryCollection = new JobDocumentDeliveryCollection(contacts.Factory, job);
			var menuItemQuery = new ZQuery(JobDocumentDeliverySchema.JDC_SU_MenuItem, menuItem.PK);
			menuItemQuery.AddToFilter(JoinCondition.Or, JobDocumentDeliverySchema.JDC_DocumentGroup, menuItem.SU_ContactType);
			menuItemQuery.AddToFilter(new ZQuery(JobDocumentDeliverySchema.JDC_DeliveryMethod, SQLComparisonOperator.NotEqual, Core.Constants.ContactNotifyModes.DoNotDeliver));
			jobDocumentDeliveryCollection.AdditionalFilter = menuItemQuery;

			jobDocumentDeliveryCollection.Select(d => d.GetDocDeliveryDetails(menuItem)).ForEach(contacts.Add);
		}

		internal static void PopulateToCcAndBccRecipients(this DocDeliveryContact contact, JobDocumentDelivery jobDocumentDelivery)
		{
			contact.ClearToCcAndBccRecipients();
			contact.Email = jobDocumentDelivery.JDC_EmailToRecipientsAsString;
			contact.EmailCarbonCopyRecipients.Value = jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString;
			contact.EmailBlindCarbonCopyRecipients.Value = jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString;
		}

		internal static void PopulateAddressDetailsFromOrganisation(this DocDeliveryContact docDeliveryContact)
		{
			OrgAddress addressToUse = null;
			var addressPKToUse = ZGuid.Empty;
			if (docDeliveryContact.Contact != null)
			{
				if (docDeliveryContact.Contact.WorkingAddressPK.IsValid && !docDeliveryContact.Contact.IsSystemDefaultContactForAutoDelivery)
				{
					addressPKToUse = docDeliveryContact.Contact.WorkingAddressPK;
				}
				else
				{
					addressToUse = docDeliveryContact.Contact.OrganisationOrAddressOverride.Addresses.AddressForDocument(docDeliveryContact.MenuItem, docDeliveryContact.DeliveryMethod, true, docDeliveryContact.DeliveryLanguage);
				}
			}
			else if (docDeliveryContact.OrgHeader != null)
			{
				addressToUse = docDeliveryContact.OrgHeader.Addresses.AddressForDocument(docDeliveryContact.MenuItem, docDeliveryContact.DeliveryMethod, true, docDeliveryContact.DeliveryLanguage);
			}

			if (!addressPKToUse.IsValid)
			{
				addressPKToUse = addressToUse?.PK ?? ZGuid.Empty;
			}

			docDeliveryContact.OrgAddressPK = addressPKToUse;
			PopulateAddressDetailsFromAddressOrContact(docDeliveryContact);
		}

		internal static void PopulateAddressDetailsFromAddressOrContact(this DocDeliveryContact docDeliveryContact)
		{
			docDeliveryContact.UpdateCompanyName();
			docDeliveryContact.UpdateEmailDetails();
			docDeliveryContact.UpdateFaxDetails();
			docDeliveryContact.UpdatePhoneDetails();
			docDeliveryContact.UNLOCO = docDeliveryContact.OrgAddress != null && docDeliveryContact.OrgAddress.RelatedPortCode != null ? docDeliveryContact.OrgAddress.RelatedPortCode : docDeliveryContact.OrgHeader != null ? docDeliveryContact.OrgHeader.UNLOCO : null;
			if (docDeliveryContact.OrgAddress != null)
			{
				docDeliveryContact.Address1 = docDeliveryContact.OrgAddress.OA_Address1;
				docDeliveryContact.Address2 = docDeliveryContact.OrgAddress.OA_Address2;
				docDeliveryContact.City = docDeliveryContact.OrgAddress.OA_City;
				docDeliveryContact.State = docDeliveryContact.OrgAddress.OA_State;
				docDeliveryContact.PostCode = docDeliveryContact.OrgAddress.OA_PostCode;
				docDeliveryContact.Language = docDeliveryContact.OrgAddress.OA_Language;
				docDeliveryContact.AdditionalAddress = docDeliveryContact.OrgAddress.OA_AdditionalAddressInformation;
			}
			else
			{
				docDeliveryContact.Address1 = DocDeliveryContact.NoOrganizationDetailsFoundMessage.ToString(docDeliveryContact.DeliveryLanguage);
				docDeliveryContact.Address2 = ZString.Empty;
				docDeliveryContact.City = ZString.Empty;
				docDeliveryContact.State = ZString.Empty;
				docDeliveryContact.PostCode = ZString.Empty;
				docDeliveryContact.Language = ZString.Empty;
				docDeliveryContact.AdditionalAddress = ZString.Empty;
			}
		}

		#region Implementation

		internal static void UpdateEmailDetails(this DocDeliveryContact docDeliveryContact)
		{
			if (docDeliveryContact.UpdateEmailAndFax)
			{
				if (docDeliveryContact.Contact != null && !docDeliveryContact.Contact.OC_Email.IsEmpty)
				{
					docDeliveryContact.Email = docDeliveryContact.Contact.OC_Email;
				}
				else if (docDeliveryContact.OrgAddress != null && !docDeliveryContact.OrgAddress.OA_Email.IsEmpty)
				{
					docDeliveryContact.Email = docDeliveryContact.OrgAddress.OA_Email;
				}
			}
		}

		internal static void UpdatePhoneDetails(this DocDeliveryContact docDeliveryContact)
		{
			if (docDeliveryContact.Contact != null && !docDeliveryContact.Contact.OC_Phone.IsEmpty)
			{
				docDeliveryContact.Phone = docDeliveryContact.Contact.OC_Phone;
			}
			else if (docDeliveryContact.OrgAddress != null)
			{
				docDeliveryContact.Phone = docDeliveryContact.OrgAddress.OA_Phone;
			}
		}

		internal static void UpdateFaxDetails(this DocDeliveryContact docDeliveryContact)
		{
			if (docDeliveryContact.UpdateEmailAndFax)
			{
				if (docDeliveryContact.Contact != null && !docDeliveryContact.Contact.OC_Fax.IsEmpty)
				{
					docDeliveryContact.Fax = docDeliveryContact.Contact.OC_Fax;
				}
				else if (docDeliveryContact.OrgAddress != null)
				{
					docDeliveryContact.Fax = docDeliveryContact.OrgAddress.OA_Fax;
				}
			}
		}

		public static void UpdateCompanyName(this DocDeliveryContact docDeliveryContact)
		{
			if (docDeliveryContact.OrgAddress != null && !docDeliveryContact.OrgAddress.OA_CompanyNameOverride.IsEmpty)
			{
				docDeliveryContact.CompanyName = docDeliveryContact.OrgAddress.OA_CompanyNameOverride;
			}
			else if (docDeliveryContact.OrgHeader != null)
			{
				docDeliveryContact.CompanyName = docDeliveryContact.OrgHeader.OH_FullName;
			}
		}

		internal static void UpdateCcAndBccEmailDetails(this DocDeliveryContact docDeliveryContact)
		{
			docDeliveryContact.ClearCcAndBccRecipients();

			if (docDeliveryContact.Contact == null)
			{
				return;
			}

			if (docDeliveryContact.DocAutoDelivery != null)
			{
				var orgDocuments = docDeliveryContact.DocAutoDelivery.FilterOrgDocuments(docDeliveryContact.Contact,
					docDeliveryContact.DocAutoDelivery.DocumentSupporter);

				orgDocuments = orgDocuments.Where(d =>
							 d.OD_DeliverBy == docDeliveryContact.DeliveryMethod &&
							 d.OD_AttachmentType == docDeliveryContact.AttachmentType).ToList();

				orgDocuments.ForEach(d =>
				{
					docDeliveryContact.Contact.PopulateCcAndBccRecipients(docDeliveryContact, d);
				});
			}
		}

		internal static void ClearCcAndBccRecipients(this DocDeliveryContact docDeliveryContact)
		{
			docDeliveryContact.EmailCarbonCopyRecipients.RemoveAndDeleteAll();
			docDeliveryContact.EmailBlindCarbonCopyRecipients.RemoveAndDeleteAll();
		}

		internal static void ClearToCcAndBccRecipients(this DocDeliveryContact docDeliveryContact)
		{
			docDeliveryContact.EmailToRecipients.RemoveAndDeleteAll();
			docDeliveryContact.ClearCcAndBccRecipients();
		}

		#endregion
	}
}
