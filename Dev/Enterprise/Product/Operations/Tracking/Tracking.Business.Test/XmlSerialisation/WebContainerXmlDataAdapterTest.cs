using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Business;
using Enterprise.Tracking.Business.XmlSerialisation;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(WebContainerXmlDataAdapter))]
	sealed class WebContainerXmlDataAdapterTest : ValueObjectDataAdapterTest<CommonContainer, Xsd.WebContainer>
	{
		protected override ValueObjectDataAdapter<CommonContainer, Xsd.WebContainer> GetNewBizObjXmlDataAdapter() => new WebContainerXmlDataAdapter();

		protected override string ExpectedRootCollectionElementName => "WebContainers";

		protected override string ExpectedRootElementName => "WebContainer";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyContainer = Factory.NewWithValidTestData<CommonContainer>(TestBusinessObjectKind.NoData);
			emptyContainer.JC_ContainerNum = "test";
			emptyContainer.JC_ContainerMode = "";
			var emptyContainerXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyContainer.xml", "EmptyContainer.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyContainer, emptyContainerXmlPath, ValidationKind.None, "Empty Container");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedContainer = Factory.New<CommonContainer>();

			populatedContainer.JC_ContainerNum = "containernum";
			populatedContainer.JC_ContainerCount = 220;
			populatedContainer.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
			populatedContainer.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			populatedContainer.JC_SealNum = "SealNum";
			populatedContainer.JC_GrossWeight = 5;
			populatedContainer.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			var fullContainerXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullContainer.xml", "FullContainer.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedContainer, fullContainerXmlPath, ValidationKind.Xsd, "Populated Container");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"Weight/Description",
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
