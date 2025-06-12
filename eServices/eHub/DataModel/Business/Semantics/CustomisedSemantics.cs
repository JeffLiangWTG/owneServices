using System.Collections.Generic;

namespace CargoWise.eHub.DataModel.Business.Semantics
{
    [eHubPortalSemanticsProvider("ITCustomsAccount")]
    public class ITCustomsSemantics
    {
        public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
        {
			{"CD_RT", "Registration Type"},
			{"CD_EH_ID", "Client System"},
            {"CD_Code", "Node"},
            {"CD_Attr1", "AccountNumber"},
            {"CD_Attr2", "Password"},
			{"CD_ConfigXml", "ConfigXml"},
			{"CD_IssuedUTC", "IssuedUTC"},
			{"CD_ExpiryUTC", "ExpiryUTC"},
            {"CD_Flag1", "Status"},
			{"CD_Qualifier", "Qualifier"}
		};
    }

	[ClientSystemRegistrationStatusAttribute("ITCustomsAccount")]
	public enum ITCustomsAccountState
	{
		Invalid = 0,
		Valid = 1,
		Unknown = 2
	}

    [eHubPortalSemanticsProvider("JPCustomsAccount_SystemLevel")]
    public class JPCustomsAccount_SystemLevelSemantics
	{
        public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
        {
			{"CD_RT", "Registration Type"},
			{"CD_EH_ID", "Client System"},
            {"CD_Code", "UserName"},
            {"CD_Attr1", "Password"},
			{"CD_Attr2", "Attribute 2"},
			{"CD_Flag1", "Flag 1"},
			{"CD_Qualifier", "Qualifier"},
			{"CD_ConfigXml", "ConfigXml"},
			{"CD_IssuedUTC", "IssuedUTC"},
			{"CD_ExpiryUTC", "ExpiryUTC"}
        };
    }

