using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.DataModel.Business.Validation
{
    [eHubPortalValidationProvider("ITCustomsAccount", typeof(eHubClientSystemRegistration))]
    public class eHubClientSystemRegistration_IT_Validation : eHubClientSystemRegistration_Default_Validation
    {
        [StringLength(250, ErrorMessage = "AccountNumber must be less than 250 characters")]
        public override string CD_Attr1 { get; set; }

        [StringLength(250, ErrorMessage = "Password cannot be more than 250 characters")]
        public override string CD_Attr2 { get; set; }

        [EnumDataType(typeof(ITCustomsAccountStatus), ErrorMessage = @"Status can be either ""0=Invalid"", ""1=Valid"", ""2=Unknown""")]
        public virtual string CD_Flag1 { get; set; }

        [Required(ErrorMessage = "Node is required")]
        [StringLength(250, ErrorMessage = "Node cannot be more than 250 characters")]
        public override string CD_Code { get; set; }

        enum ITCustomsAccountStatus
        {
            Invalid = 0,
            Valid = 1,
            Unknown = 2
        }
    }

    [eHubPortalValidationProvider("JPCustomsAccount_SystemLevel", typeof(eHubClientSystemRegistration))]
    public class eHubClientSystemRegistration_JP_Validation : eHubClientSystemRegistration_Default_Validation
    {
		[StringLength(250, ErrorMessage = "Password must be between 4 and 250 characters")]
        public override string CD_Attr1 { get; set; }

        [EnumDataType(typeof(JPCustomsAccountStatus), ErrorMessage = @"Flag1 can be only ""0=NotApplicable""")]
        public virtual string CD_Flag1 { get; set; }

        [Required(ErrorMessage = "UserName is required")]
        [StringLength(250, ErrorMessage = "UserName cannot be more than 250 characters")]
        public override string CD_Code { get; set; }

        enum JPCustomsAccountStatus
        {
            NotApplicable = 0
        }
    }

    [eHubPortalValidationProvider("JPCustomsAccountClientLevel", typeof(eHubClientRegistration))]
    public class eHubClientRegistration_JP_Validation : eHubClientSystemRegistration_Default_Validation
    {
		[Required(ErrorMessage = "PK is required")]
		public virtual System.Guid CX_PK { get; set; }

		[Required(ErrorMessage = "Please select a Client")]
		public virtual System.Guid CX_CC { get; set; }

		[Required(ErrorMessage = "Password is required")]
		public virtual string CX_Password1 { get; set; }

		[Required(ErrorMessage = "eHubID is required")]
		[StringLength(250, ErrorMessage = "eHubID cannot be more than 250 characters")]
		public virtual string CX_Code { get; set; }
	}

	[eHubPortalValidationProvider("TWCustomsAccount", typeof(eHubClientRegistration))]
	public class eHubClientRegistration_TW_Validation
	{
		[Required(ErrorMessage = "PK is required")]
		public virtual System.Guid CX_PK { get; set; }

		[Required(ErrorMessage = "Company is required")]
		public virtual System.Guid CX_CC { get; set; }

		[Required(ErrorMessage = "Staff|MailBox is required")]
		[RegularExpression(@"(.*?)\|(.*)", ErrorMessage = "Please provide a value with this format: {Staff}|{MailBox}")]
		public virtual string CX_Qualifier { get; set; }

		[EnumDataType(typeof(TWCustomsReceiveAutomatically), ErrorMessage = @"ReceiveAutomatically can be either ""0=False"", ""1=True""")]
		public virtual string CX_Flag1 { get; set; }

		enum TWCustomsReceiveAutomatically
		{
			False = 0,
			True = 1
		}
	}

    [eHubPortalValidationProvider("TWCustomsForwarderAndLicensing", typeof(eHubClientRegistration))]
    public class eHubClientRegistration_TWForwarderManifest_Validation
    {
        [Required(ErrorMessage = "PK is required")]
        public virtual System.Guid CX_PK { get; set; }

        [Required(ErrorMessage = "Company is required")]
        public virtual System.Guid CX_CC { get; set; }

        [Required(ErrorMessage = "MailBox is required")]
        public virtual string CX_Qualifier { get; set; }

        [EnumDataType(typeof(TWCustomsReceiveAutomatically), ErrorMessage = @"ReceiveAutomatically can be either ""0=False"", ""1=True""")]
        public virtual string CX_Flag1 { get; set; }

        enum TWCustomsReceiveAutomatically
        {
            False = 0,
            True = 1
        }
    }

    [eHubPortalValidationProvider("NEXDOC_Client", typeof(eHubClientSystemRegistration))]
    public class eHubClientSystemRegistration_AU_Validation : eHubClientSystemRegistration_Default_Validation
    {
		[Required(ErrorMessage = "StaffCode is required")]
		public override string CD_Qualifier { get; set; }

		[EnumDataType(typeof(AUCustomsAccountStatus), ErrorMessage = @"Status can be either ""0=INV"", ""1=VAL"", or ""2=UNK""")]
        public virtual string CD_Flag1 { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(250, ErrorMessage = "Password cannot be more than 250 characters")]
        public override string CD_Code { get; set; }

        enum AUCustomsAccountStatus
        {
            INV = 0,
            VAL = 1,
            UNK = 2
        }
    }

    [eHubPortalValidationProvider("SGCustomsAccount", typeof(eHubClientSystemRegistration))]
    public class eHubClientSystemRegistration_SG_Validation : eHubClientSystemRegistration_Default_Validation
    {
        [Required(ErrorMessage = "AccountID is required")]
        [StringLength(250, MinimumLength = 4, ErrorMessage = "AccountID must be between 4 and 250 characters")]
        public override string CD_Attr1 { get; set; }

        [StringLength(250, ErrorMessage = "Password cannot be more than 250 characters")]
        public override string CD_Attr2 { get; set; }

        [EnumDataType(typeof(SGCustomsAccountStatus), ErrorMessage = @"Status can be either ""0=Invalid"" or ""1=Valid""")]
        public virtual string CD_Flag1 { get; set; }

        [Required(ErrorMessage = "BrokerID is required")]
        [StringLength(250, ErrorMessage = "BrokerID cannot be more than 250 characters")]
        public override string CD_Code { get; set; }

        enum SGCustomsAccountStatus
        {
            Invalid = 0,
            Valid = 1
        }
    }

    public class eHubClientSystemRegistration_Default_Validation
    {
        [Required(ErrorMessage = "PK is required")]
        public virtual System.Guid CD_PK { get; set; }

        [Required(ErrorMessage = "Please select a client system")]
        public virtual System.Guid CD_EH { get; set; }

		[StringLength(250, ErrorMessage = "Qualifier cannot be more than 250 characters")]
		public virtual string CD_Qualifier { get; set; }

		[Required(ErrorMessage = "Code is required")]
        [StringLength(250, ErrorMessage = "Code cannot be more than 250 characters")]
        public virtual string CD_Code { get; set; }

        [StringLength(250, ErrorMessage = "Attribute1 cannot be more than 250 characters")]
        public virtual string CD_Attr1 { get; set; }

        [StringLength(250, ErrorMessage = "Attribute2 cannot be more than 250 characters")]
        public virtual string CD_Attr2 { get; set; }
    }

    [eHubPortalValidationProvider("CARGOSMART", typeof(eHubClientRegistration))]
	[eHubPortalValidationProvider("GTNEXUS", typeof(eHubClientRegistration))]
	public class eHubClientRegistration_OCM_WithCarriers_Validation
	{
		[Required(ErrorMessage = "PK is required")]
		public virtual System.Guid CX_PK { get; set; }

		[Required(ErrorMessage = "Please select a Client")]
		public virtual System.Guid CX_CC { get; set; }

		[StringLength(250, ErrorMessage = "EventBranch cannot be more than 250 characters")]
		public virtual string CX_Qualifier { get; set; }

		[Required(ErrorMessage = "eHubID is required")]
		[StringLength(250, ErrorMessage = "eHubID cannot be more than 250 characters")]
		public virtual string CX_Code { get; set; }

		[StringLength(250, ErrorMessage = "Carriers cannot be more than 250 characters")]
		public virtual string CX_Attr1 { get; set; }
	}

	[eHubPortalValidationProvider("CAROTRANS", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("CMACGM", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("COSCO", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("ECULINE", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("EVERGREEN", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("HAPAG", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("INTTRA", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("MAERSK", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("MOL", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("NYK", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("PORTBASE", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("PORTRIX", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("SMLINE", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("Softship", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("TNPA", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("WWA", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("YANGMING", typeof(eHubClientRegistration))]
    public class eHubClientRegistration_OCM_Validation
    {
        [Required(ErrorMessage = "PK is required")]
        public virtual System.Guid CX_PK { get; set; }

        [Required(ErrorMessage = "Please select a Client")]
        public virtual System.Guid CX_CC { get; set; }

        [StringLength(250, ErrorMessage = "EventBranch cannot be more than 250 characters")]
        public virtual string CX_Qualifier { get; set; }

        [Required(ErrorMessage = "eHubID is required")]
        [StringLength(250, ErrorMessage = "eHubID cannot be more than 250 characters")]
        public virtual string CX_Code { get; set; }
    }

    [eHubPortalValidationProvider("GLSHK", typeof(eHubClientRegistration))]
	[eHubPortalValidationProvider("GLSHK_Inbound", typeof(eHubClientRegistration))]
    [eHubPortalValidationProvider("HKC", typeof(eHubClientRegistration))]
    public class eHubClientRegistration_GLSHK_Validation
    {
        [Required(ErrorMessage = "Registration Type is required")]
        public virtual System.Guid CX_RT { get; set; }

        [Required(ErrorMessage = "PK is required")]
        public virtual System.Guid CX_PK { get; set; }

        [Required(ErrorMessage = "Please select a Client")]
        public virtual System.Guid CX_CC { get; set; }

        [StringLength(250, ErrorMessage = "Branch cannot be more than 250 characters")]
        public virtual string CX_Qualifier { get; set; }

        [EnumDataType(typeof(GLSHKStatus), ErrorMessage = @"Status can be either ""0=INV"", ""1=VAL""")]
        public virtual string CX_Flag2 { get; set; }

        [EnumDataType(typeof(GLSHKLastUpdatedFrom), ErrorMessage = @"Last Updated From can be either ""0=CW1"", ""1=eHubPortal""")]
        public virtual string CX_Flag1 { get; set; }

        [Required(ErrorMessage = "PIMA is required")]
        [StringLength(250, ErrorMessage = "PIMA cannot be more than 250 characters")]
        public virtual string CX_Code { get; set; }

		[RemoteValidation("UniqueAttr1", "ClientRegistrations", ErrorMessage = "FTP URI should be unique", AdditionalFields = "CX_RT,CX_PK")]
		[StringLength(250, ErrorMessage = "FTP URI cannot be more than 250 characters")]
        [RegularExpression(Constants.RegExFTP, ErrorMessage = "Please provide a valid FTP URI")]
        public virtual string CX_Attr1 { get; set; }

        [StringLength(250, ErrorMessage = "FTP Password cannot be more than 250 characters")]
        public virtual string CX_Password1 { get; set; }

        enum GLSHKLastUpdatedFrom
        {
            CW1 = 0,
            eHubPortal = 1,
        }

        enum GLSHKStatus
        {
            Invalid = 0,
            Valid = 1
        }
    }

	[eHubPortalValidationProvider("GBCustoms-Transport", typeof(eHubClientRegistration))]
	public class eHubClientRegistration_GBCustomsTransport_Validation
	{
		[Required(ErrorMessage = "Registration Type is required")]
		public virtual System.Guid CX_RT { get; set; }

		[Required(ErrorMessage = "PK is required")]
		public virtual System.Guid CX_PK { get; set; }

		[Required(ErrorMessage = "Please select a Client")]
		public virtual System.Guid CX_CC { get; set; }

		[Required(ErrorMessage = "Service must be provided")]
		public virtual string CX_Qualifier { get; set; }

		[Required(ErrorMessage = "Path must be provided")]
		[StringLength(250, ErrorMessage = "Path cannot be more than 250 characters")]
		[RegularExpression(Constants.RegExPath, ErrorMessage = "Path must start with 'XMLBody:','Header:','JSONBody:', or 'Parameter:'")]
        public virtual string CX_Code { get; set; }

		[Required(ErrorMessage = "Name must be provided")]
		public virtual string CX_Attr1 { get; set; }

        [EnumDataType(typeof(Source), ErrorMessage = @"Status can be either ""0=Synchronous"", ""1=Notification""")]
		public virtual string CX_Flag1 { get; set; }

		[EnumDataType(typeof(Type), ErrorMessage = @"Direction can be either ""0=Subscribed"", ""1=Additional""")]
		public virtual string CX_Flag2 { get; set; }

		enum Source
		{
			Synchronous = 0,
			Notification = 1
		}

		enum Type
		{
			Subscribed = 0,
			Additional = 1
		}
	}

    [eHubPortalValidationProvider("GBCustoms-ICSNI", typeof(eHubClientSystemRegistration))]
    public class eHubClientSystemRegistration_GBCustomsICSNI_Validation : eHubClientSystemRegistration_Default_Validation
    {
        [EnumDataType(typeof(GBCustomsICSNIStatus), ErrorMessage = @"Status can be either ""0=Active"" or ""255=Inactive""")]
        public virtual string CD_Flag1 { get; set; }

        [Required(ErrorMessage = "Active Subscription is required")]
        [EnumDataType(typeof(GBCustomsICSNIActiveSubscription), ErrorMessage = @"Active Subscription can be either ""0=False"" or ""1=True""")]
        public override string CD_Attr1 { get; set; }

        [Required(ErrorMessage = "Declarant is required")]
        [StringLength(50, ErrorMessage = "Declarant cannot be more than 50 characters")]
        [RegularExpression(@"(.*?)\-(.*)", ErrorMessage = "Please provide a value with this format: EORI-Branch")]
        public override string CD_Code { get; set; }

        enum GBCustomsICSNIStatus
        {
            Active = 0,
            Inactive = 255
        }

        enum GBCustomsICSNIActiveSubscription
        {
            False = 0,
            True = 1
        }
    }

    [eHubPortalValidationProvider("CSFTP", typeof(eHubClientRegistration))]
	public class eHubClientRegistration_CSFTP_Validation
	{
		[Required(ErrorMessage = "Registration Type is required")]
		public virtual System.Guid CX_RT { get; set; }

		[Required(ErrorMessage = "PK is required")]
		public virtual System.Guid CX_PK { get; set; }

		[Required(ErrorMessage = "Please select a Client")]
		public virtual System.Guid CX_CC { get; set; }

		[EnumDataType(typeof(CSFTPStatus), ErrorMessage = @"Status can be either ""0=Invalid"", ""1=Valid""")]
		public virtual string CX_Flag2 { get; set; }

		[EnumDataType(typeof(CSFTPDirection), ErrorMessage = @"Direction can be either ""IN=0"", ""OUT=1""")]
		public virtual string CX_Flag1 { get; set; }

		[Required(ErrorMessage = "Provider is required")]
		[StringLength(250, ErrorMessage = "Provider cannot be more than 250 characters")]
		public virtual string CX_Code { get; set; }

		[StringLength(250, ErrorMessage = "FTP URI cannot be more than 250 characters")]
		[RegularExpression(Constants.RegExFTP, ErrorMessage = "Please provide a valid FTP URI")]
		public virtual string CX_Qualifier { get; set; }

		[StringLength(250, ErrorMessage = "FTP Password cannot be more than 250 characters")]
		public virtual string CX_Password1 { get; set; }
		
		enum CSFTPStatus
		{
			Invalid = 0,
			Valid = 1
		}

		enum CSFTPDirection
		{
			IN = 0,
			OUT = 1
		}
	}

	[eHubPortalValidationProvider("Default", typeof(eHubAsyncPollingRegistration))]
	[Bind(Exclude = "PR_XML")]
	public class eHubAsyncPollingRegistration_Default_Validation
	{
		[Required(ErrorMessage = "PK is required")]
		public virtual System.Guid PR_PK { get; set; }

		[Required(ErrorMessage = "Registration Type is required")]
		public virtual System.Guid PR_RT { get; set; }
	}

	[eHubPortalValidationProvider("ACAS_BRProtocol", typeof(eHubAsyncPollingRegistration))]
	[Bind(Exclude = "PR_XML")]
	public class eHubAsyncPollingRegistration_Validation : eHubAsyncPollingRegistration_Default_Validation
	{
		[Required(ErrorMessage = "PK is required")]
		public override System.Guid PR_PK { get; set; }

		[Required(ErrorMessage = "Registration Type is required")]
		public override System.Guid PR_RT { get; set; }
	}
}
