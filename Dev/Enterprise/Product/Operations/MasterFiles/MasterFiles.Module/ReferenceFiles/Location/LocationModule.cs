using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class LocationModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Location; }
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			if (selectedBusinessObject == null)
			{
				LocationFilterBusinessObject locationObj = (LocationFilterBusinessObject)FilterBusinessObject;
				if (locationObj.LocationType == LocationTypeEnum.Country)
				{
					return ZControllerFactory.Create(ControllerIDs.RefCountry);
				}

				if (locationObj.LocationType == LocationTypeEnum.InternationalZone)
				{
					return ZControllerFactory.Create(ControllerIDs.InternationalZone);
				}

				return ZControllerFactory.Create(ControllerIDs.RefUNLOCO);
			}
			else
			{
				if (selectedBusinessObject is RefUNLOCO)
				{
					return ZControllerFactory.Create(ControllerIDs.RefUNLOCO);
				}

				if (selectedBusinessObject is RefZoneHeader)
				{
					return ZControllerFactory.Create(ControllerIDs.InternationalZone);
				}

				if (selectedBusinessObject is RefCountry)
				{
					return ZControllerFactory.Create(ControllerIDs.RefCountry);
				}

				throw new Exception("Invalid Business Object Type");
			}
		}

		protected static Type ElementTypeFromLocationType(LocationFilterBusinessObject businessObject)
		{
			switch (businessObject.LocationType)
			{
				case LocationTypeEnum.Port:
					return typeof(RefUNLOCO);

				case LocationTypeEnum.InternationalZone:
					return typeof(RefZoneHeader);

				case LocationTypeEnum.Country:
					return typeof(RefCountry);

				default:
					throw new Exception("LocationType not recognised.");
			}
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new LocationFilterControl(GridCollection, (LocationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new LocationCollection(Factory);
		}

		protected override bool IsModuleAllowAsync => false;

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var locationCollection = (LocationCollection)GridCollection;
			switch (((LocationFilterBusinessObject)FilterBusinessObject).LocationType)
			{
				case LocationTypeEnum.Country:
					locationCollection.LoadCountry(query);
					break;

				case LocationTypeEnum.InternationalZone:
					locationCollection.LoadZone(query);
					break;

				default:
					locationCollection.LoadUNLoco(query);
					break;
			}

			return PerformSearchResult.CustomGridLoad(factory, query);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			LocationFilterBusinessObject filter = new LocationFilterBusinessObject();
			filter.LocationCollection = (LocationCollection)GridCollection;
			return filter;
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Location; }
		}

		public override SecurityCheckpoint[] GetSecurityCheckpointForPopups()
		{
			if (securityCheckpointForPopups == null)
			{
				securityCheckpointForPopups = new[] { Env.Security.Countries, Env.Security.UNLOCO, Env.Security.Zone };
			}
			return securityCheckpointForPopups;
		}
		SecurityCheckpoint[] securityCheckpointForPopups;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new LocationModuleDecisionProvider(this);
		}

		class LocationModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public LocationModuleDecisionProvider(LocationModule module)
				: base(module)
			{
			}

			public override bool AllowExcelExport
			{
				get { return false; }
			}
		}

		protected override FilteredGridLoader CreateSearchManager()
			=> new LocationModuleGridLoader(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements) { ThrowExceptionOnMaximumRowsLoaded = false };

		class LocationModuleGridLoader : FilteredGridLoader
		{
			public LocationModuleGridLoader(FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
				: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
			{
			}

			public override int GetEstimatedLoadCount(IBusinessObjectCollection collection, ZQuery query)
			{
				var locationObj = (LocationFilterBusinessObject)filterBusinessObject;
				return collection.Factory.GetDatabaseCount(ElementTypeFromLocationType(locationObj), query);
			}
		}
	}
}
