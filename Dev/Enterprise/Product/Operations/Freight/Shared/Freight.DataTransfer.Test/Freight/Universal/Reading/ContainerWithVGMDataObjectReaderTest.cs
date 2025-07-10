using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ContainerWithVGMDataObjectReaderTest : TestCaseWithFactory
	{
		public void TestBasicContainerLevelFieldMappings()
		{
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();
			var logger = new TestErrorLogger();
			var reader = new ContainerWithVGMDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(containerBO);

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				ContainerDataObjectTestHelper.AssertContentsWithVGM(containerBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
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

			#endregion
		}

		public void TestDoNotImportGrossWeightOnUnverifiedContainer()
		{
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainer();
			containerDataObject.GrossWeight = 100;
			containerDataObject.GrossWeightVerificationType = new CodeDescriptionPair { Code = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, Description = Core.Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotVerified };

			var logger = new TestErrorLogger();
			var reader = new ContainerWithVGMDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(), (c) => null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertEquals("GrossWeight is behaving correctly", 146.90m, containerBO.JC_GrossWeight);
			AssertEquals("TareWeight is behaving correctly", containerDataObject.TareWeight, containerBO.JC_TareWeight);
			AssertEquals("DunnageWeight is behaving correctly", containerDataObject.DunnageWeight, containerBO.JC_DunnageWeight);
		}

		public void TestJC_GrossWeightVerificationTypeImportLog()
		{
			var containerData = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerData.GrossWeightVerificationDateTime = DateTime.Now;

			containerData.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			containerData.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.GrossWeightVerifiedBy) });

			containerData.TareWeight = 154.23;
			containerData.DunnageWeight = 10.12;
			containerData.GrossWeight = 100;
			containerData.WeightUnit = new UnitOfWeight() { Code = "LB", Description = "Pounds" };

			var logger = new TestErrorLogger();

			var reader = new ContainerWithVGMDataObjectReader<CommonContainer>(containerData, logger, new UniversalObjectFactory(),
				c => null, c =>
				{
					var container = Factory.New<CommonContainer>();
					container.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
					container.JC_GrossWeightVerificationDateTime = new ZDateTime(2016, 04, 19, 12, 0, 0);
					var verifiedByAddress = container.GrossWeightVerifiedByAddress;
					verifiedByAddress.E2_CompanyName = "Test";
					return container;
				});
			var containerBizObj = reader.ReadIntoBusinessObject();
			AssertEquals((ZDecimal)0, containerBizObj.JC_GrossWeight);
			AssertEquals("KG", containerBizObj.JC_GrossWeightUQ);
			AssertEquals(new ZDateTime(2016, 04, 19, 12, 0, 0), containerBizObj.JC_GrossWeightVerificationDateTime);
			AssertEquals(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, containerBizObj.JC_GrossWeightVerificationType);
			AssertEquals("Test", containerBizObj.DocAddresses.FindDocAddressesByType(DocAddressType.GrossWeightVerifiedBy)[0].E2_CompanyName);
			AssertEquals("TareWeight is behaving correctly", 154.23m, containerBizObj.JC_TareWeight);
			AssertEquals("DunnageWeight is behaving correctly", 10.12m, containerBizObj.JC_DunnageWeight);

			Assert(logger.Logs.Contains("Gross Weight Verification Type not found in XML, Gross Weight, Unit, Verified By, Verified Date will not be imported."));
		}

		public void TestInvalidVGMDataImportLog()
		{
			var containerData = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerData.GrossWeightVerificationDateTime = DateTime.Now;
			containerData.GrossWeightVerificationType = new CodeDescriptionPair { Code = "WTA", Description = "Wait at Terminal" };

			containerData.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			containerData.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.GrossWeightVerifiedBy), CompanyName = "Verified Company" });

			containerData.TareWeight = 154.23;
			containerData.DunnageWeight = 10.12;
			containerData.GrossWeight = 0;
			containerData.WeightUnit = new UnitOfWeight() { Code = "LB", Description = "Pounds" };

			var logger = new TestErrorLogger();
			var testDateTime = new ZDateTime(2016, 04, 19, 12, 0, 0);

			var reader = new ContainerWithVGMDataObjectReader<CommonContainer>(containerData, logger, new UniversalObjectFactory(),
				c => null, c =>
				{
					var container = Factory.New<CommonContainer>();
					container.JC_GrossWeight = 100;
					container.JC_GrossWeightUQ = "KG";
					container.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
					container.JC_GrossWeightVerificationDateTime = testDateTime;
					var verifiedByAddress = container.GrossWeightVerifiedByAddress;
					verifiedByAddress.E2_CompanyName = "Test";
					return container;
				});
			var containerBizObj = reader.ReadIntoBusinessObject();
			AssertEquals(100m, containerBizObj.JC_GrossWeight);
			AssertEquals("KG", containerBizObj.JC_GrossWeightUQ);
			AssertEquals(testDateTime, containerBizObj.JC_GrossWeightVerificationDateTime);
			AssertEquals(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, containerBizObj.JC_GrossWeightVerificationType);
			AssertEquals("Test", containerBizObj.DocAddresses.FindDocAddressesByType(DocAddressType.GrossWeightVerifiedBy)[0].E2_CompanyName);
			AssertEquals("TareWeight is behaving correctly", 154.23m, containerBizObj.JC_TareWeight);
			AssertEquals("DunnageWeight is behaving correctly", 10.12m, containerBizObj.JC_DunnageWeight);

			Assert(logger.Logs.Contains(@"When element 'GrossWeightVerificationType' is 'NON' then element 'GrossWeightVerificationDateTime' will not be imported.
When element 'GrossWeightVerificationType' is not 'NON' then 'GrossWeightVerificationDateTime' must be entered and 'GrossWeight' must be greater than 0."));
		}

		public void TestReadIntoBusinessObject_GrossWeightIsTooBig_ReplaceWithInvalidValue()
		{
			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			containerDataObject.GrossWeight = 100000000m;
			containerDataObject.GrossWeightVerificationType = new CodeDescriptionPair { Code = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, Description = Core.Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method1Container };
			var logger = new TestErrorLogger();

			var reader = new ContainerWithVGMDataObjectReader<CommonContainer>(containerDataObject, logger, new UniversalObjectFactory(),
				c => null, c => Factory.New<CommonContainer>());
			var containerBO = reader.ReadIntoBusinessObject();

			AssertEquals("The value should be corrected", 999999m, containerBO.JC_GrossWeight);
		}
	}
}
