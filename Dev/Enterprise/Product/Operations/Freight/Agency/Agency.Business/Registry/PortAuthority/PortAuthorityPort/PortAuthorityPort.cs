using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class PortAuthorityPort : AutoPortAuthorityPort
	{
		public PortAuthorityPort()
		{
		}

		public PortAuthorityPort(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region Properties

		[List("Lookups.Port_List")]
		public override CargoWise.Types.ZString Port
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Port; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Port = value; }
		}

		[List("Lookups.Version_List")]
		public override CargoWise.Types.ZString Version
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Version; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Version = value; }
		}

		public bool SupportsTesting
		{
			get { return !TestingEmail.IsEmpty && !TestingID.IsEmpty; }
		}

		#endregion

		#region Validation

		public override void ValidatePort()
		{
			base.ValidatePort();
			MandatoryValidation.CheckEntered(PortInfo);
			ListValidation.ErrorIfInvalidCode(PortInfo, Lookups.Port_List);

			if (!PortInfo.HasNotifications() && Port.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Australia)
			{
				PortInfo.AddError(Res.GetString("{F4D4E4A3-752F-439F-BF28-39221822EE65}", "Please enter a UNLOCO for an Australian port."));
			}

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(PortInfo, (IMultilingualString)ResString.GetMultilingualString("{78382F80-1335-4e77-9237-544F9980C091}", "A port may only appear once in this list."));
			}
		}

		public override void ValidateVersion()
		{
			base.ValidateVersion();
			MandatoryValidation.CheckEntered(VersionInfo);
			ListValidation.ErrorIfInvalidCode(VersionInfo, Lookups.Version_List);
		}

		public override void ValidateProductionEmail()
		{
			base.ValidateProductionEmail();
			MandatoryValidation.CheckEntered(ProductionEmailInfo);

			if (!EmailAddressValidation.IsEmailAddressValid(ProductionEmail))
			{
				ProductionEmailInfo.AddError(Res.GetString("{3C2AC995-A1ED-4669-BBA8-E3D22F31EAEF}", "Please enter a valid email address."));
			}
		}

		public override void ValidateProductionID()
		{
			base.ValidateProductionID();
			MandatoryValidation.CheckEntered(ProductionIDInfo);
		}

		public override void ValidateTestingEmail()
		{
			base.ValidateTestingEmail();
			if (!EmailAddressValidation.IsEmailAddressValid(TestingEmail))
			{
				TestingEmailInfo.AddError(Res.GetString("{15BAF1B3-684F-434b-9C75-641AE2321890}", "Please enter a valid email address."));
			}
		}

		#endregion

		#region Lookups

		public PortAuthorityPortLookups Lookups
		{
			get { return lookups ?? (lookups = new PortAuthorityPortLookups(this, CurrentFactory)); }
		}
		PortAuthorityPortLookups lookups;

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PortAuthorityPort();
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Version = PortAuthorityVersionList.Codes.V20;
		}

		#endregion
	}
}


