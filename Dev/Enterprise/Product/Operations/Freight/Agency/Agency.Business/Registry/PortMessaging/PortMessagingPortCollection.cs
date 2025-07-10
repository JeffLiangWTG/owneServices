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
	public sealed class PortMessagingPortCollection : RegistryBusinessObjectCollectionTemplate
	{
		public PortMessagingPortCollection()
		{
		}

		public PortMessagingPortCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region NewWithDefaultValues

		public static PortMessagingPortCollection NewWithDefaultValues(IEnumerable<string> ports)
		{
			var result = new PortMessagingPortCollection();
			foreach (var port in ports)
			{
				result.AddNew(port);
			}

			return result;
		}

		#endregion

		public new PortMessagingPort this[int index]
		{
			get { return (PortMessagingPort)Elements[index]; }
		}

		public new PortMessagingPort AddNew()
		{
			return (PortMessagingPort)base.AddNew();
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PortMessagingPortCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PortMessagingPort(CurrentFallbackLevel);
		}

		protected override void PerformPostCloneAction(IRegistryBusiness registryBusiness)
		{
			base.PerformPostCloneAction(registryBusiness);

			if (registryBusiness is PortMessagingPortCollection collection)
			{
				var companyPK = collection?.CurrentFallbackLevel?.CompanyPK(false);
				if (companyPK != null)
				{
					var company = CurrentFactory?.Load<GlbCompany>((ZGuid)companyPK);
					if (company != null)
					{
						var currentCountryCode = company.Country.Code;

						var itemsToRemove = new List<PortMessagingPort>();
						foreach (var element in collection)
						{
							if (element is PortMessagingPort port && !port.Port.StartsWith(currentCountryCode))
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

		protected override bool AllowNewCore => true;

		protected override bool AllowRemoveCore => true;

		PortMessagingPort AddNew(string port)
		{
			var result = AddNew();

			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				result.Port = port;
				result.Enabled = false;
				result.SenderID = RawDataRegistry.Instance.SystemEnterpriseCode.Value;
			}

			return result;
		}

		#endregion
	}
}


