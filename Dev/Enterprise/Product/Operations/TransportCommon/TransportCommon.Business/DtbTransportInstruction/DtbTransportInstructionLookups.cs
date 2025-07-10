using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionLookups : DtbBookingInstructionLookups
	{
		protected DtbTransportInstructionLookups(DtbTransportInstruction parent)
			: base(parent)
		{
		}

		#region DropModes

		public CodeDescriptionPairList DropModes
		{
			get { return BindToLists.DropModes; }
		}

		#endregion

		#region OrganisationTypes

		public LocalCartageJobOrgTypeList OrganisationTypes
		{
			get { return LocalCartageJobOrgTypeList.Instance; }
		}

		#endregion

		#region Organisations

		#region AllOrganisations

		public OrgHeaderCollection AllOrganisations
		{
			get { return BindToLists.AllOrganisations; }
		}

		#endregion

		#region ConsigneeOrganisations

		public OrganisationsFindBoxCollection ConsigneeOrganisations
		{
			get { return AddToConsignorOrgFilter(BindToLists.ConsigneeOrganisations); }
		}

		#endregion

		#region ConsignorOrganisations

		public OrganisationsFindBoxCollection ConsignorOrganisations
		{
			get { return AddToConsigneeOrgFilter(BindToLists.ConsignorOrganisations); }
		}

		#endregion

		#region Consignor

		OrgHeader Consignor
		{
			get { return GetOrganisation(OrganisationTypesList.Codes.CNR); }
		}

		#endregion

		#region Consignee

		OrgHeader Consignee
		{
			get { return GetOrganisation(OrganisationTypesList.Codes.CNE); }
		}

		#endregion

		#region GetOrganisation

		OrgHeader GetOrganisation(ZString organisationType)
		{
			var cnrInstruction =
				Instruction.Booking.Instructions.Cast<DtbTransportInstruction>()
					.FirstOrDefault(
						i => i.Address != null && i.OrganisationType == organisationType && i.Address.Organisation != null);
			return cnrInstruction != null ? cnrInstruction.Address.Organisation : null;
		}

		#endregion

		#region AddToConsignorOrgFilter

		OrganisationsFindBoxCollection AddToConsignorOrgFilter(OrganisationsFindBoxCollection orgCollection)
		{
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignee - Related Consignor", "Property",
				() => (Consignor != null && Consignor.BuyerLinks.Count > 0) ? Consignor.PK : ZGuid.Empty));

			return orgCollection;
		}

		#endregion

		#region AddToConsigneeOrgFilter

		OrganisationsFindBoxCollection AddToConsigneeOrgFilter(OrganisationsFindBoxCollection orgCollection)
		{
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignor - Related Consignee", "Property",
				() => (Consignee != null && Consignee.SupplierLinks.Count > 0) ? Consignee.PK : ZGuid.Empty));

			return orgCollection;
		}

		#endregion

		#region CFSOrganisations

		public OrgHeaderCollection CFSOrganisations
		{
			get { return BindToLists.CFSOrganisations; }
		}

		#endregion

		#region CTOOrganisations

		public OrgHeaderCollection CTOOrganisations
		{
			get { return BindToLists.CTOOrganisations; }
		}

		#endregion

		#region ContainerYardOrganisations

		public OrgHeaderCollection ContainerYardOrganisations
		{
			get { return BindToLists.ContainerYardOrganisations; }
		}

		#endregion

		#endregion

		#region InstructionTypes

		public CodeDescriptionPairList InstructionTypes
		{
			get { return BindToLists.InstructionTypes; }
		}

		#endregion

		#region PackageCategories

		public PackageCategories PackageCategories
		{
			get { return BindToLists.PackageCategories; }
		}

		#endregion

		#region BindToLists

		public TransportBindToLists BindToLists
		{
			get { return Factory.GetCachedValue("DtbTransportInstructionLookups|BindToLists|" + Instruction.PK, () => GetNewBindToLists()); }
		}

		protected virtual TransportBindToLists GetNewBindToLists()
		{
			return new TransportBindToLists(Factory);
		}

		#endregion

		#region Instruction

		DtbTransportInstruction Instruction
		{
			get { return (DtbTransportInstruction)Parent; }
		}

		#endregion
	}
}
