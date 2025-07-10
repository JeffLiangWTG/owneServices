using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyShipmentContainerReferencesTest : TestCaseWithFactory
	{
		public void TestPopulateFromContext()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AgencyShipmentReferences((IXmlEventValueObjectContextValueList)null));
			var context = new Mock<IXmlEventValueObjectContextValueList>();
			context.Setup(ctx => ctx.MBOLNumber).Returns("HBL001");
			context.Setup(ctx => ctx.LloydsNumber).Returns("12345");
			context.Setup(ctx => ctx.VesselName).Returns("TAIKO");
			context.Setup(ctx => ctx.VoyageNumber).Returns("001");
			context.Setup(ctx => ctx.HBOLOriginUNLOCO).Returns("AUSYD");
			context.Setup(ctx => ctx.HBOLDestinationUNLOCO).Returns("NZAKL");
			context.Setup(ctx => ctx.LegOriginUNLOCO).Returns("AUMEL");
			context.Setup(ctx => ctx.LegDestinationUNLOCO).Returns("NZCHC");
			context.Setup(ctx => ctx.ContainerNumbers).Returns(new List<ZString> { "TEST4100013" });
			context.Setup(ctx => ctx.ContainerISOCode).Returns("22G0");
			context.Setup(ctx => ctx.ContainerReleaseNumber).Returns("REL");
			context.Setup(ctx => ctx.GoodsItemID).Returns("");
			var references = new AgencyShipmentContainerReferences(context.Object, "TEST4100013");
			AssertEquals("TEST4100013", references.ContainerNumber);
			AssertEquals("22G0", references.ContainerISOCode);
			AssertEquals("REL", references.ContainerReleaseNumber);
			AssertEquals("HBL001", references.OceanBillNumber);
			AssertEquals("", references.GoodsItemID);
			AssertEquals(1, references.SailingReferences.Count());
			AssertEquals("12345", references.SailingReferences.ElementAt(0).LloydsNumber);
			AssertEquals("TAIKO", references.SailingReferences.ElementAt(0).VesselName);
			AssertEquals("001", references.SailingReferences.ElementAt(0).VoyageNumber);
			AssertEquals("AUMEL", references.SailingReferences.ElementAt(0).PortOfLoading);
			AssertEquals("NZCHC", references.SailingReferences.ElementAt(0).PortOfDischarge);
		}

		public void TestPopulateFromAgencyShipmentContainer()
		{
			var container = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "REL", "AUSYD", "NZAKL", "USS ESSES", "001", "12345", "HBL001", "S001");
			var references = new AgencyShipmentContainerReferences(container);
			AssertEquals("HBL001", references.OceanBillNumber);
			AssertEquals("TEST4100013", references.ContainerNumber);
			AssertEquals("22G0", references.ContainerISOCode);
			AssertEquals("REL", references.ContainerReleaseNumber);
			AssertEquals("", references.GoodsItemID);
			AssertContainsExactElementsInAnyOrder(new[] { "USS ESSES|001|AUSYD|NZAKL" }, references.SailingReferences.Select(Format));
		}

		public void TestPopulateFromContext_TopLevelPack()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AgencyShipmentReferences((IXmlEventValueObjectContextValueList)null));
			var context = new Mock<IXmlEventValueObjectContextValueList>();
			context.Setup(ctx => ctx.MBOLNumber).Returns("HBL001");
			context.Setup(ctx => ctx.LloydsNumber).Returns("12345");
			context.Setup(ctx => ctx.VesselName).Returns("TAIKO");
			context.Setup(ctx => ctx.VoyageNumber).Returns("001");
			context.Setup(ctx => ctx.HBOLOriginUNLOCO).Returns("AUSYD");
			context.Setup(ctx => ctx.HBOLDestinationUNLOCO).Returns("NZAKL");
			context.Setup(ctx => ctx.LegOriginUNLOCO).Returns("AUMEL");
			context.Setup(ctx => ctx.LegDestinationUNLOCO).Returns("NZCHC");
			context.Setup(ctx => ctx.GoodsItemID).Returns("TEST4100013");
			context.Setup(ctx => ctx.ContainerNumbers).Returns(new List<ZString>());
			context.Setup(ctx => ctx.ContainerISOCode).Returns("");
			context.Setup(ctx => ctx.ContainerReleaseNumber).Returns("");
			var references = new AgencyShipmentContainerReferences(context.Object, "");
			AssertEquals("TEST4100013", references.GoodsItemID);
			AssertEquals("HBL001", references.OceanBillNumber);
			AssertEquals("", references.ContainerNumber);
			AssertEquals("", references.ContainerISOCode);
			AssertEquals("", references.ContainerReleaseNumber);
			AssertEquals(1, references.SailingReferences.Count());
			AssertEquals("12345", references.SailingReferences.ElementAt(0).LloydsNumber);
			AssertEquals("TAIKO", references.SailingReferences.ElementAt(0).VesselName);
			AssertEquals("001", references.SailingReferences.ElementAt(0).VoyageNumber);
			AssertEquals("AUMEL", references.SailingReferences.ElementAt(0).PortOfLoading);
			AssertEquals("NZCHC", references.SailingReferences.ElementAt(0).PortOfDischarge);
		}

		public void TestPopulateFromAgencyShipmentContainer_TopLevelPack()
		{
			var container = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, "TEST4100013", "AUSYD", "NZAKL", "USS ESSES", "001", "12345", "HBL001", "S001", Constants.ContainerModes.RollOnRollOff);
			var references = new AgencyShipmentContainerReferences(container);
			AssertEquals("HBL001", references.OceanBillNumber);
			AssertEquals("TEST4100013", references.GoodsItemID);
			AssertEquals("", references.ContainerNumber);
			AssertEquals("", references.ContainerISOCode);
			AssertEquals("", references.ContainerReleaseNumber);
			AssertContainsExactElementsInAnyOrder(new[] { "USS ESSES|001|AUSYD|NZAKL" }, references.SailingReferences.Select(Format));
		}

		#region Implementation
		string Format(SailingReference sailingReference)
		{
			return string.Format("{0}|{1}|{2}|{3}", sailingReference.VesselName, sailingReference.VoyageNumber, sailingReference.PortOfLoading, sailingReference.PortOfDischarge);
		}
		#endregion
	}
}
