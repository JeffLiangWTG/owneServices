using CargoWise.eHub.AlertService.PipelineComponents;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace CargoWise.eHub.AlertService.Tests
{
	[TestClass]
	public class ErrorPreprocessingComponentTest : BaseComponentTest
	{
		eHubTransactionsContext dbContext;

		eHubClient client;

		ErrorPreprocessingComponent GetErrorPreprocessingComponent()
        {
            var testClients = new TestDbSet<eHubClient>();
            client = testClients.Add(new eHubClient() { CC_ID = "SENDALERT" });

            dbContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            dbContext.Stub(x => x.eHubClients).Return(testClients);

            var component = MockRepository.GenerateMock<ErrorPreprocessingComponent>();
            component.Stub(x => x.GetDBContext()).Return(dbContext);
			return component;
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestCommonInterfacesImplementation_ErrorPreprocessingComponent()
        {
            var component = new ErrorPreprocessingComponent();
            Assert.AreEqual("Preprocess Error Message Before AlertService Orchestration", component.Description);
            Assert.AreEqual("Error Preprocessing", component.Name);
            Assert.AreEqual("1.0", component.Version);
            Assert.AreEqual(IntPtr.Zero, component.Icon);
            Assert.AreEqual(0, component.ExceptionRetryCount);
            Assert.AreEqual(0, component.ExceptionRetryIntervalSenconds);
            Assert.IsNull(component.Validate(null));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestErrorPreprocessing_AS2_MDN_IsIgnored()
        {
            var message = MessageFactory.CreateMessage();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
            message.Context = MessageFactory.CreateMessageContext();
            message.Context.WriteProperty<InternalTrackingID>("6191facc-c2c3-4ecc-90c5-c3aee35abd37");
            message.Context.WriteProperty<ErrorReport.Description>("Error description.");
            message.Context.WriteProperty<BTS.SourceParty>("SENDER");
            message.Context.WriteProperty<ErrorReport.ErrorType>("Something");
            message.Context.WriteProperty<EdiIntAS.IsAS2PayloadMessage>(false);

            var component = GetErrorPreprocessingComponent();
            var output = component.Execute(new PipelineContext(), message);
            Assert.IsNull(output);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestErrorPreprocessing_System_Generated_Ack_IsIgnored()
        {
            var message = MessageFactory.CreateMessage();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
            message.Context = MessageFactory.CreateMessageContext();
            message.Context.WriteProperty<InternalTrackingID>("6191facc-c2c3-4ecc-90c5-c3aee35abd37");
            message.Context.WriteProperty<ErrorReport.Description>("Error description.");
            message.Context.WriteProperty<BTS.SourceParty>("SENDER");
            message.Context.WriteProperty<ErrorReport.ErrorType>("Something");
            message.Context.WriteProperty<EDI.IsSystemGeneratedAck>(true);

            var component = GetErrorPreprocessingComponent();
            var output = component.Execute(new PipelineContext(), message);
            Assert.IsNull(output);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestErrorPreprocessing_RoutingRules_ClientDoesntExist()
        {
			var context = MockRepository.GenerateMock<eHubTransactionsContext>();
			context.Stub(x => x.eHubRoutingRules).Return(new TestDbSet<eHubRoutingRule>());
			context.Stub(x => x.eHubClients).Return(new TestDbSet<eHubClient>());


			var message = MessageFactory.CreateMessage();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
            message.Context = MessageFactory.CreateMessageContext();
            message.Context.WriteProperty<InternalTrackingID>("6191facc-c2c3-4ecc-90c5-c3aee35abd37");
            message.Context.WriteProperty<ErrorReport.Description>("Error description.");
            message.Context.WriteProperty<BTS.SourceParty>("SENDER");
            message.Context.WriteProperty<ErrorReport.ErrorType>("Something");

			var component = MockRepository.GenerateMock<ErrorPreprocessingComponent>();
			component.Stub(x => x.GetDBContext()).Return(context);
			component.Execute(new PipelineContext(), message);
			Assert.AreEqual(message.Context.ReadPropertyString<CargoWise.eHub.AlertService.PropertySchemas.SkipSendAlert>(), "False");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestErrorPreprocessing_RoutingRules_RetryOnSQLException()
        {
            var dbContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            dbContext.Stub(x => x.eHubRoutingRules).Return(new TestDbSet<eHubRoutingRule>());
            dbContext.Stub(x => x.eHubClients).Return(null).WhenCalled(_ =>
            {
                using (var conn = new SqlConnection("User ID=Rubbish;Password=Rubbish;Initial Catalog=Rubbish;Data Source=Rubbish;Connection Timeout=1"))
                {
                    conn.Open();
                }
            });

            var message = MessageFactory.CreateMessage();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
            message.Context = MessageFactory.CreateMessageContext();
            message.Context.WriteProperty<InternalTrackingID>("6191facc-c2c3-4ecc-90c5-c3aee35abd37");
            message.Context.WriteProperty<ErrorReport.Description>("Error description.");
            message.Context.WriteProperty<BTS.SourceParty>("SENDER");
            message.Context.WriteProperty<ErrorReport.ErrorType>("Something");

            var component = MockRepository.GenerateMock<ErrorPreprocessingComponent>();
            component.Stub(x => x.GetDBContext()).Return(dbContext);
            component.Execute(new PipelineContext(), message);
            Assert.AreEqual(message.Context.ReadPropertyString<CargoWise.eHub.AlertService.PropertySchemas.SkipSendAlert>(), "False");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestErrorPreprocessing_RoutingRules_NoResult()
        {
            var message = MessageFactory.CreateMessage();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
            message.Context = MessageFactory.CreateMessageContext();
            message.Context.WriteProperty<InternalTrackingID>("6191facc-c2c3-4ecc-90c5-c3aee35abd37");
            message.Context.WriteProperty<ErrorReport.Description>("Error description.");
            message.Context.WriteProperty<BTS.SourceParty>("SENDER");
            message.Context.WriteProperty<ErrorReport.ErrorType>("Something");

            var component = GetErrorPreprocessingComponent();
			var iLog = LoggerHelpers.GetPipelineLogger(message);
			var results = new Collection<Result>();
			component.Stub(x => x.GetRoutingRuleEvaluate(dbContext, iLog, message, client)).Return(results);
			component.Execute(new PipelineContext(), message);
            Assert.AreEqual(message.Context.ReadPropertyString<CargoWise.eHub.AlertService.PropertySchemas.SkipSendAlert>(), "False");
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestErrorPreprocessing_RoutingRules_MessagePropertyDoesntMatch()
        {
            var message = MessageFactory.CreateMessage();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
            message.Context = MessageFactory.CreateMessageContext();
            message.Context.WriteProperty<InternalTrackingID>("6191facc-c2c3-4ecc-90c5-c3aee35abd37");
            message.Context.WriteProperty<ErrorReport.Description>("Error description.");
            message.Context.WriteProperty<BTS.SourceParty>("SENDER");
            message.Context.WriteProperty<ErrorReport.ErrorType>("Something");
            message.Context.WriteProperty<ErrorReport.FailureCode>("0xc0c0167a");

            var component = GetErrorPreprocessingComponent();
			var iLog = LoggerHelpers.GetPipelineLogger(message);
			var results = new Collection<Result> { new Result(null, null, null, "SEND") };
			component.Stub(x => x.GetRoutingRuleEvaluate(dbContext, iLog, message, client)).Return(results);
			component.Execute(new PipelineContext(), message);
            Assert.AreEqual(message.Context.ReadPropertyString<CargoWise.eHub.AlertService.PropertySchemas.SkipSendAlert>(), "False");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestErrorPreprocessing_RoutingRules_MessagePropertyMatch()
        {
            var testClients = new TestDbSet<eHubClient>();
            var client = testClients.Add(new eHubClient() { CC_ID = "SENDALERT" });

			var dbContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            dbContext.Stub(x => x.eHubClients).Return(testClients);

			var message = MessageFactory.CreateMessage();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
            message.Context = MessageFactory.CreateMessageContext();
            message.Context.WriteProperty<InternalTrackingID>("6191facc-c2c3-4ecc-90c5-c3aee35abd37");
            message.Context.WriteProperty<ErrorReport.Description>("Error description.");
            message.Context.WriteProperty<BTS.SourceParty>("SENDER");
            message.Context.WriteProperty<ErrorReport.ErrorType>("Something");
            message.Context.WriteProperty<ErrorReport.FailureCode>("0xc0c0167a");

            var component = MockRepository.GenerateMock<ErrorPreprocessingComponent>();
            component.Stub(x => x.GetDBContext()).Return(dbContext);
			var iLog = LoggerHelpers.GetPipelineLogger(message);
			var results = new Collection<Result> { new Result(null, null, null, "SKIP") };
			component.Stub(x => x.GetRoutingRuleEvaluate(dbContext, iLog, message, client)).Return(results);

			component.Execute(new PipelineContext(), message);
            Assert.AreEqual(message.Context.ReadPropertyString<CargoWise.eHub.AlertService.PropertySchemas.SkipSendAlert>(), "True");
        }
    }
}
