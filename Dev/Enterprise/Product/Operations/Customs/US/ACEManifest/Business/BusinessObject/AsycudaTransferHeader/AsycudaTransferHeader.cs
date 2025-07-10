using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaTransferHeader : ASYCUDA.Business.AsycudaTransferHeader
	{
		public AsycudaTransferHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool ReadOnly
		{
			get => base.ReadOnly || IsSubmitted;
			set => base.ReadOnly = value;
		}

		bool IsSubmitted => TransferBills.Cast<AsycudaTransferBill>().Any(x => x.IsSubmitted);

		#region ATF_ATH_ArrivalHeader

		public new AsycudaArrivalHeader ArrivalHeader => (AsycudaArrivalHeader)base.ArrivalHeader;

		public override ZGuid ATF_ATH_ArrivalHeader
		{
			get => base.ATF_ATH_ArrivalHeader;
			set
			{
				var oldValue = ATF_ATH_ArrivalHeader;
				base.ATF_ATH_ArrivalHeader = value;
				if (ATF_ATH_ArrivalHeader != oldValue && !IsCopying)
				{
					DefaultDestinationPortCodeFromManifestHeader();
					TransferBills.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		void DefaultDestinationPortCodeFromManifestHeader()
		{
			if (ATF_RL_NKDestinationPortCode.IsEmpty)
			{
				ATF_RL_NKDestinationPortCode = ArrivalHeader?.Header?.AMA_RL_NKPortOfDischarge ?? ZString.Empty;
			}
		}

		#endregion

		#region InBond Carrier
		public override ZString ATF_CarrierID
		{
			get => base.ATF_CarrierID;
			set
			{
				if (!IsCopying && ATF_CarrierID != value && !value.IsEmpty && !aTF_OA_Carrier_Being_Set)
				{
					ATF_OA_Carrier_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
					ATF_OA_Carrier = ZGuid.Empty;
				}
				base.ATF_CarrierID = value;
			}
		}
		#region ATF_OA_Carrier

		[List(nameof(ATF_OA_Carrier_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid ATF_OA_Carrier
		{
			get { return base.ATF_OA_Carrier; }
			set
			{
				if (!aTF_OA_Carrier_Being_Set)
				{
					aTF_OA_Carrier_Being_Set = true;
					var oldOrgPK = Carrier?.OA_OH;
					base.ATF_OA_Carrier = value;
					if (oldOrgPK == null || (Carrier is OrgAddress inBondCarrier && oldOrgPK != inBondCarrier.OA_OH))
					{
						CusInBondMoveHeader.DefaultInBondCarrierDetails(InBondCarrierOrg, ATF_OnwardCarrierInfo, ATF_CarrierIDInfo, IsCopying);
					}
					aTF_OA_Carrier_Being_Set = false;
				}
			}
		}
		bool aTF_OA_Carrier_Being_Set;

		#endregion

		#region ATF_OnwardCarrier

		[RelatedBusinessObject("OnwardCarrier")]
		public override ZString ATF_OnwardCarrier
		{
			get => base.ATF_OnwardCarrier;
			set => base.ATF_OnwardCarrier = value;
		}

		public USCarrierCombined OnwardCarrier
		{
			get { return Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, ATF_OnwardCarrier)); }
		}

		#endregion
		#endregion

		#region Bonded Premises
		#region ATF_OA_DestinationWarehouse

		[List(nameof(ATF_OA_DestinationWarehouse_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid ATF_OA_DestinationWarehouse
		{
			get { return base.ATF_OA_DestinationWarehouse; }
			set
			{
				if (!aTF_OA_DestinationWareHouse_Being_Set)
				{
					aTF_OA_DestinationWareHouse_Being_Set = true;
					var oldAddress = ATF_OA_DestinationWarehouse;
					base.ATF_OA_DestinationWarehouse = value;
					if (ATF_OA_DestinationWarehouse != oldAddress && !IsCopying)
					{
						DefaultDestinationWarehouseIDFromDestinationWarehouse();
					}
					aTF_OA_DestinationWareHouse_Being_Set = false;
				}
			}
		}
		bool aTF_OA_DestinationWareHouse_Being_Set;

		#endregion

		#region ATF_DestinationWarehouseID

		public override ZString ATF_DestinationWarehouseID
		{
			get => base.ATF_DestinationWarehouseID;
			set
			{
				if (!IsCopying && ATF_DestinationWarehouseID != value && !value.IsEmpty && !aTF_OA_DestinationWareHouse_Being_Set)
				{
					ATF_OA_DestinationWarehouse_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
					ATF_OA_DestinationWarehouse = ZGuid.Empty;
				}
				base.ATF_DestinationWarehouseID = value;
			}
		}

		void DefaultDestinationWarehouseIDFromDestinationWarehouse()
		{
			var warehouseID = ZString.Empty;

			var warehouseAddress = DestinationWarehouse;
			if (warehouseAddress != null)
			{
				var firmsCode = warehouseAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);
				if (FIRMSCodeValidator.IsValidFIRMS(firmsCode))
				{
					warehouseID = firmsCode;
				}
				else
				{
					var airlineDetails = warehouseAddress.Header?.MiscServ.Airline;
					if (airlineDetails != null)
					{
						warehouseID = airlineDetails.RM_TwoCharacterCode;
						if (warehouseID.IsEmpty)
						{
							warehouseID = airlineDetails.RM_ThreeLetterCode;
						}
					}
				}
			}

			ATF_DestinationWarehouseID = warehouseID;
		}

		#endregion
		#endregion

		public new IBusinessObjectCollection<AsycudaTransferBill> TransferBills => (IBusinessObjectCollection<AsycudaTransferBill>)base.TransferBills;
		protected override ManifestBase.IAsycudaTransferBillCollection<ManifestBase.AsycudaTransferBill> CreateNewAsycudaTransferBillCollection() => new ManifestBase.AsycudaTransferBillCollection<AsycudaTransferBill>(this);

		public new AsycudaTransferHeaderLookups Lookups => (AsycudaTransferHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaTransferHeaderLookups GetNewLookups() => new AsycudaTransferHeaderLookups(this);
		public new AsycudaTransferHeaderValidation Validation => (AsycudaTransferHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaTransferHeaderValidation GetNewValidation() => new AsycudaTransferHeaderValidation(this);
	}
}
