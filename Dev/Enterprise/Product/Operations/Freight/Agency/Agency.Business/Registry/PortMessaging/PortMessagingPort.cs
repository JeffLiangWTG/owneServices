using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class PortMessagingPort : AutoPortMessagingPort
	{
		public PortMessagingPort()
		{
		}

		public PortMessagingPort(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PortMessagingPort(fallbackLevel);
		}

		#region Properties

		[List("Lookups.Port_List")]
		public override ZString Port
		{
			get
			{
				return base.Port;
			}
			set
			{
				base.Port = value;

				Parent?.MarkAsNeedingValidation();
				RefreshCheckUniqueness();
			}
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

		public override ZBool Enabled
		{
			get => base.Enabled;
			set
			{
				base.Enabled = value;
				if (!IsValidationSuspended)
				{
					ValidateSenderID();
				}
			}
		}

		#endregion

		#region Implementation

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			SenderID = RawDataRegistry.Instance.SystemEnterpriseCode.Value;
			Enabled = false;
		}

		public override void ValidatePort()
		{
			base.ValidatePort();
			MandatoryValidation.CheckEntered(PortInfo);
			ListValidation.ErrorIfInvalidCode(PortInfo, Lookups.Port_List);
			CheckUniqueness(PortInfo);
		}

		public override void ValidateSenderID()
		{
			base.ValidateSenderID();

			if (Enabled && SenderID.IsEmpty)
			{
				SenderIDInfo.AddError(Res.GetString("d1f4729c-ccd2-4609-bcdd-3f235930ed91", "Sender ID is required."));
			}
		}

		public override void ValidatePrincipalPK()
		{
			base.ValidatePrincipalPK();
			CheckUniqueness(PrincipalPKInfo);
			if (!PrincipalPK.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(PrincipalPKInfo, Lookups.Principals, ResString.GetMultilingualString("D3AD1E87-55E3-432F-BE46-FC3DDBEC9026", "Please select valid Principal organization as configured on Organization > Carrier > Configuration > Sea > Principal."));
			}
		}

		void CheckUniqueness(ZPropertyInfo property)
		{
			if (Parent != null)
			{
				int count = 0;
				foreach (PortMessagingPort setting in Parent)
				{
					if (setting.Port == Port && setting.PrincipalPK == PrincipalPK)
					{
						if (++count >= 2)
						{
							property.AddError(Res.GetString("5D92AF49-5DEE-409F-98AF-817D2602F2B9", "The same Port cannot be duplicated for the same Principal."));
							break;
						}
					}
				}
			}
		}

		void RefreshCheckUniqueness()
		{
			if (Parent != null)
			{
				foreach (PortMessagingPort setting in Parent)
				{
					setting.ValidatePort();
					setting.ValidatePrincipalPK();
				}
			}
		}

		#region Parent

		public PortMessagingPortCollection Parent
		{
			get
			{
				if (parent == null)
				{
					parent = (PortMessagingPortCollection)GetParentCollection(this, typeof(PortMessagingPortCollection));
				}

				return parent;
			}
		}

		PortMessagingPortCollection parent;

		#endregion

		#region Lookups

		public PortMessagingPortLookups Lookups => lookups ?? (lookups = new PortMessagingPortLookups(this, CurrentFactory));

		PortMessagingPortLookups lookups;

		#endregion

		#endregion
	}
}


