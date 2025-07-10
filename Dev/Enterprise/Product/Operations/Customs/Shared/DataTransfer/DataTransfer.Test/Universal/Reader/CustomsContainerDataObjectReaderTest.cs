using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestJobContainerDataIsPopulateAsPartOfCusContainer()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var containerCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)container;
			Assert(containerCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDisposition, out var type));
			var dispositionAddInfo = USDispositionDataAddInfoSchema.Constants.US_Code.Substring(3) + "=12";
			container.Delete();
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();
			containerDataObject.CustomsContainerSize = new CodeDescriptionPair2Char() { Code = "20" };
			var dispositionDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USDisposition },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(dispositionAddInfo)
			};
			containerDataObject.SetAddInfoGroupCollection(() => new List<UniversalCustoms.AddInfoGroup>(new[] { dispositionDataObject }));

			var reader = new CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer>(containerDataObject, logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates), declaration);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			CombineAssertions(delegate
			{
				ContainerDataObjectTestHelper.AssertContentsWithVGM(containerBO.JobContainer);
				AssertContents(containerBO, "OOCL0000027", 72.998m, "LB", "SEAL1", "SEAL2", "LCL", "", "");

				var cusAddInfoBOs = LoadCusAddInfo(containerBO.TablePrefix, containerBO.PK);
				AssertEquals("cusAddInfoBOs.Length", 1, cusAddInfoBOs.Length);
				var dispositionBO = cusAddInfoBOs[0];
				AssertCusAddInfoContents(dispositionBO, containerBO.TablePrefix, containerBO.PK, CusAddInfoTypeAttribute.Codes.USDisposition, partialAddInfoData: dispositionAddInfo);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusContainer found, creating new CusContainer.
Information - Populating CusContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});
		}

		public void TestJobContainerDataIsPopulateAsPartOfCusContainer_UseContainerSize()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<SG.IJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			container.Delete();
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();
			containerDataObject.CustomsContainerSize = new CodeDescriptionPair2Char() { Code = "30" };

			var reader = new CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer>(containerDataObject, logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates), declaration);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			CombineAssertions(delegate
			{
				ContainerDataObjectTestHelper.AssertContentsWithVGM(containerBO.JobContainer);
				AssertContents(containerBO, "OOCL0000027", 72.998m, "LB", "SEAL1", "SEAL2", "LCL", "30", "");
			});
		}

		public void TestContainerCusAddInfo()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var containerCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)container;
			Assert(containerCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDisposition, out var type));
			var dispositionAddInfo = USDispositionDataAddInfoSchema.Constants.US_Code.Substring(3) + "=12";
			container.Delete();
			var containerDataObject = SetupContainer();
			var dispositionDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USDisposition },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(dispositionAddInfo)
			};
			containerDataObject.SetAddInfoGroupCollection(() => new List<UniversalCustoms.AddInfoGroup>(new[] { dispositionDataObject }));

			var reader = new CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer>(containerDataObject, logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates), declaration);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			CombineAssertions(delegate
			{
				var cusAddInfoBOs = LoadCusAddInfo(containerBO.TablePrefix, containerBO.PK);
				AssertEquals("cusAddInfoBOs.Length", 1, cusAddInfoBOs.Length);
				var dispositionBO = cusAddInfoBOs[0];
				AssertCusAddInfoContents(dispositionBO, containerBO.TablePrefix, containerBO.PK, CusAddInfoTypeAttribute.Codes.USDisposition, partialAddInfoData: dispositionAddInfo);
			});
		}

		public void TestBasicContainerLevelFieldMappings()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "ZW0W";
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.ShouldDeleteContainers).Returns(false);
			var declaration = declarationMock.Object;
			var containerDataObject = SetupContainer();
			var reader = new CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer>(containerDataObject, logger, CurrentCompanyHelper, declaration);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			CombineAssertions(delegate
			{
				AssertEquals("containerBO.CO_JE", declaration.PK, containerBO.CO_JE);
				AssertContents(containerBO);
				AssertEquals("containerBO.CO_RC", refContainer.PK, containerBO.CO_RC);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseCusContainer found, creating new BaseCusContainer.
Information - Populating BaseCusContainer...
Information - Successfully loaded matching Container Type.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Information - Successfully loaded matching Container Type.
".Trim(), logger.Logs);
			});
		}

		public void TestContainerModeOnContainerShouldNotBeFallbackIfEmpty()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var dataObject = ContainerDataObjectTestHelper.SetupContainer();
			dataObject.FCL_LCL_AIR = new ContainerMode();
			var reader = new CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer>(dataObject, logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates), declaration);
			var containerBO = reader.ReadIntoBusinessObject();
			AssertEquals("We should not fallback the container mode on container if empty.", ZString.Empty, containerBO.CO_FCL_LCL_AIR);
		}

		public void TestContainerModeShouldBeFilledEvenIfInvalid()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var dataObject = ContainerDataObjectTestHelper.SetupContainer();
			dataObject.FCL_LCL_AIR = new ContainerMode
			{
				Code = "JEG"
			};
			var reader = new CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer>(dataObject, logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates), declaration);
			var containerBO = reader.ReadIntoBusinessObject();
			AssertEquals("We should not fill the container mode even if invalid.", "JEG", containerBO.CO_FCL_LCL_AIR);
		}
	}
}
