//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobCartageLookups
//
//    This class should be used for overriding collections in AutoJobCartageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public class JobCartageLookups : AutoJobCartageLookups
	{
		public JobCartageLookups(AutoJobCartage parent)
			: base(parent)
		{
		}

		#region Depot_List

		DepotCollection depotList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Services, OrganisationSubType = OrganisationsSubTypeList.Codes.ContainerFreightStation)]
		public DepotCollection DepotList
		{
			get
			{
				if (depotList == null)
				{
					depotList = new DepotCollection(Factory);
				}
				return depotList;
			}
		}

		#endregion

		#region CTO_List

		CTOCollection ctoList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Services, OrganisationSubType = OrganisationsSubTypeList.Codes.ContainerTerminalOperator)]
		public CTOCollection CTOList
		{
			get
			{
				if (ctoList == null)
				{
					ctoList = new CTOCollection(Factory);
				}
				return ctoList;
			}
		}

		#endregion

		#region ContainerYard_List

		ContainerYardCollection containerYardList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Services, OrganisationSubType = OrganisationsSubTypeList.Codes.ContainerYard)]
		public ContainerYardCollection ContainerYardList
		{
			get
			{
				if (containerYardList == null)
				{
					containerYardList = new ContainerYardCollection(Factory);
				}
				return containerYardList;
			}
		}

		#endregion

		#region Consignor_List

		ConsignorCollection consignorList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignor)]
		public ConsignorCollection ConsignorList
		{
			get
			{
				if (consignorList == null)
				{
					consignorList = new ConsignorCollection(Factory);
					consignorList.AllowOtherOrgTypes = true;
				}
				return consignorList;
			}
		}

		#endregion

		#region Consignee_List

		ConsigneeCollection consigneeList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public ConsigneeCollection ConsigneeList
		{
			get
			{
				if (consigneeList == null)
				{
					consigneeList = new ConsigneeCollection(Factory);
					consigneeList.AllowOtherOrgTypes = true;
				}
				return consigneeList;
			}
		}

		#endregion

		#region OrgHeader_List

		OrgHeaderCollection orgHeaderList;
		public OrgHeaderCollection OrgHeaderList
		{
			get
			{
				if (orgHeaderList == null)
				{
					orgHeaderList = new OrgHeaderCollection(Factory);
				}
				return orgHeaderList;
			}
		}

		#endregion

		#region OuterPackTypes

		public RefPackTypeCollection OuterPackTypes
		{
			get { return BindToLists.OuterPackTypes; }
		}

		#endregion

		#region WeightUnits

		public CodeDescriptionPairList WeightUnits
		{
			get { return BindToLists.WeightUnits; }
		}

		#endregion

		#region VolumeUnits

		public CodeDescriptionPairList VolumeUnits
		{
			get { return BindToLists.VolumeUnits; }
		}

		#endregion

		#region RefUNLOCOs

		protected RefUNLOCOCollection fRefUNLOCOs;
		public RefUNLOCOCollection RefUNLOCOs
		{
			get
			{
				if (fRefUNLOCOs == null)
				{
					fRefUNLOCOs = new RefUNLOCOCollection(Factory);
				}
				return fRefUNLOCOs;
			}
		}

		#endregion

		#region Vessels

		protected RefVesselCollection fVessels;
		public RefVesselCollection Vessels
		{
			get
			{
				if (fVessels == null)
				{
					fVessels = new RefVesselCollection(Factory);
				}
				return fVessels;
			}
		}

		#endregion

		#region BindToLists

		BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region Implementation

		//CommonCartage Cartage;

		#endregion
	}
}
