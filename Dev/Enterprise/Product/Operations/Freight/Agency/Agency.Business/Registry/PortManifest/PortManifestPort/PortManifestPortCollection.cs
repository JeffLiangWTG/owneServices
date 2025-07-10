using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class PortManifestPortCollection : RegistryBusinessObjectCollectionTemplate
	{
		public PortManifestPortCollection()
		{
		}

		public PortManifestPortCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public new PortManifestPort this[int index]
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (PortManifestPort)Elements[index]; }
		}

		[System.Diagnostics.DebuggerStepThrough]
		public new PortManifestPort AddNew()
		{
			return (PortManifestPort)base.AddNew();
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PortManifestPortCollection(fallbackLevel);
		}

		protected override void PerformPostCloneAction(IRegistryBusiness registryBusiness)
		{
			base.PerformPostCloneAction(registryBusiness);

			if (registryBusiness is PortManifestPortCollection collection)
			{
				var companyPK = collection?.CurrentFallbackLevel?.CompanyPK(false);
				if (companyPK != null)
				{
					var company = CurrentFactory?.Load<GlbCompany>((ZGuid)companyPK);
					if (company != null)
					{
						var currentCountryCode = company.Country.Code;

						var itemsToRemove = new List<PortManifestPort>();
						foreach (var element in collection)
						{
							if (element is PortManifestPort port && !port.Port.StartsWith(currentCountryCode))
							{
								itemsToRemove.Add(port);
							}
						}

						foreach (var port in itemsToRemove)
						{
							collection.RemoveAndDelete(port);
						}
					}
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PortManifestPort(CurrentFallbackLevel);
		}

		#endregion
	}
}


