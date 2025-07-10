using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	[TestedType(typeof(DpsEDIMessage))]
	public class DpsEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateMessageWithDefaultValues()
		{
			var ediMessagenum = Env.NumberFountains.DpsEDIMessageNumber.PeekPreliminaryFormatted(Factory);
			var ediMessage = Factory.New<DpsEDIMessage>();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(ApplicationCodeList.Codes.DPSRequestMessage, ediMessage.EM_ApplicationCode);
				AssertEquals(EDIMessageTypeList.Codes.JDC, ediMessage.EM_MessageType);
				AssertEquals(EDIMessageSubTypeList.Codes.ScreeningRequest, ediMessage.EM_MessageSubType);
				AssertEquals(EDIInterchange.Direction.Transmit, ediMessage.EM_ReceiveTransmit);
				AssertEquals(EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
				AssertEquals(ediMessagenum, ediMessage.EM_MessageNum);
				AssertEquals(20, ediMessage.EM_MessageNum.Length);
				AssertEquals("DPS", ediMessage.EM_MessageNum.Substring(0, 3));
			});
		}

		public void TestCreateMessageWithDpsEDIMessageContent()
		{
			var nameCandidate = new List<DpsNameCandidate> { new DpsNameCandidate { NameType = "ORG", FullName = "WiseTech Global" } };
			var addressCandidate = new List<DpsAddressCandidate> { new DpsAddressCandidate { Address1 = "Address1", Address2 = "Address2", City = "City", State = "State", PostCode = "PostCode", Country = "Country", AdditionalAddressLine = "ExtraAddresLine" } };
			var registrationCandidate = new List<DpsRegistrationCodeCandidate> { new DpsRegistrationCodeCandidate { RegCountryCode = "AU", RegCodeType = "Passport", RegCodeValue = "9873-6059" } };
			var countryCandidate = new List<DpsCountryCandidate> { new DpsCountryCandidate { CountryCode = "AU" } };
			var clientSpecifiedIdentifier = Guid.NewGuid();
			var databaseType = ObjectFactory.Get<IProductRegistration>().Key.DatabaseType;
			var dpsMessage = Factory.New<DpsEDIMessage>();
			dpsMessage.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(
				new DpsEDIMessageContent
				{
					ClientLicence = "CW1-ENT-EDI",
					DatabaseType = databaseType,
					ClientSpecifiedIdentifier = clientSpecifiedIdentifier,
					EntityType = "OH",
					PersistentStatus = "UNK",
					ScreenTime = DateTime.Today,
					DpsRequestHeader = new DpsRequestHeader { DpsNameCandidates = nameCandidate, DpsAddressCandidates = addressCandidate, DpsRegistrationCodeCandidates = registrationCandidate, DpsCountryCandidates = countryCandidate, ClientLicence = "CW1-ENT-EDI", UserName = "CW1" }
				},
				Formatting.None));

			Factory.Save();

			var queryMessage = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DPSRequestMessage) { FetchOnlyFromLocalCache = true });
			AssertEquals(1, queryMessage.Length);
			var content = JsonConvert.DeserializeObject<DpsEDIMessageContent>(queryMessage[0].EM_MessageData.ToUTF8());
			AssertNotNull(content);
			AssertEquals(clientSpecifiedIdentifier, content.ClientSpecifiedIdentifier);
			AssertEquals("CW1-ENT-EDI", content.ClientLicence);
			AssertEquals(databaseType, content.DatabaseType);
			AssertEquals("OH", content.EntityType);
			AssertEquals("UNK", content.PersistentStatus);

			var name = content.DpsRequestHeader.DpsNameCandidates.FirstOrDefault();
			AssertEquals("ORG", name.NameType);
			AssertEquals("WiseTech Global", name.FullName);

			var address = content.DpsRequestHeader.DpsAddressCandidates.FirstOrDefault();
			AssertEquals("Address1", address.Address1);
			AssertEquals("Address2", address.Address2);
			AssertEquals("State", address.State);
			AssertEquals("PostCode", address.PostCode);
			AssertEquals("Country", address.Country);
			AssertEquals("ExtraAddresLine", address.AdditionalAddressLine);

			var regsCode = content.DpsRequestHeader.DpsRegistrationCodeCandidates.FirstOrDefault();
			AssertEquals("AU", regsCode.RegCountryCode);
			AssertEquals("Passport", regsCode.RegCodeType);
			AssertEquals("9873-6059", regsCode.RegCodeValue);

			var country = content.DpsRequestHeader.DpsCountryCandidates.FirstOrDefault();
			AssertEquals("AU", country.CountryCode);
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (DpsEDIMessage)GetNewBusinessObject();
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<DpsEDIMessage>();
		}
	}
}
