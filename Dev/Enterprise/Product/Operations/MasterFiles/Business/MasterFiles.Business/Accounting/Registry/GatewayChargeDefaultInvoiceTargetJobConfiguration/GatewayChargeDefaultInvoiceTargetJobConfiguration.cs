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
	[DebuggerDisplay("{ConsolDirection}-{ConsolTransportMode}-{PreviousSendingAgentType}-{InvoiceTargetJobType}")]
	public class GatewayChargeDefaultInvoiceTargetJobConfiguration : RegistryBusinessObjectTemplate
	{
		#region Constructors

		public GatewayChargeDefaultInvoiceTargetJobConfiguration()
		{ }

		#endregion

		#region Schema

		public abstract class Schema
		{
			public const string ConsolDirection = nameof(ConsolDirection);
			public const string ConsolTransportMode = nameof(ConsolTransportMode);
			public const string PreviousSendingAgentType = nameof(PreviousSendingAgentType);
			public const string InvoiceTargetJobType = nameof(InvoiceTargetJobType);
		}

		#endregion

		readonly MultilingualString IdenticalConfigurationExists = ResString.GetMultilingualString("EB73FA40-4E04-431F-BC9A-B892578865A9", "Configuration with identical criteria already exists.");

		#region Bound Properties

		#region ConsolDirection

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
				ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, PreviousSendingAgentTypeInfo);
			}
		}
		ZString fConsolDirection;

		public ZPropertyInfo ConsolDirectionInfo => GetZPropertyInfo(Schema.ConsolDirection);

		#endregion

		#region ConsolTransportMode

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
				ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, PreviousSendingAgentTypeInfo);
			}
		}
		ZString fConsolTransportMode;

		public ZPropertyInfo ConsolTransportModeInfo => GetZPropertyInfo(Schema.ConsolTransportMode);

		#endregion

		#region Previous Sending Agent Type

		[MaxLength(3)]
		[List("Lookups.PreviousSendingAgentTypeList")]
		public ZString PreviousSendingAgentType
		{
			get { return fPreviousSendingAgent; }
			set
			{
				CheckMaximumLength(PreviousSendingAgentTypeInfo, value);
				SetNonPersistentPropertyValue(PreviousSendingAgentTypeInfo, ref fPreviousSendingAgent, value);
				ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, PreviousSendingAgentTypeInfo);
			}
		}
		ZString fPreviousSendingAgent;

		public ZPropertyInfo PreviousSendingAgentTypeInfo => GetZPropertyInfo(Schema.PreviousSendingAgentType);

		#endregion

		#region Invoice Target Job Type

		[MaxLength(3)]
		[List("Lookups.InvoiceTargetJobTypeList")]
		public ZString InvoiceTargetJobType
		{
			get { return fInvoiceTargetJobType; }
			set
			{
				CheckMaximumLength(InvoiceTargetJobTypeInfo, value);
				SetNonPersistentPropertyValue(InvoiceTargetJobTypeInfo, ref fInvoiceTargetJobType, value);
				Validate(InvoiceTargetJobTypeInfo, false);
			}
		}
		ZString fInvoiceTargetJobType;

		public ZPropertyInfo InvoiceTargetJobTypeInfo => GetZPropertyInfo(Schema.InvoiceTargetJobType);

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

		#endregion

		public GatewayChargeDefaultInvoiceTargetJobConfigurationLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new GatewayChargeDefaultInvoiceTargetJobConfigurationLookups(this);
				}
				return fLookups;
			}
		}

		GatewayChargeDefaultInvoiceTargetJobConfigurationLookups fLookups;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateMany(ConsolDirectionInfo, ConsolTransportModeInfo, PreviousSendingAgentTypeInfo, InvoiceTargetJobTypeInfo);
		}

		public GatewayChargeDefaultInvoiceTargetJobConfigurationCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (GatewayChargeDefaultInvoiceTargetJobConfigurationCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
				}
			}
		}

		void CheckIdenticalConfigurationExists()
		{
			if (!ConsolDirectionInfo.HasErrors() && ParentCollections.Count > 0)
			{
				foreach (GatewayChargeDefaultInvoiceTargetJobConfiguration configuration in ParentCollection)
				{
					if (PK != configuration.PK
						&& ConsolDirection == configuration.ConsolDirection
						&& ConsolTransportMode == configuration.ConsolTransportMode
						&& PreviousSendingAgentType == configuration.PreviousSendingAgentType)
					{
						ConsolDirectionInfo.AddError(IdenticalConfigurationExists);
						break;
					}
				}
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GatewayChargeDefaultInvoiceTargetJobConfiguration();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ConsolDirection, ConsolDirection);
			writer.WriteElementString(Schema.ConsolTransportMode, ConsolTransportMode);
			writer.WriteElementString(Schema.PreviousSendingAgentType, PreviousSendingAgentType);
			writer.WriteElementString(Schema.InvoiceTargetJobType, InvoiceTargetJobType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ConsolDirection = reader.ReadElementString(Schema.ConsolDirection);
			ConsolTransportMode = reader.ReadElementString(Schema.ConsolTransportMode);
			PreviousSendingAgentType = reader.ReadElementString(Schema.PreviousSendingAgentType);
			InvoiceTargetJobType = reader.ReadElementString(Schema.InvoiceTargetJobType);
		}

		#endregion
	}
}
