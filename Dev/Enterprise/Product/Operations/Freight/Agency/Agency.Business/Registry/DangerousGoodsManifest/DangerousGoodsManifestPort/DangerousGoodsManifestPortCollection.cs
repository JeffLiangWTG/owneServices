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
	public sealed class DangerousGoodsManifestPortCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DangerousGoodsManifestPortCollection()
		{
		}

		public DangerousGoodsManifestPortCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public new DangerousGoodsManifestPort this[int index]
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DangerousGoodsManifestPort)Elements[index]; }
		}

		[System.Diagnostics.DebuggerStepThrough]
		public new DangerousGoodsManifestPort AddNew()
		{
			return (DangerousGoodsManifestPort)base.AddNew();
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DangerousGoodsManifestPortCollection(fallbackLevel);
		}

		protected override void PerformPostCloneAction(IRegistryBusiness registryBusiness)
		{
			base.PerformPostCloneAction(registryBusiness);

			if (registryBusiness is DangerousGoodsManifestPortCollection collection)
			{
				var companyPK = collection?.CurrentFallbackLevel?.CompanyPK(false);
				if (companyPK != null)
				{
					var company = CurrentFactory?.Load<GlbCompany>((ZGuid)companyPK);
					if (company != null)
					{
						var currentCountryCode = company.Country.Code;

						var itemsToRemove = new List<DangerousGoodsManifestPort>();
						foreach (var element in collection)
						{
							if (element is DangerousGoodsManifestPort port && !port.Port.StartsWith(currentCountryCode))
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
			return new DangerousGoodsManifestPort(CurrentFallbackLevel);
		}

		#endregion
	}
}


