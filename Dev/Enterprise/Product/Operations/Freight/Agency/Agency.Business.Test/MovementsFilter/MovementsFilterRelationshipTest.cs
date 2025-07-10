using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business
{
	[TestedType(typeof(ContainerMovementCollection))]
	internal class MovementsFilterRelationshipTest : ActiveBusinessObjectCollectionTestCase<ContainerMovementCollection>
	{
		public void TestLoad()
		{
			ContainerMovement movement1 = Factory.New<ContainerMovement>();
			movement1.E9_OtherLocation = "Movement1";
			movement1.E9_DetentionDays = 1;
			ContainerMovement movement2 = Factory.New<ContainerMovement>();
			movement2.E9_OtherLocation = "Movement2";
			movement2.E9_DetentionDays = 1;
			ContainerMovement movement3 = Factory.New<ContainerMovement>();
			movement3.E9_OtherLocation = "Movement3";
			movement3.E9_DetentionDays = 2;
			ContainerMovement movement4 = Factory.New<ContainerMovement>();
			movement4.E9_OtherLocation = "Movement4";
			movement4.E9_DetentionDays = 2;
			InnerRelationship.AddToRelationship(movement1);
			InnerRelationship.AddToRelationship(movement2);
			InnerRelationship.AddToRelationship(movement3);
			AssertContainsExactElementsInAnyOrder("Not loaded yet", (c) => c.E9_OtherLocation, System.Array.Empty<ContainerMovement>(), MovementCollection);
			OuterRelationship.Load(Factory, new ZQuery());
			AssertContainsExactElementsInAnyOrder("Loaded with empty query", (c) => c.E9_OtherLocation, new ContainerMovement[] { movement1, movement2, movement3 }, MovementCollection);
			OuterRelationship.Load(Factory, new ZQuery(JobContainerMoveSchema.E9_DetentionDays, (short)2));
			AssertContainsExactElementsInAnyOrder("Loaded with non-empty query", (c) => c.E9_OtherLocation, new ContainerMovement[] { movement3 }, MovementCollection);
			AssertEquals("Load should not affect the inner relationship for movement1", true, InnerRelationship.MatchesRelationshipFilter(movement1, false, true));
			AssertEquals("Load should not affect the inner relationship for movement2", true, InnerRelationship.MatchesRelationshipFilter(movement2, false, true));
			AssertEquals("Load should not affect the inner relationship for movement3", true, InnerRelationship.MatchesRelationshipFilter(movement3, false, true));
			AssertEquals("Load should not affect the inner relationship for movement4", false, InnerRelationship.MatchesRelationshipFilter(movement4, false, true));
		}

		public void TestAddRemoveBaseRelationship()
		{
			ContainerMovement movement1 = Factory.New<ContainerMovement>();
			movement1.E9_OtherLocation = "Movement1";
			ContainerMovement movement2 = Factory.New<ContainerMovement>();
			movement2.E9_OtherLocation = "Movement2";
			AssertEquals("Movement1 not added yet", false, InnerRelationship.MatchesRelationshipFilter(movement1, false, true));
			AssertEquals("Movement2 not added yet", false, InnerRelationship.MatchesRelationshipFilter(movement2, false, true));
			MovementCollection.Add(movement1);
			AssertEquals("Should have proxied the add to relationship", true, InnerRelationship.MatchesRelationshipFilter(movement1, false, true));
			AssertEquals("Movement2 was not added", false, InnerRelationship.MatchesRelationshipFilter(movement2, false, true));
			MovementCollection.RemoveFromRelationship(movement1);
			AssertEquals("Should have proxied the remove from relationship", false, InnerRelationship.MatchesRelationshipFilter(movement1, false, true));
		}

		#region Implementation
		protected override ContainerMovementCollection GetCollectionToTest()
		{
			ICollectionRelationship innerRelationship = new AdhocCollectionRelationship(typeof(ContainerMovement));
			MovementsFilterRelationship outerRelationship = new MovementsFilterRelationship(innerRelationship);
			return new ContainerMovementCollection(Factory, true, outerRelationship);
		}

		ICollectionRelationship InnerRelationship
		{
			get
			{
				return innerRelationship ?? (innerRelationship = new AdhocCollectionRelationship(typeof(ContainerMovement)));
			}
		}

		ICollectionRelationship innerRelationship;
		MovementsFilterRelationship OuterRelationship
		{
			get
			{
				return outerRelationship ?? (outerRelationship = new MovementsFilterRelationship(InnerRelationship));
			}
		}

		MovementsFilterRelationship outerRelationship;
		ContainerMovementCollection MovementCollection
		{
			get
			{
				return movementCollection ?? (movementCollection = new ContainerMovementCollection(Factory, true, OuterRelationship));
			}
		}

		ContainerMovementCollection movementCollection;
		#endregion
	}
}
