using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class PartyResolverTests
	{
		private PartyResolver _resolver;
		private IPartyAccessor _partyAccessor = MockRepository.GenerateStrictMock<IPartyAccessor>();
		private const string ServiceProvider = "CCSJ";

		[TestInitialize]
		public void SetUp()
		{
			_resolver = MockRepository.GeneratePartialMock<PartyResolver>(ServiceProvider);
			_resolver.Stub(x => x.PartyAccessor).Return(_partyAccessor);
		}

		[TestMethod]
		public void TestResolveRecipient_ResolveBySubscription_WithValidAWB()
		{
			var clientPIMA = "test PIMA";
			var clientAWB = "test AWB";
			var clientId = "client from subscription";
			_partyAccessor.Stub(x => x.GetClientIDFromClientAWB(clientAWB, ServiceProvider)).Return(clientId);

			var recipient = _resolver.ResolveRecipient(clientPIMA, clientAWB);

			Assert.IsNotNull(recipient);
			Assert.AreEqual(recipient, clientId);
			_partyAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestResolveRecipient_ResolveBySubscription_WithNoValidAWBAndValidPIMA()
		{
			var clientPIMA = "test PIMA";
			var clientAWB = "test AWB";
			var clientId = "client from PIMA";
			_partyAccessor.Stub(x => x.GetClientIDFromClientAWB(clientAWB, ServiceProvider)).Return(null);
			_partyAccessor.Stub(x => x.GetClientIDFromAirPIMA(clientPIMA, ServiceProvider)).Return(clientId);

			var recipient = _resolver.ResolveRecipient(clientPIMA, clientAWB);

			Assert.IsNotNull(recipient);
			Assert.AreEqual(recipient, clientId);
			_partyAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestResolveRecipient_WithNoValidAWBAndNoValidPIMA()
		{
			var clientPIMA = "test PIMA";
			var clientAWB = "test AWB";
			_partyAccessor.Stub(x => x.GetClientIDFromClientAWB(clientAWB, ServiceProvider)).Return(null);
			_partyAccessor.Stub(x => x.GetClientIDFromAirPIMA(clientPIMA, ServiceProvider)).Return(null);

			var recipient = _resolver.ResolveRecipient(clientPIMA, clientAWB);
			
			Assert.IsNull(recipient);
			_partyAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestResolveRecipient_WithValidSubServiceProviders()
		{
			var clientPIMA = "test PIMA";
			var clientAWB = "test AWB";
			var clientId = "client from subServiceProvider";
			var subServiceProvider = "test SubServiceProvider";
			_resolver.SubServiceProviders = new[] { "test SubServiceProvider" };
			_partyAccessor.Stub(x => x.GetClientIDFromClientAWB(clientAWB, ServiceProvider)).Return(null);
			_partyAccessor.Stub(x => x.GetClientIDFromClientAWB(clientAWB, subServiceProvider)).Return(clientId);
			_partyAccessor.Stub(x => x.GetClientIDFromAirPIMA(clientPIMA, ServiceProvider)).Return(null);

			var recipient = _resolver.ResolveRecipient(clientPIMA, clientAWB);

			Assert.IsNotNull(recipient);
			Assert.AreEqual(recipient, clientId);
			_partyAccessor.VerifyAllExpectations();
		}
	}
}
