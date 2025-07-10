using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class HierarchicalSequenceBuilderTest : TestCaseWithFactory
	{
		public void TestGetRelationshipSequence()
		{
			#region Test Data

			var opportunityA = Factory.New<OrgOpportunity>();
			var opportunityB = Factory.New<OrgOpportunity>();
			var opportunityC = Factory.New<OrgOpportunity>();
			var opportunityD = Factory.New<OrgOpportunity>();
			var opportunityE = Factory.New<OrgOpportunity>();

			opportunityA.RelatedChildActivityPivotCollection.AddNewPivot(opportunityB);
			opportunityB.RelatedChildActivityPivotCollection.AddNewPivot(opportunityC);
			opportunityC.RelatedChildActivityPivotCollection.AddNewPivot(opportunityD);
			opportunityD.RelatedChildActivityPivotCollection.AddNewPivot(opportunityE);

			opportunityC.RelatedChildActivityPivotCollection.AddNewPivot(opportunityA);

			#endregion

			var relationshipSequenceBtoA = HierarchicalSequenceBuilder.GetRelationshipSequence<IRelatableActivity>(x => x.RelatedChildActivityPivotCollection.Activities, opportunityB, opportunityA);
			AssertArrayEqualsByElements(
				new[]
				{
					opportunityB.PK,
					opportunityC.PK,
					opportunityA.PK,
				},
				relationshipSequenceBtoA.Select(x => x.PK).ToArray());

			var relationshipSequenceDtoA = HierarchicalSequenceBuilder.GetRelationshipSequence<IRelatableActivity>(x => x.RelatedChildActivityPivotCollection.Activities, opportunityD, opportunityA);
			AssertArrayEqualsByElements(
				Array.Empty<ZGuid>(),
				relationshipSequenceDtoA.Select(x => x.PK).ToArray());

			var relationshipSequenceBtoAignoringCtoA = HierarchicalSequenceBuilder.GetRelationshipSequence(x => x.RelatedChildActivityPivotCollection.Activities, opportunityB, opportunityA, Tuple.Create<IRelatableActivity, IRelatableActivity>(opportunityC, opportunityA));
			AssertArrayEqualsByElements(
				Array.Empty<ZGuid>(),
				relationshipSequenceBtoAignoringCtoA.Select(x => x.PK).ToArray());
		}
	}
}
