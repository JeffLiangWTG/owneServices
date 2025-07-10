using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(PortCallResponse))]
	public class PortCallResponseTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetValues()
		{
			var portCallItem = GetNewPortCallItem();
			var response = new PortCallResponse();

			response.RequestType = PortCallRequestType.Load;
			response.SetValues(portCallItem);
			AssertSetValuesResult(response, "NUMBEROUT", "DEP1", new ZDateTime(2017, 08, 05));

			portCallItem.Etd = null;
			response.SetValues(portCallItem);
			AssertSetValuesResult(response, "NUMBEROUT", "DEP1", ZDateTime.Empty);

			portCallItem = GetNewPortCallItem();
			response.RequestType = PortCallRequestType.Discharge;
			response.SetValues(portCallItem);
			AssertSetValuesResult(response, "NUMBERIN", "ARV1", new ZDateTime(2017, 08, 12));

			portCallItem.Eta = null;
			response.SetValues(portCallItem);
			AssertSetValuesResult(response, "NUMBERIN", "ARV1", ZDateTime.Empty);
		}

		PortCallItem GetNewPortCallItem()
		{
			return new PortCallItem
			{
				Vessel = new Vessel
				{
					VesselName = "VES1",
					ImoNumber = "IMO1",
					CallSign = "CALL1",
				},
				DepartureNumber = "DEP1",
				ArrivalNumber = "ARV1",
				Port = new Port { Unloco = "ZADUR" },
				Carrier = new Carrier { Code = "DHRC" },
				VoyageNumberIn = "NUMBERIN",
				VoyageNumberOut = "NUMBEROUT",
				Eta = new DateTime(2017, 08, 12),
				Ata = new DateTime(2017, 08, 13),
				Etd = new DateTime(2017, 08, 05),
				Atd = new DateTime(2017, 08, 06)
			};
		}

		void AssertSetValuesResult(PortCallResponse response, ZString expectedVoyageNumber, ZString expectedRefNumber, ZDateTime expectedEstimatedTime)
		{
			CombineAssertions(() =>
			{
				AssertEquals("VES1", response.VesselName);
				AssertEquals("IMO1", response.IMO);
				AssertEquals("CALL1", response.CallSign);
				AssertEquals("DHRC", response.CarrierCode);
				AssertEquals(expectedRefNumber, response.ReferenceNumber);
				AssertEquals(expectedVoyageNumber, response.VoyageNumber);
				AssertEquals(expectedEstimatedTime, response.EstimatedTime);
			});
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PortCallResponse();
		}

		#endregion
	}
}
