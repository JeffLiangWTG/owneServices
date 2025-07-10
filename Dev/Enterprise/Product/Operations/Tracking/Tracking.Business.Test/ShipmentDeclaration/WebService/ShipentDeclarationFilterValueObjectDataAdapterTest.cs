using System;
using CargoWise.IO;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Web.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(ShipentDeclarationFilterValueObjectDataAdapter))]
	sealed class ShipentDeclarationFilterValueObjectDataAdapterTest : ValueObjectDataAdapterTest<TrackingShipmentFilterBusinessObject, WebShipmentFilter>
	{
		public void TestAllNumberFilterRemoveExclusivity()
		{
			var adapter = GetNewBizObjXmlDataAdapter();

			var valueObj = new WebShipmentFilter();
			valueObj.Number = new WebShipmentFilterNumber
			{
				NumberValue = "1001",
				NumberSearchField = ShipmentNumberFieldsList.ALL,
			};

			var filter = NewBusinessObject();
			var context = new ValueObjectImportContext(filter.Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(filter, valueObj, context);

			Assert("NumberSearchField - ALL removes filter exclusivity", filter.RemoveExclusivity);
		}

		public void TestDefaultFilterExclusivity()
		{
			var adapter = GetNewBizObjXmlDataAdapter();

			var valueObj = new WebShipmentFilter();
			valueObj.Number = null;
			var filter = NewBusinessObject();
			var context = new ValueObjectImportContext(filter.Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(filter, valueObj, context);

			Assert("Exclusivity is enabled for default filter", !filter.RemoveExclusivity);
		}

		protected override string ExpectedRootCollectionElementName => "WebShipmentFilters";

		protected override string ExpectedRootElementName => "WebShipmentFilter";

		protected override ValueObjectDataAdapter<TrackingShipmentFilterBusinessObject, WebShipmentFilter> GetNewBizObjXmlDataAdapter()
		{
			return new ShipentDeclarationFilterValueObjectDataAdapter();
		}

		protected override TrackingShipmentFilterBusinessObject NewBusinessObject()
		{
			var filterFactory = new WebFilterBusinessObjectFactory(Factory);
			return filterFactory.New<TrackingShipmentFilterBusinessObject>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var filterBO = NewBusinessObject();
			var emptyShipDecXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyShipDec.xml", "EmptyShipDec.xml");
			return new BusinessObjectAndExpectedOutputFileName(filterBO, emptyShipDecXmlPath, ValidationKind.None, "Empty ShipDescFilter Business Object");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest => Array.Empty<string>();

		protected override bool IsCreateOrUpdateFromValueObjectSupported => false;

		protected override bool IsExportToValueObjectSupported => false;

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
