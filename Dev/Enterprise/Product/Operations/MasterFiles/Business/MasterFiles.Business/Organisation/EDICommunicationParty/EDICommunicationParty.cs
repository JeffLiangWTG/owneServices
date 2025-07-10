using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(nameof(Schema.ECP_Name))]
	[DescriptionProperty(nameof(Schema.ECP_Summary))]
	public class EDICommunicationParty : AutoEDICommunicationParty, IEDICommunicationParty, IAuditParent
	{
		public EDICommunicationParty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ECP_ApplicationCode = new eAdaptorNextApplicationDescriptor().Code;
		}

		#region ECP_ApplicationCode

		[List("Lookups.ApplicationCodeList")]
		public override ZString ECP_ApplicationCode
		{
			get { return base.ECP_ApplicationCode; }
			set
			{
				if (base.ECP_ApplicationCode != value)
				{
					base.ECP_ApplicationCode = value;
					RefreshBinding();
				}
			}
		}

		#endregion
		public IEDIClientApplicationDescriptor ApplicationDescriptor => ObjectFactory.Get<IEDIClientApplicationDescriptors>().GetValue(ECP_ApplicationCode);

		public ZBool SupportsEntity(AccessRequirement key)
		{
			return ApplicationDescriptor != null && ApplicationDescriptor.AccessTypes.Contains(key);
		}

		public bool IsPartyNameDuplicate(ZGuid key, string name)
		{
			var query = new ZDBOnlyQuery(typeof(EDICommunicationParty));
			query.AddToFilter(EDICommunicationPartySchema.ECP_Name, name);

			var party = Factory.LoadTop1<EDICommunicationParty>(query);
			return party != null && party.PK != key;
		}

		#region Configs

		[ChildEditable(true)]
		public DetailEDICommunicationPartyConfigCollection Configs
		{
			get
			{
				if (fConfigs == null)
				{
					fConfigs = new DetailEDICommunicationPartyConfigCollection(this);
					fConfigs.Load();
					RegisterEditableChildObject(fConfigs);
				}
				return fConfigs;
			}
		}
		DetailEDICommunicationPartyConfigCollection fConfigs;

		#endregion

		#region Form Fields

		public ZString Name => ECP_Name;

		[MaxLength(50)]
		public override ZString ECP_Name
		{
			get { return base.ECP_Name; }
			set
			{
				if (base.ECP_Name != value)
				{
					base.ECP_Name = value;
					RefreshBinding();
				}
			}
		}

		public ZString Description => ECP_Summary;

		public EDICommunicationPartyConfig OutboundConfig
		{
			get
			{
				if (_outboundConfig == null)
				{
					var existingConfig = GetDefaultCommunicationPartyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
					if (existingConfig == null)
					{
						var config = ConstructEmptyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
						_outboundConfig = config;
					}
					else
					{
						_outboundConfig = existingConfig;
					}
					RegisterEditableChildObject(_outboundConfig);
				}
				return _outboundConfig;
			}
		}
		EDICommunicationPartyConfig _outboundConfig;

		public EDICommunicationPartyConfig InboundConfig
		{
			get
			{
				if (_inboundConfig == null)
				{
					var existingConfig = GetDefaultCommunicationPartyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
					if (existingConfig == null)
					{
						var config = ConstructEmptyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
						_inboundConfig = config;
					}
					else
					{
						_inboundConfig = existingConfig;
					}
					RegisterEditableChildObject(_inboundConfig);
				}
				return _inboundConfig;
			}
		}

		EDICommunicationPartyConfig _inboundConfig;

		public EDICommunicationPartyConfig CurrentOutboundConfig => GetDefaultCommunicationPartyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Outbound);

		public EDICommunicationPartyConfig CurrentInboundConfig => GetDefaultCommunicationPartyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Inbound);

		EDICommunicationPartyConfig ConstructEmptyConfig(ZString direction)
		{
			var config = Factory.New<EDICommunicationPartyConfig>();
			var auth = Factory.New<EDICommunicationAuth>();
			using (config.SuspendSettingHasChanges())
			using (auth.SuspendSettingHasChanges())
			{
				config.ECC_ECP_Party = PK;
				config.ECC_Direction = direction;
				config.ECC_ECA_Auth = auth.PK;
				auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			}
			return config;
		}

		public ZBool InboundActive => GetDefaultCommunicationPartyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Inbound)?.ECC_IsActive ?? ZBool.False;

		public ZBool OutboundActive => GetDefaultCommunicationPartyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Outbound)?.ECC_IsActive ?? ZBool.False;

		public ZBool ClientEnabled => ECP_IsActive;

		EDICommunicationPartyConfig GetDefaultCommunicationPartyConfig(ZString direction)
		{
			return Configs.Cast<EDICommunicationPartyConfig>().FirstOrDefault(config => config.ECC_Direction == direction);
		}

		#endregion

		#region BusinessObject Overrides

		protected override ZString HumanReadableNameCore => Name;

		#endregion

		#region IAuditParent Members
		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(EDICommunicationPartyConfigSchema.ECC_ECP_Party, EDICommunicationPartyConfigSchema.ECC_Direction);
			}
		}
		#endregion

		#region validate duplicate name
		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			var baseResult = base.GetStrategies();

			return baseResult.Concat(new[] { new CheckNameUniqueness(), }).ToArray();
		}

		class CheckNameUniqueness : IBusinessObjectStrategy
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
				var party = (EDICommunicationParty)businessObject;

				var query = new ZDBOnlyQuery(typeof(EDICommunicationParty));
				query.AddToFilter(EDICommunicationPartySchema.ECP_Name, party.ECP_Name);
				query.AddToFilter(EDICommunicationPartySchema.PK, SQLComparisonOperator.NotEqual, party.PK);
				query.TableHints = TableHints.UPDLOCK | TableHints.ROWLOCK;

				if (party.Factory.LoadTop1<EDICommunicationParty>(query) != null)
				{
					throw new ZCannotSaveException(Res.GetString("EDICommunicationParty|ErrorMessages|DuplicateName", "The name '{0}' is already in use by another EDI Client. Please select a different name.", party.ECP_Name), "Saving Failed");
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
		#endregion
	}
}
