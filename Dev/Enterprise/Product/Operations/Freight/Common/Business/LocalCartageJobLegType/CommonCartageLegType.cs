using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public class CommonCartageLegType : AutoLocalCartageJobLegType, ICanDelete
	{
		public CommonCartageLegType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoLocalCartageJobLegType.Schema
		{
			public const string ToOrgDescription = "ToOrgDescription";
			public const string FromOrgDescription = "FromOrgDescription";
			public const string WaitPointOrgDescription = "WaitPointOrgDescription";
			public const string Chargeable = "Chargeable";
		}

		#endregion

		#region Property Overrides

		#region E4_E3

		[RelatedBusinessObject("CommonCartageType")]
		public override ZGuid E4_E3
		{
			[DebuggerStepThrough()]
			get { return base.E4_E3; }
			set
			{
				if (base.E4_E3 == value)
				{
					base.E4_E3 = value;
				}
				else
				{
					base.E4_E3 = value;

					if (!E4_E3.IsEmpty)
					{
						CartageTypeAttached();
					}
				}
			}
		}

		void CartageTypeAttached()
		{
		}

		#endregion

		#region E4_E5_FromOrg

		[List("Lookups+OrganisationList")]
		public override ZGuid E4_E5_FromOrg
		{
			get { return base.E4_E5_FromOrg; }
			set
			{
				base.E4_E5_FromOrg = value;
				if (CommonCartageType != null)
				{
					CommonCartageType.RefreshBindingIncludingChildren();
				}
			}
		}

		protected bool E4_E5_FromOrg_ReadOnly
		{
			get { return IsNull || (!E4_IsBooking && CommonCartageType != null && CommonCartageType.E3_IsSystem); }
		}

		#endregion

		#region E4_E5_WaitPointOrg

		[List("Lookups+OrganisationList")]
		public override ZGuid E4_E5_WaitPointOrg
		{
			get { return base.E4_E5_WaitPointOrg; }
			set
			{
				base.E4_E5_WaitPointOrg = value;
				if (CommonCartageType != null)
				{
					CommonCartageType.RefreshBindingIncludingChildren();
				}
			}
		}

		protected bool E4_E5_WaitPointOrg_ReadOnly
		{
			get { return IsNull || (!E4_IsBooking && CommonCartageType != null && CommonCartageType.E3_IsSystem); }
		}

		#endregion

		#region E4_E5_ToOrg

		[List("Lookups+OrganisationList")]
		public override ZGuid E4_E5_ToOrg
		{
			get { return base.E4_E5_ToOrg; }
			set
			{
				base.E4_E5_ToOrg = value;
				if (CommonCartageType != null)
				{
					CommonCartageType.RefreshBindingIncludingChildren();
				}
			}
		}

		protected bool E4_E5_ToOrg_ReadOnly
		{
			get { return IsNull || (!E4_IsBooking && CommonCartageType != null && CommonCartageType.E3_IsSystem); }
		}

		#endregion

		#region E4_DisplayOrder

		protected bool E4_DisplayOrder_ReadOnly
		{
			get { return CommonCartageType != null && CommonCartageType.E3_IsSystem; }
		}

		#endregion

		#region E4_ContainerMode

		[BusinessObjectTestExclude()]
		public override ZString E4_ContainerMode
		{
			get { return base.E4_ContainerMode; }
			set
			{
				if (!value.IsEmpty && value != Constants.CartageContainerMode.Loose && value != Constants.CartageContainerMode.Containerized)
				{
					throw new NotSupportedException("Only Loose and Containerized modes are supported");
				}
				base.E4_ContainerMode = value;
			}
		}
		protected bool E4_ContainerMode_ReadOnly
		{
			get { return CommonCartageType != null && CommonCartageType.E3_IsSystem; }
		}

		#endregion

		#region E4_EquipmentGroup

		[List("Lookups+EquipmentGroupList")]
		public override ZString E4_EquipmentGroup
		{
			get { return base.E4_EquipmentGroup; }
			set { base.E4_EquipmentGroup = value; }
		}

		#endregion

		#endregion

		#region Overrides

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CartageLegTypeFetchStrategy(this);
		}

		#endregion

		#endregion

		#region Related Objects

		public CommonCartageType CommonCartageType
		{
			get { return Factory.Load<CommonCartageType>(E4_E3); }
		}

		public CommonCartageOrg FromOrg
		{
			get { return Factory.Load<CommonCartageOrg>(E4_E5_FromOrg); }
		}

		public CommonCartageOrg WaitPointOrg
		{
			get { return Factory.Load<CommonCartageOrg>(E4_E5_WaitPointOrg); }
		}

		public CommonCartageOrg ToOrg
		{
			get { return Factory.Load<CommonCartageOrg>(E4_E5_ToOrg); }
		}

		#endregion

		#region New Properties

		#region Chargeable

		public ZString Chargeable
		{
			get
			{
				ZString result = Res.GetString("4d3d08df-23ff-404f-90d5-28dcbed8d539", "Not Chargeable");
				CommonCartageLegType bookingMoveType = IsLoose ? CommonCartageType.LooseBooking : CommonCartageType.ContainerizedBooking;

				if (bookingMoveType != null)
				{
					List<ZGuid> chargeableParties = new List<ZGuid>();
					if (CommonCartageType.IsExportOrOrigin)
					{
						if (!bookingMoveType.E4_E5_FromOrg.IsEmpty)
						{
							chargeableParties.Add(bookingMoveType.E4_E5_FromOrg);
						}

						if (!bookingMoveType.E4_E5_WaitPointOrg.IsEmpty && !bookingMoveType.E4_E5_ToOrg.IsEmpty)
						{
							chargeableParties.Add(bookingMoveType.E4_E5_WaitPointOrg);
						}
					}
					else
					{
						if (!bookingMoveType.E4_E5_WaitPointOrg.IsEmpty)
						{
							chargeableParties.Add(bookingMoveType.E4_E5_WaitPointOrg);
						}

						if (!bookingMoveType.E4_E5_ToOrg.IsEmpty)
						{
							chargeableParties.Add(bookingMoveType.E4_E5_ToOrg);
						}
					}

					if ((CommonCartageType.IsExportOrOrigin && chargeableParties.Contains(E4_E5_FromOrg)) ||
						chargeableParties.Contains(E4_E5_WaitPointOrg) ||
						(CommonCartageType.IsImportOrDestination && chargeableParties.Contains(E4_E5_ToOrg)))
					{
						result = Res.GetString("9bffde1f-d5d8-4b9f-a51b-a2c2cf752cdc", "Chargeable");
					}
				}

				return result;
			}
		}

		public ZPropertyInfo ChargeableInfo
		{
			get { return GetZPropertyInfo(Schema.Chargeable); }
		}

		#endregion

		#region Is ???

		public bool IsLoose
		{
			get { return E4_ContainerMode == Constants.CartageContainerMode.Loose; }
		}

		public bool IsContainerised
		{
			get { return E4_ContainerMode == Constants.CartageContainerMode.Containerized; }
		}

		public bool IsOriginMovement
		{
			get
			{
				return CommonCartageType.Direction == Constants.CartageDirection.Origin || CommonCartageType.Direction == Constants.CartageDirection.Export;
			}
		}

		public bool IsDestinationMovement
		{
			get
			{
				return CommonCartageType.Direction == Constants.CartageDirection.Destination || CommonCartageType.Direction == Constants.CartageDirection.Import;
			}
		}

		public bool IsMSC
		{
			get
			{
				return (FromOrg == null || FromOrg.IsMSC)
						&& (WaitPointOrg == null || WaitPointOrg.IsMSC)
						&& (ToOrg == null || ToOrg.IsMSC);
			}
		}

		#endregion

		#region HasConsigneeOrConsignor

		public bool HasConsigneeOrConsignor
		{
			get
			{
				return (OrgTypeFromCode == LocalCartageJobOrgTypeList.Codes.CNE ||
							OrgTypeFromCode == LocalCartageJobOrgTypeList.Codes.CNR ||
							OrgTypeWaitCode == LocalCartageJobOrgTypeList.Codes.CNE ||
							OrgTypeWaitCode == LocalCartageJobOrgTypeList.Codes.CNR ||
							OrgTypeToCode == LocalCartageJobOrgTypeList.Codes.CNE ||
							OrgTypeToCode == LocalCartageJobOrgTypeList.Codes.CNR);
			}
		}

		#endregion

		#region OrgTypeCodes

		public ZString OrgTypeFromCode
		{
			get { return FromOrg != null ? FromOrg.E5_OrgType.ToString() : ""; }
		}

		public ZString OrgTypeWaitCode
		{
			get { return WaitPointOrg != null ? WaitPointOrg.E5_OrgType.ToString() : ""; }
		}

		public ZString OrgTypeToCode
		{
			get { return ToOrg != null ? ToOrg.E5_OrgType.ToString() : ""; }
		}

		#endregion

		#region OrgDescriptions

		#region ToOrgDescription

		[MaxLength(30)]
		public ZString ToOrgDescription
		{
			get { return (ToOrg == null) ? ZString.Empty : (ZString)LocalCartageJobOrgTypeList.Instance.GetDescriptionFromCode(ToOrg.E5_OrgType); }
		}

		public ZPropertyInfo ToOrgDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ToOrgDescription); }
		}

		#endregion

		#region FromOrgDescription

		[MaxLength(30)]
		public ZString FromOrgDescription
		{
			get { return (FromOrg == null) ? ZString.Empty : (ZString)LocalCartageJobOrgTypeList.Instance.GetDescriptionFromCode(FromOrg.E5_OrgType); }
		}

		public ZPropertyInfo FromOrgDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.FromOrgDescription); }
		}
		#endregion

		#region WaitPointOrgDescription

		public ZString WaitPointOrgDescription
		{
			get { return (WaitPointOrg == null) ? ZString.Empty : (ZString)LocalCartageJobOrgTypeList.Instance.GetDescriptionFromCode(WaitPointOrg.E5_OrgType); }
		}

		public ZPropertyInfo WaitPointOrgDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.WaitPointOrgDescription); }
		}

		public int WaitPointOrgDescription_MaxLength
		{
			get { return LocalCartageJobOrgTypeList.Instance.MaxCodeLength; }
		}

		#endregion

		#endregion

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !CommonCartageType.E3_IsSystem; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("222c80f2-c715-4d6c-b64c-47acce6afb3f", "Port Transport Job Type is System Defined. This Port Transport Leg cannot be deleted."); }
		}

		#endregion
	}
}
