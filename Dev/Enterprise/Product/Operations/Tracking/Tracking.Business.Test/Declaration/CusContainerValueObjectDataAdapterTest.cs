using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Tracking.Business.Declaration;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(CusContainerValueObjectDataAdapter))]
	sealed class CusContainerValueObjectDataAdapterTest : ValueObjectDataAdapterTest<BaseCusContainer, Xsd.WebContainer>
	{
		protected override ValueObjectDataAdapter<BaseCusContainer, Xsd.WebContainer> GetNewBizObjXmlDataAdapter() => new CusContainerValueObjectDataAdapter();

		protected override string ExpectedRootCollectionElementName => "WebContainers";

		protected override string ExpectedRootElementName => "WebContainer";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyContainer = Factory.NewWithValidTestData<BaseCusContainer>(TestBusinessObjectKind.NoData);
			emptyContainer.CO_ContainerNumber = "test";
			var emptyCusContainerXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyCusContainer.xml", "EmptyCusContainer.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyContainer, emptyCusContainerXmlPath, ValidationKind.None, "Empty CusContainer");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedContainer = Factory.NewWithValidTestData<BaseCusContainer>(TestBusinessObjectKind.NoData);
			populatedContainer.CO_ContainerNumber = "test";
			populatedContainer.CO_FCL_LCL_AIR = "AIR";
			populatedContainer.CO_Seal = "1234";
			populatedContainer.CO_Weight = 123.45m;
			populatedContainer.CO_WeightUQ = "KG";
			var emptyFullCusContainerXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullCusContainer.xml", "FullCusContainer.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedContainer, emptyFullCusContainerXmlPath, ValidationKind.Xsd, "Populated CusContainer");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new[]
					{
						"Mode",
						"NumberOfContainers",
						"Weight/Description",
						"Shipper/OrganisationDetails/Addresses/AddressType",
						"Consignee/OrganisationDetails/Addresses/AddressType",
					};
			}
		}

		protected override bool IsImportFromValueObjectSupported => false;

		protected override bool IsExportToCollectionSupported => false;

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
	}
}
