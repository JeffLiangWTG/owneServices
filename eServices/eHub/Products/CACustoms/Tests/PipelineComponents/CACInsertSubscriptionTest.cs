using System.IO;
using System.Text;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.CACustoms.PipelineComponents;
using CargoWise.eHub.Products.Core.PipelineComponents;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.BizTalk.Component.Interop;
using Rhino.Mocks;
using System;

namespace CargoWise.eHub.Products.CACustoms.Tests.PipelineComponents
{
	[TestClass]
	public class CACInsertSubscriptionTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSavingRawMessage()
		{
			var fullMessage = "UNA:+.? 'UNB+UNOA:3+YUSAIRXPN+RCCECECPT+140818:1900+112'UNG+GOVCBR+U10207V2+QE+140818:1900+112+UN+S:99B+12345YUSEN'UNH+242+CUSDEC:S:99B:UN'UNS+D'CST+1'DTM+7:20140812:102'UNS+S'BGM+929+AAAAA789123+9+FR'UNT+7+242'UNH+242+CUSDEC:S:99B:UN'UNS+D'CST+1'DTM+7:20140812:102'UNS+S'BGM+929+BBBBB123456+9+FR'UNT+7+242'UNE+1+112'UNZ+1+112'";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(fullMessage));
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.SourceParty>("HYEDAUIKB");
			message.Context.WriteProperty<BTS.DestinationParty>("CACustomsMQ-Test");

			var dataModelAccessor = MockRepository.GenerateMock<DataModelAccessor>();
			dataModelAccessor.Expect(m => m.InsertSubscriptionValueWithRetries("IIDMSG", "CACustoms-Test", "HYEDAUIKB", "RCCECECPT+YUSAIRXPN+AAAAA789123",
				"UNH+242+CUSDEC:S:99B:UN'UNS+D'CST+1'DTM+7:20140812:102'UNS+S'BGM+929+AAAAA789123+9+FR'UNT+7+242'", "IIDMSG")).Repeat.Once();
			dataModelAccessor.Expect(m => m.InsertSubscriptionValueWithRetries("IIDMSG", "CACustoms-Test", "HYEDAUIKB", "RCCECECPT+YUSAIRXPN+BBBBB123456",
				"UNH+242+CUSDEC:S:99B:UN'UNS+D'CST+1'DTM+7:20140812:102'UNS+S'BGM+929+BBBBB123456+9+FR'UNT+7+242'", "IIDMSG")).Repeat.Once();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var component = MockRepository.PartialMock<CACInsertSubscription>();
			component.Enabled = true;
			component.Expect(_ => _.GetDataModelAccessor()).Repeat.Once().Return(dataModelAccessor);
			MockRepository.ReplayAll();

			component.Execute(pipelineContext, message);

			dataModelAccessor.VerifyAllExpectations();
			component.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNotSavingRawMessage()
		{
			var fullMessage = "UNA:+.? 'UNB+UNOA:3+YUSAIRPROD+RCCECECPP+150114:1920+500000000'UNG+CUSDEC+U10207V2+QE+150114:1920+1+UN+S:99B+10207YLCAYYZ'UNH+1+CUSDEC:S:99B:UN'BGM+:::QE+000:BEAT+9'UNS+D'CST+1'LOC+25+US'DTM+7:20201110:102'MOA+6::USD'UNS+S'UNT+9+1'UNE+1+1'UNZ+1+500000000'";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(fullMessage));
			message.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var component = MockRepository.PartialMock<CACInsertSubscription>();
			component.Enabled = true;
			component.Expect(_ => _.GetDataModelAccessor()).Repeat.Never();
			MockRepository.ReplayAll();

			component.Execute(pipelineContext, message);

			component.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestInsertSubscriptionForXMLMessage()
		{
			var fullMessage = @"<?xml version='1.0' encoding='utf-8'?>
<DocumentMetaData xmlns='urn:wco:datamodel:WCO:Declaration:1'>
  <CommunicationMetaData>
    <ApplicationReferenceID>3333333333333100001001</ApplicationReferenceID>
    <Recipient>
      <ID>207461995RM0001</ID>
    </Recipient>
  </CommunicationMetaData>
  <Declaration>
    <Declarant>
      <ID>222222222RM0004</ID>
    </Declarant>
    <Importer>
      <ID>111111111RM0001</ID>
    </Importer>
  </Declaration>
</DocumentMetaData>";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(fullMessage));
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.SourceParty>("HYEDAUIKB");
			message.Context.WriteProperty<BTS.DestinationParty>("CACustomsMQ-Test");
			message.Context.WriteProperty<CargoWise.eHub.Core.PropertySchemas.OverrideEmailSubject>("CAD");

			var dataModelAccessor = MockRepository.GenerateMock<DataModelAccessor>();
			dataModelAccessor.Expect(m => m.InsertSubscriptionValueWithRetries("CACMSG", "CACustoms-Test", "HYEDAUIKB", "3333333333333100001001", null, "CAD")).Repeat.Once();
			dataModelAccessor.Expect(m => m.InsertSubscriptionValueWithRetries("CACMSG", "CACustoms-Test", "HYEDAUIKB", "222222222", "DeclarantID", "CAD")).Repeat.Once();
			dataModelAccessor.Expect(m => m.InsertSubscriptionValueWithRetries("CACMSG", "CACustoms-Test", "HYEDAUIKB", "111111111RM0001", "ImporterID", "CAD")).Repeat.Once();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var component = MockRepository.PartialMock<CACInsertSubscription>();
			component.Enabled = true;
			component.Expect(_ => _.GetDataModelAccessor()).Repeat.Once().Return(dataModelAccessor);
			component.IsXmlContent = true;
			MockRepository.ReplayAll();

			component.Execute(pipelineContext, message);

			dataModelAccessor.VerifyAllExpectations();
			component.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestExceptionWhenReferenceIDNotFoundForXMLMessage()
		{
			var fullMessage = @"<?xml version='1.0' encoding='utf-8'?>
<DocumentMetaData xmlns='urn:wco:datamodel:WCO:Declaration:1'>
    <CommunicationMetaData>
        <Recipient>
            <ID>207461995RM0001</ID>
        </Recipient>
    </CommunicationMetaData>
</DocumentMetaData>";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(fullMessage));
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.SourceParty>("HYEDAUIKB");
			message.Context.WriteProperty<BTS.DestinationParty>("CACustomsMQ-Test");
			message.Context.WriteProperty<CargoWise.eHub.Core.PropertySchemas.OverrideEmailSubject>("CAD");

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var component = MockRepository.PartialMock<CACInsertSubscription>();
			component.Enabled = true;
			component.Expect(_ => _.GetDataModelAccessor()).Repeat.Never();
			component.IsXmlContent = true;
			MockRepository.ReplayAll();

			try
			{
				component.Execute(pipelineContext, message);
				Assert.Fail("Exception should be thrown");
			}
			catch(InvalidOperationException ex)
			{
				Assert.AreEqual("Unable to find ApplicationReferenceID.", ex.Message);
			}

			component.VerifyAllExpectations();
		}
	}
}
