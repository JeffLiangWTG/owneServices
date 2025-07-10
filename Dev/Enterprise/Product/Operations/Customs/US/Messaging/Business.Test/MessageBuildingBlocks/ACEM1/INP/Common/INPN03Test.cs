using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class INPN03Test : TestCase
	{
		public void TestISerialiserWithOrigAppIDImplementation()
		{
			var inpn03 = new INPN03()
			{
				CityName = "BOB",
				StateProvinceCode = "CA",
				PostalCode = "PO123",
				CountryCode = "US"
			};
			ISerialiserSupporter serialiser = inpn03;

			var inpn00 = new INPN00()
			{
				EntityCode = "N2",
				EntityName = "NOTIFIER"
			};
			var inpn01 = new INPN01()
			{
				NotifyPartyNameCode = "PARTY NAME",
				NotifyPartyAddressLine1 = "ADDRESS 1"
			};
			var inpn02 = new INPN02()
			{
				EntitysAddressLine = "ADDRESS SINGLE",
				EntitysAddressLine1 = "ADDRESS LINE 1"
			};

			var list = new List<MessageBlock>();
			AssertMultilineASCIIEquals("Zero Item List", @"-----------------INPN03-----------------
 City Name (4-22)            :BOB
 State Province Code (23-24) :CA
 Postal Code (25-33)         :PO123
 Country Code (34-35)        :US
", serialiser.Serialise(true, null, list.AsReadOnly()));

			list.Add(inpn03);
			AssertMultilineASCIIEquals("Single Item List", @"-----------------INPN03-----------------
 City Name (4-22)            :BOB
 State Province Code (23-24) :CA
 Postal Code (25-33)         :PO123
 Country Code (34-35)        :US
", serialiser.Serialise(true, null, list.AsReadOnly()));

			list.Insert(0, inpn02);
			AssertMultilineASCIIEquals("2 Items List", @"-----------------INPN03-----------------
 City Name (4-22)            :BOB
 State Province Code (23-24) :CA
 Postal Code (25-33)         :PO123
 Country Code (34-35)        :US
", serialiser.Serialise(true, null, list.AsReadOnly()));

			list.Insert(0, inpn01);
			AssertMultilineASCIIEquals("N01 List", @"--------------INPN03ForN01--------------
 Notify Party Telephone Or Telex Number (4-38) :BOB                CAPO123    US
", serialiser.Serialise(true, null, list.AsReadOnly()));

			list.Remove(inpn01);

			list.Insert(0, inpn00);
			AssertMultilineASCIIEquals("N00 List", @"-----------------INPN03-----------------
 City Name (4-22)            :BOB
 State Province Code (23-24) :CA
 Postal Code (25-33)         :PO123
 Country Code (34-35)        :US
", serialiser.Serialise(true, null, list.AsReadOnly()));

			list.Insert(0, inpn01);
			AssertMultilineASCIIEquals("N01 BEFORE N00 List", @"-----------------INPN03-----------------
 City Name (4-22)            :BOB
 State Province Code (23-24) :CA
 Postal Code (25-33)         :PO123
 Country Code (34-35)        :US
", serialiser.Serialise(true, null, list.AsReadOnly()));
		}
	}
}
