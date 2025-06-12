using System;
using System.Reflection;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.Core.PropertySchemas;
using CargoWise.eHub.Products.OceanCarrierMessaging.SymmetricalMessaging.PipelineComponents;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests.PipelineComponents
{
    [TestClass]
    public class PersistMessageComponentTests : BaseComponentTest
    {
        private delegate void InsertToInboxDelegate(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message, bool orderedDelivery, string contextProperty);

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestPersistMessageComponent()
        {
            var pipelineContext = new PipelineContext();

            var message = MessageFactory.CreateMessage();
            message.Context.WriteProperty<EmailSubject>("EmailSubject");
            message.Context.WriteProperty<FileName>("FileName");
            message.Context.WriteProperty<BTS.DestinationParty>("RECIPIENT1");
            message.Context.WriteProperty<BTS.SourceParty>("Sender1");
            message.Context.WriteProperty<ApplicationCode>("BIZ");
            message.Context.WriteProperty<SchemaType>("Xml");
            message.Context.WriteProperty<SchemaName>("http://www.cargowise.com/Schemas/Universal/2011/11");
            message.Context.WriteProperty<MessageTrackingID>("FC0E921C-A404-48ED-801C-A26863712C34");

            message.Context.Write("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema", "TestUNB_Segment");
            message.Context.Write("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "TestUNB5");
            message.Context.Write("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "TestUNB9");
            message.Context.Write("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "TestUNB11");
            message.Context.Write("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "TestUNH1");
            message.Context.Write("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "TestOverrideEDIHeader");
            message.Context.Write("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "FC0E921C-A404-48ED-801C-A26863712C34");
            message.Context.Write("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "TestDestinationPartySenderIdentifier");
            message.Context.Write("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "TestDestinationPartySenderQualifier");
            message.Context.Write("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "TestDestinationPartyReceiverIdentifier");
            message.Context.Write("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "TestDestinationPartyReceiverQualifier");
            message.Context.Write("EarlyTerminateEdifactUNB", "http://cargowise.com/ehub/processing/2010/06", "TestEarlyTerminateEdifactUNB");
            message.Context.Write("GS_Segment", "http://schemas.microsoft.com/Edi/PropertySchema", "TestGS_Segment");

            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            message.BodyPart.Data = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(GetType().Namespace + ".TestFiles.Message.xml");

            var expectedContextProperty = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(GetType().Namespace + ".TestFiles.PersistMessage.xml");

            var inboxAccessor = MockRepository.GenerateStrictMock<IInboxAccessor>();
            inboxAccessor.Expect(x => x.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null, false, string.Empty)).IgnoreArguments()
                .Do(new InsertToInboxDelegate((senderID, envelopeTrackingID, inboxPK, status, gatewayMessage, orderedDelivery, contextProperty) =>
                {
                    Assert.AreNotEqual(Guid.Empty, inboxPK);
                    Assert.AreEqual("Sender1", senderID);
                    Assert.AreEqual(Guid.Empty, envelopeTrackingID);
                    Assert.AreEqual(MessageStatus.Processing, status);
                    Assert.AreEqual("fc0e921c-a404-48ed-801c-a26863712c34", gatewayMessage.MessageTrackingID.ToString());
                    Assert.AreEqual("RECIPIENT1", gatewayMessage.ClientID);
                    Assert.AreEqual("", gatewayMessage.SchemaName);
                    Assert.AreEqual(MessageSchemaType.Xml, gatewayMessage.SchemaType);
                    Assert.AreEqual("EmailSubject", gatewayMessage.EmailSubject);
                    Assert.AreEqual("FileName", gatewayMessage.FileName);

                    message.BodyPart.Data.SeekBegin();
                    Assert.AreEqual(message.BodyPart.Data.ReadToEnd(), gatewayMessage.MessageStream.DecodeAndDecompress().ReadToEnd());
                    Assert.AreEqual(expectedContextProperty.ReadToEnd(), contextProperty);
                }));

            var component = MockRepository.GenerateStrictMock<PersistMessageComponent>();
            component.Expect(x => x.GetInboxAccessor()).Return(inboxAccessor);
            Assert.AreEqual(message, component.Execute(pipelineContext, message));

            component.Enabled = true;
            Assert.AreEqual(null, component.Execute(pipelineContext, message));

            inboxAccessor.VerifyAllExpectations();
            component.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestInterfaceImplementation_PersistMessageComponent()
        {
            var component = new PersistMessageComponent();

            Assert.AreEqual("Product: Persist Message Component", component.Description);
            Assert.AreEqual("Product: Persist Message Component", component.Name);
            Assert.AreEqual("1.0", component.Version);
            Assert.AreEqual(IntPtr.Zero, component.Icon);
            Assert.IsNull(component.Validate(null));

            Guid guid;
            component.GetClassID(out guid);
            Assert.AreEqual(new Guid("4ED9B137-ED7A-4F2F-8A00-32EC44F5DAA4"), guid);

            var propertyBag = MockRepository.GenerateStrictMock<IPropertyBag>();
            propertyBag.Expect(x => x.Write("Enabled", false));
    
            propertyBag.Expect(x => x.Read(Arg.Is("Enabled"), out Arg<Object>.Out(true).Dummy, Arg.Is(0)));

            // Test save
            component.Enabled = false;
            component.Save(propertyBag, true, true);
            // Test load
            component.Load(propertyBag, 0);
            Assert.IsTrue(component.Enabled);

            propertyBag.VerifyAllExpectations();
        }
    }
}
