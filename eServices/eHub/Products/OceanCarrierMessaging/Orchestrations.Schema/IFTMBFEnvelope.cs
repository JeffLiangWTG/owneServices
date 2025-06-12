namespace CargoWise.eHub.Products.OceanCarrierMessaging.Orchestrations.Schemas {
    using Microsoft.XLANGs.BaseTypes;
    
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.BizTalk.Schema.Compiler", "3.0.1.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [SchemaType(SchemaTypeEnum.Document)]
    [Schema(@"http://wisetechglobal.com/oceancarreirmessaging/2020/08",@"VERMASEnvelope")]
    [System.SerializableAttribute()]
    [SchemaRoots(new string[] {@IFTMBFEnvelope"})]
    [Microsoft.XLANGs.BaseTypes.SchemaReference(@"CargoWise.eHub.Products.OceanCarrierMessaging.Orchestrations.Schemas.EFACT_D99B_IFTMBF", typeof(global::CargoWise.eHub.Products.OceanCarrierMessaging.Orchestrations.Schemas.EFACT_D99B_IFTMBF))]
    public sealed class IFTMBFEnvelope : Microsoft.XLANGs.BaseTypes.SchemaBase {
        
        [System.NonSerializedAttribute()]
        private static object _rawSchema;
        
        [System.NonSerializedAttribute()]
        private const string _strSchema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema xmlns=""http://wisetechglobal.com/oceancarreirmessaging/2020/08"" xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" targetNamespace=""http://wisetechglobal.com/oceancarreirmessaging/2020/08"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:include schemaLocation=""CargoWise.eHub.Products.OceanCarrierMessaging.Orchestrations.Schemas.EFACT_D99B_IFTMBF"" />
  <xs:annotation>
    <xs:appinfo>
      <b:schemaInfo root_reference=""IFTMBFEnvelope"" xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" />
    </xs:appinfo>
  </xs:annotation>
  <xs:element name=""IFTMBFEnvelope"">
    <xs:annotation>
      <xs:appinfo>
        <b:recordInfo rootTypeName=""IFTMBFEnvelope"" xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" />
      </xs:appinfo>
    </xs:annotation>
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""EFACT_D99B_IFTMBF"" />
        <xs:element name=""EmailContent"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""html"">
                <xs:complexType>
                  <xs:sequence>
                    <xs:any />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name=""EmailAttachment"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""html"">
                <xs:complexType>
                  <xs:sequence>
                    <xs:any />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";
        
        public IFTMBFEnvelope() {
        }
        
        public override string XmlContent {
            get {
                return _strSchema;
            }
        }
        
        public override string[] RootNodes {
            get {
                string[] _RootElements = new string [1];
                _RootElements[0] = "IFTMBFEnvelope";
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
