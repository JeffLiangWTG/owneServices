using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(EDICommunicationParty), nameof(EDICommunicationParty.Configs))]
	public class EDICommunicationPartyConfig : AutoEDICommunicationPartyConfig, IEDICommunicationPartyConfig
	{
		public EDICommunicationPartyConfig(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		IEDICommunicationParty IEDICommunicationPartyConfig.Party => base.Party;

		IEDIClientAuth IEDICommunicationPartyConfig.Auth => base.Auth;

		public ZBool IsActive => ECC_IsActive;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			ECC_Status = EDICommunicationPartyConfigStatusList.Codes.Requested;
		}

		public override ZGuid ECC_ECA_Auth
		{
			get => base.ECC_ECA_Auth;
			set
			{
				if (value != base.ECC_ECA_Auth)
				{
					base.ECC_ECA_Auth = value;
					_auth = null;
				}
			}
		}

		public override EDICommunicationAuth Auth
		{
			get
			{
				if (_auth == null)
				{
					_auth = Factory.Load<EDICommunicationAuth>(ECC_ECA_Auth);
					RegisterEditableChildObject(_auth);
				}
				return _auth;
			}
		}
		EDICommunicationAuth _auth;

		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			var baseResult = base.GetStrategies();

			return baseResult.Concat(new[] { new ValidateConfig(), }).ToArray();
		}

		class ValidateConfig : IBusinessObjectStrategy
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
				var config = (EDICommunicationPartyConfig)businessObject;
				if (config.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Inbound && config.Auth != null && config.IsActive)
				{
					if (config.Auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.BasicAuthentication)
					{
						CheckUsernameUniquenessOnInboundConfigWithBasicAuthentication(config);
					}
					if (config.Auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.OAuthAuthentication && !config.Auth.ECA_ClientID.IsEmpty)
					{
						CheckClientIDUniquenessOnInboundConfigWithOAuthAuthentication(config);
					}
					if (config.Auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.OAuthAuthentication)
					{
						if (config.ECC_IsSelfManaged && EnvProxy.IsHostedWithCargowise)
						{
							throw new ZCannotSaveException(Res.GetString("EDICommunicationParty|ErrorMessages|InvalidOperationCustomerIsNotSelfHosted", "Invalid operation. This customer is not self-hosted and therefore cannot configure a Self-Managed OAuth Identity Provider."), "Saving Failed");
						}
					}
				}
			}

			ZDBOnlyQuery GenerateQueryForLookupOtherConfig(EDICommunicationPartyConfig config)
			{
				var query = new ZDBOnlyQuery(typeof(EDICommunicationPartyConfig));
				query.AddToFilter(EDICommunicationPartyConfigSchema.ECC_Direction, EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
				query.AddToFilter(EDICommunicationPartyConfigSchema.PK, SQLComparisonOperator.NotEqual, config.PK);
				query.TableHints = TableHints.UPDLOCK | TableHints.ROWLOCK;
				return query;
			}

			ZDBOnlySubQuery GenerateSubQueryForLookupAuth()
			{
				var subQuery = new ZDBOnlySubQuery(typeof(EDICommunicationAuth), EDICommunicationAuthSchema.PK);
				subQuery.TableHints = TableHints.UPDLOCK | TableHints.ROWLOCK;
				return subQuery;
			}

			void CheckUsernameUniquenessOnInboundConfigWithBasicAuthentication(EDICommunicationPartyConfig config)
			{
				var query = GenerateQueryForLookupOtherConfig(config);
				var subQuery = GenerateSubQueryForLookupAuth();
				subQuery.AddToFilter(EDICommunicationAuthSchema.ECA_AuthorizationMode, EDICommunicationAuthModesList.Codes.BasicAuthentication);
				subQuery.AddToFilter(EDICommunicationAuthSchema.ECA_Username, config.Auth.ECA_Username);
				query.AddSubQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, subQuery, JoinCondition.And);

				if (config.Factory.LoadTop1<EDICommunicationPartyConfig>(query) != null)
				{
					throw new ZCannotSaveException(Res.GetString("EDICommunicationParty|ErrorMessages|DuplicateInboundConfigsWithBasicAuthAndTheSameUsername", "Cannot have two inbound configurations with basic authentication using the same username. Please choose another username."), "Saving Failed");
				}
			}

			void CheckClientIDUniquenessOnInboundConfigWithOAuthAuthentication(EDICommunicationPartyConfig config)
			{
				var query = GenerateQueryForLookupOtherConfig(config);
				var subQuery = GenerateSubQueryForLookupAuth();
				subQuery.AddToFilter(EDICommunicationAuthSchema.ECA_AuthorizationMode, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
				subQuery.AddToFilter(EDICommunicationAuthSchema.ECA_ClientID, config.Auth.ECA_ClientID);
				query.AddSubQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, subQuery, JoinCondition.And);

				var dbConfig = config.Factory.LoadTop1<EDICommunicationPartyConfig>(query);

				if (dbConfig != null)
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
	}
}
