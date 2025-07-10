using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ViewLocationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ViewLocation; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			if (selectedBusinessObject == null)
			{
				return ZControllerFactory.Create(ControllerIDs.RefUNLOCO);
			}
			else
			{
				var location = selectedBusinessObject as ViewLocation;
				switch (location.VLO_TableCode)
				{
					case RefUNLOCOSchema.Constants.Prefix:
						return ZControllerFactory.Create(ControllerIDs.RefUNLOCO);
					case RefCountrySchema.Constants.Prefix:
						return ZControllerFactory.Create(ControllerIDs.RefCountry);
					case RefCountryStatesSchema.Constants.Prefix:
						return ZControllerFactory.Create(ControllerIDs.RefCountryStates);
					case RefCityTownSchema.Constants.Prefix:
						return ZControllerFactory.Create(ControllerIDs.RefCityTown);
					case RefZoneHeaderSchema.Constants.Prefix:
						return ZControllerFactory.Create(ControllerIDs.InternationalZone);
					case RateTransportZonesSchema.Constants.Prefix:
						return ZControllerFactory.Create(ControllerIDs.RateTransportZone);
					default:
						throw new Exception("Invalid Business Object Type");
				}
			}
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ViewLocationFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ViewLocationCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ViewLocationFilterBusinessObject((ViewLocationCollection)GridCollection);
		}

		public override bool AllowNew
		{
			get
			{
				return false;
			}
		}

		public override bool AllowDelete
		{
			get
			{
				return false;
			}
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Location; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
