using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	public class CustomsEntryInstructionDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestCEI_DateForDuty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var logger = new TestErrorLogger();
				var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.SouthAfrica);

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = "BLT";
				var inst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst1.CEI_Style = "11";
				var inst2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst2.CEI_Style = "11";
				inst2.CEI_Description = "inst2";
				var inst3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst3.CEI_Style = "X2";

				CombineAssertions(() =>
				{
					var input = new UniversalCustoms.EntryInstruction();
					input.Style = "11";
					input.Description = "inst2";
					input.DateOfValuation = new ZDateTime(2018, 2, 1);
					var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
					var result = reader.ReadIntoBusinessObject();
					AssertEquals(inst2.PK, result.PK);
					AssertEquals("11", result.CEI_Style);
					AssertEquals("inst2", result.CEI_Description);
					AssertEquals(new ZDateTime(2018, 2, 1), result.CEI_DateForDuty);
				});
			}
		}

		public void TestImportCusSupportingInfo()
		{
			var mockProvider = new Mock<IUniversalCustomsDataObjectProvider>();
			mockProvider.CallBase = true;
			var list = new ZArchitecture.Core.CodeDescriptionPairList();
			list.AddPair(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, "ABC Desc");
			list.AddPair(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, "DEF Desc");
			mockProvider
				.Setup(m => m.TableSpecificCusSupportingInfoTypeList(It.IsAny<ZString>(), It.IsAny<string>()))
				.Returns(list);
			var providers = new Hashtable
				{
					{ Core.Constants.CountryCodes.Ireland, new TestObjectHandle(mockProvider.Object) }
				};

			using (ObjectFactory.Substitute("UniversalCustomsDataObjectProviders", providers))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var logger = new TestErrorLogger();
				var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Ireland, Core.Constants.CountryCodes.Ireland);

				var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();

				CombineAssertions(() =>
				{
					var input = new UniversalCustoms.EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance);
					input.SetCustomsSupportingInformationCollection(() => new List<UniversalCustoms.CustomsSupportingInformation>()
					{
							new UniversalCustoms.CustomsSupportingInformation()
							{
								Category = new CodeDescriptionPair() { Code = "CDF" },
								ReferenceNumber = "ABC1"
							},
							new UniversalCustoms.CustomsSupportingInformation()
														{
								Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo },
								ReferenceNumber = "ABC3"
							},
							new UniversalCustoms.CustomsSupportingInformation()
														{
								Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument },
								ReferenceNumber = "ABC0"
							},
							new UniversalCustoms.CustomsSupportingInformation()
														{
								Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo },
								ReferenceNumber = "ABC2"
							},
					});
					var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
					var result = reader.ReadIntoBusinessObject();
					var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, result.PK);
					query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, result.TablePrefix);
					var supportingInfoBOs = Factory.Load<CusSupportingInfo>(query).OrderBy(x => x.CSI_ReferenceNumber).ToArray();
					AssertArrayEqualsByElements(new[] { "ABC0_PRE", "ABC2_OTH", "ABC3_OTH" }, supportingInfoBOs.Select(x => x.CSI_ReferenceNumber + "_" + x.CSI_Type).ToArray());
				});
			}
		}

		public void TestFillCusEntryInstruction()
		{
			Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var logger = new TestErrorLogger();
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.SouthAfrica);
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg6 = Factory.NewWithValidTestData<OrgHeader>();
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = "BLT";
				var inst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst1.CEI_Style = "11";
				var inst2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst2.CEI_Style = "11";
				inst2.CEI_Description = "inst2";
				var inst3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst3.CEI_Style = "X2";

				CombineAssertions(() =>
				{
					var input = new UniversalCustoms.EntryInstruction();
					input.Style = "11";
					input.Description = "inst2";
					var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
					var result = reader.ReadIntoBusinessObject();
					AssertEquals(inst2.PK, result.PK);
					AssertEquals("11", result.CEI_Style);
					AssertEquals("inst2", result.CEI_Description);
				});

				CombineAssertions(() =>
				{
					var input = new UniversalCustoms.EntryInstruction();
					input.Style = "11";
					input.Description = "inst1";
					var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
					var result = reader.ReadIntoBusinessObject();
					AssertNotEquals(inst1.PK, result.PK);
					AssertNotEquals(inst2.PK, result.PK);
					AssertNotEquals(inst3.PK, result.PK);
					AssertEquals(declaration.PK, result.CEI_JE);
					AssertEquals(4, declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Count);
					AssertEquals("11", result.CEI_Style);
					AssertEquals("inst1", result.CEI_Description);
				});

				CombineAssertions(() =>
				{
					var input = new UniversalCustoms.EntryInstruction();
					input.Style = "X2";
					input.CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>()
					{
						new UniversalCustoms.CustomsReference()
						{
							Type = new CodeDescriptionPair() { Code = "CAS" },
							Reference = "CASE001"
						}
					};

					var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
					var result = reader.ReadIntoBusinessObject();
					AssertEquals(inst3.PK, result.PK);
					AssertEquals("X2", result.CEI_Style);
					AssertEquals("", result.CEI_Description);
					var caseNum = Factory.Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, inst3.PK));
					AssertEquals("CEI", caseNum[0].CY_ParentTableCode);
					AssertEquals("CASE001", caseNum[0].CY_Data);
				});

				CombineAssertions(() =>
				{
					var input = new UniversalCustoms.EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance);
					input.Style = "13";
					input.SubStyle = new CodeDescriptionPair() { Code = "SUB", Description = "Sub Style" };
					input.AddInfoCollection = new List<AddInfo>
					{
						new AddInfo() { Key = "MRNToBeReplaced", Value = "MRN" }
					};
					input.AddOrgAddress(writeManager, testOrg1, AddressTypes.Warehouse1);
					input.AddOrgAddress(writeManager, testOrg2, AddressTypes.Warehouse2);
					input.AddOrgAddress(writeManager, testOrg3, AddressTypes.Owner);
					input.AddOrgAddress(writeManager, testOrg4, AddressTypes.BondHolder);
					input.AddOrgAddress(writeManager, testOrg5, DocAddressType.Carrier);
					input.AddOrgAddress(writeManager, testOrg6, DocAddressType.ImportBroker);
					var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
					var result = reader.ReadIntoBusinessObject();
					AssertNotEquals(inst1.PK, result.PK);
					AssertNotEquals(inst2.PK, result.PK);
					AssertNotEquals(inst3.PK, result.PK);
					AssertEquals(5, declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Count);
					AssertEquals("13", result.CEI_Style);
					AssertEquals("SUB", result.CEI_SubStyle);
					AssertEquals("", result.CEI_Description);
					AssertEquals(declaration.PK, result.CEI_JE);
					AssertEquals("MRNToBeReplaced=MRN", result.CEI_AddInfo);
					AssertEquals(testOrg1.MainAddress.PK, result.CEI_OA_Warehouse);
					AssertEquals(testOrg2.MainAddress.PK, result.CEI_OA_Warehouse2);
					AssertEquals(testOrg3.PK, result.CEI_OH_Owner);
					AssertEquals(testOrg4.PK, result.CEI_OH_BondHolder);
					AssertEquals(testOrg5.PK, result.CEI_OH_Carrier);
				});
			}
		}

		[TestDate(2019, 11, 11)]
		public void TestFillCusEntryInstructionWithSuspendedSettersProperties_GB()
		{
			var helperTest = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helperTest.CreateCusCodeType("PORT", "port");
			var codelist1 = helperTest.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, "PORT", "BLE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helperTest.CreateCusCodeListAttribute(codelist1.PK, "Type", "CA3");
			Factory.SaveForTesting();

			Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var logger = new TestErrorLogger();
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = "AIR";
				declaration.JE_RL_NKPortOfArrival = "GBBLE";
				declaration.JE_DateOfArrival = new ZDateTime(2019, 11, 23);
				declaration.JE_LocationOfGoods = "DEU";

				CombineAssertions(() =>
				{
					var input = new UniversalCustoms.EntryInstruction();
					input.DateOfValuation = new ZDateTime(2019, 11, 11);
					var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
					var result = reader.ReadIntoBusinessObject();
					AssertNotNull(result.PK);
					AssertEquals("", result.CEI_Style);
					AssertEquals("", result.CEI_SubStyle);
					AssertEquals("DEU", declaration.JE_LocationOfGoods);
				});

				CombineAssertions(() =>
				{
					var input = new UniversalCustoms.EntryInstruction();
					input.Style = "AAA";
					input.SubStyle = new CodeDescriptionPair() { Code = "S", Description = "Sub Style" };
					var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
					var result = reader.ReadIntoBusinessObject();
					AssertNotNull(result.PK);
					AssertEquals("AAA", result.CEI_Style);
					AssertEquals("S", result.CEI_SubStyle);
					AssertEquals("DEU", declaration.JE_LocationOfGoods);
				});
			}
		}

		[TestDate(2019, 11, 11)]
		public void TestFillCusEntryInstructionWithSuspendedSettersProperties_CN()
		{
			Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var logger = new TestErrorLogger();
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.China);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_RL_NKPortOfArrival = "CNSHG";
				declaration.JE_DateOfArrival = new ZDateTime(2019, 11, 23);
				declaration.JE_LocationOfGoods = "DEU";

				CombineAssertions(() =>
				{
					var input = new UniversalCustoms.EntryInstruction();
					input.DateOfValuation = new ZDateTime(2019, 11, 11);
					var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
					var result = reader.ReadIntoBusinessObject();
					AssertNotNull(result.PK);
					AssertEquals("", result.CEI_Style);
					AssertEquals("", result.CEI_SubStyle);
					AssertEquals("DEU", declaration.JE_LocationOfGoods);
				});

				CombineAssertions(() =>
				{
					var input = new UniversalCustoms.EntryInstruction();
					input.Style = "AAA";
					input.SubStyle = new CodeDescriptionPair() { Code = "S", Description = "Sub Style" };
					var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
					var result = reader.ReadIntoBusinessObject();
					AssertNotNull(result.PK);
					AssertEquals("AAA", result.CEI_Style);
					AssertEquals("S", result.CEI_SubStyle);
					AssertEquals("DEU", declaration.JE_LocationOfGoods);
				});
			}
		}

		public void TestFillAddInfoGroups()
		{
			Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var logger = new TestErrorLogger();
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.China);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = "BLT";
				var inst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst1.CEI_Style = "11";
				inst1.CEI_Description = "inst2";

				var input = new UniversalCustoms.EntryInstruction
				{
					Style = "11",
					Description = "inst2",
					AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>
					{
						new UniversalCustoms.AddInfoGroup
						{
							Type = new CodeDescriptionPair
							{
								Code = "RQD"
							},
							AddInfoCollection = new List<AddInfo>
							{
								new AddInfo
								{
									Key = "DocumentType",
									Value = "11"
								},
								new AddInfo
								{
									Key = "NumberOfCopies",
									Value = "2"
								}
							}
						}
					}
				};

				var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
				var instruction = reader.ReadIntoBusinessObject();

				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, instruction.PK);
				query.AddToFilter(CusAddInfoSchema.B7_Type, "RQD");

				var cusAddInfos = Factory.Load<CusAddInfo>(query);
				AssertEquals(1, cusAddInfos.Length);
				AssertEquals("RQD", cusAddInfos[0].B7_Type);
				AssertEquals("DocumentType=11*NumberOfCopies=2", cusAddInfos[0].B7_AddInfoData);
			}
		}

		public void TestFillDocAddresses()
		{
			Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var logger = new TestErrorLogger();
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Brazil);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

				var input = new UniversalCustoms.EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance);
				input.AddOrgAddress(writeManager, orgHeader, DocAddressType.JustificationContactDetailAddress);
				var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
				var result = reader.ReadIntoBusinessObject();
				AssertEquals(1, declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Count);
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions[0];
				AssertEquals("Fill JustificationContactDetailAddress", orgHeader.MainAddress.PK, instruction.DocAddresses.FindByDocAddressType(DocAddressType.JustificationContactDetailAddress).E2_OA_Address);
			}
		}

		public void TestCEI_Procedure()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var logger = new TestErrorLogger();
				var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.SouthAfrica);

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = "BLT";
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = "11";
				instruction.CEI_Description = "instruction";
				instruction.CEI_Procedure = "09876";

				var input = new UniversalCustoms.EntryInstruction();
				input.Style = "11";
				input.Description = "instruction";
				input.DateOfValuation = new ZDateTime(2018, 2, 1);
				var reader = new CustomsEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration);
				var result = reader.ReadIntoBusinessObject();

				AssertEquals("09876", result.CEI_Procedure);
			}
		}
	}
}
