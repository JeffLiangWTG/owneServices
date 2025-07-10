using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(AccChargeCode), "GLPostingOverrides")]
	public class AccChargeGLPostingOverride : AutoAccChargeGLPostingOverride
	{
		public AccChargeGLPostingOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.GLAccrualAccountCollection")]
		public override ZGuid Y1_AG_ACR
		{
			get
			{
				return base.Y1_AG_ACR;
			}
			set
			{
				base.Y1_AG_ACR = value;
			}
		}

		[List("Lookups.GLCostAccountCollection")]
		public override ZGuid Y1_AG_CST
		{
			get
			{
				return base.Y1_AG_CST;
			}
			set
			{
				base.Y1_AG_CST = value;
			}
		}

		[List("Lookups.GLRevenueAccountCollection")]
		public override ZGuid Y1_AG_REV
		{
			get
			{
				return base.Y1_AG_REV;
			}
			set
			{
				base.Y1_AG_REV = value;
			}
		}

		[List("Lookups.GLWIPAccountCollection")]
		public override ZGuid Y1_AG_WIP
		{
			get
			{
				return base.Y1_AG_WIP;
			}
			set
			{
				base.Y1_AG_WIP = value;
			}
		}

		[List("Lookups.ConsolidationAccountingCategoryClassCollection")]
		public override ZString Y1_ConsolidationAccountingCategoryClass
		{
			get => base.Y1_ConsolidationAccountingCategoryClass;
			set => base.Y1_ConsolidationAccountingCategoryClass = value;
		}

		[List("Lookups.GLCostClearingAccountCollection")]
		[ReadOnlyMember(nameof(Y1_AG_CST_Clearing_ReadOnly))]
		public override ZGuid Y1_AG_CST_Clearing
		{
			get => base.Y1_AG_CST_Clearing;
			set => base.Y1_AG_CST_Clearing = value;
		}

		[List("Lookups.GLRevenueClearingAccountCollection")]
		[ReadOnlyMember(nameof(Y1_AG_REV_Clearing_ReadOnly))]
		public override ZGuid Y1_AG_REV_Clearing
		{
			get => base.Y1_AG_REV_Clearing;
			set => base.Y1_AG_REV_Clearing = value;
		}

		#region Y1_JobType

		[List("Lookups.JobTypeList")]
		public override ZString Y1_JobType
		{
			get => base.Y1_JobType;
			set
			{
				base.Y1_JobType = value;
				SetNotApplicableFiledsToAll();
			}
		}

		JobInvoicingConsumerType JobType
		{
			get
			{
				JobInvoicingConsumerType resultJobType = null;
				if (!Y1_JobType.IsEmpty && !Y1_JobTypeInfo.HasErrors())
				{
					resultJobType = Lookups.JobTypeList[Y1_JobType, StringComparison.CurrentCultureIgnoreCase] as JobInvoicingConsumerType;
				}

				return resultJobType;
			}
		}

		void SetNotApplicableFiledsToAll()
		{
			if (Y1_TransportMode_ReadOnly)
			{
				Y1_TransportMode = AccChargeGLPostingOverrideLookups.TransportModeAdditionalCodes.All;
			}

			if (Y1_Direction_ReadOnly)
			{
				Y1_Direction = Enterprise.Core.Constants.FreightShipmentDirection.Code.All;
			}

			if (Y1_ConsolContainerMode_ReadOnly)
			{
				Y1_ConsolContainerMode = AccChargeGLPostingOverrideLookups.ConsolContainerModeAdditionalCodes.All;
			}

			if (Y1_MasterPaymentType_ReadOnly)
			{
				Y1_MasterPaymentType = AccChargeGLPostingOverrideLookups.MasterPaymentTypeAdditionalCodes.All;
			}

			if (Y1_HousePaymentType_ReadOnly)
			{
				Y1_HousePaymentType = AccChargeGLPostingOverrideLookups.HousePaymentTypeAdditionalCodes.All;
			}
		}

		#endregion

		[List("Lookups.TransportModeList")]
		public override ZString Y1_TransportMode
		{
			get => base.Y1_TransportMode;
			set => base.Y1_TransportMode = value;
		}

		[List("Lookups.DirectionList")]
		public override ZString Y1_Direction
		{
			get => base.Y1_Direction;
			set => base.Y1_Direction = value;
		}

		[List("Lookups.ConsolContainerModeList")]
		public override ZString Y1_ConsolContainerMode
		{
			get => base.Y1_ConsolContainerMode;
			set => base.Y1_ConsolContainerMode = value;
		}

		[List("Lookups.MasterPaymentTypeList")]
		public override ZString Y1_MasterPaymentType
		{
			get => base.Y1_MasterPaymentType;
			set => base.Y1_MasterPaymentType = value;
		}

		[List("Lookups.HousePaymentTypeList")]
		public override ZString Y1_HousePaymentType
		{
			get => base.Y1_HousePaymentType;
			set => base.Y1_HousePaymentType = value;
		}

		#endregion

		#region Readonlyness

		bool Y1_AG_REV_Clearing_ReadOnly => ChargeCode.AC_AG_RevenueClearingAccountReadOnly;
		bool Y1_AG_CST_Clearing_ReadOnly => ChargeCode.AC_AG_CostClearingAccountReadOnly;

		protected bool Y1_AG_REV_ReadOnly
		{
			get
			{
				AccChargeCode.PropertyRequired accountsRequired = ChargeCode.RequiredProperties(ChargeCode.HighestChargeType);
				return (accountsRequired.IsValid && !accountsRequired.RevenueAccount) || !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed;
			}
		}

		protected virtual bool Y1_AG_WIP_ReadOnly
		{
			get
			{
				AccChargeCode.PropertyRequired accountsRequired = ChargeCode.RequiredProperties(ChargeCode.HighestChargeType);
				return (accountsRequired.IsValid && !accountsRequired.WIPAccount) || !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed;
			}
		}

		protected virtual bool Y1_AG_CST_ReadOnly
		{
			get
			{
				AccChargeCode.PropertyRequired accountsRequired = ChargeCode.RequiredProperties(ChargeCode.HighestChargeType);
				return (accountsRequired.IsValid && !accountsRequired.CostAccount) || !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed;
			}
		}

		protected virtual bool Y1_AG_ACR_ReadOnly
		{
			get
			{
				AccChargeCode.PropertyRequired accountsRequired = ChargeCode.RequiredProperties(ChargeCode.HighestChargeType);
				return (accountsRequired.IsValid && !accountsRequired.AccrualAccount) || !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed;
			}
		}

		protected bool Y1_TransportMode_ReadOnly
		{
			get { return JobType == null || !JobType.IsTransportModeSupported; }
		}

		protected bool Y1_Direction_ReadOnly
		{
			get { return JobType == null || !JobType.IsDirectionSupported; }
		}
		protected bool Y1_ConsolContainerMode_ReadOnly => ContainerModeAndPaymentTypeApplicable;

		protected bool Y1_MasterPaymentType_ReadOnly => ContainerModeAndPaymentTypeApplicable;

		protected bool Y1_HousePaymentType_ReadOnly => Y1_JobType != JobInvoicingConsumerTypes.ShipmentCode;

		bool ContainerModeAndPaymentTypeApplicable => Y1_JobType != JobInvoicingConsumerTypes.ForwardingConsolCode &&
														Y1_JobType != JobInvoicingConsumerTypes.ShipmentCode &&
														Y1_JobType != JobInvoicingConsumerTypes.GatewayConsolCode;

		#endregion

		#region Security Checkpoints

		SecurityCheckpoint ChargeCodesEditGLAccountSetupCheckPoint
		{
			get
			{
				return ChargeCode.IsGlobal ? Env.Security.GlobalChargeCodesEditGLAccountSetup :
					ChargeCode.IsLinkedToGlobalChargeCode ? Env.Security.ChargeCodesLTGEditGLAccountSetup :
					Env.Security.ChargeCodesEditGLAccountSetup;
			}
		}

		#endregion

		internal JobTypeDirectionAndTransportInfoProvider JobTypeDirectionAndTransportListProvider
			=> jobTypeDirectionAndTransportListProvider ??= new JobTypeDirectionAndTransportInfoProvider(() => Y1_JobTypeInfo, () => Y1_DirectionInfo, () => Y1_TransportModeInfo);
		JobTypeDirectionAndTransportInfoProvider jobTypeDirectionAndTransportListProvider;
	}
}
