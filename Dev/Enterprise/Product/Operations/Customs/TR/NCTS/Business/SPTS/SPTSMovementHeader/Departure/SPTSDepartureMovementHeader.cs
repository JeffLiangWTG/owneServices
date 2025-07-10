using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSDepartureMovementHeader : CusInBondMoveHeader
	{
		public SPTSDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static SPTSDepartureMovementHeader LoadOrCreate(SPTSHeader parent, ZString movementType)
		{
			SPTSDepartureMovementHeader result = null;

			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, parent.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, movementType);
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			result = parent.Factory.LoadTop1<SPTSDepartureMovementHeader>(query);

			if (result == null)
			{
				result = parent.Factory.New<SPTSDepartureMovementHeader>();
				using (result.SuspendSettingHasChanges())
				{
					result.BM_BH = parent.PK;
					result.BM_SubApplicationCode = movementType;
				}
			}

			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BM_InlandTransportMode = SPTSTransportModeList.Codes.SEA;
		}

		public new SPTSHeader Header => (SPTSHeader)base.Header;

		public new SPTSDepartureMovementHeaderLookups Lookups => (SPTSDepartureMovementHeaderLookups)GetNewLookups();

		protected override CusInBondMoveHeaderLookups GetNewLookups() => new SPTSDepartureMovementHeaderLookups(this);

		public new SPTSDepartureMovementHeaderValidation Validation => (SPTSDepartureMovementHeaderValidation)GetNewValidation();

		protected override CusInBondMoveHeaderValidation GetNewValidation() => new SPTSDepartureMovementHeaderValidation(this);

		protected override ICusInBondMoveDetailCollection CreateMovementDetails() => new SPTSMoveDetailCollection(this);

		protected override Type MovementDetailTypeCore => typeof(SPTSMoveDetail);

		public OrgHeader InBondCarrierOrg => (OrgHeader)base.BM_OA_InBondCarrier_ZAddress.OrgHeader;

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(SPTSDepartureMovementHeaderLookups.TransitStatusList))]
		[ReadOnly(true)]
		[ResourceStringData("TRSPTSMovementHeader.BM_CustomsStatus", Caption = "Customs Status")]
		public override ZString BM_CustomsStatus { get => base.BM_CustomsStatus; set => base.BM_CustomsStatus = value; }

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(SPTSDepartureMovementHeaderLookups.TransportModeList))]
		[ResourceStringData("TRSPTSMovementHeader.BM_InlandTransportMode", Caption = "Transport Mode")]
		public override ZString BM_InlandTransportMode { get => base.BM_InlandTransportMode; set => base.BM_InlandTransportMode = value; }

		[List(nameof(Lookups) + "." + nameof(SPTSDepartureMovementHeaderLookups.ShippingProviders))]
		[ResourceStringData("TRSPTSMovementHeader.BM_OA_InBondCarrier", Caption = "Carrier")]
		public override ZGuid BM_OA_InBondCarrier { get => base.BM_OA_InBondCarrier; set => base.BM_OA_InBondCarrier = value; }

		[List(nameof(Lookups) + "." + nameof(SPTSDepartureMovementHeaderLookups.CustomsOfficeList))]
		[ResourceStringData("TRSPTSMovementHeader.BM_PortOfPresentationCode", Caption = "Departure Customs Office", ShortCaption = "Departure Cus.Off")]
		public override ZString BM_PortOfPresentationCode { get => base.BM_PortOfPresentationCode; set => base.BM_PortOfPresentationCode = value; }

		[List(nameof(Lookups) + "." + nameof(SPTSDepartureMovementHeaderLookups.CustomsOfficeList))]
		[ResourceStringData("TRSPTSMovementHeader.BM_DestinationPortCode", Caption = "Arrival Customs Office", ShortCaption = "Arrival Cus.Off")]
		public override ZString BM_DestinationPortCode { get => base.BM_DestinationPortCode; set => base.BM_DestinationPortCode = value; }
	}
}
