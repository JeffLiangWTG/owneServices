using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Tracking.Business.XmlSerialisation;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(WebPackingXmlDataAdapter))]
	sealed class WebPackingXmlDataAdapterTest : ValueObjectDataAdapterTest<TrackingPackLine, Xsd.WebPacking>
	{
		protected override ValueObjectDataAdapter<TrackingPackLine, Xsd.WebPacking> GetNewBizObjXmlDataAdapter() => new WebPackingXmlDataAdapter();

		protected override string ExpectedRootCollectionElementName => "WebPackings";

		protected override string ExpectedRootElementName => "WebPacking";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyPackLine = Factory.NewWithValidTestData<TrackingPackLine>(TestBusinessObjectKind.NoData);
			var emptyPackLineXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyPackLine.xml", "EmptyPackLine.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyPackLine, emptyPackLineXmlPath, ValidationKind.None, "Empty TrackingPackLine");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedPackLine = Factory.New<TrackingPackLine>();

			populatedPackLine.JL_F3_NKPackType = "BSK";
			populatedPackLine.JL_LinePrice = 201;
			populatedPackLine.JL_ActualWeight = new ZDecimal(182);
			populatedPackLine.JL_ActualWeightUQ = Constants.Weight.Grams;
			populatedPackLine.JL_ActualVolume = new ZDecimal(13.299);
			populatedPackLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			populatedPackLine.JL_Description = "Description";

			var fullPackLineXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullPackLine.xml", "FullPackLine.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedPackLine, fullPackLineXmlPath, ValidationKind.Xsd, "Populated TrackingPackLine");
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
					"LinePrice/CurrencyCode",
					"ContainerNumber",
					"Weight/Description",
					"Volume/Description",
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
