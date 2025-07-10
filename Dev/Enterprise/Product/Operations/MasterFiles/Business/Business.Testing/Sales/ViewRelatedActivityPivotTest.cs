using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ViewRelatedActivityPivot))]
	public class ViewRelatedActivityPivotTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This business object it is a view", true);
		}

		#region Load / Create

		public void TestLoadPivotsWithChild_WithParentTableCodes()
		{
			var opportunity = Factory.New<OrgOpportunity>();

			var parentOpportunity = Factory.New<OrgOpportunity>();
			var parentInquiry = Factory.New<SalesEnquiry>();
			var parentCommunication = Factory.New<OrgSalesCall>();
			var childInquiry = Factory.New<SalesEnquiry>();

			var parentOpportunityPivot = ViewRelatedActivityPivot.Create(Factory, parentOpportunity, opportunity);
			var parentInquiryPivot = ViewRelatedActivityPivot.Create(Factory, parentInquiry, opportunity);
			var parentCommunicationPivot = ViewRelatedActivityPivot.Create(Factory, parentCommunication, opportunity);
			var childInquiryPivot = ViewRelatedActivityPivot.Create(Factory, opportunity, childInquiry);

			var actualPivots = ViewRelatedActivityPivot.LoadPivotsWithChild(Factory, opportunity, new ZString[] { OrgOpportunitySchema.Constants.Prefix, OrgSalesCallSchema.Constants.Prefix }, false);
			var expectedPivots = new[] { parentOpportunityPivot, parentCommunicationPivot };
			AssertContainsExactElementsInAnyOrder(expectedPivots, actualPivots);
		}

		public void TestLoadPivotsWithParent_WithChildTableCodes()
		{
			var opportunity = Factory.New<OrgOpportunity>();

			var childOpportunity = Factory.New<OrgOpportunity>();
			var childInquiry = Factory.New<SalesEnquiry>();
			var childCommunication = Factory.New<OrgSalesCall>();
			var parentInquiry = Factory.New<SalesEnquiry>();

			var childOpportunityPivot = ViewRelatedActivityPivot.Create(Factory, opportunity, childOpportunity);
			var childInquiryPivot = ViewRelatedActivityPivot.Create(Factory, opportunity, childInquiry);
			var childCommunicationPivot = ViewRelatedActivityPivot.Create(Factory, opportunity, childCommunication);
			var parentInquiryPivot = ViewRelatedActivityPivot.Create(Factory, parentInquiry, opportunity);

			var actualPivots = ViewRelatedActivityPivot.LoadPivotsWithParent(Factory, opportunity, new ZString[] { OrgOpportunitySchema.Constants.Prefix, OrgSalesCallSchema.Constants.Prefix }, false);
			var expectedPivots = new[] { childOpportunityPivot, childCommunicationPivot };
			AssertContainsExactElementsInAnyOrder(expectedPivots, actualPivots);
		}

		#endregion

		#region SalesRelationTreeID

		public void TestSalesRelationTreeIDIsUpdatedOnSave()
		{
			var inqA = Factory.NewWithValidTestData<SalesEnquiry>();
			var inqB = Factory.NewWithValidTestData<SalesEnquiry>();
			var oppA = Factory.NewWithValidTestData<OrgOpportunity>();
			var oppB = Factory.NewWithValidTestData<OrgOpportunity>();
			var comA = Factory.NewWithValidTestData<OrgSalesCall>();
			var oppC = Factory.NewWithValidTestData<OrgOpportunity>();
			var oppD = Factory.NewWithValidTestData<OrgOpportunity>();
			var inqC = Factory.NewWithValidTestData<SalesEnquiry>();
			var pivotInqAInqB = inqA.RelatedChildActivityPivotCollection.AddNewPivot(inqB);
			var pivotInqBOppA = inqB.RelatedChildActivityPivotCollection.AddNewPivot(oppA);
			var pivotOppAOppB = oppA.RelatedChildActivityPivotCollection.AddNewPivot(oppB);
			var pivotOppAComA = oppA.RelatedChildActivityPivotCollection.AddNewPivot(comA);
			var pivotOppBComA = oppB.RelatedChildActivityPivotCollection.AddNewPivot(comA);
			var pivotComAOppC = comA.RelatedChildActivityPivotCollection.AddNewPivot(oppC);
			var pivotComAOppD = comA.RelatedChildActivityPivotCollection.AddNewPivot(oppD);
			var pivotOppcInqC = oppC.RelatedChildActivityPivotCollection.AddNewPivot(inqC);

			/*
			INQA
			  |
			  |
			INQB
			  |
			  |
			OPPA ------
			  |   \     \
			  |    \     \
			OPPB   \    INQD
				\   |
				  \  |
				  COMA
					|   \
					|    \
				  OPPC  OPPD
					|
					|
				  INQB
			*/

			Factory.Save();

			CombineAssertions("Should populate SalesRelationTreeId on initial save", () =>
			{
				AssertEquals("pivotInqAInqB", inqA.PK, pivotInqAInqB.RAP_SalesRelationTreeID);
				AssertEquals("pivotInqBOppA", inqA.PK, pivotInqBOppA.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAOppB", inqA.PK, pivotOppAOppB.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAComA", inqA.PK, pivotOppAComA.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppBComA", inqA.PK, pivotOppBComA.RAP_SalesRelationTreeID);
				AssertEquals("pivotComAOppC", oppC.PK, pivotComAOppC.RAP_SalesRelationTreeID);
				AssertEquals("pivotComAOppD", oppD.PK, pivotComAOppD.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppcInqC", oppC.PK, pivotOppcInqC.RAP_SalesRelationTreeID);
			});

			var inqD = Factory.NewWithValidTestData<SalesEnquiry>();
			var pivotOppAInqD = oppA.RelatedChildActivityPivotCollection.AddNewPivot(inqD);
			Factory.Save();

			CombineAssertions("Appending to tree", () =>
			{
				AssertEquals("pivotInqAInqB", inqA.PK, pivotInqAInqB.RAP_SalesRelationTreeID);
				AssertEquals("pivotInqBOppA", inqA.PK, pivotInqBOppA.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAOppB", inqA.PK, pivotOppAOppB.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAComA", inqA.PK, pivotOppAComA.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppBComA", inqA.PK, pivotOppBComA.RAP_SalesRelationTreeID);
				AssertEquals("pivotComAOppC", oppC.PK, pivotComAOppC.RAP_SalesRelationTreeID);
				AssertEquals("pivotComAOppD", oppD.PK, pivotComAOppD.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppcInqC", oppC.PK, pivotOppcInqC.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAInqD", inqA.PK, pivotOppAInqD.RAP_SalesRelationTreeID);
			});

			pivotInqBOppA.Delete();
			Factory.Save();
			CombineAssertions("Splitting tree", () =>
			{
				AssertEquals("pivotInqAInqB", inqA.PK, pivotInqAInqB.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAOppB", oppA.PK, pivotOppAOppB.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAComA", oppA.PK, pivotOppAComA.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppBComA", oppA.PK, pivotOppBComA.RAP_SalesRelationTreeID);
				AssertEquals("pivotComAOppC", oppC.PK, pivotComAOppC.RAP_SalesRelationTreeID);
				AssertEquals("pivotComAOppD", oppD.PK, pivotComAOppD.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppcInqC", oppC.PK, pivotOppcInqC.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAInqD", oppA.PK, pivotOppAInqD.RAP_SalesRelationTreeID);
			});

			pivotInqBOppA = inqB.RelatedChildActivityPivotCollection.AddNewPivot(oppA);
			Factory.Save();
			CombineAssertions("Merged two trees into one", () =>
			{
				AssertEquals("pivotInqAInqB", inqA.PK, pivotInqAInqB.RAP_SalesRelationTreeID);
				AssertEquals("pivotInqBOppA", inqA.PK, pivotInqBOppA.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAOppB", inqA.PK, pivotOppAOppB.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAComA", inqA.PK, pivotOppAComA.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppBComA", inqA.PK, pivotOppBComA.RAP_SalesRelationTreeID);
				AssertEquals("pivotComAOppC", oppC.PK, pivotComAOppC.RAP_SalesRelationTreeID);
				AssertEquals("pivotComAOppD", oppD.PK, pivotComAOppD.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppcInqC", oppC.PK, pivotOppcInqC.RAP_SalesRelationTreeID);
				AssertEquals("pivotOppAInqD", inqA.PK, pivotOppAInqD.RAP_SalesRelationTreeID);
			});
		}

		public void TestDelete_UpdateSalesRelationTreeIdForDescendants()
		{
			var inqA = Factory.NewWithValidTestData<SalesEnquiry>();
			var inqB = Factory.NewWithValidTestData<SalesEnquiry>();
			var oppA = Factory.NewWithValidTestData<OrgOpportunity>();
			var comA = Factory.NewWithValidTestData<OrgSalesCall>();
			var oppB = Factory.NewWithValidTestData<OrgOpportunity>();
			var oppC = Factory.NewWithValidTestData<OrgOpportunity>();
			var inqC = Factory.NewWithValidTestData<SalesEnquiry>();
			var inqD = Factory.NewWithValidTestData<SalesEnquiry>();
			var inqE = Factory.NewWithValidTestData<SalesEnquiry>();
			var pivotInqAInqB = inqA.RelatedChildActivityPivotCollection.AddNewPivot(inqB);
			var pivotInqBOppA = inqB.RelatedChildActivityPivotCollection.AddNewPivot(oppA);
			var pivotOppAComA = oppA.RelatedChildActivityPivotCollection.AddNewPivot(comA);
			var pivotComAOppB = comA.RelatedChildActivityPivotCollection.AddNewPivot(oppB);
			var pivotOppBOppC = oppB.RelatedChildActivityPivotCollection.AddNewPivot(oppC);
			var pivotOppAInqC = oppA.RelatedChildActivityPivotCollection.AddNewPivot(inqC);
			var pivotInqCInqD = inqC.RelatedChildActivityPivotCollection.AddNewPivot(inqD);
			var pivotInqDInqE = inqD.RelatedChildActivityPivotCollection.AddNewPivot(inqE);

			/*
			INQA
			  |
			  |
			INQB
			  |
			  |
			OPPA--
			  |	  \
			  |	   \
			COMA   INQC
			  |      |
			  |      |
			OPPB   INQD
			  |      |
			  |      |
			OPPC   INQE
			*/

			Factory.Save();

			CombineAssertions("Precondition: Should populate SalesRelationTreeId on initial save", () =>
			{
				AssertEquals(nameof(pivotInqAInqB), inqA.PK, pivotInqAInqB.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotInqBOppA), inqA.PK, pivotInqBOppA.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotOppAComA), inqA.PK, pivotOppAComA.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotComAOppB), oppB.PK, pivotComAOppB.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotOppBOppC), oppB.PK, pivotOppBOppC.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotOppAInqC), inqA.PK, pivotOppAInqC.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotInqCInqD), inqA.PK, pivotInqCInqD.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotInqDInqE), inqA.PK, pivotInqDInqE.RAP_SalesRelationTreeID);
			});

			pivotInqBOppA.Delete();
			Factory.Save();

			CombineAssertions("Deleting pivot should update SalesRelationTreeId for child pivots", () =>
			{
				AssertEquals(nameof(pivotInqAInqB), inqA.PK, pivotInqAInqB.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotOppAComA), oppA.PK, pivotOppAComA.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotComAOppB), oppB.PK, pivotComAOppB.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotOppBOppC), oppB.PK, pivotOppBOppC.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotOppAInqC), oppA.PK, pivotOppAInqC.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotInqCInqD), oppA.PK, pivotInqCInqD.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotInqDInqE), oppA.PK, pivotInqDInqE.RAP_SalesRelationTreeID);
			});

			pivotComAOppB.Delete();
			pivotInqCInqD.Delete();
			pivotInqDInqE.Delete();
			Factory.Save();

			CombineAssertions("Deleting pivot with parent activity that supports multiple parents should not update SalesRelationTreeId for child pivots", () =>
			{
				AssertEquals(nameof(pivotInqAInqB), inqA.PK, pivotInqAInqB.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotOppAComA), oppA.PK, pivotOppAComA.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotOppBOppC), oppB.PK, pivotOppBOppC.RAP_SalesRelationTreeID);
				AssertEquals(nameof(pivotOppAInqC), oppA.PK, pivotOppAInqC.RAP_SalesRelationTreeID);
			});
		}

		#endregion

		#region MoveAllPivotsToSuperActivityIfAvailableOtherwiseDelete

		public void TestMoveAllPivotsToSuperActivityIfAvailableOtherwiseDelete_WithSuperActivity()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var inquiry = Factory.New<SalesEnquiry>();
			var subactivity = Factory.New<DummySubRelatableActivity>();
			var superactivity = Factory.New<DummySuperRelatableActivity>();
			subactivity.SuperActivity = superactivity;
			ViewRelatedActivityPivot.Create(Factory, subactivity, opportunity);
			ViewRelatedActivityPivot.Create(Factory, inquiry, subactivity);

			ViewRelatedActivityPivot.MoveAllPivotsToSuperActivityIfAvailableOtherwiseDelete(subactivity);
			AssertContainsExactElementsInAnyOrder(new[] { opportunity }, ViewRelatedActivityPivot.LoadPivotsWithParent(Factory, superactivity).Select(pivot => pivot.ChildActivity));
			AssertContainsExactElementsInAnyOrder(new[] { inquiry }, ViewRelatedActivityPivot.LoadPivotsWithChild(Factory, superactivity).Select(pivot => pivot.ParentActivity));
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), ViewRelatedActivityPivot.LoadPivotsWithParent(Factory, subactivity).Select(pivot => pivot.ChildActivity));
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), ViewRelatedActivityPivot.LoadPivotsWithChild(Factory, subactivity).Select(pivot => pivot.ParentActivity));
		}

		public void TestMoveAllPivotsToSuperActivityIfAvailableOtherwiseDelete_NoSuperActivity()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var inquiry = Factory.New<SalesEnquiry>();
			var subactivity = Factory.New<DummySubRelatableActivity>();
			ViewRelatedActivityPivot.Create(Factory, subactivity, opportunity);
			ViewRelatedActivityPivot.Create(Factory, inquiry, subactivity);

			ViewRelatedActivityPivot.MoveAllPivotsToSuperActivityIfAvailableOtherwiseDelete(subactivity);
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), ViewRelatedActivityPivot.LoadPivotsWithParent(Factory, subactivity).Select(pivot => pivot.ChildActivity));
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), ViewRelatedActivityPivot.LoadPivotsWithChild(Factory, subactivity).Select(pivot => pivot.ParentActivity));
		}

		public void TestMoveAllPivotsToSuperActivityIfAvailableOtherwiseDelete_ShouldNotCauseDuplicateRecords()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var subactivity = Factory.New<DummySubRelatableActivity>();
			var superactivity = Factory.New<DummySuperRelatableActivity>();
			subactivity.SuperActivity = superactivity;
			ViewRelatedActivityPivot.Create(Factory, subactivity, opportunity);
			ViewRelatedActivityPivot.Create(Factory, superactivity, opportunity);

			ViewRelatedActivityPivot.MoveAllPivotsToSuperActivityIfAvailableOtherwiseDelete(subactivity);

			var pivotsWithSuperActivityParent = ViewRelatedActivityPivot.LoadPivotsWithParent(Factory, superactivity);
			var pivotsWithOpportunityChild = ViewRelatedActivityPivot.LoadPivotsWithChild(Factory, opportunity);

			AssertEquals("No duplicate should exist", 1, pivotsWithSuperActivityParent.Length);
			AssertEquals("No duplicate should exist", 1, pivotsWithOpportunityChild.Length);
			AssertContainsExactElementsInAnyOrder(new[] { opportunity }, pivotsWithSuperActivityParent.Select(pivot => pivot.ChildActivity));
			AssertContainsExactElementsInAnyOrder(new[] { opportunity }, pivotsWithOpportunityChild.Select(pivot => pivot.ChildActivity));
		}

		#endregion

		public void TestRelatedActivitySaving()
		{
			var activity1 = Factory.New<DummyRelatableActivity>();
			var activity2 = Factory.New<DummyRelatableActivity>();

			var pivot1 = ViewRelatedActivityPivot.Create(Factory, activity1, activity2);
			pivot1.RAP_ParentActivityTableCode = GlbCompanyCampaignSchema.Constants.Prefix;
			pivot1.RAP_ChildActivityTableCode = OrgOpportunitySchema.Constants.Prefix;

			pivot1.TablePrefixesThatShouldBeLoadedWithSpecificElementType.Add(GlbCompanyCampaignSchema.Constants.Prefix, typeof(DummyRelatableActivity));
			pivot1.TablePrefixesThatShouldBeLoadedWithSpecificElementType.Add(OrgOpportunitySchema.Constants.Prefix, typeof(DummyRelatableActivity));

			Factory.Save();

			AssertEquals(activity2, activity1.RelatedActivitySaved);
			AssertEquals(activity1, activity2.RelatedActivitySaved);

			activity1 = Factory.New<DummyRelatableActivity>();
			activity2 = Factory.New<DummyRelatableActivity>();

			var pivot2 = ViewRelatedActivityPivot.Create(Factory, activity2, activity1);
			pivot2.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot2.RAP_ChildActivityTableCode = GlbCompanyCampaignSchema.Constants.Prefix;

			Factory.Save();

			AssertEquals(activity2, activity1.RelatedActivitySaved);
			AssertEquals(activity1, activity2.RelatedActivitySaved);
		}

		public void TestSalesRelationTreeDoesNotCreateLoopToSelf()
		{
			var inqA = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			inqA.RelatedChildActivityPivotCollection.AddNewPivot(inqA);
			var expectedMessage = $"Loop in Sales Relation Tree: {GetPKPrefixName(inqA)} -> {GetPKPrefixName(inqA)}";
			AssertSalesRelationLoopErrorMessage(expectedMessage);
		}

		public void TestSalesRelationTreeDoesNotCreateLoop()
		{
			var inqA = Factory.NewWithValidTestData<SalesEnquiry>();
			var inqB = Factory.NewWithValidTestData<SalesEnquiry>();
			var inqC = Factory.NewWithValidTestData<SalesEnquiry>();
			var inqD = Factory.NewWithValidTestData<SalesEnquiry>();
			var inqE = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			inqA.RelatedChildActivityPivotCollection.AddNewPivot(inqB);
			inqB.RelatedChildActivityPivotCollection.AddNewPivot(inqC);
			inqC.RelatedChildActivityPivotCollection.AddNewPivot(inqD);
			inqD.RelatedChildActivityPivotCollection.AddNewPivot(inqE);
			AssertNoExceptionThrown("Precondition: No loop in the sales relation tree", () => Factory.Save());

			var pivotForLoop = inqE.RelatedChildActivityPivotCollection.AddNewPivot(inqA);
			var expectedMessage = $"Loop in Sales Relation Tree: {GetPKPrefixName(inqA)} -> {GetPKPrefixName(inqE)} -> {GetPKPrefixName(inqD)} -> {GetPKPrefixName(inqC)} -> {GetPKPrefixName(inqB)} -> {GetPKPrefixName(inqA)}";
			AssertSalesRelationLoopErrorMessage(expectedMessage);

			inqE.RelatedChildActivityPivotCollection.Delete(pivotForLoop);
			AssertNoExceptionThrown("Precondition: No loop in the sales relation tree", () => Factory.Save());

			pivotForLoop = inqD.RelatedChildActivityPivotCollection.AddNewPivot(inqB);
			expectedMessage = $"Loop in Sales Relation Tree: {GetPKPrefixName(inqB)} -> {GetPKPrefixName(inqD)} -> {GetPKPrefixName(inqC)} -> {GetPKPrefixName(inqB)}";
			AssertSalesRelationLoopErrorMessage(expectedMessage);

			inqD.RelatedChildActivityPivotCollection.Delete(pivotForLoop);
			AssertNoExceptionThrown("Precondition: No loop in the sales relation tree", () => Factory.Save());

			var inqF = Factory.NewWithValidTestData<SalesEnquiry>();
			TestConnection.ExecuteNonQuery($"DELETE FROM dbo.OrgColdCallRegister WHERE O1_PK = '{inqB.PK}' OR O1_PK = '{inqD.PK}'");
			Factory.Save();

			var uncachedFactory = new BusinessObjectFactory();
			var newInqE = uncachedFactory.Load<SalesEnquiry>(inqE.PK);

			newInqE.RelatedChildActivityPivotCollection.AddNewPivot(inqF);
			AssertNoExceptionThrown("Multiple NULL ChildActivities with different (RAP_ChildActivityID - RAP_ChildActivityTableCode) does not trigger loop error.", () => uncachedFactory.Save());
		}

		void AssertSalesRelationLoopErrorMessage(string expectedMessage)
		{
			AssertExceptionThrown("Should not be able to save when there is a loop in the sales relation tree",
									typeof(ZCannotSaveException), "Top level sales relation pivot cannot be found due to loop in the sales relation.",
									() => Factory.Save());
			AssertEquals(ErrorReporter.LastKeyReported, "LoopDetectedInGetTopLevelSalesRelationPivot");
			AssertEquals(ErrorReporter.LastMessageReported, expectedMessage);
			ErrorReporter.Clear();
		}

		string GetPKPrefixName(SalesEnquiry enquiry)
		{
			return $"({enquiry.PK + " - " + enquiry.TablePrefix + " - " + enquiry.HumanReadableName})";
		}
	}
}
