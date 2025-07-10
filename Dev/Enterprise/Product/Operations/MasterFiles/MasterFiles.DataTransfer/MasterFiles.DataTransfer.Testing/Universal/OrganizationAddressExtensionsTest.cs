using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class OrganizationAddressExtensionsTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestAddAddressErrorMessage()
		{
			var builder1 = new ZStringBuilder();
			OrganizationAddress addressDataObject = null;
			addressDataObject.AddAddressErrorMessage("test", builder1);
			AssertEquals("", builder1.ToString());

			addressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			addressDataObject.AddAddressErrorMessage("test", builder1);
			AssertEquals("Unable to match test Address, please make sure the supplied test Address is valid. Details were:", builder1.ToString());

			addressDataObject.Address1 = "123 Something st";
			addressDataObject.Address2 = "Some Place";
			addressDataObject.AddressShortCode = "123Place";
			addressDataObject.City = "Some City";
			addressDataObject.CompanyName = "Some Company";
			addressDataObject.Country = new Country { Code = "AU", Name = "Australia" };
			addressDataObject.Email = "something@something.com";
			addressDataObject.Fax = "11112222";
			addressDataObject.GovRegNum = "12345";
			addressDataObject.GovRegNumType = new RegistrationNumberType { Code = "XXX", Description = "Registrar" };
			addressDataObject.Mobile = "0411111111";
			addressDataObject.OrganizationCode = "SOMETHING";
			addressDataObject.Phone = "22223333";
			addressDataObject.Port = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			addressDataObject.Postcode = "5555";
			addressDataObject.State = "Some State";
			addressDataObject.UniversalNettingCode = "SomeSome";
			addressDataObject.UniversalOfficeCode = "OfficeSome";
			addressDataObject.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var registrationNumber1 = new RegistrationNumber
			{
				CountryOfIssue = new Country { Code = "NZ" },
				Type = new RegistrationNumberType { Code = "TTT", Description = "Tarantula" },
				Value = "654321"
			};
			var registrationNumber2 = new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = "ZZZ" },
				Value = "999999"
			};
			addressDataObject.RegistrationNumberCollection.Add(registrationNumber1);
			addressDataObject.RegistrationNumberCollection.Add(registrationNumber2);

			var builder2 = new ZStringBuilder();
			addressDataObject.AddAddressErrorMessage("test", builder2);
			AssertEquals(@"
Unable to match test Address, please make sure the supplied test Address is valid. Details were:
Address1: 123 Something st
Address2: Some Place
AddressShortCode: 123Place
City: Some City
CompanyName: Some Company
Country: AU - Australia
Email: something@something.com
Fax: 11112222
GovRegNum: 12345
GovRegNumType: XXX - Registrar
Mobile: 0411111111
OrganizationCode: SOMETHING
Phone: 22223333
Port: AUSYD - Sydney
Postcode: 5555
State: Some State
UniversalNettingCode: SomeSome
UniversalOfficeCode: OfficeSome
RegistrationNumber 1:
CountryOfIssue: NZ
Type: TTT - Tarantula
Value: 654321
RegistrationNumber 2:
Type: ZZZ
Value: 999999".Trim(), builder2.ToStringWithNewLineBetweenAppends());
		}
	}
}
