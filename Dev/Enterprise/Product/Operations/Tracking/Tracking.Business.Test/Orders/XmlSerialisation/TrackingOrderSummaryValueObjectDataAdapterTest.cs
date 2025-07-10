using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(TrackingOrderSummaryValueObjectDataAdapter))]
	sealed class TrackingOrderSummaryValueObjectDataAdapterTest : ValueObjectDataAdapterTest<Order, Xsd.WebOrderSummary>
	{
		protected override ValueObjectDataAdapter<Order, Xsd.WebOrderSummary> GetNewBizObjXmlDataAdapter() => new TrackingOrderSummaryValueObjectDataAdapter();

		protected override string ExpectedRootCollectionElementName => "WebOrderSummaries";

		protected override string ExpectedRootElementName => "WebOrderSummary";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyOrder = Factory.NewWithValidTestData<TrackingOrder>(TestBusinessObjectKind.NoData);
			emptyOrder.JD_OrderNumber = "test";
			var emptyOrderSummaryXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyOrderSummary.xml", "EmptyOrderSummary.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyOrder, emptyOrderSummaryXmlPath, ValidationKind.None, "Empty Order");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedOrder = Factory.New<TrackingOrder>();
			populatedOrder.JD_OrderNumber = "80123890731";
			populatedOrder.JD_OrderStatus = Core.Constants.OrderStatus.Confirmed;
			populatedOrder.JD_Packs = new ZInt(29);
			populatedOrder.JD_F3_NKPackType = Core.Constants.PkgUnit.Package;
			populatedOrder.JD_OrderDate = new ZDateTime(2005, 01, 01);

			var fullOrderSummaryXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullOrderSummary.xml", "FullOrderSummary.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedOrder, fullOrderSummaryXmlPath, ValidationKind.Xsd, "Populated Order");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
					{
						"Packs/Description",
						"QuoteNumber"
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
