using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public static class OrgHeaderExtensions
	{
		class ContactDetails : IPGAContactDetails
		{
			public ZString EmailAddress { get; set; }
			public ZString Fax { get; set; }
			public ZString Name { get; set; }
			public ZString PhoneNumber { get; set; }

			ZString IPGAContactDetails.EmailAddress
			{
				get { return EmailAddress; }
			}

			ZString IPGAContactDetails.Fax
			{
				get { return Fax; }
			}

			ZString IPGAContactDetails.Name
			{
				get { return Name; }
			}

			ZString IPGAContactDetails.PhoneNumber
			{
				get { return PhoneNumber.GetLocalPhoneNumber(Core.Constants.CountryCodes.UnitedStates); }
			}

			[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
			IAddressDetails IPGAContactDetails.CompanyAddress
			{
				get { throw new NotImplementedException(); }
			}
		}

		public static IPGAContactDetails GetContactDetails(this OrgHeader organisation, IEnumerable<string> allocationTypes)
		{
			var contactDetails = new ContactDetails();

			foreach (var allocationType in allocationTypes)
			{
				var orgContact = organisation.Contacts.GetContactForAllocation(allocationType);
				if (orgContact != null && orgContact.OC_IsActive)
				{
					contactDetails.Name = orgContact.OC_ContactName;
					contactDetails.EmailAddress = orgContact.OC_Email;
					contactDetails.PhoneNumber = orgContact.OC_Phone;
					contactDetails.Fax = orgContact.OC_Fax;

					return contactDetails;
				}
			}

			return null;
		}
	}
}
