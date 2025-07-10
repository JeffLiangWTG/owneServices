using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.TR.NCTS.Business
{
	[SystemDefinedValues]
	public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader
		, Integration.Customs.ICusSupportingInfoTypeSupporter
		, Integration.Customs.TR.IDepartureMovementHeader
	{
		public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BM_InBondEntryType = NctsDepartureMovementHeaderHelper.DeclarationTypeListCode;
			MoveToFTZ = ZBool.False;
			IsGIKEnabled = ZBool.False;
		}

		public new INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

		public new NctsDepartureMovementHeaderValidation Validation => base.Validation;

		protected override EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Validation GetNewPhase4Validation() => new NctsDepartureMovementHeaderPhase4Validation(this);
		protected override EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation GetNewPhase5Validation() => new NctsDepartureMovementHeaderPhase5Validation(this);

		public new INctsDepartureMovementHeaderLookups Lookups => (INctsDepartureMovementHeaderLookups)base.Lookups;
		protected override CusInBondMoveHeaderLookups GetNewPhase5Lookups() => new NctsDepartureMovementHeaderPhase5Lookups(this);
		protected override CusInBondMoveHeaderLookups GetNewPhase4Lookups() => new NctsDepartureMovementHeaderPhase4Lookups(this);

		public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

		public new NctsHeader Header => (NctsHeader)base.Header;

		protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuarantee>(this);

		protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsDepartureCargoDescCollection(this);

		protected override Type CusInBondCargoDescTypeCore => typeof(NctsDepartureCargoDesc);

		#region ICusSupportingInfoTypeSupporterMembers
		protected override INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypes = base.GetCusSupportingInfoTypes();
			cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.PRE] = typeof(NctsWarehouseToOpen);
			return cusSupportingInfoTypes;
		}
		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}
		#endregion

		#region WarehouseToOpenList
		[ChildEditable(true)]
		public NctsWarehouseToOpenCollection WarehouseToOpenList
		{
			get
			{
				if (warehouseToOpenList == null)
				{
					warehouseToOpenList = new NctsWarehouseToOpenCollection(this, CusSupportingInfoTypeList.Codes.PRE);
					warehouseToOpenList.Load();
					RegisterEditableChildObject(warehouseToOpenList);
				}
				return warehouseToOpenList;
			}
		}
		NctsWarehouseToOpenCollection warehouseToOpenList;
		#endregion

		[ResourceStringData("12992ACB-3536-4D35-B5C8-07ABB12F1625", Caption = "Goods Shipping Location", ShortCaption = "Goods Ship.Loc.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.GoodsShippingLocationList))]
		public override ZString BM_LocationOfGoodsCode { get => base.BM_LocationOfGoodsCode; set => base.BM_LocationOfGoodsCode = value; }

		[ResourceStringData("NctsDepartureMovementHeader|IsGIKEnabled", Caption = "Shipment with the scope of GİK 117", ShortCaption = "GİK 117 ?")]
		public ZBool IsGIKEnabled
		{
			get
			{ return this.BM_LocationQualifier == YesNoList.Codes.Yes; }
			set
			{
				this.BM_LocationQualifier = value ? YesNoList.Codes.Yes : "";
				IsGIKEnabledInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IsGIKEnabledInfo => GetZPropertyInfo(nameof(IsGIKEnabled));

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.CustomsStatusList))]
		public override ZString BM_CustomsStatus { get => base.BM_CustomsStatus; set => base.BM_CustomsStatus = value; }

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.DeclarationTypeList))]
		public override ZString BM_InBondEntryType
		{
			get => base.BM_InBondEntryType;
			set
			{
				var oldValue = BM_InBondEntryType;
				if (oldValue != value)
				{
					base.BM_InBondEntryType = value;
					UpdateBH_FTZMove();
				}
			}
		}

		[ResourceStringData("F9EFC985-7B7F-4E00-8771-7AA1AADF0B80", Caption = "[S17] Place of Loading Code", ShortCaption = "Place of Loading Code")]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TRWarehouseList))]
		public override ZString BM_PlaceOfLoading { get => base.BM_PlaceOfLoading; set => base.BM_PlaceOfLoading = value; }

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TRWarehouseList))]
		public override ZString BM_PlaceOfUnloading { get => base.BM_PlaceOfUnloading; set => base.BM_PlaceOfUnloading = value; }

		[ResourceStringData("D1362300-6155-4AB1-A551-A50491344A33", Caption = "GKİ 117")]
		public ZBool MoveToFTZ
		{
			get
			{
				ZBool moveToFTZ = false;

				if (BM_MoveToFTZ == "Y")
				{
					moveToFTZ = ZBool.True;
				}

				return moveToFTZ;
			}
			set
			{
				ZString moveToFTZset = "N";

				if (value == true)
				{
					moveToFTZset = "Y";
				}

				else
				{
					BM_CustomsOfficeAtBorder = ZString.Empty;
				}

				MoveToFTZInfo.RefreshBinding();

				BM_MoveToFTZ = moveToFTZset;
			}
		}

		public ZPropertyInfo MoveToFTZInfo => GetZPropertyInfo(nameof(MoveToFTZ));

		[ResourceStringData("3CCFD023-3B15-4CF6-BDD6-16F899863180", Caption = "Goods Ship to Code")]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.GoodsShiptoCodeList))]
		public override ZString BM_CustomsOfficeAtBorder { get => base.BM_CustomsOfficeAtBorder; set => base.BM_CustomsOfficeAtBorder = value; }

		public override void OnSaving()
		{
			UpdateBH_FTZMove();
			base.OnSaving();
		}

		void UpdateBH_FTZMove()
		{
			if (!IsCopying && Header != null && BM_InBondEntryType == NctsDepartureMovementHeaderHelper.DeclarationTypeListCode)
			{
				Header.BH_FTZMove = true;
			}
		}

		#region TankerStatus
		[MaxLength(1)]
		[ResourceStringData("BB22F96E-BB8F-4499-8CEF-2EF3F7E8AFB6", Caption = "Tanker Status")]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TankerStatusList))]

		public ZString TankerStatus
		{
			get => this.GetSystemDefinedValue<ZString>(GenAddOnHelper.TankerStatus);
			set
			{
				CheckMaximumLength(TankerStatusInfo, value);
				var oldValue = TankerStatus;
				if (value != oldValue)
				{
					this.SetSystemDefinedValue(GenAddOnHelper.TankerStatus, value);
					TankerStatusInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo TankerStatusInfo => GetZPropertyInfo(nameof(TankerStatus));
		#endregion
	}
}
