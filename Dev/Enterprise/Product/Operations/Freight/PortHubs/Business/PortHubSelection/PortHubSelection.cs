using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.PortHubs.Business
{
	public class PortHubSelection : AutoPortHubSelection, IPortHubSelection
	{
		public const string All = "ALL";

		#region Schema

		public new class Schema : AutoPortHubSelection.Schema
		{
			public const string DepotPK = "DepotPK";
			public const string DispatchDepotPK = "DispatchDepotPK";
			public const string DepotPortCode = "DepotPortCode";
			public const string DispatchDepotPortCode = "DispatchDepotPortCode";
		}

		#endregion

		public PortHubSelection(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TY_RatingFreightMode = Constants.TransportModes.All;
		}

		#endregion

		#region Properties

		[List("Lookups.DirectionList")]
		public override ZString TY_Direction
		{
			get { return base.TY_Direction; }
			set
			{
				base.TY_Direction = value;

				if (DispatchDepotReadOnly)
				{
					DispatchDepotPK = ZGuid.Empty;
				}
			}
		}

		[List("Lookups.DGClassList")]
		public override ZString TY_UndgClass
		{
			get { return base.TY_UndgClass; }
			set { base.TY_UndgClass = value; }
		}

		[List("Lookups.ServiceLevels")]
		public override ZString TY_RS_NKServiceLevel
		{
			get { return base.TY_RS_NKServiceLevel; }
			set { base.TY_RS_NKServiceLevel = value; }
		}

		[List("Lookups.FreightModeList")]
		public override ZString TY_RatingFreightMode
		{
			get { return base.TY_RatingFreightMode; }
			set { base.TY_RatingFreightMode = value; }
		}

		[List("Lookups.PackModeList")]
		public override ZString TY_PackMode
		{
			get { return base.TY_PackMode; }
			set { base.TY_PackMode = value; }
		}

		[List("Lookups.ProcessTypeList")]
		public override ZString TY_ProcessType
		{
			get => base.TY_ProcessType;
			set => base.TY_ProcessType = value;
		}

		[List("Lookups.WeightUQList")]
		public override ZString TY_WeightUQ
		{
			get => base.TY_WeightUQ;
			set => base.TY_WeightUQ = value;
		}

		[List("Lookups.VolumeUQList")]
		public override ZString TY_VolumeUQ
		{
			get => base.TY_VolumeUQ;
			set => base.TY_VolumeUQ = value;
		}

		#region Depot

		public OrgHeader Depot
		{
			get { return DepotAddress != null ? DepotAddress.Header : null; }
		}

		[RelatedBusinessObject("Depot")]
		[List("Lookups.Depots")]
		[ResourceStringData("PortHubSelection|DepotPK", Caption = "Destination Depot")]
		public ZGuid DepotPK
		{
			get { return TY_OA_DepotAddress_ZAddress.OrgPK; }
			set
			{
				TY_OA_DepotAddress_ZAddress.OrgPK = value;
				DepotPKInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateDepotPK();
				}
			}
		}

		public ZPropertyInfo DepotPKInfo
		{
			get { return GetZPropertyInfo(Schema.DepotPK); }
		}

		protected override ZAddress GetNewTY_OA_DepotAddress_ZAddress()
		{
			ZAddress result = base.GetNewTY_OA_DepotAddress_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = header => header != null && header.MainAddress != null ? header.MainAddress.PK : ZGuid.Empty;
			return result;
		}

		public override ZGuid TY_OA_DepotAddress
		{
			get { return base.TY_OA_DepotAddress; }
			set
			{
				base.TY_OA_DepotAddress = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateDepotPK();
				}
			}
		}

		RefUNLOCO DepotPort
		{
			get { return DepotAddress != null ? DepotAddress.EffectiveRelatedPortCode : null; }
		}

		[List("Lookups.RefUNLOCO_List")]
		public ZString DepotPortCode
		{
			get
			{
				if (!TY_RL_NKDestinationPort.IsEmpty)
				{
					return TY_RL_NKDestinationPort;
				}
				else
				{
					return DepotPort?.RL_Code ?? ZString.Empty;
				}
			}
			set
			{
				base.TY_RL_NKDestinationPort = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTY_RL_NKDestinationPort();
				}
			}
		}

		public ZPropertyInfo DepotPortCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DepotPortCode, (x) => TY_RL_NKDestinationPortInfo); }
		}

		#endregion

		#region DispatchDepot

		public OrgHeader DispatchDepot
		{
			get { return DispatchDepotAddress != null ? DispatchDepotAddress.Header : null; }
		}

		[RelatedBusinessObject("DispatchDepot")]
		[List("Lookups.Depots")]
		[ResourceStringData("PortHubSelection|DispatchDepotPK", Caption = "Origin Depot")]
		[ReadOnlyMember(nameof(DispatchDepotReadOnly))]
		public ZGuid DispatchDepotPK
		{
			get { return TY_OA_DispatchDepotAddress_ZAddress.OrgPK; }
			set
			{
				TY_OA_DispatchDepotAddress_ZAddress.OrgPK = value;
				DispatchDepotPKInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateDispatchDepotPK();
				}
			}
		}

		public ZPropertyInfo DispatchDepotPKInfo
		{
			get { return GetZPropertyInfo(Schema.DispatchDepotPK); }
		}

		protected override ZAddress GetNewTY_OA_DispatchDepotAddress_ZAddress()
		{
			ZAddress result = base.GetNewTY_OA_DispatchDepotAddress_ZAddress();
			result.DefaultAddressType = AddressType.DLV;
			return result;
		}

		[ReadOnlyMember(nameof(DispatchDepotReadOnly))]
		public override ZGuid TY_OA_DispatchDepotAddress
		{
			get { return base.TY_OA_DispatchDepotAddress; }
			set
			{
				base.TY_OA_DispatchDepotAddress = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateDispatchDepotPK();
				}
			}
		}

		bool DispatchDepotReadOnly
		{
			get { return TY_Direction == PortHubSelectionDirectionList.Codes.Pickup && TY_ProcessType == PortHubSelectionProcessTypeList.Codes.Shipment; }
		}

		RefUNLOCO DispatchDepotPort
		{
			get { return DispatchDepotAddress != null ? DispatchDepotAddress.EffectiveRelatedPortCode : null; }
		}

		[List("Lookups.RefUNLOCO_List")]
		public ZString DispatchDepotPortCode
		{
			get
			{
				if (!TY_RL_NKOriginPort.IsEmpty)
				{
					return TY_RL_NKOriginPort;
				}
				else
				{
					return DispatchDepotPort?.RL_Code ?? ZString.Empty;
				}
			}
			set
			{
				base.TY_RL_NKOriginPort = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTY_RL_NKOriginPort();
				}
			}
		}

		public ZPropertyInfo DispatchDepotPortCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DispatchDepotPortCode, (x) => TY_RL_NKOriginPortInfo); }
		}

		[List("Lookups.CarrierCollection")]
		public override ZGuid TY_OH_Carrier
		{
			get => base.TY_OH_Carrier;
			set => base.TY_OH_Carrier = value;
		}

		#endregion

		#endregion

		#region Related Objects

		[ChildEditable]
		public PortHubZonePivotCollection PortHubZonePivots
		{
			get
			{
				if (portHubZonePivots == null)
				{
					portHubZonePivots = GetNewPortHubZonePivotCollection();
					portHubZonePivots.Load();

					RegisterEditableChildObject(portHubZonePivots);
				}

				return portHubZonePivots;
			}
		}
		PortHubZonePivotCollection portHubZonePivots;

		PortHubZonePivotCollection GetNewPortHubZonePivotCollection()
		{
			return new PortHubZonePivotCollection(this, Factory);
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			RemoveNoZonesError();
			AddNoZonesError();
		}

		public void AddNoZonesError()
		{
			if (PortHubZonePivots.Count == 0)
			{
				AddRowError(NoZonesError);
			}
		}

		public void RemoveNoZonesError()
		{
			RemoveRowError(NoZonesError);
		}

		static string NoZonesError
		{
			get { return Res.GetString("65082d29-4fc9-4896-be8a-22c1e196acbd", "This selection has no zones attached."); }
		}

		#endregion

		#region Find

		public static PortHubSelection FindPortHubForZone(RateTransportZone zone, ZString direction)
		{
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(PortHubZonePivot), PortHubZonePivotSchema.TX_TY_Hub);
			pivotSubQuery.AddToFilter(PortHubZonePivotSchema.TX_TZ_Zone, zone.PK);

			var selectionQuery = new ZDBOnlyQuery(typeof(PortHubSelection));
			selectionQuery.AddToFilter(PortHubSelectionSchema.TY_Direction, direction);
			selectionQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

			return zone.Factory.LoadTop1<PortHubSelection>(selectionQuery);
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new PortHubSelectionFetchStrategy(this);
		}

		#endregion
	}
}