    [eHubPortalSemanticsProvider("JPCustomsAccount_ClientLevel")]
    public class JPCustomsAccount_ClientLevelSemantics
	{
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Client System"},
            {"CX_Code", "UserName"},
            {"CX_Password1", "Password"}
        };
    }

	public class XPathCustomValue
	{
		public string XPath { get; set; }
		public bool IsPasswordValue { get; set; }
		public bool IsHidden { get; set; }
	}

	[eHubPortalSemanticsProvider("TWCustomsAccount")]
    public class TWCustomsSemantics
    {
	    public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
	    {
		    {"CX_CC_ID", "Company"},
		    {"CX_Qualifier", "Staff|MailBox"},
		    {"CX_Flag1", "ReceiveAutomatically"},
			{"CustomValue1", "ClientPassword"},
		    {"CustomValue2", "CertificatePassword"},
		    {"CX_ConfigXml", "ConfigXml"},

		};

	    public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
	    {
		    {0, "False"},
		    {1, "True"}
	    };

	    public static  XPathCustomValue CustomValue1 = new XPathCustomValue()
	    {
		    XPath = "//*[local-name()='Group'][@Type='MailBoxID']/*[local-name()='Credential']/*[local-name()='Password']",
		    IsPasswordValue = true,
		    IsHidden = true
	    };

	    public static  XPathCustomValue CustomValue2 = new XPathCustomValue()
	    {
		    XPath = "//*[local-name()='Group'][@Type='MailBoxID']/*[local-name()='Certificate']/*[local-name()='Passphrase']",
		    IsPasswordValue = true,
		    IsHidden = true
	    };
	}

    [eHubPortalSemanticsProvider("TWCustomsForwarderAndLicensing")]
    public class TWForwarderManifestSemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Company"},
            {"CX_Qualifier", "MailBox"},
            {"CX_Flag1", "ReceiveAutomatically"},
            {"CustomValue1", "ClientPassword"},
            {"CustomValue2", "CertificatePassword"},
            {"CX_ConfigXml", "ConfigXml"}
        };

        public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
        {
            {0, "False"},
            {1, "True"}
        };

        public static XPathCustomValue CustomValue1 = new XPathCustomValue()
        {
            XPath = "//*[local-name()='Group'][@Type='MailBoxID']/*[local-name()='Credential']/*[local-name()='Password']",
            IsPasswordValue = true,
            IsHidden = true
        };

        public static XPathCustomValue CustomValue2 = new XPathCustomValue()
        {
            XPath = "//*[local-name()='Group'][@Type='MailBoxID']/*[local-name()='Certificate']/*[local-name()='Passphrase']",
            IsPasswordValue = true,
            IsHidden = true
        };
    }

    [eHubPortalSemanticsProvider("TWCustomsNCATK")]
	public class TWCustomsNCATKSemantics
	{
		public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
		{
			{"CX_CC_ID", "ClientID"},
			{"CX_Code", "NCATK ClientID"}
		};
	}

	[eHubPortalSemanticsProvider("CARGOWISE")]
	public class CW1Semantics
	{
		public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
		{
			{"CX_CC_ID", "eHub ID"},
			{"CX_Qualifier", "SCAC/C1C"},
			{"CX_Flag1", "Party Type"}
		};

		public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
		{
			{0, "ShippingLine"},
			{1, "NVOCC"}
		};
	}

	[eHubPortalSemanticsProvider("CNCustomsSW")]
    public class CNCustomsSWSemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "ClientID"},
            {"CX_Code", "SW ClientID"},
			{"CX_Qualifier", "Branch"}
		};
    }

    [eHubPortalSemanticsProvider("GEI_IN_AuthenticationClientLevel")]
    public class GEI_IN_AuthenticationClientLevelSemantics
	{
	    public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
	    {
		    {"CX_CC_ID", "ClientID"},
		    {"CX_Qualifier", "Branch"},
		    {"CX_Flag1", "Status"},
		    {"CustomValue1", "Password"},
		    {"CustomValue2", "Client Secret"},
		    {"CX_ConfigXml", "ConfigXml"}
	    };

	    public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
	    {
		    {0, "Valid"},
		    {1, "Invalid"}
	    };

	    public static XPathCustomValue CustomValue1 = new XPathCustomValue()
	    {
			XPath = "//*[local-name()='Credential' and @Name='Taxpayer']/*[local-name()='Password']",
			IsPasswordValue = true,
		    IsHidden = true
	    };

	    public static XPathCustomValue CustomValue2 = new XPathCustomValue()
	    {
		    XPath = "//*[local-name()='Credential' and @Name='Service-Provider']/*[local-name()='Password']",
		    IsPasswordValue = true,
		    IsHidden = true
	    };
	}

    [eHubPortalSemanticsProvider("GEI_IN_AuthenticationSystemLevel")]
    public class GEI_IN_AuthenticationSystemLevel
    {
	    public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
	    {
			{"CD_RT", "Registration Type"},
			{"CD_EH_ID", "Client System"},
		    {"CD_ConfigXml", "ConfigXml"},
		    {"CustomValue1", "Client Secret"},
			{"CD_Code", "Code"},
            {"CD_Attr1", "Attribute 1"},
            {"CD_Attr2", "Attribute 2"},
            {"CD_Flag1", "Flag 1"},
			{"CD_IssuedUTC", "IssuedUTC"},
			{"CD_ExpiryUTC", "ExpiryUTC"},
			{"CD_Qualifier", "CD Qualifier"}
		};

	    public static XPathCustomValue CustomValue1 = new XPathCustomValue()
	    {
		    XPath = "//*[local-name()='Credential'][@Name='Service-Provider']/*[local-name()='Password']",
		    IsPasswordValue = true,
		    IsHidden = true
	    };
    }

	[eHubPortalSemanticsProvider("GEI_IN_Token")]
    public class GEI_IN_Token
	{
	    public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
	    {
		    {"CX_CC_ID", "ClientID"},
		    {"CX_Qualifier", "Branch"},
		    {"CX_Flag1", "Status"},
		    {"CX_ConfigXml", "ConfigXml"}
	    };

	    public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
	    {
		    { 0, "Valid"},
		    { 1, "Expired"}
	    };
    }

    [eHubPortalSemanticsProvider("NEXDOC_Client")]
    public class AUCustomsSemantics
    {
        public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
        {
			{"CD_RT", "Registration Type"},
            {"CD_EH_ID", "Client System"},
            {"CD_Code", "Password"},
            {"CD_Flag1", "Status"},
            {"CD_Qualifier", "StaffCode"},
            {"CD_Attr1", "Attribute 1"},
            {"CD_Attr2", "Attribute 2"},
			{"CD_ConfigXml", "ConfigXml"},
			{"CD_IssuedUTC", "IssuedUTC"},
			{"CD_ExpiryUTC", "ExpiryUTC"}
        };
    }

    [ClientSystemRegistrationStatusAttribute("NEXDOC_Client")]
    public enum AUCustomsState
    {
        Invalid = 0,
        Valid = 1
    }

    [eHubPortalSemanticsProvider("GBCustomsAuthorisationToken")]
    public class GBCustomsAuthorisationSemantics
    {
        public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
        {
            {"CD_EH_ID", "Client System"},
            {"CD_Code", "EORI Badge"},
            {"CD_Attr1", "Authorisation Token"},
            {"CD_Attr2", "Redirect URI"},
            {"CD_Flag1", "Token State"},
			{"CD_Qualifier", "Qualifier"},
			{"CD_IssuedUTC", "Issued UTC" },
            {"CD_ExpiryUTC", "Expiry UTC"},
			{"CD_ConfigXml", "ConfigXml"}
        };
    }

    [ClientSystemRegistrationStatusAttribute("GBCustomsAuthorisationToken")]
    public enum GBCustomsAuthorisationTokenState
    {
        Valid = 0,
        RequestingAccessToken = 1,
        SucceededToRequestAccessToken = 2,
        FailedToRequestAccessToken = 255
    }

    [eHubPortalSemanticsProvider("GBCustomsAccessToken")]
    public class GBCustomsAccessSemantics
    {
        public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
        {
			{"CD_RT", "Registration Type"},
            {"CD_EH_ID", "Client System"},
            {"CD_Code", "EORI Badge"},
            {"CD_Attr1", "Access Token"},
            {"CD_Attr2", "Refresh Token"},
            {"CD_Flag1", "Token State"},
			{"CD_Qualifier", "Qualifier"},
			{"CD_IssuedUTC", "Issued UTC" },
            {"CD_ExpiryUTC", "Expiry UTC"},
			{"CD_ConfigXml", "ConfigXml"}
        };
    }

    [ClientSystemRegistrationStatusAttribute("GBCustomsAccessToken")]
    public enum GBCustomsAccessTokenState
    {
        Valid = 0,
        RejectedAndRequiringRefresh = 1,
        RefreshingDueToRejection = 2,
        RefreshingDueToBeingCloseToExpiry = 3,
        RefreshFailed = 255
    }

    [eHubPortalSemanticsProvider("GBCustoms-Direct")]
    public class GBCustomsSemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Provider"},
            {"CX_Qualifier", "Type"},
            {"CX_Code", "Authorisation"}
        };
    }

    [eHubPortalSemanticsProvider("GBCustoms-EMCS")]
    public class GBCustomsEMCS
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Client"},
            {"CX_ConfigXml", "ConfigXML"},
            {"CX_Flag1", "Should poll"}
        };
		public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
		{
			{ 0, "No"},
			{ 1, "Yes"}
		};
	}

    [eHubPortalSemanticsProvider("GBCustoms-MCP")]
    public class GBCustomsMCP
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Provider"},
            {"CX_Attr1", "URL"},
            {"CX_Code", "Authorisation"}
        };
    }

    [eHubPortalSemanticsProvider("GBCustoms-Pentant")]
    public class GBCustomsPentant
    {
	    public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
	    {
		    {"CX_CC_ID", "Provider"},
		    {"CX_Attr1", "URL"},
		    {"CX_Code", "Authorisation"}
	    };
    }

    [eHubPortalSemanticsProvider("GBCustoms-MCPAccount")]
    public class GBCustomMCPAccountSemantics
    {
        public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
        {
			{"CD_RT", "Registration Type"},
            {"CD_EH_ID", "Client System"},
            {"CD_Code", "Badge"},
            {"CD_Attr1", "Topic"},
			{"CD_Attr2", "Attribute 2"},
			{"CD_Flag1", "Token State"},
			{"CD_Qualifier", "Qualifier"},
			{"CD_ConfigXml", "ConfigXml"},
			{"CD_IssuedUTC", "IssuedUTC"},
			{"CD_ExpiryUTC", "ExpiryUTC"}
        };
    }

    [ClientSystemRegistrationStatusAttribute("GBCustoms-MCPAccount")]
    public enum GBCustomMCPAccountRegistrationState
    {
        BadgeSuccessfullyRegistered = 0,
        BadgeIsBeingRegistered = 1,
        BadgeRegistrationFailedRetryWithNextMessage = 2,
        BadgeRegistrationRequestedManually = 3
    }

    [eHubPortalSemanticsProvider("GBCustoms-CNS")]
    public class GBCustomsCNS
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Provider"},
            {"CX_Attr1", "URL"},
            {"CX_Code", "Authorisation"}
        };
    }

    [eHubPortalSemanticsProvider("GBCustoms-CNSAccount")]
    public class GBCustomCNSAccountSemantics
    {
        public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
        {
			{"CD_RT", "Registration Type"},
            {"CD_EH_ID", "Client System"},
            {"CD_Code", "Badge"},
            {"CD_Attr1", "Topic"},
            {"CD_Flag1", "Token State"},
            {"CD_Attr2", "Attribute 2"},
			{"CD_Qualifier", "Qualifier"},
			{"CD_ConfigXml", "ConfigXml"},
			{"CD_IssuedUTC", "IssuedUTC"},
			{"CD_ExpiryUTC", "ExpiryUTC"}
        };
    }

    [ClientSystemRegistrationStatusAttribute("GBCustoms-CNSAccount")]
    public enum GBCustomCNSAccountRegistrationState
    {
        BadgeSuccessfullyRegistered = 0,
        BadgeIsBeingRegistered = 1,
        BadgeRegistrationFailedRetryWithNextMessage = 2,
        BadgeRegistrationRequestedManually = 3
    }

    [eHubPortalSemanticsProvider("GBCustoms-Transport")]
    public class GBCustomsTransport
    {
	    public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
	    {
		    {"CX_CC_ID", "Provider"},
		    {"CX_Qualifier","Service" },
		    {"CX_Code", "Path"},
		    {"CX_Attr1", "Name"},
		    {"CX_Flag1", "Source"},
		    {"CX_Flag2", "Type"},
        };

	    public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
	    {
		    {0, "Synchronous"},
		    {1, "Notification"}
	    };

	    public static readonly Dictionary<int, string> Flag2Options = new Dictionary<int, string>
	    {
		    {0, "Subscribed"},
		    {1, "Additional"},
            {2, "Data"}
	    };
    }

	[eHubPortalSemanticsProvider("SOGET")]
	public class SOGETSemantics
	{
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Client"},
            {"CX_Code", "Code"},
            {"CX_Qualifier", "Qualifier"},
            {"CX_Attr1", "Attribute 1"},
            {"CX_Password1", "Password 1"}
        };

        public static readonly Dictionary<string, string> Attr1Options = new Dictionary<string, string>
		{
			{"SOGET", "SOGET"},
			{"MGI", "MGI"}
		};
	}

    [eHubPortalSemanticsProvider("MGI")]
    public class MGISemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Client"},
            {"CX_Code", "Code"},
            {"CX_Qualifier", "Qualifier"},
            {"CX_Attr1", "Attribute 1"},
            {"CX_Password1", "Password 1"}
        };

        public static readonly Dictionary<string, string> Attr1Options = new Dictionary<string, string>
        {
            {"MGI", "MGI"},
            {"SOGET", "SOGET"}
        };
    }

    [eHubPortalSemanticsProvider("GBCustoms-ICSNI")]
    public class GBCustomsICSNI
    {
        public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
        {
			{"CD_RT", "Registration Type"},
			{"CD_EH_ID", "Client System"},
            {"CD_Code", "Declarant"},
            {"CD_Flag1", "Status"},
            {"CD_Attr1", "Active Subscription"},
			{"CD_Qualifier", "Qualifier"},
			{"CD_IssuedUTC", "Issued UTC"},
            {"CD_ConfigXml", "Config Xml"},
            {"CD_Attr2", "Attribute 2"},
			{"CD_ExpiryUTC", "ExpiryUTC"}
		};

        public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
        {
            {0, "False"},
            {1, "True"}
        };
    }

    [ClientSystemRegistrationStatusAttribute("GBCustoms-ICSNI")]
    public enum GBCustomsICSNIStatus
    {
        Active = 0,
        Inactive = 255
    }

    [eHubPortalSemanticsProvider("ACAS_BRToken")]
    public class ACASBRAuthorisationToken
    {
	    public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
	    {
			{"CD_RT", "Registration Type"},
		    {"CD_EH_ID", "Client System"},
		    {"CD_Code", "Code"},
		    {"CD_Attr1", "Attribute 1"},
		    {"CD_Attr2", "Attribute 2"},
		    {"CD_Flag1", "Flag 1"},
			{"CD_Qualifier", "Qualifier"},
			{"CD_ConfigXml", "ConfigXml"},
			{"CD_IssuedUTC", "IssuedUTC"},
			{"CD_ExpiryUTC", "ExpiryUTC"}
	    };
    }

    [eHubPortalSemanticsProvider("ACAS_BRProtocol")]
    public class ACAS_BR_AsyncPollingRegistration
	{
	    public static readonly Dictionary<string, string> AsyncPollingRegistrationHeaders = new Dictionary<string, string>
	    {
		    {"PR_EH_ID", "Client System"},
		    {"PR_Text", "Staff"},
		    {"PR_XML", "XML"},
		    {"PR_CC_ID", "Client"},
		    {"PR_CreatedUTC", "Created UTC"}
	    };
    }

    [ClientSystemRegistrationStatusAttribute("ACAS_BRToken")]
    public enum ACAS_BRTokenRegistrationState
	{
		Expired = 0,
		Valid = 1
	}

	[eHubPortalSemanticsProvider("GLSHK")]
	[eHubPortalSemanticsProvider("GLSHK_Inbound")]
	[eHubPortalSemanticsProvider("GLSHK_Inbound_TST")]
	[eHubPortalSemanticsProvider("HKC")]
    public class GLSHKSemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "eHubID"},
            {"CX_Code", "PIMA"},
            {"CX_Qualifier", "Branch"},
            {"CX_Attr1", "FTP URI"},
            {"CX_Password1", "FTP Password"},
            {"CX_Flag1", "Last Updated From"},
            {"CX_Flag2", "Status"}
        };

        public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
        {
            {0, "CW1"},
            {1, "eHubPortal"}
        };

        public static readonly Dictionary<int, string> Flag2Options = new Dictionary<int, string>
        {
            {0, "Invalid"},
            {1, "Valid"}
        };

        public static readonly Dictionary<EncodeOptions, string> EncodeSettings = new Dictionary<EncodeOptions, string>
        {
            {EncodeOptions.EncodePasswordRSA, "Encode Password"},
            {EncodeOptions.DecodePasswordRSA, "Decode Password"}
        };
    }

    [eHubPortalSemanticsProvider("NXPORTAPI")]
    public class NXPORTAPISemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Client"},
            {"CX_Qualifier", "Branch Code"},
            {"CX_Code", "Code"},
            {"CX_Attr1", "Username"},
            {"CX_Password1", "Password"},
            {"CX_Flag1", "Account Status"}
        };

        public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
        {
            {0, "Invalid"},
            {1, "Valid"}
        };

        public static readonly Dictionary<EncodeOptions, string> EncodeSettings = new Dictionary<EncodeOptions, string>
        {
            {EncodeOptions.EncodePasswordRSA, "Encode Password"},
            {EncodeOptions.DecodePasswordRSA, "Decode Password"}
        };
    }

    public enum EncodeOptions
    {
        EncodePasswordBase64 = 0,
        DecodePasswordForEditBase64 = 1,
        EncodePasswordSHA256 = 2,
        EncodePasswordRSA = 3,
        DecodePasswordRSA = 4,
        OriginalPassword = 5
    }

    [eHubPortalSemanticsProvider("SGCustomsAccount")]
    public class SGCustomsSemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Client System"},
            {"CX_Qualifier", "BrokerID"},
            {"CX_Code", "AccountID"},
            {"CX_Password1", "Password"},
            {"CX_Flag1", "Status"}
        };

        public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
        {
            {0, "Invalid"},
            {1, "Valid"}
        };
    }

    [eHubPortalSemanticsProvider("CSFTP")]
    public class CSFTPSemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "eHubID"},
            {"CX_Code", "Provider"},
            {"CX_Qualifier", "FTP URI"},
            {"CX_Password1", "FTP Password"},
            {"CX_Flag1", "Direction"},
            {"CX_Flag2", "Status"}
        };

        public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
        {
            {0, "IN"},
            {1, "OUT"}
        };

        public static readonly Dictionary<int, string> Flag2Options = new Dictionary<int, string>
        {
            {0, "Invalid"},
            {1, "Valid"}
        };
    }

    [eHubPortalSemanticsProvider("Tradevan")]
    public class TradevanSemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "eHubID"},
            {"CX_Qualifier", "Branch"},
            {"CX_Code", "PIMA"},
            {"CX_Attr1", "FTP URI"},
            {"CX_Password1", "FTP Password"},
            {"CX_Flag1", "Last Updated From"},
            {"CX_Flag2", "Status"}
        };

        public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
        {
            {0, "CW1"},
            {1, "eHubPortal"}
        };

        public static readonly Dictionary<int, string> Flag2Options = new Dictionary<int, string>
        {
            {0, "Invalid"},
            {1, "Valid"}
        };

        public static readonly Dictionary<EncodeOptions, string> EncodeSettings = new Dictionary<EncodeOptions, string>
        {
            {EncodeOptions.EncodePasswordRSA, "Encode Password"},
            {EncodeOptions.DecodePasswordRSA, "Decode Password"}
        };
    }

    [eHubPortalSemanticsProvider("CargoStart")]
    public class CargoStartSemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "eHubID"},
            {"CX_Qualifier", "Branch"},
            {"CX_Code", "PIMA"},
            {"CX_Attr1", "FTP URI"},
            {"CX_Password1", "FTP Password"},
            {"CX_Flag1", "Last Updated From"},
            {"CX_Flag2", "Status"}
        };

        public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
        {
            {0, "CW1"},
            {1, "eHubPortal"}
        };

        public static readonly Dictionary<int, string> Flag2Options = new Dictionary<int, string>
        {
            {0, "Invalid"},
            {1, "Valid"}
        };

        public static readonly Dictionary<EncodeOptions, string> EncodeSettings = new Dictionary<EncodeOptions, string>
        {
            {EncodeOptions.EncodePasswordRSA, "Encode Password"},
            {EncodeOptions.DecodePasswordRSA, "Decode Password"}
        };
    }

	[eHubPortalSemanticsProvider("CCSJ_FWB_FTP")]
	[eHubPortalSemanticsProvider("CCSJ_FHL_FTP")]
	[eHubPortalSemanticsProvider("CCSJ_Inbound")]
    public class CCSJSemantics
    {
	    public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
	    {
		    {"CX_CC_ID", "eHubID"},
		    {"CX_Qualifier", "Branch"},
		    {"CX_Code", "PIMA"},
		    {"CX_Attr1", "FTP URI"},
		    {"CX_Password1", "FTP Password"},
		    {"CX_Flag1", "Last Updated From"},
		    {"CX_Flag2", "Status"}
	    };

	    public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
	    {
		    {0, "CW1"},
		    {1, "eHubPortal"}
	    };

	    public static readonly Dictionary<int, string> Flag2Options = new Dictionary<int, string>
	    {
		    {0, "Invalid"},
		    {1, "Valid"}
	    };

	    public static readonly Dictionary<EncodeOptions, string> EncodeSettings = new Dictionary<EncodeOptions, string>
	    {
		    {EncodeOptions.EncodePasswordRSA, "Encode Password"},
		    {EncodeOptions.DecodePasswordRSA, "Decode Password"}
	    };
    }

    [eHubPortalSemanticsProvider("ARINC_SP")]
    [eHubPortalSemanticsProvider("ARINC_SPTest")]
    [eHubPortalSemanticsProvider("Qatar")]
    [eHubPortalSemanticsProvider("Qatar_Test")]
    [eHubPortalSemanticsProvider("Nallian")]
    [eHubPortalSemanticsProvider("Nallian_Test")]
    public class AirServiceProviderMappingSemantics
    {
        public static readonly Dictionary<string, string> AirServiceProviderMappingFields = new Dictionary<string, string>
        {
            {"RecipientAddress", "enabled" },
            {"ClientPIMA", "enabled" },
            {"DoubleSignatureCode", "enabled" }
        };
    }
    
	[eHubPortalSemanticsProvider("CARGOSMART")]
	[eHubPortalSemanticsProvider("GTNEXUS")]
	public class OCMSemanticsWithCarriers
	{
		public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
		{
			{"CX_CC_ID", "eHubID"},
			{"CX_Code", "ID"},
			{"CX_Qualifier", "EventBranch"},
			{"CX_Attr1", "Carriers"},
		};
	}

	[eHubPortalSemanticsProvider("CAROTRANS")]
	[eHubPortalSemanticsProvider("CMACGM")]
    [eHubPortalSemanticsProvider("COSCO")]
    [eHubPortalSemanticsProvider("ECULINE")]
    [eHubPortalSemanticsProvider("EVERGREEN")]
    [eHubPortalSemanticsProvider("HAPAG")]
    [eHubPortalSemanticsProvider("INTTRA")]
    [eHubPortalSemanticsProvider("MAERSK")]
    [eHubPortalSemanticsProvider("MOL")]
    [eHubPortalSemanticsProvider("NYK")]
    [eHubPortalSemanticsProvider("PORTBASE")]
    [eHubPortalSemanticsProvider("PORTRIX")]
    [eHubPortalSemanticsProvider("SMLINE")]
    [eHubPortalSemanticsProvider("Softship")]
    [eHubPortalSemanticsProvider("TNPA")]
    [eHubPortalSemanticsProvider("WWA")]
    [eHubPortalSemanticsProvider("YANGMING")]
	[eHubPortalSemanticsProvider("ACAS_US")]
    [eHubPortalSemanticsProvider("ACAS_BR")]
	public class OCMSemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "eHubID"},
            {"CX_Code", "ID"},
            {"CX_Qualifier", "EventBranch"}
        };
    }

    [eHubPortalSemanticsProvider("Default")]
    public class DefaultSemantics
    {
        public static readonly Dictionary<string, string> ClientRegistrationHeaders = new Dictionary<string, string>
        {
            {"CX_CC_ID", "Client"},
            {"CX_Code", "Code"},
            {"CX_Qualifier", "Qualifier"},
            {"CX_Attr1", "Attribute 1"},
            {"CX_Password1", "Password 1"},
            {"CX_Flag1", "Flag 1"},
            {"CX_Flag2", "Flag 2"},
			{"CX_ConfigXml", "ConfigXml"},
			{"CX_IssuedUTC", "IssuedUTC"},
			{"CX_ExpiryUTC", "ExpiryUTC"}
        };

		public static readonly Dictionary<string, string> ClientSystemRegistrationHeaders = new Dictionary<string, string>
        {
			{"CD_RT", "Registration Type"},
            {"CD_EH_ID", "Client System"},
            {"CD_Code", "Code"},
            {"CD_Attr1", "Attribute 1"},
            {"CD_Attr2", "Attribute 2"},
            {"CD_Flag1", "Flag 1"},
			{"CD_Qualifier", "Qualifier"},
			{"CD_ConfigXml", "ConfigXml"},
			{"CD_IssuedUTC", "IssuedUTC"},
			{"CD_ExpiryUTC", "ExpiryUTC"}
        };

		public static readonly Dictionary<string, string> AsyncPollingRegistrationHeaders = new Dictionary<string, string>
		{
			{"PR_EH_ID", "Client System"},
			{"PR_Text", "Text"},
			{"PR_XML", "XML"},
			{"PR_CC_ID", "Client"},
			{"PR_CreatedUTC", "Created UTC"}
		};

        public static readonly Dictionary<int, string> Flag1Options = new Dictionary<int, string>
        {
            {0, "Not Applicable"}
        };

        public static readonly Dictionary<int, string> Flag2Options = new Dictionary<int, string>
        {
            {0, "Not Applicable"}
        };
    }
}
