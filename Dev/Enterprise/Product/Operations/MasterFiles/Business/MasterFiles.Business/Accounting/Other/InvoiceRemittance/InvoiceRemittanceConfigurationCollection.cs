using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class InvoiceRemittanceConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public InvoiceRemittanceConfigurationCollection()
			: this(new BusinessObjectFactory() { NameForDebugging = "InvoiceRemittanceConfigurationCollection_Ctor" })
		{
		}

		public InvoiceRemittanceConfigurationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public InvoiceRemittanceConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new InvoiceRemittanceConfiguration this[int x]
		{
			get { return (InvoiceRemittanceConfiguration)base[x]; }
		}

		public new InvoiceRemittanceConfiguration AddNew()
		{
			return (InvoiceRemittanceConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceRemittanceConfigurationCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceRemittanceConfiguration(CurrentFallbackLevel, CurrentFactory);
		}

		public InvoiceRemittanceConfiguration[] GetMatchInvoiceRemittanceConfiguration(OrgHeader org)
		{
			var listConfiguration = new List<InvoiceRemittanceConfiguration>();
			if (org != null && org.UNLOCO != null)
			{
				var fallbackLocations = AccountingTaxLocations.GetLocationFallbackCodes(CurrentFactory, org.UNLOCO, ZString.Empty, ZString.Empty);

				foreach (var location in fallbackLocations)
				{
					var configuration = this.Cast<InvoiceRemittanceConfiguration>().FirstOrDefault(x => x.DebtorLocation == location.ToString());
					if (configuration != null)
					{
						listConfiguration.Add(configuration);
					}
				}
			}

			return listConfiguration.ToArray();
		}

		public InvoiceRemittanceConfiguration GetBestMatchInvoiceRemittanceConfiguration(OrgHeader org)
		{
			var configurations = GetMatchInvoiceRemittanceConfiguration(org);

			return configurations.Length > 0 ? configurations[0] : null;
		}
	}
}
