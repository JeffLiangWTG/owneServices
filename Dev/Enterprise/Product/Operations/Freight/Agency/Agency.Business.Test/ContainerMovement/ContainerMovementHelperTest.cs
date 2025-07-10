using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ContainerMovementHelperTest : TestCaseWithFactory
	{
		public void TestHasDuplicate()
		{
			ZDateTime epoch = new ZDateTime(2012, 1, 1, 10, 0, 0);
			RefContainerStock stock1 = NewStock("TEST4100013", "20GP");
			RefContainerStock stock2 = NewStock("FAKE4100029", "20GP");
			ContainerMovement movement1 = NewMovement(stock1, ContainerMovementTypes.Codes.YardGateIn, epoch.AddMinutes(-1).AddSeconds(45));
			ContainerMovement movement2 = NewMovement(stock1, ContainerMovementTypes.Codes.YardGateOut, epoch.AddSeconds(30));
			ContainerMovement movement3 = NewMovement(stock1, ContainerMovementTypes.Codes.YardGateIn, epoch.AddMinutes(1).AddSeconds(35));
			AssertEquals("prerequisite - movement1.E9_MovementDate", new ZDateTime(2012, 1, 1, 10, 0, 0), movement1.E9_MovementDate);
			AssertEquals("prerequisite - movement2.E9_MovementDate", new ZDateTime(2012, 1, 1, 10, 1, 0), movement2.E9_MovementDate);
			AssertEquals("prerequisite - movement3.E9_MovementDate", new ZDateTime(2012, 1, 1, 10, 2, 0), movement3.E9_MovementDate);
			Factory.Save();
			const string type = ContainerMovementTypes.Codes.YardGateIn;
			AssertEquals("conflict with current", movement1, ContainerMovementHelper.FindDuplicate(stock1, type, epoch));
			AssertEquals("no conflict", null, ContainerMovementHelper.FindDuplicate(stock1, type, epoch.AddSeconds(59)));
			AssertEquals("conflict with previous", movement1, ContainerMovementHelper.FindDuplicate(stock1, type, epoch.AddSeconds(-1)));
			AssertEquals("conflict with next", movement3, ContainerMovementHelper.FindDuplicate(stock1, type, epoch.AddSeconds(96)));
		}

		[ExpectNoExceptions]
		public void TestCheckingDuplicate()
		{
			var stock1 = NewStock("TEST4100013", "20GP");
			ContainerMovementHelper.FindDuplicate(stock1, ContainerMovementTypes.Codes.YardGateIn, new ZDateTime(2130, 8, 27, 11, 0, 0));
		}

		public void TestGetEventReferenceForMovement_ComplexTest()
		{
			var lOC = Constants.EventReferenceParameters.Codes.Location;
			var fAC = Constants.EventReferenceParameters.Codes.Facility;
			var mOD = Constants.EventReferenceParameters.Codes.Mode;
			var cTO = Constants.Facilities.Code.Terminal;
			var cFS = Constants.Facilities.Code.Depot;
			var consignee = Constants.Facilities.Code.Consignee;
			var containerYard = Constants.Facilities.Code.ContainerYard;
			AssertReference(ContainerMovementTypes.Codes.WharfGateOut, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(cTO) });
			AssertReference(ContainerMovementTypes.Codes.WharfGateIn, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(cTO) });
			AssertReference(ContainerMovementTypes.Codes.ReturnToWharf, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(cTO) });
			AssertReference(ContainerMovementTypes.Codes.YardGateOut, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(containerYard) });
			AssertReference(ContainerMovementTypes.Codes.YardGateIn, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(containerYard) });
			AssertReference(ContainerMovementTypes.Codes.ReturnedUnshipped, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(containerYard) });
			AssertReference(ContainerMovementTypes.Codes.RePositionIntoYard, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(containerYard) });
			AssertReference(ContainerMovementTypes.Codes.RePositionOutOfYard, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(containerYard) });
			AssertReference(ContainerMovementTypes.Codes.OffHire, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(containerYard) });
			AssertReference(ContainerMovementTypes.Codes.OnHire, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(containerYard) });
			AssertReference(ContainerMovementTypes.Codes.ReShipRequested, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(consignee) });
			AssertReference(ContainerMovementTypes.Codes.DepotGateIn, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(cFS) });
			AssertReference(ContainerMovementTypes.Codes.Discharge, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(cTO), mOD.As("ROA") }, transportMode: "ROA");
			AssertReference(ContainerMovementTypes.Codes.Load, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(cTO), mOD.As("ROA") }, transportMode: "ROA");
			AssertReference(ContainerMovementTypes.Codes.Discharge, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(cTO), mOD.As("SEA") });
			AssertReference(ContainerMovementTypes.Codes.Load, "UAIEV", string.Empty, new[] { lOC.As("UAIEV"), fAC.As(cTO), mOD.As("SEA") });
		}

		void AssertReference(string movementType, string movementDepot, string expectedReferenceFreeText, IEnumerable<KeyValuePair<string, string>> expectedParameters = null, string transportMode = "")
		{
			Func<KeyValuePair<string, string>, string> toString = (pair) => string.Format("{0}={1}", pair.Key, pair.Value);
			Func<string, string> msg = (m) => string.Format("{0} [MovementType={1}, MovementDepot={2}]", m, movementType, movementDepot);
			if (expectedParameters == null)
			{
				expectedParameters = new Dictionary<string, string>();
			}

			var stock = Factory.NewWithValidTestData<RefContainerStock>();
			var movement = NewMovement(stock, movementType, ZDateTime.Now, movementDepot);
			movement.TransportMode = transportMode;
			var reference = ContainerMovementHelper.GetEventReferenceForMovement(movement);
			var actualParameters = StmALog.GetParametersFromReference(reference);
			var actualReferenceFreeText = StmALog.GetFreeTextFromReference(reference);
			AssertEquals(msg("Reference free text"), expectedReferenceFreeText, actualReferenceFreeText);
			AssertContainsExactElementsInAnyOrder(msg("Parameters"), expectedParameters.Select(p => toString(p)), actualParameters.Select(p => toString(p)));
		}

		#region Implementation
		RefContainerStock NewStock(string containerNumber, string containerType)
		{
			RefContainerStock result = Factory.New<RefContainerStock>();
			result.R6_ContainerNum = containerNumber;
			result.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;
			return result;
		}

		static ContainerMovement NewMovement(RefContainerStock stock, ZString type, ZDateTime dateTime)
		{
			ContainerMovement result = stock.Movements.AddNew();
			result.E9_MovementType = type;
			result.E9_MovementDate = dateTime;
			return result;
		}

		ContainerMovement NewMovement(RefContainerStock stock, ZString type, ZDateTime dateTime, ZString depot)
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
			voyage.JV_AirSeaRoad = "SEA";
			voyage.GenerateSailings();
			var movement = NewMovement(stock, type, dateTime);
			movement.E9_OA_Depot = Factory.NewWithValidTestData<OrgHeader>().MainAddress.With(oA_RL_NKRelatedPortCode: depot).PK;
			movement.E9_JV = voyage.PK;
			return movement;
		}
		#endregion
	}
}
