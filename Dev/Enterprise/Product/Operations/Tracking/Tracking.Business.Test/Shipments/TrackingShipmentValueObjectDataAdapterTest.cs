using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(TrackingShipmentValueObjectDataAdapter))]
	sealed class TrackingShipmentValueObjectDataAdapterTest : ValueObjectDataAdapterTest<TrackingShipment, Xsd.WebShipment>
	{
		protected override ValueObjectDataAdapter<TrackingShipment, Xsd.WebShipment> GetNewBizObjXmlDataAdapter() => new TrackingShipmentValueObjectDataAdapter();

		protected override string ExpectedRootCollectionElementName => "WebShipments";

		protected override string ExpectedRootElementName => "WebShipment";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyShipment = Factory.NewWithValidTestData<TrackingShipment>(TestBusinessObjectKind.NoData);
			var emptyShipmentXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyShipment.xml", "EmptyShipment.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyShipment, emptyShipmentXmlPath, ValidationKind.None, "Empty Shipment");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var consol = Factory.New<TrackingConsol>();
			var populatedShipment = (TrackingShipment)consol.Shipments.AddNew();

			populatedShipment.JS_UniqueConsignRef = "S00000008";
			populatedShipment.JS_HouseBill = "HouseBill";
			populatedShipment.JS_BookingReference = "BookingRef";

			populatedShipment.ConsigneePK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			populatedShipment.Consignee.OH_FullName = "Consignee";
			populatedShipment.ConsignorPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			populatedShipment.Consignor.OH_FullName = "Consignor";
			populatedShipment.JS_E_ARV = new ZDateTime(2005, 2, 2);
			populatedShipment.JS_E_DEP = new ZDateTime(2005, 3, 3);
			populatedShipment.JS_RL_NKOrigin = "AUSYD";
			populatedShipment.JS_RL_NKDestination = "MYPKG";
			populatedShipment.JS_GoodsDescription = "GoodsDescription";
			populatedShipment.DetailedGoodsDescriptionNoteText = "GoodsDescription";
			populatedShipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2006, 10, 12, 12, 47, 30).ToDateTime();

			populatedShipment.Consols.AddNew();
			var order = populatedShipment.AttachedOrders.AddNew();
			order.JD_OrderNumber = "order1";
			order.JD_OrderDate = new ZDateTime(2004, 12, 12);

			populatedShipment.JS_RS_NKServiceLevel = "D2D";
			populatedShipment.JS_OuterPacks = 5;
			populatedShipment.JS_ActualVolume = 6;
			populatedShipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			populatedShipment.JS_ActualWeight = 7;
			populatedShipment.JS_UnitOfVolume = Core.Constants.Weight.Kilograms;

			var packLine = populatedShipment.OuterPackLines.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "containernum";
			packLine.JL_Calc_ContainerNumber = container.JC_ContainerNum;

			var note = populatedShipment.Notes.AddNew(true, "long", "text");
			note.ST_NoteType = nameof(StmNoteVisibility.PUB);

			populatedShipment.DocsAndCartage.JP_CustomAttrib1 = "Snow Patrol";
			populatedShipment.DocsAndCartage.JP_CustomAttrib2 = "Open Eyes";
			populatedShipment.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2006, 08, 31, 09, 12, 15).ToDateTime();
			populatedShipment.DocsAndCartage.JP_CustomDate2 = new ZDateTime(1974, 10, 25, 09, 35, 45).ToDateTime();
			populatedShipment.DocsAndCartage.JP_CustomDecimal1 = new ZDecimal(155.232);
			populatedShipment.DocsAndCartage.JP_CustomDecimal2 = new ZDecimal(98);
			populatedShipment.DocsAndCartage.JP_CustomFlag1 = ZBool.False;
			populatedShipment.DocsAndCartage.JP_CustomFlag2 = ZBool.True;

			populatedShipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2006, 10, 01, 09, 31, 28).ToDateTime();
			populatedShipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2006, 10, 02, 12, 30, 00).ToDateTime();
			populatedShipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2006, 10, 10, 09, 30, 25).ToDateTime();
			populatedShipment.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2006, 10, 12, 14, 30, 30).ToDateTime();
			populatedShipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2006, 10, 11, 10, 45, 33).ToDateTime();

			var fullShipmentXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullShipment.xml", "FullShipment.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedShipment, fullShipmentXmlPath, ValidationKind.Xsd, "Populated Shipment");
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
						"Shipper",
						"Consignee",
						"GoodsDescription",
						"DeliveredDate",
						"Packings",
						"Notes",
						"Quantity/Description",
						"Weight/Description",
						"Size/Description",
						"Packings",
						"Consols",
						"Orders",
						"Containers",
						"DocumentLinks",
						"RelatedInvoiceLinks"
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
