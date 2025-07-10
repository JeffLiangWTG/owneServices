using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingInstructionLookups : Common.DtbBookingInstructionLookups
	{
		public DtbBookingInstructionLookups(DtbBookingInstruction parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList DropModes
		{
			get { return BindToLists.DropModes; }
		}

		public LocalCartageJobOrgTypeList OrganisationTypes
		{
			get { return LocalCartageJobOrgTypeList.Instance; }
		}

		public OrgHeaderCollection AllOrganisations
		{
			get { return BindToLists.AllOrganisations; }
		}

		public OrganisationsFindBoxCollection ConsigneeOrganisations
		{
			get { return AddToConsignorOrgFilter(BindToLists.ConsigneeOrganisations); }
		}

		public OrganisationsFindBoxCollection ConsignorOrganisations
		{
			get { return AddToConsigneeOrgFilter(BindToLists.ConsignorOrganisations); }
		}

		OrgHeader Consignor
		{
			get { return GetOrganisation(OrganisationTypesList.Codes.CNR); }
		}

		OrgHeader Consignee
		{
			get { return GetOrganisation(OrganisationTypesList.Codes.CNE); }
		}

		OrgHeader GetOrganisation(ZString organisationType)
		{
			var cnrInstruction =
				Instruction.Booking.Instructions.Cast<DtbBookingInstruction>()
					.FirstOrDefault(
						i => i.Address != null && i.OrganisationType == organisationType && i.Address.Organisation != null);
			return cnrInstruction != null ? cnrInstruction.Address.Organisation : null;
		}

		OrganisationsFindBoxCollection AddToConsignorOrgFilter(OrganisationsFindBoxCollection orgCollection)
		{
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignee - Related Consignor", "Property",
				() => (Consignor != null && Consignor.BuyerLinks.Count > 0) ? Consignor.PK : ZGuid.Empty));

			return orgCollection;
		}

		OrganisationsFindBoxCollection AddToConsigneeOrgFilter(OrganisationsFindBoxCollection orgCollection)
		{
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignor - Related Consignee", "Property",
				() => (Consignee != null && Consignee.SupplierLinks.Count > 0) ? Consignee.PK : ZGuid.Empty));

			return orgCollection;
		}

		public OrgHeaderCollection CFSOrganisations
		{
			get { return BindToLists.CFSOrganisations; }
		}

		public OrgHeaderCollection CTOOrganisations
		{
			get { return BindToLists.CTOOrganisations; }
		}

		public OrgHeaderCollection ContainerYardOrganisations
		{
			get { return BindToLists.ContainerYardOrganisations; }
		}

		public CodeDescriptionPairList InstructionTypes
		{
			get { return BindToLists.InstructionTypes; }
		}

		public PackageCategories PackageCategories
		{
			get { return BindToLists.PackageCategories; }
		}

		public TransportBindToLists BindToLists
		{
			get { return Factory.GetCachedValue("DtbBookingInstructionLookups|BindToLists|" + Instruction.PK, () => GetNewBindToLists()); }
		}

		TransportBindToLists GetNewBindToLists()
		{
			return new BindToLists(Factory);
		}

		DtbBookingInstruction Instruction
		{
			get { return (DtbBookingInstruction)Parent; }
		}
	}
}
