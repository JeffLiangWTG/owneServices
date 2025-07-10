using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	sealed class FailedMessageBuilderTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerate()
		{
			var builder = new FailedMessageHtmlBuilder();
			builder.WriteEmailHeader("Error Sending E-IDO Message");
			builder.WriteText("Line 1");
			builder.WriteText("Line 2");
			builder.WriteTable("Error Text1", new List<FailedMessage>()
			{
				new FailedMessage("Error Text1")
				{
					MessageNo = "1", MessageDateTime = new DateTime(2010, 2, 5, 11, 30, 00), ContainerNo = "TEST4100013", ContainerType = "20GP", BillOfLading = "BOL1", ShipmentNo = "Shipment1", Principal = "Principal1", Origin = "SGSIN", Destination = "AUBNE",
				},
				new FailedMessage("Error Text1")
				{
					MessageNo = "2", MessageDateTime = new DateTime(2010, 2, 5, 11, 35, 00), ContainerNo = "TEST4100029", ContainerType = "40GP", BillOfLading = "BOL2", ShipmentNo = "Shipment2", Principal = "Principal2", Origin = "NLAMS", Destination = "AUBNE",
				},
			});
			builder.WriteTable("Error Text2", new List<FailedMessage>()
			{
				new FailedMessage("Error Text2")
				{
					MessageNo = "3", MessageDateTime = new DateTime(2010, 2, 5, 11, 40, 00), ContainerNo = "TEST4100035", ContainerType = "20RE", BillOfLading = "BOL3", ShipmentNo = "Shipment3", Principal = "Principal1", Origin = "SGSIN", Destination = "AUSYD",
				},
				new FailedMessage("Error Text2")
				{
					MessageNo = "4", MessageDateTime = new DateTime(2010, 2, 5, 11, 45, 00), ContainerNo = "TEST4100040", ContainerType = "40RE", BillOfLading = "BOL4", ShipmentNo = "Shipment4", Principal = "Principal2", Origin = "NLAMS", Destination = "AUSYD",
				},
			});
			builder.WriteText("Line 3");
			builder.WriteText("Line 4");
			builder.WriteEmailFooter();
			AssertMultilineASCIIEquals("", TestFileHelper.FailedMessageHtmlBuilder.GetSample(), builder.ToString());
		}
	}
}
