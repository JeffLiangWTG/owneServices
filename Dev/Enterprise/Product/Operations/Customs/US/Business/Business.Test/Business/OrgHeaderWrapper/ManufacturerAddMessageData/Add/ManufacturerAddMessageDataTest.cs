using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ManufacturerAddMessageData))]
	internal class ManufacturerAddMessageDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReadonly()
		{
			AssertPropertyReadOnly(AutoManufacturerAddMessageData.Schema.US_City, true);
			AssertPropertyReadOnly(AutoManufacturerAddMessageData.Schema.US_Country, true);
			AssertPropertyReadOnly(AutoManufacturerAddMessageData.Schema.US_FirmName, true);
			AssertPropertyReadOnly(AutoManufacturerAddMessageData.Schema.US_MID, false);
			AssertPropertyReadOnly(AutoManufacturerAddMessageData.Schema.US_OA_AddressDetails, false);
			AssertPropertyReadOnly(AutoManufacturerAddMessageData.Schema.US_Street, true);
			AssertPropertyReadOnly(AutoManufacturerAddMessageData.Schema.US_Zip, true);
		}

		void AssertPropertyReadOnly(string propertyName, bool shouldBeReadOnly)
		{
			System.ComponentModel.ReadOnlyAttribute[] attributes = (System.ComponentModel.ReadOnlyAttribute[])typeof(AutoManufacturerAddMessageData).GetProperty(propertyName).GetCustomAttributes(typeof(System.ComponentModel.ReadOnlyAttribute), false);
			if (shouldBeReadOnly)
			{
				AssertNotEquals(0, attributes.Length);
				AssertEquals(true, attributes[0].IsReadOnly);
			}
			else
			{
				AssertEquals(0, attributes.Length);
			}
		}

		public void TestSetDefaultValuesWithWrapper()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ABC Company";
			organisation.MainAddress.OA_Address1 = "978 Main Road";
			organisation.MainAddress.OA_Address2 = "(back of Eat me restaurant)";
			organisation.MainAddress.OA_City = "Vancouver";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "CAVAN";
			organisation.MainAddress.OA_State = "AB";
			organisation.MainAddress.OA_PostCode = "1B1 A3B";

			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(organisation);
			ManufacturerAddMessageData messageData = new ManufacturerAddMessageData(wrapper);

			AssertEquals("ABC COMPANY", messageData.US_FirmName);
			AssertEquals("978 MAIN ROAD (BACK OF EAT ME RESTAURANT)", messageData.US_Street);
			AssertEquals("VANCOUVER", messageData.US_City);
			AssertEquals(CanadaProvinceTerritoryCodes.Codes.XA, messageData.US_Country);
			AssertEquals("postCode", "1B1A3B", messageData.US_Zip);

			organisation.MainAddress.OA_State = "";
			messageData = new ManufacturerAddMessageData(wrapper);
			AssertEquals("CA", messageData.US_Country);
		}

		public void TestIManufacturerAdd()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ABC import pty ltd";
			organisation.MainAddress.OA_Address1 = "1234 main road";
			organisation.MainAddress.OA_City = "Vancouver";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "CAVAN";
			organisation.MainAddress.OA_State = "AB";
			organisation.MainAddress.OA_PostCode = "1B1- A3B";

			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(organisation);
			ManufacturerAddMessageData messageData = new ManufacturerAddMessageData(wrapper);

			IManufacturerAdd idata = messageData;
			AssertEquals("FirmName", "ABC IMPORT PTY LTD", idata.FirmName);
			AssertEquals("Street", "1234 MAIN ROAD", idata.Street);
			AssertEquals("City", "VANCOUVER", idata.City);
			AssertEquals("Country", "XA", idata.Country);
			AssertEquals("ZIPCODE:SPACE AND - SHOULD HAVE BEEN REMOVED", "1B1A3B", idata.Zip);
			AssertEquals(false, idata.MID.IsEmpty);
			AssertEquals("AddressPK", organisation.MainAddress.PK, idata.AddressPK);
		}

		public void TestSetDefaultValuesWithWrapperForBU()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ABC import pty ltd";
			organisation.MainAddress.OA_Address1 = "1234 main road";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "BUAKY";
			organisation.MainAddress.OA_RN_NKCountryCode = USCCountry.Burma;

			var wrapper = OrgHeaderWrapper.New(organisation);
			var messageData = new ManufacturerAddMessageData(wrapper);

			AssertEquals("Country", USCCountry.Burma, messageData.US_Country);
			AssertEquals("Country", USCCountry.Burma, ((IAddressDetails)messageData).Country);
		}

		public void TestSetDefaultValuesWithWrapperForMM()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ABC import pty ltd";
			organisation.MainAddress.OA_Address1 = "1234 main road";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "MMAKY";
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Myanmar;

			var wrapper = OrgHeaderWrapper.New(organisation);
			var messageData = new ManufacturerAddMessageData(wrapper);

			AssertEquals("Country", Core.Constants.CountryCodes.Myanmar, messageData.US_Country);
			AssertEquals("Country", Core.Constants.CountryCodes.Myanmar, ((IAddressDetails)messageData).Country);
		}

		public void TestMIDWithSecondAddressNotEmpty()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "DRUMARKON INTERNATIONAL BV";
			organisation.MainAddress.OA_Address1 = "SPORTLAAN 1A";
			organisation.MainAddress.OA_Address2 = "4209 AX";
			organisation.MainAddress.OA_City = "SCHELLUINEN";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "NLSLN";
			organisation.MainAddress.OA_PostCode = "12222";

			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(organisation);
			ManufacturerAddMessageData messageData = new ManufacturerAddMessageData(wrapper);

			IManufacturerAdd idata = messageData;
			AssertEquals("Street", "SPORTLAAN 1A 4209 AX", idata.Street);
			AssertEquals("MID", "NLDRUINT4209SCH", idata.MID);
		}

		public void TestMIDWithslashIncludedInCompanyName()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Nivedita CD/DVD producties";
			organisation.MainAddress.OA_Address1 = "309 SPORTLAAN";
			organisation.MainAddress.OA_Address2 = "309 AX";
			organisation.MainAddress.OA_City = "SNE";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "NLSLN";
			organisation.MainAddress.OA_PostCode = "3091";

			var wrapper = OrgHeaderWrapper.New(organisation);
			var messageData = new ManufacturerAddMessageData(wrapper);

			IManufacturerAdd idata = messageData;
			AssertEquals("MID", "NLNIVCD309SNE", idata.MID);

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_FullName = "solune/vanessa bruno";
			organisation2.MainAddress.OA_Address1 = "100, avenue du general leclerc";
			organisation2.MainAddress.OA_Address2 = "attn: akilan - local 0633";
			organisation2.MainAddress.OA_City = "pantin";
			organisation2.MainAddress.OA_RL_NKRelatedPortCode = "FRTIN";
			organisation2.MainAddress.OA_PostCode = "93500";

			var wrapper2 = OrgHeaderWrapper.New(organisation2);
			var messageData2 = new ManufacturerAddMessageData(wrapper2);

			IManufacturerAdd idata2 = messageData2;
			AssertEquals("MID", "FRSOLVAN0633PAN", idata2.MID);
		}

		public void TestFullCompanyNameForDollar2Block()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			organisation.OH_FullName = "ALPHANTRANS INTERNATIONAL555554444444444555555555566666666667777777777888888888899999999991111111111";
			organisation.MainAddress.OA_Address1 = "215-5000 MILLER ROAD";
			organisation.MainAddress.OA_City = "VANCOUVER AIRPORT 901234";
			organisation.MainAddress.OA_PostCode = "V7A 4E9";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "CAYVR";
			organisation.MainAddress.OA_State = "AB";

			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(organisation);
			ManufacturerAddMessageData messageData = new ManufacturerAddMessageData(wrapper);
			IManufacturerAdd manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("Long Company Name", organisation.OH_FullName, manufacturerAdd.FirmName);
		}

		public void TestCorrectCommonErrors()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ABC import pty ltd";
			organisation.MainAddress.OA_Address1 = "1234 main road";
			organisation.MainAddress.OA_State = "AB";
			organisation.MainAddress.OA_PostCode = "1B1 A3B";

			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(organisation);

			#region common errors with city name

			organisation.MainAddress.OA_City = WrongHongKongCity;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "HKABC";
			IManufacturerAdd manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.HongKongCity, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", WrongHongKongCity, manufacturerAdd.City);

			organisation.MainAddress.OA_City = WrongMacaoCity;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "MOABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.MacaoCity, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", WrongMacaoCity, manufacturerAdd.City);

			organisation.MainAddress.OA_City = WrongSingaporeCity;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "SGABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.SingaporeCity, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", WrongSingaporeCity, manufacturerAdd.City);

			organisation.MainAddress.OA_City = WrongVaticanCity;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "VAABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.VaticanCity, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", WrongVaticanCity, manufacturerAdd.City);

			organisation.MainAddress.OA_City = WrongMonacoCity;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "MCABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.MonacoCity, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", WrongMonacoCity, manufacturerAdd.City);

			organisation.MainAddress.OA_City = WrongSanMarinoCity;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "SMABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.SanMarinoCity, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", WrongSanMarinoCity, manufacturerAdd.City);

			organisation.MainAddress.OA_City = WrongAndorraCity;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "ADABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.AndorraCity, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", WrongAndorraCity, manufacturerAdd.City);

			organisation.MainAddress.OA_City = ManufacturerAddMessageData.WrongViennaName;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "ATABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.CorrectViennaName, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", ManufacturerAddMessageData.WrongViennaName, manufacturerAdd.City);

			organisation.MainAddress.OA_City = ManufacturerAddMessageData.WrongMunichName;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "DEABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.CorrectMunichName, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", ManufacturerAddMessageData.WrongMunichName, manufacturerAdd.City);

			organisation.MainAddress.OA_City = ManufacturerAddMessageData.WrongCologneName1;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "DEABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.CorrectCologneName, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", ManufacturerAddMessageData.WrongCologneName1, manufacturerAdd.City);

			organisation.MainAddress.OA_City = ManufacturerAddMessageData.WrongCologneName2;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "DEABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.CorrectCologneName, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", ManufacturerAddMessageData.WrongCologneName2, manufacturerAdd.City);

			organisation.MainAddress.OA_City = ManufacturerAddMessageData.WrongFlorenceName;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "ITABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.CorrectFlorenceName, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", ManufacturerAddMessageData.WrongFlorenceName, manufacturerAdd.City);

			organisation.MainAddress.OA_City = ManufacturerAddMessageData.WrongRangoonName;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "MMABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should be corrected", ManufacturerAddMessageData.CorrectRangoonName, manufacturerAdd.City);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("City should NOT be corrected", ManufacturerAddMessageData.WrongRangoonName, manufacturerAdd.City);

			#endregion

			#region common errors with country

			organisation.MainAddress.OA_City = BelfastCity;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "IEABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("Country should be corrected", Core.Constants.CountryCodes.UnitedKingdom, manufacturerAdd.Country);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "AUABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("Country should NOT be corrected", Core.Constants.CountryCodes.Australia, manufacturerAdd.Country);

			organisation.MainAddress.OA_City = LisburnCity;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "IEABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("Country should be corrected", Core.Constants.CountryCodes.UnitedKingdom, manufacturerAdd.Country);

			organisation.MainAddress.OA_RL_NKRelatedPortCode = "AUABC";
			manufacturerAdd = new ManufacturerAddMessageData(wrapper);
			AssertEquals("Country should NOT be corrected", Core.Constants.CountryCodes.Australia, manufacturerAdd.Country);

			#endregion
		}
		const string WrongHongKongCity = "1HONG KONG";
		const string WrongMacaoCity = "1MACAO";
		const string WrongSingaporeCity = "1SINGAPORE";
		const string WrongVaticanCity = "1VATICAN";
		const string WrongMonacoCity = "1MONACO";
		const string WrongSanMarinoCity = "1SAN MARINO";
		const string WrongAndorraCity = "1ANDORRA";

		const string BelfastCity = "Belfast";
		const string LisburnCity = "Lisburn";

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(organisation);
			return new ManufacturerAddMessageData(wrapper);
		}
	}
}
