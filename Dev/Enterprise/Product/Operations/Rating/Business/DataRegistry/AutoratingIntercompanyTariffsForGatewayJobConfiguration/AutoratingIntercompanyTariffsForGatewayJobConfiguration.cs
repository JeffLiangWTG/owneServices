using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	[DebuggerDisplay("{LoginRole}-{LoginAgentRole}-{ShipmentDirection}-{AutoratingJob}-{AutoratingRule}-{ICTServiceProvider}")]
	public class AutoratingIntercompanyTariffsForGatewayJobConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string LoginRole = nameof(LoginRole);
			public const string LoginAgentRole = nameof(LoginAgentRole);
			public const string ShipmentDirection = nameof(ShipmentDirection);
			public const string AutoratingJob = nameof(AutoratingJob);
			public const string AutoratingRule = nameof(AutoratingRule);
			public const string ICTServiceProvider = nameof(ICTServiceProvider);
		}

		#endregion

		public static readonly MultilingualString IdenticalGatewayBillingConfigConfigurationExists = ResString.GetMultilingualString("fce95681-7f8f-4c86-9495-03bf2ea3aa8f", "Autorating Intercompany Tariff For Gateway Billing Configuration with identical criteria already exists");
		public static readonly MultilingualString ShipmentDirectionCannotSetToAll = ResString.GetMultilingualString("a3b691ed-a324-4cda-a05e-2ebfea93d0f8", "ALL is not supported for Shipment direction");
		public static readonly MultilingualString ShipmentGatewayCanNotAutorateCost = ResString.GetMultilingualString("69e81c27-6022-4371-98a8-6196f1665078", "Cannot select Autorate Revenue for Gateway Billing of Shipment");
		public static readonly MultilingualString ConsolGatewayCanNotAutorateRevenue = ResString.GetMultilingualString("6d595ea8-0200-4207-9265-10297f0448a1", "Cannot select Autorate Cost for Gateway Billing of Forwarding Consolidation");

		#region Bound Properties

		[MaxLength(3)]
		[List("Lookups.LoginRoleList")]
		[ResourceStringData("AutoratingIntercompanyTariffsForGatewayJobConfiguration|LoginRole", Caption = "Login Role")]
		public ZString LoginRole
		{
			get { return loginRole; }
			set
			{
				CheckMaximumLength(LoginRoleInfo, value);
				SetNonPersistentPropertyValue(LoginRoleInfo, ref loginRole, value);

				LoginRoleInfo.ClearAllNotifications();
				if (!string.IsNullOrEmpty(value))
				{
					ListValidation.ErrorIfInvalidCode(LoginRoleInfo);
				}
				ValidateCompositeKey();
			}
		}
		ZString loginRole;

		public ZPropertyInfo LoginRoleInfo => GetZPropertyInfo(Schema.LoginRole);

		[MaxLength(3)]
		[List("Lookups.LoginAgentRoleList")]
		[ResourceStringData("AutoratingIntercompanyTariffsForGatewayJobConfiguration|LoginAgentRole", Caption = "Login Agent Role")]
		public ZString LoginAgentRole
		{
			get { return loginAgentRole; }
			set
			{
				CheckMaximumLength(LoginAgentRoleInfo, value);
				SetNonPersistentPropertyValue(LoginAgentRoleInfo, ref loginAgentRole, value);

				LoginAgentRoleInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(LoginAgentRoleInfo);
				ListValidation.ErrorIfInvalidCode(LoginAgentRoleInfo);
				ValidateCompositeKey();
			}
		}
		ZString loginAgentRole;

		public ZPropertyInfo LoginAgentRoleInfo => GetZPropertyInfo(Schema.LoginAgentRole);

		[MaxLength(3)]
		[List("Lookups.ShipmentDirectionList")]
		[ResourceStringData("AutoratingIntercompanyTariffsForGatewayJobConfiguration|ShipmentDirection", Caption = "Shipment Direction")]
		public ZString ShipmentDirection
		{
			get { return shipmentDirection; }
			set
			{
				CheckMaximumLength(ShipmentDirectionInfo, value);
				SetNonPersistentPropertyValue(ShipmentDirectionInfo, ref shipmentDirection, value);

				ShipmentDirectionInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ShipmentDirectionInfo);
				ListValidation.ErrorIfInvalidCode(ShipmentDirectionInfo);
				if (value == FreightShipmentDirection.Code.All)
				{
					ShipmentDirectionInfo.AddError(ShipmentDirectionCannotSetToAll);
				}
				ValidateCompositeKey();
			}
		}
		ZString shipmentDirection;

		public ZPropertyInfo ShipmentDirectionInfo => GetZPropertyInfo(Schema.ShipmentDirection);

		[MaxLength(3)]
		[List("Lookups.AutoratingJobList")]
		[ResourceStringData("AutoratingIntercompanyTariffsForGatewayJobConfiguration|AutoratingJob", Caption = "Autorating Job")]
		public ZString AutoratingJob
		{
			get { return autoratingJob; }
			set
			{
				CheckMaximumLength(AutoratingJobInfo, value);
				SetNonPersistentPropertyValue(AutoratingJobInfo, ref autoratingJob, value);

				AutoratingJobInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(AutoratingJobInfo);
				ListValidation.ErrorIfInvalidCode(AutoratingJobInfo);
				CheckJobRuleConsistency(AutoratingRule, autoratingJob: value);
				ValidateCompositeKey();
			}
		}
		ZString autoratingJob;

		public ZPropertyInfo AutoratingJobInfo => GetZPropertyInfo(Schema.AutoratingJob);

		[MaxLength(3)]
		[List("Lookups.AutoratingRuleList")]
		[ResourceStringData("AutoratingIntercompanyTariffsForGatewayJobConfiguration|AutoratingRuleList", Caption = "Autorating Rule")]
		public ZString AutoratingRule
		{
			get { return autoratingRule; }
			set
			{
				CheckMaximumLength(AutoratingRuleInfo, value);
				SetNonPersistentPropertyValue(AutoratingRuleInfo, ref autoratingRule, value);

				AutoratingRuleInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(AutoratingRuleInfo);
				ListValidation.ErrorIfInvalidCode(AutoratingRuleInfo);
				CheckJobRuleConsistency(autoratingRule: value, AutoratingJob);
				ValidateCompositeKey();
				if (ICTServiceProvider_ReadOnly)
				{
					ICTServiceProvider = GatewayICTServiceProvider.Code.NotAutorate;
				}
			}
		}

		void CheckJobRuleConsistency(ZString autoratingRule, ZString autoratingJob)
		{
			if ((autoratingJob == JobInvoicingConsumerTypes.ShipmentCode) && (autoratingRule == GatewayAutoratingRule.Code.AutoratingRevenue))
			{
				AutoratingRuleInfo.AddError(ShipmentGatewayCanNotAutorateCost);
			}
			else if (AutoratingRuleInfo.HasError(ShipmentGatewayCanNotAutorateCost))
			{
				AutoratingRuleInfo.ClearAllNotifications();
			}

			if ((autoratingJob == JobInvoicingConsumerTypes.ForwardingConsolCode) && (autoratingRule == GatewayAutoratingRule.Code.AutoratingCost))
			{
				AutoratingRuleInfo.AddError(ConsolGatewayCanNotAutorateRevenue);
			}
			else if (AutoratingRuleInfo.HasError(ConsolGatewayCanNotAutorateRevenue))
			{
				AutoratingRuleInfo.ClearAllNotifications();
			}
		}

		ZString autoratingRule;

		public ZPropertyInfo AutoratingRuleInfo => GetZPropertyInfo(Schema.AutoratingRule);

		[MaxLength(3)]
		[List("Lookups.ICTServiceProviderList")]
		[ResourceStringData("AutoratingIntercompanyTariffsForGatewayJobConfiguration|ICTServiceProvider", Caption = "ICT Service Provider")]
		public ZString ICTServiceProvider
		{
			get { return iCTServiceProvider; }
			set
			{
				CheckMaximumLength(ICTServiceProviderInfo, value);
				SetNonPersistentPropertyValue(ICTServiceProviderInfo, ref iCTServiceProvider, value);

				ICTServiceProviderInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ICTServiceProviderInfo);
				ListValidation.ErrorIfInvalidCode(ICTServiceProviderInfo);
				ValidateCompositeKey();
			}
		}

		protected bool ICTServiceProvider_ReadOnly =>
			AutoratingRule == GatewayAutoratingRule.Code.StopAutoratingCost ||
			AutoratingRule == GatewayAutoratingRule.Code.StopAutoratingCostFromICT;

		ZString iCTServiceProvider;

		public ZPropertyInfo ICTServiceProviderInfo => GetZPropertyInfo(Schema.ICTServiceProvider);
		public bool ICTServiceProviderIsReadOnly => ICTServiceProvider_ReadOnly;

		#endregion

		ZPropertyInfo[] CompositeKeyFields => new[]
		{
			LoginRoleInfo,
			LoginAgentRoleInfo,
			ShipmentDirectionInfo,
			AutoratingJobInfo,
			AutoratingRuleInfo,
			ICTServiceProviderInfo
		};

		void ValidateCompositeKey()
		{
			var first = true;
			foreach (var info in CompositeKeyFields)
			{
				Validate(info, first);
				first = false;
			}
		}

		void Validate(ZPropertyInfo info, bool checkIdentical = true)
		{
			if (!IsValidationSuspended)
			{
				if (checkIdentical)
				{
					CheckIdenticalConfigurationExists();
				}
			}
		}

		public AutoratingIntercompanyTariffsForGatewayJobConfigurationLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new AutoratingIntercompanyTariffsForGatewayJobConfigurationLookups(this);
				}
				return fLookups;
			}
		}

		AutoratingIntercompanyTariffsForGatewayJobConfigurationLookups fLookups;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCompositeKey();
		}

		public AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return new AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection();
				}
			}
		}

		void CheckIdenticalConfigurationExists()
		{
			var identicalConfigurationFound = false;

			foreach (AutoratingIntercompanyTariffsForGatewayJobConfiguration configuration in ParentCollection)
			{
				if (PK != configuration.PK
					&& LoginRole == configuration.loginRole
					&& LoginAgentRole == configuration.LoginAgentRole
					&& ShipmentDirection == configuration.ShipmentDirection
					&& AutoratingRule == configuration.AutoratingRule
					&& AutoratingJob == configuration.AutoratingJob)
				{
					identicalConfigurationFound = true;
					break;
				}
			}

			if (identicalConfigurationFound)
			{
				AddRowError(IdenticalGatewayBillingConfigConfigurationExists);
			}
			else
			{
				RemoveRowError(IdenticalGatewayBillingConfigConfigurationExists);
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutoratingIntercompanyTariffsForGatewayJobConfiguration
			{
				LoginAgentRole = LoginAgentRole,
				ShipmentDirection = ShipmentDirection,
				AutoratingJob = AutoratingJob,
				AutoratingRule = AutoratingRule,
				ICTServiceProvider = ICTServiceProvider
			};
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.LoginRole, LoginRole);
			writer.WriteElementString(Schema.LoginAgentRole, LoginAgentRole);
			writer.WriteElementString(Schema.ShipmentDirection, ShipmentDirection);
			writer.WriteElementString(Schema.AutoratingJob, AutoratingJob);
			writer.WriteElementString(Schema.AutoratingRule, AutoratingRule);
			writer.WriteElementString(Schema.ICTServiceProvider, ICTServiceProvider);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			LoginRole = reader.ReadElementString(Schema.LoginRole);
			LoginAgentRole = reader.ReadElementString(Schema.LoginAgentRole);
			ShipmentDirection = reader.ReadElementString(Schema.ShipmentDirection);
			AutoratingJob = reader.ReadElementString(Schema.AutoratingJob);
			AutoratingRule = reader.ReadElementString(Schema.AutoratingRule);
			ICTServiceProvider = reader.ReadElementString(Schema.ICTServiceProvider);
		}

		#endregion
	}
}

