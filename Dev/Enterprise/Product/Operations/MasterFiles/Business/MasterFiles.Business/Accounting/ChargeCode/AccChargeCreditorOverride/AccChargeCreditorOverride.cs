using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeCreditorOverride : AutoAccChargeCreditorOverride
	{
		public AccChargeCreditorOverride(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region JobType

		[List("Lookups.JobTypeList")]
		public override ZString ACC_JobType
		{
			get => base.ACC_JobType;
			set
			{
				base.ACC_JobType = value;
				UpdateAccDirectionByJobType();
				UpdateAccTransportModeByJobType();
				UpdateAccDepartmentByJobType();
				UpdateAccPaymentTermByJobType();
				UpdateAccCreditorRole();
			}
		}

		JobInvoicingConsumerType JobType
		{
			get
			{
				JobInvoicingConsumerType resultJobType = null;
				if (!ACC_JobType.IsEmpty && !ACC_JobTypeInfo.HasErrors())
				{
					resultJobType = Lookups.JobTypeList[ACC_JobType, StringComparison.CurrentCultureIgnoreCase] as JobInvoicingConsumerType;
				}

				return resultJobType;
			}
		}

		#endregion

		#region TransportMode

		[List("Lookups.TransportModeList")]
		public override ZString ACC_TransportMode
		{
			get => base.ACC_TransportMode;
			set => base.ACC_TransportMode = value;
		}

		protected bool ACC_TransportMode_ReadOnly
		{
			get { return JobType == null || !JobType.IsTransportModeSupported; }
		}

		void UpdateAccTransportModeByJobType()
		{
			if (ACC_Direction_ReadOnly)
			{
				previousTransportMode = ACC_TransportMode;
				ACC_TransportMode = ZString.Empty;
			}
			else
			{
				if (!previousTransportMode.IsEmpty && ACC_TransportMode.IsEmpty)
				{
					ACC_TransportMode = previousTransportMode;
				}
			}
		}
		ZString previousTransportMode;
		public bool TransportModeIsReadOnly => ACC_TransportMode_ReadOnly;

		#endregion

		#region Direction

		[List("Lookups.DirectionList")]
		public override ZString ACC_Direction
		{
			get => base.ACC_Direction;
			set
			{
				base.ACC_Direction = value;
				UpdateAccCreditorRole();
			}
		}

		protected bool ACC_Direction_ReadOnly
		{
			get { return JobType == null || !JobType.IsDirectionSupported; }
		}

		void UpdateAccDirectionByJobType()
		{
			if (ACC_Direction_ReadOnly)
			{
				previousDirection = ACC_Direction;
				ACC_Direction = ZString.Empty;
			}
			else
			{
				if (!previousDirection.IsEmpty && ACC_Direction.IsEmpty)
				{
					ACC_Direction = previousDirection;
				}
			}
		}

		ZString previousDirection;
		public bool DirectionIsReadOnly => ACC_Direction_ReadOnly;

		#endregion

		#region DefaultingRule

		[List("Lookups.DefaultingRuleList")]
		public override ZString ACC_DefaultingRule
		{
			get => base.ACC_DefaultingRule;
			set => base.ACC_DefaultingRule = value;
		}

		#endregion

		#region Department

		protected bool ACC_GE_Department_ReadOnly
		{
			get { return JobType == null || !JobType.IsActive; }
		}

		void UpdateAccDepartmentByJobType()
		{
			if (ACC_GE_Department_ReadOnly)
			{
				previousDepartment = ACC_GE_Department;
				ACC_GE_Department = ZGuid.Empty;
			}
			else
			{
				if (!previousDepartment.IsEmpty && ACC_GE_Department.IsEmpty)
				{
					ACC_GE_Department = previousDepartment;
				}
			}
		}
		ZGuid previousDepartment;
		public bool DepartmentIsReadOnly => ACC_GE_Department_ReadOnly;

		#endregion

		#region Creditor

		[RelatedBusinessObject("Creditor")]
		[List("Lookups.Creditors")]
		public override ZGuid ACC_OH_Creditor
		{
			get
			{
				return base.ACC_OH_Creditor;
			}
			set
			{
				base.ACC_OH_Creditor = value;
				Validation.ValidateACC_CreditorRole();
			}
		}

		public OrgHeader CreditorOrganisation
		{
			get
			{
				if (ACC_OH_Creditor == ZGuid.Empty)
				{
					return null;
				}
				ZQuery query = new ZQuery(OrgHeaderSchema.PK, ACC_OH_Creditor);
				Lookups.Creditors.Load(query);
				if (Lookups.Creditors.Count > 0)
				{
					return Lookups.Creditors[0];
				}

				return null;
			}
		}

		public ZString CreditorName
		{
			get
			{
				if (CreditorOrganisation == null)
				{
					return ZString.Empty;
				}
				return CreditorOrganisation.OH_FullName;
			}
		}

		#endregion

		#region PaymentTerm

		[List(nameof(Lookups) + "." + nameof(AccChargeCreditorOverrideLookups.PaymentTermList))]
		public override ZString ACC_PaymentTerm
		{
			get => base.ACC_PaymentTerm;
			set => base.ACC_PaymentTerm = value;
		}

		protected bool ACC_PaymentTerm_ReadOnly => !IsJobTypeShipmentOrConsol(ACC_JobType);

		public static bool IsJobTypeShipmentOrConsol(string jobType) => jobType == JobInvoicingConsumerTypes.ShipmentCode
			|| jobType == JobInvoicingConsumerTypes.ForwardingConsolCode
			|| jobType == JobInvoicingConsumerTypes.GatewayConsolCode;

		public bool PaymentTermIsReadOnly => ACC_PaymentTerm_ReadOnly;

		void UpdateAccPaymentTermByJobType()
		{
			if (ACC_PaymentTerm_ReadOnly)
			{
				ACC_PaymentTerm = ZString.Empty;
			}
		}

		#endregion

		#region CreditorRole

		[List(nameof(Lookups) + "." + nameof(AccChargeCreditorOverrideLookups.CreditorRoleList))]
		public override ZString ACC_CreditorRole
		{
			get => base.ACC_CreditorRole;
			set
			{
				base.ACC_CreditorRole = value;
				Validation.ValidateACC_OH_Creditor();
			}
		}

		protected bool ACC_CreditorRole_ReadOnly
		{
			get
			{
				if (!IsJobTypeShipmentOrConsol(ACC_JobType))
				{
					return true;
				}

				if (!(ACC_Direction == Constants.FreightShipmentDirection.Code.Import
					|| ACC_Direction == Constants.FreightShipmentDirection.Code.Export))
				{
					return true;
				}

				return false;
			}
		}

		public bool CreditorRoleIsReadOnly => ACC_CreditorRole_ReadOnly;

		void UpdateAccCreditorRole()
		{
			if (ACC_CreditorRole_ReadOnly)
			{
				ACC_CreditorRole = ZString.Empty;
			}
		}

		#endregion

		#region For test
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (ACC_AC_ChargeCode.IsEmpty)
			{
				ACC_AC_ChargeCode = Factory.New<AccChargeCode>().PK;
			}
			ACC_TransportMode = "ALL";
			ACC_DefaultingRule = "SCA";
			ACC_Direction = "ALL";
			ACC_JobType = "ALL";
			if (ACC_OH_Creditor.IsEmpty)
			{
				var creditor = Factory.NewWithValidTestData<OrgHeader>();
				creditor.OH_Code = "ABC";
				ACC_OH_Creditor = creditor.PK;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion

		internal JobTypeDirectionAndTransportInfoProvider JobTypeDirectionAndTransportListProvider
			=> jobTypeDirectionAndTransportListProvider ??= new JobTypeDirectionAndTransportInfoProvider(() => ACC_JobTypeInfo, () => ACC_DirectionInfo, () => ACC_TransportModeInfo);
		JobTypeDirectionAndTransportInfoProvider jobTypeDirectionAndTransportListProvider;
	}
}
