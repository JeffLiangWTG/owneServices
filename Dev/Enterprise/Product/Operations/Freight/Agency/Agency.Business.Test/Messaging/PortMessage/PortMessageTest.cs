using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortMessage))]
	internal sealed class PortMessageBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PortMessage(Factory.New<JobVoyage>());
		}

		#endregion
	}

	internal class PortMessageTest : BaseAgencyTest
	{
		public void TestPortSetter_ShouldUpdateDirection()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZWLG";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAWL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAWL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

			var portMessage = new PortMessage(voyage);
			portMessage.Port = "";
			portMessage.Direction = "";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
			{
				portMessage.Port = "NZAWL";
				AssertEquals(Constants.PortDirection.Load, portMessage.Direction);

				portMessage.Port = "NZAKL";
				AssertEquals(Constants.PortDirection.Discharge, portMessage.Direction);

				portMessage.Port = "NZAWL";
				AssertEquals(Constants.PortDirection.Discharge, portMessage.Direction);

				portMessage.Port = "NZWLG";
				AssertEquals(Constants.PortDirection.Load, portMessage.Direction);

				portMessage.Port = "NZAWL";
				AssertEquals(Constants.PortDirection.Load, portMessage.Direction);

				portMessage.Port = "SGSIN";
				AssertEquals("", portMessage.Direction);
			}
		}

		public void TestGetRelatedShipments()
		{
			var voyage_1_1 = Factory.New<JobVoyage>().With(jV_AirSeaRoad: "SEA", jV_RV_NKVessel: "Titanic", jV_VoyageFlight: "401");
			voyage_1_1.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage_1_1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage_1_1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			voyage_1_1.GenerateSailings();

			var voyage_1_2 = Factory.New<JobVoyage>().With(jV_AirSeaRoad: "SEA", jV_RV_NKVessel: "Titanic", jV_VoyageFlight: "401");
			voyage_1_2.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage_1_2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage_1_2.GenerateSailings();

			var voyage_1_3 = Factory.New<JobVoyage>().With(jV_AirSeaRoad: "SEA", jV_RV_NKVessel: "Titanic", jV_VoyageFlight: "401");
			voyage_1_3.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage_1_3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			voyage_1_3.GenerateSailings();

			var voyage_2_1 = Factory.New<JobVoyage>().With(jV_AirSeaRoad: "SEA", jV_RV_NKVessel: "Costa Concordia", jV_VoyageFlight: "6122");
			voyage_2_1.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage_2_1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			voyage_2_1.GenerateSailings();

			var shipments = new[]
			{
				Factory.New<BillOfLading>().With(jS_JX: voyage_1_1.Sailings[0].PK),		//	Voyage_1	UAIEV -> AUSYD		[0]
				Factory.New<BillOfLading>().With(jS_JX: voyage_1_1.Sailings[1].PK),		//	Voyage_1	UAIEV -> USLAX		[1]
				Factory.New<BillOfLading>().With(jS_JX: voyage_1_2.Sailings[0].PK),		//	Voyage_1	UAIEV -> SGSIN		[2]
				Factory.New<BillOfLading>().With(jS_JX: voyage_1_3.Sailings[0].PK),		//	Voyage_1	SGSIN -> USLAX		[3]
				Factory.New<BillOfLading>().With(jS_JX: voyage_2_1.Sailings[0].PK),		//	Voyage_2	UAIEV -> USLAX		[4]
			};

			Factory.Save();

			AssertRelatedShipments(voyage_1_1, "UAIEV", Constants.PortDirection.Load, shipments[0], shipments[1], shipments[2]);
			AssertRelatedShipments(voyage_1_1, "UAIEV", Constants.PortDirection.Discharge);
			AssertRelatedShipments(voyage_1_1, "USLAX", Constants.PortDirection.Discharge, shipments[1], shipments[3]);
			AssertRelatedShipments(voyage_1_2, "SGSIN", Constants.PortDirection.Discharge, shipments[2]);
			AssertRelatedShipments(voyage_1_3, "USLAX", Constants.PortDirection.Discharge, shipments[1], shipments[3]);
			AssertRelatedShipments(voyage_2_1, "UAIEV", Constants.PortDirection.Load, shipments[4]);
		}

		void AssertRelatedShipments(JobVoyage voyage, ZString port, ZString direction, params BillOfLading[] expectedShipments)
		{
			var message = new PortMessage(voyage);

			message.Port = port;
			message.Direction = direction;

			var actualShipments = message.GetRelatedShipments();
			AssertContainsExactElementsInAnyOrder("Shipments", expectedShipments.Select(s => s.PK), actualShipments.Select(s => s.PK));
		}
	}
}
