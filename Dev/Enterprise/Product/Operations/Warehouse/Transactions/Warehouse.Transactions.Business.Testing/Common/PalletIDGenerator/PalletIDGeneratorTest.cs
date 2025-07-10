using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PalletIDGeneratorTest : WhsTestCaseWithFactory
	{
		#region Constructor

		public void TestConstructor_NullSSCCIDGenerator_ShouldError()
		{
			var mockDocketGenerator = new Mock<IPalletIDFromDocketIDGenerator>();
			var ssccPrefixFinder = new Mock<ISSCCPrefixFinder>();
			AssertExceptionThrown<ArgumentNullException>(() => new PalletIDGenerator(null, mockDocketGenerator.Object, ssccPrefixFinder.Object));
		}

		public void TestConstructor_NullDocketIDGenerator_ShouldError()
		{
			var mockSSCCGenerator = new Mock<IPalletIDFromSSCCGenerator>();
			var ssccPrefixFinder = new Mock<ISSCCPrefixFinder>();
			AssertExceptionThrown<ArgumentNullException>(() => new PalletIDGenerator(mockSSCCGenerator.Object, null, ssccPrefixFinder.Object));
		}

		public void TestConstructor_NullSSCCPrefixFinder_ShouldError()
		{
			var mockSSCCGenerator = new Mock<IPalletIDFromSSCCGenerator>();
			var mockDocketGenerator = new Mock<IPalletIDFromDocketIDGenerator>();
			AssertExceptionThrown<ArgumentNullException>(() => new PalletIDGenerator(mockSSCCGenerator.Object, mockDocketGenerator.Object, null));
		}

		public void TestObjectFactoryRegistration()
		{
			AssertType<PalletIDGenerator>(ObjectFactory.Get<IPalletIDGenerator>());
		}

		#endregion

		#region GenerateIDs

		public void TestGenerateIDs_NullDocket_ShouldError()
		{
			var (generator, _, _) = GetPalletIDGeneratorForTesting();

			AssertExceptionThrown<ArgumentNullException>(() => generator.GenerateIDs(null, 1, shouldPrompt: false));
		}

		public void TestGenerateIDs_MultipleDifferentDockets_ShouldError()
		{
			var (generator, _, _) = GetPalletIDGeneratorForTesting();

			var docket1 = Factory.NewWithValidTestData<WhsReceive>();
			var docket2 = Factory.NewWithValidTestData<WhsReceive>();
			var docket3 = Factory.NewWithValidTestData<WhsTransfer>();

			generator.GenerateIDs(docket1, 1, shouldPrompt: false);
			generator.GenerateIDs(docket1, 2, shouldPrompt: false);

			AssertExceptionThrown(
				"Should not use PalletIDGenerator with multiple different dockets.",
				typeof(ArgumentException),
				() => generator.GenerateIDs(docket2, 1, shouldPrompt: false));

			AssertExceptionThrown(
				"Should not use PalletIDGenerator with multiple different dockets.",
				typeof(ArgumentException),
				() => generator.GenerateIDs(docket3, 1, shouldPrompt: false));
		}

		public void TestGenerateIDs_NotWhsGenerateSSCCOnInbound_FallsBackDocket()
		{
			AssertGenerateIDCalls<WhsReceive>(
				whsGenerateSSCCOnInbound: false,
				hasClientPrefix: true,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: true,
				shouldPrompt: false);

			AssertGenerateIDCalls<WhsTransfer>(
				whsGenerateSSCCOnInbound: false,
				hasClientPrefix: true,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: true,
				shouldPrompt: false);
		}

		public void TestGenerateIDs_PrioritisesClient()
		{
			AssertGenerateIDCalls<WhsReceive>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: true,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: true,
				shouldPrompt: false,
				ClientPrefix);
			AssertGenerateIDCalls<WhsTransfer>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: true,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: true,
				shouldPrompt: false,
				ClientPrefix);
		}

		public void TestGenerateIDs_NoClient_FallsBackWarehouse()
		{
			AssertGenerateIDCalls<WhsReceive>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: true,
				shouldPrompt: false,
				WarehousePrefix);
			AssertGenerateIDCalls<WhsTransfer>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: true,
				shouldPrompt: false,
				WarehousePrefix);
		}

		public void TestGenerateIDs_NoClientNoWarehouse_FallsBackDocket()
		{
			AssertGenerateIDCalls<WhsReceive>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: false,
				warehouseUsesPrefix: true,
				shouldPrompt: false);
			AssertGenerateIDCalls<WhsTransfer>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: false,
				warehouseUsesPrefix: true,
				shouldPrompt: false);
		}

		public void TestGenerateIDs_NoClientAndWarehouseNotUsePrefix_CannotPrompt_FallsBackDocket()
		{
			AssertGenerateIDCalls<WhsReceive>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: false,
				shouldPrompt: false);
			AssertGenerateIDCalls<WhsTransfer>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: false,
				shouldPrompt: false);
		}

		public void TestGenerateIDs_NoClientAndWarehouseNotUsePrefix_PromptUserYes_SSCC()
		{
			AssertGenerateIDCallsWithNotify<WhsReceive>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: false,
				userResponse: true,
				WarehousePrefix);
			AssertGenerateIDCallsWithNotify<WhsTransfer>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: false,
				userResponse: true,
				WarehousePrefix);
		}

		public void TestGenerateIDs_NoClientAndWarehouseNotUsePrefix_PromptUserCacheYes_SSCC()
		{
			AssertGenerateIDCallsWithNotifyCache<WhsReceive>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: false,
				firstUserResponse: true,
				WarehousePrefix);
			AssertGenerateIDCallsWithNotifyCache<WhsTransfer>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: false,
				firstUserResponse: true,
				WarehousePrefix);
		}

		public void TestGenerateIDs_NoClientAndWarehouseNotUsePrefix_PromptUserNo_FallsBackDocket()
		{
			AssertGenerateIDCallsWithNotify<WhsReceive>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: false,
				userResponse: false);
			AssertGenerateIDCallsWithNotify<WhsTransfer>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: false,
				userResponse: false);
		}

		public void TestGenerateIDs_NoClientAndWarehouseNotUsePrefix_PromptUserCacheNo_FallsBackDocket()
		{
			AssertGenerateIDCallsWithNotifyCache<WhsReceive>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: false,
				firstUserResponse: false);
			AssertGenerateIDCallsWithNotifyCache<WhsTransfer>(
				whsGenerateSSCCOnInbound: true,
				hasClientPrefix: false,
				hasWarehousePrefix: true,
				warehouseUsesPrefix: false,
				firstUserResponse: false);
		}

		T GetDocketForTesting<T>(bool whsGenerateSSCCOnInbound, bool hasClientPrefix, bool hasWarehousePrefix, bool warehouseUsesPrefix) where T : WhsDocket
		{
			var docket = Factory.NewWithValidTestData<T>();
			var client = Factory.New<OrgHeader>();
			var warehouse = Factory.New<WhsWarehouse>();
			var warehouseAddress = Factory.New<OrgAddress>();
			var warehouseAddressHeader = Factory.New<OrgHeader>();

			docket.WD_OH_Client = client.PK;
			docket.WD_WW_Whs = warehouse.PK;
			warehouse.WW_OA_WarehouseAddress = warehouseAddress.PK;
			warehouseAddress.OA_OH = warehouseAddressHeader.PK;

			client.MiscServ.OM_WhsGenerateSSCCOnInbound = whsGenerateSSCCOnInbound;
			client.OH_FullName = ClientName;

			if (hasClientPrefix)
			{
				client.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, ClientPrefix);
			}

			if (hasWarehousePrefix)
			{
				var cusCode = warehouseAddressHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, WarehousePrefix);
				cusCode.OK_OA_PremisesAddress = warehouseAddress.PK;
			}

			warehouse.WW_UseGS1PrefixFallback = warehouseUsesPrefix;

			return docket;
		}

		(PalletIDGenerator, Mock<IPalletIDFromDocketIDGenerator>, Mock<IPalletIDFromSSCCGenerator>) GetPalletIDGeneratorForTesting()
		{
			var mockDocketGenerator = new Mock<IPalletIDFromDocketIDGenerator>();
			var mockSSCCGenerator = new Mock<IPalletIDFromSSCCGenerator>();

			mockDocketGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsDocket>(), It.IsAny<int>(), It.IsAny<int>())).Returns(ResultDocket);
			mockSSCCGenerator.Setup(g => g.GenerateIDs(It.IsAny<ZString>(), It.IsAny<int>())).Returns(ResultSSCC);

			return (new PalletIDGenerator(mockSSCCGenerator.Object, mockDocketGenerator.Object, ObjectFactory.New<ISSCCPrefixFinder>()),
				mockDocketGenerator,
				mockSSCCGenerator);
		}

		void AssertGenerateIDCallsWithNotifyCache<T>(
			bool whsGenerateSSCCOnInbound,
			bool hasClientPrefix,
			bool hasWarehousePrefix,
			bool warehouseUsesPrefix,
			bool firstUserResponse,
			string ssccPrefix = null) where T : WhsDocket
		{
			var docket = GetDocketForTesting<T>(whsGenerateSSCCOnInbound, hasClientPrefix, hasWarehousePrefix, warehouseUsesPrefix);
			var notificationBuffer = new TestNotificationBuffer(firstUserResponse);
			docket.NotificationManager.Push(notificationBuffer);
			var (generator, mockDocketGenerator, mockSSCCGenerator) = GetPalletIDGeneratorForTesting();

			// Preconditon
			AssertGenerateIDCalls(docket, generator, mockDocketGenerator, mockSSCCGenerator, shouldPrompt: true, ssccPrefix);
			AssertLastQueryCorrect(notificationBuffer);

			notificationBuffer.LastQueryUserEventArgs = null;
			mockDocketGenerator.Reset();
			mockSSCCGenerator.Reset();
			mockDocketGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsDocket>(), It.IsAny<int>(), It.IsAny<int>())).Returns(ResultDocket);
			mockSSCCGenerator.Setup(g => g.GenerateIDs(It.IsAny<ZString>(), It.IsAny<int>())).Returns(ResultSSCC);

			// Assert
			AssertGenerateIDCalls(docket, generator, mockDocketGenerator, mockSSCCGenerator, shouldPrompt: true, ssccPrefix);
			AssertNull(notificationBuffer.LastQueryUserEventArgs);
		}

		void AssertGenerateIDCallsWithNotify<T>(
			bool whsGenerateSSCCOnInbound,
			bool hasClientPrefix,
			bool hasWarehousePrefix,
			bool warehouseUsesPrefix,
			bool userResponse,
			string ssccPrefix = null) where T : WhsDocket
		{
			var docket = GetDocketForTesting<T>(whsGenerateSSCCOnInbound, hasClientPrefix, hasWarehousePrefix, warehouseUsesPrefix);
			var notificationBuffer = new TestNotificationBuffer(userResponse);
			docket.NotificationManager.Push(notificationBuffer);
			var (generator, mockDocketGenerator, mockSSCCGenerator) = GetPalletIDGeneratorForTesting();

			AssertGenerateIDCalls(docket, generator, mockDocketGenerator, mockSSCCGenerator, shouldPrompt: true, ssccPrefix);
			AssertLastQueryCorrect(notificationBuffer);
		}

		void AssertLastQueryCorrect(TestNotificationBuffer notificationBuffer)
		{
			AssertNotNull(notificationBuffer.LastQueryUserEventArgs);
			Assert(notificationBuffer.LastQueryUserEventArgs is QueryUserYesNoEventArgs);
			AssertEquals(
				$"Client {ClientName} does not have a GS1 company prefix, would you like to use the warehouse GS1 company prefix?",
				((QueryUserYesNoEventArgs)notificationBuffer.LastQueryUserEventArgs).Message);
			AssertEquals(
				"Generate SSCC Number",
				((QueryUserYesNoEventArgs)notificationBuffer.LastQueryUserEventArgs).Caption);
		}

		void AssertGenerateIDCalls<T>(
			bool whsGenerateSSCCOnInbound,
			bool hasClientPrefix,
			bool hasWarehousePrefix,
			bool warehouseUsesPrefix,
			bool shouldPrompt,
			string ssccPrefix = null) where T : WhsDocket
		{
			var (generator, mockDocketGenerator, mockSSCCGenerator) = GetPalletIDGeneratorForTesting();

			AssertGenerateIDCalls(
				GetDocketForTesting<T>(whsGenerateSSCCOnInbound, hasClientPrefix, hasWarehousePrefix, warehouseUsesPrefix),
				generator,
				mockDocketGenerator,
				mockSSCCGenerator,
				shouldPrompt,
				ssccPrefix);
		}

		void AssertGenerateIDCalls(
			WhsDocket docket,
			PalletIDGenerator generator,
			Mock<IPalletIDFromDocketIDGenerator> mockDocketGenerator,
			Mock<IPalletIDFromSSCCGenerator> mockSSCCGenerator,
			bool shouldPrompt,
			string ssccPrefix = null)
		{
			var number = 7;
			var start = 6;

			AssertContainsExactElementsInExactOrder((ssccPrefix is null) ? ResultDocket : ResultSSCC.Select(id => new GeneratedID(id, -1)).ToArray(), generator.GenerateIDs(docket, number, shouldPrompt, start));

			if (ssccPrefix is null)
			{
				mockSSCCGenerator.Verify(g => g.GenerateIDs(It.IsAny<ZString>(), It.IsAny<int>()), Times.Never);
				mockDocketGenerator.Verify(g => g.GenerateIDs(docket, number, start), Times.Once);
			}
			else
			{
				mockSSCCGenerator.Verify(g => g.GenerateIDs(ssccPrefix, number), Times.Once);
				mockDocketGenerator.Verify(g => g.GenerateIDs(It.IsAny<WhsDocket>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
			}
		}

		#endregion

		const string ClientName = nameof(ClientName);
		const string ClientPrefix = nameof(ClientPrefix);
		const string WarehousePrefix = nameof(WarehousePrefix);

		IEnumerable<GeneratedID> ResultDocket => new[] { new GeneratedID("DocketResult1", 1), new GeneratedID("DocketResult2", 2) };

		IEnumerable<ZString> ResultSSCC => new ZString[] { "SSCCResult1", "SSCCResult2" };
	}
}
