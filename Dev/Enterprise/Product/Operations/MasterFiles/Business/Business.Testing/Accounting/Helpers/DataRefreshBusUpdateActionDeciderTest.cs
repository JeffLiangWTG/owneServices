using System;
using System.Data;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class DataRefreshBusUpdateActionDeciderTest : TestCaseWithFactory
	{
		public void TestObjectFactoryUsesThisClass()
		{
			AssertType<DataRefreshBusUpdateActionDecider>(ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>());
		}

		public void TestSkipDataRefreshBusUpdateBusinessContextIsSetOnlyOnceOnBusinessObject()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();

			var subscriberFactory = new BusinessObjectFactory();
			var subscriberCharge = subscriberFactory.Load<JobChargeForTest>(charge.PK);
			var publisherFactory = new BusinessObjectFactory();
			var publisherCharge = publisherFactory.Load<JobChargeForTest>(charge.PK);

			publisherCharge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			subscriberCharge.JR_RX_NKCostCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, subscriberCharge, publisherCharge, subscriberCharge.GetStrictProperties());
			Assert(subscriberCharge.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
			ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, subscriberCharge, publisherCharge, subscriberCharge.GetStrictProperties());
			Assert(subscriberCharge.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));

			ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().RemoveSkipDataRefreshBusUpdateBusinessContexts(subscriberCharge);
			Assert(!subscriberCharge.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
		}

		public void TestRemoveSkipDataRefreshBusUpdateBusinessContexts()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();

			Assert(!dummyBizObj.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
			ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().RemoveSkipDataRefreshBusUpdateBusinessContexts(dummyBizObj);
			Assert(!dummyBizObj.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));

			dummyBizObj.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange);
			Assert(dummyBizObj.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
			ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().RemoveSkipDataRefreshBusUpdateBusinessContexts(dummyBizObj);
			Assert(!dummyBizObj.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
		}

		public void TestShouldApplyDataRefreshBusUpdateIgnoresDisplaySequenceChangeInJobCharge()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();

			var subscriberFactory = new BusinessObjectFactory();
			var subscriberCharge = subscriberFactory.Load<JobChargeForTest>(charge.PK);

			var publisherFactory = new BusinessObjectFactory();
			var publisherCharge = publisherFactory.Load<JobChargeForTest>(charge.PK);

			Assert(!subscriberCharge.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
			publisherCharge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			AssertEquals("No changes on subscriber charge", true, ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, subscriberCharge, publisherCharge, subscriberCharge.GetStrictProperties()));
			Assert(!subscriberCharge.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));

			subscriberCharge.JR_DisplaySequence = 3;
			AssertEquals("display sequence changed on subscriber charge", true, ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, subscriberCharge, publisherCharge, subscriberCharge.GetStrictProperties()));
			Assert(!subscriberCharge.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));

			subscriberCharge.JR_Desc = "test description";
			AssertEquals("display sequence and description changed on subscriber charge", false, ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, subscriberCharge, publisherCharge, subscriberCharge.GetStrictProperties()));
			Assert(subscriberCharge.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
		}

		[UseSnapshotProtection(true)]
		public void TestShouldApplyDataRefreshBusUpdate_ShouldNotReportCrossThreadAccessException()
		{
			var subscriber = Factory.New<DummyBusinessObjectEnsureCurrentThreadIsOwnerWhenGetDescription>();
			Factory.Save();
			var dataRefreshBusUpdateActionDecider = ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>();
			var publisherPK = subscriber.PK;

			subscriber.Z0_Bool = true;

			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factoryInThread = new BusinessObjectFactory() { NameForDebugging = "Thread Factory" };
					var publisherInThread = factoryInThread.Load<DummyBusinessObject>(publisherPK);

					AssertEquals("Subscriber factory was not created in the current thread", false, subscriber.Factory.ThreadSentry.IsOwner);

					dataRefreshBusUpdateActionDecider.ShouldApplyDataRefreshBusUpdate(
						DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated,
						subscriber,
						publisherInThread,
						new[] { subscriber.Z0_BoolInfo });
				}
			});

			thread.Start();
			thread.Join(2000);

			AssertNull("No exceptions should be reported", ErrorReporter.LastExceptionReported);
		}

		sealed class DummyBusinessObjectEnsureCurrentThreadIsOwnerWhenGetDescription : DummyBusinessObject
		{
			public DummyBusinessObjectEnsureCurrentThreadIsOwnerWhenGetDescription(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString Z0_Description
			{
				get
				{
					Factory.ThreadSentry.EnsureCurrentThreadIsOwner();
					return base.Z0_Description;
				}
				set => base.Z0_Description = value;
			}
		}

		public void TestShouldApplyDataRefreshBusUpdateDoesNotDependOnBizoHasChanges()
		{
			var testBizos = SetupBizos();

			AssertDataRefreshBusActionDeciderResults(testBizos.subscriber, testBizos.nonSubscriber, testBizos.publisher, DataRefreshAction.None, true);

			testBizos.subscriber.HasChanges = true;

			//updating critical property and no real data row change on subscriber
			testBizos.publisher.Z0_FK_Code = "456";
			AssertDataRefreshBusActionDeciderResults(testBizos.subscriber, testBizos.nonSubscriber, testBizos.publisher, DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, true);

			//making subscriber data row change
			testBizos.subscriber.Z0_Description = "Another description";
			AssertDataRefreshBusActionDeciderResults(testBizos.subscriber, testBizos.nonSubscriber, testBizos.publisher, DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, false);
		}

		public void TestShouldApplyDataRefreshBusUpdate_ForNotImplementedAction()
		{
			var subscriber = Factory.New<DummyBusinessObject>();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var publisher = newFactory.Load<DummyBusinessObject>(subscriber.PK);
			Assert(ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate((DataRefreshAction)int.MaxValue, subscriber, publisher, new[] { subscriber.Z0_FK_CodeInfo }));
			AssertEquals("Unexpected DataRefreshAction value 2147483647. Please implemented processing logic for it here.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestHasSkippedDataRefreshBusUpdate()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			Assert(!dummyBizObj.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
			Assert(!ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().HasSkippedDataRefreshBusUpdate(dummyBizObj));

			using (new DisposableAction(() => dummyBizObj.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange), () => dummyBizObj.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange)))
			{
				Assert(dummyBizObj.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
				Assert(ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().HasSkippedDataRefreshBusUpdate(dummyBizObj));
			}
		}

		public void TestValidateDataRefreshBusChanges()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			Assert(!dummyBizObj.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
			AssertRowError(dummyBizObj, false);

			using (new DisposableAction(() => dummyBizObj.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange), () => dummyBizObj.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange)))
			{
				Assert(dummyBizObj.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
				AssertRowError(dummyBizObj, true);
			}
		}

		(DummyBusinessObject subscriber, DummyBusinessObject nonSubscriber, DummyBusinessObject publisher) SetupBizos()
		{
			var subscriber = Factory.New<DummyBusinessObject>();
			var nonSubscriber = Factory.New<DummyBusinessObject>();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var publisher = newFactory.Load<DummyBusinessObject>(subscriber.PK);
			newFactory.Save();

			return (subscriber, nonSubscriber, publisher);
		}

		public void TestShouldApplyDataRefreshBusUpdate_AsRequiredBySituation()
		{
			var testBizos = SetupBizos();

			AssertDataRefreshBusActionDeciderResults(testBizos.subscriber, testBizos.nonSubscriber, testBizos.publisher, DataRefreshAction.None, expectedResult: true);

			//updating non critical property
			testBizos.publisher.Z0_Description = "Publisher description";
			AssertDataRefreshBusActionDeciderResults(testBizos.subscriber, testBizos.nonSubscriber, testBizos.publisher, DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, expectedResult: true);

			testBizos.subscriber.Z0_Description = "Subscriber description";
			AssertDataRefreshBusActionDeciderResults(testBizos.subscriber, testBizos.nonSubscriber, testBizos.publisher, DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, expectedResult: true);

			//updating critical property
			testBizos.publisher.Z0_FK_Code = "456";
			AssertDataRefreshBusActionDeciderResults(testBizos.subscriber, testBizos.nonSubscriber, testBizos.publisher, DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, expectedResult: false);

			testBizos = SetupBizos();
			testBizos.publisher.Delete();

			//updating non critical property
			testBizos.subscriber.Z0_Description = "Subscriber description";
			AssertDataRefreshBusActionDeciderResults(testBizos.subscriber, testBizos.nonSubscriber, testBizos.publisher, DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherDeleted, expectedResult: false);

			testBizos = SetupBizos();

			testBizos.subscriber.Delete();

			//updating non critical property
			testBizos.publisher.Z0_Description = "Publisher description";
			AssertDataRefreshBusActionDeciderResults(testBizos.subscriber, testBizos.nonSubscriber, testBizos.publisher, DataRefreshAction.UpdateDeletedSubscriberWhenPublisherUpdated, expectedResult: true);
		}

		static void AssertDataRefreshBusActionDeciderResults(DummyBusinessObject subscriber, DummyBusinessObject nonSubscriber, DummyBusinessObject publisher, DataRefreshAction action, bool expectedResult)
		{
			Assert(!subscriber.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
			Assert(!nonSubscriber.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));

			AssertEquals(expectedResult, ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate(action, subscriber, publisher, new[] { subscriber.Z0_FK_CodeInfo }));
			AssertSubscriberBusinessContext(action, subscriber, nonSubscriber, expectedResult);
		}

		static void AssertSubscriberBusinessContext(DataRefreshAction action, DummyBusinessObject subscriber, DummyBusinessObject nonSubscriber, bool expectedDataRefreshBusUpdateActionDeciderResult)
		{
			switch (action)
			{
				case DataRefreshAction.None:
					Assert(!subscriber.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
					Assert(!nonSubscriber.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
					break;
				case DataRefreshAction.UpdateDeletedSubscriberWhenPublisherDeleted:
				case DataRefreshAction.UpdateDeletedSubscriberWhenPublisherUpdated:
				case DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherDeleted:
				case DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated:
					if (expectedDataRefreshBusUpdateActionDeciderResult)
					{
						Assert(!subscriber.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
						Assert(!nonSubscriber.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
					}
					else
					{
						Assert(subscriber.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
						Assert(!nonSubscriber.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
						subscriber.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange);
					}
					break;
				default:
					Fail("Unexpected DataRefreshAction value");
					break;
			}
		}

		static void AssertRowError(DummyBusinessObject dummyBizObj, bool isRowErrorExpected)
		{
			var expectedErrorMessage = "This record was modified by this user during another operation. Please cancel your changes and reload the form.";
			dummyBizObj.RemoveRowError(expectedErrorMessage);
			AssertNoRowError(dummyBizObj, expectedErrorMessage);
			ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ValidateDataRefreshBusChanges(dummyBizObj);
			if (isRowErrorExpected)
			{
				AssertHasRowError(dummyBizObj, expectedErrorMessage);
			}
			else
			{
				AssertNoRowError(dummyBizObj, expectedErrorMessage);
			}
		}

		class JobChargeForTest : JobCharge
		{
			public JobChargeForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{ }

			public ZPropertyInfo[] GetStrictProperties() => GetPropertiesWithStrictConcurrency();

			public override ZDecimal JR_OSCostGSTAmt_Calc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public override bool CanReautorate(CostSell costOrSell, params ZString[] adapterIDs)
			{
				throw new NotImplementedException();
			}

			public override ILocation CostPlaceOfSupplyLocation => null;

			public override ILocation SellPlaceOfSupplyLocation => null;
		}

		public void TestIsStrictPropertyChangedOnSubscriberOrPublisher_ReturnsTrue_WhenStrictProperty_UnchangedInSubscriberButChangedInPublisher()
		{
			AssertIsStrictPropertyChangedOnSubscriberOrPublisher(
				(bizoInPublisherFactory, _) =>
				{
					bizoInPublisherFactory.Z0_FK_Code = "NEW";
				},
				(bizoInPublisherFactory, bizoInSubscriberFactory) =>
				{
					AssertEquals("Pre-condition", false, bizoInSubscriberFactory.Z0_FK_CodeInfo.HasChanges);
					AssertEquals("Pre-condition", true, bizoInPublisherFactory.Z0_FK_CodeInfo.HasChanges);
				},
				expectedResult: true);
		}

		public void TestIsStrictPropertyChangedOnSubscriberOrPublisher_ReturnsFalse_WhenStrictProperty_UnchangedInSubscriberAndUnchangedInPublisher()
		{
			AssertIsStrictPropertyChangedOnSubscriberOrPublisher(
				(_, _) => { },
				(bizoInPublisherFactory, bizoInSubscriberFactory) =>
				{
					AssertEquals("Pre-condition", false, bizoInSubscriberFactory.Z0_FK_CodeInfo.HasChanges);
					AssertEquals("Pre-condition", false, bizoInPublisherFactory.Z0_FK_CodeInfo.HasChanges);
				},
				expectedResult: false);
		}

		public void TestIsStrictPropertyChangedOnSubscriberOrPublisher_ReturnsTrue_WhenStrictProperty_ChangedInSubscriberAndChangedInPublisher()
		{
			AssertIsStrictPropertyChangedOnSubscriberOrPublisher(
				(bizoInPublisherFactory, bizoInSubscriberFactory) =>
				{
					bizoInSubscriberFactory.Z0_FK_Code = "NEW";
					bizoInPublisherFactory.Z0_FK_Code = "NEW";
				},
				(bizoInPublisherFactory, bizoInSubscriberFactory) =>
				{
					AssertEquals("Pre-condition", true, bizoInSubscriberFactory.Z0_FK_CodeInfo.HasChanges);
					AssertEquals("Pre-condition", true, bizoInPublisherFactory.Z0_FK_CodeInfo.HasChanges);
				},
				expectedResult: true);
		}

		public void TestIsStrictPropertyChangedOnSubscriberOrPublisher_ReturnsTrue_WhenStrictProperty_ChangedInSubscriberButUnchangedInPublisher()
		{
			AssertIsStrictPropertyChangedOnSubscriberOrPublisher(
				(_, bizoInSubscriberFactory) => bizoInSubscriberFactory.Z0_FK_Code = "NEW",
				(bizoInPublisherFactory, bizoInSubscriberFactory) =>
				{
					AssertEquals("Pre-condition", true, bizoInSubscriberFactory.Z0_FK_CodeInfo.HasChanges);
					AssertEquals("Pre-condition", false, bizoInPublisherFactory.Z0_FK_CodeInfo.HasChanges);
				},
				expectedResult: true);
		}

		void AssertIsStrictPropertyChangedOnSubscriberOrPublisher(Action<DummyBusinessObject, DummyBusinessObject> setup, Action<DummyBusinessObject, DummyBusinessObject> preConditionAssert, bool expectedResult)
		{
			var subscriberFactory = new BusinessObjectFactory();
			var bizoInSubscriberFactory = subscriberFactory.New<DummyBusinessObject>();
			bizoInSubscriberFactory.Z0_FK_Code = "OLD";
			subscriberFactory.Save();

			var publisherFactory = new BusinessObjectFactory();
			var bizoInPublisherFactory = publisherFactory.Load<DummyBusinessObject>(bizoInSubscriberFactory.PK);

			setup(bizoInPublisherFactory, bizoInSubscriberFactory);

			preConditionAssert(bizoInPublisherFactory, bizoInSubscriberFactory);

			AssertEquals(expectedResult, ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().IsStrictPropertyChangedOnSubscriberOrPublisher(bizoInPublisherFactory, new[] { bizoInSubscriberFactory.Z0_FK_CodeInfo }));
		}
	}
}
