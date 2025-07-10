using CargoWise.Integration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.InBond.Business
{
	public class USInBondMoveHeaderLookups : AutoUSInBondMoveHeaderLookups
	{
		public USInBondMoveHeaderLookups(AutoUSInBondMoveHeader parent)
			: base(parent)
		{
		}

		CusInBondHeaderLookups HeaderLookups => fHeaderLookups ?? (fHeaderLookups = ((USInBondMoveHeader)Parent).Header.Lookups);
		CusInBondHeaderLookups fHeaderLookups;

		CusInBondMoveHeaderLookups MoveHeaderLookups => fMoveHeaderLookups ?? (fMoveHeaderLookups = ((USInBondMoveHeader)Parent).MoveHeader.Lookups);
		CusInBondMoveHeaderLookups fMoveHeaderLookups;

		public GlbBranchCollection Branches => HeaderLookups.Branches;

		public OrgAddressCollection Importers => HeaderLookups.Importers;

		public ZZRefCusCodeListCombinedCollection RegionDistrictPorts => MoveHeaderLookups.RegionDistrictPorts;

		public ZZRefCusCodeListCombinedCollection FIRMSCollection => HeaderLookups.FIRMSCollection;

		public ZZRefCusCodeListCombinedCollection ForeignPorts => MoveHeaderLookups.ForeignPorts;

		public USCarrierCombinedCollection CarrierCollection => HeaderLookups.CarrierCollection;

		public CodeDescriptionPairList InbondQPMessageStatusList => MoveHeaderLookups.InbondQPMessageStatusList;

		public CodeDescriptionPairList InbondWPMessageStatusList => MoveHeaderLookups.InbondWPMessageStatusList;

		public ICodeDescriptionPairList MessageStatusList => MoveHeaderLookups.MessageStatusList;

		public RefCountryCollection Countries => HeaderLookups.Countries;

		public RefVesselCollection ImportingConveyanceList => HeaderLookups.ImportingConveyanceList;

		public ZZRefCusCodeListCombinedCollection ScheduleKCodes => HeaderLookups.ScheduleKCodes;

		public InBondTransportModeCodes TransportModeCodes => HeaderLookups.TransportModeCodes;

		public InbondCommonTypeList EntryTypeList => MoveHeaderLookups.EntryTypeList;

		public OrgAddressCollection InBondCarriers => MoveHeaderLookups.InBondCarriers;
	}
}
