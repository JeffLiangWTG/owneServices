using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class ScheduledRuleProcessorTest : TestCaseWithFactory
	{
		#region TestInformationMessageForNothingProcessed

		public void TestGetBranchToRunRulesAgainst_ThrowsWithNullArguments()
		{
			var processor = GetProcessor();
			AssertExceptionThrown<ArgumentNullException>(() =>
				processor.GetBranchToRunRulesAgainst(null, Factory.New<ProductionRuleSet>()));
			AssertExceptionThrown<ArgumentNullException>(() =>
				processor.GetBranchToRunRulesAgainst(Factory.GetCachedReadOnlyFactory(), null));
		}

		public void TestGetBranchToRunRulesAgainst()
		{
			var processor = GetProcessor();
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			var warehouse = Helper.CreateWarehouse("WH1");
			ruleSet.PRS_WW_Warehouse = warehouse.PK;
			AssertNotEquals("Precondition.", ZGuid.Empty, warehouse.WW_GB_RelatedCompanyBranch);
			Factory.Save();

			AssertEquals(warehouse.WW_GB_RelatedCompanyBranch,
				processor.GetBranchToRunRulesAgainst(Factory.GetCachedReadOnlyFactory(), ruleSet));
		}

		#endregion

		#region TestInformationMessageForNothingProcessed

		public void TestInformationMessageForNothingProcessed()
		{
			var processor = GetProcessor();
			AssertEquals(GetInformationMessageForNothingProcessed(), processor.InformationMessageForNothingProcessed);
		}

		#endregion

		#region TestErrorContactGroupRegistryItem

		public void TestErrorContactGroupRegistryItem()
		{
			var processor = GetProcessor();
			AssertEquals(GetErrorContactGroupRegistryItem(), processor.ErrorContactGroupRegistryItem);
		}

		#endregion

		#region TestLoadInputFacts_ThroughWithNullArguments

		public void TestLoadInputFacts_ThroughWithNullArguments()
		{
			var processor = GetProcessor();
			AssertExceptionThrown<ArgumentNullException>(() => processor.LoadInputFacts(null, Factory.New<ProductionRuleSet>(), new CancellationToken()));
			AssertExceptionThrown<ArgumentNullException>(() => processor.LoadInputFacts(Factory.GetCachedReadOnlyFactory(), null, new CancellationToken()));
		}

		#endregion

		#region TestProcessResults

		public void TestProcessResults_NullArgs()
		{
			var processor = GetProcessor();
			var notificationsMock = new Mock<INotifications>();
			var sampleResult = new ProductionRulesEngineResult(Enumerable.Empty<IFact>());

			AssertExceptionThrown<ArgumentNullException>(() => processor.ProcessResults(null, sampleResult, notificationsMock.Object, new CancellationToken()));
			AssertExceptionThrown<ArgumentNullException>(() => processor.ProcessResults(Factory, null, notificationsMock.Object, new CancellationToken()));
			AssertExceptionThrown<ArgumentNullException>(() => processor.ProcessResults(Factory, sampleResult, null, new CancellationToken()));
		}

		public void TestProcessResults_NoFacts()
		{
			TestProcessResults_BadFactsCore(Enumerable.Empty<IFact>());
		}

		public void TestProcessResults_UnrelatedFacts()
		{
			TestProcessResults_BadFactsCore(new[] { Mock.Of<IFact>(), Mock.Of<IFact>(), Mock.Of<IFact>() });
		}

		void TestProcessResults_BadFactsCore(IEnumerable<IFact> facts)
		{
			var processor = GetProcessor();
			var notificationsMock = new Mock<INotifications>();

			processor.ProcessResults(Factory, new ProductionRulesEngineResult(facts), notificationsMock.Object, new CancellationToken());

			VerifyNoErrors(notificationsMock);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == GetInformationMessageForNothingProcessed())), Times.Once);

			// No neccasary assertions besides mock verification
			Assert(true);
		}

		#endregion

		#region VerifyNotifications

		protected void VerifyError(string expectedError, Mock<INotifications> notificationsMock)
		{
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal && notif.Message == expectedError)));
		}

		protected void VerifyNoErrors(Mock<INotifications> notificationsMock)
		{
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never);
		}

		protected void VerifyWarning(string expectedWarning, Mock<INotifications> notificationsMock)
		{
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == expectedWarning)));
		}

		#endregion

		protected virtual string GetInformationMessageForNothingProcessed()
		{
			return null;
		}

		protected virtual GuidRegistryItem GetErrorContactGroupRegistryItem()
		{
			return null;
		}

		protected abstract IScheduledRuleProcessor GetProcessor();

		#region Implementation

		public WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
