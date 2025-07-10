using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.CALINF;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class CALINFMessasgeBuilderTest : TestCaseWithFactory
	{
		public void TestUNH_MessageHeader()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var testCase in new[]
				{
					new { TransportMode = Core.Constants.TransportModes.Sea, Contains = true },
					new { TransportMode = Core.Constants.TransportModes.Air, Contains = true }
				})
				{
					var jobVoyage = Factory.New<JobVoyage>();
					jobVoyage.JV_AirSeaRoad = testCase.TransportMode;

					new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), subType).PopulateMessages();
					jobVoyage.Messages.Load();
					var message = jobVoyage.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.TransportMode}-{subType}", testCase.Contains, message.EM_MessageText.Contains("UNH+<<MSGNO PLACEHOLDER>>+CALINF:D:16A:UN:RCG001'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestBGM_BeginningOfMessage()
		{
			var messageTypeLookup = new Dictionary<string, string>()
			{
				{ Core.Constants.TransportModes.Sea, "SCH" },
				{ Core.Constants.TransportModes.Air, "ASC" }
			};

			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var transportMode in new[]
			{
				Core.Constants.TransportModes.Sea,
				Core.Constants.TransportModes.Air
			})
			{
				foreach (var testCase in new[]
				{
					new { SubType = MessageSubTypes.Create, FuntionCode = 9 },
					new { SubType = MessageSubTypes.Change, FuntionCode = 4 },
					new { SubType = MessageSubTypes.Withdraw, FuntionCode = 1 }
				})
				{
					var jobVoyage = Factory.New<JobVoyage>();
					jobVoyage.JV_AirSeaRoad = transportMode;

					new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), testCase.SubType).PopulateMessages();
					jobVoyage.Messages.Load();
					var message = jobVoyage.Messages[0];

					assertDelegate += () => { AssertContains($"{transportMode}-{testCase.SubType}", $"BGM+96:::{messageTypeLookup[transportMode]}+<<SYSCAR>>+{testCase.FuntionCode}'", message.EM_MessageText); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		[TestDate(2020, 05, 28, 14, 35, 0)]
		public void TestDTM_DocumentDateTime()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var testCase in new[]
				{
					new { TransportMode = Core.Constants.TransportModes.Sea, Contains = true },
					new { TransportMode = Core.Constants.TransportModes.Air, Contains = true }
				})
				{
					var jobVoyage = Factory.New<JobVoyage>();
					jobVoyage.JV_AirSeaRoad = testCase.TransportMode;

					new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), subType).PopulateMessages();
					jobVoyage.Messages.Load();
					var message = jobVoyage.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.TransportMode}-{subType}", testCase.Contains, message.EM_MessageText.Contains("DTM+137:202005281435:203'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestRFF_DocumentToBeAmended()
		{
			SetupZZRefDB(Factory);

			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var transportMode in new[]
			{
				Core.Constants.TransportModes.Sea,
				Core.Constants.TransportModes.Air
			})
			{
				foreach (var testCase in new[]
				{
					new { SubType = MessageSubTypes.Create, Snippet = "RFF+ACW:B9C73560F3A54797909A498BE37010B5'", Contains = false },
					new { SubType = MessageSubTypes.Change, Snippet = "RFF+ACW:B9C73560F3A54797909A498BE37010B5'", Contains = true },
					new { SubType = MessageSubTypes.Withdraw, Snippet = "RFF+ACW:B9C73560F3A54797909A498BE37010B5'", Contains = true },
				})
				{
					var jobVoyage = Factory.New<JobVoyage>();
					jobVoyage.JV_AirSeaRoad = transportMode;

					var calinf = Factory.New<CALINFEDIMessage>();
					calinf.EM_MessageNum = "123";
					calinf.EM_MessageText = CALINFTestMessage.Replace("\r\n", "");
					calinf.EM_LinkUniqueID = jobVoyage.PK;
					calinf.EM_LinkTable = JobVoyage.Schema.TableName;

					var cusres = Factory.New<CUSRESEDIMessage>();
					cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					cusres.EM_MessageText = CUSRESTestMessage.Replace("\r\n", "");
					cusres.EM_LinkUniqueID = jobVoyage.PK;
					cusres.EM_LinkTable = JobVoyage.Schema.TableName;

					jobVoyage.Messages.AddRange(calinf, cusres);

					new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), testCase.SubType).PopulateMessages();
					jobVoyage.Messages.Load();
					var message = jobVoyage.Messages.LastMessage;

					assertDelegate += () => { AssertEquals($"{transportMode}-{testCase.SubType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestNAD_MessageSender()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var testCase in new[]
				{
					new { TransportMode = Core.Constants.TransportModes.Sea, Contains = true },
					new { TransportMode = Core.Constants.TransportModes.Air, Contains = true }
				})
				{
					var orgHeader = Factory.New<OrgHeader>();
					var customsCode = orgHeader.CustomsCodes.AddNew();
					customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
					customsCode.OK_CodeType = "CCC";
					customsCode.OK_CustomsRegNo = "125";

					var jobVoyage = Factory.New<JobVoyage>();
					jobVoyage.JV_AirSeaRoad = testCase.TransportMode;
					jobVoyage.JV_OH_Line = orgHeader.PK;

					new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), subType).PopulateMessages();
					jobVoyage.Messages.Load();
					var message = jobVoyage.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.TransportMode}-{subType}", testCase.Contains, message.EM_MessageText.Contains("NAD+MS+125::ZZZ'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestTDT_TransportInformation()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_CarrierCode = "MSC";
			vessel.RV_RadioCallSign = "RCSBS";
			vessel.RV_Code = "BlueSeas";
			vessel.RV_RN_NKCountryOfReg = "GB";

			var airline = Factory.New<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "125";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MiscServ.OM_RM_Airline = airline.PK;

			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			customsCode.OK_CodeType = "CCC";
			customsCode.OK_CustomsRegNo = "MSC";

			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var testCase in new[]
				{
					new { TransportMode = Core.Constants.TransportModes.Sea, Snippet = $"TDT+20+VF7755+1++MSC:172:20+++RCSBS:103::BLUESEAS:GB'" },
					new { TransportMode = Core.Constants.TransportModes.Air, Snippet = $"TDT+20+VF7755+4++125:172:3+++:146'" }
				})
				{
					var jobVoyage = Factory.New<JobVoyage>();
					jobVoyage.JV_AirSeaRoad = testCase.TransportMode;
					jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
					jobVoyage.JV_VoyageFlight = "VF7755";
					jobVoyage.JV_OH_Line = orgHeader.PK;

					new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), subType).PopulateMessages();
					jobVoyage.Messages.Load();
					var message = jobVoyage.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.TransportMode}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestRFF_PrincipalCarrierConveyanceNumber()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var testCase in new[]
				{
					new { TransportMode = Core.Constants.TransportModes.Sea, Contains = true },
					new { TransportMode = Core.Constants.TransportModes.Air, Contains = true }
				})
				{
					var jobVoyage = Factory.New<JobVoyage>();
					jobVoyage.JV_AirSeaRoad = testCase.TransportMode;
					jobVoyage.JV_VoyageFlight = "123";

					new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), subType).PopulateMessages();
					jobVoyage.Messages.Load();
					var message = jobVoyage.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.TransportMode}-{subType}", testCase.Contains, message.EM_MessageText.Contains("RFF+ACL:123'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestLOC_and_DTM_DepartureDetails()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var testCase in new[]
				{
					new { TransportMode = Core.Constants.TransportModes.Sea, Snippet = "LOC+5+DEHAM:139:6'DTM+136:202005201330:203'" },
					new { TransportMode = Core.Constants.TransportModes.Air, Snippet = "LOC+5+HAM:145:3'DTM+136:202005201330:203'" }
				})
				{
					var jobVoyage = Factory.New<JobVoyage>();
					jobVoyage.JV_AirSeaRoad = testCase.TransportMode;

					var origin = jobVoyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = "DEHAM";
					origin.JA_A_DEP = new ZDateTime(2020, 05, 20, 13, 30, 0);

					new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), subType).PopulateMessages();
					jobVoyage.Messages.Load();
					var message = jobVoyage.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.TransportMode}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestLOC_and_DTM_CallDetails()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var testCase in new[]
				{
					new { TransportMode = Core.Constants.TransportModes.Sea, Snippet = "LOC+153+ZAJHB:139:6'DTM+132:202006251535:203'" },
					new { TransportMode = Core.Constants.TransportModes.Air, Snippet = "LOC+153+JHB:145:3'DTM+132:202006251535:203'" }
				})
				{
					var jobVoyage = Factory.New<JobVoyage>();
					jobVoyage.JV_AirSeaRoad = testCase.TransportMode;

					var destination = jobVoyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = "ZAJHB";
					destination.JB_A_ARV = new ZDateTime(2020, 06, 25, 15, 35, 0);

					new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), subType).PopulateMessages();
					jobVoyage.Messages.Load();
					var message = jobVoyage.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.TransportMode}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestUNT_MessageTrailer()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var testCase in new[]
				{
					new { TransportMode = Core.Constants.TransportModes.Sea, Snippet = subType == MessageSubTypes.Create ? "UNT+7+<<MSGNO PLACEHOLDER>>'" : "UNT+8+<<MSGNO PLACEHOLDER>>'" },
					new { TransportMode = Core.Constants.TransportModes.Air, Snippet = subType == MessageSubTypes.Create ? "UNT+7+<<MSGNO PLACEHOLDER>>'" : "UNT+8+<<MSGNO PLACEHOLDER>>'" }
				})
				{
					var jobVoyage = Factory.New<JobVoyage>();
					jobVoyage.JV_AirSeaRoad = testCase.TransportMode;

					new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), subType).PopulateMessages();
					jobVoyage.Messages.Load();
					var message = jobVoyage.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.TransportMode}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestMessageContentEscapedWithCorrectCharacterSet()
		{
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			jobVoyage.JV_VoyageFlight = "1-+':?-9";

			new CALINFMessageBuilder(new CALINFMessageData(jobVoyage), MessageSubTypes.Create).PopulateMessages();
			jobVoyage.Messages.Load();
			var message = jobVoyage.Messages[0].EM_MessageText;

			AssertContains("Voyage Escaped", "RFF+ACL:1-?+?'?:??-9'", message);
		}

		internal static void SetupZZRefDB(BusinessObjectFactory factory, string countryCode = "ZA")
		{
			var typeCustomsStatus = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus;
			var minDateTime = ZDateTime.MinSmallDateTimeValue;
			var maxDateTime = ZDateTime.MaxSmallDateTimeValue;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);

			helper.CreateCusCodeType(typeCustomsStatus, "Customs Manifest Status");
			var list6 = helper.CreateCusCodeList(countryCode, typeCustomsStatus, "6", "Rejected", minDateTime, maxDateTime);
			var list8 = helper.CreateCusCodeList(countryCode, typeCustomsStatus, "8", "Proceed to Border", minDateTime, maxDateTime);

			helper.CreateCusCodeListAttribute(list6.PK, "CustomsRejected", "true");
			helper.CreateCusCodeListAttribute(list6.PK, "IUpdateCustomsStatus", "");
			helper.CreateCusCodeListAttribute(list6.PK, "INotify", "");

			helper.CreateCusCodeListAttribute(list8.PK, "CustomsCleared", "true");
			helper.CreateCusCodeListAttribute(list8.PK, "INotify", "");
			helper.CreateCusCodeListAttribute(list8.PK, "IAllowCancel", "true");
			helper.CreateCusCodeListAttribute(list8.PK, "IUpdateCustomsStatus", "");
			factory.Save();
		}

		const string CALINFTestMessage = @"UNH+316+CALINF:D:16A:UN:RCG001'
BGM+96:::SCH+B9C73560F3A54797909A498BE37010B5+9'
DTM+137:201603310615:203'
NAD+MS+125::ZZZ'
TDT+20++++:172:20+++:103'
RFF+ACL:DDD4455667'
LOC+5+GBLON:139:6'
DTM+136:202009010600:203'
UNT+9+1'";

		const string CUSRESTestMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+B9C73560F3A54797909A498BE37010B5:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+8:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:123'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";
	}
}
