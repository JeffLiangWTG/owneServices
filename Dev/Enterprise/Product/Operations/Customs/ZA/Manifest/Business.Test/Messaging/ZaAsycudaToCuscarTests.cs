using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ZA.Manifest.Business.EDIFACT;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;
using static Enterprise.Customs.ASYCUDA.Business.Testing.AsycudaManifestHeaderTestHelper;
using CargoStatusList = Enterprise.Customs.ASYCUDA.Business.CargoStatusList;
using EmptyFullIndicatorList = Enterprise.Customs.ASYCUDA.Business.EmptyFullIndicatorList;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	sealed class ZaAsycudaToCuscarTests : TestCaseWithFactory
	{
		public void TestMessage_NAD_RL_WI00215238()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "Manifest Validation");
			var validationRuleZa = helper.CreateNewOrGetExistingCusCodeList("ZA", RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.BillIssuer, "A Bill issuer is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMESSAGETYPE", nameof(ManifestDocumentType.HAB));
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMANIFESTTYPE", nameof(ManifestDocumentType.COH));
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMANIFESTTYPE", nameof(ManifestDocumentType.ALH));
			Factory.Save();

			VoidParameterlessDelegate assertDelegate = () => { };

			SetupSealingTypeMapping(Factory);

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.COH), ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.BBB), ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.ECL), ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.FFM), ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.FWB), ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.HAB), ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.RMA), ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.RFM), ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.AQM), ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.ALM), ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine, Contains = true, Snippet = "NAD+RL+000'" },
					new { ManifestType = nameof(ManifestDocumentType.ALH), ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator, Contains = true, Snippet = "NAD+RL+000'" },
				})
				{
					assertDelegate += () =>
					{
						var manifest = SetupManifestForAir(testCase.ManifestType, testCase.ApplicationCode);
						manifest.AMA_JobReference = $"{subType}-{testCase.ManifestType}";
						manifest.AMA_AgentType = Core.Constants.AgentType.CoLoad;
						manifest.AMA_ManifestType = testCase.ManifestType;
						manifest.MasterCarrierCode = "000";

						CusCarMessagingHelper.CreateCusCars(manifest, subType);

						var message = manifest.Messages[0];

						AssertEquals($"{subType}-{testCase.ManifestType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
					};
				}
			}
			CombineAssertions(assertDelegate);
		}
		[TestDate(2018, 12, 02)]
		public void TestExitLRN_EntryLRN_WI00211234()
		{
			var manifest = SetupManifestForAir(nameof(ManifestDocumentType.RFM), ApplicationCodeTypeList.Codes.Consolidator);
			manifest.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			manifest.AMA_AgentType = Core.Constants.AgentType.CoLoad;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			manifest.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			manifest.AMA_CarrierCode = "111";
			manifest.MasterCarrierCode = "000";
			manifest.AMA_RadioCallSign = "OVYQ2";
			var bill1 = manifest.Bills[0];
			bill1.ABL_BillIssuer = "ISS1";
			bill1.ABL_ShipmentType = ShipmentTypeList.Codes.Transhipment28;
			bill1.CustomsEntryNumberType = ZaLRNTypes.Codes.AFM;
			bill1.CustomsEntryNumber = "LRN1";
			var bill2 = manifest.Bills.AddNew();
			bill2.ABL_BillIssuer = "ISS1";
			bill2.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			bill2.CustomsEntryNumberType = ZaLRNTypes.Codes.ABT;
			bill2.CustomsEntryNumber = "LRN2";
			var bill3 = manifest.Bills.AddNew();
			bill3.ABL_BillIssuer = "ISS1";
			bill3.ABL_ShipmentType = ShipmentTypeList.Codes.Transit24;
			bill3.CustomsEntryNumberType = ZaLRNTypes.Codes.AFM;
			bill3.CustomsEntryNumber = "LRN3";
			var bill4 = manifest.Bills.AddNew();
			bill4.ABL_BillIssuer = "ISS1";
			bill4.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			bill4.CustomsEntryNumberType = ZaLRNTypes.Codes.AFM;
			bill4.CustomsEntryNumber = "LRN4";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			var message = manifest.Messages[0];

			AssertMultilineASCIIEquals("Expected CUSCAR content", @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::RFM+<<SYSCAR>>+9'
DTM+137:20181202:102'
RFF+LO:C1234'
NAD+RL+000'
NAD+MS+1111111122222222'
TDT+20++4++111:172:20+++:::BA123'
LOC+35+US'
LOC+36+ZA'
LOC+17'
DTM+132::102'
GEI+5+ZZZ:71:ZZZ'
GEI+5+:122:ZZZ'
CNI+1+000111-2222222A::::19710919'
RFF+BM:HAWB1234:1'
LOC+8+JNB'
LOC+9+ATL'
TDT+20'
RFF+AFM:LRN1'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL'
GID+1+69:PK'
FTX+AAA++9+DESC'
MEA+AAE+AAB+KGM:456'
PCI+24+MARKS AND NUMBS'
CST++COMM1'
RFF+BM::2'
LOC+8+JNB'
LOC+9+ATL'
TDT+20'
RFF+ABT:LRN2'
GID+1+0'
FTX+AAA++9'
MEA+AAE+AAB+KGM:0'
PCI+24'
RFF+BM::3'
LOC+8+JNB'
LOC+9+ATL'
TDT+20'
RFF+AFM:LRN3'
GID+1+0'
FTX+AAA++9'
MEA+AAE+AAB+KGM:0'
PCI+24'
RFF+BM::4'
LOC+8+JNB'
LOC+9+ATL'
TDT+20'
RFF+AFM:LRN4'
GID+1+0'
FTX+AAA++9'
MEA+AAE+AAB+KGM:0'
PCI+24'
UNT+54+1'
".Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper()), message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestTDT4_TransportInformation_AIR()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "VWG";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234", "ZA");
			var carrierAddress = carrier.Addresses.AddNew();
			carrierAddress.Address1 = "Address 1";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel";
			vessel.RV_RadioCallSign = "CSN";

			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = true, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = true, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = true, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:146'" },
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_TransportMode = Core.Constants.TransportModes.Air;
					var bill = header.Bills.AddNew();
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
					header.TSS_CargoCarrierPK = carrier.PK;
					header.TSS_VoyageFlight = "FL001";
					header.TSS_Vessel = vessel.RV_Code;

					CusCarMessagingHelper.CreateCusCars(header, subType);
					var message = header.Messages[0];
					assertDelegate += () =>
					{
						AssertEquals($"{subType}-{testCase.ManifestType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
					};
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestTDT4_TransportInformation_SEA()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "VWG";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234", "ZA");
			var carrierAddress = carrier.Addresses.AddNew();
			carrierAddress.Address1 = "Address 1";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel";
			vessel.RV_RadioCallSign = "CSN";
			vessel.RV_CarrierCode = "3456";

			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = true, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = true, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = true, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = false, Snippet = "TDT+4+FL001+++1234:172:20+++CSN:103'" },
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					var bill = header.Bills.AddNew();
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
					header.TSS_CargoCarrierPK = carrier.PK;
					header.TSS_VoyageFlight = "FL001";
					header.TSS_Vessel = vessel.RV_Code;

					CusCarMessagingHelper.CreateCusCars(header, subType);
					var message = header.Messages[0];
					assertDelegate += () =>
					{
						AssertEquals($"{subType}-{testCase.ManifestType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
					};
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestDTM133_DateTimePeriod()
		{
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = false, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = true, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = true, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = false, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = false, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = false, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = true, Snippet = "DTM+133:201811111111:203'" },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = false, Snippet = "DTM+133:201811111111:203'" },
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					var bill = header.Bills.AddNew();
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
					header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
					header.TSS_DateOfDeparture = new ZDateTime(2018, 11, 11, 11, 11, 0);

					CusCarMessagingHelper.CreateCusCars(header, subType);
					var message = header.Messages[0];
					assertDelegate += () =>
					{
						AssertEquals($"{subType}-{testCase.ManifestType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
					};
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestRFF_AFM_LRNMRNEntry()
		{
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var nature in new[]
				{
					ShipmentTypeList.Codes.Transhipment28,
					ShipmentTypeList.Codes.Import23
				})
				{
					foreach (var testCase in new[]
					{
						new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.COH), Contains = false, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = false, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = false, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = false, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = false, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = true, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = true, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = false, Snippet = "RFF+AFM:LRN'" },
						new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = false, Snippet = "RFF+AFM:LRN'" },
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						var bill = header.Bills.AddNew();
						bill.CustomsEntryNumberType = ZaLRNTypes.Codes.AFM;
						bill.CustomsEntryNumber = "LRN";
						bill.ABL_ShipmentType = nature;
						header.AMA_ManifestType = testCase.ManifestType;
						header.AMA_Nature = nature;

						CusCarMessagingHelper.CreateCusCars(header, subType);
						var message = header.Messages[0];
						assertDelegate += () =>
						{
							AssertEquals($"{subType}-{testCase.ManifestType}-{nature}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
						};
					}
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestRFF_ABT_LRNExit()
		{
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = false, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = false, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = false, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = false, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = false, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = true, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = true, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = false, Snippet = "RFF+ABT:LRN'" },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = false, Snippet = "RFF+ABT:LRN'" },
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					var bill = header.Bills.AddNew();
					bill.CustomsEntryNumberType = ZaLRNTypes.Codes.ABT;
					bill.CustomsEntryNumber = "LRN";
					bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = ShipmentTypeList.Codes.Export22;

					CusCarMessagingHelper.CreateCusCars(header, subType);
					var message = header.Messages[0];
					assertDelegate += () =>
					{
						AssertEquals($"{subType}-{testCase.ManifestType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
					};
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestGEI5_ProcessingInformation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var goodsReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "1", "Goods released", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsReleased.PK, "AQM", "desc.");
			var goodsStoppedDetained = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "2", "Goods stopped / detained", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsStoppedDetained.PK, "AQM", "desc.");
			var conditionalReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "3", "Goods may move under Customs transfer (Customs Intervention) - Conditional Release", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(conditionalReleased.PK, "AQM", "desc.");
			var releasedToStatesWarehouse = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "50", "Released to States Warehouse", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(releasedToStatesWarehouse.PK, "AQM", "desc.");
			var other = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "51", "Other (Overboard, destroyed, lost etc)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(other.PK, "AQM", "desc.");
			Factory.Save();
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var relStatus in new[]
			{
				new { RelStatus= "1", Desc="Goods released" },
				new { RelStatus= "2", Desc="Goods stopped / detained" },
				new { RelStatus= "3", Desc="Goods may move under Customs transfer (Customs Intervention) - Conditional Release" },
				new { RelStatus= "50", Desc="Released to States Warehouse" },
			})
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ManifestType = "AQM";
				var bill = header.Bills.AddNew();
				bill.CargoReleaseStatus = relStatus.RelStatus;

				CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
				var message = header.Messages[0];
				assertDelegate += () =>
				{
					AssertEquals($"{relStatus.RelStatus}", true, message.EM_MessageText.Contains($"GEI+5+{relStatus.RelStatus}:42::{relStatus.Desc.ToUpper()}'"));
				};
			}
			CombineAssertions(assertDelegate);
		}

		public void TestGEI5_ProcessingInformation_Other()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "AQM";
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatus = "51";
			bill.CargoReleaseStatusOtherDescription = "Other Desc.";

			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var message = header.Messages[0];
			AssertEquals(true, message.EM_MessageText.Contains("GEI+5+51:42::OTHER DESC.'"));
		}

		public void TestNoSendingContainers_WhenIsAQM()
		{
			SetupGisTypeMapping();

			var header = SetUpManifestFortSea("AQM", ApplicationCodeTypeList.Codes.ShippingLine);
			header.Bills[0].CustomsEntryNumberType = ZaLRNTypes.Codes.AFM;
			VoidParameterlessDelegate assertDelegate = () => { };
			assertDelegate += () =>
			{
				var expectedMsgTxt = @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::AQM+<<SYSCAR>>+9'
DTM+137:19710917:102'
DTM+136:19710917:102'
RFF+LO:C1234'
NAD+RL+000'
NAD+FZ+ISS1'
NAD+MS+1111111122222222'
TDT+20+BA123+1++111:172:20+++OVYQ2:103:::GB'
LOC+60+ZAJNB'
DTM+132:19710918:102'
CNI+1+:BOL:::19710917'
RFF+BM:HAWB1234'
GEI+5+:42'
TDT+20'
RFF+AFM'
GID+1+69:PK'
UNT+18+1'
";

				header.AMA_MasterBillIssueDate = new ZDate(1971, 09, 17);
				header.AMA_CarrierCode = "111";
				header.MasterCarrierCode = "000";
				header.AMA_RadioCallSign = "OVYQ2";
				header.AMA_ManifestType = "AQM";
				header.Messages.RemoveAndDeleteAllFromTest();
				CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
				var message = header.Messages[0];
				expectedMsgTxt = expectedMsgTxt.Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper());

				AssertMultilineASCIIEquals(
					"ManifestType: AQM",
					expectedMsgTxt,
					message.EM_MessageText.Replace("'", "'\r\n")
				);
			};

			CombineAssertions(assertDelegate);
		}

		public void TestCNT16_NoSendingWhenIsAQM()
		{
			SetupGisTypeMapping();
			var header = SetUpManifestFortSea("AQM", ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_MasterBillIssueDate = new ZDate(1971, 09, 17);
			header.AMA_ManifestType = "AQM";
			header.Messages.RemoveAndDeleteAllFromTest();
			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var message = header.Messages[0];
			AssertEquals(false, message.EM_MessageText.Contains("CNT+16"));
		}

		public void TestGID_GoodsItemDetails()
		{
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var containerMode in new[]
				{
					new { ContainerMode = Core.Constants.ContainerModes.Bulk, PackageTypeCode = "VQ" },
					new { ContainerMode = Core.Constants.ContainerModes.Liquid, PackageTypeCode = "VL" },
				})
				{
					foreach (var testCase in new[]
					{
						new { ManifestType = nameof(ManifestDocumentType.COM), Contains = true, Snippet = "GID+1+10:NO'" },
						new { ManifestType = nameof(ManifestDocumentType.COH), Contains = true, Snippet = "GID+1+10:NO'" },
						new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = true, Snippet = "GID+1+10:NO'" },
						new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false, Snippet = "GID+1+10:NO'" },
						new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = true, Snippet = "GID+1+10:NO'" },
						new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = true, Snippet = "GID+1+10:NO'" },
						new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = true, Snippet = "GID+1+10:NO'" },
						new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = true, Snippet = "GID+1+10:NO'" },
						new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = true, Snippet = "GID+1+10:NO'" },
						new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = true, Snippet = "GID+1+10:NO'" },
						new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = true, Snippet = $"GID+1+10:{containerMode.PackageTypeCode}'" },
						new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = true, Snippet = "GID+1+10:NO'" },
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_ManifestType = testCase.ManifestType;
						header.AMA_Nature = ShipmentTypeList.Codes.Import23;
						header.AMA_ContainerMode = containerMode.ContainerMode;
						var bill = header.Bills.AddNew();
						bill.CustomsEntryNumber = "123";
						var pack = bill.Packs.AddNew();
						pack.APA_PackQty = 10;
						pack.APA_PackUQ = "NO";

						CusCarMessagingHelper.CreateCusCars(header, subType);
						var message = header.Messages[0];
						assertDelegate += () =>
						{
							AssertEquals($"{subType}-{testCase.ManifestType}-{containerMode.ContainerMode}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
						};
					}
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestLOC104_DepotOfUnpack()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "VWG", Core.Constants.CountryCodes.SouthAfrica);
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = true, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = true, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = true, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = true, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = false, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = false, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = false, Snippet = "LOC+104+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = true, Snippet = "LOC+104+VWG'" },
				})
				{
					foreach (var nature in new[]
					{
						ShipmentTypeList.Codes.Import23, ShipmentTypeList.Codes.Transit24, ShipmentTypeList.Codes.Transhipment28
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_OA_DeconsolidateAddress = org.PK;
						header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
						header.AMA_ManifestType = testCase.ManifestType;
						header.AMA_Nature = nature;
						var bill = header.Bills.AddNew();

						CusCarMessagingHelper.CreateCusCars(header, subType);
						var message = header.Messages[0];
						assertDelegate += () =>
						{
							AssertEquals($"{subType}-{testCase.ManifestType}-{nature}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
						};
					}
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestLOC104_DepotOfUnpack_IfEmpty()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = true, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = false, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = false, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = true, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = false, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = false, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = false, Snippet = "LOC+104'" },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = true, Snippet = "LOC+104'" },
				})
				{
					foreach (var nature in new[]
					{
						ShipmentTypeList.Codes.Import23, ShipmentTypeList.Codes.Transit24, ShipmentTypeList.Codes.Transhipment28
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_OA_DeconsolidateAddress = org.PK;
						header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
						header.AMA_ManifestType = testCase.ManifestType;
						header.AMA_Nature = nature;
						var bill = header.Bills.AddNew();

						CusCarMessagingHelper.CreateCusCars(header, subType);
						var message = header.Messages[0];
						assertDelegate += () =>
						{
							AssertEquals($"{subType}-{testCase.ManifestType}-{nature}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
						};
					}
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestLOC65_TerminalOfDischarge()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "VWG", Core.Constants.CountryCodes.SouthAfrica);
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = true, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = true, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = true, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = true, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = false, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = false, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = false, Snippet = "LOC+65+VWG'" },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = true, Snippet = "LOC+65+VWG'" },
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_OA_DischargeTerminalAddress = org.PK;
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = ShipmentTypeList.Codes.Import23;
					var bill = header.Bills.AddNew();

					CusCarMessagingHelper.CreateCusCars(header, subType);
					var message = header.Messages[0];
					assertDelegate += () =>
					{
						AssertEquals($"{subType}-{testCase.ManifestType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
					};
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestLOC65_TerminalOfDischarge_IfEmpty()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = false, Snippet = "LOC+65'" },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = false, Snippet = "LOC+65'" },
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_OA_DischargeTerminalAddress = org.PK;
					header.AMA_ManifestType = testCase.ManifestType;
					header.AMA_Nature = ShipmentTypeList.Codes.Import23;
					var bill = header.Bills.AddNew();

					CusCarMessagingHelper.CreateCusCars(header, subType);
					var message = header.Messages[0];
					assertDelegate += () =>
					{
						AssertEquals($"{subType}-{testCase.ManifestType}", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
					};
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestCUSCARMessageTextBuilder_GrossVolume()
		{
			SetupGisTypeMapping();

			var header = SetUpManifestFortSea("COM", ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_ContainerMode = "LQD";
			header.AMA_ManifestType = "COM";
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var pack = header.Bills[0].Packs[0];
			pack.APA_Volume = 123.34m;
			pack.APA_VolumeUQ = "M3";
			header.Messages.RemoveAndDeleteAllFromTest();
			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var message = header.Messages[0];
			AssertContains("MEA+AAE+AAW+MTQ:123'", message.EM_MessageText);

			pack.APA_Volume = 123.34m;
			pack.APA_VolumeUQ = "L";
			header.Messages.RemoveAndDeleteAllFromTest();
			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			message = header.Messages[0];
			AssertContains("MEA+AAE+AAW+LTR:123.34'", message.EM_MessageText);
		}

		public void TestSG5_TwoContainers()
		{
			SetupSealingTypeMapping(Factory);
			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_TareWeight = 1M;
			refContainer1.RC_Code = "V1";
			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_TareWeight = 2M;
			refContainer2.RC_Code = "V2";
			Factory.Save();

			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var nature in new[]
				{
					new { Nature = ShipmentTypeList.Codes.Import23, ContainerStatus = "3" },
					new { Nature = ShipmentTypeList.Codes.Export22, ContainerStatus = "2" },
					new { Nature = ShipmentTypeList.Codes.Transhipment28, ContainerStatus = "6" },
					new { Nature = ShipmentTypeList.Codes.Transit24, ContainerStatus = "1" },
				})
				{
					foreach (var testCase in new[]
					{
						new { ManifestType = nameof(ManifestDocumentType.COM), Contains = true, Snippet = $"EQD+CN+DANU1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:2'MEA+AAE+VGM+KGM:3'SEL+S1+CA'SEL+S2+TO'SEL+S3+CU'EQD+CN+WANG1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:3'MEA+AAE+VGM+KGM:5'SEL+V1+CA'SEL+V2+TO'SEL+V3+CU'" },
						new { ManifestType = nameof(ManifestDocumentType.COH), Contains = true, Snippet = $"EQD+CN+DANU1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:2'MEA+AAE+VGM+KGM:3'SEL+S1+CA'SEL+S2+TO'SEL+S3+CU'EQD+CN+WANG1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:3'MEA+AAE+VGM+KGM:5'SEL+V1+CA'SEL+V2+TO'SEL+V3+CU'" },
						new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = false, Snippet = "EQD+CN" },
						new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = true, Snippet = $"EQD+CN+DANU1234567+:102++{nature.ContainerStatus}+8'EQD+CN+WANG1234567+:102++{nature.ContainerStatus}+8'" },
						new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false, Snippet = "EQD+CN" },
						new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = false, Snippet = "EQD+CN" },
						new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = false, Snippet = "EQD+CN" },
						new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = true, Snippet = $"EQD+CN+DANU1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:2'MEA+AAE+VGM+KGM:3'SEL+S1+CA'SEL+S2+TO'SEL+S3+CU'EQD+CN+WANG1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:3'MEA+AAE+VGM+KGM:5'SEL+V1+CA'SEL+V2+TO'SEL+V3+CU'" },
						new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = true, Snippet = $"EQD+CN+DANU1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:2'MEA+AAE+VGM+KGM:3'SEL+S1+CA'SEL+S2+TO'SEL+S3+CU'EQD+CN+WANG1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:3'MEA+AAE+VGM+KGM:5'SEL+V1+CA'SEL+V2+TO'SEL+V3+CU'" },
						new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false, Snippet = "EQD+CN" },
						new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = true, Snippet = $"EQD+CN+DANU1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:2'MEA+AAE+VGM+KGM:3'SEL+S1+CA'SEL+S2+TO'SEL+S3+CU'EQD+CN+WANG1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:3'MEA+AAE+VGM+KGM:5'SEL+V1+CA'SEL+V2+TO'SEL+V3+CU'" },
						new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = true, Snippet = $"EQD+CN+DANU1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:2'MEA+AAE+VGM+KGM:3'SEL+S1+CA'SEL+S2+TO'SEL+S3+CU'EQD+CN+WANG1234567+:102++{nature.ContainerStatus}+8'MEA+AAE+AAM+KGM:3'MEA+AAE+VGM+KGM:5'SEL+V1+CA'SEL+V2+TO'SEL+V3+CU'" },
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_Nature = nature.Nature;
						header.AMA_ManifestType = testCase.ManifestType;
						var bill = header.Bills.AddNew();
						bill.CustomsEntryNumber = "123";

						var cont1 = header.Containers.AddNew();
						cont1.ACN_ContainerNumber = "DANU1234567";
						cont1.ACN_RC_ContainerType = TwentyFootNOR;
						cont1.ACN_Seal1 = "S1";
						cont1.ACN_Seal2 = "S2";
						cont1.ACN_Seal3 = "S3";
						cont1.ACN_SealingPartyName = "Daniel";
						cont1.ACN_SealingPartyType = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
						cont1.ACN_SealingPartyType2 = Core.Constants.ContainerSealParties.Codes.Terminal;
						cont1.ACN_SealingPartyType3 = Core.Constants.ContainerSealParties.Codes.Customs;
						cont1.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.FullContainerLoad;
						cont1.ACN_NumberOfPackages = 69;
						cont1.ACN_GoodsWeight = 2M;
						cont1.ACN_GoodsWeightUQ = "KG";
						cont1.ACN_RC_ContainerType = refContainer1.PK;
						var cont2 = header.Containers.AddNew();
						cont2.ACN_ContainerNumber = "WANG1234567";
						cont2.ACN_RC_ContainerType = TwentyFootNOR;
						cont2.ACN_Seal1 = "V1";
						cont2.ACN_Seal2 = "V2";
						cont2.ACN_Seal3 = "V3";
						cont2.ACN_SealingPartyName = "Daniel";
						cont2.ACN_SealingPartyType = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
						cont2.ACN_SealingPartyType2 = Core.Constants.ContainerSealParties.Codes.Terminal;
						cont2.ACN_SealingPartyType3 = Core.Constants.ContainerSealParties.Codes.Customs;
						cont2.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.FullContainerLoad;
						cont2.ACN_NumberOfPackages = 69;
						cont2.ACN_GoodsWeight = 3M;
						cont2.ACN_GoodsWeightUQ = "KG";
						cont2.ACN_RC_ContainerType = refContainer2.PK;

						var pack1 = bill.Packs.AddNew();
						pack1.ContainerPK = cont1.PK;
						var pack2 = bill.Packs.AddNew();
						pack2.ContainerPK = cont2.PK;

						CusCarMessagingHelper.CreateCusCars(header, subType);
						var message = header.Messages[0];
						assertDelegate += () =>
						{
							AssertEquals($"{subType}-{testCase.ManifestType}-{nature.Nature} => '{message.EM_MessageText}' *CONTAINS* '{testCase.Snippet}'", testCase.Contains, message.EM_MessageText.Contains(testCase.Snippet));
						};
					}
				}

				CombineAssertions(assertDelegate);
			}
		}

		public void TestGEI5_ManifestType()
		{
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var subType in new[]
			{
				MessageSubTypeCodes.Codes.Original,
				MessageSubTypeCodes.Codes.Change,
				MessageSubTypeCodes.Codes.Cancellation
			})
			{
				foreach (var mainfestType in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = false },
				})
				{
					foreach (var testCase in new[]
					{
						new { Nature = ShipmentTypeList.Codes.Export22, Snippet = "GEI+5+22:71:ZZZ'" },
						new { Nature = ShipmentTypeList.Codes.Import23, Snippet = "GEI+5+23:71:ZZZ'" },
						new { Nature = ShipmentTypeList.Codes.Transit24, Snippet = "GEI+5+24:71:ZZZ'" },
						new { Nature = ShipmentTypeList.Codes.Transhipment28, Snippet = "GEI+5+28:71:ZZZ'" }
					})
					{
						var header = Factory.New<AsycudaManifestHeader>();
						header.AMA_ManifestType = mainfestType.ManifestType;
						header.AMA_Nature = testCase.Nature;
						var bill = header.Bills.AddNew();
						CusCarMessagingHelper.CreateCusCars(header, subType);
						var message = header.Messages[0];
						assertDelegate += () =>
						{
							AssertEquals($"{subType}-{mainfestType.ManifestType}-{testCase.Nature}", mainfestType.Contains, message.EM_MessageText.Replace("'", "'\r\n").Contains(testCase.Snippet));
						};
					}
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestMasterBol_ManifestTypeIsCOH_AgentTypeIsCLD()
		{
			var header = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			header.AMA_AgentType = "CLD";
			header.MasterBOL = "123";
			header.AMA_MasterBillIssueDate = new ZDate(2017, 12, 2);
			header.AMA_MasterBill = "9994555666";
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var message = header.Messages[0];
			AssertContains("CNI+1+999-4555666:PBL:123::20171202'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestMasterBol_ManifestTypeIsCOH_AgentTypeIsEmpty()
		{
			var header = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			header.AMA_AgentType = "";
			header.MasterBOL = "123";
			header.AMA_MasterBillIssueDate = new ZDate(2017, 12, 2);
			header.AMA_MasterBill = "9994555666";
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var message = header.Messages[0];
			AssertContains("CNI+1+999-4555666:BOL:999-4555666::20171202'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestMasterBol_ManifestTypeIsHAB_AgentTypeIsCLD()
		{
			var header = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.HAB), Factory);
			header.AMA_AgentType = "CLD";
			header.MasterBOL = "123";
			header.AMA_MasterBillIssueDate = new ZDate(2017, 12, 2);
			header.AMA_MasterBill = "9994555666";
			header.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var message = header.Messages[0];
			AssertContains("CNI+1+999-4555666:PBL:123::20171202'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestMasterBol_ManifestTypeIsHAB_AgentTypeIsEmpty()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.HAB), Factory);
			manifest.AMA_AgentType = "";
			manifest.MasterBOL = "123";
			manifest.AMA_MasterBillIssueDate = new ZDate(2017, 12, 2);
			manifest.AMA_MasterBill = "9994555666";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("CNI+1+999-4555666:BOL:999-4555666::20171202'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestMasterBol_ManifestTypeIsALH_AgentTypeIsCLD()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALH), Factory);
			manifest.AMA_AgentType = "CLD";
			manifest.MasterBOL = "123";
			manifest.AMA_MasterBillIssueDate = new ZDate(2017, 12, 2);
			manifest.AMA_MasterBill = "9994555666";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("CNI+1+999-4555666:PBL:123::20171202'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestMasterBol_ManifestTypeIsALH_AgentTypeIsEmpty()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALH), Factory);
			manifest.AMA_AgentType = "";
			manifest.MasterBOL = "123";
			manifest.AMA_MasterBillIssueDate = new ZDate(2017, 12, 2);
			manifest.AMA_MasterBill = "9994555666";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("CNI+1+999-4555666:BOL:999-4555666::20171202'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestMasterBol_ManifestTypeIsALM()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALM), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_MasterBillIssueDate = new ZDate(2017, 12, 2);
			manifest.AMA_MasterBill = "9994555666";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALM);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("CNI+1+999-4555666:BOL:999-4555666::20171202'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestMasterBol_ManifestTypeIsBBB()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.BBB), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_MasterBillIssueDate = new ZDate(2017, 12, 2);
			manifest.AMA_MasterBill = "9994555666";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.BBB);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("CNI+1+999-4555666::999-4555666::20171202'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestMasterBol_ManifestTypeIsCOM()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COM), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_MasterBillIssueDate = new ZDate(2017, 12, 2);
			manifest.AMA_MasterBill = "9994555666";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COM);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("CNI+1+999-4555666::999-4555666::20171202'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestMasterBol_ManifestTypeIsFWB()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.FWB), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_MasterBillIssueDate = new ZDate(2017, 12, 2);
			manifest.AMA_MasterBill = "9994555666";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.FWB);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("CNI+1+999-4555666::999-4555666::20171202'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestUCRNumber()
		{
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var nature in new ZaShipmentTypeList().GetAllCodes())
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = true },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = true },
				})
				{
					var manifest = Factory.New<AsycudaManifestHeader>();
					manifest.AMA_ManifestType = testCase.ManifestType;
					manifest.AMA_Nature = nature;
					var bill = manifest.Bills.AddNew();
					bill.CustomsEntryNumber = "123";
					bill.ABL_UCRNumber = "UCR001";

					CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
					var message = manifest.Messages[0];
					assertDelegate += () =>
					{
						AssertEquals($"{testCase.ManifestType}-{nature}", testCase.Contains, message.EM_MessageText.Contains("RFF+UCN:UCR001'"));
					};
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestUCRNumber_Empty()
		{
			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var nature in new ZaShipmentTypeList().GetAllCodes())
			{
				foreach (var testCase in new[]
				{
					new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.COH), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = false },
					new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = false },
				})
				{
					var manifest = Factory.New<AsycudaManifestHeader>();
					manifest.AMA_ManifestType = testCase.ManifestType;
					manifest.AMA_Nature = nature;
					var bill = manifest.Bills.AddNew();
					bill.ABL_UCRNumber = ZString.Empty;

					CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
					var message = manifest.Messages[0];
					assertDelegate += () =>
					{
						AssertEquals($"{testCase.ManifestType}-{nature}", testCase.Contains, message.EM_MessageText.Contains("RFF+UCN'"));
					};
				}
			}

			CombineAssertions(assertDelegate);
		}

		public void TestPlaceOfLoading_COM()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "JPG1S";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = "OOO";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COM), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_RL_NKPortOfLoading = "JPG1S";
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COM);
			var bill = manifest.Bills[0];
			bill.ABL_UCRNumber = "VWG";
			bill.CustomsEntryNumberType = ZaLRNTypes.Codes.ABT;
			bill.CustomsEntryNumber = "TST";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+9+OOO'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPlaceOfLoading_COH()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "JPG1S";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = "OOO";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_RL_NKPortOfLoading = "JPG1S";
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var bill = manifest.Bills[0];
			bill.ABL_UCRNumber = "VWG";
			bill.CustomsEntryNumberType = ZaLRNTypes.Codes.ABT;
			bill.CustomsEntryNumber = "TST";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+9+OOO'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPlaceOfLoading_BBB()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "JPG1S";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = "OOO";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.BBB), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_RL_NKPortOfLoading = "JPG1S";
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.BBB);
			var bill = manifest.Bills[0];
			bill.ABL_UCRNumber = "VWG";
			bill.CustomsEntryNumberType = ZaLRNTypes.Codes.ABT;
			bill.CustomsEntryNumber = "TST";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+9+OOO'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPlaceOfLoading_ECL()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "JPG1S";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = "OOO";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ECL), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_RL_NKPortOfLoading = "JPG1S";
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ECL);
			var bill = manifest.Bills[0];
			bill.ABL_UCRNumber = "VWG";
			bill.CustomsEntryNumberType = ZaLRNTypes.Codes.ABT;
			bill.CustomsEntryNumber = "TST";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+9+OOO'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPlaceOfLoading_FWB()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "JPG1S";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = "OOO";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.FWB), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_RL_NKPortOfLoading = "JPG1S";
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.FWB);
			var bill = manifest.Bills[0];
			bill.ABL_UCRNumber = "VWG";
			bill.CustomsEntryNumberType = ZaLRNTypes.Codes.ABT;
			bill.CustomsEntryNumber = "TST";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+9+OOO'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPlaceOfLoading_HAB()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "JPG1S";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = "OOO";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.HAB), Factory);
			manifest.AMA_RL_NKPortOfLoading = "JPG1S";
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			var bill = manifest.Bills[0];
			bill.ABL_UCRNumber = "VWG";
			bill.CustomsEntryNumberType = ZaLRNTypes.Codes.ABT;
			bill.CustomsEntryNumber = "TST";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+9+OOO'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestEstimatedTimeOfLoading_ALH()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALH), Factory);
			manifest.EstimatedTimeOfLoading = new ZDateTime(2017, 07, 27, 09, 52, 0);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("DTM+369:201707270952:203'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestVersionChange()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("UNH+1+CUSCAR:D:16A:UN:RCG001'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCARNNumber_Create()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			manifest.RegistrationNumber = "CARN01123456";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertNotContains("RFF+AFB:CARN01123456'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCARNNumber_Amend()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			manifest.RegistrationNumber = "CARN01123456";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Change);
			var message = manifest.Messages[0];
			AssertContains("RFF+AFB:CARN01123456'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCARNNumber_Cancel()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			manifest.RegistrationNumber = "CARN01123456";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Cancellation);
			var message = manifest.Messages[0];
			AssertContains("RFF+AFB:CARN01123456'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPrincipalReferenceNumber_COH()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_Voyage = "V345";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("RFF+ACL:V345'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPrincipalReferenceNumber_HAB()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.HAB), Factory);
			manifest.AMA_Voyage = "V345";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("RFF+ACL:V345'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPrincipalReferenceNumber_ALH()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALH), Factory);
			manifest.AMA_Voyage = "V345";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("RFF+ACL:V345'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPrincipalReferenceNumber_COM()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COM), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_Voyage = "V345";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COM);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("RFF+ACL:V345'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPrincipalReferenceNumber_BBB()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.BBB), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_Voyage = "V345";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.BBB);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("RFF+ACL:V345'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPrincipalReferenceNumber_ECL()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ECL), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_Voyage = "V345";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ECL);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("RFF+ACL:V345'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPrincipalReferenceNumber_FWB()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.FWB), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_Voyage = "V345";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.FWB);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("RFF+ACL:V345'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPrincipalReferenceNumber_ALM()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALM), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_Voyage = "V345";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALM);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("RFF+ACL:V345'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestBorderCrossingPlace_RFM()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RFM), Factory);
			manifest.AMA_CustomsOffice = "BBR";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+17+BBR'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestConsignmentExitCustomsOfficeLocation_RFM()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RFM), Factory);
			manifest.PlaceOfExit = "KOM";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+42+KOM'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCargoType_RFM()
		{
			var startDate = ZDateTime.FromSqlFormat("1900-01-01 00:00:00.000");
			var endDate = ZDateTime.FromSqlFormat("2079-06-06 00:00:00.000");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.CMODE, "OUT", "Container Mode Mapping", true);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.CMODE, Core.Constants.ContainerModes.BreakBulk, "BB", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RFM), Factory);
			manifest.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("GEI+5+BB:122:ZZZ'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCargoType_BBB()
		{
			var startDate = ZDateTime.FromSqlFormat("1900-01-01 00:00:00.000");
			var endDate = ZDateTime.FromSqlFormat("2079-06-06 00:00:00.000");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.CMODE, "OUT", "Container Mode Mapping", true);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.CMODE, Core.Constants.ContainerModes.BreakBulk, "BB", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.BBB), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.BBB);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("GEI+5+BB:122:ZZZ'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCargoType_RMA()
		{
			var startDate = ZDateTime.FromSqlFormat("1900-01-01 00:00:00.000");
			var endDate = ZDateTime.FromSqlFormat("2079-06-06 00:00:00.000");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.CMODE, "OUT", "Container Mode Mapping", true);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.CMODE, Core.Constants.ContainerModes.BreakBulk, "BB", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RMA), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RMA);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("GEI+5+BB:122:ZZZ'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCallPurpose_HAB()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.HAB), Factory);
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			manifest.AMA_RL_NKPortOfLoading = "ZAJNB";
			manifest.AMA_RL_NKPortOfDischarge = "USATL";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("GEI+5+17:176:ZZZ'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCallPurpose_COH()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			manifest.AMA_RL_NKPortOfLoading = "ZAJNB";
			manifest.AMA_RL_NKPortOfDischarge = "USATL";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("GEI+5+17:176:ZZZ'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestContainerStatus_COH()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var bill = manifest.Bills[0];
			var cont = manifest.Containers[0];
			cont.ACN_ContainerNumber = "AAAA1111111";
			var pack1A = bill.Packs.AddNew();
			pack1A.ContainerPK = cont.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("EQD+CN+AAAA1111111+22R0:102:3++3+8'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestContainerStatus_RFM()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RFM), Factory);
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			var bill = manifest.Bills[0];
			var cont = manifest.Containers[0];
			cont.ACN_ContainerNumber = "AAAA1111111";
			var pack1A = bill.Packs.AddNew();
			pack1A.ContainerPK = cont.PK;
			bill.CustomsEntryNumbers.AddNew().PackPivots.AddPivotFor(pack1A);
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("EQD+CN+AAAA1111111+22R0:102:3++3+8'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestContainerStatus_ALH()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALH), Factory);
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			var bill = manifest.Bills[0];
			var cont = manifest.Containers[0];
			cont.ACN_ContainerNumber = "AAAA1111111";
			var pack1A = bill.Packs.AddNew();
			pack1A.ContainerPK = cont.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("EQD+CN+AAAA1111111+22R0:102:3++3+8'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestVerifiedGrossMass_COH()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var cont1 = manifest.Containers[0];
			cont1.ACN_GoodsWeight = 100M;
			cont1.ACN_GoodsWeightUQ = "KG";
			var cont2 = manifest.Containers.AddNew();
			cont2.ACN_GoodsWeight = 300M;
			cont2.ACN_GoodsWeightUQ = "KG";
			var bill = manifest.Bills[0];
			var pack1 = bill.Packs.AddNew();
			pack1.ContainerPK = cont1.PK;
			var pack2 = bill.Packs.AddNew();
			pack2.ContainerPK = cont2.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("MEA+AAE+VGM+KGM:3050'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestVerifiedGrossMass_ALH()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALH), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			var cont1 = manifest.Containers[0];
			cont1.ACN_GoodsWeight = 100M;
			cont1.ACN_GoodsWeightUQ = "KG";
			var cont2 = manifest.Containers.AddNew();
			cont2.ACN_GoodsWeight = 300M;
			cont2.ACN_GoodsWeightUQ = "KG";
			var bill = manifest.Bills[0];
			var pack1 = bill.Packs.AddNew();
			pack1.ContainerPK = cont1.PK;
			var pack2 = bill.Packs.AddNew();
			pack2.ContainerPK = cont2.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("MEA+AAE+VGM+KGM:3050'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPlaceOfDispatch_COH()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "VWg";
			var address = orgHeader.Addresses.AddNew();
			address.Address1 = "xxx";
			address.CompanyName = "qing";
			address.OA_RL_NKRelatedPortCode = "JPXDK";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var bill = manifest.Bills[0];
			bill.ABL_OA_Shipper = address.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+80+ATL'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPlaceOfDispatch_HAB()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "VWg";
			var address = orgHeader.Addresses.AddNew();
			address.Address1 = "xxx";
			address.CompanyName = "qing";
			address.OA_RL_NKRelatedPortCode = "JPXDK";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.HAB), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			var bill = manifest.Bills[0];
			bill.ABL_OA_Shipper = address.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+80+ATL'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestPlaceOfDispatch_ALH()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "VWg";
			var address = orgHeader.Addresses.AddNew();
			address.Address1 = "xxx";
			address.CompanyName = "qing";
			address.OA_RL_NKRelatedPortCode = "JPXDK";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALH), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			var bill = manifest.Bills[0];
			bill.ABL_OA_Shipper = address.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("LOC+80+ATL'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestConsignorPostalAddress_ALH()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "VWg";
			var address = orgHeader.Addresses.AddNew();
			address.Address1 = "xxx";
			address.CompanyName = "qing";
			var postalAddress = orgHeader.Addresses.AddNew();
			postalAddress.Address1 = "123";
			postalAddress.Address2 = "456";
			postalAddress.City = "Nanjing";
			postalAddress.Postcode = "789890";
			postalAddress.State = "JN";
			postalAddress.OA_RN_NKCountryCode = "CN";
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALH), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			var bill = manifest.Bills[0];
			bill.ABL_OA_Shipper = address.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("NAD+CZ++QING:XXX'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestConsignorPostalAddress_HAB()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "VWg";
			var address = orgHeader.Addresses.AddNew();
			address.Address1 = "xxx";
			address.CompanyName = "qing";
			var postalAddress = orgHeader.Addresses.AddNew();
			postalAddress.Address1 = "123";
			postalAddress.Address2 = "456";
			postalAddress.City = "Nanjing";
			postalAddress.Postcode = "789890";
			postalAddress.State = "JN";
			postalAddress.OA_RN_NKCountryCode = "CN";
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.HAB), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			var bill = manifest.Bills[0];
			bill.ABL_OA_Shipper = address.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("NAD+CZ++QING:XXX++123:456:NANJING:789890 JN CN'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestConsignorPostalAddress_RMA()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "VWg";
			var address = orgHeader.Addresses.AddNew();
			address.Address1 = "xxx";
			address.CompanyName = "qing";
			var postalAddress = orgHeader.Addresses.AddNew();
			postalAddress.Address1 = "123";
			postalAddress.Address2 = "456";
			postalAddress.City = "Nanjing";
			postalAddress.Postcode = "789890";
			postalAddress.State = "JN";
			postalAddress.OA_RN_NKCountryCode = "CN";
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RMA), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RMA);
			var bill = manifest.Bills[0];
			bill.ABL_OA_Shipper = address.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("NAD+CZ++QING:XXX++123:456:NANJING:789890 JN CN'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestConsigneePostalAddress_ALH()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "VWg";
			var address = orgHeader.Addresses.AddNew();
			address.Address1 = "xxx";
			address.CompanyName = "qing";
			var postalAddress = orgHeader.Addresses.AddNew();
			postalAddress.Address1 = "123";
			postalAddress.Address2 = "456";
			postalAddress.City = "Nanjing";
			postalAddress.Postcode = "789890";
			postalAddress.State = "JN";
			postalAddress.OA_RN_NKCountryCode = "CN";
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALH), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			var bill = manifest.Bills[0];
			bill.ABL_OA_Consignee = address.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("NAD+CN++QING:XXX'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestConsigneePostalAddress_HAB()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "VWg";
			var address = orgHeader.Addresses.AddNew();
			address.Address1 = "xxx";
			address.CompanyName = "qing";
			var postalAddress = orgHeader.Addresses.AddNew();
			postalAddress.Address1 = "123";
			postalAddress.Address2 = "456";
			postalAddress.City = "Nanjing";
			postalAddress.Postcode = "789890";
			postalAddress.State = "JN";
			postalAddress.OA_RN_NKCountryCode = "CN";
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.HAB), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			var bill = manifest.Bills[0];
			bill.ABL_OA_Consignee = address.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("NAD+CN++QING:XXX++123:456:NANJING:789890 JN CN'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestConsigneePostalAddress_RMA()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "VWg";
			var address = orgHeader.Addresses.AddNew();
			address.Address1 = "xxx";
			address.CompanyName = "qing";
			var postalAddress = orgHeader.Addresses.AddNew();
			postalAddress.Address1 = "123";
			postalAddress.Address2 = "456";
			postalAddress.City = "Nanjing";
			postalAddress.Postcode = "789890";
			postalAddress.State = "JN";
			postalAddress.OA_RN_NKCountryCode = "CN";
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RMA), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RMA);
			var bill = manifest.Bills[0];
			bill.ABL_OA_Consignee = address.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("NAD+CN++QING:XXX++123:456:NANJING:789890 JN CN'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestNotifyParty()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "VWg";
			var address = orgHeader.Addresses.AddNew();
			address.CompanyName = "qing";
			address.Address1 = "123";
			address.Address2 = "456";
			address.City = "Nanjing";
			address.Postcode = "789890";
			address.State = "JN";
			address.OA_RN_NKCountryCode = "CN";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RMA), Factory, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RMA);
			var bill = manifest.Bills[0];
			bill.ABL_OA_NotifyParty = address.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("NAD+NI++QING:123:NANJING:JN:789890'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestZAManifestEDIMessagesAddress_FreeType()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = CreateAddress(org1, "111", "Address11", "Address12", "City1", "State1", "CpName1");
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = CreateAddress(org2, "222", "Address21", "Address22", "City2", "State2", "CpName2");
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var address3 = CreateAddress(org3, "333", "Address31", "Address32", "City3", "State3", "CpName3");
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			bill.ABL_BillNumber = "BILL1";
			bill.ABL_CargoStatus = CargoStatusList.Codes.PartShipment;
			bill.ABL_BillIssuer = "ISS2";
			bill.ABL_BillNumber = "BILL2";
			bill.ABL_OA_Shipper = address1.PK;
			bill.ABL_OA_Consignee = address2.PK;
			bill.ABL_OA_NotifyParty = address3.PK;

			var popupText = CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			AssertEquals(1, manifest.Messages.Count);
			var message = manifest.Messages[0];
			AssertContains("Shipper detail", "NAD+CZ++CPNAME1:ADDRESS11:CITY1:STATE1:111", message.EM_MessageText);
			AssertContains("Consignee detail", "NAD+CN++CPNAME2:ADDRESS21:CITY2:STATE2:222", message.EM_MessageText);
			AssertContains("NotifyParty detail", "NAD+NI++CPNAME3:ADDRESS31:CITY3:STATE3:333", message.EM_MessageText);
		}

		public void TestZAManifestEDIMessagesAddress_ManualDetail()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			bill.ABL_BillNumber = "BILL1";
			bill.ABL_CargoStatus = CargoStatusList.Codes.PartShipment;
			bill.ABL_BillIssuer = "ISS2";
			bill.ABL_BillNumber = "BILL2";
			bill.ABL_OA_Shipper = Guid.Empty;
			bill.ABL_OA_Consignee = Guid.Empty;
			bill.ABL_OA_NotifyParty = Guid.Empty;

			bill.ABL_ShipperName = "CpName1";
			bill.ABL_ShipperStreet1 = "Address11";
			bill.ABL_ShipperStreet2 = "Address12";
			bill.ABL_ShipperCity = "City1";
			bill.ABL_ShipperState = "State1";
			bill.ABL_ShipperPostcode = "111";
			bill.ABL_ConsigneeName = "CpName2";
			bill.ABL_ConsigneeStreet1 = "Address21";
			bill.ABL_ConsigneeStreet2 = "Address22";
			bill.ABL_ConsigneeCity = "City2";
			bill.ABL_ConsigneeState = "State2";
			bill.ABL_ConsigneePostcode = "222";
			bill.ABL_NotifyPartyName = "CpName3";
			bill.ABL_NotifyPartyStreet1 = "Address31";
			bill.ABL_NotifyPartyStreet2 = "Address32";
			bill.ABL_NotifyPartyCity = "City3";
			bill.ABL_NotifyPartyState = "State3";
			bill.ABL_NotifyPartyPostcode = "333";

			var popupText = CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			AssertEquals(1, manifest.Messages.Count);
			var message = manifest.Messages[0];
			AssertContains("Shipper detail", "NAD+CZ++CPNAME1:ADDRESS11:CITY1:STATE1:111", message.EM_MessageText);
			AssertContains("Consignee detail", "NAD+CN++CPNAME2:ADDRESS21:CITY2:STATE2:222", message.EM_MessageText);
			AssertContains("NotifyParty detail", "NAD+NI++CPNAME3:ADDRESS31:CITY3:STATE3:333'", message.EM_MessageText);
		}

		public void TestZAManifestEDIMessagesAddress_ALHDetail()
		{
			var op = GlbCompany.CurrentCompany.OrgProxy;
			op.OH_Code = "VWg";
			op.OH_FullName = "EDI CUSTOMS BROKERS";
			op.MainAddress.CompanyName = "qing";
			op.MainAddress.Address1 = "123";
			op.MainAddress.Address2 = "456";
			op.MainAddress.City = "Nanjing";
			op.MainAddress.Postcode = "789890";
			op.MainAddress.State = "JN";
			op.MainAddress.OA_RN_NKCountryCode = "CN";
			Factory.Save();

			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			bill.ABL_BillNumber = "BILL1";
			bill.ABL_CargoStatus = CargoStatusList.Codes.PartShipment;
			bill.ABL_BillIssuer = "ISS2";
			bill.ABL_BillNumber = "BILL2";
			bill.ABL_OA_Shipper = Guid.Empty;
			bill.ABL_OA_Consignee = Guid.Empty;
			bill.ABL_OA_NotifyParty = Guid.Empty;

			var popupText = CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			AssertEquals(1, manifest.Messages.Count);
			var message = manifest.Messages[0];
			AssertContains("Forward detail", "NAD+FW++EDI CUSTOMS BROKERS:123:NANJING:JN:789890'", message.EM_MessageText);
		}

		[TestDate(2018, 12, 02)]
		public void TestAmend()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RFM), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			manifest.CARN = "123456";
			var bill = manifest.Bills[0];
			bill.ABL_BillIssuer = "DJC";
			CusCarMessagingHelper.CreateCusCars(manifest, manifest.MessageFunctionSubTypeForAmend);
			var message = manifest.Messages[0];
			AssertContains("Type 4 = change", "+4'DTM+137:20181202:102'RFF+LO:C1234'RFF+AFB:123456'", message.EM_MessageText);
		}

		[TestDate(2018, 12, 02)]
		public void TestCancel()
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RFM), Factory);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			manifest.CARN = "123456";
			var bill = manifest.Bills[0];
			bill.ABL_BillIssuer = "DJC";
			CusCarMessagingHelper.CreateCusCars(manifest, manifest.MessageFunctionSubTypeForCancel);  // WITHDRAW ==> CANCEL?
			var message = manifest.Messages[0];
			AssertContains("Type 1 = cancel", "+1'DTM+137:20181202:102'RFF+LO:C1234'RFF+AFB:123456'", message.EM_MessageText);
		}

		[TestDate(2018, 12, 02)]
		public void TestCreateNewCusCarRoadIncPersons()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.MSELT, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.MSELT, SealTypeList.Codes.MechanicalSeal, "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);

			SetupSealingTypeMapping(Factory);
			SetupGisTypeMapping();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RFM), Factory);
			manifest.AMA_TransportMode = "ROA";
			manifest.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			manifest.AMA_MasterBill = "BOL";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			manifest.AMA_VehicleRegistration = "MT10VHC";
			manifest.ShippingAgent.Header.CustomsCodes.AddNew("CCC", "HAHAHAHA", "ZA");  // For NAD+AH
			manifest.Carrier.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			manifest.Carrier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CCC", "000", "ZA");
			manifest.PlaceOfExit = "JSA";
			manifest.AMA_Trailer1RegNo = "BC12DFGP";
			manifest.AMA_Trailer2RegNo = "ZX89QWGP";
			manifest.AMA_DateAtCustomsOffice = new ZDate(1971, 09, 18);
			manifest.AMA_CustomsOffice = "Bloom";
			manifest.AMA_ContainerMode = "CNT";

			CreateCusPersonAndGlbPerson("A Driver", 1, manifest, nationality: Core.Constants.CountryCodes.UnitedKingdom, placeOfIssue: Core.Constants.CountryCodes.Australia);
			CreateCusPersonAndGlbPerson("B Trolly Dolly", 2, manifest, nationality: Core.Constants.CountryCodes.Australia);
			CreateCusPersonAndGlbPerson("C Hostess", 3, manifest);
			CreateCusPersonAndGlbPerson("D Steward", 4, manifest);
			CreateCusPersonAndGlbPerson("E Passenger", 5, manifest);
			CreateCusPersonAndGlbPerson("F Traveller", 6, manifest);
			CreateCusPersonAndGlbPerson("G Holidaymaker", 7, manifest);
			CreateCusPersonAndGlbPerson("H Worker", 8, manifest);

			var bill = manifest.Bills[0];
			bill.ABL_BillIssuer = "ISS1";
			bill.CustomsEntryNumber = "LRN123";
			bill.CustomsEntryNumberType = ZaLRNTypes.Codes.AFM;
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = manifest.Containers[0].PK;
			pack.APA_MarksAndNumbers = "Marks and numbs";
			pack.APA_GoodsDescription = "Desc";
			pack.APA_PackQty = 69;
			pack.APA_PackUQ = "PK";
			pack.APA_Volume = 123m;
			pack.APA_Weight = 456m;
			pack.APA_WeightUQ = "KG";
			pack.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			pack.APA_CommodityCode = "comm1";

			manifest.AMA_CarrierCode = "111";
			manifest.MasterCarrierCode = "000";
			manifest.AMA_RadioCallSign = "OVYQ2";

			var container = manifest.Containers[0];
			container.ACN_SealType1 = SealTypeList.Codes.MechanicalSeal;
			container.ACN_SealType2 = SealTypeList.Codes.MechanicalSeal;
			container.ACN_SealType3 = SealTypeList.Codes.MechanicalSeal;
			container.ACN_SealingPartyType = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
			container.ACN_SealingPartyType2 = Core.Constants.ContainerSealParties.Codes.Terminal;
			container.ACN_SealingPartyType3 = Core.Constants.ContainerSealParties.Codes.Customs;

			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = ((IEDIMessageCollectionProvider)manifest).Messages.LastOutgoingMessage;
			AssertMultilineASCIIEquals("Full message text for RFM CUSCAR", @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::RFM+<<SYSCAR>>+9'
DTM+137:20181202:102'
RFF+LO:C1234'
NAD+RL+000'
NAD+DR+PP1:L++DRIVER:A DRIVER:DL1:1981091819720718:FGDVBVGBGBAU'
NAD+FL+PP2:M++DOLLY:B TROLLY DOLLY:DL2:1991091819730518:MHESVRAUAUAU'
NAD+FL+PP3:F++HOSTESS:C HOSTESS:DL3:2001091819740318:FGDVVPZAZAZA'
NAD+FL+PP4:A++STEWARD:D STEWARD:DL4:2011091819750118:MHEVBKZAZAZA'
NAD+FL+PP5:N++PASSENGER:E PASSENGER:DL5:2021091819751118:FGDVBVZAZAZA'
TDT+20++3++111:172:20+++:::MT10VHC   BC12DFGP  ZX89QWGP  '
LOC+35+US'
LOC+36+ZA'
LOC+17+BLOOM'
LOC+42+JSA'
DTM+132:19710918:102'
GEI+5+23:71:ZZZ'
GEI+5+CN:122:ZZZ'
EQD+CN+DANU1234567+22R0:102++3+8'
MEA+AAE+AAM+KGM:0'
MEA+AAE+VGM+KGM:2950'
SEL+S1+CA+++1'
SEL+S2+TO+++1'
SEL+S3+CU+++1'
CNT+16:1'
CNI+1+000BOL::::19710919'
CNT+16:1'
RFF+BM:HAWB1234:1'
LOC+8+ZAJNB'
LOC+9+USATL'
TDT+20'
RFF+AFM:LRN123'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL'
GID+1+69:PK'
FTX+AAA++9+DESC'
MEA+AAE+AAB+KGM:456'
SGP+DANU1234567'
PCI+24+MARKS AND NUMBS'
CST++COMM1'
UNT+41+1'
".Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper()), message.EM_MessageText.Replace("'", "'\r\n"));

			bill.ABL_CargoStatus = CargoStatusList.Codes.LastPartShipment;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			message = manifest.Messages.Cast<EDIMessage>().Last();
			AssertContains("FTX+AAA++10+DESC'", message.EM_MessageText);

			manifest.AMA_ContainerMode = "BLK";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			message = manifest.Messages.Cast<EDIMessage>().Last();
			AssertContains("GEI+5+DB:122:ZZZ'", message.EM_MessageText);

			manifest.AMA_ContainerMode = "LQD";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			message = manifest.Messages.Cast<EDIMessage>().Last();
			AssertContains("GEI+5+LB:122:ZZZ'", message.EM_MessageText);

			manifest.AMA_ContainerMode = "BBK";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			message = manifest.Messages.Cast<EDIMessage>().Last();
			AssertContains("GEI+5+BB:122:ZZZ'", message.EM_MessageText);

			manifest.AMA_ContainerMode = "OTH";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			message = manifest.Messages.Cast<EDIMessage>().Last();
			AssertContains("GEI+5+MX:122:ZZZ'", message.EM_MessageText);

			manifest.Carrier.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Namibia;

			Factory.Save();

			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			message = manifest.Messages.Cast<EDIMessage>().Last();
			AssertContains("NAD+AH+HAHAHAHA'", message.EM_MessageText);

			CombineAssertions("Test Transit Manifest", () =>
			{
				AssertEquals("ZA", manifest.AMA_RL_NKPortOfDischarge.Left(2));
				AssertNotContains("Manifest is Import", "GIS+23:71'", message.EM_MessageText);

				manifest.AMA_RL_NKPortOfDischarge = "LSMSU";
				manifest.AMA_Nature = ShipmentTypeList.Codes.Transit24;
				Factory.Save();

				AssertNotEquals("ZA", manifest.AMA_RL_NKPortOfLoading.Left(2));
				AssertNotEquals("ZA", manifest.AMA_RL_NKPortOfDischarge.Left(2));
				CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
				message = manifest.Messages.Cast<EDIMessage>().Last();
				AssertNotContains("Manifest is Transit shipment", "GIS+24:71'", message.EM_MessageText);
			});

			manifest.AMA_Nature = ShipmentTypeList.Codes.Import23;

			var entryNumber = bill.CustomsEntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "LRN456";
			entryNumber.CE_EntryType = ZaLRNTypes.Codes.AFM;

			var pack2 = bill.Packs.AddNew();
			pack2.ContainerPK = manifest.Containers[0].PK;
			pack2.APA_MarksAndNumbers = "Marks and numbs 2";
			pack2.APA_GoodsDescription = "Desc 2";
			pack2.APA_PackQty = 70;
			pack2.APA_PackUQ = "PK";
			pack2.APA_Volume = 124m;
			pack2.APA_Weight = 457m;
			pack2.APA_WeightUQ = "KG";
			pack2.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").First().PK;
			pack2.APA_CommodityCode = "comm2";

			entryNumber.PackPivots.RemoveAll();
			entryNumber.PackPivots.AddPivotFor(pack2);

			Factory.Save();

			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			message = manifest.Messages.Cast<EDIMessage>().Last();
			AssertContains("Multiple Customs Numbers", @"RFF+BM:HAWB1234:1'
LOC+8+ZAJNB'
LOC+9+USATL'
TDT+20'
RFF+AFM:LRN123'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL'
GID+1+69:PK'
FTX+AAA++10+DESC'
MEA+AAE+AAB+KGM:456'
SGP+DANU1234567'
PCI+24+MARKS AND NUMBS'
CST++COMM1'
RFF+BM:HAWB1234:2'
LOC+8+ZAJNB'
LOC+9+USATL'
TDT+20'
RFF+AFM:LRN456'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL'
GID+1+70:PK'
FTX+AAA++10+DESC 2'
MEA+AAE+AAB+KGM:457'
SGP+DANU1234567'
PCI+24+MARKS AND NUMBS 2'", message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCreateNewCusCarSeaWithoutContainers()
		{
			var header = SetUpManifestFortSea("COH", ApplicationCodeTypeList.Codes.Consolidator);
			header.Containers.RemoveAndDeleteAll();
			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var message = header.Messages[0];
			AssertNotContains("DANU1234567", message.EM_MessageText);
			AssertNotContains("SGP+", message.EM_MessageText);
			AssertNotContains("EQD+", message.EM_MessageText);
			AssertContains("BGM+85:::COH+", message.EM_MessageText);
		}

		[TestDate(2018, 12, 02)]
		public void TestCreateNewCusCarSea()
		{
			SetupSealingTypeMapping(Factory);
			var header = SetUpManifestFortSea("COH", ApplicationCodeTypeList.Codes.Consolidator);
			header.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			header.AMA_CarrierCode = "111";
			header.MasterCarrierCode = "000";
			header.AMA_RadioCallSign = "OVYQ2";

			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var message = header.Messages[0];
			Assert(message.IsInDatabase);
			AssertEquals("ZAC", message.EM_ApplicationCode);
			AssertEquals("CAR", message.EM_MessageType);
			AssertEquals(MessageSubTypeCodes.Codes.Original, message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("QUE", message.EM_Status);
			AssertEquals("AWA", header.AMA_MessageStatus);
			AssertMultilineASCIIEquals("Expected CUSCAR content", @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::COH+<<SYSCAR>>+9'
DTM+137:20181202:102'
DTM+136:19710917:102'
RFF+LO:C1234'
RFF+ACL:BA123'
NAD+RL+000'
NAD+FZ+ISS1'
NAD+MS+1111111122222222'
TDT+20+BA123+1++111:172:20+++OVYQ2:103:::GB'
LOC+60+ZAJNB'
DTM+132:19710918:102'
GEI+5+16:176:ZZZ'
EQD+CN+DANU1234567+22R0:102:5++3+8'
MEA+AAE+AAM+KGM:0'
MEA+AAE+VGM+KGM:2950'
SEL+S1+CA'
SEL+S2+TO'
SEL+S3+CU'
CNT+16:1'
CNI+1+:BOL:::19710919'
CNT+16:1'
RFF+BM:HAWB1234'
LOC+8+ZAJNB'
LOC+9+USATL'
LOC+80+USATL'
LOC+104'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161++6 HIXON COURT:ALEXANDRIA HILLS  QLD::4161  AU'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL++20A INDUSTRIAL DAS ERVOSAS LUT9::HAVO: IL NL'
NAD+NI++TFS FREIGHT INTNL INC./BEIJING BRAN:RM 108, BUILDING D, LONSDALE CENTER:P.R., CHINA::100015'
GID+1+69:PK'
FTX+AAA+++DESC'
MEA+AAE+AAB+KGM:456'
SGP+DANU1234567+69'
DGS+IMD+1.1D+0004'
PCI+24+MARKS AND NUMBS'
CST++COMM1'
UNT+38+1'
".Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper()), message.EM_MessageText.Replace("'", "'\r\n"));
		}

		[TestDate(2018, 12, 02)]
		public void TestCreateNewCusCarSea_CLD()
		{
			SetupSealingTypeMapping(Factory);
			var header = SetUpManifestFortSea("COH", ApplicationCodeTypeList.Codes.Consolidator);
			header.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			header.AMA_AgentType = Core.Constants.AgentType.CoLoad;
			header.AMA_CarrierCode = "111";
			header.MasterCarrierCode = "000";
			header.AMA_RadioCallSign = "OVYQ2";

			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var message = header.Messages[0];
			Assert(message.IsInDatabase);
			AssertEquals("ZAC", message.EM_ApplicationCode);
			AssertEquals("CAR", message.EM_MessageType);
			AssertEquals(MessageSubTypeCodes.Codes.Original, message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("QUE", message.EM_Status);
			AssertEquals("AWA", header.AMA_MessageStatus);
			AssertMultilineASCIIEquals("Expected CUSCAR content", @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::COH+<<SYSCAR>>+9'
DTM+137:20181202:102'
DTM+136:19710917:102'
RFF+LO:C1234'
RFF+ACL:BA123'
NAD+RL+000'
NAD+FZ+ISS1'
NAD+MS+1111111122222222'
TDT+20+BA123+1++111:172:20+++OVYQ2:103:::GB'
LOC+60+ZAJNB'
DTM+132:19710918:102'
GEI+5+16:176:ZZZ'
EQD+CN+DANU1234567+22R0:102:5++3+8'
MEA+AAE+AAM+KGM:0'
MEA+AAE+VGM+KGM:2950'
SEL+S1+CA'
SEL+S2+TO'
SEL+S3+CU'
CNT+16:1'
CNI+1+:PBL:::19710919'
CNT+16:1'
RFF+BM:HAWB1234'
LOC+8+ZAJNB'
LOC+9+USATL'
LOC+80+USATL'
LOC+104'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161++6 HIXON COURT:ALEXANDRIA HILLS  QLD::4161  AU'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL++20A INDUSTRIAL DAS ERVOSAS LUT9::HAVO: IL NL'
NAD+NI++TFS FREIGHT INTNL INC./BEIJING BRAN:RM 108, BUILDING D, LONSDALE CENTER:P.R., CHINA::100015'
GID+1+69:PK'
FTX+AAA+++DESC'
MEA+AAE+AAB+KGM:456'
SGP+DANU1234567+69'
DGS+IMD+1.1D+0004'
PCI+24+MARKS AND NUMBS'
CST++COMM1'
UNT+38+1'
".Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper()), message.EM_MessageText.Replace("'", "'\r\n"));
		}

		[TestDate(2018, 12, 02)]
		public void TestCUSCARMessageTextBuilder_COM()
		{
			SetupGisTypeMapping();
			var manifestType = "COM";
			var containerModes = new Dictionary<string, string>
			{
				{ "BBK", "BB" }, { "BLK", "DB" }, { "CNT", "CN" }, { "LQD", "LB" }, { "OTH", "MX" }
			};

			var messageNum = 0;
			var header = SetUpManifestFortSea(manifestType, ApplicationCodeTypeList.Codes.ShippingLine);
			Factory.Save();
			AssertEquals("C00001000_1", header.AMA_JobReference);

			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var containerMode in containerModes.Keys)
			{
				assertDelegate += () =>
				{
					var segmentCount = 39;
					var expectedMsgTxt = ExpectedCarMsgTxt_2018_3.Replace("<<MANIFESTTYPE>>", manifestType);

					expectedMsgTxt = expectedMsgTxt.Replace("<<GISSEGMENT>>'\r\n", string.Empty);
					expectedMsgTxt = expectedMsgTxt.Replace("<<GISSEGMENT2>>'\r\n", string.Empty);
					segmentCount -= 2;
					if (containerMode != "LQD")
					{
						expectedMsgTxt = expectedMsgTxt.Replace("MEA+AAE+AAW+MTQ:123'", string.Empty);
						segmentCount -= 1;
					}

					expectedMsgTxt = expectedMsgTxt.Replace("<<BILLOFLADINGINDICATOR>>", string.Empty);
					expectedMsgTxt = expectedMsgTxt.Replace("<<SEGMENTNUMBER>>", segmentCount.ToString());
					expectedMsgTxt = expectedMsgTxt.Replace("<<MESSAGENUMBER>>", (++messageNum).ToString());

					header.AMA_ContainerMode = containerMode;
					header.AMA_MasterBillIssueDate = new ZDate(1971, 09, 17);

					header.AMA_CarrierCode = "111";
					header.MasterCarrierCode = "000";
					header.AMA_RadioCallSign = "OVYQ2";

					header.AMA_ManifestType = manifestType;
					header.AMA_Nature = ShipmentTypeList.Codes.Import23;
					header.Messages.RemoveAndDeleteAllFromTest();

					CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
					var message = header.Messages[0];
					expectedMsgTxt = expectedMsgTxt.Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper());

					AssertMultilineASCIIEquals(
						"ManifestType: " + manifestType + ", ContainerMode: " + containerMode,
						expectedMsgTxt,
						message.EM_MessageText.Replace("'", "'\r\n")
					);
				};
			}

			CombineAssertions(assertDelegate);
		}

		const string ExpectedCarMsgTxt_2018_3 = @"UNH+<<MESSAGENUMBER>>+CUSCAR:D:16A:UN:RCG001'
BGM+85:::<<MANIFESTTYPE>>+<<SYSCAR>>+9'
DTM+137:20181202:102'
DTM+136:19710917:102'
RFF+LO:C00001000 1'
RFF+ACL:BA123'
NAD+RL+000'
NAD+MS+1111111122222222'
TDT+20+BA123+1++111:172:20+++OVYQ2:103:::GB'
LOC+60+ZAJNB'
DTM+132:19710918:102'
GEI+5+16:176:ZZZ'
<<GISSEGMENT>>'
<<GISSEGMENT2>>'
EQD+CN+DANU1234567+22R0:102:5++3+8'
MEA+AAE+AAM+KGM:0'
MEA+AAE+VGM+KGM:2950'
SEL+S1'
SEL+S2'
SEL+S3'
CNT+16:1'
CNI+1+:<<BILLOFLADINGINDICATOR>>:::19710917'
CNT+16:1'
RFF+BM:HAWB1234'
LOC+8+ZAJNB'
LOC+9+USATL'
LOC+80+USATL'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161++6 HIXON COURT:ALEXANDRIA HILLS  QLD::4161  AU'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL++20A INDUSTRIAL DAS ERVOSAS LUT9::HAVO: IL NL'
NAD+NI++TFS FREIGHT INTNL INC./BEIJING BRAN:RM 108, BUILDING D, LONSDALE CENTER:P.R., CHINA::100015'
GID+1+69:PK'
FTX+AAA+++DESC'
MEA+AAE+AAW+MTQ:123'
MEA+AAE+AAB+KGM:456'
SGP+DANU1234567+69'
DGS+IMD+1.1D+0004'
PCI+24+MARKS AND NUMBS'
CST++COMM1'
UNT+<<SEGMENTNUMBER>>+<<MESSAGENUMBER>>'";

		[TestDate(2018, 12, 02)]
		public void TestCUSCARMessageTextBuilder_BBB()
		{
			SetupGisTypeMapping();
			var manifestType = "BBB";
			var header = SetUpManifestFortSea(manifestType, ApplicationCodeTypeList.Codes.ShippingLine);
			VoidParameterlessDelegate assertDelegate = () => { };
			assertDelegate += () =>
			{
				var expectedMsgTxt = @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::BBB+<<SYSCAR>>+9'
DTM+137:20181202:102'
DTM+136:19710917:102'
RFF+LO:C1234'
RFF+ACL:BA123'
NAD+RL+000'
NAD+MS+1111111122222222'
TDT+20+BA123+1++111:172:20+++OVYQ2:103:::GB'
LOC+60+ZAJNB'
DTM+132:19710918:102'
GEI+5+23:71:ZZZ'
GEI+5+MX:122:ZZZ'
GEI+5+16:176:ZZZ'
CNI+1+::::19710917'
RFF+BM:HAWB1234'
LOC+8+ZAJNB'
LOC+9+USATL'
LOC+80+USATL'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161++6 HIXON COURT:ALEXANDRIA HILLS  QLD::4161  AU'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL++20A INDUSTRIAL DAS ERVOSAS LUT9::HAVO: IL NL'
NAD+NI++TFS FREIGHT INTNL INC./BEIJING BRAN:RM 108, BUILDING D, LONSDALE CENTER:P.R., CHINA::100015'
GID+1+69:PK'
FTX+AAA+++DESC'
MEA+AAE+AAB+KGM:456'
DGS+IMD+1.1D+0004'
PCI+24+MARKS AND NUMBS'
CST++COMM1'
UNT+29+1'";
				header.AMA_MasterBillIssueDate = new ZDate(1971, 09, 17);
				header.AMA_CarrierCode = "111";
				header.MasterCarrierCode = "000";
				header.AMA_RadioCallSign = "OVYQ2";

				header.AMA_ManifestType = manifestType;
				header.Messages.RemoveAndDeleteAllFromTest();
				CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
				var message = header.Messages[0];
				expectedMsgTxt = expectedMsgTxt.Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper());

				AssertMultilineASCIIEquals(
					"ManifestType: BBB",
					expectedMsgTxt,
					message.EM_MessageText.Replace("'", "'\r\n")
				);
			};

			CombineAssertions(assertDelegate);
		}

		[TestDate(2018, 12, 02)]
		public void TestCUSCARMessageTextBuilder_ECL()
		{
			SetupGisTypeMapping();

			var manifestType = "ECL";

			var header = SetUpManifestFortSea(manifestType, ApplicationCodeTypeList.Codes.ShippingLine);
			VoidParameterlessDelegate assertDelegate = () => { };
			assertDelegate += () =>
			{
				var expectedMsgTxt = @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::ECL+<<SYSCAR>>+9'
DTM+137:20181202:102'
DTM+136:19710917:102'
RFF+LO:C1234'
RFF+ACL:BA123'
NAD+RL+000'
NAD+MS+1111111122222222'
TDT+20+BA123+1++111:172:20+++OVYQ2:103:::GB'
LOC+60+ZAJNB'
DTM+132:19710918:102'
GEI+5+16:176:ZZZ'
EQD+CN+DANU1234567+22R0:102:5++3+8'
CNT+16:1'
UNT+15+1'
";

				header.AMA_MasterBillIssueDate = new ZDate(1971, 09, 17);
				header.AMA_CarrierCode = "111";
				header.MasterCarrierCode = "000";
				header.AMA_RadioCallSign = "OVYQ2";

				header.AMA_ManifestType = manifestType;
				header.Messages.RemoveAndDeleteAllFromTest();
				CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
				var message = header.Messages[0];
				expectedMsgTxt = expectedMsgTxt.Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper());

				AssertMultilineASCIIEquals(
					"ManifestType: ECL",
					expectedMsgTxt,
					message.EM_MessageText.Replace("'", "'\r\n")
				);
			};

			CombineAssertions(assertDelegate);
		}

		public void TestMakeMessagesForMultipleIssuersPackedIntoDifferentContainers()
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("AGT", "11111111", "ZA");
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("CDP", "22222222", "ZA");
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var bill1 = manifest.Bills[0];
			bill1.ABL_BillNumber = "BILL1";
			var za = bill1.ABL_BillIssuer = "ISS1";
			var bill2 = manifest.Bills.AddNew();
			bill2.ABL_BillIssuer = "ISS2";
			bill2.ABL_BillNumber = "BILL2";
			var contA = manifest.Containers[0];
			var contB = manifest.Containers.AddNew();
			contA.ACN_ContainerNumber = "AAAA1111111";
			contB.ACN_ContainerNumber = "BBBB1111111";
			var pack1A = bill1.Packs.AddNew();
			var pack1B = bill1.Packs.AddNew();
			var pack2B = bill2.Packs.AddNew();
			pack1A.ContainerPK = contA.PK;
			pack1B.ContainerPK = contB.PK;
			pack2B.ContainerPK = contB.PK;
			var popupText = CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			AssertEquals(2, manifest.Messages.Count);
			AssertEquals(@"Issuer code: ISS1
	Successfully created message
Issuer code: ISS2
	Successfully created message", popupText);

			CombineAssertions(() =>
			{
				var message = manifest.Messages.Cast<EDIMessage>().FirstOrDefault(c => c.EM_MessageText.Contains("NAD+FZ+ISS1"));

				AssertNotNull("'Grouping Centre' party is issuer", message);
				AssertContains("'Message Sender' party is agent/dual code", "NAD+MS+1111111122222222", message.EM_MessageText);
				AssertContains("EQD+CN+AAAA1111111", message.EM_MessageText);
				AssertContains("EQD+CN+BBBB1111111", message.EM_MessageText);
				AssertContains("RFF+BM:BILL1'", message.EM_MessageText);
				AssertContains("CNT+16:2'", message.EM_MessageText);
				AssertContains("SGP+AAAA1111111", message.EM_MessageText);
				AssertContains("SGP+BBBB1111111", message.EM_MessageText);
			});

			CombineAssertions(() =>
			{
				var message = manifest.Messages.Cast<EDIMessage>().FirstOrDefault(c => c.EM_MessageText.Contains("NAD+FZ+ISS2"));

				AssertNotNull("'Grouping Centre' party is issuer", message);
				AssertContains("RFF+BM:BILL2'", message.EM_MessageText);
				AssertContains("EQD+CN+BBBB1111111", message.EM_MessageText);
				AssertContains("CNT+16:1'", message.EM_MessageText);
				AssertContains("SGP+BBBB1111111", message.EM_MessageText);
			});
		}

		public void TestSplitGoodsWithAndWithoutContainerNumber()
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("AGT", "11111111", "ZA");
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("CDP", "22222222", "ZA");
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var bill1 = manifest.Bills[0];
			bill1.ABL_BillNumber = "BILL1";
			var za = bill1.ABL_BillIssuer = "ISS1";
			var bill2 = manifest.Bills.AddNew();
			bill2.ABL_BillIssuer = "ISS2";
			bill2.ABL_BillNumber = "BILL2";
			var contA = manifest.Containers[0];
			var contB = manifest.Containers.AddNew();
			contA.ACN_ContainerNumber = "AAAA1111111";
			contB.ACN_ContainerNumber = "";
			var pack1A = bill1.Packs.AddNew();
			var pack2B = bill2.Packs.AddNew();
			pack1A.ContainerPK = contA.PK;
			pack2B.ContainerPK = contB.PK;
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			AssertEquals(2, manifest.Messages.Count);

			CombineAssertions(() =>
			{
				var message1 = manifest.Messages.Cast<EDIMessage>().FirstOrDefault(c => c.EM_MessageText.Contains("NAD+FZ+ISS1"));
				var message2 = manifest.Messages.Cast<EDIMessage>().FirstOrDefault(c => c.EM_MessageText.Contains("NAD+FZ+ISS2"));

				AssertNotNull("'Grouping Centre' party is issuer", message1);
				AssertContains("'Message Sender' party is agent/dual code", "NAD+MS+1111111122222222", message1.EM_MessageText);
				AssertContains("EQD+CN+AAAA1111111", message1.EM_MessageText);
				AssertContains("RFF+BM:BILL1'", message1.EM_MessageText);
				AssertContains("CNT+16:1'", message1.EM_MessageText);
				AssertContains("SGP+AAAA1111111", message1.EM_MessageText);

				AssertNotNull("'Grouping Centre' party is issuer", message2);
				AssertContains("RFF+BM:BILL2'", message2.EM_MessageText);
				AssertContains("CNT+16:1'", message2.EM_MessageText);
				AssertNotContains("SGP", message2.EM_MessageText);
			});
		}

		public void TestCreateNewCusCarAIR_ALM()
		{
			SetupSealingTypeMapping(Factory);
			var manifest = SetupManifestForAir(nameof(ManifestDocumentType.ALM), ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALM);
			manifest.AMA_CarrierCode = "111";
			manifest.MasterCarrierCode = "000";
			manifest.AMA_RadioCallSign = "OVYQ2";

			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			Assert(message.IsInDatabase);
			AssertEquals("ZAC", message.EM_ApplicationCode);
			AssertEquals("CAR", message.EM_MessageType);
			AssertEquals(MessageSubTypeCodes.Codes.Original, message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("QUE", message.EM_Status);
			AssertEquals("AWA", manifest.AMA_MessageStatus);
			AssertMultilineASCIIEquals("Expected CUSCAR content", @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::ALM+<<SYSCAR>>+9'
DTM+137:19710919:102'
DTM+136:19710917:102'
RFF+LO:C1234'
RFF+ACL:BA123'
NAD+RL+000'
NAD+FZ+ISS1'
NAD+MS+1111111122222222'
NAD+DEG+111'
TDT+20+BA123+4++111:172:20+++OVYQ2:103:::GB'
LOC+60+JNB'
DTM+132:19710918:102'
DTM+369:197109170000:203'
CNI+1+111-2222222A:BOL:111-2222222A::19710919'
RFF+BM:HAWB1234'
LOC+8+JNB'
LOC+9+ATL'
LOC+80+MAN'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL'
NAD+NI++TFS FREIGHT INTNL INC./BEIJING BRAN:RM 108, BUILDING D, LONSDALE CENTER:P.R., CHINA::100015'
GID+1+69:PK'
FTX+AAA++9+DESC'
MEA+AAE+AAB+KGM:456'
DGS+IMD+1.1D+0004'
PCI+24+MARKS AND NUMBS'
CST++COMM1'
UNT+29+1'
".Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper()), message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCreateNewCusCarAIR_AQM()
		{
			SetupSealingTypeMapping(Factory);
			var manifest = SetupManifestForAir(nameof(ManifestDocumentType.AQM), ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			manifest.AMA_CarrierCode = "111";
			manifest.MasterCarrierCode = "000";
			manifest.AMA_RadioCallSign = "OVYQ2";
			manifest.Bills[0].CustomsEntryNumberType = ZaLRNTypes.Codes.AFM;

			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			Assert(message.IsInDatabase);
			AssertEquals("ZAC", message.EM_ApplicationCode);
			AssertEquals("CAR", message.EM_MessageType);
			AssertEquals(MessageSubTypeCodes.Codes.Original, message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("QUE", message.EM_Status);
			AssertEquals("AWA", manifest.AMA_MessageStatus);
			AssertMultilineASCIIEquals("Expected CUSCAR content", @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::AQM+<<SYSCAR>>+9'
DTM+137:19710919:102'
DTM+136:19710917:102'
RFF+LO:C1234'
NAD+RL+000'
NAD+FZ+ISS1'
NAD+MS+1111111122222222'
TDT+20+BA123+4++111:172:20+++OVYQ2:103:::GB'
LOC+60+JNB'
DTM+132:19710918:102'
CNI+1+111-2222222A:BOL:::19710919'
RFF+BM:HAWB1234'
GEI+5+:42'
TDT+20'
RFF+AFM'
GID+1+69:PK'
UNT+18+1'
".Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper()), message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCreateNewCusCarAIR_FFM()
		{
			SetupSealingTypeMapping(Factory);
			var manifest = SetupManifestForAir(nameof(ManifestDocumentType.FFM), ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.FFM);
			manifest.AMA_CarrierCode = "111";
			manifest.MasterCarrierCode = "000";
			manifest.AMA_RadioCallSign = "OVYQ2";

			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			Assert(message.IsInDatabase);
			AssertEquals("ZAC", message.EM_ApplicationCode);
			AssertEquals("CAR", message.EM_MessageType);
			AssertEquals(MessageSubTypeCodes.Codes.Original, message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("QUE", message.EM_Status);
			AssertEquals("AWA", manifest.AMA_MessageStatus);
			AssertMultilineASCIIEquals("Expected CUSCAR content", @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::FFM+<<SYSCAR>>+9'
DTM+137:19710919:102'
DTM+136:19710917:102'
RFF+LO:C1234'
NAD+RL+000'
NAD+MS+1111111122222222'
TDT+20+BA123+4++111:172:20+++OVYQ2:103:::GB'
LOC+60+JNB'
DTM+232:197109180000:203'
GEI+5+23:71:ZZZ'
GEI+5+:122:ZZZ'
GEI+5+16:176:ZZZ'
CNI+1+111-2222222A:BOL:::19710919'
RFF+BM:HAWB1234'
LOC+8+JNB'
LOC+9+ATL'
LOC+80+MAN'
GID+1+69:PK'
FTX+AAA++9+DESC'
MEA+AAE+AAB+KGM:456'
UNT+22+1'
".Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper()), message.EM_MessageText.Replace("'", "'\r\n"));
		}

		public void TestCreateNewCusCarAirWithPackageMarks()
		{
			using (ZA.DataRegistry.Business.ZACustomsRegistry.Instance.CusCarHabSendPciMarks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manifest = SetupManifestForAir(nameof(ManifestDocumentType.HAB), ApplicationCodeTypeList.Codes.Consolidator);
				CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);

				var message = manifest.Messages[0];
				AssertContains("PCI+24+MARKS AND NUMBS'", message.EM_MessageText);
			}
		}

		public void TestCreateNewCusCarAirWithoutPackageMarks()
		{
			using (ZA.DataRegistry.Business.ZACustomsRegistry.Instance.CusCarHabSendPciMarks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var manifest = SetupManifestForAir(nameof(ManifestDocumentType.HAB), ApplicationCodeTypeList.Codes.Consolidator);
				CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
				var message = manifest.Messages[0];
				AssertNotContains("PCI+24+MARKS AND NUMBS'", message.EM_MessageText);
			}
		}

		[TestDate(2018, 12, 02)]
		public void TestCreateNewCusCarAir_FWB()
		{
			var manifest = SetupManifestForAir(nameof(ManifestDocumentType.FWB), ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			var bill = manifest.Bills[0];
			bill.Packs.RemoveAndDeleteAll();
			bill.ABL_BillIssuer = "";
			Factory.Save();
			AssertEquals("C00001000", manifest.Consol.JK_UniqueConsignRef);
			AssertEquals("C00001000_1", manifest.AMA_JobReference);
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.FWB);
			manifest.AMA_CarrierCode = "111";
			manifest.MasterCarrierCode = "000";
			manifest.AMA_RadioCallSign = "OVYQ2";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			var message = manifest.Messages[0];
			AssertEquals("ZAC", message.EM_ApplicationCode);
			AssertEquals("CAR", message.EM_MessageType);
			AssertEquals(MessageSubTypeCodes.Codes.Original, message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("QUE", message.EM_Status);

			AssertMultilineASCIIEquals("Expected CUSCAR content", @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::FWB+<<SYSCAR>>+9'
DTM+137:20181202:102'
DTM+136:19710917:102'
RFF+LO:C00001000 1'
RFF+ACL:BA123'
NAD+RL+000'
NAD+MS+1111111122222222'
TDT+20+BA123+4++111:172:3'
LOC+60+JNB'
LOC+9+ATL'
DTM+232:197109180000:203'
GEI+5+23:71:ZZZ'
GEI+5+16:176:ZZZ'
CNI+1+111-2222222A::111-2222222A::19710919'
RFF+BM:HAW-B1234'
LOC+8+JNB'
LOC+9+ATL'
LOC+80+MAN'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161++6 HIXON COURT:ALEXANDRIA HILLS  QLD::4161  AU'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL++20A INDUSTRIAL DAS ERVOSAS LUT9::HAVO: IL NL'
NAD+NI++TFS FREIGHT INTNL INC./BEIJING BRAN:RM 108, BUILDING D, LONSDALE CENTER:P.R., CHINA::100015'
GID+1+5:PK'
FTX+AAA+++BOOKS'
MEA+AAE+AAB+KGM:1.360777'
PCI+24+MARKS'
UNT+27+1'
".Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper()), message.EM_MessageText.Replace("'", "'\r\n"));
		}

		[TestDate(2018, 12, 02)]
		public void TestCreateNewCusCarAirAndPackedIntoInterchange()
		{
			var manifest = SetupManifestForAir(nameof(ManifestDocumentType.HAB), ApplicationCodeTypeList.Codes.Consolidator);
			manifest.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			manifest.AMA_CarrierCode = "111";
			manifest.MasterCarrierCode = "000";
			manifest.AMA_RadioCallSign = "OVYQ2";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			var message = manifest.Messages[0];
			AssertEquals("ZAC", message.EM_ApplicationCode);
			AssertEquals("CAR", message.EM_MessageType);
			AssertEquals(MessageSubTypeCodes.Codes.Original, message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("QUE", message.EM_Status);

			AssertMultilineASCIIEquals("Expected CUSCAR content", @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::HAB+<<SYSCAR>>+9'
DTM+137:20181202:102'
DTM+136:19710917:102'
RFF+LO:C1234'
RFF+ACL:BA123'
NAD+RL+000'
NAD+FZ+ISS1'
NAD+MS+1111111122222222'
TDT+20+BA123+4++111:172:3'
LOC+60+JNB'
LOC+9+ATL'
DTM+232:197109180000:203'
GEI+5+23:71:ZZZ'
GEI+5+16:176:ZZZ'
CNI+1+111-2222222A:BOL:111-2222222A::19710919'
RFF+BM:HAWB1234'
LOC+8+JNB'
LOC+9+ATL'
LOC+80+MAN'
LOC+104'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161++6 HIXON COURT:ALEXANDRIA HILLS  QLD::4161  AU'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL++20A INDUSTRIAL DAS ERVOSAS LUT9::HAVO: IL NL'
NAD+NI++TFS FREIGHT INTNL INC./BEIJING BRAN:RM 108, BUILDING D, LONSDALE CENTER:P.R., CHINA::100015'
GID+1+69:PK'
FTX+AAA+++DESC'
MEA+AAE+AAB+KGM:456'
DGS+ICA+0004'
PCI+24+MARKS AND NUMBS'
UNT+30+1'
".Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper()), message.EM_MessageText.Replace("'", "'\r\n"));

			//  Add this to expected message and change segment count:  DGS+IMD+1.1D+0004A'

			var nonDependentCollection = new NonDependentEDIMessageCollection(Factory);
			nonDependentCollection.Add(message);
			var zaInterchangeProvider = new ZA.Business.BatchProcessor.ZAInterchangeProvider(new LoggingInformation(), nonDependentCollection);
			var interchange = zaInterchangeProvider.Interchanges[0];
			AssertStartsWith("UNB is generated correctly", "UNB+UNOB:4+1111111122222", interchange.EI_HeaderText);
		}

		[TestDate(2018, 12, 02)]
		public void TestCreateNewCusCarAirAndPackedIntoInterchange_CLD()
		{
			var manifest = SetupManifestForAir(nameof(ManifestDocumentType.HAB), ApplicationCodeTypeList.Codes.Consolidator);
			manifest.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			manifest.AMA_AgentType = Core.Constants.AgentType.CoLoad;
			manifest.AMA_CarrierCode = "111";
			manifest.MasterCarrierCode = "000";
			manifest.AMA_RadioCallSign = "OVYQ2";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			var message = manifest.Messages[0];
			AssertEquals("ZAC", message.EM_ApplicationCode);
			AssertEquals("CAR", message.EM_MessageType);
			AssertEquals(MessageSubTypeCodes.Codes.Original, message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("QUE", message.EM_Status);

			AssertMultilineASCIIEquals("Expected CUSCAR content", @"UNH+1+CUSCAR:D:16A:UN:RCG001'
BGM+85:::HAB+<<SYSCAR>>+9'
DTM+137:20181202:102'
DTM+136:19710917:102'
RFF+LO:C1234'
RFF+ACL:BA123'
NAD+RL+000'
NAD+FZ+ISS1'
NAD+MS+1111111122222222'
TDT+20+BA123+4++111:172:3'
LOC+60+JNB'
LOC+9+ATL'
DTM+232:197109180000:203'
GEI+5+23:71:ZZZ'
GEI+5+16:176:ZZZ'
CNI+1+111-2222222A:PBL:::19710919'
RFF+BM:HAWB1234'
LOC+8+JNB'
LOC+9+ATL'
LOC+80+MAN'
LOC+104'
NAD+CN++RAYMOND LUKE TURNER:6 HIXON COURT:::4161++6 HIXON COURT:ALEXANDRIA HILLS  QLD::4161  AU'
NAD+CZ++LSA PRODUCTOS TECNICOS PARA A INDUS:20A INDUSTRIAL DAS ERVOSAS LUT9:HAVO:IL++20A INDUSTRIAL DAS ERVOSAS LUT9::HAVO: IL NL'
NAD+NI++TFS FREIGHT INTNL INC./BEIJING BRAN:RM 108, BUILDING D, LONSDALE CENTER:P.R., CHINA::100015'
GID+1+69:PK'
FTX+AAA+++DESC'
MEA+AAE+AAB+KGM:456'
DGS+ICA+0004'
PCI+24+MARKS AND NUMBS'
UNT+30+1'
".Replace("<<SYSCAR>>", new ZString(message.PK.ToString()).KeepAlphanumericCharacters().ToUpper()), message.EM_MessageText.Replace("'", "'\r\n"));

			//  Add this to expected message and change segment count:  DGS+IMD+1.1D+0004A'

			var nonDependentCollection = new NonDependentEDIMessageCollection(Factory);
			nonDependentCollection.Add(message);
			var zaInterchangeProvider = new ZA.Business.BatchProcessor.ZAInterchangeProvider(new LoggingInformation(), nonDependentCollection);
			var interchange = zaInterchangeProvider.Interchanges[0];
			AssertStartsWith("UNB is generated correctly", "UNB+UNOB:4+1111111122222", interchange.EI_HeaderText);
		}

		[ExpectNoExceptions]
		public void TestWhenNoContainers()
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("AGT", "11111111", "ZA");
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("CDP", "22222222", "ZA");
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var bill1 = manifest.Bills[0];
			bill1.ABL_CargoStatus = CargoStatusList.Codes.FullShipment;
			bill1.ABL_BillNumber = "BILL1";
			bill1.ABL_BillIssuer = "ISS1";
			var bill2 = manifest.Bills.AddNew();
			bill2.ABL_CargoStatus = CargoStatusList.Codes.PartShipment;
			bill2.ABL_BillIssuer = "ISS2";
			bill2.ABL_BillNumber = "BILL2";
			manifest.Containers.RemoveAndDeleteAll();
			var pack1A = bill1.Packs.AddNew();
			var pack1B = bill1.Packs.AddNew();
			var pack2B = bill2.Packs.AddNew();
			var popupText = CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			AssertEquals("Should have been able to make a message without puking even when no containers on packs pivots", 2, manifest.Messages.Count);
		}

		public void TestCanSendIfManifestType_COM_BBK_ECL_EvenIfBillIssuerIsEmpty()
		{
			// Try to create a message using ManifestType: "COH" and a list of Bills with empty ABL_BillIssuer, shall fail with a error "At least one Bill..."
			// Try to create a message using ManifestType: "COM", "BBK", "ECL" and a list of Bills with empty ABL_BillIssuer, shall succeedd without error.
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "Manifest Validation");
			var validationRuleZa = helper.CreateNewOrGetExistingCusCodeList("ZA", RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.BillIssuer, "A Bill issuer is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMESSAGETYPE", nameof(ManifestDocumentType.FWB));
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMANIFESTTYPE", nameof(ManifestDocumentType.COH));
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";

			var errorTxt = "At least one Bill is not eligible for sending to South Africa.\r\nBill '123' must have a Bill Issuer.\r\nPlease fix this before sending again.";
			var popText = CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			AssertContains(
				"ManifestType: COH, Empty Bill Issuer list",
				errorTxt,
				popText);

			bill.ABL_BillIssuer = "00281124";

			popText = CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			AssertNotContains("ManifestType: COH, Not empty Bill Issuer list", errorTxt, popText);

			header.Messages.RemoveAndDeleteAllFromTest();

			header.AMA_ManifestType = "BBB";
			popText = CusCarMessagingHelper.CreateCusCars(header, new[] { bill }, MessageSubTypeCodes.Codes.Original);
			AssertNotContains("ManifestType: BBB, Not empty Bill Issuer list", errorTxt, popText);
			header.Messages.RemoveAndDeleteAllFromTest();

			header.AMA_ManifestType = "COM";
			popText = CusCarMessagingHelper.CreateCusCars(header, new[] { bill }, MessageSubTypeCodes.Codes.Original);
			AssertNotContains("ManifestType: COM, Not empty Bill Issuer list", errorTxt, popText);
			header.Messages.RemoveAndDeleteAllFromTest();

			header.AMA_ManifestType = "ECL";
			popText = CusCarMessagingHelper.CreateCusCars(header, new[] { bill }, MessageSubTypeCodes.Codes.Original);
			AssertNotContains("ManifestType: ECL, Not empty Bill Issuer list", errorTxt, popText);
		}

		public void TestCannotSendWithoutBillCountryZaAndBillIssuerIfNotMandatory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "Manifest Validation");

			var validationRuleZa = helper.CreateNewOrGetExistingCusCodeList("ZA", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.BillIssuer, "A Bill issuer is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMANIFESTTYPE", nameof(ManifestDocumentType.COH));
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var bill1 = header.Bills.AddNew();
			var popupText = CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			AssertContains("not eligible for sending to South Africa", popupText);
			popupText = CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			AssertContains("not eligible for sending to South Africa", popupText);
			bill1.ABL_BillIssuer = "XX";
			popupText = CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			AssertNotContains("not eligible for sending to South Africa", popupText);
			header.AMA_TransportMode = "ROA";
			header.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			popupText = CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			AssertContains("Successfully created message", popupText);
		}

		public void TestGetCarrierCodeForNadRlSegment()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var queryCarrierCode = new ZQuery(RefCarrierCodeSchema.ZZ4_Code, "JDNL");
			queryCarrierCode.AddToFilter(RefCarrierCodeSchema.ZZ4_ZZZ_NKDataGrouping, "ZA");
			var carrierCode = Factory.LoadTop1<RefCarrierCode>(queryCarrierCode) ?? helper.CreateCarrierCode("JDNL", "Jan de Nul", "ZA");
			var queryVesselZZ = new ZQuery(RefVesselZZSchema.ZZO_Code, "LA BOUDEUSE");
			queryVesselZZ.AddToFilter(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, "ZA");
			var vessel = Factory.LoadTop1<RefVesselZZ>(queryVesselZZ) ?? helper.CreateVesselZZ("LA BOUDEUSE", "LXBH", "CV", "ZA");
			var queryCarrierVesselPivot = new ZQuery(RefCarrierVesselPivotSchema.ZZQ_ZZ4, carrierCode.PK);
			queryCarrierVesselPivot.AddToFilter(RefCarrierVesselPivotSchema.ZZQ_ZZO, vessel.PK);
			if (!Factory.Exists(typeof(RefCarrierVesselPivot), queryCarrierVesselPivot))
			{
				helper.CreateCarrierVesselPivot(carrierCode.PK, vessel.PK);
			}
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_VesselName = "";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			manifest.AMA_CarrierCode = "111";
			manifest.MasterCarrierCode = "000";
			manifest.AMA_RadioCallSign = "OVYQ2";
			var bill = manifest.Bills[0];
			bill.ABL_BillIssuer = "ISS1";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("NAD+RL+000'", message.EM_MessageText);

			manifest.AMA_VesselName = "LA BOUDEUSE";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			message = manifest.Messages[1];
			AssertContains("NAD+RL+000'", message.EM_MessageText);

			manifest.AMA_TransportMode = "AIR";
			manifest.AMA_MasterBill = "125-22222222";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			manifest.MasterCarrierCode = "000";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			message = manifest.Messages[2];
			AssertContains("NAD+RL+000'", message.EM_MessageText);
		}

		public void TestGetCarrierDetailsForNadDEGSegment()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ccc = helper.CreateCarrierCode("111", "Vic.Wang", "ZA");
			ccc.Attributes.AddNew("MASTER", "MASTER");
			ccc.Attributes.AddNew("SEA", "SEA");
			var queryCarrierCode = new ZQuery(RefCarrierCodeSchema.ZZ4_Code, "JDNL");
			queryCarrierCode.AddToFilter(RefCarrierCodeSchema.ZZ4_ZZZ_NKDataGrouping, "ZA");
			var carrierCode = Factory.LoadTop1<RefCarrierCode>(queryCarrierCode) ?? helper.CreateCarrierCode("JDNL", "Jan de Nul", "ZA");
			var queryVesselZZ = new ZQuery(RefVesselZZSchema.ZZO_Code, "LA BOUDEUSE");
			queryVesselZZ.AddToFilter(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, "ZA");
			var vessel = Factory.LoadTop1<RefVesselZZ>(queryVesselZZ) ?? helper.CreateVesselZZ("LA BOUDEUSE", "LXBH", "CV", "ZA");
			var queryCarrierVesselPivot = new ZQuery(RefCarrierVesselPivotSchema.ZZQ_ZZ4, carrierCode.PK);
			queryCarrierVesselPivot.AddToFilter(RefCarrierVesselPivotSchema.ZZQ_ZZO, vessel.PK);
			if (!Factory.Exists(typeof(RefCarrierVesselPivot), queryCarrierVesselPivot))
			{
				helper.CreateCarrierVesselPivot(carrierCode.PK, vessel.PK);
			}
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.ALH), Factory);
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_VesselName = "";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			manifest.AMA_CarrierCode = "111";
			manifest.MasterCarrierCode = "000";
			manifest.AMA_RadioCallSign = "OVYQ2";
			var bill = manifest.Bills[0];
			bill.ABL_BillIssuer = "ISS1";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			var message = manifest.Messages[0];
			AssertContains("NAD+DEG+111++VIC.WANG'", message.EM_MessageText);

			manifest.AMA_VesselName = "LA BOUDEUSE";
			CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			message = manifest.Messages[1];
			AssertContains("NAD+DEG+111++VIC.WANG'", message.EM_MessageText);
		}

		public void TestSGPSegmentHasPackageNumber_2018_3_ALH()
		{
			SetupSealingTypeMapping(Factory);
			var header = SetUpManifestFortSea(nameof(ManifestDocumentType.ALH), ApplicationCodeTypeList.Codes.Consolidator);
			header.AMA_MasterBillIssueDate = new ZDate(1971, 09, 19);
			header.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var message = header.Messages[0];
			AssertContains("SGP Segment", "SGP+DANU1234567+69'", message.EM_MessageText);
		}

		public void TestCreateHABMultipleBillsAmendment()
		{
			SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			var header = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.HAB), Factory);
			header.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
			var bill1 = header.Bills[0];
			bill1.ABL_BillNumber = "BILL1";
			var za = bill1.ABL_BillIssuer = "ISS1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillIssuer = "ISS1";
			bill2.ABL_BillNumber = "BILL2";

			// Create messages
			CusCarMessagingHelper.CreateCusCars(header, new[] { bill1, bill2 }, header.MessageFunctionSubTypeForAmend);
			AssertEquals(2, header.Messages.Count);
			foreach (EDIMessage message in header.Messages)
			{
				Assert(message.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None)[1].EndsWith("+9'"));
			}

			var nonDependentCollection = new NonDependentEDIMessageCollection(Factory);
			nonDependentCollection.Add(header.Messages[0]);
			var zaInterchangeProvider = new ZA.Business.BatchProcessor.ZAInterchangeProvider(new LoggingInformation(), nonDependentCollection);
			var interchange = zaInterchangeProvider.Interchanges[0];
			AssertNotNullOrEmpty(interchange.EI_HeaderText);

			var message11 = bill1.Messages[0];
			Assert(message11.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None)[1].EndsWith("+9'"));
			AssertNullOrEmpty(bill1.RegistrationNumber);
			var incomingMessage = Factory.NewWithValidTestData<ZA.Business.CUSRESEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = "RCV";
			incomingMessage.EM_ApplicationCode = "ZAC";
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = $@"UNH+1+CUSRES:D:96B:UN:ZZZ01
BGM+962+{((ZString)message11.PK.ToString()).KeepAlphanumericCharacters()}
LOC+22+ ::ZZZ
GIS+8:120:ZZZ
NAD+AG+00000000
RFF+BH:RFMHBOL1
RFF+ACD:{message11.EM_MessageNum}
DTM+137:20170316:102
RFF+AFB:CARN0111123J
UNT+10+1
".Replace("\r\n", "'");

			var proc = new ZA.Business.MessageProcessor.CUSRESMessageProcessor(new LoggingInformation());
			proc.PreProcessMessage(incomingMessage);
			proc.ProcessMessage(incomingMessage);
			Factory.Save();

			// Amend and Create message
			CusCarMessagingHelper.CreateCusCars(header, new[] { bill1, bill2 }, header.MessageFunctionSubTypeForAmend);
			var message13 = bill1.Messages[2];
			Assert(message13.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None)[1].EndsWith("+4'"));
			var message22 = bill2.Messages[1];
			Assert(message22.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None)[1].EndsWith("+9'"));

			// Cancel message
			CusCarMessagingHelper.CreateCusCars(header, new[] { bill1 }, header.MessageFunctionSubTypeForCancel);
			var message14 = bill1.Messages[3];
			Assert(message14.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None)[1].EndsWith("+1'"));
		}

		public void TestCreateCOHContainerFCL()
		{
			SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			var header = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			header.AMA_IsBuyersConsolidation = true;
			var bill1 = header.Bills[0];
			bill1.ABL_BillNumber = "BILL1";
			bill1.ABL_BillIssuer = "ISS1";
			var pack = bill1.Packs.AddNew();
			var cont = header.Containers[0];
			pack.ContainerPK = cont.PK;
			Factory.Save();

			// Create message
			CusCarMessagingHelper.CreateCusCars(header, new[] { bill1 }, header.MessageFunctionSubTypeForAmend);
			AssertEquals(1, header.Messages.Count);
			var message = header.Messages[0];
			var lines = message.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
			var bgm = lines.FirstOrDefault(l => l.StartsWith("BGM"));
			Assert(bgm.EndsWith("+9'"));
			var eqd = lines.FirstOrDefault(l => l.StartsWith("EQD"));
			Assert(eqd.EndsWith("+8'"));
			header.Messages.RemoveAndDeleteAllFromTest();

			header.AMA_IsBuyersConsolidation = false;
			CusCarMessagingHelper.CreateCusCars(header, new[] { bill1 }, header.MessageFunctionSubTypeForAmend);
			AssertEquals(1, header.Messages.Count);
			message = header.Messages[0];
			lines = message.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
			eqd = lines.FirstOrDefault(l => l.StartsWith("EQD"));
			Assert(eqd.EndsWith("+8'"));
			header.Messages.RemoveAndDeleteAllFromTest();

			cont.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.EmptyContainer;
			CusCarMessagingHelper.CreateCusCars(header, new[] { bill1 }, header.MessageFunctionSubTypeForAmend);
			AssertEquals(1, header.Messages.Count);
			message = header.Messages[0];
			lines = message.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
			eqd = lines.FirstOrDefault(l => l.StartsWith("EQD"));
			Assert(eqd.EndsWith("+4'"));
		}

		public void TestMRNForAmendOrDelete()
		{
			SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "8";
			var message1 = Factory.New<ZA.Business.CUSRESEDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message1.EM_MessageType = ZA.Business.SARSEDIMessage.MessageTypes.CUSRES;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			message1.EM_MessageText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+6332A821D8364BBA9A76BE1678FD52BF'LOC+22+ ::ZZZ'GIS+8:120:ZZZ'NAD+AG+00000000'RFF+BH:S700049788'RFF+AAS:RFM'DTM+137:20170419:102'RFF+AFB:CARN0111138D'UNT+10+1'";
			message1.EM_LinkedObject = bill;
			var message2 = Factory.New<ZA.Business.CUSRESEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message2.EM_MessageType = ZA.Business.SARSEDIMessage.MessageTypes.CUSRES;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message2.EM_MessageText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+04E362079C1245A48B4A388237F4FE5F'LOC+22+ ::ZZZ'GIS+8:120:ZZZ'NAD+AG+00000000'RFF+BH:S700049788'RFF+AAS:RFM'DTM+137:20170419:102'RFF+AFB:CARN0111138D'UNT+10+1'";
			message2.EM_LinkedObject = bill;
			Factory.Save();

			CusCarMessagingHelper.CreateCusCars(header, new[] { bill }, MessageSubTypeCodes.Codes.Change);

			var message = header.Messages.Cast<EDIMessage>().FirstOrDefault(m => m.EM_MessageSubType == "CHG");
			AssertNotNull(message);
			var lines = message.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
			var rffAcw = lines.FirstOrDefault(l => l.StartsWith("RFF+ACW"));
			AssertNotNull(rffAcw);
			Assert(rffAcw.EndsWith("04E362079C1245A48B4A388237F4FE5F'"));
		}

		public void TestCreateVINNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "ROA";
			header.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			bill.ABL_BillIssuer = "DJC";
			bill.CustomsEntryNumber = "LRN123";
			bill.CustomsEntryNumberType = ZaLRNTypes.Codes.AFM;
			var pack = bill.Packs.AddNew();
			pack.APA_MarksAndNumbers = "Marks and numbs";
			pack.APA_GoodsDescription = "Desc";
			pack.APA_PackQty = 69;
			pack.APA_PackUQ = "PK";
			pack.APA_Volume = 123m;
			pack.APA_Weight = 456m;
			pack.APA_WeightUQ = "KG";
			pack.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			pack.APA_CommodityCode = "comm1";
			pack.APA_VINNumber = "VIN1";
			Factory.Save();

			CusCarMessagingHelper.CreateCusCars(header, header.MessageFunctionSubTypeForAmend);
			AssertEquals(1, header.Messages.Count);
			var message = header.Messages[0];
			var lines = message.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
			var gid = lines.FirstOrDefault(l => l.StartsWith("GID"));
			Assert(gid.EndsWith("VN'"));
			var pci = lines.FirstOrDefault(l => l.StartsWith("PCI"));
			Assert(pci.EndsWith("VIN=VIN1'"));
		}

		public void TestInvalidManifestType()
		{
			var header = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			header.AMA_ManifestType = ZString.Empty;
			header.CARN = "123456";
			var abc = header.Bills[0];
			abc.ABL_BillIssuer = "DJC";
			var popText = CusCarMessagingHelper.CreateCusCars(header, MessageSubTypeCodes.Codes.Original);
			var errorTxt = "Cannot create manifest message for type None. Most likely this means you have not supplied an acceptable value in Manifest Type.";
			AssertContains("Contains error text", errorTxt, popText);
		}

		public void TestRollbackOnUnsuccessfulSave()
		{
			var factory = new BusinessObjectFactory();
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), factory);
			var bill = manifest.Bills[0];
			bill.ABL_BillIssuer = "ISS1";
			manifest.AMA_MessageStatus = "DJC";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			factory.Save();
			factory.Saving += FactorySavingHandlerForTest;
			var errorMessage = CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			AssertEquals("DJC", manifest.AMA_MessageStatus);
			AssertEquals(0, manifest.Messages.Count);
			AssertContains("Fail for Testing", errorMessage);
			factory.Saving -= FactorySavingHandlerForTest;
		}

		public void TestPopulateRegNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var outgoingMessage = Factory.NewWithValidTestData<ZA.Business.CUSRESEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageNum = "TestNum";
			outgoingMessage.EM_LinkedObject = header;
			outgoingMessage.EM_MessageType = "CAR";
			outgoingMessage.EM_MessageText = "UNH+7616+CUSCAR:D:16A:UN:RCG001'BGM+85:::RFM+F513A29470F34FF391AD0B3DD41E2B5D+4'DTM+137:20180523:102'DTM+136:20180522:102'RFF+ACW:EACBF377B6804C00BA276C8B9D382844'RFF+LO:MAN0000315'NAD+RL+00655953'NAD+MS+00505655TST'NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2019041820160201:MHDSVRZAZAZA'TDT+20++3++00655953:172:20+++:::LIC                           'LOC+35+BW'LOC+36+ZA'LOC+17+KFN'DTM+132:20180522:102'GEI+5+24:71:ZZZ'GEI+5+DB:122:ZZZ'CNI+1+00655953ROADMAN1::::20180521'RFF+BM:THEONE:1'LOC+8+ZADUR'LOC+9+BWBBK'TDT+20'RFF+ABT:00505655KOM20180523007139'NAD+CN++DIGICORE ELECTRONICS (PTY) LTD:37 HILLCLIMB ROAD:DURBAN::3608'NAD+CZ++TIM EXPORT BLNS GROUP:1 FAR STREET'GID+1+5:BAG'FTX+AAA++9+DESC'MEA+AAE+AAB+KGM:5'PCI+24+GGG'UNT+29+7616'";
			var incomingMessage = Factory.NewWithValidTestData<ZA.Business.CUSRESEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = "RCV";
			incomingMessage.EM_ApplicationCode = "ZAC";
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01
BGM+962+F513A29470F34FF391AD0B3DD41E2B5D
LOC+22+ ::ZZZ
GIS+8:120:ZZZ
NAD+AG+00000000
RFF+BH:RFMHBOL1
RFF+ACD:TestNum
DTM+137:20170316:102
RFF+AFB:CARN0111123J
UNT+10+1
".Replace("\r\n", "'");
			var processor = new ZA.Business.MessageProcessor.CUSRESMessageProcessor(new LoggingInformation());
			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("RegistrationNumber", "CARN0111123J", header.RegistrationNumber);
		}

		public void TestUpdateBillCountryStatusAndDate()
		{
			SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			var header = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.RFM), Factory);
			var bill = header.Bills[0];
			var outgoingMessage = Factory.NewWithValidTestData<ZA.Business.CUSRESEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageNum = "TestNum";
			outgoingMessage.EM_LinkedObject = bill;
			outgoingMessage.EM_MessageType = "CAR";
			outgoingMessage.EM_MessageText = "UNH+7616+CUSCAR:D:16A:UN:RCG001'BGM+85:::RFM+F513A29470F34FF391AD0B3DD41E2B5D+4'DTM+137:20180523:102'DTM+136:20180522:102'RFF+ACW:EACBF377B6804C00BA276C8B9D382844'RFF+LO:MAN0000315'NAD+RL+00655953'NAD+MS+00505655TST'NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2019041820160201:MHDSVRZAZAZA'TDT+20++3++00655953:172:20+++:::LIC                           'LOC+35+BW'LOC+36+ZA'LOC+17+KFN'DTM+132:20180522:102'GEI+5+24:71:ZZZ'GEI+5+DB:122:ZZZ'CNI+1+00655953ROADMAN1::::20180521'RFF+BM:THEONE:1'LOC+8+ZADUR'LOC+9+BWBBK'TDT+20'RFF+ABT:00505655KOM20180523007139'NAD+CN++DIGICORE ELECTRONICS (PTY) LTD:37 HILLCLIMB ROAD:DURBAN::3608'NAD+CZ++TIM EXPORT BLNS GROUP:1 FAR STREET'GID+1+5:BAG'FTX+AAA++9+DESC'MEA+AAE+AAB+KGM:5'PCI+24+GGG'UNT+29+7616'";
			var incomingMessage = Factory.NewWithValidTestData<ZA.Business.CUSRESEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = "RCV";
			incomingMessage.EM_ApplicationCode = "ZAC";
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01
BGM+962+F513A29470F34FF391AD0B3DD41E2B5D
DTM+137:20170316:102
LOC+22+ ::ZZZ
GIS+8:120:ZZZ
NAD+AG+00000000
RFF+BH:RFMHBOL1
RFF+ACD:TestNum
RFF+AFB:CARN0111123J
UNT+10+1
".Replace("\r\n", "'");
			var processor = new ZA.Business.MessageProcessor.CUSRESMessageProcessor(new LoggingInformation());
			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			AssertEquals("RegistrationStatus", "8", bill.ABL_BillStatus);
			AssertEquals("RegistrationDate", "16-Mar-17", bill.RegistrationDate.ToShortDateString());
		}

		public void TestMessageContentEscapedWithCorrectCharacterSet()
		{
			SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			var header = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", nameof(ManifestDocumentType.COH), Factory);
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			header.AMA_IsBuyersConsolidation = true;
			var bill1 = header.Bills[0];
			bill1.ABL_BillNumber = "1-+':?-9";
			bill1.ABL_BillIssuer = "ISS1";
			var pack = bill1.Packs.AddNew();
			var cont = header.Containers[0];
			pack.ContainerPK = cont.PK;
			Factory.Save();

			CusCarMessagingHelper.CreateCusCars(header, new[] { bill1 }, header.MessageFunctionSubTypeForAmend);
			AssertEquals(1, header.Messages.Count);
			var message = header.Messages[0].EM_MessageText;

			AssertContains("Bill Num Escaped", "1'RFF+BM:1-?+?'?:??-9'", message);
		}

		public static void SetupZZ(BusinessObjectFactory factory, ZString countryCode)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "Customs Manifest Status");
			var list6 = helper.CreateCusCodeList(countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "6", "Rejected", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var list8 = helper.CreateCusCodeList(countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(list6.PK, "CustomsRejected", "true");
			helper.CreateCusCodeListAttribute(list6.PK, "IUpdateCustomsStatus", "");
			helper.CreateCusCodeListAttribute(list6.PK, "INotify", "");
			helper.CreateCusCodeListAttribute(list8.PK, "CustomsCleared", "true");
			helper.CreateCusCodeListAttribute(list8.PK, "INotify", "");
			helper.CreateCusCodeListAttribute(list8.PK, "IAllowCancel", "true");
			helper.CreateCusCodeListAttribute(list8.PK, "IUpdateCustomsStatus", "");
			factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			// Enable ASY for ZA
			var zzd = Factory.New<ZZRefCusCodeListCombined>();
			zzd.ZZD_Code = Core.Constants.CountryCodes.SouthAfrica;
			zzd.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry;
			zzd.ZZD_StartDate = ZDateTime.BrettsBirthday;
			zzd.ZZD_EndDate = ZDateTime.Now.AddYears(10);
			zzd.ZZD_Description = "Suuth Ifrika";
			zzd.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			Factory.Save();
		}

		void CreateCusPersonAndGlbPerson(string fullName, int serial, AsycudaManifestHeader manifest, string nationality = "ZA", string placeOfIssue = "")
		{
			var gs = Factory.New<GlbStaff>();
			gs.GS_Code = "~" + serial;
			gs.GS_LoginName = gs.GS_Code;
			gs.GS_FullName = fullName;
			gs.GS_RN_NKNationalityCode = nationality;
			gs.GS_RN_NKCountryCode = nationality;
			gs.GS_Gender = serial % 2 == 0 ? "M" : "F";
			gs.GS_Birthdate = ZDate.BrettsBirthday.AddMonths(10 * serial);
			var pas = gs.Certificates.AddNew();
			pas.XZ_Type = Core.Constants.StaffCertificateType.PAS;
			pas.XZ_RefNumber = "PP" + serial.ToString();
			pas.XZ_RN_NKCountryOfIssuance = string.IsNullOrEmpty(placeOfIssue) ? nationality : placeOfIssue;
			pas.XZ_ExpiryOrDueDate = ZDateTime.BrettsBirthday.AddYears(10 * serial);
			var car = gs.Certificates.AddNew();
			car.XZ_Type = Core.Constants.StaffCertificateType.NID;
			car.XZ_RefNumber = "DL" + serial.ToString();
			var glbPerson = GlbPerson.CreateFromStaff(Factory, gs);
			var cusPerson = manifest.Persons.AddNew();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			cusPerson.CPN_IsPassenger = serial > 1;
			cusPerson.OccupationInZA = new ZaOccupations().GetAllCodes()[serial % 2];
			cusPerson.ReasonForMovementInZA = new ZaReasonForMovement().GetAllCodes()[serial % 2];
			cusPerson.TravellerTypeInZA = new ZaTravellerTypes().GetAllCodes()[serial % 4];
			cusPerson.TravelDocumentTypeInZA = new ZaTravelDocumentTypes().GetAllCodes()[serial % 8];
		}

		AsycudaManifestHeader SetUpManifestFortSea(string manifestType, string applicationCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var queryCarrierCode = new ZQuery(RefCarrierCodeSchema.ZZ4_Code, "MAEU");
			queryCarrierCode.AddToFilter(RefCarrierCodeSchema.ZZ4_ZZZ_NKDataGrouping, "ZA");
			var carrierCode = Factory.LoadTop1<RefCarrierCode>(queryCarrierCode) ?? helper.CreateCarrierCode("MAEU", "Mearsk Line", "ZA");
			var queryVesselZZ = new ZQuery(RefVesselZZSchema.ZZO_Code, "A P MOLLER");
			queryVesselZZ.AddToFilter(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, "ZA");
			var vesselZZ = Factory.LoadTop1<RefVesselZZ>(queryVesselZZ) ?? helper.CreateVesselZZ("A P MOLLER", "OVYQ2", "CV", "ZA");
			var queryCarrierVesselPivot = new ZQuery(RefCarrierVesselPivotSchema.ZZQ_ZZ4, carrierCode.PK);
			queryCarrierVesselPivot.AddToFilter(RefCarrierVesselPivotSchema.ZZQ_ZZO, vesselZZ.PK);
			if (!Factory.Exists(typeof(RefCarrierVesselPivot), queryCarrierVesselPivot))
			{
				helper.CreateCarrierVesselPivot(carrierCode.PK, vesselZZ.PK);
			}

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_RadioCallSign = "OVYQ2";
			vessel.RV_Code = "A P MOLLER";
			Factory.Save();

			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", manifestType, Factory, applicationCode);
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_VesselName = vessel.RV_Code;
			var bill = manifest.Bills[0];
			manifest.AMA_ManifestType = manifestType;
			var op = GlbCompany.CurrentCompany.OrgProxy;
			op.CustomsCodes.AddNew("CCC", "ISS1", "ZA");
			op.CustomsCodes.AddNew("AGT", "11111111", "ZA");
			op.CustomsCodes.AddNew("CDP", "22222222", "ZA");
			bill.ABL_BillIssuer = "ISS1";
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = manifest.Containers[0].PK;
			pack.APA_MarksAndNumbers = "Marks and numbs";
			pack.APA_GoodsDescription = "Desc";
			pack.APA_PackQty = 69;
			pack.APA_PackUQ = "PK";
			pack.APA_Volume = 123m;
			pack.APA_Weight = 456m;
			pack.APA_WeightUQ = "KG";
			pack.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			pack.APA_CommodityCode = "comm1";
			return manifest;
		}

		AsycudaManifestHeader SetupManifestForAir(string manifestType, string applicationCode)
		{
			var manifest = CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("ZAJNB", manifestType, Factory, applicationCode);
			manifest.AMA_TransportMode = "AIR";
			manifest.AMA_MasterBill = "1-1-1*2222 222A !";
			manifest.AMA_ManifestType = manifestType;
			manifest.AMA_VesselName = "BA123";
			var op = GlbCompany.CurrentCompany.OrgProxy;
			op.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CCC", "ISS1", "ZA");
			op.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AGT", "11111111", "ZA");
			op.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CDP", "22222222", "ZA");
			var bill = manifest.Bills[0];
			bill.ABL_RL_NKOrigin = "GBMNC"; // IATA != Unloco.Right(3) !!
			bill.ABL_BillIssuer = "ISS1";
			var pack = bill.Packs.AddNew();
			pack.APA_MarksAndNumbers = "Marks and numbs";
			pack.APA_GoodsDescription = "Desc";
			pack.APA_PackQty = 69;
			pack.APA_PackUQ = "PK";
			pack.APA_Volume = 123m;
			pack.APA_Weight = 456m;
			pack.APA_WeightUQ = "KG";
			pack.APA_CommodityCode = "comm1";
			pack.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			return manifest;
		}

		void FactorySavingHandlerForTest(BusinessObjectFactory factory)
		{
			throw new ZSaveException(new ZDataException(new ApplicationException("Fail for Testing"), null, Db.Connection), factory);
		}

		void SetupSealingTypeMapping(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.STYPE, "BTH", "Seal Type Mapping", false);
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, startDate, endDate, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.Customs, Core.Constants.ContainerSealParties.Codes.Customs, startDate, endDate, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.ConsignorShipper, Core.Constants.ContainerSealParties.Codes.ConsignorShipper, startDate, endDate, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.Terminal, Core.Constants.ContainerSealParties.Codes.Terminal, startDate, endDate, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, "CA", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.Customs, "CU", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.Terminal, "TO", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.ConsignorShipper, "SH", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			factory.Save();
		}

		void SetupGisTypeMapping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("CMODE", "BTH", "Seal Type Mapping", false);
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateCusMap("CMODE", "BBK", "BB", startDate, endDate, "ZA");
			helper.CreateCusMap("CMODE", "BLK", "DB", startDate, endDate, "ZA");
			helper.CreateCusMap("CMODE", "CNT", "CN", startDate, endDate, "ZA");
			helper.CreateCusMap("CMODE", "LQD", "LB", startDate, endDate, "ZA");
			helper.CreateCusMap("CMODE", "OTH", "MX", startDate, endDate, "ZA");
			Factory.Save();
		}

		OrgAddress CreateAddress(OrgHeader org, string postCode, string address1, string address2, string city, string state, string companyName)
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.CompanyName = companyName;
			address.Address1 = address1;
			address.Address2 = address2;
			address.City = city;
			address.State = state;
			address.Postcode = postCode;
			return address;
		}
	}
}
