using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Transactions;
using System.Xml;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.Core.PipelineComponents;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.SymmetricalMessaging.PipelineComponents
{
    [Serializable]
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    [Guid("A261DE9A-0CAA-4F96-9E92-F0B2E4D40D4C")]
    public class PersistMessageComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
    {
        #region IBaseComponent Members

        public string Description
        {
            get { return "Product: Persist Message Component"; }
        }

        public string Name
        {
            get { return "Product: Persist Message Component"; }
        }

        public string Version
        {
            get { return "1.0"; }
        }

        #endregion

        #region IComponentUI Members

        public IntPtr Icon
        {
            get { return IntPtr.Zero; }
        }

        public IEnumerator Validate(object projectSystem)
        {
            return null;
        }

        #endregion

        #region IComponent Members

        public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
        {
            if (!Enabled) return message;
            if (pipelineContext == null) throw new ArgumentException("PipelineContext");

            MessageHelper.WrapMessageInReadOnlySeekableStream(pipelineContext, message);
            Stream compressAndEncodedStream = message.BodyPart.Data.CompressAndEncode();
            pipelineContext.ResourceTracker.AddResource(compressAndEncodedStream);

            string emailSubject = message.Context.ReadPropertyString<EmailSubject>();
            string fileName = message.Context.ReadPropertyString<FileName>();
            string senderID = message.Context.ReadPropertyString<BTS.SourceParty>();
            string clientID = message.Context.ReadPropertyString<BTS.DestinationParty>();
            string applicationCode = "BIZ";
            var schemaType = MessageSchemaType.Xml;
            string messageType = string.Empty; 
            var messageTrackingID = MessageHelper.GetMessageTrackingID(message);

            var contextProperty = CreateContextProperty(pipelineContext, message);
            try
            {
                GetInboxAccessor().InsertToInbox(
                    senderID,
                    Guid.Empty,
                    messageTrackingID,
                    MessageStatus.Processing,
                    new eHubGatewayMessage()
                    {
                        MessageTrackingID = messageTrackingID,
                        ClientID = clientID,
                        ApplicationCode = applicationCode,
                        SchemaName = messageType,
                        SchemaType = schemaType,
                        EmailSubject = string.IsNullOrEmpty(emailSubject) ? string.Empty : emailSubject,
                        FileName = string.IsNullOrEmpty(fileName) ? string.Empty : fileName,
                        MessageStream = compressAndEncodedStream
                    },
                    false,
                    contextProperty);
            }
            catch (SqlException ex)
            {
                var exception = ExceptionBuilder.New(ex);
                throw exception;
            }
            return null;
        }

        private const string BtsNamespace = "http://cargowise.com/ehub/system-properties/2010/06";
        private const string PropertySchemaNamespace = "http://schemas.microsoft.com/Edi/PropertySchema";
        private const string EdiPropertiesNamespace = "http://schemas.microsoft.com/BizTalk/2006/edi-properties";
        private const string ProcessingNamespace = "http://cargowise.com/ehub/processing/2010/06";
        private const string TrackingNamespace = "http://cargowise.com/ehub/tracking/2010/06";

        string CreateContextProperty(IPipelineContext context, IBaseMessage message)
        {
            var sourceProperty = message.Context.ReadPropertyString<SourceParty>();
            var destinationParty = message.Context.ReadPropertyString<DestinationParty>();
            var unbSegment = message.Context.Read("UNB_Segment", PropertySchemaNamespace);
            var unb5 = message.Context.Read("UNB5", EdiPropertiesNamespace);
            var unb9 = message.Context.Read("UNB9", EdiPropertiesNamespace);
            var unb11 = message.Context.Read("UNB11", EdiPropertiesNamespace);
            var unh1 = message.Context.Read("UNH1", EdiPropertiesNamespace);
            var ediHeader = message.Context.Read("OverrideEDIHeader", EdiPropertiesNamespace);
            var messageTrackingId = MessageHelper.GetMessageTrackingID(message);
            var internalTrackingId = message.Context.ReadPropertyString<InternalTrackingID>();
            var destinationPartySenderIdentifier = message.Context.Read("DestinationPartySenderIdentifier", PropertySchemaNamespace);
            var destinationPartySenderQualifier = message.Context.Read("DestinationPartySenderQualifier", PropertySchemaNamespace);
            var destinationPartyReceiverIdentifier = message.Context.Read("DestinationPartyReceiverIdentifier", PropertySchemaNamespace);
            var destinationPartyReceiverQualifier = message.Context.Read("DestinationPartyReceiverQualifier", PropertySchemaNamespace);
            var earlyTerminateEdifactUnb = message.Context.Read("EarlyTerminateEdifactUNB", ProcessingNamespace);
            var gsSegment = message.Context.Read("GS_Segment", PropertySchemaNamespace);

            Stream stream = new VirtualStream();
            context.ResourceTracker.AddResource(stream);
            var writer = XmlTextWriter.Create(stream, new XmlWriterSettings() { OmitXmlDeclaration = true });

            writer.WriteStartElement("ContextProperty");
            if (sourceProperty != null) WriteElement(writer, "SourceProperty", sourceProperty, BtsNamespace);
            if (destinationParty != null) WriteElement(writer, "DestinationParty", destinationParty, BtsNamespace);
            if (unbSegment != null) WriteElement(writer, "UNB_Segment", unbSegment.ToString(), PropertySchemaNamespace);
            if (unb5 != null) WriteElement(writer, "UNB5", unb5.ToString(), EdiPropertiesNamespace);
            if (unb9 != null) WriteElement(writer, "UNB9", unb9.ToString(), EdiPropertiesNamespace);
            if (unb11 != null) WriteElement(writer, "UNB11", unb11.ToString(), EdiPropertiesNamespace);
            if (unh1 != null) WriteElement(writer, "UNH1", unh1.ToString(), EdiPropertiesNamespace);
            if (ediHeader != null)
                WriteElement(writer, "OverrideEDIHeader", ediHeader.ToString(), EdiPropertiesNamespace);
            WriteElement(writer, "MessageTrackingID", messageTrackingId.ToString(), TrackingNamespace);
            if (internalTrackingId != null)
                WriteElement(writer, "InternalTrackingID", internalTrackingId, TrackingNamespace);
            if (destinationPartySenderIdentifier != null)
                WriteElement(writer, "DestinationPartySenderIdentifier", destinationPartySenderIdentifier.ToString(),
                    PropertySchemaNamespace);
            if (destinationPartySenderQualifier != null)
                WriteElement(writer, "DestinationPartySenderQualifier", destinationPartySenderQualifier.ToString(),
                    PropertySchemaNamespace);
            if (destinationPartyReceiverIdentifier != null)
                WriteElement(writer, "DestinationPartyReceiverIdentifier",
                    destinationPartyReceiverIdentifier.ToString(), PropertySchemaNamespace);
            if (destinationPartyReceiverQualifier != null)
            WriteElement(writer, "DestinationPartyReceiverQualifier", destinationPartyReceiverQualifier.ToString(), PropertySchemaNamespace);
            if (earlyTerminateEdifactUnb != null)
            WriteElement(writer, "EarlyTerminateEdifactUNB", earlyTerminateEdifactUnb.ToString(), ProcessingNamespace);
            if (gsSegment != null)
            WriteElement(writer, "GS_Segment", gsSegment.ToString(), PropertySchemaNamespace);

            writer.WriteEndElement();
            writer.Flush();

            stream.Position = 0;
            stream.Position = 0;
            return stream.ReadToEnd();
        }

        void WriteElement(XmlWriter writer, string localName, object value, string nameSpace)
        {
            writer.WriteStartElement(localName);
            writer.WriteAttributeString("namespace", nameSpace);
            writer.WriteString(value != null ? value.ToString() : string.Empty);
            writer.WriteEndElement();
        }

        #endregion

        #region IPersistPropertyBag Members

        public void GetClassID(out Guid classID)
        {
            classID = new Guid("4ED9B137-ED7A-4F2F-8A00-32EC44F5DAA4");
        }

        public void InitNew()
        {
        }

        public void Load(IPropertyBag propertyBag, int errorLog)
        {
            object val = null;
            Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

            if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
        }

        public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
        {
            object val;
            val = Enabled; propertyBag.Write("Enabled", ref val);
        }

        #endregion

        #region Properties
        public bool Enabled { get; set; }
        #endregion

        #region Implementation

        public virtual IInboxAccessor GetInboxAccessor()
        {
            return DataAccessFactories.NewInboxAccessorInstance();
        }

        #endregion
    }
}
