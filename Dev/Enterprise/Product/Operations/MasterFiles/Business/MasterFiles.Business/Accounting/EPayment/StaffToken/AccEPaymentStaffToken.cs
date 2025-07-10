using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccEPaymentStaffTokenLookups;

namespace Enterprise.MasterFiles.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Consumed here, wrapped in nameof()")]
	enum DBOperationType
	{
		Insert,
		Update
	}

	public class AccEPaymentStaffToken : AutoAccEPaymentStaffToken
	{
		const string ClientIDForProduction = "4X04vlVSUTfF7HVKaO2WdyctbxGY0Sy6";
		const string ClientIDForNonProduction = "xwFiSN389IiPKBdqXROEyUFpcG3w6lM6";

		public AccEPaymentStaffToken(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TK_Scope = ScopeCodes.Payments; // It seems Payments scope is the only one we need for CW1 user's everyday use.
			TK_RequestedUtc = ZDateTime.UtcNow;
			TK_GC = GlbCompany.CurrentCompany.PK;
		}

		public void ResetToPendingStatus()
		{
			TK_Status = StatusCodes.Pending;
			TK_RequestedUtc = ZDateTime.UtcNow;
			TK_ExpiryUtc = ZDateTime.Empty;
			TK_ErrorDescription = ZString.Empty;
		}

		public string PrepareOAuthURL()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var dbOperationType = TK_AccountName.IsEmpty ? nameof(DBOperationType.Insert) : nameof(DBOperationType.Update);
			var useTestEhubGateway = !Env.Instance.IsProductionSystem && eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.Value;
			string ofxLoginURL = AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.Value;
			var isUsingProductionURL = AccountingMasterFilesRegistry.Instance.OFXOAuthWebURLEnvironment.Value;
			string state = string.Format("{0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}",
				registrationKey.EnterpriseCode + registrationKey.ServerCode,
				Company.GC_Code,
				TK_GS_NKStaffCode,
				BankAccount.AB_Code,
				TK_Scope,
				dbOperationType,
				useTestEhubGateway ? bool.TrueString : bool.FalseString,
				isUsingProductionURL ? bool.TrueString : bool.FalseString);

			var secret = (new AESCrypto()).DecryptStringAES(AccountingMasterFilesRegistry.Instance.OFXEncryptionKey.Value, AESCrypto.RANDOM_SHAREDSECRET);
			var encryptedState = (new AESCrypto()).EncryptStringAES(state, secret);
			string responseType = (NoResString)"code";
			string callbackURL = AccountingMasterFilesRegistry.Instance.WTCCallbackSiteWebURL.Value;
			var clientID = isUsingProductionURL ? ClientIDForProduction : ClientIDForNonProduction;
			string authorizeURL = string.Format((NoResString)"{0}?response_type={1}&client_id={2}&state={3}&scope={4}&redirect_uri={5}",
									ofxLoginURL,
									responseType,
									clientID,
									encryptedState,
									TK_Scope,
									callbackURL);
			return authorizeURL;
		}

		public bool IsAuthorised => TK_Status == StatusCodes.Authorised;

		[List("Lookups.StatusCodeList")]
		[ReadOnly(true)]
		[ResourceStringData("664B7995-C740-4E4E-9916-5DB83403DC75", Caption = "Status")]
		public override ZString TK_Status { get => base.TK_Status; set => base.TK_Status = value; }

		[List("Lookups.ScopeList")]
		[ResourceStringData("996ECCF0-2AB1-46F1-BA2D-1D91573C7C0A", Caption = "Scope")]
		public override ZString TK_Scope { get => base.TK_Scope; set => base.TK_Scope = value; }

		[ReadOnly(true)]
		[ResourceStringData("2FDBA135-E4C5-4D1A-86A7-EB303E7EED6E", Caption = "Account Name", ShortCaption = "Account")]
		public override ZString TK_AccountName { get => base.TK_AccountName; set => base.TK_AccountName = value; }

		[ReadOnly(true)]
		[ResourceStringData("7AA78117-0821-4EFB-9D95-FE5FFF4A854F", Caption = "Authorization Requested (UTC)", ShortCaption = "Requested (UTC)")]
		public override ZDateTime TK_RequestedUtc { get => base.TK_RequestedUtc; set => base.TK_RequestedUtc = value; }

		[ReadOnly(true)]
		[ResourceStringData("E44B9EEC-5580-4BAA-8976-12448FF105CB", Caption = "Authorization Expiry (UTC)", ShortCaption = "Expiry (UTC)")]
		public override ZDateTime TK_ExpiryUtc { get => base.TK_ExpiryUtc; set => base.TK_ExpiryUtc = value; }

		[ReadOnly(true)]
		[ResourceStringData("C17C858C-2C4D-4F17-B3C7-542A89CEC4A1", Caption = "Error Message", ShortCaption = "Error")]
		public override ZString TK_ErrorDescription { get => base.TK_ErrorDescription; set => base.TK_ErrorDescription = value; }

		[ReadOnly(true)]
		[ResourceStringData("A2A15E77-EEF7-4C24-B8D3-707C44FA1F11", Caption = "Status")]
		public ZString Status => Lookups.StatusCodeList.GetDescriptionFromCode(TK_Status);

		[ReadOnlyMember(nameof(IsRecordInDatabase))]
		[ResourceStringData("8D5C26D8-24D4-4970-BA84-CCD18410DB1C", Caption = "Staff Code", ShortCaption = "Code")]
		public override ZString TK_GS_NKStaffCode { get => base.TK_GS_NKStaffCode; set => base.TK_GS_NKStaffCode = value; }

		bool IsRecordInDatabase => IsInDatabase;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			TK_GC = GlbCompany.CurrentCompany.PK;
		}
#endif
	}
}
