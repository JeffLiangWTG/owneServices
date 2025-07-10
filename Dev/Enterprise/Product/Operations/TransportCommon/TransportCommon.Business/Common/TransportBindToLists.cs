using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportCommon.Business
{
	public class TransportBindToLists
	{
		public TransportBindToLists(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		protected readonly BusinessObjectFactory Factory;

		#region DropModes

		public CodeDescriptionPairList DropModes
		{
			get { return dropModes ?? (dropModes = new CombinedEquipmentNeededList()); }
		}
		CodeDescriptionPairList dropModes;

		#endregion

		#region Organisations

		#region AllOrganisations

		public OrgHeaderCollection AllOrganisations
		{
			get { return allOrganisations ?? (allOrganisations = new OrgHeaderCollection(Factory)); }
		}

		OrgHeaderCollection allOrganisations;

		#endregion

		#region ConsigneeOrganisations

		public OrganisationsFindBoxCollection ConsigneeOrganisations
		{
			get
			{
				if (consigneeOrganisations == null)
				{
					consigneeOrganisations = new ConsigneeCollection(Factory);
					consigneeOrganisations.AllowOtherOrgTypes = true;
				}
				return consigneeOrganisations;
			}
		}

		ConsigneeCollection consigneeOrganisations;

		#endregion

		#region ConsignorOrganisations

		public OrganisationsFindBoxCollection ConsignorOrganisations
		{
			get
			{
				if (consignorOrganisations == null)
				{
					consignorOrganisations = new ConsignorCollection(Factory);
					consignorOrganisations.AllowOtherOrgTypes = true;
				}
				return consignorOrganisations;
			}
		}
		ConsignorCollection consignorOrganisations;

		#endregion

		#region CFSOrganisations

		public OrgHeaderCollection CFSOrganisations
		{
			get { return cfsOrganisations ?? (cfsOrganisations = new DepotCollection(Factory)); }
		}
		OrgHeaderCollection cfsOrganisations;

		#endregion

		#region CTOOrganisations

		public OrgHeaderCollection CTOOrganisations
		{
			get { return ctoOrganisations ?? (ctoOrganisations = new CTOCollection(Factory)); }
		}
		OrgHeaderCollection ctoOrganisations;

		#endregion

		#region ContainerYardOrganisations

		public OrgHeaderCollection ContainerYardOrganisations
		{
			get { return containerYardOrganisations ?? (containerYardOrganisations = new ContainerYardCollection(Factory)); }
		}
		OrgHeaderCollection containerYardOrganisations;

		#endregion

		#region LocalTransportOrganisations

		public OrgHeaderCollection LocalTransportOrganisations
		{
			get { return localTransportOrganisations ?? (localTransportOrganisations = new LocalTransportCollection(Factory)); }
		}

		LocalTransportCollection localTransportOrganisations;

		#endregion

		#endregion

		#region PackageCategories

		public PackageCategories PackageCategories
		{
			get { return Factory.GetCachedValue("TransportBindToLists|PackageCategories", () => new PackageCategories()); }
		}

		#endregion

		#region InstructionTypes

		public CodeDescriptionPairList InstructionTypes
		{
			get { return Factory.GetCachedValue("3B8E3B0C-B6F1-4AC7-9A71-B5F5B4E14E97", () => new InstructionTypes().List); }
		}

		#endregion

		#region Statuses

		public CodeDescriptionPairList Statuses
		{
			get { return Factory.GetCachedValue("63BF5B4B-9B95-4CF6-806A-FBA268A45D35", () => new TransportStatuses()); }
		}

		public CodeDescriptionPairList ConsignmentStatuses
		{
			get { return Factory.GetCachedValue("920ceb58-46d2-11ee-be56-0242ac120002", () => new ConsignmentStatuses()); }
		}

		public CodeDescriptionPairList BookingStatuses
		{
			get { return Factory.GetCachedValue("3D0492F0-CCCD-463D-93A8-8304B15DD07B", () => new BookingStatuses()); }
		}

		public CodeDescriptionPairList BookingInstructionStatuses
		{
			get { return Factory.GetCachedValue("0636D46A-7DF3-44EE-BFA5-42E15C60A1FB", () => new BookingInstructionStatuses()); }
		}

		public CodeDescriptionPairList BookingConsolidationStatuses
		{
			get { return Factory.GetCachedValue("BBF576F4-C242-4D34-92BB-4D374661A570", () => new BookingConsolidationStatuses()); }
		}

		#endregion
	}
}
