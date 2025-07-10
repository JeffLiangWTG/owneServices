using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVOriginLoadListConsolAllocator))]
	sealed class HVLVOriginLoadListConsolAllocatorTest : TestCaseWithFactory
	{
		public void TestLoadAllocatorFromObjectFactory()
		{
			var logger = new DummyLogger();

			var allocator = ObjectFactory.Get<IHVLVOriginLoadListConsolAllocator>(nameof(IHVLVOriginLoadListConsolAllocator), logger);

			AssertType<HVLVOriginLoadListConsolAllocator>("HVLVOriginLoadListConsolAllocator shoule be registered in Object Factory", allocator);
		}

		public void TestLoadProperAllocatorByRegistrySetting()
		{
			TestCase_TestLoadProperAllocatorByRegistrySetting
				(registryValue: true,
				xusAllocatorShouldBeLoaded: true,
				legacyAllocatorShouldBeLoaded: false);
		}

		public void TestLoadProperAllocatorByRegistrySetting_Legacy()
		{
			TestCase_TestLoadProperAllocatorByRegistrySetting
				(registryValue: false,
				xusAllocatorShouldBeLoaded: false,
				legacyAllocatorShouldBeLoaded: true);
		}

		void TestCase_TestLoadProperAllocatorByRegistrySetting(bool registryValue, bool xusAllocatorShouldBeLoaded, bool legacyAllocatorShouldBeLoaded)
		{
			var logger = new DummyLogger();
			var allocator = new HVLVOriginLoadListConsolAllocator(logger);

			using (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var loadList = GetLoadList("11223344");
				var succeed = allocator.TryCreateConsolAndAttachLoadLists(new[] { loadList }, out _);

				AssertNotNull("Precondition: processing succeed", succeed);
				AssertEquals("XUS Allocator", xusAllocatorShouldBeLoaded, GetAllocatorByReflection("consolAllocator", allocator) is HVLVOriginLoadListConsolXUSAllocator);
				AssertEquals("Legacy Allocator", legacyAllocatorShouldBeLoaded, GetAllocatorByReflection("consolAllocatorLegacy", allocator) is HVLVOriginLoadListHelper);
			}
		}

		object GetAllocatorByReflection(string allocatorFieldName, HVLVOriginLoadListConsolAllocator managingAllocator)
		{
			var allocatorFieldInfo = typeof(HVLVOriginLoadListConsolAllocator).GetField(allocatorFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

			return allocatorFieldInfo.GetValue(managingAllocator);
		}

		public void TestTryCreateConsolAndAttachLoadLists_ShouldFallbackToLegacyWhenXUSProcessingFails()
		{
			var logger = new DummyLogger();
			var allocator = new HVLVOriginLoadListConsolAllocator(logger);

			const string masterBillNumberExceedsLength11AndWillCausingXUSProcessingFailure = "112233445566";
			var loadList = GetLoadList(masterBillNumberExceedsLength11AndWillCausingXUSProcessingFailure);

			Factory.Save();

			using (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var succeed = allocator.TryCreateConsolAndAttachLoadLists(new[] { loadList }, out _);

				Assert("Precondition: the processing should succeed", succeed);

				CombineAssertions("Both allocator should be used, and xus processing failure reason should be reported", () =>
				{
					AssertNotNull("XUS Allocator", GetAllocatorByReflection("consolAllocator", allocator) as HVLVOriginLoadListConsolXUSAllocator);
					AssertNotNull("Legacy Allocator", GetAllocatorByReflection("consolAllocatorLegacy", allocator) as HVLVOriginLoadListHelper);
					AssertEquals("Error is reported", 1, ErrorReporter.TotalErrorCount);
					AssertEquals("Error detected while trying to use Universal XML for load list processing, completed using direct data access instead.", ErrorReporter.LastMessageReported);
				});
			}

			ErrorReporter.Clear();
		}

		HVLVOriginLoadList GetLoadList(string masterBillNumber)
		{
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_TransportMode = TransportModes.Air;
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var bookingHeader = GetBookingHeader();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = originLoadList.PK;

			return originLoadList;
		}

		HVLVBookingHeader GetBookingHeader()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_IsBookingConfirmed = true;
			bookingHeader.HVH_IsBookingReceived = true;

			return bookingHeader;
		}
	}
}
