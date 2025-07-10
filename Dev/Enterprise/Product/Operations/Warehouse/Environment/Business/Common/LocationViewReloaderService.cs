using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business
{
	class LocationViewReloaderService : IAfterOnSavingBOProcessingService
	{
		LocationViewReloaderService(BusinessObjectFactory factory)
		{
			factory.Saved += ReloadLocationStringAfterSave;
		}

		public static void AddLocationReloaderService(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));

			var locationReloaderService = factory.ServiceContainer.GetAfterOnSavingService<LocationViewReloaderService>();
			if (locationReloaderService == null)
			{
				locationReloaderService = new LocationViewReloaderService(factory);
				factory.ServiceContainer.AddAfterOnSavingService(locationReloaderService);
			}
		}

		void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			LocationViewDependendencies = (from bizObj in businessObjectsInOnSavingOrder
									   let affector = bizObj as IAffectLocationView
									   where affector != null
									   let columns = affector.GetColumnsThatAffectLocationView()
									   where !bizObj.IsInDatabase || columns.Any(c => bizObj.ZPropertyInfoHash[c.Name].HasChanges)
									   select affector).ToKeyListDictionary(a => a.ParentThatMayReloadMyLocations);
		}

		void ReloadLocationStringAfterSave(BusinessObjectFactory factory, bool saveSucceeded)
		{
			if (LocationViewDependendencies != null)
			{
				try
				{
					if (saveSucceeded && LocationViewDependendencies.Count > 0)
					{
						// Reload Locations for Parent Affectors first
						List<IAffectLocationView> affectorsWithoutParent;
						if (LocationViewDependendencies.TryGetValue(ZGuid.Empty, out affectorsWithoutParent))
						{
							foreach (var affector in affectorsWithoutParent)
							{
								affector.ReloadLocationsFromDB();

								// If parent is Reloaded, any child Reloaders do not need to be reloaded
								if (LocationViewDependendencies.ContainsKey(affector.PK))
								{
									LocationViewDependendencies.Remove(affector.PK);
								}
							}
							LocationViewDependendencies.Remove(ZGuid.Empty);
						}

						// Reload remaining Locations
						foreach (var affector in LocationViewDependendencies.SelectMany(a => a.Value))
						{
							affector.ReloadLocationsFromDB();
						}
					}
				}
				finally
				{
					LocationViewDependendencies.Clear();
				}
			}
		}

		Dictionary<ZGuid, List<IAffectLocationView>> LocationViewDependendencies = new Dictionary<ZGuid, List<IAffectLocationView>>();
	}
}

