using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RatingAdaptersProviderTest : TestCaseWithFactory
	{
		public class TestUIInteractor : IAutoRatingInteractor
		{
			public readonly List<string> errors = new List<string>();
			public readonly Dictionary<string, bool> answersPrepaired = new Dictionary<string, bool>();
			public readonly List<Tuple<string, bool>> answersProvided = new List<Tuple<string, bool>>();

			bool IAutoRatingInteractor.YesNoWarning(string warningMessage)
			{
				bool result = answersPrepaired[warningMessage];
				answersProvided.Add(new Tuple<string, bool>(warningMessage, result));
				return result;
			}

			void ILogger.Log(LogType type, string message)
			{
				errors.Add(message);
			}

			void ILogger.Log(LogType type, string message, Exception ex)
			{
				errors.Add(message);
				errors.Add(ex.Message);
			}
		}

		public void TestGetAdaptersForPersistent()
		{
			var adapters = new List<IAutoRating> { new Mock<IAutoRating>().Object };

			var persistentBizO = Factory.NewWithValidTestData<PersistentRatingSupporter>();
			persistentBizO.AdaptersGetter = () => adapters;

			var provider = (PersistentRatingSupporterAdaptersProvider)persistentBizO.AdaptersProvider;

			Factory.Save();
			AssertEquals("Provider doesn't currently reload the bizO and doesn't loose access to the fields of initial bizO, but statusInfo is null so 0 adapters is expected", 0, provider.GetAdapters(null, AutoRateOptions.AutorateRevenue).Count);
			Assert(!provider.CanExecuteAutoRating(null, AutoRateOptions.AutorateRevenue));
		}

		public void TestGetAdaptersForNonPersistent()
		{
			var adapter = new Mock<IAutoRating>();

			adapter.Setup(m => m.StatusInformation).Returns(new AutoRatingStatusInfo(true));

			var adapters = new List<IAutoRating> { adapter.Object };

			var nonPersistentBizO = new NonPersistentRatingSupporter();
			nonPersistentBizO.AdaptersGetter = () => adapters;

			var provider = (NonPersistentRatingSupporterAdaptersProvider)nonPersistentBizO.AdaptersProvider;
			Factory.Save();

			AssertEquals("Provider should not reload the NonPersistentBusinessObject and should retain access to the fields of initial bizO", 1, provider.GetAdapters(null, AutoRateOptions.AutorateRevenue).Count);
			Assert(provider.CanExecuteAutoRating(null, AutoRateOptions.AutorateRevenue));
		}

		public void TestStatusCanExecute()
		{
			var dummy = Factory.NewWithValidTestData<PersistentRatingSupporter>();
			Factory.Save();

			var adapter1 = new Mock<IAutoRating>();
			var adapter2 = new Mock<IAutoRating>();
			var adapter3 = new Mock<IAutoRating>();

			dummy.AdaptersGetter = () => new List<IAutoRating> { adapter1.Object, adapter2.Object };
			var provider = (PersistentRatingSupporterAdaptersProvider)dummy.AdaptersProvider;

			adapter1.Setup(m => m.StatusInformation).Returns(new AutoRatingStatusInfo(false, "Some Test Error"));
			adapter2.Setup(m => m.StatusInformation).Returns(new AutoRatingStatusInfo(true));
			adapter3.Setup(m => m.StatusInformation).Returns(new AutoRatingStatusInfo(true, "Some Test Warning"));

			var uiInteractor = new TestUIInteractor { answersPrepaired = { ["Some Test Warning"] = true } };

			using (provider.NewRatingSession())
			{
				AssertEquals(0, provider.GetAdapters(uiInteractor, AutoRateOptions.AutorateRevenue).Count);
				Assert(!provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
				Assert(!provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
				AssertEquals(0, provider.GetAdapters(uiInteractor, AutoRateOptions.AutorateRevenue).Count);
				AssertEquals(0, provider.GetAdapters(uiInteractor, AutoRateOptions.AutorateRevenue).Count);
				Assert(!provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
				Assert(!provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
				AssertEquals("Some Test Error", uiInteractor.errors[0]);
				AssertEquals("UIInteractor should display only 1 error per session", 1, uiInteractor.errors.Count);
				uiInteractor.errors.Clear();
			}

			dummy.AdaptersGetter = () => new List<IAutoRating> { adapter3.Object, adapter2.Object };

			using (provider.NewRatingSession())
			{
				Assert(provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
				AssertEquals(2, provider.GetAdapters(uiInteractor, AutoRateOptions.AutorateRevenue).Count);
				Assert(provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
				AssertEquals(2, provider.GetAdapters(uiInteractor, AutoRateOptions.AutorateRevenue).Count);
				Assert(provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
				AssertEquals(2, provider.GetAdapters(uiInteractor, AutoRateOptions.AutorateRevenue).Count);
				AssertEquals(0, uiInteractor.errors.Count);
				AssertEquals("UIInteractor should display only 1 warning per session", 1, uiInteractor.answersProvided.Count);
			}
		}

		public void TestStatusIsActive()
		{
			var dummy = Factory.NewWithValidTestData<PersistentRatingSupporter>();
			Factory.Save();

			var adapter1 = new Mock<IAutoRating>();
			var adapter2 = new Mock<IAutoRating>();
			var adapter3 = new Mock<IAutoRating>();

			dummy.AdaptersGetter = () => new List<IAutoRating> { adapter1.Object, adapter2.Object, adapter3.Object };

			var provider = (PersistentRatingSupporterAdaptersProvider)dummy.AdaptersProvider;

			var statusInfo1 = new AutoRatingStatusInfo(true);
			var statusInfo2 = new AutoRatingStatusInfo(true);
			var statusInfo3 = new AutoRatingStatusInfo(true, "Some Test Warning");
			statusInfo2.IsActive = false;
			adapter1.Setup(m => m.StatusInformation).Returns(statusInfo1);
			adapter2.Setup(m => m.StatusInformation).Returns(statusInfo2);
			adapter3.Setup(m => m.StatusInformation).Returns(statusInfo3);

			var uiInteractor = new TestUIInteractor { answersPrepaired = { ["Some Test Warning"] = true } };

			AssertEquals(2, provider.GetAdapters(uiInteractor, AutoRateOptions.AutorateRevenue).Count);
			Assert(provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
		}

		public void TestStatusWarning()
		{
			var dummy = Factory.NewWithValidTestData<PersistentRatingSupporter>();
			Factory.Save();

			var adapter1 = new Mock<IAutoRating>();
			var adapter2 = new Mock<IAutoRating>();
			var adapter3 = new Mock<IAutoRating>();

			dummy.AdaptersGetter = () => new List<IAutoRating> { adapter1.Object, adapter2.Object, adapter3.Object };

			var provider = (PersistentRatingSupporterAdaptersProvider)dummy.AdaptersProvider;

			var statusInfo1 = new AutoRatingStatusInfo(true);
			var statusInfo2 = new AutoRatingStatusInfo(true, "Some Test Warning");
			var statusInfo3 = new AutoRatingStatusInfo(true);
			adapter1.Setup(m => m.StatusInformation).Returns(statusInfo1);
			adapter2.Setup(m => m.StatusInformation).Returns(statusInfo2);
			adapter3.Setup(m => m.StatusInformation).Returns(statusInfo3);

			var uiInteractor = new TestUIInteractor { answersPrepaired = { ["Some Test Warning"] = true } };

			using (provider.NewRatingSession())
			{
				AssertEquals(3, provider.GetAdapters(uiInteractor, AutoRateOptions.AutorateRevenue).Count);
				Assert(provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
			}

			uiInteractor.answersPrepaired["Some Test Warning"] = false;

			using (provider.NewRatingSession())
			{
				AssertEquals(0, provider.GetAdapters(uiInteractor, AutoRateOptions.AutorateRevenue).Count);
				Assert(!provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
			}
		}
	}
}
