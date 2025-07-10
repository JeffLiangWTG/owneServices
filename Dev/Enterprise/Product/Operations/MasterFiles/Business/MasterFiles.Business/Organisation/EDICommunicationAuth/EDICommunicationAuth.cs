using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationAuth : AutoEDICommunicationAuth, IEDIClientAuth
	{
		public const string ScopesDelimiter = ", ";
		const string AuthorizationEndpoint = "https://login.microsoftonline.com/{0}/v2.0";
		const string Module = "eAdaptor";
		public const string CaRoot = "EAP";

		public EDICommunicationAuth(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		[List("Lookups.AuthModesList")]
		public override ZString ECA_AuthorizationMode
		{
			get { return base.ECA_AuthorizationMode; }
			set { base.ECA_AuthorizationMode = value; }
		}

		[List("Lookups.GrantTypesList")]
		public override ZString ECA_FlowCode
		{
			get { return base.ECA_FlowCode; }
			set { base.ECA_FlowCode = value; }
		}

		public EDICommunicationPartyConfig Config
		{
			get
			{
				if (config == null)
				{
					var relatedConfigsQuery = new ZQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, PK);
					config = Factory.Load<EDICommunicationPartyConfig>(relatedConfigsQuery).FirstOrDefault();
				}
				return config;
			}
		}
		EDICommunicationPartyConfig config;

		public string CachingKey => PK.ToString();

		public string AuthorizationMode => ECA_AuthorizationMode;

		public string AuthorizationURL => ECA_AuthorizationEndpoint;

		public string ClientID => ECA_ClientID;

		public string ClientSecret => ECA_ClientSecret;

		public string Username => ECA_Username;

		public string Password => ECA_Password;

		public string PrivateKey => ECA_EncodedPrivateKey != null ? Encoding.Unicode.GetString(TwoWayEncoder.Decrypt(ECA_EncodedPrivateKey)) : string.Empty;

		public string Certificate => ECA_Certificate != null ? Encoding.Unicode.GetString(ECA_Certificate) : string.Empty;

		public IEnumerable<string> Scopes => ECA_Scopes.Split(ScopesDelimiter).Select(p => p.ToString());

		public string InboundScope => Scopes.FirstOrDefault();

		public string FlowCode => ECA_FlowCode;

		ZBool isVerified;

		public ZBool IsVerified
		{
			get => isVerified;
			set
			{
				SetNonPersistentPropertyValue(IsVerifiedInfo, ref isVerified, value);
			}
		}

		public ZPropertyInfo IsVerifiedInfo
		{
			get { return GetZPropertyInfo(nameof(IsVerified)); }
		}

		public TwoWayEncoder TwoWayEncoder => TwoWayEncoder.NewWithStandardInitialisationVector();

		#if DEBUG
		public
		#endif
		void SetPrivateKey(string privateKeyPem)
		{
			base.ECA_RenewalEncodedPrivateKey = TwoWayEncoder.Encrypt(Encoding.Unicode.GetBytes(privateKeyPem));
			Factory.Save();
		}

		public string SetPrivateKeyAndCsr(string ediClientName)
		{
			var (privateKeyPem, csrPem) = EDIClientOutboundCsrGenerator.GeneratePrivateKeyAndCsr(ediClientName);
			SetPrivateKey(privateKeyPem);
			return csrPem;
		}

		public override ZBlob ECA_EncodedPrivateKey
		{
			get
			{
				return base.ECA_EncodedPrivateKey;
			}
		}

		public void SetCertificate(string certificatePem, string ediClientName, string csrSubject = "")
		{
			if (!EDIClientOutboundCsrGenerator.IsCertificateValid(certificatePem))
			{
				throw new Exception(Res.GetString("EDICommunicationParty|ErrorMessages|CertificateNotValid", "Certificate is not valid."));
			}
			var privateKey = Encoding.Unicode.GetString(TwoWayEncoder.Decrypt(base.ECA_RenewalEncodedPrivateKey));
			if (!EDIClientOutboundCsrGenerator.IsCertificateMatchingPrivateKey(certificatePem, privateKey))
			{
				throw new Exception(Res.GetString("EDICommunicationParty|ErrorMessages|CertificateDoesNotMatchPrivateKey", "Certificate does not match the Private Key."));
			}
			if (!EDIClientOutboundCsrGenerator.IsCertificateMatchingCsrSubject(certificatePem, ediClientName, csrSubject))
			{
				throw new Exception(Res.GetString("EDICommunicationParty|ErrorMessages|CertificateDoesNotMatchCsr", "Certificate subject information does not match the CSR."));
			}
			base.ECA_EncodedPrivateKey = base.ECA_RenewalEncodedPrivateKey;
			base.ECA_Certificate = Encoding.Unicode.GetBytes(certificatePem);
		}

		public override ZBlob ECA_Certificate
		{
			get
			{
				return base.ECA_Certificate;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate;
		}

		public void RegisterCertificate(string csrPem, string partyName)
		{
			if (string.IsNullOrEmpty(ECA_OperationId))
			{
				var result = ObjectFactory.Get<ICertificateManager>();
				ECA_OperationId = result.RegisterCertificate(csrPem, Module, partyName, CaRoot);
				Factory.Save();
				return;
			}
			if (!string.IsNullOrEmpty(ECA_ClientID) && string.IsNullOrEmpty(ECA_RenewalOperationId))
			{
				var result = ObjectFactory.Get<ICertificateManager>();
				ECA_RenewalOperationId = result.RolloverCertificate(ECA_ClientID, csrPem, CaRoot);
				Factory.Save();
				return;
			}
			throw new InvalidOperationException("A certificate renewal cannot be issued while another one is already in progress.");
		}

		public void SetInboundCertificate(string tenantId, string clientId, ZBlob certificate, string renewalOperationId)
		{
			if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || certificate.IsEmpty)
			{
				return;
			}
			ECA_AuthorizationEndpoint = string.Format(AuthorizationEndpoint, tenantId);
			ECA_ClientID = clientId;
			ECA_Certificate = certificate;
			ECA_Scopes = $"{clientId}/.default";
			if (!string.IsNullOrEmpty(renewalOperationId))
			{
				ECA_OperationId = renewalOperationId;
				ECA_RenewalOperationId = string.Empty;
			}
			Factory.Save();
		}

		public void RevertFailedCertificateRegistration()
		{
			if (!string.IsNullOrEmpty(ECA_RenewalOperationId))
			{
				ECA_RenewalOperationId = string.Empty;
			}
			else if (!string.IsNullOrEmpty(ECA_OperationId) && string.IsNullOrEmpty(ECA_ClientID))
			{
				ECA_OperationId = string.Empty;
			}

			Factory.Save();
		}

		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			var baseResult = base.GetStrategies();

			return baseResult.Concat(new IBusinessObjectStrategy[]
				{
					new ValidateAuth(),
					new ResetNonUsedFieldsByMode()
				}).ToArray();
		}

		class ValidateAuth : IBusinessObjectStrategy
		{
			public void BeforeSuccessfulDelete(BusinessObject businessObject)
			{
			}

			public DeleteDetails DeleteDetails(BusinessObject businessObject)
			{
				return null;
			}

			public void FetchForLoad(BusinessObject businessObject)
			{
			}

			public void OnDelete(BusinessObject businessObject)
			{
			}

			public void OnFactorySaved(BusinessObject businessObject, bool saveSucceeded)
			{
			}

			public void OnFactorySaving(BusinessObject businessObject)
			{
				var auth = (EDICommunicationAuth)businessObject;
				var conf = CheckUniquenessAndGetConfig(auth);
				if (conf != null && conf.IsActive && conf.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Inbound)
				{
					if (auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.BasicAuthentication)
					{
						CheckUsernameUniquenessOnInboundConfigWithBasicAuthentication(auth, conf);
					}
					if (auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.OAuthAuthentication && !auth.ECA_ClientID.IsEmpty)
					{
						CheckClientIDUniquenessOnInboundConfigWithOAuthAuthentication(auth, conf);
					}
				}
			}

			EDICommunicationPartyConfig CheckUniquenessAndGetConfig(EDICommunicationAuth auth)
			{
				var relatedConfigsQuery = new ZQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, auth.PK);
				var relatedConfigs = auth.Factory.Load<EDICommunicationPartyConfig>(relatedConfigsQuery);

				if (relatedConfigs.Length > 1)
				{
					throw new ZCannotSaveException(Res.GetString("EDICommunicationParty|ErrorMessages|DuplicateIConfigsUsingTheSameAuthConfig", "Cannot have two configurations with authentication using the same authentication configuration. Please choose a different authentication configuration."), "Saving Failed");
				}

				return relatedConfigs.Length == 0 ? null : relatedConfigs[0];
			}

			ZDBOnlyQuery GenerateQueryForLookupOtherConfig(EDICommunicationPartyConfig conf)
			{
				var query = new ZDBOnlyQuery(typeof(EDICommunicationPartyConfig));
				query.AddToFilter(EDICommunicationPartyConfigSchema.ECC_Direction, EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
				query.AddToFilter(EDICommunicationPartyConfigSchema.PK, SQLComparisonOperator.NotEqual, conf.PK);
				query.TableHints = TableHints.UPDLOCK | TableHints.ROWLOCK;
				return query;
			}

			ZDBOnlySubQuery GenerateSubQueryForLookupAuth()
			{
				var subQuery = new ZDBOnlySubQuery(typeof(EDICommunicationAuth), EDICommunicationAuthSchema.PK);
				subQuery.TableHints = TableHints.UPDLOCK | TableHints.ROWLOCK;
				return subQuery;
			}

			void CheckUsernameUniquenessOnInboundConfigWithBasicAuthentication(EDICommunicationAuth auth, EDICommunicationPartyConfig conf)
			{
				var query = GenerateQueryForLookupOtherConfig(conf);
				var subQuery = GenerateSubQueryForLookupAuth();
				subQuery.AddToFilter(EDICommunicationAuthSchema.ECA_AuthorizationMode, EDICommunicationAuthModesList.Codes.BasicAuthentication);
				subQuery.AddToFilter(EDICommunicationAuthSchema.ECA_Username, auth.ECA_Username);
				query.AddSubQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, subQuery, JoinCondition.And);

				if (auth.Factory.LoadTop1<EDICommunicationPartyConfig>(query) != null)
				{
					throw new ZCannotSaveException(Res.GetString("EDICommunicationParty|ErrorMessages|DuplicateInboundConfigsWithBasicAuthAndTheSameUsername", "Cannot have two inbound configurations with basic authentication using the same username. Please choose another username."), "Saving Failed");
				}
			}

			void CheckClientIDUniquenessOnInboundConfigWithOAuthAuthentication(EDICommunicationAuth auth, EDICommunicationPartyConfig conf)
			{
				var query = GenerateQueryForLookupOtherConfig(conf);
				var subQuery = GenerateSubQueryForLookupAuth();
				subQuery.AddToFilter(EDICommunicationAuthSchema.ECA_AuthorizationMode, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
				subQuery.AddToFilter(EDICommunicationAuthSchema.ECA_ClientID, auth.ECA_ClientID);
				query.AddSubQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, subQuery, JoinCondition.And);

				var dbAuth = auth.Factory.LoadTop1<EDICommunicationPartyConfig>(query);

				if (dbAuth != null)
				{
					throw new ZCannotSaveException(Res.GetString("EDICommunicationParty|ErrorMessages|DuplicateInboundConfigsWithOAuthAuthAndTheSameClientID", "Cannot have two inbound configurations with OAuth authentication using the same Client ID. Please choose another Client ID."), "Saving Failed");
				}
			}

			public void OnSaved(BusinessObject businessObject, bool saveSucceeded)
			{
			}

			public void OnSaveRollback(BusinessObject businessObject)
			{
			}

			public void OnSaving(BusinessObject businessObject)
			{
			}

			public void OnSavingInObjectsWithLateChanges(BusinessObject businessObject)
			{
			}
		}

		class ResetNonUsedFieldsByMode : IBusinessObjectStrategy
		{
			public void BeforeSuccessfulDelete(BusinessObject businessObject)
			{
			}

			public DeleteDetails DeleteDetails(BusinessObject businessObject)
			{
				return null;
			}

			public void FetchForLoad(BusinessObject businessObject)
			{
			}

			public void OnDelete(BusinessObject businessObject)
			{
			}

			public void OnFactorySaved(BusinessObject businessObject, bool saveSucceeded)
			{
			}

			public void OnFactorySaving(BusinessObject businessObject)
			{
			}

			public void OnSaved(BusinessObject businessObject, bool saveSucceeded)
			{
			}

			public void OnSaveRollback(BusinessObject businessObject)
			{
			}

			void ResetUserPasswordFields(EDICommunicationAuth auth)
			{
				auth.ECA_Username = string.Empty;
				auth.ECA_Password = string.Empty;
			}

			void ResetClientCertificateFields(EDICommunicationAuth auth)
			{
				auth.ECA_Certificate = ZBlob.Empty;
				auth.ECA_EncodedPrivateKey = ZBlob.Empty;
				auth.ECA_OperationId = string.Empty;
				auth.ECA_RenewalEncodedPrivateKey = ZBlob.Empty;
				auth.ECA_RenewalOperationId = string.Empty;
			}

			void ResetClientCredentialsFields(EDICommunicationAuth auth)
			{
				auth.ECA_ClientSecret = string.Empty;
			}

			void ResetOutBoundOAuthCommonFields(EDICommunicationAuth auth)
			{
				auth.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate;
				auth.ECA_Scopes = string.Empty;
			}

			void ResetOAuthCommonFields(EDICommunicationAuth auth)
			{
				auth.ECA_ClientID = string.Empty;
				auth.ECA_AuthorizationEndpoint = string.Empty;
				ResetOutBoundOAuthCommonFields(auth);
			}

			void ResetOAuthFields(EDICommunicationAuth auth)
			{
				ResetOAuthCommonFields(auth);
				ResetClientCredentialsFields(auth);
				ResetClientCertificateFields(auth);
			}

			public void OnSaving(BusinessObject businessObject)
			{
				var auth = (EDICommunicationAuth)businessObject;

				if (auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.BasicAuthentication)
				{
					ResetOAuthFields(auth);
				}
				if (auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.NoAuthentication)
				{
					ResetOAuthFields(auth);
					ResetUserPasswordFields(auth);
				}
				if (auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.OAuthAuthentication && auth.Config != null && auth.Config.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Inbound)
				{
					ResetUserPasswordFields(auth);
				}
				if (auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.OAuthAuthentication && auth.Config != null && auth.Config.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Outbound)
				{
					if (auth.ECA_FlowCode == EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate)
					{
						ResetUserPasswordFields(auth);
						ResetClientCredentialsFields(auth);
					}
					if (auth.ECA_FlowCode == EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials)
					{
						ResetUserPasswordFields(auth);
						ResetClientCertificateFields(auth);
					}
					if (auth.ECA_FlowCode == EDICommunicationAuthOutboundGrantTypesList.Codes.Password)
					{
						ResetClientCertificateFields(auth);
					}
				}
			}

			public void OnSavingInObjectsWithLateChanges(BusinessObject businessObject)
			{
			}
		}

		[ChildEditable]
		public OAuth2ScopesCollection FormScopes
		{
			get
			{
				if (formScopes == null)
				{
					formScopes = new OAuth2ScopesCollection(Factory);
					RegisterEditableChildObject(formScopes);
					var scopes = ECA_Scopes.Split(ScopesDelimiter);
					foreach (var scope in scopes)
					{
						var oAuth2Scope = new OAuth2Scope();
						oAuth2Scope.ScopeName = scope;
						formScopes.Add(oAuth2Scope);
					}
					((IBindingList)formScopes).ListChanged += (object sender, ListChangedEventArgs e) => {
						ECA_Scopes = string.Join(ScopesDelimiter, formScopes.ToStringArray());
					};
				}

				return formScopes;
			}
		}

		OAuth2ScopesCollection formScopes;
	}
}
