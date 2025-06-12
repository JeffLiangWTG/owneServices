namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Schemas.APPLUS {
    using Microsoft.XLANGs.BaseTypes;
    
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.BizTalk.Schema.Compiler", "3.0.1.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [SchemaType(SchemaTypeEnum.Document)]
    [System.SerializableAttribute()]
    [SchemaRoots(new string[] {@"Request", @"Interchanges", @"MessageSet", @"Destinataire", @"Emetteur"})]
    [Microsoft.XLANGs.BaseTypes.SchemaReference(@"CargoWise.eHub.Products.ForwardingPortMessaging.FR.Schemas.APPLUS.patterns", typeof(global::CargoWise.eHub.Products.ForwardingPortMessaging.FR.Schemas.APPLUS.patterns))]
    public sealed class caed : Microsoft.XLANGs.BaseTypes.SchemaBase {
        
        [System.NonSerializedAttribute()]
        private static object _rawSchema;
        
        [System.NonSerializedAttribute()]
        private const string _strSchema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema xmlns=""http://www.cargowise.com/Schemas/FPM/APPLUS/CAED"" xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" xmlns:std=""http://www.cargowise.com/Schemas/FPM/APPLUS"" attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" targetNamespace=""http://www.cargowise.com/Schemas/FPM/APPLUS/CAED"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:import schemaLocation=""CargoWise.eHub.Products.ForwardingPortMessaging.FR.Schemas.APPLUS.patterns"" namespace=""http://www.cargowise.com/Schemas/FPM/APPLUS"" />
  <xs:annotation>
    <xs:appinfo>
      <references xmlns=""http://schemas.microsoft.com/BizTalk/2003"">
        <reference targetNamespace=""http://www.cargowise.com/Schemas/FPM/APPLUS"" />
      </references>
    </xs:appinfo>
  </xs:annotation>
  <xs:complexType name=""tiers_ctrl_type"">
    <xs:attribute name=""code"" type=""xs:string"" use=""required"" />
  </xs:complexType>
  <xs:complexType name=""lieux_ctrl_type"">
    <xs:attribute name=""md"" type=""xs:string"" use=""optional"" />
    <xs:attribute name=""bdd"" type=""xs:string"" use=""optional"" />
  </xs:complexType>
  <xs:complexType name=""lmarchandise_ctrl_type"">
    <xs:attribute name=""nb"" type=""xs:integer"" use=""required"" />
  </xs:complexType>
  <xs:complexType name=""equipement_ctrl_type"">
    <xs:attribute name=""id"" type=""xs:string"" use=""required"" />
  </xs:complexType>
  <xs:complexType name=""controle_prealable_type"">
    <xs:sequence>
      <xs:element name=""references-ctrl"" type=""references_ctrl_type"" />
      <xs:element minOccurs=""0"" name=""tiers-ctrl"" type=""tiers_ctrl_type"" />
      <xs:element minOccurs=""0"" name=""lieux-ctrl"" type=""lieux_ctrl_type"" />
      <xs:element minOccurs=""0"" name=""lmarchandise-ctrl"" type=""lmarchandise_ctrl_type"" />
      <xs:element minOccurs=""0"" maxOccurs=""unbounded"" name=""equipement-ctrl"" type=""equipement_ctrl_type"" />
      <xs:element minOccurs=""0"" name=""droits-ctrl"">
        <xs:complexType>
          <xs:complexContent mixed=""false"">
            <xs:extension base=""droits_sofi_type"" />
          </xs:complexContent>
        </xs:complexType>
      </xs:element>
    </xs:sequence>
  </xs:complexType>
  <xs:complexType name=""references_ctrl_type"">
    <xs:attribute name=""rca"" type=""xs:string"" use=""required"" />
    <xs:attribute name=""globale"" type=""std:Booleen_type"" use=""optional"" />
    <xs:attribute name=""type"" type=""xs:string"" use=""optional"" />
    <xs:attribute name=""dos"" type=""xs:string"" use=""optional"" />
  </xs:complexType>
  <xs:complexType name=""droits_sofi_type"">
    <xs:annotation>
      <xs:documentation>Droits indique le montant des droits de port dont font l_objet les marchandises faisant l_objet d_une avis d_enregistrement.</xs:documentation>
    </xs:annotation>
    <xs:attribute name=""montant"" use=""optional"">
      <xs:annotation>
        <xs:documentation>Valeur du montant des droits de port.</xs:documentation>
      </xs:annotation>
      <xs:simpleType>
        <xs:restriction base=""xs:integer"">
          <xs:minInclusive value=""0"" />
          <xs:maxInclusive value=""999999999"" />
        </xs:restriction>
      </xs:simpleType>
    </xs:attribute>
    <xs:attribute name=""unite"" use=""optional"">
      <xs:annotation>
        <xs:documentation>Unité monétaire dans laquelle sont exprimés les droits de port.</xs:documentation>
      </xs:annotation>
      <xs:simpleType>
        <xs:restriction base=""xs:string"">
          <xs:minLength value=""1"" />
          <xs:maxLength value=""3"" />
        </xs:restriction>
      </xs:simpleType>
    </xs:attribute>
    <xs:attribute name=""port"" type=""xs:string"" use=""optional"" />
    <xs:attribute name=""port-lib"" use=""optional"">
      <xs:simpleType>
        <xs:restriction base=""xs:string"">
          <xs:maxLength value=""17"" />
        </xs:restriction>
      </xs:simpleType>
    </xs:attribute>
  </xs:complexType>
  <xs:complexType name=""caed_user_type"">
    <xs:attribute name=""user"" use=""required"">
      <xs:annotation>
        <xs:documentation>Code utilisateur du tiers profession du message.</xs:documentation>
      </xs:annotation>
      <xs:simpleType>
        <xs:restriction base=""xs:string"">
          <xs:minLength value=""1"" />
          <xs:maxLength value=""9"" />
        </xs:restriction>
      </xs:simpleType>
    </xs:attribute>
    <xs:attribute name=""tiersProf"" use=""required"">
      <xs:annotation>
        <xs:documentation>Identifiant tiers profession du message.</xs:documentation>
      </xs:annotation>
      <xs:simpleType>
        <xs:restriction base=""xs:string"">
          <xs:minLength value=""1"" />
          <xs:maxLength value=""9"" />
        </xs:restriction>
      </xs:simpleType>
    </xs:attribute>
    <xs:attribute name=""email"" type=""xs:string"" use=""optional"" />
    <xs:attribute name=""adresse"" type=""xs:string"" use=""optional"" />
    <xs:attribute name=""reseau"" type=""xs:string"" use=""optional"" />
  </xs:complexType>
  <xs:complexType name=""caed_request_type"">
    <xs:choice>
      <xs:element name=""controle-prealable"" type=""controle_prealable_type"" />
    </xs:choice>
    <xs:attribute name=""action"" use=""optional"">
      <xs:annotation>
        <xs:documentation>Code fonction du document (Codification SIC).</xs:documentation>
      </xs:annotation>
      <xs:simpleType>
        <xs:restriction base=""xs:string"">
          <xs:pattern value=""CREATE|UPDATE|DELETE|ADD|ERASE|UKN"" />
        </xs:restriction>
      </xs:simpleType>
    </xs:attribute>
    <xs:attribute name=""id"" use=""required"">
      <xs:annotation>
        <xs:documentation>Identifiant fonctionnel du message utilisé pour effectuer le suivi de l intégration.</xs:documentation>
      </xs:annotation>
      <xs:simpleType>
        <xs:restriction base=""xs:string"">
          <xs:minLength value=""1"" />
          <xs:maxLength value=""75"" />
        </xs:restriction>
      </xs:simpleType>
    </xs:attribute>
    <xs:attribute name=""type"" use=""required"">
      <xs:annotation>
        <xs:documentation>Type du document à intégrer (Codification SIC).</xs:documentation>
      </xs:annotation>
      <xs:simpleType>
        <xs:restriction base=""xs:string"">
          <xs:minLength value=""0"" />
          <xs:maxLength value=""7"" />
        </xs:restriction>
      </xs:simpleType>
    </xs:attribute>
    <xs:attribute name=""statut"" use=""optional"">
      <xs:simpleType>
        <xs:restriction base=""xs:string"" />
      </xs:simpleType>
    </xs:attribute>
  </xs:complexType>
  <xs:element name=""Request"" type=""caed_request_type"">
    <xs:annotation>
      <xs:documentation>Request précise la nature du document à intégrer.</xs:documentation>
    </xs:annotation>
  </xs:element>
  <xs:element name=""Interchanges"">
    <xs:annotation>
      <xs:documentation>Interchanges décrit le contexte de la transmission des données entre le SIC et le système partenaire.</xs:documentation>
    </xs:annotation>
    <xs:complexType>
      <xs:sequence>
        <xs:element maxOccurs=""unbounded"" ref=""MessageSet"" />
      </xs:sequence>
      <xs:attribute name=""id"" use=""required"">
        <xs:annotation>
          <xs:documentation>Identifiant interchange</xs:documentation>
        </xs:annotation>
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:minLength value=""1"" />
            <xs:maxLength value=""14"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:attribute>
      <xs:attribute name=""from"" use=""required"">
        <xs:annotation>
          <xs:documentation>Nom du système émetteur</xs:documentation>
        </xs:annotation>
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:minLength value=""1"" />
            <xs:maxLength value=""75"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:attribute>
      <xs:attribute name=""to"" use=""required"">
        <xs:annotation>
          <xs:documentation>Nom du système destinataire</xs:documentation>
        </xs:annotation>
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:minLength value=""1"" />
            <xs:maxLength value=""75"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:attribute>
      <xs:attribute name=""servername"" type=""xs:string"" use=""optional"" />
    </xs:complexType>
  </xs:element>
  <xs:element name=""MessageSet"">
    <xs:annotation>
      <xs:documentation>MessageSet identifie le message et permet d'assurer le suivi des échanges.</xs:documentation>
    </xs:annotation>
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""Destinataire"" />
        <xs:element ref=""Emetteur"" />
        <xs:element name=""Messages"" type=""caed_messages_type"">
          <xs:annotation>
            <xs:documentation>Messages contient le document à integrer, à notifier, à imprimer ou à acquitter</xs:documentation>
          </xs:annotation>
        </xs:element>
      </xs:sequence>
      <xs:attribute name=""id"" use=""required"">
        <xs:annotation>
          <xs:documentation>Identifiant du message</xs:documentation>
        </xs:annotation>
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:minLength value=""1"" />
            <xs:maxLength value=""14"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:attribute>
      <xs:attribute name=""icid"" use=""required"">
        <xs:annotation>
          <xs:documentation>Identifiant interchange de rattachement du message</xs:documentation>
        </xs:annotation>
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:minLength value=""1"" />
            <xs:maxLength value=""14"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:attribute>
      <xs:attribute name=""date"" type=""std:DateLongue_type"" use=""required"">
        <xs:annotation>
          <xs:documentation>""31/12/2004 23:59:59"" Date d'envoi du message.</xs:documentation>
        </xs:annotation>
      </xs:attribute>
    </xs:complexType>
  </xs:element>
  <xs:element name=""Destinataire"" type=""caed_user_type"">
    <xs:annotation>
      <xs:documentation>Identification du tiers destinataire du message</xs:documentation>
    </xs:annotation>
  </xs:element>
  <xs:element name=""Emetteur"" type=""caed_user_type"">
    <xs:annotation>
      <xs:documentation>Identification de l'émetteur du message</xs:documentation>
    </xs:annotation>
  </xs:element>
  <xs:complexType name=""caed_messages_type"">
    <xs:choice maxOccurs=""unbounded"">
      <xs:element ref=""Request"" />
    </xs:choice>
  </xs:complexType>
</xs:schema>";
        
        public caed() {
        }
        
        public override string XmlContent {
            get {
                return _strSchema;
            }
        }
        
        public override string[] RootNodes {
            get {
                string[] _RootElements = new string [5];
                _RootElements[0] = "Request";
                _RootElements[1] = "Interchanges";
                _RootElements[2] = "MessageSet";
                _RootElements[3] = "Destinataire";
                _RootElements[4] = "Emetteur";
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
        
        [Schema(@"http://www.cargowise.com/Schemas/FPM/APPLUS/CAED",@"Request")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"Request"})]
        public sealed class Request : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public Request() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "Request";
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
        
        [Schema(@"http://www.cargowise.com/Schemas/FPM/APPLUS/CAED",@"Interchanges")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"Interchanges"})]
        public sealed class Interchanges : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public Interchanges() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "Interchanges";
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
        
        [Schema(@"http://www.cargowise.com/Schemas/FPM/APPLUS/CAED",@"MessageSet")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"MessageSet"})]
        public sealed class MessageSet : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public MessageSet() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "MessageSet";
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
        
        [Schema(@"http://www.cargowise.com/Schemas/FPM/APPLUS/CAED",@"Destinataire")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"Destinataire"})]
        public sealed class Destinataire : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public Destinataire() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "Destinataire";
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
        
        [Schema(@"http://www.cargowise.com/Schemas/FPM/APPLUS/CAED",@"Emetteur")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"Emetteur"})]
        public sealed class Emetteur : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public Emetteur() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "Emetteur";
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
