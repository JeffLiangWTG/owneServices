using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProductionRules.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Core;
using WTG.ProductionRules.TestFramework;

namespace Enterprise.ProductionRules.GUI.Testing
{
	class UserHaltableProductionRulesEngineServiceTest : TestCaseWithFactory
	{
		public void TestObjectFactoryConfiguration()
		{
			var instance1 = ObjectFactory.Get<IUserHaltableProductionRulesEngineService>();
			AssertType<UserHaltableProductionRulesEngineService>(instance1);

			var instance2 = ObjectFactory.Get<IUserHaltableProductionRulesEngineService>();
			AssertEquals("Should be a singleton.", true, ReferenceEquals(instance1, instance2));
		}

		public void TestRunRulesEngine_Factory_Null_Throws()
		{
			var service = new UserHaltableProductionRulesEngineService();
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(null, new NotificationBuffer(), RulesContextType.DummyForTesting, () => Enumerable.Empty<IInputFact>(), r => null));
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(null, new NotificationBuffer(), RulesContextType.DummyForTesting, ProductionRuleSetFilter.Empty, () => Enumerable.Empty<IInputFact>(), r => null));
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(null, new NotificationBuffer(), RulesContextType.DummyForTesting, ProductionRuleSetFilter.Empty, () => Enumerable.Empty<IInputFact[]>(), r => null));
		}

		public void TestRunRulesEngine_GetFacts_Null_Throws()
		{
			var service = new UserHaltableProductionRulesEngineService();
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(Factory, new NotificationBuffer(), RulesContextType.DummyForTesting, null, r => null));
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(Factory, new NotificationBuffer(), RulesContextType.DummyForTesting, ProductionRuleSetFilter.Empty, (Func<IEnumerable<IInputFact>>)null, r => null));
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(Factory, new NotificationBuffer(), RulesContextType.DummyForTesting, ProductionRuleSetFilter.Empty, (Func<IEnumerable<IEnumerable<IInputFact>>>)null, r => null));
		}

		public void TestRunRulesEngine_Filters_Null_Throws()
		{
			var service = new UserHaltableProductionRulesEngineService();
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(Factory, new NotificationBuffer(), RulesContextType.DummyForTesting, null, () => Enumerable.Empty<IInputFact>(), null));
		}

		public void TestRunRulesEngine_ProcessResults_Null_Throws()
		{
			var service = new UserHaltableProductionRulesEngineService();
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(null, new NotificationBuffer(), RulesContextType.DummyForTesting, () => Enumerable.Empty<IInputFact>(), null));
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(null, new NotificationBuffer(), RulesContextType.DummyForTesting, ProductionRuleSetFilter.Empty, () => Enumerable.Empty<IInputFact>(), null));
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(null, new NotificationBuffer(), RulesContextType.DummyForTesting, ProductionRuleSetFilter.Empty, () => Enumerable.Empty<IInputFact[]>(), null));
		}

		public void TestRunRulesEngine_GetFacts_Result_Null_Throws()
		{
			var service = new UserHaltableProductionRulesEngineService();
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(Factory, new NotificationBuffer(), RulesContextType.DummyForTesting, () => null, r => null));
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(Factory, new NotificationBuffer(), RulesContextType.DummyForTesting, ProductionRuleSetFilter.Empty, () => (IEnumerable<IInputFact>)null, r => null));
			AssertExceptionThrown<ArgumentNullException>(() => service.RunRulesEngine(Factory, new NotificationBuffer(), RulesContextType.DummyForTesting, ProductionRuleSetFilter.Empty, () => (IEnumerable<IEnumerable<IInputFact>>)null, r => null));
		}

		public void TestRunRulesEngine()
		{
			var facts = new[] { new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = true } };
			var result = new ProductionRulesEngineResult(facts);

			var hitGetFacts = false;
			IEnumerable<IInputFact> getFacts()
			{
				var progressBarForm = WaitForProgressFormToOpen();
				AssertNotNull("Should have shown progress form.", progressBarForm);
				AssertProgressBarForm(progressBarForm, "Loading facts...", 0);
				AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
				AssertEquals("Should have no cancel button.", false, progressBarForm.ShowCancelButton);
				hitGetFacts = true;
				return facts;
			}

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);
			mock.Setup(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == null), It.Is<IEnumerable<IEnumerable<IInputFact>>>(f => CheckFactCollection(f, facts)), null, It.IsAny<CancellationToken>()))
				.Returns(() =>
				{
					var progressBarForm = WaitForProgressFormToOpen();
					AssertNotNull("Should have shown progress form.", progressBarForm);
					AssertProgressBarForm(progressBarForm, "Running rules...", 30);
					AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
					AssertEquals("Should have a cancel button.", true, progressBarForm.ShowCancelButton);

					return result;
				});

			var hitProcessResults = false;
			INotification processResults(ProductionRulesEngineResult returnedResult)
			{
				var progressBarForm = WaitForProgressFormToOpen();
				AssertNotNull("Should have shown progress form.", progressBarForm);
				AssertProgressBarForm(progressBarForm, "Processing results...", 75);
				AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
				AssertEquals("Should have no cancel button.", false, progressBarForm.ShowCancelButton);

				AssertEquals("Should have returned the result.", result, returnedResult);
				hitProcessResults = true;

				return null;
			}

			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notifications = new NotificationBuffer();
				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, getFacts, processResults);
				AssertEquals(false, notifications.Events.Any());
			}

			Thread.Sleep(1000); // Just in case form is slow to close
			Application.DoEvents();
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());

			AssertEquals("Should have hit getFacts.", true, hitGetFacts);
			AssertEquals("Should have hit processResults.", true, hitProcessResults);
			mock.VerifyAll();
		}

		public void TestRunRulesEngine_WithBatches()
		{
			var facts1 = new[] { new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = true } };
			var facts2 = new[] { new EmployeeFactDummy { HourlyRate = 10.00m, IsActive = true } };

			var facts = new[] { facts1, facts2 };
			var result = new ProductionRulesEngineResult(new[] { facts1[0], facts2[0] });

			var hitGetFacts = false;
			IEnumerable<IEnumerable<IInputFact>> getFactBatches()
			{
				var progressBarForm = WaitForProgressFormToOpen();
				AssertNotNull("Should have shown progress form.", progressBarForm);
				AssertProgressBarForm(progressBarForm, "Loading facts...", 0);
				AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
				AssertEquals("Should have no cancel button.", false, progressBarForm.ShowCancelButton);
				hitGetFacts = true;
				return facts;
			}

			var hitCount = 0;
			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);
			mock.Setup(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == null), It.Is<IEnumerable<IEnumerable<IInputFact>>>(f => CheckFactCollection(f, facts.SelectMany(f1 => f1).ToArray())), null, It.IsAny<CancellationToken>()))
				.Returns(() =>
				{
					var progressBarForm = WaitForProgressFormToOpen();
					AssertNotNull("Should have shown progress form.", progressBarForm);
					AssertProgressBarForm(progressBarForm, "Running rules...", 30);
					AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
					AssertEquals("Should have a cancel button.", true, progressBarForm.ShowCancelButton);

					hitCount++;
					return result;
				});

			var hitProcessResultsCount = 0;
			INotification processResults(ProductionRulesEngineResult returnedResult)
			{
				var progressBarForm = WaitForProgressFormToOpen();
				AssertNotNull("Should have shown progress form.", progressBarForm);
				AssertProgressBarForm(progressBarForm, "Processing results...", 75);
				AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
				AssertEquals("Should have no cancel button.", false, progressBarForm.ShowCancelButton);

				AssertEquals("Should have returned the result.", result, returnedResult);
				hitProcessResultsCount++;

				return null;
			}

			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notifications = new NotificationBuffer();
				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, ProductionRuleSetFilter.Empty, getFactBatches, processResults);
				AssertEquals(false, notifications.Events.Any());
			}

			Thread.Sleep(1000); // Just in case form is slow to close
			Application.DoEvents();
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());

			AssertEquals("Should have hit getFacts.", true, hitGetFacts);
			AssertEquals("Should have hit the rules engine.", 1, hitCount);
			AssertEquals("Should have hit processResults.", 1, hitProcessResultsCount);
			mock.VerifyAll();
		}

		public void TestRunRulesEngine_WithFilters()
		{
			var filter = ProductionRuleSetFilter.WithWarehouse(Guid.NewGuid());
			var facts = new[] { new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = true } };
			var result = new ProductionRulesEngineResult(facts);

			var hitGetFacts = false;
			IEnumerable<IInputFact> getFacts()
			{
				var progressBarForm = WaitForProgressFormToOpen();
				AssertNotNull("Should have shown progress form.", progressBarForm);
				AssertProgressBarForm(progressBarForm, "Loading facts...", 0);
				AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
				AssertEquals("Should have no cancel button.", false, progressBarForm.ShowCancelButton);
				hitGetFacts = true;
				return facts;
			}

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);
			mock.Setup(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, filter, It.Is<IEnumerable<IEnumerable<IInputFact>>>(f => CheckFactCollection(f, facts)), null, It.IsAny<CancellationToken>()))
				.Returns(() =>
				{
					Thread.Sleep(500);
					Application.DoEvents();

					var progressBarForm = Application.OpenForms.OfType<ProgressForm>().SingleOrDefault();
					AssertNotNull("Should have shown progress form.", progressBarForm);
					AssertProgressBarForm(progressBarForm, "Running rules...", 30);
					AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
					AssertEquals("Should have a cancel button.", true, progressBarForm.ShowCancelButton);

					return result;
				});

			var hitProcessResults = false;
			INotification processResults(ProductionRulesEngineResult returnedResult)
			{
				var progressBarForm = WaitForProgressFormToOpen();
				AssertNotNull("Should have shown progress form.", progressBarForm);
				AssertProgressBarForm(progressBarForm, "Processing results...", 75);
				AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
				AssertEquals("Should have no cancel button.", false, progressBarForm.ShowCancelButton);

				AssertEquals("Should have returned the result.", result, returnedResult);
				hitProcessResults = true;

				return null;
			}

			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notifications = new NotificationBuffer();
				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, filter, getFacts, processResults);
				AssertEquals(false, notifications.Events.Any());
			}

			Thread.Sleep(1000); // Just in case form is slow to close
			Application.DoEvents();
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());

			AssertEquals("Should have hit getFacts.", true, hitGetFacts);
			AssertEquals("Should have hit processResults.", true, hitProcessResults);
			mock.VerifyAll();
		}

		public void TestRunRulesEngine_FactLoadException()
		{
			var facts = new[] { new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = true } };
			var result = new ProductionRulesEngineResult(facts);

			var hitGetFacts = false;
			IEnumerable<IInputFact> getFacts()
			{
				var progressBarForm = WaitForProgressFormToOpen();
				AssertNotNull("Should have shown progress form.", progressBarForm);
				AssertProgressBarForm(progressBarForm, "Loading facts...", 0);
				AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
				AssertEquals("Should have no cancel button.", false, progressBarForm.ShowCancelButton);
				hitGetFacts = true;

				throw new FactLoadingException("No data to provide!");
			}

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);

			var hitProcessResults = false;
			INotification processResults(ProductionRulesEngineResult returnedResult)
			{
				hitProcessResults = true;
				return null;
			}

			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notifications = new NotificationBuffer();
				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, getFacts, processResults);
				AssertEquals(true, notifications.HasErrors);
				AssertEquals("Error occurred while loading data: No data to provide!", notifications.AsString.Trim());
			}

			Thread.Sleep(1000); // Just in case form is slow to close
			Application.DoEvents();
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());

			AssertEquals("Should have hit getFacts.", true, hitGetFacts);
			AssertEquals("Should *not* have hit processResults.", false, hitProcessResults);
			mock.Verify(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == null), new[] { facts }, null, It.IsAny<CancellationToken>()), Times.Never);
		}

		public void TestRunRulesEngine_Error()
		{
			var facts = new[] { new EmployeeFactDummy() };
			var result = new ProductionRulesEngineResult("ERROR");

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);
			mock.Setup(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == null), It.Is<IEnumerable<IEnumerable<IInputFact>>>(f => CheckFactCollection(f, facts)), null, It.IsAny<CancellationToken>()))
				.Returns(() =>
				{
					var progressBarForm = WaitForProgressFormToOpen();
					AssertNotNull("Should have shown progress form.", progressBarForm);
					AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
					AssertEquals("Should have a cancel button.", true, progressBarForm.ShowCancelButton);

					return result;
				});

			var hitResultProcessor = false;
			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var service = new UserHaltableProductionRulesEngineService();

				var notifications = new NotificationBuffer();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, () => facts,
					r =>
					{
						hitResultProcessor = true;
						return null;
					});

				AssertEquals(true, notifications.HasErrors);
				AssertEquals("ERROR", notifications.AsString.Trim());
			}

			AssertEquals("Should not have hit results processor.", false, hitResultProcessor);

			Thread.Sleep(1000); // Just in case form is slow to close
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());

			mock.VerifyAll();
		}

		public void TestRunRulesEngine_Notification()
		{
			var facts = new[] { new EmployeeFactDummy() };
			var result = new ProductionRulesEngineResult(facts);

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);
			mock.Setup(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == null), It.Is<IEnumerable<IEnumerable<IInputFact>>>(f => CheckFactCollection(f, facts)), null, It.IsAny<CancellationToken>()))
				.Returns(() =>
				{
					var progressBarForm = WaitForProgressFormToOpen();
					AssertNotNull("Should have shown progress form.", progressBarForm);
					AssertEquals("Should have a progress bar.", true, progressBarForm.ShowProgressBar);
					AssertEquals("Should have a cancel button.", true, progressBarForm.ShowCancelButton);

					return result;
				});

			var hitResultProcessor = false;
			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notification = Mock.Of<INotification>();
				var notificationBuffer = new NotificationBuffer();

				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notificationBuffer, RulesContextType.DummyForTesting, () => facts,
						r =>
						{
							hitResultProcessor = true;
							return notification;
						});

				AssertEquals(notification, notificationBuffer.Events.Single());
			}

			AssertEquals("Should have hit results processor.", true, hitResultProcessor);

			Thread.Sleep(1000); // Just in case form is slow to close
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());

			mock.VerifyAll();
		}

		public void TestRunRulesEngine_NoFacts()
		{
			var facts = Enumerable.Empty<IInputFact>();
			var result = new ProductionRulesEngineResult("ERROR");

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict); // Should not get hit

			var hitResultProcessor = false;
			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notifications = new NotificationBuffer();

				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, Enumerable.Empty<IInputFact>,
					r =>
					{
						hitResultProcessor = true;
						return null;
					});
				AssertEquals(false, notifications.Events.Any());
			}

			Thread.Sleep(1000); // Just in case form is slow to close/dispose
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
			AssertEquals("Should not have hit results processor.", false, hitResultProcessor);
			mock.VerifyAll();
		}

		[SnailTest]
		public void TestRunRulesEngine_Cancel_HaltsExecution()
		{
			var facts = new[] { new EmployeeFactDummy() };
#if !WINZOR
			CancellationToken capturedToken = default;
#else
			var source = new CancellationTokenSource();
			CancellationToken capturedToken = source.Token;
#endif

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);
			mock.Setup(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == null), It.Is<IEnumerable<IEnumerable<IInputFact>>>(f => CheckFactCollection(f, facts)), null, It.IsAny<CancellationToken>()))
				.Callback<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IEnumerable<IInputFact>>, DateTime?, CancellationToken>((c, s, f, fs, dt, ct) => capturedToken = ct)
				.Returns(() =>
				{
					var form = WaitForProgressFormToOpen();
					AssertNotNull("Should have shown progress form.", form);
					AssertEquals("Should have shown progress form.", true, form.Enabled);
					AssertEquals("Should have a progress bar.", true, form.ShowProgressBar);
					AssertEquals("Should have a cancel button.", true, form.ShowCancelButton);

					var button = (Button)form.CancelButton;
					AssertEquals("Precondition.", false, capturedToken.IsCancellationRequested);
					button.Invoke(new Action(button.PerformClick));
					AssertEquals("Precondition.", true, capturedToken.IsCancellationRequested);

					return ProductionRulesEngineResult.Halted;
				});

			var hitResultProcessor = false;
			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notifications = new NotificationBuffer();
				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, () => facts,
					r =>
					{
						hitResultProcessor = true;
						return null;
					});
				AssertEquals(false, notifications.Events.Any());
			}

			Thread.Sleep(1000); // Just in case form is slow to close/dispose
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
			AssertEquals("Should not have hit results processor.", false, hitResultProcessor);
			mock.VerifyAll();
		}

		public void TestRunRulesEngine_NotUserInteractive()
		{
			var facts = new[] { new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = true } };
			var result = new ProductionRulesEngineResult(facts);

			var hitGetFacts = false;
			IEnumerable<IInputFact> getFacts()
			{
				Thread.Sleep(1000);
				Application.DoEvents();
				AssertNull("Should *not* have shown progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
				hitGetFacts = true;
				return facts;
			}

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);
			mock.Setup(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == null), It.Is<IEnumerable<IEnumerable<IInputFact>>>(f => CheckFactCollection(f, facts)), null, It.IsAny<CancellationToken>()))
				.Returns(() =>
				{
					Thread.Sleep(1000);
					Application.DoEvents();
					AssertNull("Should *not* have shown progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
					return result;
				});

			var hitProcessResults = false;
			INotification processResults(ProductionRulesEngineResult returnedResult)
			{
				AssertEquals("Should have returned the result.", result, returnedResult);

				Thread.Sleep(1000);
				Application.DoEvents();
				AssertNull("Should *not* have shown progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());

				hitProcessResults = true;
				return null;
			}

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notifications = new NotificationBuffer();
				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, getFacts, processResults);
				AssertEquals(false, notifications.Events.Any());
			}

			AssertEquals("Should have hit getFacts.", true, hitGetFacts);
			AssertEquals("Should have hit processResults.", true, hitProcessResults);
			mock.VerifyAll();
		}

		public void TestRunRulesEngine_NotUserInteractive_FactLoadException()
		{
			var facts = new[] { new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = true } };
			var result = new ProductionRulesEngineResult(facts);

			var hitGetFacts = false;
			IEnumerable<IInputFact> getFacts()
			{
				Thread.Sleep(1000);
				Application.DoEvents();
				AssertNull("Should *not* have shown progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
				hitGetFacts = true;

				throw new FactLoadingException("No data to provide!");
			}

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);

			var hitProcessResults = false;
			INotification processResults(ProductionRulesEngineResult returnedResult)
			{
				hitProcessResults = true;
				return null;
			}

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notifications = new NotificationBuffer();
				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, getFacts, processResults);
				AssertEquals(true, notifications.HasErrors);
				AssertEquals("Error occurred while loading data: No data to provide!", notifications.AsString.Trim());
			}

			AssertEquals("Should have hit getFacts.", true, hitGetFacts);
			AssertEquals("Should *not* have hit processResults.", false, hitProcessResults);
			mock.VerifyNoOtherCalls();
		}

		public void TestRunRulesEngine_NotUserInteractive_WithFilter()
		{
			var filter = ProductionRuleSetFilter.WithWarehouse(Guid.NewGuid());
			var facts = new[] { new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = true } };
			var result = new ProductionRulesEngineResult(facts);

			var hitGetFacts = false;
			IEnumerable<IInputFact> getFacts()
			{
				Thread.Sleep(1000);
				Application.DoEvents();
				AssertNull("Should *not* have shown progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
				hitGetFacts = true;
				return facts;
			}

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);
			mock.Setup(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, filter, It.Is<IEnumerable<IEnumerable<IInputFact>>>(f => CheckFactCollection(f, facts)), null, It.IsAny<CancellationToken>())).Returns(() =>
				{
					Thread.Sleep(1000);
					Application.DoEvents();
					AssertNull("Should *not* have shown progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
					return result;
				});

			var hitProcessResults = false;
			INotification processResults(ProductionRulesEngineResult returnedResult)
			{
				AssertEquals("Should have returned the result.", result, returnedResult);

				Thread.Sleep(1000);
				Application.DoEvents();
				AssertNull("Should *not* have shown progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());

				hitProcessResults = true;
				return null;
			}

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notifications = new NotificationBuffer();
				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, filter, getFacts, processResults);
				AssertEquals(false, notifications.Events.Any());
			}

			AssertEquals("Should have hit getFacts.", true, hitGetFacts);
			AssertEquals("Should have hit processResults.", true, hitProcessResults);
			mock.VerifyAll();
		}

		public void TestRunRulesEngine_NotUserInteractive_Error()
		{
			var facts = new[] { new EmployeeFactDummy() };
			var result = new ProductionRulesEngineResult("ERROR");

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);
			mock.Setup(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == null), It.Is<IEnumerable<IEnumerable<IInputFact>>>(f => CheckFactCollection(f, facts)), null, It.IsAny<CancellationToken>()))
				.Returns(() =>
				{
					Thread.Sleep(1000);
					Application.DoEvents();
					AssertNull("Should not have shown progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
					return result;
				});

			var hitResultProcessor = false;
			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notificationBuffer = new NotificationBuffer();
				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notificationBuffer, RulesContextType.DummyForTesting, () => facts,
					r =>
					{
						hitResultProcessor = true;
						return null;
					});

				AssertEquals(true, notificationBuffer.HasErrors);
				AssertEquals("ERROR", notificationBuffer.AsString.Trim());
			}

			AssertEquals("Should not have hit results processor.", false, hitResultProcessor);

			mock.VerifyAll();
		}

		public void TestRunRulesEngine_NotUserInteractive_Notification()
		{
			var facts = new[] { new EmployeeFactDummy() };
			var result = new ProductionRulesEngineResult(facts);

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict);
			mock.Setup(pre => pre.RunRulesEngine(RulesContextType.DummyForTesting, RulesContextSubType.None, It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == null), It.Is<IEnumerable<IEnumerable<IInputFact>>>(f => CheckFactCollection(f, facts)), null, It.IsAny<CancellationToken>()))
				.Returns(() =>
				{
					Thread.Sleep(1000);
					Application.DoEvents();
					AssertNull("Should not have shown progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
					return result;
				});

			var hitResultProcessor = false;
			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notificationBuffer = new NotificationBuffer();
				var notification = Mock.Of<INotification>();

				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notificationBuffer, RulesContextType.DummyForTesting, () => facts,
						r =>
						{
							hitResultProcessor = true;
							return notification;
						});

				AssertEquals(notification, notificationBuffer.Events.Single());
			}

			AssertEquals("Should have hit results processor.", true, hitResultProcessor);

			Thread.Sleep(1000); // Just in case form is slow to close
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());

			mock.VerifyAll();
		}

		public void TestRunRulesEngine_NotUserInteractive_NoFacts()
		{
			var facts = Enumerable.Empty<IInputFact>();
			var result = new ProductionRulesEngineResult("ERROR");

			var mock = new Mock<IProductionRulesEnginePushService>(MockBehavior.Strict); // Should not get hit

			var hitResultProcessor = false;
			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute((objs) => objs.Single() == Factory ? mock.Object : null))
			{
				var notifications = new NotificationBuffer();

				var service = new UserHaltableProductionRulesEngineService();
				service.RunRulesEngine(Factory, notifications, RulesContextType.DummyForTesting, Enumerable.Empty<IInputFact>,
					r =>
					{
						hitResultProcessor = true;
						return null;
					});
				AssertEquals(false, notifications.Events.Any());
			}

			AssertEquals("Should not have hit results processor.", false, hitResultProcessor);
			mock.VerifyAll();
		}

		ProgressForm WaitForProgressFormToOpen()
		{
			ProgressForm progressForm = null;

			// The form uses a timer with no good methods of synchronisation
			for (int i = 0; i < 50; i++)
			{
				Thread.Sleep(250);
				Application.DoEvents();

				var progressForms = Application.OpenForms.OfType<ProgressForm>().ToArray();
				if (progressForms.Length == 1)
				{
					progressForm = progressForms[0];
					break;
				}
			}

			return progressForm;
		}

		void AssertProgressBarForm(Form form, string text, int value)
		{
			// The form status gets updated on a 250ms tick with no good methods of synchronisation
			for (int i = 0; i < 30; i++)
			{
				Thread.Sleep(100);
				Application.DoEvents();

				if (form.FindSingleOrDefault<ZLabel>(c => c.Name == "ProgressLabel")?.Text == text)
				{
					break;
				}
			}

			var label = form.FindSingleOrDefault<ZLabel>(c => c.Name == "ProgressLabel");
			AssertEquals("Progress bar should have correct caption.", text, label?.Text ?? "No label found.");

			var progressBar = form.FindSingleOrDefault<ProgressBar>(c => c.Name == "ProgressBar");
			AssertEquals("Progress bar should have correct value.", value, progressBar?.Value ?? -1);
		}

		static bool CheckFactCollection(IEnumerable<IEnumerable<IInputFact>> f, EmployeeFactDummy[] facts)
		{
			var flattenedInput = f.SelectMany(f1 => f1).ToArray();
			return facts.Length == flattenedInput.Length && facts.Intersect(flattenedInput).Count() == facts.Length;
		}
	}
}
