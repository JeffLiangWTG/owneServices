using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRefFacilityValidation : AutoOrgRefFacilityValidation
	{
		public OrgRefFacilityValidation(AutoOrgRefFacility parent) : base(parent)
		{
		}

		new OrgRefFacility Parent => (OrgRefFacility)base.Parent;

		protected override void CheckOFC_RFT_Facility()
		{
			base.CheckOFC_RFT_Facility();
			if (!Parent.OFC_RFT_FacilityInfo.HasErrors())
			{
				ValidateCargoWiseOneFacility();
				ValidateDuplicate();
			}
		}

		void ValidateDuplicate()
		{
			var duplicateOrgFacilities = Parent.Factory.Load<OrgRefFacility>(GetDuplicateQuery(Parent));

			if (duplicateOrgFacilities.Length > 1)
			{
				var duplicateOrganizations = ZString.Join(", ", duplicateOrgFacilities.Select(c => c.Organization.OH_Code).Distinct().ToArray());
				Parent.OFC_RFT_FacilityInfo.AddError(Res.GetString("89ab1162-2522-42f6-91b4-df971d3c10c7",
					"This facility is already linked to following organizations:'{0}'", duplicateOrganizations));
				return;
			}
		}

		public static ZQuery GetDuplicateQuery(OrgRefFacility parent)
		{
			var queryFilter = new ZQuery(OrgRefFacilitySchema.OFC_RFT_Facility, parent.OFC_RFT_Facility);
			return queryFilter;
		}

		void ValidateCargoWiseOneFacility()
		{
			MandatoryValidation.CheckEntered(Parent.OFC_RFT_FacilityInfo);

			switch (Parent.Facility?.RFT_FacilityType)
			{
				case Core.Constants.FacilityType.Code.Terminal:
					if (!Parent.Organization.OH_IsAirCTO && !Parent.Organization.OH_IsSeaCTO && !Parent.Organization.OH_IsRailHead && !Parent.Organization.OH_IsRoadFreightDepot)
					{
						Parent.OFC_RFT_FacilityInfo.AddError(Res.GetString("22ab1462-2522-4df6-91b4-df971d3c10c8",
							"CTO / Terminal Facilities can only be linked to Organizations with at least one of Air CTO, Sea CTO/Stevedore, Rail Head/Depot or Road Depot/Transit Shed Service Type checked."));
					}
					break;
				case Core.Constants.FacilityType.Code.ContainerYard:
					if (!Parent.Organization.OH_IsContainerYard)
					{
						Parent.OFC_RFT_FacilityInfo.AddError(Res.GetString("22ab16ba-2522-4df6-1125-df971d3c10c8",
							"Container Yard Facilities can only be linked to Organizations with Container Yard Service Type checked."));
					}
					break;
				case Core.Constants.FacilityType.Code.TransitWarehouse:
					if (!Parent.Organization.OH_IsPackDepot && !Parent.Organization.OH_IsUnpackDepot)
					{
						Parent.OFC_RFT_FacilityInfo.AddError(Res.GetString("20ab152a-2522-4df6-91b4-af971d3c40c8",
							"CFS / Transit Warehouse Facilities can only be linked to Organizations with at least one of Packing CFS or Unpacking CFS Service Type checked."));
					}
					break;
				default:
					break;
			}
		}

		protected override void CheckOFC_OA_PremisesAddress()
		{
			base.CheckOFC_OA_PremisesAddress();
			ValidatePremisesAddressForCargoWiseOneFacilityUniqueAddressLinkWithSameType();
			ValidatePremisesAddressCanNotBlankIfMoreThanOneCargoWiseOneFacilityWithSameType();
		}

		void ValidatePremisesAddressCanNotBlankIfMoreThanOneCargoWiseOneFacilityWithSameType()
		{
			var facilities = Parent.Organization.OrgRefFacilities.Where(c => c.Facility != null &&
					Parent.Facility != null &&
					c.Facility.RFT_FacilityType == Parent.Facility.RFT_FacilityType).ToList();

			if (facilities.Count > 1 && Parent.OFC_OA_PremisesAddress.IsEmpty)
			{
				var message = Res.GetString("d9a52134-53de-404b-af5a-a0d9f5522111", "If more than one facility of the same type has been selected, each facility must have a corresponding address linked");
				Parent.OFC_OA_PremisesAddressInfo.AddError(message);
			}
		}

		void ValidatePremisesAddressForCargoWiseOneFacilityUniqueAddressLinkWithSameType()
		{
			var facilities = Parent.Organization.OrgRefFacilities.Where(c => c.Facility != null &&
					Parent.Facility != null &&
					c.Facility.RFT_FacilityType == Parent.Facility.RFT_FacilityType &&
					c.OFC_OA_PremisesAddress == Parent.OFC_OA_PremisesAddress).ToList();

			if (facilities.Count > 1)
			{
				var facilityPremisesAddressConflictings = ZString.Join(", ", facilities.Where(c => c.OFC_RFT_Facility != Parent.OFC_RFT_Facility).Select(c => c.Facility.RFT_Code).ToArray());
				var message = Res.GetString("d9a52139-53de-404e-af5a-a0d9f5522721", "This address has already been linked to facility: '{0}'", facilityPremisesAddressConflictings);
				Parent.OFC_OA_PremisesAddressInfo.AddError(message);
			}
		}
	}
}
