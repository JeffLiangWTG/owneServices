using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class PortManifestPort : AutoPortManifestPort
	{
		public PortManifestPort()
		{
		}

		public PortManifestPort(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region Properties

		[List("Lookups.Port_List")]
		public override ZString Port
		{
			[DebuggerStepThrough]
			get { return base.Port; }
			[DebuggerStepThrough]
			set { base.Port = value; }
		}

		[List("Lookups.Principals")]
		public override ZGuid PrincipalPK
		{
			[DebuggerStepThrough]
			get { return base.PrincipalPK; }
			[DebuggerStepThrough]
			set { base.PrincipalPK = value; }
		}

		#endregion

		#region Validation

		public override void ValidatePort()
		{
			base.ValidatePort();
			MandatoryValidation.CheckEntered(PortInfo);
			ListValidation.ErrorIfInvalidCode(PortInfo, Lookups.Port_List);

			if (ParentCollections.Count > 0)
			{
				if (!Port.IsEmpty)
				{
					if (!ArePortAndPrincipalPKUnique())
					{
						PortInfo.AddError(UniquePortAndPrincipalErrorMessage);
					}
				}
			}
		}

		public override void ValidatePrincipalPK()
		{
			base.ValidatePrincipalPK();

			if (ParentCollections.Count > 0)
			{
				if (!Port.IsEmpty)
				{
					if (!ArePortAndPrincipalPKUnique())
					{
						PrincipalPKInfo.AddError(UniquePortAndPrincipalErrorMessage);
					}
				}
			}
		}

		public override void ValidateSenderID()
		{
			base.ValidateSenderID();
			if (Enabled)
			{
				MandatoryValidation.CheckEntered(SenderIDInfo);
			}
		}

		#endregion

		#region Lookups

		public PortManifestPortLookups Lookups
		{
			get { return lookups ?? (lookups = new PortManifestPortLookups(this, CurrentFactory)); }
		}
		PortManifestPortLookups lookups;

		#endregion

		#region Implementation

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			SenderID = RawDataRegistry.Instance.SystemEnterpriseCode.Value;
		}

		bool ArePortAndPrincipalPKUnique()
		{
			var parentCollection = (PortManifestPortCollection)GetParentCollection(this, typeof(PortManifestPortCollection));

			return parentCollection == null || !parentCollection
						.Cast<PortManifestPort>()
						.Any(y =>
							y.PK != PK
							&& !y.IsDeleted
							&& y.Port == Port
							&& y.PrincipalPK == PrincipalPK);
		}

		ZString UniquePortAndPrincipalErrorMessage
		{
			get
			{
				return Res.GetString("60BE9784-0A8A-4F77-B72B-DA495AC3F9B0", "The combination of Port and Principal may only appear once in this list.");
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PortManifestPort(fallbackLevel);
		}

		#endregion
	}
}


