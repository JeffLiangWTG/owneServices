using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class CommunicationTest : TestCaseWithFactory
	{
		public void TestPhoneNumbersStripNonNumericCharacters()
		{
			string phoneNo = "+61 2 9001 5982";
			var testComms = new Communication(phoneNo, CommunicationTypeList.Codes.AL);
			AssertEquals("TSW system will only accept numeric Telephone numbers in the message, no formatting", "61290015982", testComms.ContactDetail);
		}

		public void TestCommsDetails()
		{
			Organisation.OH_FullName = "Test OrgHeader";
			Organisation.MainAddress.OA_Address1 = "100 Main St.";
			Organisation.MainAddress.OA_City = "Sydney";
			Organisation.MainAddress.OA_State = "NSW";
			Organisation.MainAddress.OA_PostCode = "2000";
			Organisation.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			Organisation.MainAddress.OA_Phone = "+61 2 80012201";
			Organisation.MainAddress.OA_Fax = "+61 2 99999999";
			Organisation.MainAddress.OA_Email = "admin@Organisation.com";
			var tswCode = Organisation.CustomsCodes.AddNew();
			tswCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			tswCode.OK_CustomsRegNo = "372845965J";
			AssertNotNull("OrgHeaderWrapper", OrgWrapper);
			AssertEquals("CustomsClientCode", "372845965", OrgWrapper.CustomsClientCode);
			AssertEquals("Name", "Test OrgHeader", OrgWrapper.Name);
			AssertEquals("ItemValue", "Sydney", OrgWrapper.City);
			AssertEquals("CountryCode", "AU", OrgWrapper.CountryCode);
			AssertEquals("CountryRegion", "NSW", OrgWrapper.CountryRegion);
			AssertEquals("PostCode", "2000", OrgWrapper.PostCode);
			AssertEquals("Street", "100 Main St.", OrgWrapper.Address);
			var commsCount = 0;
			foreach (ICommunication comm in OrgWrapper.Communications)
			{
				commsCount++;
				if (comm.ContactType == CommunicationTypeList.Codes.TE)
				{
					AssertEquals("Comms - phone", "61280012201", comm.ContactDetail);
				}
				else if (comm.ContactType == CommunicationTypeList.Codes.EM)
				{
					AssertEquals("Comms - email", "admin@Organisation.com", comm.ContactDetail);
				}
				else if (comm.ContactType == CommunicationTypeList.Codes.FX)
				{
					AssertEquals("Comms - fax", "61299999999", comm.ContactDetail);
				}
			}

			AssertEquals("Communications count", 3, commsCount);
		}

		#region Implementation
		IOrganisation OrgWrapper
		{
			get
			{
				if (fOrgWrapper == null)
				{
					fOrgWrapper = OrgHeaderWrapper.New(Organisation);
				}

				return fOrgWrapper;
			}
		}
		IOrganisation fOrgWrapper;

		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.NewWithValidTestData<OrgHeader>();
				}

				return fOrganisation;
			}
		}
		OrgHeader fOrganisation;
		#endregion
	}
}
