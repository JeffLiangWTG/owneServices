using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class CustomsEntryInstructionDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestDateOfValuation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "11";
				entryInstruction.CEI_DateForDuty = new ZDateTime(2017, 11, 2);

				var writer = new CustomsEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.SouthAfrica));
				var result = writer.GetDataObject(entryInstruction);
				CombineAssertions(() =>
				{
					AssertEquals(new ZDateTime(2017, 11, 2), result.DateOfValuation);
				});
			}
		}

		public void TestProcedure()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "11";
				entryInstruction.CEI_Procedure = "09876";

				var writer = new CustomsEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.SouthAfrica));
				var result = writer.GetDataObject(entryInstruction);

				AssertEquals("09876", result.Procedure);
			}
		}

		public void TestPopulateCustomsSupportingInformationCollection()
		{
			var mockProvider = new Mock<IUniversalCustomsDataObjectProvider>();
			mockProvider.CallBase = true;
			var list = new ZArchitecture.Core.CodeDescriptionPairList();
			list.AddPair("ABC", "ABC Desc");
			list.AddPair("DEF", "DEF Desc");
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
				var helper = new UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.Ireland);
				var logger = new TestErrorLogger();

				var declaration = Factory.New<BaseJobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var createTime = ZDateTime.Today;
				CreateSupportingInfo(entryInstruction.PK, entryInstruction.TablePrefix, "ABC", "ABC2", createTime);
				CreateSupportingInfo(entryInstruction.PK, entryInstruction.TablePrefix, "DEF", "ABC3", createTime);
				CreateSupportingInfo(entryInstruction.PK, entryInstruction.TablePrefix, "CDF", "ABC1", createTime);
				CreateSupportingInfo(entryInstruction.PK, entryInstruction.TablePrefix, "DEF", "ABC4", createTime.AddMinutes(-11));
				CreateSupportingInfo(entryInstruction.PK, entryInstruction.TablePrefix, "ABC", "ABC5", createTime.AddMinutes(-11));
				CreateSupportingInfo(entryInstruction.PK, entryInstruction.TablePrefix, "DEF", "ABC6", createTime.AddMinutes(-10));
				CreateSupportingInfo(entryInstruction.PK, entryInstruction.TablePrefix, "ABC", "ABC7", createTime.AddMinutes(-10));

				var writer = new CustomsEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), helper);
				var result = writer.GetDataObject(entryInstruction);
				CombineAssertions(() =>
				{
					AssertEquals(6, result.CustomsSupportingInformationCollection.Count);
					AssertCustomsSupportingInformation("1", result.CustomsSupportingInformationCollection[0], "ABC", "ABC Desc", "ABC5");
					AssertCustomsSupportingInformation("2", result.CustomsSupportingInformationCollection[1], "ABC", "ABC Desc", "ABC7");
					AssertCustomsSupportingInformation("3", result.CustomsSupportingInformationCollection[2], "ABC", "ABC Desc", "ABC2");
					AssertCustomsSupportingInformation("4", result.CustomsSupportingInformationCollection[3], "DEF", "DEF Desc", "ABC4");
					AssertCustomsSupportingInformation("5", result.CustomsSupportingInformationCollection[4], "DEF", "DEF Desc", "ABC6");
					AssertCustomsSupportingInformation("6", result.CustomsSupportingInformationCollection[5], "DEF", "DEF Desc", "ABC3");
				});
			}
		}

		void AssertCustomsSupportingInformation(string prefix, CustomsSupportingInformation customsSupportingInformation, ZString category, ZString categoryDesc, ZString reference)
		{
			AssertEquals(prefix + ".Category.Code", category, customsSupportingInformation.Category.Code);
			AssertEquals(prefix + ".Category.Description", categoryDesc, customsSupportingInformation.Category.Description);
			AssertEquals(prefix + ".ReferenceNumber", reference, customsSupportingInformation.ReferenceNumber);
		}

		CusSupportingInfo CreateSupportingInfo(ZGuid parentPK, ZString tablePrefix, ZString type, ZString reference, ZDateTime createTime)
		{
			var result = Factory.New<CusSupportingInfo>();
			result.CSI_Type = type;
			result.CSI_ParentID = parentPK;
			result.CSI_ParentTableCode = tablePrefix;
			result.CSI_ReferenceNumber = reference;
			result.CSI_SystemCreateTimeUtc = createTime;
			return result;
		}

		public void TestPopulateDataObject()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				OrgHeader testOwner = CreateOrgForTesting();
				OrgHeader testBondHolder = CreateOrgForTesting();
				OrgHeader testRemover = CreateOrgForTesting();
				OrgHeader testWareHouse1 = CreateOrgForTesting();
				OrgHeader testWareHouse2 = CreateOrgForTesting();

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInstruction1.CEI_Style = "11";
				testInstruction1.CEI_SubStyle = "SUB";
				testInstruction1.CEI_Description = "Desc1";
				testInstruction1.CEI_MergeBy = "TRF";
				testInstruction1.CEI_OA_Warehouse = testWareHouse1.MainAddress.PK;
				testInstruction1.CEI_OA_Warehouse2 = testWareHouse2.MainAddress.PK;
				testInstruction1.CEI_OH_Owner = testOwner.PK;
				testInstruction1.CEI_OH_BondHolder = testBondHolder.PK;
				testInstruction1.CEI_OH_Carrier = testRemover.PK;

				var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInstruction2.CEI_Style = "22";
				testInstruction2.CEI_Description = "Desc2";

				Type type = null;
				(testInstruction2 as ICusCodeDataTypeSupporter).GetCusCodeDataTypes().TryGetValue("CAS", out type);
				var testCase = Factory.New(type) as CusCodeData;
				testCase.CY_Type = "CAS";
				testCase.CY_Code = "PND";
				testCase.CY_Data = "CASE001";
				testCase.CY_ParentTableCode = "CEI";
				testCase.CY_ParentID = testInstruction2.PK;
				Factory.Save();

				var writer = new CustomsEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, testInstruction1)), new UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.SouthAfrica));

				CombineAssertions("Test 1", () =>
				{
					var instructionData = writer.GetDataObject(testInstruction1);
					AssertEquals("LinkID", 1, instructionData.Link);
					AssertEquals("CPC", "11", instructionData.Style);
					AssertEquals("SubStyle", "SUB", instructionData.SubStyle.Code);
					AssertEquals("Desc", "Desc1", instructionData.Description);
					AssertEquals("MergeBy", "TRF", instructionData.MergeBy.Code);
					AssertEquals("MergeByDESC", "Tariff", instructionData.MergeBy.Description);
					var ownerData = instructionData.OrganizationAddressCollection.First(x => x.AddressType.Value == AddressTypes.Owner);
					var bondHolderData = instructionData.OrganizationAddressCollection.First(x => x.AddressType.Value == AddressTypes.Owner);
					var removerData = instructionData.OrganizationAddressCollection.First(x => x.AddressType.Value == AddressTypes.Owner);
					var warehouse1Data = instructionData.OrganizationAddressCollection.First(x => x.AddressType.Value == AddressTypes.Owner);
					var warehouse2Data = instructionData.OrganizationAddressCollection.First(x => x.AddressType.Value == AddressTypes.Owner);
					AssertEquals("ownerData.CompanyName", testOwner.OH_FullName, ownerData.CompanyName);
					AssertEquals("bondHolder.CompanyName", testBondHolder.OH_FullName, bondHolderData.CompanyName);
					AssertEquals("remover.CompanyName", testRemover.OH_FullName, removerData.CompanyName);
					AssertEquals("warehouse1.CompanyName", testWareHouse1.OH_FullName, warehouse1Data.CompanyName);
					AssertEquals("warehouse2.CompanyName", testWareHouse2.OH_FullName, warehouse2Data.CompanyName);
				});
				CombineAssertions("Test 2", () =>
				{
					var instructionData = writer.GetDataObject(testInstruction2);
					AssertEquals("LinkID", 2, instructionData.Link);
					AssertEquals("CPC", "22", instructionData.Style);
					AssertEquals("Desc", "Desc2", instructionData.Description);
					AssertEquals("MergeBy", "NON", instructionData.MergeBy.Code);
					AssertEquals("MergeByDESC", "No Merge", instructionData.MergeBy.Description);
					AssertEquals(1, instructionData.CustomsReferenceCollection.Count);
					var caseDataObject = instructionData.CustomsReferenceCollection[0];
					AssertEquals("CAS", caseDataObject.Type.Code);
					AssertEquals("Case Number", caseDataObject.Type.Description);
					AssertEquals("PND", caseDataObject.SubType.Code);
					AssertEquals("Pending", caseDataObject.SubType.Description);
					AssertEquals("CASE001", caseDataObject.Reference);
				});
			}
		}

		public void TestOutputAddInfoGroups()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

				var declarationCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)testInstruction1;
				declarationCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CIQRequiredDocument, out var itDocType);
				var addInfo = (CusAddInfo)Factory.New(itDocType);
				addInfo.B7_ParentID = testInstruction1.PK;
				addInfo.B7_ParentTableCode = testInstruction1.TablePrefix;
				addInfo.B7_Type = "RQD";
				addInfo.B7_AddInfoData = "DocumentType=11*NumberOfCopies=2";

				var writer = new CustomsEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, testInstruction1)), new UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.China));
				var output = writer.GetDataObject(testInstruction1);

				AssertEquals(1, output.AddInfoGroupCollection.Count);
				var outputGroup = output.AddInfoGroupCollection.FirstOrDefault();

				AssertEquals("RQD", outputGroup.Type.Code);
				AssertEquals(2, outputGroup.AddInfoCollection.Count);
				Assert(outputGroup.AddInfoCollection.Any(info => info.Key == new ZString?("DocumentType") && info.Value == new ZString?("11")));
				Assert(outputGroup.AddInfoCollection.Any(info => info.Key == new ZString?("NumberOfCopies") && info.Value == new ZString?("2")));
			}
		}

		public void TestPopulateDocAddresses()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var org = CreateOrgForTesting("JCDORGHD");

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var docAddress = instruction1.DocAddresses.AddNew(MasterFiles.Integration.DocAddressType.JustificationContactDetailAddress);
				docAddress.E2_OA_Address = org.MainAddress.PK;
				var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

				Factory.Save();

				var writer = new CustomsEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)), new UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.Brazil));

				CombineAssertions("Instruction with JustificationContactDetailAddress", () =>
				{
					var instructionData = writer.GetDataObject(instruction1);
					var addressData = instructionData.OrganizationAddressCollection.First(x => x.AddressType.Value == nameof(MasterFiles.Integration.DocAddressType.JustificationContactDetailAddress));
					AssertNotNull("JustificationContactDetailAddress populated", addressData);
					AssertEquals("JCDORGHD", addressData.OrganizationCode);
				});
				CombineAssertions("Instruction without JustificationContactDetailAddress", () =>
				{
					var instructionData = writer.GetDataObject(instruction2);
					AssertNull("No OrganizationAddress populated", instructionData.OrganizationAddressCollection);
				});
			}
		}

		OrgHeader CreateOrgForTesting(string code = null, string fullName = null, string mainAddress1 = null)
		{
			var result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_Code = code == null ? result.OH_Code : new ZString(code);
			result.OH_FullName = fullName == null ? result.OH_FullName : new ZString(fullName);
			result.MainAddress.OA_Address1 = mainAddress1 == null ? result.MainAddress.OA_Address1 : new ZString(mainAddress1);
			return result;
		}
	}
}
