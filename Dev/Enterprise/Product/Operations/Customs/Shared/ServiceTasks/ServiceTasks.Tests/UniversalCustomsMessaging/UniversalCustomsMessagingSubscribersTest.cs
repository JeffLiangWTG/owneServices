using System;
using System.Linq;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class UniversalCustomsMessagingSubscribersTestCase : TransactionedTestCase
	{
		public void TestHasUniversalCustomsMessagingSubscribers()
		{
			using (new UCMPProcessorsRegistrationSubstitute())
			{
				AssertEquals("HasUniversalCustomsMessagingSubscribers()", expected: false, actual: UniversalCustomsMessagingSubscribers.HasUniversalCustomsMessagingSubscribers());
			}
		}

		public void TestHasUniversalCustomsMessagingSubscribers_EnableUCMTest()
		{
			using (SetupUCMTestApplicationCodes("_T1"))
			using (EnableUCMTest())
			using (new UCMPProcessorsRegistrationSubstitute())
			{
				AssertEquals("HasUniversalCustomsMessagingSubscribers()", expected: true, actual: UniversalCustomsMessagingSubscribers.HasUniversalCustomsMessagingSubscribers());
			}
		}

		public void TestGetMessageProcessor_EnableUCMTest()
		{
			using (SetupUCMTestApplicationCodes("_T1", "_T2"))
			using (EnableUCMTest())
			using (new UCMPProcessorsRegistrationSubstitute())
			{
				AssertType<T1UniversalCustomsMessageProcessor>("T1UniversalCustomsMessageProcessor for _T1", UniversalCustomsMessagingSubscribers.GetMessageProcessor("_T1"));
				AssertType<T1UniversalCustomsMessageProcessor>("T1UniversalCustomsMessageProcessor for _T2", UniversalCustomsMessagingSubscribers.GetMessageProcessor("_T2"));
			}
		}

		public void TestGetApplicationCodes()
		{
			using (new UCMPProcessorsRegistrationSubstitute(
					("A1$", new UniversalCustomsMessageProcessorTestClassA()),
					("A2$", new UniversalCustomsMessageProcessorTestClassB()),
					("A3$", new UniversalCustomsMessageProcessorTestClassC())))
			{
				AssertContainsExactElementsInAnyOrder("GetApplicationCodes()", new[] { "A1$", "A2$", "A3$" }, UniversalCustomsMessagingSubscribers.GetApplicationCodes());
			}
		}

		public void TestGetMessageProcessor()
		{
			using (new UCMPProcessorsRegistrationSubstitute(
					("A1$", new UniversalCustomsMessageProcessorTestClassA()),
					("A2$", new UniversalCustomsMessageProcessorTestClassB())))
			{
				AssertType<UniversalCustomsMessageProcessorTestClassA>("MessageProcessor A", UniversalCustomsMessagingSubscribers.GetMessageProcessor("A1$"));
				AssertType<UniversalCustomsMessageProcessorTestClassB>("MessageProcessor B", UniversalCustomsMessagingSubscribers.GetMessageProcessor("A2$"));
				AssertNull("Unknown", UniversalCustomsMessagingSubscribers.GetMessageProcessor("A3$"));
			}
		}

		/// <remarks>This test requires up to date <c>AssemblyMetaData.xml</c></remarks>
		public void TestApplicationCodesAreNotEmpty()
		{
			var assemblyAttributesWithEmptyApplicationCode = UniversalCustomsMessagingSubscribers.ConfigurationProvider()
				.Where(x => string.IsNullOrEmpty(x.ApplicationCode))
				.Select(x => $"{x.Creator.Create().GetType().FullName}")
				.ToArray();

			AssertSequencesEqual("Each [assembly:UniversalCustomsMessageProcessor()] attribute instance must specify non-empty ApplicationCode", Array.Empty<string>(), assemblyAttributesWithEmptyApplicationCode);
		}

		/// <remarks>This test requires up to date <c>AssemblyMetaData.xml</c></remarks>
		public void TestApplicationCodesAreUnique()
		{
			var duplicatingApplicationCodes = UniversalCustomsMessagingSubscribers.ConfigurationProvider()
				.GroupBy(
					x => x.ApplicationCode.ToUpperInvariant(),
					x => x.Creator,
					(appCode, registrations) =>
						new
						{
							ApplicationCode = appCode,
							Count = registrations.Count(),
							Creators = registrations.ToArray()
						})
				.Where(g => g.Count > 1)
				.Select(g => $"{g.ApplicationCode} ({string.Join(",", g.Creators.Select(x => x.Create().GetType().FullName))})")
				.ToArray();

			AssertSequencesEqual("Each [assembly:UniversalCustomsMessageProcessor()] instance must specify unique ApplicationCode", Array.Empty<string>(), duplicatingApplicationCodes);
		}

		public void TestDuplicatingApplicationCodesInitializationException()
		{
			const string appCode = "A1$";
			using (new UCMPProcessorsRegistrationSubstitute(
					(appCode, new UniversalCustomsMessageProcessorTestClassA()),
					(appCode, new UniversalCustomsMessageProcessorTestClassB())))
			{
				AssertExceptionThrown<DuplicatingUCMCodeException>(
					"Initialization with duplicating ApplicationCode should throw an exception",
					$"Each [assembly:{typeof(UniversalCustomsMessageProcessorAttribute).FullName}] instance must specify unique ApplicationCode. Found duplicates: {appCode} ({typeof(UniversalCustomsMessageProcessorTestClassA).FullName},{typeof(UniversalCustomsMessageProcessorTestClassB).FullName})",
					() => UniversalCustomsMessagingSubscribers.GetApplicationCodes());
			}
		}

		static IDisposable SetupUCMTestApplicationCodes(params string[] applicationCodes)
		{
			var testTypeCollection = new UCMEDIMessageTestTypeCollection();
			foreach (var appCode in applicationCodes)
			{
				var item = testTypeCollection.AddNew();
				item.ApplicationCode = appCode;
			}

			return CustomsDataRegistry.Instance.UCMTestApplicationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testTypeCollection);
		}

		static IDisposable EnableUCMTest() => CustomsDataRegistry.Instance.EnableUCMTest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

		sealed class UniversalCustomsMessageProcessorTestClassA : UniversalCustomsMessageProcessorTestClass { }

		sealed class UniversalCustomsMessageProcessorTestClassB : UniversalCustomsMessageProcessorTestClass { }

		sealed class UniversalCustomsMessageProcessorTestClassC : UniversalCustomsMessageProcessorTestClass { }
	}
}
