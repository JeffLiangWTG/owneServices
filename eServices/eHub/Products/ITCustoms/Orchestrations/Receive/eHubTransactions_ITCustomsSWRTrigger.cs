namespace CargoWise.eHub.Products.ITCustoms.Orchestrations.PollResponse {
    using Microsoft.XLANGs.BaseTypes;
    
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.BizTalk.Schema.Compiler", "3.0.1.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [SchemaType(SchemaTypeEnum.Document)]
    [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "EH_ID", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='EH_ID' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
    [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "IT_Filename", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_Filename' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
    [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "IT_LastStatus", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_LastStatus' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
    [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.Boolean), "IT_ProdInd", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_ProdInd' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"boolean")]
    [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "IT_CC_SenderID", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_CC_SenderID' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
    [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "IT_MessageTrackingID", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_MessageTrackingID' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
    [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "IT_DeclarationContent", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_DeclarationContent' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
    [System.SerializableAttribute()]
    [SchemaRoots(new string[] {@"TypedPollingResultSet0", @"ArrayOfTypedPollingResultSet0", @"TypedPolling"})]
    public sealed class eHubTransactions_ITC_SWR_TriggerTypedPolling_ITCustomsSWRTrigger : Microsoft.XLANGs.BaseTypes.SchemaBase {
        
        [System.NonSerializedAttribute()]
        private static object _rawSchema;
        
        [System.NonSerializedAttribute()]
        private const string _strSchema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema xmlns=""http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger"" xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" xmlns:ns0=""https://CargoWise.eHub.Products.ITCustoms.Orchestrations.PollResponse.PropertySchema"" elementFormDefault=""qualified"" targetNamespace=""http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger"" version=""1.0"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:annotation>
    <xs:appinfo>
      <fileNameHint xmlns=""http://schemas.microsoft.com/servicemodel/adapters/metadata/xsd"">TypedPolling.ITCustomsSWRTrigger</fileNameHint>
      <b:schemaInfo is_envelope=""yes"" xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" />
    </xs:appinfo>
  </xs:annotation>
  <xs:complexType name=""TypedPollingResultSet0"">
    <xs:sequence>
      <xs:element minOccurs=""1"" maxOccurs=""1"" name=""EH_ID"" nillable=""true"" type=""xs:string"" />
      <xs:element minOccurs=""1"" maxOccurs=""1"" name=""IT_Filename"" nillable=""true"" type=""xs:string"" />
      <xs:element minOccurs=""1"" maxOccurs=""1"" default=""NULL"" name=""IT_LastStatus"" nillable=""true"" type=""xs:string"" />
      <xs:element minOccurs=""1"" maxOccurs=""1"" name=""IT_ProdInd"" nillable=""true"" type=""xs:boolean"" />
      <xs:element minOccurs=""1"" maxOccurs=""1"" name=""IT_CC_SenderID"" nillable=""true"" type=""xs:string"" />
      <xs:element minOccurs=""1"" maxOccurs=""1"" name=""IT_MessageTrackingID"" nillable=""true"" type=""xs:string"" />
      <xs:element minOccurs=""1"" maxOccurs=""1"" name=""IT_DeclarationContent"" nillable=""true"" type=""xs:string"" />
    </xs:sequence>
  </xs:complexType>
  <xs:element xmlns:q1=""http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger"" name=""TypedPollingResultSet0"" nillable=""true"" type=""q1:TypedPollingResultSet0"">
    <xs:annotation>
      <xs:appinfo>
        <b:properties>
          <b:property distinguished=""true"" xpath=""/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='EH_ID' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']"" />
          <b:property distinguished=""true"" xpath=""/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_Filename' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']"" />
          <b:property distinguished=""true"" xpath=""/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_LastStatus' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']"" />
          <b:property distinguished=""true"" xpath=""/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_ProdInd' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']"" />
          <b:property distinguished=""true"" xpath=""/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_CC_SenderID' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']"" />
          <b:property distinguished=""true"" xpath=""/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_MessageTrackingID' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']"" />
          <b:property distinguished=""true"" xpath=""/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_DeclarationContent' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']"" />
        </b:properties>
      </xs:appinfo>
    </xs:annotation>
  </xs:element>
  <xs:complexType name=""ArrayOfTypedPollingResultSet0"">
    <xs:sequence>
      <xs:element xmlns:q2=""http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger"" minOccurs=""0"" maxOccurs=""1"" name=""TypedPollingResultSet0"" type=""q2:TypedPollingResultSet0"" />
    </xs:sequence>
  </xs:complexType>
  <xs:element xmlns:q3=""http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger"" name=""ArrayOfTypedPollingResultSet0"" nillable=""true"" type=""q3:ArrayOfTypedPollingResultSet0"" />
  <xs:element name=""TypedPolling"">
    <xs:annotation>
      <xs:appinfo>
        <b:recordInfo body_xpath=""/*[local-name()='TypedPolling' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']"" />
      </xs:appinfo>
    </xs:annotation>
    <xs:complexType>
      <xs:sequence>
        <xs:element xmlns:q4=""http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger"" minOccurs=""0"" maxOccurs=""1"" name=""TypedPollingResultSet0"" nillable=""true"" type=""q4:ArrayOfTypedPollingResultSet0"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";
        
        public eHubTransactions_ITC_SWR_TriggerTypedPolling_ITCustomsSWRTrigger() {
        }
        
        public override string XmlContent {
            get {
                return _strSchema;
            }
        }
        
        public override string[] RootNodes {
            get {
                string[] _RootElements = new string [3];
                _RootElements[0] = "TypedPollingResultSet0";
                _RootElements[1] = "ArrayOfTypedPollingResultSet0";
                _RootElements[2] = "TypedPolling";
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
        
        [Schema(@"http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger",@"TypedPollingResultSet0")]
        [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "EH_ID", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='EH_ID' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
        [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "IT_Filename", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_Filename' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
        [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "IT_LastStatus", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_LastStatus' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
        [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.Boolean), "IT_ProdInd", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_ProdInd' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"boolean")]
        [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "IT_CC_SenderID", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_CC_SenderID' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
        [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "IT_MessageTrackingID", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_MessageTrackingID' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
        [Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute(typeof(System.String), "IT_DeclarationContent", XPath = @"/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='IT_DeclarationContent' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']", XsdType = @"string")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"TypedPollingResultSet0"})]
        public sealed class TypedPollingResultSet0 : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public TypedPollingResultSet0() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "TypedPollingResultSet0";
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
        
        [Schema(@"http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger",@"ArrayOfTypedPollingResultSet0")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"ArrayOfTypedPollingResultSet0"})]
        public sealed class ArrayOfTypedPollingResultSet0 : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public ArrayOfTypedPollingResultSet0() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "ArrayOfTypedPollingResultSet0";
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
        
        [Schema(@"http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger",@"TypedPolling")]
        [BodyXPath(@"/*[local-name()='TypedPolling' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ITCustomsSWRTrigger']")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"TypedPolling"})]
        public sealed class TypedPolling : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public TypedPolling() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "TypedPolling";
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
}
