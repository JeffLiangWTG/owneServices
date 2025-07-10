using System;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class PortAuthoritySetting : AutoPortAuthoritySetting
	{
		public PortAuthoritySetting()
		{
		}

		public PortAuthoritySetting(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region Properties

		[List("Lookups.Port_List")]
		public override ZString Port
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.Port;
			}
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.Port = value;

				Parent?.MarkAsNeedingValidation();
				RefreshCheckUniqueness();
			}
		}

		[List("Lookups.Status_List")]
		public override ZString Status
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Status; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Status = value; }
		}

		[List("Lookups.Principals")]
		public override ZGuid PrincipalPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.PrincipalPK;
			}
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.PrincipalPK = value;

				Parent?.MarkAsNeedingValidation();
				RefreshCheckUniqueness();
			}
		}

		void RefreshCheckUniqueness()
		{
			if (Parent != null)
			{
				foreach (PortAuthoritySetting setting in Parent)
				{
					setting.ValidatePort();
					setting.ValidatePrincipalPK();
				}
			}
		}

		public override ZString SenderID
		{
			get { return (Status == PortAuthoritySettingStatus.Codes.Disabled) ? ZString.Empty : base.SenderID; }
			set { base.SenderID = value; }
		}
		protected override bool SenderID_ReadOnly
		{
			get { return Status == PortAuthoritySettingStatus.Codes.Disabled; }
		}

		public override ZString Version
		{
			get
			{
				PortAuthorityPort thisPort = Ports.FindByPort(Port);
				return thisPort == null ? ZString.Empty : thisPort.Version;
			}
		}

		public override ZString Email
		{
			get
			{
				PortAuthorityPort thisPort = Ports.FindByPort(Port);

				switch (Status)
				{
					case PortAuthoritySettingStatus.Codes.Production:
						return thisPort == null ? ZString.Empty : thisPort.ProductionEmail;
					case PortAuthoritySettingStatus.Codes.Testing:
						return thisPort == null ? ZString.Empty : thisPort.TestingEmail;
					default:
						return ZString.Empty;
				}
			}
		}

		public override ZString RecipientID
		{
			get
			{
				PortAuthorityPort thisPort = Ports.FindByPort(Port);

				switch (Status)
				{
					case PortAuthoritySettingStatus.Codes.Production:
						return thisPort == null ? ZString.Empty : thisPort.ProductionID;
					case PortAuthoritySettingStatus.Codes.Testing:
						return thisPort == null ? ZString.Empty : thisPort.TestingID;
					default:
						return ZString.Empty;
				}
			}
		}

		public bool SupportsTesting
		{
			get
			{
				PortAuthorityPort port = Ports.FindByPort(Port);
				return port != null && port.SupportsTesting;
			}
		}

		#endregion

		#region Validation

		public override void ValidatePort()
		{
			base.ValidatePort();
			CheckUniqueness(PortInfo);
			MandatoryValidation.CheckEntered(PortInfo);
			if (!Port.IsEmpty)
			{
				ListValidation.IfInvalidCode(CargoWise.ComponentModel.NotificationType.Error, PortInfo, Lookups.Port_List, (NoResString)"Please select a port from the drop-down.");
			}
		}

		public override void ValidateSenderID()
		{
			base.ValidateSenderID();
			if (Status != PortAuthoritySettingStatus.Codes.Disabled)
			{
				MandatoryValidation.CheckEntered(SenderIDInfo);
			}
		}

		public override void ValidateStatus()
		{
			base.ValidateStatus();
			MandatoryValidation.CheckEntered(StatusInfo);
			ListValidation.ErrorIfInvalidCode(StatusInfo, Lookups.Status_List);
		}

		public override void ValidatePrincipalPK()
		{
			base.ValidatePrincipalPK();
			CheckUniqueness(PrincipalPKInfo);
			if (!PrincipalPK.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(PrincipalPKInfo, Lookups.Principals, (NoResString)"Please select valid Principal organization as configured on Organization > Carrier > Configuration > Sea > Principal).");
			}
		}

		void CheckUniqueness(ZPropertyInfo property)
		{
			if (Parent != null)
			{
				int count = 0;
				foreach (PortAuthoritySetting setting in Parent)
				{
					if (setting.Port == Port && setting.PrincipalPK == PrincipalPK)
					{
						if (++count >= 2)
						{
							property.AddError(Res.GetString("50E5D80C-76BB-446C-8A36-D4C66363BB32", "The same Port cannot be duplicated for the same Principal."));
							break;
						}
					}
				}
			}
		}

		#endregion

		#region Lookups

		public PortAuthoritySettingLookups Lookups
		{
			get { return lookups ?? (lookups = new PortAuthoritySettingLookups(this, CurrentFactory)); }
		}
		PortAuthoritySettingLookups lookups;

		#endregion

		#region Implementation

		#region

		public PortAuthoritySettingCollection Parent
		{
			get
			{
				if (parent == null)
				{
					parent = (PortAuthoritySettingCollection)GetParentCollection(this, typeof(PortAuthoritySettingCollection));
				}

				return parent;
			}
		}

		PortAuthoritySettingCollection parent;

		#endregion

		#region Ports

		PortAuthorityPortCollection Ports
		{
			get
			{
				if (ports == null)
				{
					var retriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.PortAuthorityPorts, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
					ports = (PortAuthorityPortCollection)retriever.GetCurrentValue().Value;
				}

				return ports;
			}
		}
		PortAuthorityPortCollection ports;

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PortAuthoritySetting();
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Status = PortAuthoritySettingStatus.Codes.Disabled;
		}

		#endregion
	}
}



