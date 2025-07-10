using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using static NUnit.Framework.AssertionWithHtml;
using ApplicationCodes = Enterprise.Customs.Common.CusInBondApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

static class OrgContactsTestsHelper
{
	internal static void ValidateContactsPhones(BusinessObjectFactory factory, Func<NctsHeader, NctsDepartureMovementHeader, JobDocAddress> getJobDocAddress, params ZString[] testMovementTypeCodes)
	{
		const string errorMessageNoPhones = "Contact Name is present. However, Phone number is missing which is required. Hence, Contact information will be skipped in Customs Edi message.";

		CombineAssertions(() =>
		{
			foreach (var movementTypeCodes in testMovementTypeCodes)
			{
				TestValidatePrincipalPhonesInContext(movementTypeCodes, ApplicationCodes.NCTS5);
				TestValidatePrincipalPhonesInContext(movementTypeCodes, ApplicationCodes.NCTS4);
			}
		});

		void TestValidatePrincipalPhonesInContext(ZString movementType, ZString applicationCode)
		{
			var nctsHeader = factory.New<NctsHeader>();
			var orgHeader = NCTSTestHelper.CreateOrgHeaderForTest(factory);

			nctsHeader.SetMovementType(movementType);
			nctsHeader.BH_ApplicationCode = applicationCode;

			var movementHeader = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : null;

			var jobDocAddress = getJobDocAddress(nctsHeader, movementHeader);
			TestCaseWithFactory.AssertNoMessageError($"No representative selected, MovementType = {movementType}, ApplicationCode = {applicationCode}", jobDocAddress.OrganisationPKInfo, errorMessageNoPhones);
			jobDocAddress.OrganisationPK = orgHeader.PK;

			AssertMessageError("No selected", false);
			void AssertMessageError(string message, bool expectedError)
			{
				message += $", MovementType = {movementType}, ApplicationCode = {applicationCode}";

				jobDocAddress.Validation.ValidateOrganisationPK();
				if (expectedError && nctsHeader.IsPhase5Departure)
				{
					TestCaseWithFactory.AssertHasMessageError(message, jobDocAddress.OrganisationPKInfo, errorMessageNoPhones);
				}
				else
				{
					TestCaseWithFactory.AssertNoMessageError(message, jobDocAddress.OrganisationPKInfo, errorMessageNoPhones);
				}
			}

			orgHeader.Contacts.RemoveAll();
			AssertMessageError("No contacts", false);

			var fistContact = orgHeader.Contacts.AddNew();
			AssertMessageError("1 contact, with empty name, no phones", false);
			fistContact.OC_ContactName = "ABC";

			AssertPhones(fistContact, "1 contact");
			void AssertPhones(OrgContact contact, string message)
			{
				AssertMessageError(message + " has no phones", true);

				contact.OC_Phone = "1234";
				AssertMessageError(message + " has OC_Phone", false);
				contact.OC_Phone = ZString.Empty;

				contact.OC_Mobile = "1234";
				AssertMessageError(message + " has OC_Mobile", false);
				contact.OC_Mobile = ZString.Empty;

				contact.OC_HomePhone = "1234";
				AssertMessageError(message + " has OC_HomePhone", false);
				contact.OC_HomePhone = ZString.Empty;

				contact.OC_OtherPhone = "1234";
				AssertMessageError(message + " has OC_OtherPhone", false);
			}

			var secondContact = orgHeader.Contacts.AddNew();
			secondContact.OC_ContactName = "QSD";
			secondContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CAPGA;
			AssertMessageError("2 contacts, no with CUS allocation", false);

			var thirdContact = orgHeader.Contacts.AddNew();
			thirdContact.OC_ContactName = "563";
			thirdContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			AssertPhones(thirdContact, "3 contacts, contact with CUS allocation");
		}
	}
}
