using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentAddressLookups : AutoDtbConsignmentAddressLookups
	{
		public DtbConsignmentAddressLookups(AutoDtbConsignmentAddress parent) : base(parent)
		{
		}

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
			var cnrAddress =
				Address.Cast<DtbConsignmentAddress>()
					.FirstOrDefault(
						i => i.Address != null && i.OrganisationType == organisationType && i.Address.Organisation != null);
			return cnrAddress != null ? cnrAddress.Address.Organisation : null;
		}

		#endregion

		#region AddToConsignorOrgFilter

		OrganisationsFindBoxCollection AddToConsignorOrgFilter(OrganisationsFindBoxCollection orgCollection)
		{
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignee - Related Consignor", "Property", // Filter Strip PropertyFor
				() => (Consignor != null && Consignor.BuyerLinks.Count > 0) ? Consignor.PK : ZGuid.Empty));

			return orgCollection;
		}

		#endregion

		#region AddToConsigneeOrgFilter

		OrganisationsFindBoxCollection AddToConsigneeOrgFilter(OrganisationsFindBoxCollection orgCollection)
		{
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignor - Related Consignee", "Property", // Filter Strip PropertyFor
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

		#region BindToLists

		public TransportBindToLists BindToLists
		{
			get { return Factory.GetCachedValue("DtbTransportInstructionLookups|BindToLists", () => GetNewBindToLists()); }
		}

		protected virtual TransportBindToLists GetNewBindToLists()
		{
			return new TransportBindToLists(Factory);
		}

		#endregion

		#region Address

		DtbConsignmentAddress Address
		{
			get { return (DtbConsignmentAddress)Parent; }
		}

		#endregion
	}
}
