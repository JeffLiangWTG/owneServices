namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Schemas.CPOINT {
    using Microsoft.XLANGs.BaseTypes;
    
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.BizTalk.Schema.Compiler", "3.0.1.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [SchemaType(SchemaTypeEnum.Document)]
    [Schema(@"http://www.w3.org/2003/05/soap-envelope",@"Envelope")]
    [System.SerializableAttribute()]
    [SchemaRoots(new string[] {@"Envelope"})]
    [Microsoft.XLANGs.BaseTypes.SchemaReference(@"CargoWise.eHub.Products.ForwardingPortMessaging.BE.Schemas.CPOINT.oasis_200401_wss_wssecurity_secext_v1_0", typeof(global::CargoWise.eHub.Products.ForwardingPortMessaging.BE.Schemas.CPOINT.oasis_200401_wss_wssecurity_secext_v1_0))]
    [Microsoft.XLANGs.BaseTypes.SchemaReference(@"CargoWise.eHub.Products.ForwardingPortMessaging.BE.Schemas.CPOINT.portcommunity", typeof(global::CargoWise.eHub.Products.ForwardingPortMessaging.BE.Schemas.CPOINT.portcommunity))]
    public sealed class EBADEC_SOAP_Request : Microsoft.XLANGs.BaseTypes.SchemaBase {
        
        [System.NonSerializedAttribute()]
        private static object _rawSchema;
        
        [System.NonSerializedAttribute()]
        private const string _strSchema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" xmlns:SOAP-ENV=""http://portcommunity.haven.antwerpen.be/"" attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" targetNamespace=""http://www.w3.org/2003/05/soap-envelope"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:import schemaLocation=""CargoWise.eHub.Products.ForwardingPortMessaging.BE.Schemas.CPOINT.oasis_200401_wss_wssecurity_secext_v1_0"" namespace=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" />
  <xs:import schemaLocation=""CargoWise.eHub.Products.ForwardingPortMessaging.BE.Schemas.CPOINT.portcommunity"" namespace=""http://portcommunity.haven.antwerpen.be/"" />
  <xs:annotation>
    <xs:appinfo>
      <b:references>
        <b:reference targetNamespace=""http://portcommunity.haven.antwerpen.be/"" />
        <b:reference targetNamespace=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" />
        <b:reference targetNamespace=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" />
      </b:references>
    </xs:appinfo>
  </xs:annotation>
  <xs:element name=""Envelope"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""Header"">
          <xs:complexType>
            <xs:sequence>
              <xs:element ref=""wsse:Security"" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name=""Body"">
          <xs:complexType>
            <xs:sequence>
              <xs:element ref=""SOAP-ENV:sendMessages"" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";
        
        public EBADEC_SOAP_Request() {
        }
        
        public override string XmlContent {
            get {
                return _strSchema;
            }
        }
        
        public override string[] RootNodes {
            get {
                string[] _RootElements = new string [1];
                _RootElements[0] = "Envelope";
                return _RootElements;
            }
        }
        
        protected override object RawSchema {
            get {
                return _rawSchema;
            }
            set {
                _rawSchema = value;
            }
        }
    }
}
