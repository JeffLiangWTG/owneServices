using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Tests.Fakes
{
	public class TestContextWithData
	{
		TestContext _context;

		protected TestContextWithData()
		{
		}

		public static TestContext Create()
		{
			var testContextWithData = new TestContextWithData();
			testContextWithData._context = new TestContext();
			testContextWithData.LoadData();
			return testContextWithData._context;
		}

		public static TestContext Create(Exception saveException)
		{
			var testContextWithData = new TestContextWithData();
			testContextWithData._context = new TestContext(saveException);
			testContextWithData.LoadData();
			return testContextWithData._context;
		}

		public void LoadData()
		{
			LoadeHubClients();
			LoadEdiProdClients();
			LoadeHubMessageTypes();
			LoadeHubClientSystems();
			LoadeHubTransformationSets();
			LoadeHubCodeSets();
			LoadeHubCodeSetResults();
			LoadeHubCodeMapKeys();
			LoadeHubCodeMapValues();
			LoadeHubAirConnection();
			LoadeHubAirConnectionPerBranch();
			LoadeHubUSCustomsRegistry();
			LoadeHubMessageReferenceRegistry();
			LoadeHubSubscriptionTypes();
			LoadeHubSubscriptionAutoSubscribes();
			LoadeHubSubscriptionLookups();
			LoadeHubSubscriptionValues();
			LoadeHubSubscriptionBroadcasters();
		}

		public void ReloadData()
		{
			LoadeHubClients();
			LoadEdiProdClients();
			LoadeHubClientSystems();
			LoadeHubMessageTypes();
			LoadeHubTransformationSets();
			LoadeHubCodeSets();
			LoadeHubCodeSetResults();
			LoadeHubCodeMapKeys();
			LoadeHubCodeMapValues();
			LoadeHubAirConnection();
			LoadeHubAirConnectionPerBranch();
			LoadeHubUSCustomsRegistry();
			LoadeHubMessageReferenceRegistry();
			LoadeHubSubscriptionTypes();
			LoadeHubSubscriptionAutoSubscribes();
			LoadeHubSubscriptionLookups();
			LoadeHubSubscriptionValues();
			LoadeHubSubscriptionBroadcasters();
		}

		public void LoadeHubClients()
		{
			_context.eHubClients.AddObject(NeweHubClient("{00000000-AAAA-1111-0000-000000000000}", "TEST0001", "Test Client 1", "TestAS2Code1", false));
			_context.eHubClients.AddObject(NeweHubClient("{00000000-AAAA-2222-0000-000000000000}", "TEST0002", "Test Client 2", "TestAS2Code2", true));
			_context.eHubClients.AddObject(NeweHubClient("{00000000-AAAA-3333-0000-000000000000}", "TEST0003", "Test Client 3", "TestAS2Code3", true));

			// Test Clients For PIMA Tests
			_context.eHubClients.AddObject(NeweHubClient("{00000000-AAAA-4444-0000-000000000000}", "CLIENT0004", "Client 4", "TestAS2Code4", false));
			_context.eHubClients.AddObject(NeweHubClient("{00000000-AAAA-5555-0000-000000000000}", "CLIENT0005", "Client 4", "TestAS2Code5", true));

			_context.eHubClients.AddObject(NeweHubClient("{00000000-AAAA-6666-0000-000000000000}", "CLIENT0006", "Client 6", "", false));
			_context.eHubClients.AddObject(NeweHubClient("{00000000-AAAA-7777-0000-000000000000}", "CLIENT0007", "Client 7", "", false));
			_context.eHubClients.AddObject(NeweHubClient("{00000000-AAAA-8888-0000-000000000000}", "CLIENT0008", "Client 8", "", false));
		}

		public void LoadEdiProdClients()
		{
			_context.eHubClients.AddObject(NeweHubClient("{10000000-AAAA-6666-0000-000000000000}", "WTLPRDSV1", "Prod System 1", "", false, "Client", "Enterprise"));
			_context.eHubClients.AddObject(NeweHubClient("{10000000-AAAA-7777-0000-000000000000}", "WTLTSTSV2", "Test System 2", "", false, "Client", "Enterprise"));

			_context.ediProdClients.AddObject(NewEdiProdClient("PRD", "WTL", "SV1", "WTLSV1", "WTLPRDSV1", "{10000000-AAAA-6666-0000-000000000000}"));
			_context.ediProdClients.AddObject(NewEdiProdClient("TST", "WTL", "SV2", "WTLSV2", "WTLTSTSV2", "{10000000-AAAA-7777-0000-000000000000}"));
		}

		public void LoadeHubClientSystems()
		{
			var utcDateTime = new DateTime(2022, 09, 26, 10, 05, 20);
			_context.eHubClientSystems.AddObject(NeweHubClientSystem("{00000000-EEEE-1111-0000-000000000000}", "Test WTLEHK1", utcDateTime));
			_context.eHubClientSystems.AddObject(NeweHubClientSystem("{00000000-EEEE-2222-0000-000000000000}", "Test WTLEHK2", utcDateTime.AddDays(-5)));
			_context.eHubClientSystems.AddObject(NeweHubClientSystem("{00000000-EEEE-3333-0000-000000000000}", "WTLEHK3", utcDateTime.AddMinutes(-5)));
			_context.eHubClientSystems.AddObject(NeweHubClientSystem("{00000000-EEEE-4444-0000-000000000000}", "WTLEHK4", utcDateTime.AddHours(-5)));
			_context.eHubClientSystems.AddObject(NeweHubClientSystem("{00000000-EEEE-5555-0000-000000000000}", "WTLSV1", utcDateTime.AddHours(-5)));
			_context.eHubClientSystems.AddObject(NeweHubClientSystem("{00000000-EEEE-6666-0000-000000000000}", "WTLSV2", utcDateTime.AddHours(-5)));
		}

		public void LoadeHubMessageTypes()
		{
			_context.eHubMessageTypes.AddObject(NeweHubMessageType("{00000000-EEEE-1111-0000-000000000000}", "MSG0001", false, false, null, null, null));
			_context.eHubMessageTypes.AddObject(NeweHubMessageType("{00000000-EEEE-2222-0000-000000000000}", "MSG0002", false, false, null, null, null));
		}

		public void LoadeHubTransformationSets()
		{
			_context.eHubTransformationSets.AddObject(NeweHubTransformationSet("{00000000-BBBB-1111-0000-000000000000}", "Test Transformation 1", "TEST0001", "TEST0002"));
			_context.eHubTransformationSets.AddObject(NeweHubTransformationSet("{00000000-BBBB-4444-0000-000000000000}", "Test Transformation 4", "TEST0001", "TEST0002"));
			_context.eHubTransformationSets.AddObject(NeweHubTransformationSet("{00000000-BBBB-5555-0000-000000000000}", "Test Transformation 5", "TEST0001", "TEST0002"));
		}

		public void LoadeHubCodeSets()
		{
			_context.eHubCodeSets.AddObject(NeweHubCodeSet("{00000000-CCCC-1111-0000-000000000000}", "Code Set 1", "TEST0001", "TEST0002", "Test Transformation 1", "Input Code", null, null, null, null));
			_context.eHubCodeSets.AddObject(NeweHubCodeSet("{00000000-CCCC-2222-0000-000000000000}", "Code Set 2", "TEST0001", "TEST0002", "Test Transformation 1", null, null, null, null, null));
			_context.eHubCodeSets.AddObject(NeweHubCodeSet("{00000000-CCCC-3333-0000-000000000000}", "Code Set 3", "TEST0001", "TEST0002", "Test Transformation 1", "Key 1", "Key 2", "Key 3", null, null));
			_context.eHubCodeSets.AddObject(NeweHubCodeSet("{00000000-CCCC-4444-0000-000000000000}", "Code Set 4", "TEST0001", "TEST0002", "Test Transformation 4", "Key 1", "Key 2", "Key 3", null, null));
			_context.eHubCodeSets.AddObject(NeweHubCodeSet("{00000000-CCCC-9999-0000-000000000000}", "Code Set Unassigned", "TEST0001", "TEST0002", null, "Key 1", null, null, null, null));
			_context.eHubCodeSets.AddObject(NeweHubCodeSet("{00000000-CCCC-5555-0000-000000000000}", "Code Set 5", "TEST0001", "TEST0002", "Test Transformation 5", "Key", null, null, null, null));
		}

		public void LoadeHubCodeSetResults()
		{
			_context.eHubCodeSetResults.AddObject(NeweHubCodeSetResult("{00000000-DDDD-1111-1111-000000000000}", "Code Set 1", 1, "Result 1"));
			_context.eHubCodeSetResults.AddObject(NeweHubCodeSetResult("{00000000-DDDD-2222-1111-000000000000}", "Code Set 2", 1, "Config Value 1"));
			_context.eHubCodeSetResults.AddObject(NeweHubCodeSetResult("{00000000-DDDD-3333-1111-000000000000}", "Code Set 3", 1, "Result 1"));
			_context.eHubCodeSetResults.AddObject(NeweHubCodeSetResult("{00000000-DDDD-3333-2222-000000000000}", "Code Set 3", 2, "Result 2"));
			_context.eHubCodeSetResults.AddObject(NeweHubCodeSetResult("{00000000-DDDD-4444-1111-000000000000}", "Code Set 4", 1, "Result 1"));
			_context.eHubCodeSetResults.AddObject(NeweHubCodeSetResult("{00000000-DDDD-4444-2222-000000000000}", "Code Set 4", 2, "Result 2"));
			_context.eHubCodeSetResults.AddObject(NeweHubCodeSetResult("{00000000-DDDD-4444-3333-000000000000}", "Code Set 4", 3, "Result 3"));
			_context.eHubCodeSetResults.AddObject(NeweHubCodeSetResult("{00000000-DDDD-9999-1111-000000000000}", "Code Set Unassigned", 1, "Result 1"));
			_context.eHubCodeSetResults.AddObject(NeweHubCodeSetResult("{00000000-DDDD-5555-1111-000000000000}", "Code Set 5", 1, "Value"));
			_context.eHubCodeSetResults.AddObject(NeweHubCodeSetResult("{00000000-DDDD-5555-2222-000000000000}", "Code Set 5", 2, "Fallback Value"));
		}

		public void LoadeHubCodeMapKeys()
		{
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-1111-1111-000000000000}", "Code Set 1", 1, "AAA", null, null, null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-1111-2222-000000000000}", "Code Set 1", 2, "BBB", null, null, null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-1111-3333-000000000000}", "Code Set 1", 3, "CCC", null, null, null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-1111-FFFF-000000000000}", "Code Set 1", 4, "%", null, null, null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-2222-1111-000000000000}", "Code Set 2", 1, null, null, null, null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-3333-1111-000000000000}", "Code Set 3", 1, "XXX", "AA", "111%", null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-3333-2222-000000000000}", "Code Set 3", 2, "XXX", "AA", "%222", null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-3333-3333-000000000000}", "Code Set 3", 3, "XXX", "AA", "%333%", null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-3333-4444-000000000000}", "Code Set 3", 4, "XXX", "%", "%", null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-3333-5555-000000000000}", "Code Set 3", 5, "%", "%", "%", null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-9999-1111-000000000000}", "Code Set Unassigned", 1, "XXX", null, null, null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-9999-2222-000000000000}", "Code Set Unassigned", 2, "YYY", null, null, null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-9999-3333-000000000000}", "Code Set Unassigned", 3, "", null, null, null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-5555-1111-000000000000}", "Code Set 5", 1, "Provider1", null, null, null, null));
			_context.eHubCodeMapKeys.AddObject(NeweHubCodeMapKey("{00000000-EEEE-5555-2222-000000000000}", "Code Set 5", 2, "%", null, null, null, null));
		}

		public void LoadeHubCodeMapValues()
		{
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 1", 1, "Result 1", "111", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 1", 2, "Result 1", "121", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 1", 3, "Result 1", "131", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 1", 4, "Result 1", null, 1));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 2", 1, "Config Value 1", "12345 67890", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 3", 1, "Result 1", "311", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 3", 2, "Result 1", "321", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 3", 3, "Result 1", "331", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 3", 4, "Result 1", "341", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 3", 5, "Result 1", null, 1));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 3", 1, "Result 2", "312", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 3", 2, "Result 2", "322", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 3", 3, "Result 2", "332", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 3", 4, "Result 2", "342", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 3", 5, "Result 2", null, 2));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set Unassigned", 1, "Result 1", "X11", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set Unassigned", 2, "Result 1", "X21", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set Unassigned", 3, "Result 1", "", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 5", 1, "Value", "SI", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 5", 1, "Fallback Value", "ZZZ", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 5", 2, "Value", "SI", null));
			_context.eHubCodeMapValues.AddObject(NeweHubCodeMapValue("Code Set 5", 2, "Fallback Value", "", null));
		}

		public void LoadeHubAirConnection()
		{
			_context.eHubAirConnections.AddObject(NeweHubAirConnection("{00000000-AAAA-1111-0000-000000000000}", "{00000000-AAAA-2222-0000-000000000000}", "TestPIMA1", "TestPassword2"));
			_context.eHubAirConnections.AddObject(NeweHubAirConnection("{00000000-AAAA-1111-0000-000000000000}", "{00000000-AAAA-5555-0000-000000000000}", "TestPIMA1", "TestPassword2"));
			_context.eHubAirConnections.AddObject(NeweHubAirConnection("{00000000-AAAA-4444-0000-000000000000}", "{00000000-AAAA-5555-0000-000000000000}", "TestPIMA2", "TestPassword2"));
		}

		public void LoadeHubAirConnectionPerBranch()
		{
			_context.eHubAirConnectionPerBranches.AddObject(NeweHubAirConnectionPerBranch("{00000000-AAAA-1111-0000-000000000000}", "{00000000-AAAA-2222-0000-000000000000}", "TestIATA1", "TestPIMAPerBranch1", "TestPasswordPerBranch1"));
			_context.eHubAirConnectionPerBranches.AddObject(NeweHubAirConnectionPerBranch("{00000000-AAAA-1111-0000-000000000000}", "{00000000-AAAA-5555-0000-000000000000}", "TestIATA1", "TestPIMAPerBranch1", "TestPasswordPerBranch1"));
			_context.eHubAirConnectionPerBranches.AddObject(NeweHubAirConnectionPerBranch("{00000000-AAAA-4444-0000-000000000000}", "{00000000-AAAA-5555-0000-000000000000}", "TestIATA2", "TestPIMAPerBranch2", "TestPasswordPerBranch2"));
		}

		public void LoadeHubUSCustomsRegistry()
		{
			var client1 = NeweHubClient("{5A5F369A-907B-4612-836E-44C6D76C069B}", "CLIENT_USCUST", "Client US Customs", "CLIENT_USCUST_AS2", false);
			var client2 = NeweHubClient("{5A5F369A-907B-4612-836E-44C6D76C069A}", "CLIENT_USCUST2", "Client US Customs 2", "CLIENT_USCUST_AS2_2", false);

			_context.eHubClients.AddObject(client1);
			_context.eHubClients.AddObject(client2);

			var man = NeweHubUSCustomsRegistry("{BF07FC57-EA37-4BF0-A06C-DCBEB40DFF1A}", "{5A5F369A-907B-4612-836E-44C6D76C069B}", "MAN", "Client Network ID", "MAN_VALUE", false);
			var man_2 = NeweHubUSCustomsRegistry("{BF07FC57-EA37-4BF0-A06C-DCBEB40DFF1A}", "{5A5F369A-907B-4612-836E-44C6D76C069A}", "MAN", "Client Network ID", "MAN_VALUE", false);
			_context.eHubUSCustomsRegistry.AddObject(man);
			_context.eHubUSCustomsRegistry.AddObject(man_2);
			client1.eHubUSCustomsRegistry.Add(man);
			client2.eHubUSCustomsRegistry.Add(man_2);

			var man2 = NeweHubUSCustomsRegistry("{BF07FC57-EA37-4BF0-A06C-DCBEB40DFF1B}", "{5A5F369A-907B-4612-836E-44C6D76C069B}", "MAN", "Client Network ID", "MAN_VALUE_2", false);
			var man2_2 = NeweHubUSCustomsRegistry("{BF07FC57-EA37-4BF0-A06C-DCBEB40DFF1B}", "{5A5F369A-907B-4612-836E-44C6D76C069A}", "MAN", "Client Network ID", "MAN_VALUE_2", false);
			_context.eHubUSCustomsRegistry.AddObject(man2);
			_context.eHubUSCustomsRegistry.AddObject(man2_2);
			client1.eHubUSCustomsRegistry.Add(man2);
			client2.eHubUSCustomsRegistry.Add(man2_2);

			var ams = NeweHubUSCustomsRegistry("{F8680D43-B46E-4F73-B73B-E14B1995992A}", "{5A5F369A-907B-4612-836E-44C6D76C069B}", "AMS", "AMS_NAME", "AMS_VALUE", false);
			var ams2 = NeweHubUSCustomsRegistry("{F8680D43-B46E-4F73-B73B-E14B1995992A}", "{5A5F369A-907B-4612-836E-44C6D76C069A}", "AMS", "AMS_NAME", "AMS_VALUE", false);
			_context.eHubUSCustomsRegistry.AddObject(ams);
			_context.eHubUSCustomsRegistry.AddObject(ams2);
			client1.eHubUSCustomsRegistry.Add(ams);
			client2.eHubUSCustomsRegistry.Add(ams2);

			var ama = NeweHubUSCustomsRegistry("{E853FBA0-1804-40BD-8114-1DA4428E6FBE}", "{5A5F369A-907B-4612-836E-44C6D76C069B}", "AMA", "AMA_NAME", "AMA_VALUE", false);
			var ama2 = NeweHubUSCustomsRegistry("{E853FBA0-1804-40BD-8114-1DA4428E6FBE}", "{5A5F369A-907B-4612-836E-44C6D76C069A}", "AMA", "AMA_NAME", "AMA_VALUE", false);
			_context.eHubUSCustomsRegistry.AddObject(ama);
			_context.eHubUSCustomsRegistry.AddObject(ama2);
			client1.eHubUSCustomsRegistry.Add(ama);
			client2.eHubUSCustomsRegistry.Add(ama2);

			var isf = NeweHubUSCustomsRegistry("{5C0D434C-952C-4AAA-8E14-3917BDA5DE9F}", "{5A5F369A-907B-4612-836E-44C6D76C069B}", "USI", "ISF User Data", "ISF_Value", false);
			var isf2 = NeweHubUSCustomsRegistry("{5C0D434C-952C-4AAA-8E14-3917BDA5DE9F}", "{5A5F369A-907B-4612-836E-44C6D76C069A}", "USI", "ISF User Data", "ISF_Value", false);
			_context.eHubUSCustomsRegistry.AddObject(isf);
			_context.eHubUSCustomsRegistry.AddObject(isf2);
			client1.eHubUSCustomsRegistry.Add(isf);
			client2.eHubUSCustomsRegistry.Add(isf2);

			var usi = NeweHubUSCustomsRegistry("{A2DBEE82-4C6D-4A84-AD43-5EFE42504B7A}", "{5A5F369A-907B-4612-836E-44C6D76C069B}", "USI", "Entry Filer Code", "USI_VALUE", false);
			var usi2 = NeweHubUSCustomsRegistry("{A2DBEE82-4C6D-4A84-AD43-5EFE42504B7A}", "{5A5F369A-907B-4612-836E-44C6D76C069A}", "USI", "Entry Filer Code", "USI_VALUE", false);
			_context.eHubUSCustomsRegistry.AddObject(usi);
			_context.eHubUSCustomsRegistry.AddObject(usi2);
			client1.eHubUSCustomsRegistry.Add(usi);
			client2.eHubUSCustomsRegistry.Add(usi2);

			var use = NeweHubUSCustomsRegistry("{0A37A1EE-FD2A-4D74-85DC-A11DEEC107D9}", "{5A5F369A-907B-4612-836E-44C6D76C069B}", "USE", "USE_NAME", "USE_VALUE", false);
			var use2 = NeweHubUSCustomsRegistry("{0A37A1EE-FD2A-4D74-85DC-A11DEEC107D9}", "{5A5F369A-907B-4612-836E-44C6D76C069A}", "USE", "USE_NAME", "USE_VALUE", false);
			_context.eHubUSCustomsRegistry.AddObject(use);
			_context.eHubUSCustomsRegistry.AddObject(use2);
			client1.eHubUSCustomsRegistry.Add(use);
			client2.eHubUSCustomsRegistry.Add(use2);

			var uem = NeweHubUSCustomsRegistry("{6830BB2D-0AA7-41B8-9418-20A4CE4A5DB6}", "{5A5F369A-907B-4612-836E-44C6D76C069B}", "UEM", "Carrier Code", "UEM_VALUE", false);
			var uem2 = NeweHubUSCustomsRegistry("{6830BB2D-0AA7-41B8-9418-20A4CE4A5DB6}", "{5A5F369A-907B-4612-836E-44C6D76C069A}", "UEM", "Carrier Code", "UEM_VALUE", false);
			_context.eHubUSCustomsRegistry.AddObject(uem);
			_context.eHubUSCustomsRegistry.AddObject(uem2);
			client1.eHubUSCustomsRegistry.Add(uem);
			client2.eHubUSCustomsRegistry.Add(uem2);

			var usd = NeweHubUSCustomsRegistry("{7875D4AC-100F-4BC8-A470-9BDE5D085E33}", "{FCCA6258-1A17-4844-BC68-970C217993AB}", "USD", "Entry Filer Code", "USD_VALUE", false);
			var usd2 = NeweHubUSCustomsRegistry("{7875D4AC-100F-4BC8-A470-9BDE5D085E33}", "{FCCA6258-1A17-4844-BC68-970C217993AB}", "USD", "Entry Filer Code", "USD_VALUE", false);
			_context.eHubUSCustomsRegistry.AddObject(usd);
			_context.eHubUSCustomsRegistry.AddObject(usd2);
			client1.eHubUSCustomsRegistry.Add(usd);
			client2.eHubUSCustomsRegistry.Add(usd2);
		}

		public void LoadeHubMessageReferenceRegistry()
		{
			var client = NeweHubClient("{07B420E9-9A5D-437C-B9F3-42B753950E4E}", "CLIENT_MESSAGE_REF", "Client Message Ref", "CLIENT_MESSAGE_REF_AS2", false);
			_context.eHubClients.AddObject(client);

			var messageReference = NeweHubMessageReferenceRegistry("{B6758272-6EE6-4F0D-83C6-0FA8008FC211}", "{07B420E9-9A5D-437C-B9F3-42B753950E4E}", "JPC", "JPC_MSG_REF", "JPC_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			messageReference = NeweHubMessageReferenceRegistry("{1F8A23E8-748E-44A2-93FA-B9A1FDA549CB}", "{07B420E9-9A5D-437C-B9F3-42B753950E4E}", "CMR", "CMR_MSG_REF", "CMR_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			client = NeweHubClient("{92F24EA0-F35A-4316-B23B-BFC90CDBC8BF}", "CLIENT_MESSAGE_REF_2", "Client Message Ref 2", "CLIENT_MESSAGE_REF_AS2_2", false);
			_context.eHubClients.AddObject(client);

			messageReference = NeweHubMessageReferenceRegistry("{CEE76ED4-DC2B-4EAF-A2E3-D4E923C025D9}", "{92F24EA0-F35A-4316-B23B-BFC90CDBC8BF}", "NZC", "NZC_MSG_REF", "NZC_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			messageReference = NeweHubMessageReferenceRegistry("{72551B19-BFF7-404B-A006-58CB1B0AFE79}", "{92F24EA0-F35A-4316-B23B-BFC90CDBC8BF}", "JPC", "JPC_MSG_REF", "JPC_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			messageReference = NeweHubMessageReferenceRegistry("{E1BF5B9C-76AF-443E-A5F5-8A7C97D62DA5}", "{92F24EA0-F35A-4316-B23B-BFC90CDBC8BF}", "CMR", "CMR_MSG_REF", "CMR_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			client = NeweHubClient("{AEACE3D8-4268-40B4-9577-93D44CDFADDE}", "CLIENT_MESSAGE_REF_3", "Client Message Ref 3", "CLIENT_MESSAGE_REF_AS2_3", false);
			_context.eHubClients.AddObject(client);

			messageReference = NeweHubMessageReferenceRegistry("{8F8F9D75-A0B5-41D6-BBA0-D669F946CB15}", "{AEACE3D8-4268-40B4-9577-93D44CDFADDE}", "NZC", "NZC_MSG_REF", "NZC_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			messageReference = NeweHubMessageReferenceRegistry("{A0B5F2B4-9B57-4F38-B51D-6322763BB4F9}", "{AEACE3D8-4268-40B4-9577-93D44CDFADDE}", "JPC", "JPC_MSG_REF", "JPC_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			messageReference = NeweHubMessageReferenceRegistry("{FE5DB2FC-672E-4862-BDFA-FD46DBA9C636}", "{AEACE3D8-4268-40B4-9577-93D44CDFADDE}", "CMR", "CMR_MSG_REF", "CMR_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			client = NeweHubClient("{8470CD71-99CB-4617-9E7D-FBE72F022ECF}", "CLIENT_MESSAGE_REF_4", "Client Message Ref 4", "CLIENT_MESSAGE_REF_AS2_4", false);
			_context.eHubClients.AddObject(client);

			messageReference = NeweHubMessageReferenceRegistry("{201EC886-5929-49E6-AAF5-0FDADD237523}", "{8470CD71-99CB-4617-9E7D-FBE72F022ECF}", "NZC", "NZC_MSG_REF", "NZC_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			messageReference = NeweHubMessageReferenceRegistry("{A47701EB-BBD0-41FB-8AFD-FE0EC8CA89B5}", "{8470CD71-99CB-4617-9E7D-FBE72F022ECF}", "JPC", "JPC_MSG_REF", "JPC_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			messageReference = NeweHubMessageReferenceRegistry("{726B879C-BEEE-4EE1-9C35-571F94A1BAA9}", "{8470CD71-99CB-4617-9E7D-FBE72F022ECF}", "CMR", "CMR_MSG_REF", "CMR_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);

			messageReference = NeweHubMessageReferenceRegistry("{CEE76ED4-DC2B-4EAF-A2E3-D4E923C025D9}", "{8470CD71-99CB-4617-9E7D-FBE72F022ECF}", "NZC", "NZC_MSG_REF", "NZC_MSG_PASS");
			_context.eHubMessageReferenceRegistries.AddObject(messageReference);
			client.eHubMessageReferenceRegistries.Add(messageReference);
		}

		public void LoadeHubSubscriptionTypes()
		{
			_context.eHubSubscriptionTypes.AddObject(NeweHubSubscriptionType("{00000000-AAAA-1111-0000-000000000000}", "SUB1", "Subscription 1", null));
			_context.eHubSubscriptionTypes.AddObject(NeweHubSubscriptionType("{00000000-AAAA-2222-0000-000000000000}", "SUB2", "Subscription 2", 30));
			_context.eHubSubscriptionTypes.AddObject(NeweHubSubscriptionType("{00000000-AAAA-3333-0000-000000000000}", "SUB3", "Subscription 3", null));
			_context.eHubSubscriptionTypes.AddObject(NeweHubSubscriptionType("{00000000-AAAA-5555-0000-000000000000}", "SUB5", "Subscription 5", null));
		}

		public void LoadeHubSubscriptionAutoSubscribes()
		{
			_context.eHubSubscriptionAutoSubscribes.AddObject(NeweHubSubscriptionAutoSubscribe("{00000000-BBBB-1111-0000-000000000000}", "SUB2", "MSG0001", null, "/*/VALUE", null, "/*/REFERENCE", null));
		}

		public void LoadeHubSubscriptionLookups()
		{
			_context.eHubSubscriptionLookups.AddObject(NeweHubSubscriptionLookup("{00000000-CCCC-1111-0000-000000000000}", "SUB1", "MSG0001", "/*/VALUE", null));
		}

		public void LoadeHubSubscriptionValues()
		{
			_context.eHubSubscriptionValues.AddObject(NeweHubSubscriptionValue("{00000000-DDDD-1111-1111-000000000000}", "SUB1", "TEST0001", "TEST0002", "VALUE1", null, DateTime.Parse("2014-01-01T12:00"), (DateTime?)null, null));
			_context.eHubSubscriptionValues.AddObject(NeweHubSubscriptionValue("{00000000-DDDD-1111-2222-000000000000}", "SUB1", "TEST0001", "TEST0003", "VALUE2", null, DateTime.Parse("2014-01-02T12:00"), (DateTime?)null, null));
			_context.eHubSubscriptionValues.AddObject(NeweHubSubscriptionValue("{00000000-DDDD-2222-1111-000000000000}", "SUB2", "TEST0001", "TEST0002", "VALUE3", "REF1", DateTime.Parse("2014-01-03T12:00"), DateTime.Parse("2014-02-03T12:00"), "REFTYPE1"));
			_context.eHubSubscriptionValues.AddObject(NeweHubSubscriptionValue("{00000000-DDDD-2222-2222-000000000000}", "SUB2", "TEST0001", "TEST0003", "VALUE4", "REF2", DateTime.Parse("2014-01-04T12:00"), DateTime.Parse("2014-02-04T12:00"), "REFTYPE2"));
			_context.eHubSubscriptionValues.AddObject(NeweHubSubscriptionValue("{00000000-DDDD-3333-1111-000000000000}", "SUB3", "TEST0001", "CLIENT0006", "FALSE", null, DateTime.Parse("2014-01-06T12:00"), (DateTime?)null, null));
			_context.eHubSubscriptionValues.AddObject(NeweHubSubscriptionValue("{00000000-DDDD-3333-2222-000000000000}", "SUB3", "TEST0001", "CLIENT0007", "FALSE", null, DateTime.Parse("2014-01-07T12:00"), (DateTime?)null, null));
		}

		public void LoadeHubSubscriptionBroadcasters()
		{
			_context.eHubSubscriptionBroadcasters.AddObject(NeweHubSubscriptionBroadcaster("{00000000-EEEE-1111-1111-000000000000}", "SUB3", "TEST0001"));
			_context.eHubSubscriptionBroadcasters.AddObject(NeweHubSubscriptionBroadcaster("{00000000-EEEE-1111-2222-000000000000}", "SUB5", "TEST0001", "TEST0002", "SELECT *"));
		}

		private eHubClient NeweHubClient(string CC_PK, string CC_ID, string CC_FriendlyName, string CC_AS2_Code, bool isAirServiceProvider, string CC_OwnerCategory = null, string CC_SystemCategory = null)
		{
			return new TestEHubClient(_context)
			{
				CC_PK = new Guid(CC_PK),
				CC_ID = CC_ID,
				CC_FriendlyName = CC_FriendlyName,
				CC_AS2_Code = CC_AS2_Code,
				CC_IsAirServiceProvider = isAirServiceProvider,
				CC_OwnerCategory = CC_OwnerCategory,
				CC_SystemCategory = CC_SystemCategory
			};
		}

		private ediProdClient NewEdiProdClient(string LD_LicenceType, string LE_EnterpriseCode, string LD_ServerCode, string EnterpriseServerCode, string CC_ID, string CC_PK)
		{
			return new ediProdClient
			{
				LD_LicenceType = LD_LicenceType,
				LE_EnterpriseCode = LE_EnterpriseCode,
				LD_ServerCode = LD_ServerCode,
				EnterpriseServerCode = EnterpriseServerCode,
				CC_ID = CC_ID,
				CC_PK = new Guid(CC_PK),
			};
		}

		private eHubClientSystem NeweHubClientSystem(string EH_PK, string EH_ID, DateTime EH_LastUpdateUTC)
		{
			return new TestEHubClientSystem(_context)
			{
				EH_PK = new Guid(EH_PK),
				EH_ID = EH_ID,
				EH_LastUpdateUTC = EH_LastUpdateUTC,
			};
		}

		private eHubMessageType NeweHubMessageType(string DT_PK, string DT_Code, bool DT_IsFlatFile, bool DT_IsEDI, string DT_Charset, string DT_EnvelopeXpath, string DT_InnerType)
		{
			var innerType = _context.eHubMessageTypes.FirstOrDefault(m => m.DT_Code == DT_InnerType);

			var type = new eHubMessageType()
			{
				DT_PK = new Guid(DT_PK),
				DT_Code = DT_Code,
				DT_IsFlatFile = DT_IsFlatFile,
				DT_IsEDI = DT_IsEDI,
				DT_Charset = DT_Charset,
				DT_EnvelopeXpath = DT_EnvelopeXpath,
				eHubMessageType_InnerType = innerType
			};

			if (innerType != null) innerType.eHubMessageType_Envelopes.Add(type);

			return type;
		}

		eHubTransformationSet NeweHubTransformationSet(string TS_PK, string TS_Name, string Sender_CC_ID, string Recipient_CC_ID)
		{
			eHubClient sender = _context.eHubClients.Where(cc => cc.CC_ID == Sender_CC_ID).First();
			eHubClient recipient = _context.eHubClients.Where(cc => cc.CC_ID == Recipient_CC_ID).First();

			eHubTransformationSet transformationSet = new eHubTransformationSet
			{
				TS_PK = new Guid(TS_PK),
				TS_Name = TS_Name,
				eHubClient_Sender = sender,
				TS_CC_Sender = sender.CC_PK,
				eHubClient_Recipient = recipient,
				TS_CC_Recipient = recipient.CC_PK,
			};

			sender.eHubTransformationSets_Sender.Add(transformationSet);
			recipient.eHubTransformationSets_Recipient.Add(transformationSet);

			return transformationSet;
		}

		eHubCodeSet NeweHubCodeSet(string CS_PK, string CS_Name, string Sender_CC_ID, string Recipient_CC_ID, string Transformation_TS_Name, string CS_Key1Name, string CS_Key2Name, string CS_Key3Name, string CS_Key4Name, string CS_Key5Name)
		{
			eHubClient sender = _context.eHubClients.Where(cc => cc.CC_ID == Sender_CC_ID).First();
			eHubClient recipient = _context.eHubClients.Where(cc => cc.CC_ID == Recipient_CC_ID).First();
			eHubTransformationSet transformationSet = (Transformation_TS_Name == null) ? null as eHubTransformationSet :
													  _context.eHubTransformationSets.Where(ts => ts.TS_CC_Sender == sender.CC_PK
																							&& ts.TS_CC_Recipient == recipient.CC_PK
																							&& ts.TS_Name == Transformation_TS_Name).First();

			eHubCodeSet codeSet = new eHubCodeSet
			{
				CS_PK = new Guid(CS_PK),
				CS_Name = CS_Name,
				eHubClient_Sender = sender,
				CS_CC_Sender = sender.CC_PK,
				eHubClient_Recipient = recipient,
				CS_CC_Recipient = recipient.CC_PK,
				eHubTransformationSet = transformationSet,
				CS_TS = (transformationSet == null) ? null as Guid? : transformationSet.TS_PK,
				CS_Key1Name = CS_Key1Name,
				CS_Key2Name = CS_Key2Name,
				CS_Key3Name = CS_Key3Name,
				CS_Key4Name = CS_Key4Name,
				CS_Key5Name = CS_Key5Name,
			};

			sender.eHubCodeSets_Sender.Add(codeSet);
			if (transformationSet != null)
			{
				sender.eHubTransformationSets_Sender.Add(transformationSet);
				transformationSet.eHubCodeSets.Add(codeSet);
			}

			return codeSet;
		}

		eHubCodeSetResult NeweHubCodeSetResult(string CR_PK, string CS_Name, int CR_Order, string CR_Name)
		{
			eHubCodeSet codeSet = _context.eHubCodeSets.Where(cs => cs.CS_Name == CS_Name).First();

			eHubCodeSetResult codeSetResult = new eHubCodeSetResult
			{
				CR_PK = new Guid(CR_PK),
				eHubCodeSet = codeSet,
				CR_CS = codeSet.CS_PK,
				CR_Order = CR_Order,
				CR_Name = CR_Name,
			};

			codeSet.eHubCodeSetResults.Add(codeSetResult);

			return codeSetResult;
		}

		eHubCodeMapKey NeweHubCodeMapKey(string CK_PK, string CS_Name, int CK_Order, string CK_Key1Value, string CK_Key2Value, string CK_Key3Value, string CK_Key4Value, string CK_Key5Value)
		{
			eHubCodeSet codeSet = _context.eHubCodeSets.Where(cs => cs.CS_Name == CS_Name).First();

			eHubCodeMapKey codeMapKey = new eHubCodeMapKey
			{
				CK_PK = new Guid(CK_PK),
				eHubCodeSet = codeSet,
				CK_CS = codeSet.CS_PK,
				CK_Order = CK_Order,
				CK_Key1Value = CK_Key1Value,
				CK_Key2Value = CK_Key2Value,
				CK_Key3Value = CK_Key3Value,
				CK_Key4Value = CK_Key4Value,
				CK_Key5Value = CK_Key5Value
			};

			codeSet.eHubCodeMapKeys.Add(codeMapKey);

			return codeMapKey;
		}

		eHubCodeMapValue NeweHubCodeMapValue(string CS_Name, int CK_Order, string CR_Name, string CV_OutputCode, int? CV_PassThroughKey)
		{
			eHubCodeSet codeSet = _context.eHubCodeSets.Where(cs => cs.CS_Name == CS_Name).First();
			eHubCodeMapKey codeMapKey = _context.eHubCodeMapKeys.First(ck => ck.CK_CS == codeSet.CS_PK && ck.CK_Order == CK_Order);
			eHubCodeSetResult codeSetResult = _context.eHubCodeSetResults.First(cr => cr.CR_CS == codeSet.CS_PK && cr.CR_Name == CR_Name);

			eHubCodeMapValue codeMapValue = new eHubCodeMapValue
			{
				eHubCodeMapKey = codeMapKey,
				CV_CK = codeMapKey.CK_PK,
				eHubCodeSetResult = codeSetResult,
				CV_CR = codeSetResult.CR_PK,
				CV_OutputCode = CV_OutputCode,
				CV_PassThroughKey = CV_PassThroughKey
			};

			return codeMapValue;
		}

		eHubAirConnection NeweHubAirConnection(string AC_CC_Client, string AC_CC_AirServiceProvider, string AC_PIMA, string AC_PASSWORD)
		{
			return new eHubAirConnection
			{
				AC_CC_Client = new Guid(AC_CC_Client),
				AC_CC_AirServiceProvider = new Guid(AC_CC_AirServiceProvider),
				AC_PIMA = AC_PIMA,
				AC_PASSWORD = AC_PASSWORD
			};
		}

		eHubAirConnectionPerBranch NeweHubAirConnectionPerBranch(string AB_CC_Client, string AB_CC_AirServiceProvider, string IATA, string PIMA, string Password)
		{
			return new eHubAirConnectionPerBranch
			{
				AB_CC_Client = new Guid(AB_CC_Client),
				AB_CC_AirServiceProvider = new Guid(AB_CC_AirServiceProvider),
				AB_IssuingCarrierAgentIATACode = IATA,
				AB_PIMA = PIMA,
				AB_PASSWORD = Password
			};
		}

		eHubUSCustomsRegistry NeweHubUSCustomsRegistry(string ER_PK, string ER_CC_Client, string ER_ApplicationCode, string ER_Name, string ER_Value, bool ER_IsProduction)
		{
			return new eHubUSCustomsRegistry
			{
				ER_PK = new Guid(ER_PK),
				ER_CC_Client = new Guid(ER_CC_Client),
				ER_ApplicationCode = ER_ApplicationCode,
				ER_Name = ER_Name,
				ER_Value = ER_Value,
				ER_IsProduction = ER_IsProduction
			};
		}

		eHubMessageReferenceRegistry NeweHubMessageReferenceRegistry(string CR_PK, string CR_CC_Client, string CR_ApplicationCode, string CR_MessageReference, string CR_Password)
		{
			return new eHubMessageReferenceRegistry
			{
				CR_PK = new Guid(CR_PK),
				CR_CC_Client = new Guid(CR_CC_Client),
				CR_ApplicationCode = CR_ApplicationCode,
				CR_MessageReference = CR_MessageReference,
				CR_Password = CR_Password
			};
		}

		private eHubSubscriptionType NeweHubSubscriptionType(string ST_PK, string ST_ID, string ST_Name, int? ST_ExpiryDays)
		{
			return new eHubSubscriptionType()
			{
				ST_PK = new Guid(ST_PK),
				ST_ID = ST_ID,
				ST_Name = ST_Name,
				ST_ExpiryDays = ST_ExpiryDays,
			};
		}

		eHubSubscriptionAutoSubscribe NeweHubSubscriptionAutoSubscribe(string SA_PK, string ST_ID, string DT_Code, string Recipient_CC_ID, string SA_ValueXpath, string SA_ValueProperty, string SA_ReferenceXpath, string SA_ReferenceProperty)
		{
			var subtype = _context.eHubSubscriptionTypes.First(t => t.ST_ID == ST_ID);
			var message = _context.eHubMessageTypes.First(m => m.DT_Code == DT_Code);
			var recipient = _context.eHubClients.FirstOrDefault(c => c.CC_ID == Recipient_CC_ID);

			var auto = new eHubSubscriptionAutoSubscribe()
			{
				SA_PK = new Guid(SA_PK),
				eHubSubscriptionType = subtype,
				eHubMessageType = message,
				eHubClient_Recipient = recipient,
				SA_ValueXpath = SA_ValueXpath,
				SA_ValueProperty = SA_ValueProperty,
				SA_ReferenceXpath = SA_ReferenceXpath,
				SA_ReferenceProperty = SA_ReferenceProperty,
			};

			subtype.eHubSubscriptionAutoSubscribes.Add(auto);
			message.eHubSubscriptionAutoSubscribes.Add(auto);
			if (recipient != null) recipient.eHubSubscriptionAutoSubscribes.Add(auto);

			return auto;
		}

		eHubSubscriptionLookup NeweHubSubscriptionLookup(string SL_PK, string ST_ID, string DT_Code, string SL_ValueXpath, string SL_ValueProperty)
		{
			var subtype = _context.eHubSubscriptionTypes.First(t => t.ST_ID == ST_ID);
			var message = _context.eHubMessageTypes.First(m => m.DT_Code == DT_Code);

			var lookup = new eHubSubscriptionLookup()
			{
				SL_PK = new Guid(SL_PK),
				eHubSubscriptionType = subtype,
				eHubMessageType = message,
				SL_ValueXpath = SL_ValueXpath,
				SL_ValueProperty = SL_ValueProperty
			};

			subtype.eHubSubscriptionLookups.Add(lookup);
			message.eHubSubscriptionLookups.Add(lookup);

			return lookup;
		}

		eHubSubscriptionValue NeweHubSubscriptionValue(string SV_PK, string ST_ID, string Provider_CC_ID, string Subscriber_CC_ID, string SV_Value, string SV_Reference, DateTime SV_SubscribedUTC, DateTime? SV_ExpiryUTC, string SV_ReferenceType)
		{
			var subtype = _context.eHubSubscriptionTypes.First(t => t.ST_ID == ST_ID);
			var provider = _context.eHubClients.FirstOrDefault(c => c.CC_ID == Provider_CC_ID);
			var subscriber = _context.eHubClients.FirstOrDefault(c => c.CC_ID == Subscriber_CC_ID);

			var value = new eHubSubscriptionValue()
			{
				SV_PK = new Guid(SV_PK),
				eHubSubscriptionType = subtype,
				eHubClient_Provider = provider,
				eHubClient_Subscriber = subscriber,
				SV_Value = SV_Value,
				SV_Reference = SV_Reference,
				SV_SubscribedUTC = SV_SubscribedUTC,
				SV_ExpiryUTC = SV_ExpiryUTC,
				SV_ReferenceType = SV_ReferenceType
			};

			subtype.eHubSubscriptionValues.Add(value);
			provider.eHubSubscriptionValues_Provider.Add(value);
			subscriber.eHubSubscriptionValues_Subscriber.Add(value);

			return value;
		}

		eHubSubscriptionBroadcaster NeweHubSubscriptionBroadcaster(string SB_PK, string ST_ID, string Broadcast_Sender_CC_ID, string Broadcast_Recipient_CC_ID = null, string SB_SubscriberSelectSql = null)
		{
			var subtype = _context.eHubSubscriptionTypes.First(t => t.ST_ID == ST_ID);
			var broadcastSender = _context.eHubClients.FirstOrDefault(c => c.CC_ID == Broadcast_Sender_CC_ID);
			var broadcastRecipient = string.IsNullOrWhiteSpace(Broadcast_Recipient_CC_ID) ? null : _context.eHubClients.FirstOrDefault(c => c.CC_ID == Broadcast_Recipient_CC_ID);

			var value = new eHubSubscriptionBroadcaster()
			{
				SB_PK = new Guid(SB_PK),
				eHubSubscriptionType = subtype,
				eHubClient_Sender = broadcastSender,
				eHubClient_Recipient = broadcastRecipient,
				SB_SubscriberSelectSql = SB_SubscriberSelectSql
			};

			subtype.eHubSubscriptionBroadcasters.Add(value);
			broadcastSender.eHubSubscriptionBroadcasters_Senders.Add(value);
			broadcastRecipient?.eHubSubscriptionBroadcasters_Recipients.Add(value);

			return value;
		}
	}

	public abstract class RegistryContainer<T> : FixupCollection<T>, ICollection<T>
	{
		void ICollection<T>.Add(T registry)
		{
			DoAdd(registry);
			base.Add(registry);
		}

		public abstract void DoAdd(T entity);
	}

	public class eHubUsCustomsRegistryContainer : RegistryContainer<eHubUSCustomsRegistry>
	{
		TestContext context;
		public eHubClient client { get; set; }

		public eHubUsCustomsRegistryContainer(TestContext context, eHubClient client)
		{
			this.context = context;
			this.client = client;
		}

		public override void DoAdd(eHubUSCustomsRegistry entity)
		{
			entity.ER_CC_Client = client.CC_PK;
			entity.eHubClient = client;
			context.eHubUSCustomsRegistry.AddObject(entity);
		}
	}

	public class eHubMessageReferenceRegistryContainer : RegistryContainer<eHubMessageReferenceRegistry>
	{
		TestContext context;
		public eHubClient client;

		public eHubMessageReferenceRegistryContainer(TestContext context, eHubClient client)
		{
			this.context = context;
			this.client = client;
		}

		public override void DoAdd(eHubMessageReferenceRegistry entity)
		{
			entity.CR_CC_Client = client.CC_PK;
			entity.eHubClient = client;
			context.eHubMessageReferenceRegistries.AddObject(entity);
		}
	}

	public class TestEHubClient : eHubClient
	{
		TestContext context;

		public TestEHubClient(TestContext context)
		{
			this.context = context;
			this.eHubUSCustomsRegistryContainer = new eHubUsCustomsRegistryContainer(context, this);
			this.eHubMessageReferenceRegistryContainer = new eHubMessageReferenceRegistryContainer(context, this);
		}

		public override Guid CC_PK
		{
			set
			{
				base.CC_PK = value;
				eHubUSCustomsRegistryContainer.client = this;
				eHubMessageReferenceRegistryContainer.client = this;
			}
			get { return base.CC_PK; }
		}

		eHubUsCustomsRegistryContainer eHubUSCustomsRegistryContainer;
		public override ICollection<eHubUSCustomsRegistry> eHubUSCustomsRegistry { get { return eHubUSCustomsRegistryContainer; } }

		eHubMessageReferenceRegistryContainer eHubMessageReferenceRegistryContainer;
		public override ICollection<eHubMessageReferenceRegistry> eHubMessageReferenceRegistries { get { return eHubMessageReferenceRegistryContainer; } }
	}

	public class TestEHubClientSystem : eHubClientSystem
	{
		TestContext context;

		public TestEHubClientSystem(TestContext context)
		{
			this.context = context;
		}

		public override Guid EH_PK
		{
			set
			{
				base.EH_PK = value;
			}
			get { return base.EH_PK; }
		}

		public override DateTime EH_LastUpdateUTC
		{
			get { return base.EH_LastUpdateUTC; }
			set
			{
				base.EH_LastUpdateUTC = value;
			}
		}
	}
}
