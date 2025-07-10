using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class EPaymentConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public EPaymentConfigurationCollection() : base()
		{
		}

		public new EPaymentConfiguration this[int x]
		{
			get { return (EPaymentConfiguration)base[x]; }
		}

		public new EPaymentConfiguration AddNew()
		{
			return (EPaymentConfiguration)base.AddNew();
		}

		internal void PopulateDefaultConfiguration(ZString countryCode, ZString countryDescription)
		{
			var config = AddNew();
			config.CountryCode = countryCode;
			config.CountryDescription = countryDescription;
			config.OFXEPaymentEnabled = CheckOFXEPaymentEnabled(countryCode);
		}

		public bool IsEPaymentEnabledForAnyProvider => IsOFXEPaymentEnabled;

		#region OFX E-Payment

		public bool IsOFXEPaymentEnabled
		{
			get
			{
				if (this[0] != null)
				{
					return this[0].OFXEPaymentEnabled;
				}
				return false;
			}
		}

		bool CheckOFXEPaymentEnabled(ZString countryCode)
		{
			var ofxSupportedCountries = new List<string>()
			{
				Core.Constants.CountryCodes.Austria,
				Core.Constants.CountryCodes.Australia,
				Core.Constants.CountryCodes.Belgium,
				Core.Constants.CountryCodes.Canada,
				Core.Constants.CountryCodes.Switzerland,
				Core.Constants.CountryCodes.Cyprus,
				Core.Constants.CountryCodes.Germany,
				Core.Constants.CountryCodes.Denmark,
				Core.Constants.CountryCodes.Estonia,
				Core.Constants.CountryCodes.Spain,
				Core.Constants.CountryCodes.Finland,
				Core.Constants.CountryCodes.France,
				Core.Constants.CountryCodes.UnitedKingdom,
				Core.Constants.CountryCodes.Greece,
				Core.Constants.CountryCodes.HongKong,
				Core.Constants.CountryCodes.Hungary,
				Core.Constants.CountryCodes.Ireland,
				Core.Constants.CountryCodes.Italy,
				Core.Constants.CountryCodes.Liechtenstein,
				Core.Constants.CountryCodes.Lithuania,
				Core.Constants.CountryCodes.Luxembourg,
				Core.Constants.CountryCodes.Latvia,
				Core.Constants.CountryCodes.Malta,
				Core.Constants.CountryCodes.Netherlands,
				Core.Constants.CountryCodes.Norway,
				Core.Constants.CountryCodes.NewZealand,
				Core.Constants.CountryCodes.Poland,
				Core.Constants.CountryCodes.Portugal,
				Core.Constants.CountryCodes.Sweden,
				Core.Constants.CountryCodes.Singapore,
				Core.Constants.CountryCodes.Slovenia,
				Core.Constants.CountryCodes.Slovakia,
				Core.Constants.CountryCodes.UnitedStates
			};

			return ofxSupportedCountries.Contains(countryCode);
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new EPaymentConfiguration();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new EPaymentConfigurationCollection();

		#endregion

	}
}
