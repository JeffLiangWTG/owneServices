using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgLocatedWithinModuleFilterValidation : ModuleFilterValidation
	{
		public OrgLocatedWithinModuleFilterValidation(OrgLocatedWithinModuleFilter parent)
		: base(parent)
		{
			this.parent = parent;
		}

		readonly OrgLocatedWithinModuleFilter parent;

		public override Type AutoValidationType => GetType();

		public void ValidateUnitForGeolocation()
		{
			ValidateCalculatedProperty(parent.UnitForGeolocationInfo);
		}

		protected void CheckUnitForGeolocation()
		{
			ListValidation.ErrorIfInvalidCode(parent.UnitForGeolocationInfo);
			MandatoryValidation.CheckEntered(parent.UnitForGeolocationInfo);
			ValidationForUnitOrDistance(parent.UnitForGeolocationInfo);
		}

		public void ValidateDistance()
		{
			ValidateCalculatedProperty(parent.DistanceInfo);
		}

		protected void CheckDistance()
		{
			MandatoryValidation.CheckEntered(parent.DistanceInfo);
			ValidationForUnitOrDistance(parent.DistanceInfo);
		}

		void ValidationForUnitOrDistance(ZPropertyInfo info)
		{
			if (parent.UnitForGeolocation == OrgLocatedWithinModuleFilter.UnitMile)
			{
				if (parent.Distance < 0.1 || parent.Distance > 310)
				{
					info.AddError(Res.GetString("337A75F5-18F4-40C8-809A-171EC4D453D7", "The Distance is out of range(0.1 Miles to 310 Miles)"));
				}
			}
			else if (parent.UnitForGeolocation == OrgLocatedWithinModuleFilter.UnitKilometer)
			{
				if (parent.Distance < 0.1 || parent.Distance > 500)
				{
					info.AddError(Res.GetString("0AFF2B89-D39A-4A47-BD08-DCBC458E8D27", "The Distance is out of range(0.1KM to 500KM)"));
				}
			}
		}

		public void ValidateAddressPK()
		{
			ValidateCalculatedProperty(parent.AddressPKInfo);
		}

		protected void CheckAddressPK()
		{
			if (parent.FilterType == OrgLocatedWithinModuleFilter.SearchTypeOrgAddress)
			{
				ListValidation.ErrorIfInvalidPK(parent.AddressPKInfo);
				if (!parent.AddressPK.IsEmpty && parent.CentraGeolocation.IsEmpty)
				{
					parent.AddressPKInfo.AddError(Res.GetString("63459CE3-758D-4EC3-B78B-283AE1BA293F", "The org. address should have a none-empty Geo-location."));
				}
			}
		}

		public void ValidateOrgPK()
		{
			ValidateCalculatedProperty(parent.OrgPKInfo);
		}

		protected void CheckOrgPK()
		{
			if (parent.FilterType == OrgLocatedWithinModuleFilter.SearchTypeOrgAddress)
			{
				ListValidation.ErrorIfInvalidPK(parent.OrgPKInfo);
			}
		}

		public void ValidateUNLOCO()
		{
			ValidateCalculatedProperty(parent.UNLOCOInfo);
		}

		protected void CheckUNLOCO()
		{
			if (parent.FilterType == OrgLocatedWithinModuleFilter.SearchTypeUNLOCO)
			{
				ListValidation.ErrorIfInvalidPK(parent.UNLOCOInfo);
				if (!parent.UNLOCO.IsEmpty && parent.CentraGeolocation.IsEmpty)
				{
					parent.UNLOCOInfo.AddError(Res.GetString("8C2069F9-93B1-43C3-B949-FC163E384403",
						"The UNLOCO selected does not have any co-ordinates configured. Wise Tech are working to source accurate co-ordinates for more UNLOCO's. You can fill them in manually against the UNLOCO record if you wish to use this search."));
				}
			}
		}

		public void ValidateFilterType()
		{
			ValidateCalculatedProperty(parent.FilterTypeInfo);
		}

		protected void CheckFilterType()
		{
			ListValidation.ErrorIfInvalidCode(parent.FilterTypeInfo);
			MandatoryValidation.CheckEntered(parent.FilterTypeInfo);
		}

		public override void ValidateAll()
		{
			ValidateUnitForGeolocation();
			ValidateAddressPK();
			ValidateOrgPK();
			ValidateUNLOCO();
			ValidateFilterType();
			ValidateDistance();
		}
	}
}
