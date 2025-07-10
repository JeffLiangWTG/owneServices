using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(TopLevelPackPackingLineCollectionReader<AgencyShipmentContainer>))]
	internal class TopLevelPackPackingLineCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var links = new LinksManager();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "3000";
			substance.DG_Variant = "c";
			substance.DG_FlashPoint = "100 C";
			substance.DG_Class = "Clas";
			substance.DG_PG = "Gr1";
			substance.DG_PSN = "Name1";
			substance.DG_TechName = "T";
			substance.DG_MP = "Y";
			var billOfLading = Factory.New<BillOfLading>();
			var container1 = billOfLading.ShippingContainers.AddNew();
			container1.JC_ContainerNum = "AAA";
			container1.JC_Description = "DESC";
			var undg1 = container1.UNDGs.AddNew();
			var contact1 = container1.Factory.New<OrgContact>();
			undg1.DI_OC_DGContact = contact1.PK;
			undg1.DI_DG = substance.PK;
			var container2 = billOfLading.ShippingContainers.AddNew();
			container2.JC_ContainerNum = "BBB";
			var container3 = billOfLading.ShippingContainers.AddNew();
			container3.JC_ContainerNum = "YYY";
			container3.JC_Description = "YYY DESC";
			var dataObject1 = CreateDataObject(container1.JC_ContainerNum, 3);
			dataObject1.GoodsDescription = "ANGRYDESC";
			var dataObject2 = CreateDataObject("ZZZ", 4);
			dataObject2.GoodsDescription = "KHERSON";
			var dataObject3 = CreateDataObject("MMM", 5);
			dataObject3.GoodsDescription = "MMM DESC";
			links.AddLink(LinksManager.LinkType.Container, 5, container3);
			var logger = new TestErrorLogger();
			var reader = new TopLevelPackPackingLineCollectionReader<AgencyShipmentContainer>(new[] { dataObject1, dataObject2, dataObject3 }, logger, new UniversalObjectFactory(), billOfLading.ShippingContainers, links);
			reader.ReadIntoCollection();
			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching BillOfLadingContainer.
Information - Populating BillOfLadingContainer...
Information - No matching AgencyShipmentContainer found, creating new AgencyShipmentContainer.
Information - Populating AgencyShipmentContainer...
Information - Successfully loaded matching BillOfLadingContainer.
Information - Populating BillOfLadingContainer...
".Trim(), logger.Logs);
			AssertContainsExactElementsInAnyOrder(new[] { "AAA|ANGRYDESC", "ZZZ|KHERSON", "MMM|MMM DESC" }, FormatContainers(billOfLading));
			AssertEquals(0, container1.UNDGs.Count);
		}

		#region Implementation
		string[] FormatContainers(AgencyShipment bo)
		{
			return bo.ShippingContainers.Cast<AgencyShipmentContainer>().Select(c => string.Format("{0}|{1}", c.JC_ContainerNum, c.JC_Description)).ToArray();
		}

		PackingLine CreateDataObject(string vin, ZInt? containerLink)
		{
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.ReferenceNumber = vin;
			dataObject.ContainerLink = containerLink;
			dataObject.SetUNDGCollection(() => new List<UNDG>());
			return dataObject;
		}
		#endregion
	}
}
