using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondMenuItemMessageDataLookups : ZLookups
	{
		public InBondMenuItemMessageDataLookups(InBondMenuItemMessageData parent) : base(parent)
		{
		}

		protected new InBondMenuItemMessageData Parent => (InBondMenuItemMessageData)base.Parent;

		public ZZRefCusCodeListCombinedCollection RegionDistrictPorts
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZZRefCusCodeListCombinedCollection FIRMSCollection
		{
			get
			{
				return UniversalReferenceDataHelper.GetCachedRefCusCodeListCombinedCollection(Factory,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode,
					true,
					Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DistrictPortCode,
					Parent.USDestinationPortCode);
			}
		}

		public CodeDescriptionPairList TransportModeCodes
		{
			get
			{
				return Factory.GetCachedValue("USInBondBulkSendMessages|ExportTransportModeCodes", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(InBondTransportModeCodes.Codes.VesselNonContainer, InBondTransportModeCodes.Descriptions.VesselNonContainer);
					result.AddPair(InBondTransportModeCodes.Codes.VesselContainer, InBondTransportModeCodes.Descriptions.VesselContainer);
					return result;
				});
			}
		}

		public RefVesselCollection ConveyanceList
		{
			get { return new RefVesselCollection(Factory); }
		}
	}
}
