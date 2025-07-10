using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderDetailsBulkUpdateBusinessObject : NonPersistentBusinessObject, IOrderVoyageData, IObsoleteValidation, IStmNoteParent
	{
		public static class Schema
		{
			public const string JD_E_EXW = "JD_E_EXW";
			public const string JD_A_EXW = "JD_A_EXW";
			public const string JD_E_IST = "JD_E_IST";
			public const string JD_A_IST = "JD_A_IST";
			public const string JD_E_RCV = "JD_E_RCV";
			public const string JD_A_RCV = "JD_A_RCV";
			public const string JD_E_DEP = "JD_E_DEP";
			public const string JD_A_DEP = "JD_A_DEP";
			public const string JD_E_ARV = "JD_E_ARV";
			public const string JD_A_ARV = "JD_A_ARV";
			public const string JD_E_CCC = "JD_E_CCC";
			public const string JD_A_CCC = "JD_A_CCC";
			public const string JD_E_CLR = "JD_E_CLR";
			public const string JD_A_CLR = "JD_A_CLR";
			public const string JD_E_UNP = "JD_E_UNP";
			public const string JD_A_UNP = "JD_A_UNP";
			public const string JD_E_PUP = "JD_E_PUP";
			public const string JD_A_PUP = "JD_A_PUP";
		}

		public OrderDetailsBulkUpdateBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Saving

		public event EventHandler SaveSucceeded;

		protected override void OnFactorySaving()
		{
			UpdateAllOrdersNow();
		}

		protected override void OnFactorySaved(bool didSaveSucceed)
		{
			base.OnFactorySaved(didSaveSucceed);
			if (didSaveSucceed && SaveSucceeded != null)
			{
				SaveSucceeded(this, EventArgs.Empty);
			}
		}

		void UpdateAllOrdersNow()
		{
			foreach (OrderToBulkUpdate order in this.SelectedOrders)
			{
				order.UpdateFrom(this);
			}
		}

		#endregion

		#region Related Business Objects

		#region Selected Orders

		[ChildEditable(true)]
		public OrderToBulkUpdateCollection SelectedOrders
		{
			get
			{
				if (fSelectedOrders == null)
				{
					fSelectedOrders = new OrderToBulkUpdateCollection(Factory, this);
					RegisterEditableChildObject(fSelectedOrders);
				}
				return fSelectedOrders;
			}
		}
		OrderToBulkUpdateCollection fSelectedOrders;

		#endregion

		#endregion

		#region Properties

		#region Voyage Planning States

		public PlanningVoyageState PlanningVoyageState
		{
			get
			{
				if (!JD_IntermediateVoyage.IsEmpty)
				{
					return PlanningVoyageState.ThreeVoyage;
				}
				else if (
					!JD_DepartureVoyage.IsEmpty &&
					!JD_ArrivalVoyage.IsEmpty &&
					JD_DepartureVoyage != JD_ArrivalVoyage)
				{
					return PlanningVoyageState.TwoVoyage;
				}
				else
				{
					return PlanningVoyageState.OneVoyage;
				}
			}
		}

		#endregion

		#region JD_TransportMode
		[List("JD_TransportMode_List")]
		[MaxLength(Order.Schema.JD_TransportModeMaxLength)]
		public ZString JD_TransportMode
		{
			get { return fJD_TransportMode; }
			set
			{
				CheckMaximumLength(JD_TransportModeInfo, value);
				SetNonPersistentPropertyValue(JD_TransportModeInfo, ref fJD_TransportMode, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_TransportMode();
					SelectedOrders.RunPreSaveValidation();
				}
			}
		}

		ZString fJD_TransportMode;

		public ZPropertyInfo JD_TransportModeInfo
		{
			get { return GetZPropertyInfo(nameof(JD_TransportMode)); }
		}

		#endregion

		#region JD_OrderStatus

		[List("JD_OrderStatus_List")]
		[MaxLength(Order.Schema.JD_OrderStatusMaxLength)]
		public ZString JD_OrderStatus
		{
			get { return fJD_OrderStatus; }
			set
			{
				CheckMaximumLength(JD_OrderStatusInfo, value);
				SetNonPersistentPropertyValue(JD_OrderStatusInfo, ref fJD_OrderStatus, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_OrderStatus();
				}
			}
		}

		ZString fJD_OrderStatus;

		public ZPropertyInfo JD_OrderStatusInfo
		{
			get { return GetZPropertyInfo(nameof(JD_OrderStatus)); }
		}

		#endregion

		#region JD_E_EXW

		public ZDateTime JD_E_EXW
		{
			get { return fJD_E_EXW; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_EXWInfo, ref fJD_E_EXW, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_EXW();
				}
			}
		}
		ZDateTime fJD_E_EXW;

		public ZPropertyInfo JD_E_EXWInfo
		{
			get { return GetZPropertyInfo(Schema.JD_E_EXW); }
		}

		#endregion

		#region JD_E_IST

		public ZDateTime JD_E_IST
		{
			get { return fJD_E_IST; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_ISTInfo, ref fJD_E_IST, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_IST();
				}
			}
		}
		ZDateTime fJD_E_IST;

		public ZPropertyInfo JD_E_ISTInfo
		{
			get { return GetZPropertyInfo(Schema.JD_E_IST); }
		}

		#endregion

		#region JD_E_RCV

		public ZDateTime JD_E_RCV
		{
			get { return fJD_E_RCV; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_RCVInfo, ref fJD_E_RCV, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_RCV();
				}
			}
		}
		ZDateTime fJD_E_RCV;

		public ZPropertyInfo JD_E_RCVInfo
		{
			get { return GetZPropertyInfo(Schema.JD_E_RCV); }
		}

		#endregion

		#region JD_E_DEP

		public ZDateTime JD_E_DEP
		{
			get { return fJD_E_DEP; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_DEPInfo, ref fJD_E_DEP, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_DEP();
				}
			}
		}
		ZDateTime fJD_E_DEP;

		public ZPropertyInfo JD_E_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.JD_E_DEP); }
		}

		#endregion

		#region JD_E_ARV

		public ZDateTime JD_E_ARV
		{
			get { return fJD_E_ARV; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_ARVInfo, ref fJD_E_ARV, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_ARV();
				}
			}
		}
		ZDateTime fJD_E_ARV;

		public ZPropertyInfo JD_E_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JD_E_ARV); }
		}

		#endregion

		#region JD_E_CCC

		public ZDateTime JD_E_CCC
		{
			get { return fJD_E_CCC; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_CCCInfo, ref fJD_E_CCC, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_CCC();
				}
			}
		}
		ZDateTime fJD_E_CCC;

		public ZPropertyInfo JD_E_CCCInfo
		{
			get { return GetZPropertyInfo(Schema.JD_E_CCC); }
		}

		#endregion

		#region JD_E_CLR

		public ZDateTime JD_E_CLR
		{
			get { return fJD_E_CLR; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_CLRInfo, ref fJD_E_CLR, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_CLR();
				}
			}
		}
		ZDateTime fJD_E_CLR;

		public ZPropertyInfo JD_E_CLRInfo
		{
			get { return GetZPropertyInfo(Schema.JD_E_CLR); }
		}

		#endregion

		#region JD_E_UNP

		public ZDateTime JD_E_UNP
		{
			get { return fJD_E_UNP; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_UNPInfo, ref fJD_E_UNP, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_UNP();
				}
			}
		}
		ZDateTime fJD_E_UNP;

		public ZPropertyInfo JD_E_UNPInfo
		{
			get { return GetZPropertyInfo(Schema.JD_E_UNP); }
		}

		#endregion

		#region JD_E_PUP

		public ZDateTime JD_E_PUP
		{
			get { return fJD_E_PUP; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_PUPInfo, ref fJD_E_PUP, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_PUP();
				}
			}
		}
		ZDateTime fJD_E_PUP;

		public ZPropertyInfo JD_E_PUPInfo
		{
			get { return GetZPropertyInfo(Schema.JD_E_PUP); }
		}

		#endregion

		#region JD_A_EXW

		public ZDateTime JD_A_EXW
		{
			get { return fJD_A_EXW; }
			set
			{
				SetNonPersistentPropertyValue(JD_A_EXWInfo, ref fJD_A_EXW, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_A_EXW();
				}
			}
		}
		ZDateTime fJD_A_EXW;

		public ZPropertyInfo JD_A_EXWInfo
		{
			get { return GetZPropertyInfo(Schema.JD_A_EXW); }
		}

		#endregion

		#region JD_A_IST

		public ZDateTime JD_A_IST
		{
			get { return fJD_A_IST; }
			set
			{
				SetNonPersistentPropertyValue(JD_A_ISTInfo, ref fJD_A_IST, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_A_IST();
				}
			}
		}
		ZDateTime fJD_A_IST;

		public ZPropertyInfo JD_A_ISTInfo
		{
			get { return GetZPropertyInfo(Schema.JD_A_IST); }
		}

		#endregion

		#region JD_A_RCV

		public ZDateTime JD_A_RCV
		{
			get { return fJD_A_RCV; }
			set
			{
				SetNonPersistentPropertyValue(JD_A_RCVInfo, ref fJD_A_RCV, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_A_RCV();
				}
			}
		}
		ZDateTime fJD_A_RCV;

		public ZPropertyInfo JD_A_RCVInfo
		{
			get { return GetZPropertyInfo(Schema.JD_A_RCV); }
		}

		#endregion

		#region JD_A_DEP

		public ZDateTime JD_A_DEP
		{
			get { return fJD_A_DEP; }
			set
			{
				SetNonPersistentPropertyValue(JD_A_DEPInfo, ref fJD_A_DEP, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_A_DEP();
				}
			}
		}
		ZDateTime fJD_A_DEP;

		public ZPropertyInfo JD_A_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.JD_A_DEP); }
		}

		#endregion

		#region JD_A_ARV

		public ZDateTime JD_A_ARV
		{
			get { return fJD_A_ARV; }
			set
			{
				SetNonPersistentPropertyValue(JD_A_ARVInfo, ref fJD_A_ARV, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_A_ARV();
				}
			}
		}
		ZDateTime fJD_A_ARV;

		public ZPropertyInfo JD_A_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JD_A_ARV); }
		}

		#endregion

		#region JD_A_CCC

		public ZDateTime JD_A_CCC
		{
			get { return fJD_A_CCC; }
			set
			{
				SetNonPersistentPropertyValue(JD_A_CCCInfo, ref fJD_A_CCC, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_A_CCC();
				}
			}
		}
		ZDateTime fJD_A_CCC;

		public ZPropertyInfo JD_A_CCCInfo
		{
			get { return GetZPropertyInfo(Schema.JD_A_CCC); }
		}

		#endregion

		#region JD_A_CLR

		public ZDateTime JD_A_CLR
		{
			get { return fJD_A_CLR; }
			set
			{
				SetNonPersistentPropertyValue(JD_A_CLRInfo, ref fJD_A_CLR, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_A_CLR();
				}
			}
		}
		ZDateTime fJD_A_CLR;

		public ZPropertyInfo JD_A_CLRInfo
		{
			get { return GetZPropertyInfo(Schema.JD_A_CLR); }
		}

		#endregion

		#region JD_A_UNP

		public ZDateTime JD_A_UNP
		{
			get { return fJD_A_UNP; }
			set
			{
				SetNonPersistentPropertyValue(JD_A_UNPInfo, ref fJD_A_UNP, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_A_UNP();
				}
			}
		}
		ZDateTime fJD_A_UNP;

		public ZPropertyInfo JD_A_UNPInfo
		{
			get { return GetZPropertyInfo(Schema.JD_A_UNP); }
		}

		#endregion

		#region JD_A_PUP

		public ZDateTime JD_A_PUP
		{
			get { return fJD_A_PUP; }
			set
			{
				SetNonPersistentPropertyValue(JD_A_PUPInfo, ref fJD_A_PUP, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_A_PUP();
				}
			}
		}
		ZDateTime fJD_A_PUP;

		public ZPropertyInfo JD_A_PUPInfo
		{
			get { return GetZPropertyInfo(Schema.JD_A_PUP); }
		}

		#endregion

		#region JD_RV_NKDepartureVessel
		[List("JD_RV_Vessel_List")]
		[MaxLength(Order.Schema.JD_RV_NKDepartureVesselMaxLength)]
		public ZString JD_RV_NKDepartureVessel
		{
			get { return fJD_RV_NKDepartureVessel; }
			set
			{
				SetNonPersistentPropertyValue(JD_RV_NKDepartureVesselInfo, ref fJD_RV_NKDepartureVessel, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_RV_NKDepartureVessel();
				}
			}
		}

		ZString fJD_RV_NKDepartureVessel;

		public ZPropertyInfo JD_RV_NKDepartureVesselInfo
		{
			get { return GetZPropertyInfo(nameof(JD_RV_NKDepartureVessel)); }
		}

		void ValidateJD_RV_NKDepartureVessel()
		{
			JD_RV_NKDepartureVesselInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(JD_RV_NKDepartureVesselInfo, JD_RV_Vessel_List);
		}

		#endregion

		#region JD_RV_NKIntermediateVessel

		[List("JD_RV_Vessel_List")]
		[MaxLength(Order.Schema.JD_RV_NKIntermediateVesselMaxLength)]
		public ZString JD_RV_NKIntermediateVessel
		{
			get { return fJD_RV_NKIntermediateVessel; }
			set
			{
				SetNonPersistentPropertyValue(JD_RV_NKIntermediateVesselInfo, ref fJD_RV_NKIntermediateVessel, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_RV_NKIntermediateVessel();
				}
			}
		}

		ZString fJD_RV_NKIntermediateVessel;

		public ZPropertyInfo JD_RV_NKIntermediateVesselInfo
		{
			get { return GetZPropertyInfo(nameof(JD_RV_NKIntermediateVessel)); }
		}

		void ValidateJD_RV_NKIntermediateVessel()
		{
			JD_RV_NKIntermediateVesselInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(JD_RV_NKIntermediateVesselInfo, JD_RV_Vessel_List);
		}

		#endregion

		#region JD_RV_NKArrivalVessel

		[List("JD_RV_Vessel_List")]
		[MaxLength(Order.Schema.JD_RV_NKArrivalVesselMaxLength)]
		public ZString JD_RV_NKArrivalVessel
		{
			get { return fJD_RV_NKArrivalVessel; }
			set
			{
				SetNonPersistentPropertyValue(JD_RV_NKArrivalVesselInfo, ref fJD_RV_NKArrivalVessel, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_RV_NKArrivalVessel();
				}
			}
		}

		ZString fJD_RV_NKArrivalVessel;

		public ZPropertyInfo JD_RV_NKArrivalVesselInfo
		{
			get { return GetZPropertyInfo(nameof(JD_RV_NKArrivalVessel)); }
		}

		void ValidateJD_RV_NKArrivalVessel()
		{
			JD_RV_NKArrivalVesselInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(JD_RV_NKArrivalVesselInfo, JD_RV_Vessel_List);
		}

		#endregion

		#region JD_DepartureVoyage

		[MaxLength(Order.Schema.JD_DepartureVoyageMaxLength)]
		public ZString JD_DepartureVoyage
		{
			get { return fJD_DepartureVoyage; }
			set { SetNonPersistentPropertyValue(JD_DepartureVoyageInfo, ref fJD_DepartureVoyage, value); }
		}

		ZString fJD_DepartureVoyage;

		public ZPropertyInfo JD_DepartureVoyageInfo
		{
			get { return GetZPropertyInfo(nameof(JD_DepartureVoyage)); }
		}

		#endregion

		#region JD_IntermediateVoyage

		[MaxLength(Order.Schema.JD_IntermediateVoyageMaxLength)]
		public ZString JD_IntermediateVoyage
		{
			get { return fJD_IntermediateVoyage; }
			set { SetNonPersistentPropertyValue(JD_IntermediateVoyageInfo, ref fJD_IntermediateVoyage, value); }
		}

		ZString fJD_IntermediateVoyage;

		public ZPropertyInfo JD_IntermediateVoyageInfo
		{
			get { return GetZPropertyInfo(nameof(JD_IntermediateVoyage)); }
		}

		#endregion

		#region JD_ArrivalVoyage

		[MaxLength(Order.Schema.JD_ArrivalVoyageMaxLength)]
		public ZString JD_ArrivalVoyage
		{
			get { return fJD_ArrivalVoyage; }
			set { SetNonPersistentPropertyValue(JD_ArrivalVoyageInfo, ref fJD_ArrivalVoyage, value); }
		}

		ZString fJD_ArrivalVoyage;

		public ZPropertyInfo JD_ArrivalVoyageInfo
		{
			get { return GetZPropertyInfo(nameof(JD_ArrivalVoyage)); }
		}

		#endregion

		#region JD_Milestone_E_DEP

		public ZDateTime JD_Milestone_E_DEP
		{
			get { return fJD_Milestone_E_DEP; }
			set
			{
				SetNonPersistentPropertyValue(JD_Milestone_E_DEPInfo, ref fJD_Milestone_E_DEP, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_Milestone_E_DEP();
				}
			}
		}
		ZDateTime fJD_Milestone_E_DEP;

		public ZPropertyInfo JD_Milestone_E_DEPInfo
		{
			get { return GetZPropertyInfo(Order.Schema.JD_Milestone_E_DEP); }
		}

		#endregion

		#region JD_E_DEP_2

		public ZDateTime JD_E_DEP_2
		{
			get { return fJD_E_DEP_2; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_DEP_2Info, ref fJD_E_DEP_2, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_DEP_2();
				}
			}
		}
		ZDateTime fJD_E_DEP_2;

		public ZPropertyInfo JD_E_DEP_2Info
		{
			get { return GetZPropertyInfo(nameof(JD_E_DEP_2)); }
		}

		#endregion

		#region JD_E_DEP_3

		public ZDateTime JD_E_DEP_3
		{
			get { return fJD_E_DEP_3; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_DEP_3Info, ref fJD_E_DEP_3, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_DEP_3();
				}
			}
		}
		ZDateTime fJD_E_DEP_3;

		public ZPropertyInfo JD_E_DEP_3Info
		{
			get { return GetZPropertyInfo(nameof(JD_E_DEP_3)); }
		}

		#endregion

		#region JD_E_ARV_1stIntermediate

		public ZDateTime JD_E_ARV_1stIntermediate
		{
			get { return fJD_E_ARV_1stIntermediate; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_ARV_1stIntermediateInfo, ref fJD_E_ARV_1stIntermediate, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_ARV_1stIntermediate();
				}
			}
		}
		ZDateTime fJD_E_ARV_1stIntermediate;

		public ZPropertyInfo JD_E_ARV_1stIntermediateInfo
		{
			get { return GetZPropertyInfo(nameof(JD_E_ARV_1stIntermediate)); }
		}

		#endregion

		#region JD_E_ARV_2ndIntermediate

		public ZDateTime JD_E_ARV_2ndIntermediate
		{
			get { return fJD_E_ARV_2ndIntermediate; }
			set
			{
				SetNonPersistentPropertyValue(JD_E_ARV_2ndIntermediateInfo, ref fJD_E_ARV_2ndIntermediate, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_E_ARV_2ndIntermediate();
				}
			}
		}
		ZDateTime fJD_E_ARV_2ndIntermediate;

		public ZPropertyInfo JD_E_ARV_2ndIntermediateInfo
		{
			get { return GetZPropertyInfo(nameof(JD_E_ARV_2ndIntermediate)); }
		}

		#endregion

		#region JD_Milestone_E_ARV

		public ZDateTime JD_Milestone_E_ARV
		{
			get { return fJD_Milestone_E_ARV; }
			set
			{
				SetNonPersistentPropertyValue(JD_Milestone_E_ARVInfo, ref fJD_Milestone_E_ARV, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_Milestone_E_ARV();
				}
			}
		}
		ZDateTime fJD_Milestone_E_ARV;

		public ZPropertyInfo JD_Milestone_E_ARVInfo
		{
			get { return GetZPropertyInfo(Order.Schema.JD_Milestone_E_ARV); }
		}

		#endregion

		#region JD_ActualUserDate1

		public ZDateTime JD_ActualUserDate1
		{
			get { return fJD_ActualUserDate1; }
			set
			{
				SetNonPersistentPropertyValue(JD_ActualUserDate1Info, ref fJD_ActualUserDate1, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_ActualUserDate1();
				}
			}
		}
		ZDateTime fJD_ActualUserDate1;

		public ZPropertyInfo JD_ActualUserDate1Info
		{
			get { return GetZPropertyInfo(nameof(JD_ActualUserDate1)); }
		}

		#endregion

		#region JD_ActualUserDate2

		public ZDateTime JD_ActualUserDate2
		{
			get { return fJD_ActualUserDate2; }
			set
			{
				SetNonPersistentPropertyValue(JD_ActualUserDate2Info, ref fJD_ActualUserDate2, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_ActualUserDate2();
				}
			}
		}
		ZDateTime fJD_ActualUserDate2;

		public ZPropertyInfo JD_ActualUserDate2Info
		{
			get { return GetZPropertyInfo(nameof(JD_ActualUserDate2)); }
		}

		#endregion

		#region JD_ActualUserDate3

		public ZDateTime JD_ActualUserDate3
		{
			get { return fJD_ActualUserDate3; }
			set
			{
				SetNonPersistentPropertyValue(JD_ActualUserDate3Info, ref fJD_ActualUserDate3, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_ActualUserDate3();
				}
			}
		}
		ZDateTime fJD_ActualUserDate3;

		public ZPropertyInfo JD_ActualUserDate3Info
		{
			get { return GetZPropertyInfo(nameof(JD_ActualUserDate3)); }
		}

		#endregion

		#region JD_ActualUserDate4

		public ZDateTime JD_ActualUserDate4
		{
			get { return fJD_ActualUserDate4; }
			set
			{
				SetNonPersistentPropertyValue(JD_ActualUserDate4Info, ref fJD_ActualUserDate4, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_ActualUserDate4();
				}
			}
		}
		ZDateTime fJD_ActualUserDate4;

		public ZPropertyInfo JD_ActualUserDate4Info
		{
			get { return GetZPropertyInfo(nameof(JD_ActualUserDate4)); }
		}

		#endregion

		#region JD_EstimateUserDate1

		public ZDateTime JD_EstimateUserDate1
		{
			get { return fJD_EstimateUserDate1; }
			set
			{
				SetNonPersistentPropertyValue(JD_EstimateUserDate1Info, ref fJD_EstimateUserDate1, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_EstimateUserDate1();
				}
			}
		}
		ZDateTime fJD_EstimateUserDate1;

		public ZPropertyInfo JD_EstimateUserDate1Info
		{
			get { return GetZPropertyInfo(nameof(JD_EstimateUserDate1)); }
		}

		#endregion

		#region JD_EstimateUserDate2

		public ZDateTime JD_EstimateUserDate2
		{
			get { return fJD_EstimateUserDate2; }
			set
			{
				SetNonPersistentPropertyValue(JD_EstimateUserDate2Info, ref fJD_EstimateUserDate2, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_EstimateUserDate2();
				}
			}
		}
		ZDateTime fJD_EstimateUserDate2;

		public ZPropertyInfo JD_EstimateUserDate2Info
		{
			get { return GetZPropertyInfo(nameof(JD_EstimateUserDate2)); }
		}

		#endregion

		#region JD_EstimateUserDate3

		public ZDateTime JD_EstimateUserDate3
		{
			get { return fJD_EstimateUserDate3; }
			set
			{
				SetNonPersistentPropertyValue(JD_EstimateUserDate3Info, ref fJD_EstimateUserDate3, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_EstimateUserDate3();
				}
			}
		}
		ZDateTime fJD_EstimateUserDate3;

		public ZPropertyInfo JD_EstimateUserDate3Info
		{
			get { return GetZPropertyInfo(nameof(JD_EstimateUserDate3)); }
		}

		#endregion

		#region JD_EstimateUserDate4

		public ZDateTime JD_EstimateUserDate4
		{
			get { return fJD_EstimateUserDate4; }
			set
			{
				SetNonPersistentPropertyValue(JD_EstimateUserDate4Info, ref fJD_EstimateUserDate4, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_EstimateUserDate4();
				}
			}
		}
		ZDateTime fJD_EstimateUserDate4;

		public ZPropertyInfo JD_EstimateUserDate4Info
		{
			get { return GetZPropertyInfo(nameof(JD_EstimateUserDate4)); }
		}

		#endregion

		#region IsSeaTransport

		public ZBool IsSeaTransport
		{
			get
			{
				return
					JD_TransportMode == Constants.TransportModes.Sea ||
					JD_TransportMode == Constants.TransportModes.SeaAir ||
					JD_TransportMode == Constants.TransportModes.AirSea;
			}
		}

		public ZPropertyInfo IsSeaTransportInfo
		{
			get { return GetZPropertyInfo(nameof(IsSeaTransport)); }
		}

		#endregion

		#endregion

		#region Validation

		void ValidateJD_OrderStatus()
		{
			JD_OrderStatusInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(JD_OrderStatusInfo, JD_OrderStatus_List);
		}

		[BusinessObjectTestExclude]
		public ZString PropertyForEnsuringDataEntered
		{
			get { return ""; }
		}

		[BusinessObjectTestExclude]
		public ZPropertyInfo PropertyForEnsuringDataEnteredInfo
		{
			get { return GetZPropertyInfo(nameof(PropertyForEnsuringDataEntered)); }
		}

		public void ValidatePropertyForEnsuringDataEntered()
		{
			PropertyForEnsuringDataEnteredInfo.ClearAllNotifications();
			if (SelectedOrders.Count == 0)
			{
				PropertyForEnsuringDataEnteredInfo.AddError(Res.GetString("31a5049e-3165-439d-b15b-c28d166181b5", "You must select 1 or more orders to update otherwise nothing will happen"));
			}
			bool atLeastOnePropertyEntered = false;
			foreach (string propertyName in OrderToBulkUpdate.PropertiesToUpdate)
			{
				IZType value = (IZType)this[propertyName];
				if (value.IsValid && !value.IsEmpty)
				{
					atLeastOnePropertyEntered = true;
				}
			}

			if (Notes.GetAllNotes().Count > 0)
			{
				atLeastOnePropertyEntered = true;
			}

			if (!atLeastOnePropertyEntered)
			{
				PropertyForEnsuringDataEnteredInfo.AddError(Res.GetString("0fc11c00-a2be-4f35-81ab-d7a8b42c4be8", "You must enter some details to update or nothing will happen"));
			}
		}

		void ValidateJD_TransportMode()
		{
			JD_TransportModeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(JD_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(JD_TransportModeInfo, JD_TransportMode_List);
		}

		void ValidateJD_E_EXW()
		{
			JD_E_EXWInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_EXWInfo);
		}

		void ValidateJD_E_IST()
		{
			JD_E_ISTInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_ISTInfo);
		}

		void ValidateJD_E_RCV()
		{
			JD_E_RCVInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_RCVInfo);
		}

		void ValidateJD_E_CCC()
		{
			JD_E_CCCInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_CCCInfo);
		}

		void ValidateJD_E_CLR()
		{
			JD_E_CLRInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_CLRInfo);
		}

		void ValidateJD_E_UNP()
		{
			JD_E_UNPInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_UNPInfo);
		}

		void ValidateJD_E_PUP()
		{
			JD_E_PUPInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_PUPInfo);
		}

		void ValidateJD_A_EXW()
		{
			JD_A_EXWInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_A_EXWInfo);
		}

		void ValidateJD_A_RCV()
		{
			JD_A_RCVInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_A_RCVInfo);
		}

		void ValidateJD_Milestone_E_DEP()
		{
			JD_Milestone_E_DEPInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_Milestone_E_DEPInfo);
		}

		void ValidateJD_E_DEP_2()
		{
			JD_E_DEP_2Info.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_DEP_2Info);
		}

		void ValidateJD_E_DEP_3()
		{
			JD_E_DEP_3Info.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_DEP_3Info);
		}

		void ValidateJD_E_ARV_1stIntermediate()
		{
			JD_E_ARV_1stIntermediateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_ARV_1stIntermediateInfo);
		}

		void ValidateJD_E_ARV_2ndIntermediate()
		{
			JD_E_ARV_2ndIntermediateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_ARV_2ndIntermediateInfo);
		}

		void ValidateJD_Milestone_E_ARV()
		{
			JD_Milestone_E_ARVInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_Milestone_E_ARVInfo);
		}

		void ValidateJD_ActualUserDate1()
		{
			JD_ActualUserDate1Info.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_ActualUserDate1Info);
		}

		void ValidateJD_ActualUserDate2()
		{
			JD_ActualUserDate2Info.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_ActualUserDate2Info);
		}

		void ValidateJD_ActualUserDate3()
		{
			JD_ActualUserDate3Info.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_ActualUserDate3Info);
		}

		void ValidateJD_ActualUserDate4()
		{
			JD_ActualUserDate4Info.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_ActualUserDate4Info);
		}

		void ValidateJD_EstimateUserDate1()
		{
			JD_EstimateUserDate1Info.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_EstimateUserDate1Info);
		}

		void ValidateJD_EstimateUserDate2()
		{
			JD_EstimateUserDate2Info.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_EstimateUserDate2Info);
		}

		void ValidateJD_EstimateUserDate3()
		{
			JD_EstimateUserDate3Info.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_EstimateUserDate3Info);
		}

		void ValidateJD_EstimateUserDate4()
		{
			JD_EstimateUserDate4Info.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_EstimateUserDate4Info);
		}

		void ValidateJD_A_IST()
		{
			JD_A_ISTInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_A_ISTInfo);
			if (JD_A_IST.IsValid)
			{
				ValidateNoShipmentOrDeclarationOnAnyOrder(JD_A_ISTInfo);
			}
		}

		void ValidateJD_E_DEP()
		{
			JD_E_DEPInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_DEPInfo);
			if (JD_E_DEP.IsValid)
			{
				ValidateNoShipmentOrDeclarationOnAnyOrder(JD_E_DEPInfo);
			}
		}

		void ValidateJD_A_DEP()
		{
			JD_A_DEPInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_A_DEPInfo);
			if (JD_A_DEP.IsValid)
			{
				ValidateNoShipmentOrDeclarationOnAnyOrder(JD_A_DEPInfo);
			}
		}

		void ValidateJD_E_ARV()
		{
			JD_E_ARVInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_ARVInfo);
			if (JD_E_ARV.IsValid)
			{
				ValidateNoShipmentOrDeclarationOnAnyOrder(JD_E_ARVInfo);
			}
		}

		void ValidateJD_A_ARV()
		{
			JD_A_ARVInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_A_ARVInfo);
			if (JD_A_ARV.IsValid)
			{
				ValidateNoShipmentOrDeclarationOnAnyOrder(JD_A_ARVInfo);
			}
		}

		void ValidateJD_A_CCC()
		{
			JD_E_CCCInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_E_CCCInfo);
			if (JD_A_CCC.IsValid)
			{
				ValidateNoShipmentOrDeclarationOnAnyOrder(JD_A_CCCInfo);
			}
		}

		void ValidateJD_A_CLR()
		{
			JD_A_CLRInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_A_CLRInfo);
			if (JD_A_CLR.IsValid)
			{
				ValidateNoShipmentOrDeclarationOnAnyOrder(JD_A_CLRInfo);
			}
		}

		void ValidateJD_A_UNP()
		{
			JD_A_UNPInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_A_UNPInfo);
			if (JD_A_UNP.IsValid)
			{
				ValidateNoShipmentOrDeclarationOnAnyOrder(JD_A_UNPInfo);
			}
		}

		void ValidateJD_A_PUP()
		{
			JD_A_PUPInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_A_PUPInfo);
			if (JD_A_PUP.IsValid)
			{
				ValidateNoShipmentOrDeclarationOnAnyOrder(JD_A_PUPInfo);
			}
		}

		void ValidateNoShipmentOrDeclarationOnAnyOrder(ZPropertyInfo infoWithNotifications)
		{
			infoWithNotifications.ClearAllNotifications();
			foreach (OrderToBulkUpdate order in this.SelectedOrders)
			{
				if (order.Order != null &&
					(order.Order.IsShipmentAttached || order.Order.IsDeclarationAttached))
				{
					infoWithNotifications.AddError(Res.GetString("82bb4173-648d-466c-a695-24c801878ff2", "You can't modify this field because 1 or more orders have a shipment or declaration attached."));
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePropertyForEnsuringDataEntered();
			ValidateJD_OrderStatus();
			ValidateJD_TransportMode();
			ValidateJD_A_ARV();
			ValidateJD_A_CCC();
			ValidateJD_A_CLR();
			ValidateJD_A_DEP();
			ValidateJD_A_IST();
			ValidateJD_A_PUP();
			ValidateJD_A_UNP();
			ValidateJD_E_ARV();
			ValidateJD_E_DEP();
			ValidateJD_E_EXW();
			ValidateJD_E_IST();
			ValidateJD_E_RCV();
			ValidateJD_E_CCC();
			ValidateJD_E_CLR();
			ValidateJD_E_UNP();
			ValidateJD_E_PUP();
			ValidateJD_A_EXW();
			ValidateJD_A_RCV();
			ValidateJD_Milestone_E_DEP();
			ValidateJD_E_DEP_2();
			ValidateJD_E_DEP_3();
			ValidateJD_E_ARV_1stIntermediate();
			ValidateJD_E_ARV_2ndIntermediate();
			ValidateJD_Milestone_E_ARV();
			ValidateJD_ActualUserDate1();
			ValidateJD_ActualUserDate2();
			ValidateJD_ActualUserDate3();
			ValidateJD_ActualUserDate4();
			ValidateJD_EstimateUserDate1();
			ValidateJD_EstimateUserDate2();
			ValidateJD_EstimateUserDate3();
			ValidateJD_EstimateUserDate4();
		}

		#endregion

		#region Lookups

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public RefVesselCollection JD_RV_Vessel_List
		{
			get { return BindingLists.RefVessel_List; }
		}

		public OrderCollection AllOrders
		{
			get { return new OrderCollection(Factory); }
		}

		public CodeDescriptionPairList JD_TransportMode_List
		{
			get { return OrdersConstants.GetTransportModeList(); }
		}

		public CodeDescriptionPairList JD_OrderStatus_List
		{
			get
			{
				if (fJD_OrderStatus_List == null)
				{
					fJD_OrderStatus_List = new CodeDescriptionPairList(OLookUpEditType.OrderHeaderStatus);

					fJD_OrderStatus_List.AddRange(Env.Registry.OrderHeaderStatusList);

					if (GlbBranch.CurrentBranch.OrgProxy != null &&
						GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderStatusList != null)
					{
						fJD_OrderStatus_List.AddRange(new CodeDescriptionPairList(GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderStatusList));
					}
				}

				return fJD_OrderStatus_List;
			}
		}
		CodeDescriptionPairList fJD_OrderStatus_List;

		#endregion

		#region IStmNoteParent

		[BusinessObjectTestExclude]
		public Notes Notes
		{
			get { return notes ?? (notes = new Notes(this)); }
		}
		Notes notes;

		ZGuid IStmNoteParent.NotesParentPK
		{
			get { return PK; }
		}

		string IStmNoteParent.NotesParentTableName
		{
			get { return TableName; }
		}

		bool IStmNoteParent.SupportsNotes
		{
			get { return true; }
		}

		BusinessObject[] IStmNoteParent.BusinessObjectsWithRelatedNotes
		{
			get { return Array.Empty<EnterpriseBusinessObject>(); }
		}

		StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
		{
			get { return StmNoteContexts.Default; }
		}

		BusinessObjectFactory IStmNoteParent.NotesFactory
		{
			get { return Factory; }
		}

		GetValueDelegate<NoteTypeCollection> IStmNoteParent.CustomNoteTypesDelegate { get; set; }

		NoteTypeCollection IStmNoteParent.NoteTypes
		{
			get
			{
				if (noteTypes == null)
				{
					noteTypes = new NoteTypeCollection();
					noteTypes.Add(PredefinedNoteTypes.Instance.SpecialInstructions);
					noteTypes.Add(PredefinedNoteTypes.Instance.ExtraOrderDetails);
					noteTypes.Add(PredefinedNoteTypes.Instance.OrderManagementUpdate);
					noteTypes.Add(PredefinedNoteTypes.Instance.OrderManagementNote);
					noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
					noteTypes.Add(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog);
					noteTypes.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);
					noteTypes.Add(PredefinedNoteTypes.Instance.AgentNotes);
				}
				return noteTypes;
			}
		}
		NoteTypeCollection noteTypes;

		#endregion
	}
}
