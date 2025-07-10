using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class COSTCOMessasgeBuilderTest : TestCaseWithFactory
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("UNH+<<MSGNO PLACEHOLDER>>+COSTCO:D:16A:UN:RCG001'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestBGM_BeginningOfMessage()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var manifestType in new[]
			{
				ManifestTypeList.Codes.DepotOutturnReport,
				ManifestTypeList.Codes.BulkBreakBulkOutturnReport,
				ManifestTypeList.Codes.VesselOutturnReport,
				ManifestTypeList.Codes.AirCargoOutturnReport,
				ManifestTypeList.Codes.AirExcessOutturnReport,
				ManifestTypeList.Codes.AirLoadDischarge
			})
			{
				foreach (var testCase in new[]
				{
					new { SubType = MessageSubTypes.Create, FuntionCode = 9 },
					new { SubType = MessageSubTypes.Change, FuntionCode = 4 },
					new { SubType = MessageSubTypes.Withdraw, FuntionCode = 1 }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = manifestType;
					new COSTCOMessageBuilder(new COSTCOHeader(header), testCase.SubType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertContains($"{manifestType}-{testCase.SubType}", $"BGM+788:::{manifestType}+<<SYSCAR>>+{testCase.FuntionCode}'", message.EM_MessageText); };
				}
			}

			CombineAssertions(assertDelegate);
		}

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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_IssueDate = new ZDate(2018, 3, 8);
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("DTM+137:201803080000:203'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestDTM_ActualArrivalDate()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = NatureList.Codes.Import23;
					header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 8);
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("DTM+132:201803080000:203'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestDTM132_ActualArrivalDate_OnlyExistsForIMP()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var manifest in new[]
				{
					ManifestTypeList.Codes.DepotOutturnReport,
					ManifestTypeList.Codes.BulkBreakBulkOutturnReport,
					ManifestTypeList.Codes.VesselOutturnReport,
					ManifestTypeList.Codes.AirCargoOutturnReport,
					ManifestTypeList.Codes.AirExcessOutturnReport,
					ManifestTypeList.Codes.AirLoadDischarge
				})
				{
					foreach (var testCase in new[]
					{
						new { Nature = NatureList.Codes.Import23, Contains = true },
						new { Nature = NatureList.Codes.Export22, Contains = false },
						new { Nature = NatureList.Codes.Transhipment28, Contains = false },
						new { Nature = NatureList.Codes.Transit24, Contains = false },
						new { Nature = NatureList.Codes.FreightRemainingOnBoard, Contains = false },
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 8);
						header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 9);
						header.AMA_ManifestType = manifest;
						header.AMA_Nature = testCase.Nature;
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{manifest}-{subType}-{testCase.Nature}", testCase.Contains, message.EM_MessageText.Contains("DTM+132:201803080000:203'", StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestDTM_EstimatedDateOfDeparture()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = NatureList.Codes.Export22;
					header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 8);
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("DTM+133:201803080000:203'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestDTM132_EstimatedDateOfDeparture_OnlyExistsForEXP()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var manifest in new[]
				{
					ManifestTypeList.Codes.DepotOutturnReport,
					ManifestTypeList.Codes.BulkBreakBulkOutturnReport,
					ManifestTypeList.Codes.VesselOutturnReport,
					ManifestTypeList.Codes.AirCargoOutturnReport,
					ManifestTypeList.Codes.AirExcessOutturnReport,
					ManifestTypeList.Codes.AirLoadDischarge
				})
				{
					foreach (var testCase in new[]
					{
						new { Nature = NatureList.Codes.Import23, Contains = false },
						new { Nature = NatureList.Codes.Export22, Contains = true },
						new { Nature = NatureList.Codes.Transhipment28, Contains = false },
						new { Nature = NatureList.Codes.Transit24, Contains = false },
						new { Nature = NatureList.Codes.FreightRemainingOnBoard, Contains = false },
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 8);
						header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 9);
						header.AMA_ManifestType = manifest;
						header.AMA_Nature = testCase.Nature;
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{manifest}-{subType}-{testCase.Nature}", testCase.Contains, message.EM_MessageText.Contains("DTM+133:201803090000:203'", StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestDTM_DateTimeFullyUnloadedLoaded()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.FullyLoadedUnloadedDate = new ZDateTime(2018, 3, 8);
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("DTM+778:201803080000:203'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestFTX_ExcessIndicator()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.ExcessIndicator = ExcessIndicatorList.Codes.Excess;
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("FTX+ABO++1'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestFTX_ImportExportTranshipmentIndicator()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var manifest in new[]
				{
					ManifestTypeList.Codes.DepotOutturnReport,
					ManifestTypeList.Codes.BulkBreakBulkOutturnReport,
					ManifestTypeList.Codes.VesselOutturnReport,
					ManifestTypeList.Codes.AirCargoOutturnReport,
					ManifestTypeList.Codes.AirExcessOutturnReport,
					ManifestTypeList.Codes.AirLoadDischarge
				})
				{
					foreach (var testCase in new[]
					{
						new { Nature = NatureList.Codes.Import23, Snippet = "FTX+ADI++929'" },
						new { Nature = NatureList.Codes.Export22, Snippet = "FTX+ADI++830'" },
						new { Nature = NatureList.Codes.Transhipment28, Snippet = "FTX+ADI++399'" },
						new { Nature = NatureList.Codes.Transit24, Snippet = "FTX+ADI++950'" },
						new { Nature = NatureList.Codes.FreightRemainingOnBoard, Snippet = "FTX+ADI'" },
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_ManifestType = manifest;
						header.AMA_Nature = testCase.Nature;
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{manifest}-{subType}-{testCase.Nature}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestRFF_DocumentToBeAmended()
		{
			OutturnTestHelper.SetupZZ(Factory);

			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var manifest in new[]
			{
				ManifestTypeList.Codes.DepotOutturnReport,
				ManifestTypeList.Codes.BulkBreakBulkOutturnReport,
				ManifestTypeList.Codes.VesselOutturnReport,
				ManifestTypeList.Codes.AirCargoOutturnReport,
				ManifestTypeList.Codes.AirExcessOutturnReport,
				ManifestTypeList.Codes.AirLoadDischarge
			})
			{
				foreach (var testCase in new[]
				{
					new { SubType = MessageSubTypes.Create, Snippet = "RFF+ACW:B9C73560F3A54797909A498BE37010B5'", Contains = false },
					new { SubType = MessageSubTypes.Change, Snippet = "RFF+ACW:B9C73560F3A54797909A498BE37010B5'", Contains = true },
					new { SubType = MessageSubTypes.Withdraw, Snippet = "RFF+ACW:B9C73560F3A54797909A498BE37010B5'", Contains = true },
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = manifest;
					var costco = Factory.New<COSTCOEDIMessage>();
					costco.EM_MessageNum = "123";
					costco.EM_MessageText = COSTCOTestMessage.Replace("\r\n", "");
					costco.EM_LinkUniqueID = header.PK;
					costco.EM_LinkTable = header.TablePrefix;
					var cusres = Factory.New<CUSRESEDIMessage>();
					cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					cusres.EM_MessageText = CUSRESTestMessage.Replace("\r\n", "");
					cusres.EM_LinkUniqueID = header.PK;
					cusres.EM_LinkTable = header.TablePrefix;
					header.Messages.AddRange(costco, cusres);
					new COSTCOMessageBuilder(new COSTCOHeader(header), testCase.SubType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages.LastMessage;

					assertDelegate += () => { AssertEquals($"{manifest}-{testCase.SubType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		const string COSTCOTestMessage = @"UNH+316+COSTCO:D:16A:UN:RCG001'
BGM+788+B9C73560F3A54797909A498BE37010B5+9'
FTX+ADI'
TDT+20++++:172:20+++:103'
RFF+ACL'
LOC+11+:139:6+::ZZZ'
NAD+MS+::ZZZ'
NAD+RL+::ZZZ'
EQD+BB'
SEL+NO SEAL NO'
CNT+8:0'";

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

		public void TestTDT_TransportInformation()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "456";
			vessel.RV_RadioCallSign = "BBB";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "VVV";
			var address = org.Addresses.AddNew();
			address.Address1 = "XXX";
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "VWG", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var transportCode in new[]
				{
					new { TransportMode = Core.Constants.TransportModes.Air, TrasnportCode = "4" , AgencyCode = "3" },
					new { TransportMode = Core.Constants.TransportModes.Sea, TrasnportCode = "1", AgencyCode = "20" },
					new { TransportMode = Core.Constants.TransportModes.Road, TrasnportCode = "3", AgencyCode = "20" },
					new { TransportMode = Core.Constants.TransportModes.Rail, TrasnportCode = "2", AgencyCode = "20" },
				})
				{
					foreach (var testCase in new[]
					{
						new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = $"TDT+20+123+{transportCode.TrasnportCode}++VWG:172:{transportCode.AgencyCode}+++BBB:103'" },
						new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = $"TDT+20+123+{transportCode.TrasnportCode}++VWG:172:{transportCode.AgencyCode}+++BBB:103'" },
						new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = $"TDT+20+123+{transportCode.TrasnportCode}++VWG:172:{transportCode.AgencyCode}+++BBB:103'" },
						new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = $"TDT+20+123+{transportCode.TrasnportCode}++VWG:172:{transportCode.AgencyCode}+++BBB:103'" },
						new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = $"TDT+20+123+{transportCode.TrasnportCode}++VWG:172:{transportCode.AgencyCode}+++BBB:103'" },
						new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = $"TDT+20+123+{transportCode.TrasnportCode}++VWG:172:{transportCode.AgencyCode}'" }
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_Voyage = "123";
						header.AMA_TransportMode = transportCode.TransportMode;
						header.AMA_VesselName = "456";
						header.AMA_ManifestType = testCase.ManifestType;
						header.AMA_OA_Carrier = address.PK;
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}-{transportCode.TransportMode}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestRFF_ManifestType()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var manifest in new[]
				{
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					foreach (var testCase in new[]
					{
						new { Nature = NatureList.Codes.Import23, Snippet = "RFF+AFB:23'" },
						new { Nature = NatureList.Codes.Export22, Snippet = "RFF+AFB:22'" },
						new { Nature = NatureList.Codes.Transhipment28, Snippet = "RFF+AFB:28'" },
						new { Nature = NatureList.Codes.Transit24, Snippet = "RFF+AFB:24'" },
						new { Nature = NatureList.Codes.FreightRemainingOnBoard, Snippet = "RFF+AFB:57'" },
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_ManifestType = manifest.ManifestType;
						header.AMA_Nature = testCase.Nature;
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{manifest.ManifestType}-{subType}-{testCase.Nature}", manifest.Contains, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
					}
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Voyage = "123";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("RFF+ACL:123'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestLOC_PlacePortOfDischarge_SEA()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = "LOC+11+VWG:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = "LOC+11+VWG:139:6+PPP::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = "LOC+11+VWG:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = "LOC+11+VWG:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = "LOC+11+VWG:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = "LOC+11+VWG:139:6+PPP::ZZZ'" }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = NatureList.Codes.Import23;
					header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					header.MasterBill.ABL_RL_NKPortOfDischarge = "VWG";
					header.MasterBill.ABL_GoodsLocation = "PPP";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestLOC_PlacePortOfDischarge_AIR()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.Code = "VWG";
			unloco.RL_IATA = "XXX";
			Factory.Save();

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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = "LOC+11+XXX:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = "LOC+11+XXX:139:6+PPP::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = "LOC+11+XXX:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = "LOC+11+XXX:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = "LOC+11+XXX:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = "LOC+11+XXX:139:6+PPP::ZZZ'" }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_TransportMode = Core.Constants.TransportModes.Air;
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = NatureList.Codes.Import23;
					header.MasterBill.ABL_RL_NKPortOfDischarge = "VWG";
					header.MasterBill.ABL_GoodsLocation = "PPP";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestLOC_PlacePortOfLoading_SEA()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "PPP", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = "LOC+9+VWG:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = "LOC+9+VWG:139:6+PPP::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = "LOC+9+VWG:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = "LOC+9+VWG:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = "LOC+9+VWG:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = "LOC+9+VWG:139:6+PPP::ZZZ'" }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = NatureList.Codes.Export22;
					header.AMA_OA_DischargeTerminalAddress = orgHeader.MainAddress.PK;
					header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					header.MasterBill.ABL_RL_NKPortOfLoading = "VWG";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestLOC_PlacePortOfLoading_AIR()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "PPP", Core.Constants.CountryCodes.SouthAfrica);
			var unloco = Factory.New<RefUNLOCO>();
			unloco.Code = "VWG";
			unloco.RL_IATA = "XXX";
			Factory.Save();

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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = "LOC+9+XXX:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = "LOC+9+XXX:139:6+PPP::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = "LOC+9+XXX:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = "LOC+9+XXX:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = "LOC+9+XXX:139:6+::ZZZ'" },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = "LOC+9+XXX:139:6+PPP::ZZZ'" }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_OA_DischargeTerminalAddress = orgHeader.MainAddress.PK;
					header.AMA_TransportMode = Core.Constants.TransportModes.Air;
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = NatureList.Codes.Export22;
					header.MasterBill.ABL_RL_NKPortOfLoading = "VWG";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.OutturnProvider = "02";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("NAD+MS+02::ZZZ'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestNAD_MessageSender_OutturnFacilityCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "VW", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			Factory.Save();

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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.OutturnProvider = "VW";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("NAD+MS+00000001::ZZZ'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestNAD_OutturnProviderCode()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.OutturnProvider = "02";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("NAD+RL+02::ZZZ'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestEQD_EquipmentDetails()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "VWG";
			Factory.Save();

			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var nature in new[]
				{
					new { Nature = NatureList.Codes.Import23, ContainerStatus = "3" },
					new { Nature = NatureList.Codes.Export22, ContainerStatus = "2" },
					new { Nature = NatureList.Codes.Transhipment28, ContainerStatus = "6" },
					new { Nature = NatureList.Codes.Transit24, ContainerStatus = "1" },
					new { Nature = NatureList.Codes.FreightRemainingOnBoard, ContainerStatus = "" }
				})
				{
					foreach (var emptyFullIndicator in new[]
					{
						new { EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer, ServiceType = "4" },
						new { EmptyFullIndicator = EmptyFullList.Codes.FullContainerLoad, ServiceType = "5" },
						new { EmptyFullIndicator = EmptyFullList.Codes.LessThanFullContainerLoad, ServiceType = "7" },
					})
					{
						foreach (var testCase in new[]
						{
							new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = $"EQD+CN+123+VWG:102:5++{nature.ContainerStatus}+{emptyFullIndicator.ServiceType}'" },
							new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = "EQD+CN'" },
							new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = "EQD+CN+123'" },
							new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = "EQD+CN+123'" },
							new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = "EQD+CN+123'" },
							new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = "EQD+CN+123'" }
						})
						{
							var header = Factory.New<AsycudaManifestHeader>();
							header.AMA_ManifestType = testCase.ManifestType;
							header.AMA_Nature = nature.Nature;
							var container = header.Containers.AddNew();
							container.ACN_ContainerNumber = "123";
							container.ACN_RC_ContainerType = refContainer.PK;
							container.ACN_EmptyFullIndicator = emptyFullIndicator.EmptyFullIndicator;
							new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
							header.Messages.Load();
							var message = header.Messages[0];

							assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}-{nature.Nature}-{emptyFullIndicator.EmptyFullIndicator}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
						}
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestEQD_EquipmentDetails_NoContainers()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var nature in new[]
				{
					new { Nature = NatureList.Codes.Import23, ContainerStatus = "3" },
					new { Nature = NatureList.Codes.Export22, ContainerStatus = "2" },
					new { Nature = NatureList.Codes.Transhipment28, ContainerStatus = "6" },
					new { Nature = NatureList.Codes.Transit24, ContainerStatus = "1" },
					new { Nature = NatureList.Codes.FreightRemainingOnBoard, ContainerStatus = "" }
				})
				{
					foreach (var testCase in new[]
					{
						new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = nature.ContainerStatus.IsNullOrEmpty() ? "EQD+BB+1+:102:5'" : $"EQD+BB+1+:102:5++{nature.ContainerStatus}'" },
						new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = "EQD+BB'" },
						new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = "EQD+BB+1'" },
						new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = "EQD+UL+1'" },
						new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = "EQD+UL+1'" },
						new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = "EQD+UL+1'" }
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_ManifestType = testCase.ManifestType;
						header.AMA_Nature = nature.Nature;
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}-{nature.Nature}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestDTM_DateTimeUnpackedDeconsolidated()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var container = header.Containers.AddNew();
					container.ContUnpackTime = new ZDateTime(2018, 03, 22);
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("DTM+164:201803220000:203'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestDTM_DateTimeFullyUnloaded()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					header.FullyLoadedUnloadedDate = new ZDateTime(2018, 03, 22);
					var container = header.Containers.AddNew();
					container.ContUnpackTime = new ZDateTime(2018, 03, 22);
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("DTM+420:201803220000:203'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestSEL_SealNumber()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var sealParty in new[]
				{
					new { SealParty = SealTypeList.Codes.AgentForwarder, Code = "AB" },
					new { SealParty = SealTypeList.Codes.Carrier, Code = "CA" },
					new { SealParty = SealTypeList.Codes.Customs, Code = "CU" },
					new { SealParty = SealTypeList.Codes.ExporterShipper, Code = "SH" },
					new { SealParty = SealTypeList.Codes.TerminalOperator, Code = "TO" },
				})
				{
					foreach (var testCase in new[]
					{
						new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true, Snippet = $"SEL+123+{sealParty.Code}+1'" },
						new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true, Snippet = $"SEL++{sealParty.Code}+1'" },
						new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true , Snippet = $"SEL+123+{sealParty.Code}+1'" },
						new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true , Snippet = $"SEL+123+{sealParty.Code}+1'" },
						new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true , Snippet = $"SEL+123+{sealParty.Code}+1'" },
						new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true, Snippet = $"SEL+123+{sealParty.Code}+1'" }
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_ManifestType = testCase.ManifestType;
						var container = header.Containers.AddNew();
						container.ACN_Seal1 = "123";
						container.ACN_SealingPartyType = sealParty.SealParty;
						var bill = header.Bills.AddNew();
						var pack = bill.Packs.AddNew();
						pack.ContainerPK = container.PK;
						pack.Outturn.C5_SealIntactIndicator = true;
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}-{sealParty.SealParty}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestSEL_SealNumber_NoContainer()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var sealParty in new[]
				{
					new { SealParty = SealTypeList.Codes.AgentForwarder, Code = "" },
					new { SealParty = SealTypeList.Codes.Carrier, Code = "" },
					new { SealParty = SealTypeList.Codes.Customs, Code = "" },
					new { SealParty = SealTypeList.Codes.ExporterShipper, Code = "" },
					new { SealParty = SealTypeList.Codes.TerminalOperator, Code = "" },
				})
				{
					foreach (var testCase in new[]
					{
						new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true, Snippet = "SEL'" },
						new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true, Snippet = "SEL'" },
						new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true , Snippet = "SEL'" },
						new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true , Snippet = "SEL'" },
						new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true , Snippet = "SEL'" },
						new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true, Snippet = "SEL'" }
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_ManifestType = testCase.ManifestType;
						var bill = header.Bills.AddNew();
						var pack = bill.Packs.AddNew();
						pack.Outturn.C5_SealIntactIndicator = true;
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}-{sealParty.SealParty}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestCNI_ConsignmentInformation()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					bill.ABL_BillNumber = "123";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("CNI+1+123'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestRFF_CargoCarrierCode()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					bill.ABL_BillIssuer = "123";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("RFF+AHY:123'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestRFF_ExternalReference()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_JobReference = "VWG123";
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("RFF+AIH:VWG123'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestRFF_MasterCargoCarrierCode()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "VVV";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "GGG", Core.Constants.CountryCodes.SouthAfrica);
			var carrierAddress = carrier.Addresses.AddNew();
			carrierAddress.OA_Address1 = "VVV Address";
			Factory.Save();

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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					foreach (var transportMode in new[]
					{
						new { TransportMode = Core.Constants.TransportModes.Air, Snippet = "RFF+AAZ:GGG'" },
						new { TransportMode = Core.Constants.TransportModes.Sea, Snippet = "RFF+AAZ:GGG'" },
						new { TransportMode = Core.Constants.TransportModes.Road, Snippet = "RFF+AAZ:GGG'" },
						new { TransportMode = Core.Constants.TransportModes.Rail, Snippet = "RFF+AAZ:GGG'" },
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_ManifestType = testCase.ManifestType;
						header.AMA_OA_Carrier = carrierAddress.PK;
						header.AMA_TransportMode = transportMode.TransportMode;
						var bill = header.Bills.AddNew();
						bill.ABL_BillNumber = "VWG";
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}-{transportMode.TransportMode}", testCase.Contains, message.EM_MessageText.Contains(transportMode.Snippet, StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestRFF_MasterTransportDocumentNumber()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					foreach (var agent in new[]
					{
						new { AgentType = Core.Constants.AgentType.Agent, Snippet = "RFF+MB:123::CO'" },
						new { AgentType = Core.Constants.AgentType.CoLoad, Snippet = "RFF+MB:123::CO'" },
						new { AgentType = Core.Constants.AgentType.Other, Snippet = "RFF+MB:123::CO'" },
						new { AgentType = Core.Constants.AgentType.AWBCoload, Snippet = "RFF+MB:123::ST'" },
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_ManifestType = testCase.ManifestType;
						header.MasterBill.ABL_BillNumber = "123";
						header.AMA_AgentType = agent.AgentType;
						var bill = header.Bills.AddNew();
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}-{agent.AgentType}", testCase.Contains, message.EM_MessageText.Contains(agent.Snippet, StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestRFF_LRNExist()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					foreach (var testSubCase in new[]
					{
						new { Nature = NatureList.Codes.Import23, Contains = false },
						new { Nature = NatureList.Codes.Export22, Contains = true },
						new { Nature = NatureList.Codes.Transhipment28, Contains = false },
						new { Nature = NatureList.Codes.Transit24, Contains = false },
						new { Nature = NatureList.Codes.FreightRemainingOnBoard, Contains = false },
					})
					{
						var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
						header.AMA_ManifestType = testCase.ManifestType;
						header.AMA_Nature = testSubCase.Nature;
						var bill = header.Bills.AddNew();
						bill.ABL_BillNumber = "456";
						bill.CustomsCPC = "12";
						var entryNumber = Factory.New<CusEntryNumber>();
						entryNumber.CE_EntryNum = "123";
						entryNumber.CE_EntryType = Enterprise.Customs.Common.CusEntryNumberTypes.SouthAfrica.LocalReferenceNumber;
						entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
						entryNumber.CE_ParentID = bill.PK;
						entryNumber.CE_ParentTable = bill.TableName;
						Factory.Save();

						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}-{testSubCase.Nature}", testCase.Contains && testSubCase.Contains, message.EM_MessageText.Contains("RFF+ABT:123::12'", StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestGID_GoodsItemDetails()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var subType in new[]
			{
				MessageSubTypes.Create,
				MessageSubTypes.Change,
				MessageSubTypes.Withdraw
			})
			{
				foreach (var containerMode in new[]
				{
					new { ContainerMode = Core.Constants.ContainerModes.Containerised, TypeOfPackages = "12", Indicator = "CN" },
					new { ContainerMode = Core.Constants.ContainerModes.BreakBulk, TypeOfPackages = "12", Indicator = "BB" },
					new { ContainerMode = Core.Constants.ContainerModes.Liquid, TypeOfPackages = "12" , Indicator = "LB" },
					new { ContainerMode = Core.Constants.ContainerModes.Bulk, TypeOfPackages = "12", Indicator = "DB" },
				})
				{
					foreach (var testCase in new[]
					{
						new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = $"GID+1+4:{containerMode.TypeOfPackages}:::{containerMode.Indicator}+9" },
						new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = $"GID+1+4:{containerMode.TypeOfPackages}:::{containerMode.Indicator}+9" },
						new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = $"GID+1+:{containerMode.TypeOfPackages}:::{containerMode.Indicator}+9" },
						new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = $"GID+1+4:{containerMode.TypeOfPackages}:::{containerMode.Indicator}+9" },
						new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = $"GID+1+:{containerMode.TypeOfPackages}:::{containerMode.Indicator}+9" },
						new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = $"GID+1+4:{containerMode.TypeOfPackages}:::{containerMode.Indicator}+9" }
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_ManifestType = testCase.ManifestType;
						header.AMA_ContainerMode = containerMode.ContainerMode;
						var bill = header.Bills.AddNew();
						var pack = bill.Packs.AddNew();
						pack.APA_PackQty = 4;
						pack.APA_PackUQ = "12";
						pack.Outturn.C5_PackagesOutturned = 9;
						pack.Outturn.C5_CargoType = containerMode.Indicator;
						new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
						header.Messages.Load();
						var message = header.Messages[0];

						assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}-{containerMode.ContainerMode}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
					}
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestFTX_Condition()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.Outturn.C5_PackageCondition = "123";
					pack.Outturn.PackCondDesc = "456";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("FTX+DAR++123+456'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestFTX_Condition_IfEmpty()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.Outturn.C5_PackageCondition = "";
					pack.Outturn.PackCondDesc = "456";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("FTX+DAR", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestFTX_ContentsFound()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.APA_PackQty = 1;
					pack.Outturn.C5_PackagesOutturned = 2;
					pack.Outturn.C5_GoodsDescription = "345";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("FTX+AAQ+++345'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestFTX_ContentsFound_IfEmpty()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.Outturn.C5_GoodsDescription = "";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("FTX+AAQ", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestFTX_ContentsFoundWhenPacksAreTheSame()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.APA_PackQty = 2;
					pack.Outturn.C5_PackagesOutturned = 2;
					pack.Outturn.C5_GoodsDescription = "345";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("FTX+AAQ+++345'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestFTX_ContentsShouldBe()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = "FTX+AEC++1+345'", Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = "FTX+AEC++1+345'", Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = "FTX+AEC++4+345'", Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = "FTX+AEC++1+345'", Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = "FTX+AEC++4+345'", Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = "FTX+AEC++1+345'", Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.Outturn.ExcessShortInd = ExcessShortIndicatorList.Codes.Excess;
					pack.Outturn.ContShouldBe = "345";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestFTX_DescriptionOfGoods()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = "FTX+AAA+++123'" },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = "FTX+AAA+++123'" },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = "FTX+AAA+++123'" },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = "FTX+AAA+++123'" },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = "FTX+AAA+++123'" },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = "FTX+AAA+++123'" }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.APA_GoodsDescription = "123";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestMEA_GrossWeight()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.APA_Weight = 1m;
					pack.APA_WeightUQ = "KG";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("MEA+AAE+AAB+KGM:1'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestMEA_GrossWeightFound()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.Outturn.C5_WeightOutturned = 1m;
					pack.Outturn.C5_WeightOutturnedUQ = "KG";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("MEA+AAL+ACE+KGM:1'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestMEA_GrossLitres()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.APA_Volume = 1m;
					pack.APA_VolumeUQ = "M3";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("MEA+AAE+ABJ+LTR:1000'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestMEA_GrossLitres_IfEmpty()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.APA_Volume = 0m;
					pack.APA_VolumeUQ = "M3";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("MEA+AAE+ABJ", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestMEA_GrossLitresFound()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.Outturn.C5_VolumeOutturned = 1m;
					pack.Outturn.C5_VolumeOutturnedUQ = "M3";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("MEA+AAL+BRF+LTR:1000'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestMEA_GrossLitresFound_IfEmpty()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = false },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = false }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.Outturn.C5_VolumeOutturned = 0m;
					pack.Outturn.C5_VolumeOutturnedUQ = "M3";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("MEA+AAL+BRF", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestPCI_PackageIdentification()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.APA_MarksAndNumbers = "123456789012345678901234567890123456789012345678901234567890";
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains("PCI+24+12345678901234567890123456789012345:6789012345678901234567890'", StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestSGP_SplitGoodsPlacement()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true, Snippet = "SGP+123+456'" },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true , Snippet = "SGP+123'" },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true , Snippet = "SGP+123+456'" },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true , Snippet = "SGP+123'" },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true , Snippet = "SGP+123+456'" },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true , Snippet = "SGP+123+456'" }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var container = header.Containers.AddNew();
					container.ACN_ContainerNumber = "123";
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.ContainerPK = container.PK;
					pack.Outturn.C5_PackagesOutturned = 456;
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestSGP_SplitGoodsPlacement_NoContainer()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true, Snippet = "SGP+1+0'" },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true , Snippet = "SGP+1'" },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true , Snippet = "SGP+1'" },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true , Snippet = "SGP+1'" },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true , Snippet = "SGP+1'" },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true , Snippet = "SGP+1'" }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.Outturn.C5_PackagesOutturned = 0;
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestSGP_SplitGoodsPlacement_NoContainer_NonEmptyContainerContent()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Contains = true, Snippet = "SGP+1+456'" },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Contains = true , Snippet = "SGP+1'" },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Contains = true , Snippet = "SGP+1+456'" },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Contains = true , Snippet = "SGP+1'" },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Contains = true , Snippet = "SGP+1+456'" },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Contains = true , Snippet = "SGP+1+456'" }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					pack.Outturn.C5_PackagesOutturned = 456;
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestCNT_ControlTotal()
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = "CNT+8:14'" },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = "CNT+8:14'"  },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = "CNT+8:14'"  },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = "CNT+8:14'"  },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = "CNT+8:14'"  },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = "CNT+8:14'"  }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					var bill1 = header.Bills.AddNew();
					var pack11 = bill1.Packs.AddNew();
					pack11.APA_PackQty = 1;
					pack11.Outturn.C5_PackagesOutturned = 2;
					var pack12 = bill1.Packs.AddNew();
					pack12.APA_PackQty = 2;
					pack12.Outturn.C5_PackagesOutturned = 3;
					var bill2 = header.Bills.AddNew();
					var pack21 = bill2.Packs.AddNew();
					pack21.APA_PackQty = 3;
					pack21.Outturn.C5_PackagesOutturned = 4;
					var pack22 = bill2.Packs.AddNew();
					pack22.APA_PackQty = 4;
					pack22.Outturn.C5_PackagesOutturned = 5;
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
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
					new { ManifestType = ManifestTypeList.Codes.DepotOutturnReport, Snippet = subType == MessageSubTypes.Create ? "UNT+11+<<MSGNO PLACEHOLDER>>'" : "UNT+12+<<MSGNO PLACEHOLDER>>'" },
					new { ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport, Snippet = subType == MessageSubTypes.Create ? "UNT+12+<<MSGNO PLACEHOLDER>>'" : "UNT+13+<<MSGNO PLACEHOLDER>>'" },
					new { ManifestType = ManifestTypeList.Codes.VesselOutturnReport, Snippet = subType == MessageSubTypes.Create ? "UNT+12+<<MSGNO PLACEHOLDER>>'" : "UNT+13+<<MSGNO PLACEHOLDER>>'" },
					new { ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport, Snippet = subType == MessageSubTypes.Create ? "UNT+12+<<MSGNO PLACEHOLDER>>'" : "UNT+13+<<MSGNO PLACEHOLDER>>'" },
					new { ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport, Snippet = subType == MessageSubTypes.Create ? "UNT+12+<<MSGNO PLACEHOLDER>>'" : "UNT+13+<<MSGNO PLACEHOLDER>>'" },
					new { ManifestType = ManifestTypeList.Codes.AirLoadDischarge, Snippet = subType == MessageSubTypes.Create ? "UNT+12+<<MSGNO PLACEHOLDER>>'" : "UNT+13+<<MSGNO PLACEHOLDER>>'" }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = testCase.ManifestType;
					new COSTCOMessageBuilder(new COSTCOHeader(header), subType).PopulateMessages();
					header.Messages.Load();
					var message = header.Messages[0];

					assertDelegate += () => { AssertEquals($"{testCase.ManifestType}-{subType}", true, message.EM_MessageText.Contains(testCase.Snippet, StringComparison.OrdinalIgnoreCase)); };
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestMessageContentEscapedWithCorrectCharacterSet()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			var container = header.Containers.AddNew();
			container.ACN_Seal1 = "1-+':?-9";
			container.ACN_SealingPartyType = SealTypeList.Codes.Carrier;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			pack.Outturn.C5_SealIntactIndicator = true;
			new COSTCOMessageBuilder(new COSTCOHeader(header), MessageSubTypes.Create).PopulateMessages();
			header.Messages.Load();
			var message = header.Messages[0].EM_MessageText;

			AssertContains("Escaped Seal", "SEL+1-?+?'?:??-9+CA+1'", message);
		}
	}
}
