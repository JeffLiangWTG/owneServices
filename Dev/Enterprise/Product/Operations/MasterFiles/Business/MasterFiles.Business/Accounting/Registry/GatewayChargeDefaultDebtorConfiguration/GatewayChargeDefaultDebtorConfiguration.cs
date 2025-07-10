using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	[DebuggerDisplay("{ConsolDirection}-{ConsolTransportMode}-{ChargeGroup}-{ConsolPaymentTerm}-{RelatedJob}-{PreviousSendingAgent}-{Debtor}")]
	public class GatewayChargeDefaultDebtorConfiguration : RegistryBusinessObjectTemplate
	{
		#region Constructors

		public GatewayChargeDefaultDebtorConfiguration()
		{ }

		#endregion

		#region Schema

		public abstract class Schema
		{
			public const string ConsolDirection = nameof(ConsolDirection);
			public const string ConsolTransportMode = nameof(ConsolTransportMode);
			public const string ChargeGroup = nameof(ChargeGroup);
			public const string ConsolPaymentTerm = nameof(ConsolPaymentTerm);
			public const string RelatedJob = nameof(RelatedJob);
			public const string PreviousSendingAgent = nameof(PreviousSendingAgent);
			public const string Debtor = nameof(Debtor);
		}

		#endregion

		readonly MultilingualString IdenticalConfigurationExists = ResString.GetMultilingualString("9FE788CD-DAFA-4A25-8FFC-13E6791DE12B", "Configuration with identical criteria already exists.");

		#region Bound Properties

		[MaxLength(3)]
		[List("Lookups.ConsolDirectionList")]
		[ResourceStringData("GatewayChargeDefaultDebtorConfiguration|ConsolDirection", Caption = "Consol Direction")]
		public ZString ConsolDirection
		{
			get { return fConsolDirection; }
			set
			{
				CheckMaximumLength(ConsolDirectionInfo, value);
				SetNonPersistentPropertyValue(ConsolDirectionInfo, ref fConsolDirection, value);
				ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, ChargeGroupInfo, ConsolPaymentTermInfo, RelatedJobInfo, PreviousSendingAgentInfo);
			}
		}
		ZString fConsolDirection;

		public ZPropertyInfo ConsolDirectionInfo => GetZPropertyInfo(Schema.ConsolDirection);

		[MaxLength(3)]
		[List("Lookups.ConsolTransportModeList")]
		[ResourceStringData("GatewayChargeDefaultDebtorConfiguration|ConsolTransport", Caption = "Consol Transport")]
		public ZString ConsolTransportMode
		{
			get { return fConsolTransportMode; }
			set
			{
				CheckMaximumLength(ConsolTransportModeInfo, value);
				SetNonPersistentPropertyValue(ConsolTransportModeInfo, ref fConsolTransportMode, value);
				ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, ChargeGroupInfo, ConsolPaymentTermInfo, RelatedJobInfo, PreviousSendingAgentInfo);
			}
		}
		ZString fConsolTransportMode;

		public ZPropertyInfo ConsolTransportModeInfo => GetZPropertyInfo(Schema.ConsolTransportMode);

		[MaxLength(3)]
		[List("Lookups.ChargeGroupList")]
		[ResourceStringData("GatewayChargeDefaultDebtorConfiguration|ChargeGroup", Caption = "Charge Group")]
		public ZString ChargeGroup
		{
			get { return fChargeGroup; }
			set
			{
				CheckMaximumLength(ChargeGroupInfo, value);
				SetNonPersistentPropertyValue(ChargeGroupInfo, ref fChargeGroup, value);
				ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, ChargeGroupInfo, ConsolPaymentTermInfo, RelatedJobInfo, PreviousSendingAgentInfo);
			}
		}
		ZString fChargeGroup;

		public ZPropertyInfo ChargeGroupInfo => GetZPropertyInfo(Schema.ChargeGroup);

		[MaxLength(3)]
		[List("Lookups.ConsolPaymentTermList")]
		[ResourceStringData("GatewayChargeDefaultDebtorConfiguration|ConsolPaymentTerm", Caption = "Consol Payment Term")]
		public ZString ConsolPaymentTerm
		{
			get { return fConsolPaymentTerm; }
			set
			{
				CheckMaximumLength(ConsolPaymentTermInfo, value);
				SetNonPersistentPropertyValue(ConsolPaymentTermInfo, ref fConsolPaymentTerm, value);
				ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, ChargeGroupInfo, ConsolPaymentTermInfo, RelatedJobInfo, PreviousSendingAgentInfo);
			}
		}
		ZString fConsolPaymentTerm;

		public ZPropertyInfo ConsolPaymentTermInfo => GetZPropertyInfo(Schema.ConsolPaymentTerm);

		[MaxLength(3)]
		[List("Lookups.RelatedJobList")]
		[ResourceStringData("GatewayChargeDefaultDebtorConfiguration|RelatedJob", Caption = "Related Job")]
		public ZString RelatedJob
		{
			get { return fRelatedJob; }
			set
			{
				CheckMaximumLength(RelatedJobInfo, value);
				SetNonPersistentPropertyValue(RelatedJobInfo, ref fRelatedJob, value);
				ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, ChargeGroupInfo, ConsolPaymentTermInfo, RelatedJobInfo, PreviousSendingAgentInfo, DebtorInfo);
			}
		}
		ZString fRelatedJob;

		public ZPropertyInfo RelatedJobInfo => GetZPropertyInfo(Schema.RelatedJob);

		[MaxLength(3)]
		[List("Lookups.PreviousSendingAgentList")]
		[ResourceStringData("GatewayChargeDefaultDebtorConfiguration|PreviousSendingAgent", Caption = "Previous Sending Agent")]
		public ZString PreviousSendingAgent
		{
			get { return fPreviousSendingAgent; }
			set
			{
				CheckMaximumLength(PreviousSendingAgentInfo, value);
				SetNonPersistentPropertyValue(PreviousSendingAgentInfo, ref fPreviousSendingAgent, value);
				ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, ChargeGroupInfo, ConsolPaymentTermInfo, RelatedJobInfo, PreviousSendingAgentInfo);
			}
		}
		ZString fPreviousSendingAgent;

		public ZPropertyInfo PreviousSendingAgentInfo => GetZPropertyInfo(Schema.PreviousSendingAgent);

		[MaxLength(3)]
		[List("Lookups.DebtorList")]
		[ResourceStringData("GatewayChargeDefaultDebtorConfiguration|Debtor", Caption = "Debtor")]
		public ZString Debtor
		{
			get { return fDebtor; }
			set
			{
				CheckMaximumLength(DebtorInfo, value);
				SetNonPersistentPropertyValue(DebtorInfo, ref fDebtor, value);
				Validate(DebtorInfo, false);
			}
		}
		ZString fDebtor;

		public ZPropertyInfo DebtorInfo => GetZPropertyInfo(Schema.Debtor);

		#endregion

		void ValidateMany(params ZPropertyInfo[] infos)
		{
			var first = true;
			foreach (var info in infos)
			{
				Validate(info, first);
				first = false;
			}
		}

		void Validate(ZPropertyInfo info, bool chechIdentical = true)
		{
			if (!IsValidationSuspended)
			{
				info.ClearAllNotifications();
				MandatoryValidation.CheckEntered(info);
				if (!info.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(info);
				}

				if (chechIdentical)
				{
					CheckIdenticalConfigurationExists();
				}
			}
		}

		public GatewayChargeDefaultDebtorConfigurationLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new GatewayChargeDefaultDebtorConfigurationLookups(this);
				}
				return fLookups;
			}
		}

		GatewayChargeDefaultDebtorConfigurationLookups fLookups;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, ChargeGroupInfo, ConsolPaymentTermInfo, RelatedJobInfo, PreviousSendingAgentInfo, DebtorInfo);
		}

		public GatewayChargeDefaultDebtorConfigurationCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (GatewayChargeDefaultDebtorConfigurationCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return new GatewayChargeDefaultDebtorConfigurationCollection();
				}
			}
		}

		void CheckIdenticalConfigurationExists()
		{
			if (!ConsolDirectionInfo.HasErrors() && ParentCollections.Count > 0)
			{
				foreach (GatewayChargeDefaultDebtorConfiguration configuration in ParentCollection)
				{
					if (PK != configuration.PK
						&& ConsolDirection == configuration.ConsolDirection
						&& ConsolTransportMode == configuration.ConsolTransportMode
						&& ChargeGroup == configuration.ChargeGroup
						&& ConsolPaymentTerm == configuration.ConsolPaymentTerm
						&& RelatedJob == configuration.RelatedJob
						&& PreviousSendingAgent == configuration.PreviousSendingAgent)
					{
						ConsolDirectionInfo.AddError(IdenticalConfigurationExists);
						break;
					}
				}
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GatewayChargeDefaultDebtorConfiguration();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ConsolDirection, ConsolDirection);
			writer.WriteElementString(Schema.ConsolTransportMode, ConsolTransportMode);
			writer.WriteElementString(Schema.ChargeGroup, ChargeGroup);
			writer.WriteElementString(Schema.ConsolPaymentTerm, ConsolPaymentTerm);
			writer.WriteElementString(Schema.RelatedJob, RelatedJob);
			writer.WriteElementString(Schema.PreviousSendingAgent, PreviousSendingAgent);
			writer.WriteElementString(Schema.Debtor, Debtor);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ConsolDirection = reader.ReadElementString(Schema.ConsolDirection);
			ConsolTransportMode = reader.ReadElementString(Schema.ConsolTransportMode);
			ChargeGroup = reader.ReadElementString(Schema.ChargeGroup);
			ConsolPaymentTerm = reader.ReadElementString(Schema.ConsolPaymentTerm);
			RelatedJob = reader.ReadElementString(Schema.RelatedJob);
			PreviousSendingAgent = reader.ReadElementString(Schema.PreviousSendingAgent);
			Debtor = reader.ReadElementString(Schema.Debtor);
		}

		#endregion
	}
}
