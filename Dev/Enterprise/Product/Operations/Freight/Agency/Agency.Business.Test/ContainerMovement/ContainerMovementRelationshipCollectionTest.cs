using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerMovementCollection))]
	internal class ContainerMovementRelationshipCollectionTest : ActiveBusinessObjectCollectionTestCase<ContainerMovementCollection>
	{
		public void TestUpdateFilters()
		{
			RefContainerStock stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "TEST4100011";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			RefContainerStock stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "TEST4100027";
			stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage1.GenerateSailings();
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage2.GenerateSailings();
			JobVoyage voyage3 = Factory.New<JobVoyage>();
			voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage3.GenerateSailings();
			ContainerMovement movement11a = AddMovement(stock1, voyage1, "11a");
			ContainerMovement movement11b = AddMovement(stock1, voyage1, "11b");
			ContainerMovement movement12a = AddMovement(stock1, voyage2, "12a");
			ContainerMovement movement13a = AddMovement(stock1, voyage3, "13a");
			ContainerMovement movement21a = AddMovement(stock2, voyage1, "21a");
			Factory.Save();
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = voyage1.Sailings[0].PK;
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = stock1.R6_ContainerNum;
			ContainerMovementRelationship relationship = new ContainerMovementRelationship(container);
			ContainerMovementCollection collection = new ContainerMovementCollection(Factory, false, relationship);
			AssertContainsExactElementsInAnyOrder("stock1, voyage1", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement11a, movement11b }, collection);
			Transport precarrage = shipment.Transports.AddNew();
			precarrage.JW_IsLinked = true;
			precarrage.JW_JX = voyage3.Sailings[0].PK;
			relationship.UpdateFilters();
			AssertContainsExactElementsInAnyOrder("stock1, voyage1, voyage3", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement11a, movement11b, movement13a }, collection);
			shipment.JS_JX = voyage2.Sailings[0].PK;
			relationship.UpdateFilters();
			AssertContainsExactElementsInAnyOrder("stock1, voyage2", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement12a, movement13a }, collection);
			shipment.JS_JX = ZGuid.Empty;
			relationship.UpdateFilters();
			AssertContainsExactElementsInAnyOrder("stock1, no voyage", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement13a }, collection);
			precarrage.Delete();
			precarrage = null;
			relationship.UpdateFilters();
			AssertContainsExactElementsInAnyOrder("stock1, no voyage", (m) => m.E9_OtherLocation, System.Array.Empty<ContainerMovement>(), collection);
			shipment.JS_JX = voyage1.Sailings[0].PK;
			container.JC_ContainerNum = stock2.R6_ContainerNum;
			relationship.UpdateFilters();
			AssertContainsExactElementsInAnyOrder("stock2, voyage1", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement21a }, collection);
			container.JC_ContainerNum = "";
			relationship.UpdateFilters();
			AssertContainsExactElementsInAnyOrder("no stock, voyage1", (m) => m.E9_OtherLocation, System.Array.Empty<ContainerMovement>(), collection);
		}

		#region Implementation
		ContainerMovement AddMovement(RefContainerStock stock, JobVoyage voyage, string label)
		{
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_JV = voyage.PK;
			movement.E9_OtherLocation = label;
			return movement;
		}

		protected override ContainerMovementCollection GetCollectionToTest()
		{
			return new ContainerMovementCollection(Factory, false, new ContainerMovementRelationship(Container));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ContainerMovement movement = Factory.New<ContainerMovement>();
			movement.E9_R6 = Stock.PK;
			movement.E9_JV = Sailing.Voyage.PK;
			return movement;
		}

		JobSailing Sailing
		{
			get
			{
				if (sailing == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
					voyage.GenerateSailings();
					sailing = voyage.Sailings[0];
				}

				return sailing;
			}
		}

		JobSailing sailing;
		RefContainerStock Stock
		{
			get
			{
				if (stock == null)
				{
					stock = Factory.New<RefContainerStock>();
					stock.R6_ContainerNum = "FROG4100011";
				}

				return stock;
			}
		}

		RefContainerStock stock;
		AgencyShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<AgencyShipment>();
					shipment.JS_JX = Sailing.PK;
				}

				return shipment;
			}
		}

		AgencyShipment shipment;
		AgencyShipmentContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Shipment.RealContainers.AddNew();
					container.JC_ContainerNum = Stock.R6_ContainerNum;
				}

				return container;
			}
		}

		AgencyShipmentContainer container;
		#endregion
	}
}
