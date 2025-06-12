using CargoWise.eHub.Integration;
using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.MessageHandlerMAN;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceBroker.Common;

namespace CargoWise.eServices.USCustoms.Tests
{
	[TestClass]
	public class MANInboundMessageHandlerTest
	{
		private class TestRegistryRepository : IRegistryRepository
		{
			#region IRegistryRepository Members

			public bool CacheReferenceFiles(string referenceID, System.IO.Stream bodyStream, bool isProd)
			{
				throw new System.NotImplementedException();
			}

			public bool CheckClientIDExist(string clientId)
			{
				throw new System.NotImplementedException();
			}

			public string[] GetClientIDFromRegistryValue(string value, string applicationCode, string name, bool isProduction)
			{
				if (value == "5DS9" && applicationCode == ApplicationCode.USeManifest && name == Constants.RegistryName.MAN.ClientNetworkID)
					return new[] { isProduction ? "testclient" : "testclient2" };
				else
					throw new System.NotImplementedException();
			}

			public string[] GetUnsolicitedClientIDs(string applicationCode, string name, bool isProduction, bool allProdAndTest)
			{
				throw new System.NotImplementedException();
			}

			public string InsertRegistryValue(string clientId, string applicationCode, string name, bool isProduction, string registryValue)
			{
				throw new System.NotImplementedException();
			}

			public string LoadRegistryValue(string clientId, string applicationCode, string name, bool? isProduction = null)
			{
				throw new System.NotImplementedException();
			}

			public string[] LoadRegistryValues(string clientId, string applicationCode, string name, bool? isProduction = null)
			{
				throw new System.NotImplementedException();
			}

			#endregion
		}

		[TestMethod]
		public void MANInboundMessageHandlerGetClientID()
		{
			var inputData_1 = "UNB+UNOA:4+CBP-ACE-TEST:ZZ+XXXX:ZZ+20140219:2309+473++ACE'UNG+CUSRES+ACE:ZZ+5DS9:ZZ+20140219:2309+473+UN+D:03B'UNH+473+CUSRES:D:03B:UN'BGM+132:::STANDARD+AAGCMAN0000104+22'DTM+132:201402201655:203'FTX+AIQ+++MAN510'TDT+11++03+:::BT+AAGC+I++:146::1M8GDM9AXKP042788'TDT+11++03+:::BT+AAGC+I++:274::12345678'TDT+11++03+:::BT+AAGC+I++:8::1234567890'LOC+60+0901'RFF+ACD:EQU123'LOC+89+VA:163'LOC+89+US:162'ERP+1'ERC+418'FTX+AAO+++Duplicate Trip Number'ERP+1'ERC+509'FTX+AAO+++Manifest Rejected'DOC+:ZZZ'NAD+VW+14133:109+++1234 SUNSETT BVD+LOS ANGELESE+CA+123456'UNT+20+473'UNE+1+473'UNZ+1+473'";
			var inputData_2 = "UNB+UNOA:4+CBP-ACE-TEST:ZZ+XXXX:ZZ+20140219:2309+473++ACE'UNG+CT+ACE:ZZ+5DS9:ZZ+20140219:2309+473+UN+D:03B'UNH+473+CUSRES:D:03B:UN'BGM+132:::STANDARD+AAGCMAN0000104+22'DTM+132:201402201655:203'FTX+AIQ+++MAN510'TDT+11++03+:::BT+AAGC+I++:146::1M8GDM9AXKP042788'TDT+11++03+:::BT+AAGC+I++:274::12345678'TDT+11++03+:::BT+AAGC+I++:8::1234567890'LOC+60+0901'RFF+ACD:EQU123'LOC+89+VA:163'LOC+89+US:162'ERP+1'ERC+418'FTX+AAO+++Duplicate Trip Number'ERP+1'ERC+509'FTX+AAO+++Manifest Rejected'DOC+:ZZZ'NAD+VW+14133:109+++1234 SUNSETT BVD+LOS ANGELESE+CA+123456'UNT+20+473'UNE+1+473'UNZ+1+473'";
			//var config = ConfigurationHelper<IInboundConfiguration>.GetInboundConfiguration(Constants.MessagaType.MAN, new TestRegistryRepository());
			var config = new MANInboundMessageHandler(new TestRegistryRepository());
			var result1 = config.GetClientId(inputData_1, true);
			var result2 = config.GetClientId(inputData_2, true);
			var result3 = config.GetClientId(inputData_2, false);
			Assert.AreEqual(1, result1.Length);
			Assert.AreEqual(1, result2.Length);
			Assert.AreEqual(1, result3.Length);
			Assert.AreEqual("testclient", result1[0]);
			Assert.AreEqual("testclient", result2[0]);
			Assert.AreEqual("testclient2", result3[0]);
		}
	}
}
