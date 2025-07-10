using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(MovementsFilter))]
	internal class MovementsFilterTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate]
		public void TestReset()
		{
			OrgAddress address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			AgencyRegistry.Instance.MovementArchiveDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			Filter.FromDate = ZDateTime.Today.AddDays(-5);
			Filter.ToDate = ZDateTime.Today.AddDays(-3);
			Filter.MovementType = "XXX";
			Filter.DepotPK = address.PK;
			AssertEquals("precondition: FromDate", ZDateTime.Today.AddDays(-5), Filter.FromDate);
			AssertEquals("precondition: ToDate", ZDateTime.Today.AddDays(-3), Filter.ToDate);
			AssertEquals("precondition: MovementType", "XXX", Filter.MovementType);
			AssertEquals("precondition: Depot", address.PK, Filter.DepotPK);
			AssertEquals("precondition: Depot_ZAddress.OrgPK", address.OA_OH, Filter.DepotPK_ZAddress.OrgPK);
			Filter.Reset();
			AssertEquals("FromDate", ZDateTime.Today.AddDays(-10), Filter.FromDate);
			AssertEquals("ToDate", ZDateTime.Empty, Filter.ToDate);
			AssertEquals("MovementType", "", Filter.MovementType);
			AssertEquals("Depot", ZGuid.Empty, Filter.DepotPK);
			AssertEquals("Depot_ZAddress.OrgPK", ZGuid.Empty, Filter.DepotPK_ZAddress.OrgPK);
		}

		public void TestMovementTypeFilter()
		{
			Movement1.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			Movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			Movement3.E9_MovementType = ContainerMovementTypes.Codes.OffHire;
			Filter.MovementType = "";
			AssertMatches("Empty Filter", Movement1, Movement2, Movement3);
			Filter.MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			AssertMatches("WGO", Movement1, Movement2);
		}

		public void TestDateFilter()
		{
			ZDateTime today = ZDateTime.Today;
			Movement1.E9_MovementDate = ZDateTime.Empty;
			Movement2.E9_MovementDate = today.AddDays(-3);
			Movement3.E9_MovementDate = today.AddDays(-2);
			Movement4.E9_MovementDate = today.AddDays(-1);
			Filter.FromDate = ZDateTime.Empty;
			Filter.ToDate = ZDateTime.Empty;
			AssertMatches("Empty Filter", Movement1, Movement2, Movement3, Movement4);
			Filter.FromDate = today.AddDays(-2);
			AssertMatches("-2 <= date", Movement1, Movement3, Movement4);
			Filter.ToDate = today.AddDays(-2);
			AssertMatches("-2 <= date <= -2", Movement1, Movement3);
			Filter.FromDate = ZDateTime.Empty;
			AssertMatches("date <= -2", Movement1, Movement2, Movement3);
		}

		public void TestDepotFilter()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgAddress address1A = org1.Addresses.AddNew();
			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgAddress address2A = org2.Addresses.AddNew();
			OrgAddress address2B = org2.Addresses.AddNew();
			Movement1.E9_OA_Depot = ZGuid.Empty;
			Movement2.E9_OA_Depot = address1A.PK;
			Movement3.E9_OA_Depot = address2A.PK;
			Movement4.E9_OA_Depot = address2B.PK;
			Filter.DepotPK = ZGuid.Empty;
			AssertMatches("Empty Filter", Movement1, Movement2, Movement3, Movement4);
			Filter.DepotPK = address2A.PK;
			AssertMatches("Address", Movement3);
			Filter.DepotPK = ZGuid.Empty;
			AssertMatches("Org Only", Movement3, Movement4);
		}

		public void TestVoyageFilter()
		{
			var vessel1 = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();
			var vessel2 = RefVessel.LookupVesselByName("BANOWATI", Factory).First();
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.JV_VoyageFlight = "081";
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = vessel1.RV_FK;
			voyage2.JV_VoyageFlight = "081";
			JobVoyage voyage3 = Factory.New<JobVoyage>();
			voyage3.JV_RV_NKVessel = vessel1.RV_FK;
			voyage3.JV_VoyageFlight = "082";
			JobVoyage voyage4 = Factory.New<JobVoyage>();
			voyage4.JV_RV_NKVessel = vessel2.RV_FK;
			voyage4.JV_VoyageFlight = "081";
			Factory.Save();
			Movement1.E9_JV = voyage1.PK;
			Movement2.E9_JV = voyage2.PK;
			Movement3.E9_JV = voyage3.PK;
			Movement4.E9_JV = voyage4.PK;
			Movement5.E9_JV = ZGuid.Empty;
			Filter.Vessel = ZString.Empty;
			Filter.VoyageNo = ZString.Empty;
			AssertMatches("Empty Filter", Movement1, Movement2, Movement3, Movement4, Movement5);
			Filter.Vessel = "MAJAPAHIT";
			AssertMatches("Vessel", Movement1, Movement2, Movement3);
			Filter.VoyageNo = "081";
			AssertMatches("Vessel, Voyage", Movement1, Movement2);
			Filter.Vessel = "";
			AssertMatches("Voyage", Movement1, Movement2, Movement4);
		}

		public void TestMaxLengths()
		{
			AssertEquals("Vessel MaxLength", JobVoyage.Schema.JV_RV_NKVesselMaxLength, Filter.VesselInfo.MaxLength);
			AssertEquals("VoyageNo MaxLength", JobVoyage.Schema.JV_VoyageFlightMaxLength, Filter.VoyageNoInfo.MaxLength);
		}

		public void TestSetTypeFilterFromMovement()
		{
			Movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			Movement2.E9_MovementType = "";
			Filter.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			Filter.SetToShow(Movement1);
			AssertEquals("if the filter is already filtering on movement type, then change the movement type to filter on", ContainerMovementTypes.Codes.YardGateIn, Filter.MovementType);
			Filter.SetToShow(Movement2);
			AssertEquals("clear the movement type if not set on the desired movement", "", Filter.MovementType);
			Filter.SetToShow(Movement1);
			AssertEquals("if the filter is not already filtering on movement type, then don't add a filter", "", Filter.MovementType);
		}

		public void TestSetDateFilterFromMovement()
		{
			AgencyRegistry.Instance.MovementArchiveDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			ZDateTime now = ZDateTime.Now.ToSmallDateTime();
			Movement1.E9_MovementDate = now.AddDays(-10);
			Movement2.E9_MovementDate = now.AddDays(-11);
			Movement3.E9_MovementDate = now.AddDays(-2);
			Movement4.E9_MovementDate = now.AddDays(-1);
			Movement5.E9_MovementDate = ZDateTime.Empty;
			AssertEquals("precondition:", now.Date.ToDateTime().AddDays(-5), Filter.FromDate);
			AssertEquals("precondition:", ZDateTime.Empty, Filter.ToDate);
			Filter.SetToShow(Movement1);
			AssertEquals("slide down", now.AddDays(-10).AddHours(-1), Filter.FromDate);
			AssertEquals("slide down", now.AddDays(-5).AddHours(-1), Filter.ToDate);
			Filter.SetToShow(Movement2);
			AssertEquals("slide down again", now.AddDays(-11).AddHours(-1), Filter.FromDate);
			AssertEquals("slide down again", now.AddDays(-6).AddHours(-1), Filter.ToDate);
			Filter.SetToShow(Movement1);
			AssertEquals("don't slide if already within range", now.AddDays(-11).AddHours(-1), Filter.FromDate);
			AssertEquals("don't slide if already within range", now.AddDays(-6).AddHours(-1), Filter.ToDate);
			Filter.SetToShow(Movement3);
			AssertEquals("slide up", now.AddDays(-7).AddHours(1), Filter.FromDate);
			AssertEquals("slide up", now.AddDays(-2).AddHours(1), Filter.ToDate);
			Filter.SetToShow(Movement4);
			AssertEquals("slide up again", now.AddDays(-6).AddHours(1), Filter.FromDate);
			AssertEquals("slide up again", now.AddDays(-1).AddHours(1), Filter.ToDate);
			Filter.SetToShow(Movement3);
			AssertEquals("don't slide if already within range", now.AddDays(-6).AddHours(1), Filter.FromDate);
			AssertEquals("don't slide if already within range", now.AddDays(-1).AddHours(1), Filter.ToDate);
			Filter.SetToShow(Movement5);
			AssertEquals("no need to change if desired movement has no date, movements with an empty date will always be shown anyway", now.AddDays(-6).AddHours(1), Filter.FromDate);
			AssertEquals("no need to change if desired movement has no date, movements with an empty date will always be shown anyway", now.AddDays(-1).AddHours(1), Filter.ToDate);
			Filter.FromDate = now.AddDays(-1);
			Filter.ToDate = ZDateTime.Empty;
			Filter.SetToShow(Movement3);
			AssertEquals("within archive days of 'now'", now.AddDays(-2).AddHours(-1), Filter.FromDate);
			AssertEquals("within archive days of 'now'", ZDateTime.Empty, Filter.ToDate);
			Filter.FromDate = now.AddDays(-1);
			Filter.ToDate = now.AddDays(-2);
			Filter.SetToShow(Movement3);
			AssertEquals("minimum of 1 hour each side", now.AddDays(-2).AddHours(-1), Filter.FromDate);
			AssertEquals("minimum of 1 hour each side", now.AddDays(-2).AddHours(1), Filter.ToDate);
		}

		public void TestSetVoyageFilterFromMovement()
		{
			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_RV_NKVessel = vessel.RV_FK;
			voyage1.JV_VoyageFlight = "081";
			Movement1.E9_JV = voyage1.PK;
			Movement2.E9_JV = ZGuid.Empty;
			Filter.Vessel = "BANOWATI";
			Filter.VoyageNo = "082";
			Filter.SetToShow(Movement1);
			AssertEquals("if the filter is already filtering on vessel, then change the vessel to filter on", "MAJAPAHIT", Filter.Vessel);
			AssertEquals("if the filter is already filtering on voyage, then change the voyage to filter on", "081", Filter.VoyageNo);
			Filter.SetToShow(Movement2);
			AssertEquals("clear the vessel if not set on the desired movement", "", Filter.Vessel);
			AssertEquals("clear the voyage if not set on the desired movement", "", Filter.VoyageNo);
			Filter.SetToShow(Movement1);
			AssertEquals("if the filter is not already filtering on vessel, then don't add a filter", "", Filter.Vessel);
			AssertEquals("if the filter is not already filtering on voyage, then don't add a filter", "", Filter.VoyageNo);
		}

		public void TestSetDepotFilterFromMovement()
		{
			OrgHeader depot1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader depot2 = Factory.NewWithValidTestData<OrgHeader>();
			Movement1.E9_OA_Depot = depot1.MainAddress.PK;
			Movement2.E9_OA_Depot = ZGuid.Empty;
			Filter.DepotPK = depot2.MainAddress.PK;
			Filter.SetToShow(Movement1);
			AssertEquals("if the filter is already filtering on depot org, then change the depot org to filter on", depot1.MainAddress.PK, Filter.DepotPK);
			AssertEquals("if the filter is already filtering on depod addr, then change the depot addr to filter on", depot1.PK, Filter.DepotPK_ZAddress.OrgPK);
			Filter.SetToShow(Movement2);
			AssertEquals("clear the depot org if not set on the desired movement", ZGuid.Empty, Filter.DepotPK);
			AssertEquals("clear the depot addr if not set on the desired movement", ZGuid.Empty, Filter.DepotPK_ZAddress.OrgPK);
			Filter.SetToShow(Movement1);
			AssertEquals("if the filter is not already filtering on depot org, then don't add a filter", ZGuid.Empty, Filter.DepotPK);
			AssertEquals("if the filter is not already filtering on depot addr, then don't add a filter", ZGuid.Empty, Filter.DepotPK_ZAddress.OrgPK);
			Filter.DepotPK_ZAddress.OrgPK = depot2.PK;
			Filter.DepotPK = ZGuid.Empty;
			Filter.SetToShow(Movement1);
			AssertEquals("if only filtering on depot org then only update the org part of the filter", ZGuid.Empty, filter.DepotPK);
			AssertEquals("if only filtering on depot org then don't set the address part of the filter", depot1.PK, filter.DepotPK_ZAddress.OrgPK);
		}

		#region Implementation
		void AssertMatches(string message, params ContainerMovement[] movements)
		{
			Filter.Find();
			AssertContainsExactElementsInAnyOrder(message, (m) => m.E9_OtherLocation, movements, Filter.Movements);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MovementsFilter(Factory, new CollectionRelationship(typeof(ContainerMovement)));
		}

		MovementsFilter Filter
		{
			get
			{
				return filter ?? (filter = new MovementsFilter(Factory, Collection.Relationship));
			}
		}

		MovementsFilter filter;
		ContainerMovementCollection Collection
		{
			get
			{
				return collection ?? (collection = new ContainerMovementCollection(Factory, true, new AdhocCollectionRelationship(typeof(ContainerMovement))));
			}
		}

		ContainerMovementCollection collection;
		ContainerMovement Movement1
		{
			get
			{
				if (movement1 == null)
				{
					movement1 = Collection.AddNew();
					movement1.E9_OtherLocation = "Movement 1";
				}

				return movement1;
			}
		}

		ContainerMovement movement1;
		ContainerMovement Movement2
		{
			get
			{
				if (movement2 == null)
				{
					movement2 = Collection.AddNew();
					movement2.E9_OtherLocation = "Movement 2";
				}

				return movement2;
			}
		}

		ContainerMovement movement2;
		ContainerMovement Movement3
		{
			get
			{
				if (movement3 == null)
				{
					movement3 = Collection.AddNew();
					movement3.E9_OtherLocation = "Movement 3";
				}

				return movement3;
			}
		}

		ContainerMovement movement3;
		ContainerMovement Movement4
		{
			get
			{
				if (movement4 == null)
				{
					movement4 = Collection.AddNew();
					movement4.E9_OtherLocation = "Movement 4";
				}

				return movement4;
			}
		}

		ContainerMovement movement4;
		ContainerMovement Movement5
		{
			get
			{
				if (movement5 == null)
				{
					movement5 = Collection.AddNew();
					movement5.E9_OtherLocation = "Movement 5";
				}

				return movement5;
			}
		}

		ContainerMovement movement5;
		#endregion
	}
}
