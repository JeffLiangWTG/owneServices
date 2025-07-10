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
	public sealed class DangerousGoodsManifestPort : AutoDangerousGoodsManifest
	{
		public DangerousGoodsManifestPort()
		{
		}

		public DangerousGoodsManifestPort(FallbackLevel fallbackLevel) : base(fallbackLevel)
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

		public DangerousGoodsManifestPortLookups Lookups
		{
			get { return lookups ?? (lookups = new DangerousGoodsManifestPortLookups(this, CurrentFactory)); }
		}
		DangerousGoodsManifestPortLookups lookups;

		#endregion

		#region Implementation

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			SenderID = RawDataRegistry.Instance.SystemEnterpriseCode.Value;
		}

		bool ArePortAndPrincipalPKUnique()
		{
			var parentCollection = (DangerousGoodsManifestPortCollection)GetParentCollection(this, typeof(DangerousGoodsManifestPortCollection));

			return !parentCollection?
						.Cast<DangerousGoodsManifestPort>()
						.Any(y =>
							y.PK != PK
							&& !y.IsDeleted
							&& y.Port == Port
							&& y.PrincipalPK == PrincipalPK) ?? true;
		}

		ZString UniquePortAndPrincipalErrorMessage
		{
			get
			{
				return Res.GetString("6484219F-03AA-4927-B0C4-2C65529F436A", "The combination of Port and Principal may only appear once in this list.");
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DangerousGoodsManifestPort(fallbackLevel);
		}

		#endregion
	}
}


