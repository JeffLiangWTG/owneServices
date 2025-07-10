using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	[SystemDefinedValues]
	public class CusInBondMoveHeader : Customs.Business.CusInBondMoveHeader
	{
		public CusInBondMoveHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type MovementDetailTypeCore => typeof(CusInBondMoveDetail);

		protected override Customs.Business.ICusInBondMoveDetailCollection CreateMovementDetails() => new CusInBondMoveDetailCollection(this);

		public new CusInBondMoveDetailCollection MovementDetails => (CusInBondMoveDetailCollection)base.MovementDetails;

		public CusInBondMoveDetail InBondMoveDetail
		{
			get
			{
				if (fCusInBondMoveDetail?.IsDeleted ?? true)
				{
					if (Header != null)
					{
						var movementBillPK = Header.MovementBill.PK;
						fCusInBondMoveDetail = MovementDetails.FirstOrDefault(x => x.B9_B0 == movementBillPK) ?? MovementDetails.AddNew(movementBillPK);
					}
				}
				return fCusInBondMoveDetail;
			}
		}
		CusInBondMoveDetail fCusInBondMoveDetail;

		public new CusInBondHeader Header
		{
			get
			{
				var header = (CusInBondHeader)base.Header;
				return header == null || header.IsDeleted ? null : header;
			}
		}

		public override ZString BM_GS_NKCusAgent
		{
			get => base.BM_GS_NKCusAgent;
			set
			{
				base.BM_GS_NKCusAgent = value;
				if (Header != null)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveHeader|BM_InBondEntryType", Caption = "Entry Type")]
		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.EntryTypeList))]
		public override ZString BM_InBondEntryType
		{
			get => base.BM_InBondEntryType;
			set
			{
				var oldValue = BM_InBondEntryType;
				base.BM_InBondEntryType = value;
				if (!IsCopying && oldValue != BM_InBondEntryType)
				{
					BM_ForeignDestPortKCode = ZString.Empty;
					BM_RL_NKForeignDestPort = ZString.Empty;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveHeader|BM_ExportTransportMode", Caption = "Transport Code")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.TranshipmentTransportCodeList))]
		public override ZString BM_ExportTransportMode { get => base.BM_ExportTransportMode; set => base.BM_ExportTransportMode = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveHeader|BM_ExportLadenOn", Caption = "Vessel")]
		[MaxLength(25)]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.Vessels))]
		public override ZString BM_ExportLadenOn { get => base.BM_ExportLadenOn; set => base.BM_ExportLadenOn = value; }

		public ZString BM_ExportLadenOnDescription => BM_ExportLadenOn.IsEmpty ? ZString.Empty : CommonHelper.GetVesselDescription(Vessel);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveHeader|BM_ConveyanceNumber", Caption = "Flight No/Voyage")]
		[MaxLength(12)]
		public override ZString BM_ConveyanceNumber { get => base.BM_ConveyanceNumber; set => base.BM_ConveyanceNumber = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveHeader|BM_PlaceOfLoading", Caption = "Departure Port")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.FacilityCollection))]
		public override ZString BM_PlaceOfLoading { get => base.BM_PlaceOfLoading; set => base.BM_PlaceOfLoading = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveHeader|BM_ForeignDestPortKCode", Caption = "Destination")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.FacilityCollection))]
		public override ZString BM_ForeignDestPortKCode
		{
			get => base.BM_ForeignDestPortKCode;
			set
			{
				bool hasChanged = base.BM_ForeignDestPortKCode != value;
				base.BM_ForeignDestPortKCode = value;
				if (hasChanged && !IsCopying)
				{
					Validation.ValidateBM_RL_NKForeignDestPort();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveHeader|BM_RL_NKForeignDestPort", Caption = "Destination (UN)")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.DestinationPorts))]
		public override ZString BM_RL_NKForeignDestPort
		{
			get => base.BM_RL_NKForeignDestPort;
			set
			{
				bool hasChanged = base.BM_RL_NKForeignDestPort != value;
				base.BM_RL_NKForeignDestPort = value;
				if (hasChanged && !IsCopying)
				{
					Validation.ValidateBM_ForeignDestPortKCode();
				}
			}
		}

		[MaxLength(6)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveHeader|BM_TransportAtDeparture", Caption = "Vessel Reg.")]
		public override ZString BM_TransportAtDeparture { get => base.BM_TransportAtDeparture; set => base.BM_TransportAtDeparture = value; }

		public ZBool IsSeaMovement
			=> BM_ExportTransportMode == TranshipmentTransportCodeList.Codes.SeaPackedSundryGoods || BM_ExportTransportMode == TranshipmentTransportCodeList.Codes.SeaContainer
			|| BM_ExportTransportMode == TranshipmentTransportCodeList.Codes.SeaBulkGoods || BM_ExportTransportMode == TranshipmentTransportCodeList.Codes.SeaPassengerOrCREW;

		public ZBool DestinationVisible => BM_InBondEntryType == EntryTypeList.Codes.T1;

		public ZBool DestinationUNVisible => BM_InBondEntryType != EntryTypeList.Codes.T1;

		public new CusInBondMoveHeaderValidation Validation => (CusInBondMoveHeaderValidation)base.Validation;

		public new CusInBondMoveHeaderLookups Lookups => (CusInBondMoveHeaderLookups)base.Lookups;

		protected override Customs.Business.CusInBondMoveHeaderLookups GetNewLookups() => new CusInBondMoveHeaderLookups(this);

		protected override Customs.Business.CusInBondMoveHeaderValidation GetNewValidation() => new CusInBondMoveHeaderValidation(this);

		public RefVessel Vessel => Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, BM_ExportLadenOn);
	}
}
