using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(AccChargeCode), "SupplyTypeOverrides")]
	public class AccChargeSupplyTypeOverride : AutoAccChargeSupplyTypeOverride, ISupplyTypeSelector
	{
		public AccChargeSupplyTypeOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (ACS_SupplyType.IsEmpty)
			{
				ACS_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX;
			}
			ACS_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		#region JobType

		[List("JobTypeList")]
		public override ZString ACS_JobType
		{
			get
			{
				return base.ACS_JobType;
			}
			set
			{
				base.ACS_JobType = value;
				UpdateACS_DirectionOnReadOnly();
				UpdateACS_TransportModeOnReadOnly();
				UpdateACS_IncoTermOnReadOnly();
			}
		}

		public ZString JobType
		{
			get
			{
				return ACS_JobType;
			}
			set
			{
				ACS_JobType = value;
			}
		}

		public ZPropertyInfo JobTypeInfo
		{
			get { return ACS_JobTypeInfo; }
		}

		public CodeDescriptionPairList JobTypeList
		{
			get { return SupplyTypeConfigurationLookups.JobTypeList; }
		}

		public void ValidateJobType()
		{
			Validation.ValidateACS_JobType();
		}

		#endregion

		#region Direction

		[List("DirectionList")]
		public override ZString ACS_Direction
		{
			get
			{
				return base.ACS_Direction;
			}
			set
			{
				base.ACS_Direction = value;
			}
		}

		protected bool ACS_Direction_ReadOnly => ReadOnlyHelper.DirectionCode_ReadOnly;

		void UpdateACS_DirectionOnReadOnly()
		{
			if (ACS_Direction_ReadOnly)
			{
				ACS_Direction = Constants.FreightShipmentDirection.Code.All;
			}
		}

		public ZString DirectionCode
		{
			get
			{
				return ACS_Direction;
			}
			set
			{
				ACS_Direction = value;
			}
		}

		public bool DirectionCode_ReadOnly
		{
			get { return ReadOnlyHelper.DirectionCode_ReadOnly; }
		}

		public ZPropertyInfo DirectionCodeInfo
		{
			get { return ACS_DirectionInfo; }
		}

		public CodeDescriptionPairList DirectionList
		{
			get { return SupplyTypeConfigurationLookups.DirectionList; }
		}

		public void ValidateDirectionCode()
		{
			Validation.ValidateACS_Direction();
		}

		#endregion

		#region TransportMode

		[List("ModeList")]
		public override ZString ACS_TransportMode
		{
			get
			{
				return base.ACS_TransportMode;
			}
			set
			{
				base.ACS_TransportMode = value;
			}
		}

		protected bool ACS_TransportMode_ReadOnly => ReadOnlyHelper.Mode_ReadOnly;

		void UpdateACS_TransportModeOnReadOnly()
		{
			if (ACS_TransportMode_ReadOnly)
			{
				ACS_TransportMode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			}
		}

		public ZString Mode
		{
			get
			{
				return ACS_TransportMode;
			}

			set
			{
				ACS_TransportMode = value;
			}
		}

		public bool Mode_ReadOnly
		{
			get { return ReadOnlyHelper.Mode_ReadOnly; }
		}

		public ZPropertyInfo ModeInfo
		{
			get { return ACS_TransportModeInfo; }
		}

		public CodeDescriptionPairList ModeList
		{
			get { return SupplyTypeConfigurationLookups.ModeList; }
		}

		public void ValidateMode()
		{
			Validation.ValidateACS_TransportMode();
		}

		#endregion

		#region IncoTerm

		[List("IncotermList")]
		[ReadOnlyMember(nameof(ACS_IncoTerm_ReadOnly))]
		public override ZString ACS_IncoTerm
		{
			get
			{
				return base.ACS_IncoTerm;
			}
			set
			{
				base.ACS_IncoTerm = value;
			}
		}

		bool ACS_IncoTerm_ReadOnly => ACS_JobType == JobInvoicingConsumerTypes.ForwardingConsolCode;

		void UpdateACS_IncoTermOnReadOnly()
		{
			if (ACS_IncoTerm_ReadOnly)
			{
				ACS_IncoTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			}
		}

		public ZString Incoterm
		{
			get
			{
				return ACS_IncoTerm;
			}

			set
			{
				ACS_IncoTerm = value;
			}
		}

		public ZPropertyInfo IncotermInfo
		{
			get { return ACS_IncoTermInfo; }
		}

		public CodeDescriptionPairList IncotermList
		{
			get { return SupplyTypeConfigurationLookups.IncotermList; }
		}

		public void ValidateIncoterm()
		{
			Validation.ValidateACS_IncoTerm();
		}

		#endregion

		#region SupplyType

		[List("SupplyTypeConfigurationLookups.SupplyTypeList")]
		public override ZString ACS_SupplyType
		{
			get => base.ACS_SupplyType;
			set => base.ACS_SupplyType = value;
		}

		public ZString SupplyType
		{
			get
			{
				return ACS_SupplyType;
			}
			set
			{
				ACS_SupplyType = value;
			}
		}

		public ZPropertyInfo SupplyTypeInfo
		{
			get { return ACS_SupplyTypeInfo; }
		}

		public CodeDescriptionPairList SupplyTypeList
		{
			get { return SupplyTypeConfigurationLookups.SupplyTypeList; }
		}

		public void ValidateSupplyType()
		{
			Validation.ValidateACS_SupplyType();
		}

		#endregion

		#region Department

		public ZGuid LineDepartmentPK
		{
			get
			{
				return ACS_GE;
			}
			set
			{
				ACS_GE = value;
			}
		}

		public ZPropertyInfo LineDepartmentPKInfo => ACS_GEInfo;

		public void ValidateLineDepartmentPK() => Validation.ValidateACS_GE();

		#endregion

		#region ISupplyTypeConfiguration

		JobConfigurationSelectorLookups IJobConfigurationSelector.Lookups
		{
			get { return SupplyTypeConfigurationLookups; }
		}

		public SupplyTypeConfigurationLookups SupplyTypeConfigurationLookups
		{
			get
			{
				if (fSupplyTypeConfigurationLookups == null)
				{
					fSupplyTypeConfigurationLookups = new SupplyTypeConfigurationLookups(this);
				}

				return fSupplyTypeConfigurationLookups;
			}
		}
		SupplyTypeConfigurationLookups fSupplyTypeConfigurationLookups;

		JobConfigurationSelectorReadOnly ReadOnlyHelper
		{
			get
			{
				if (fReadOnlyHelper == null)
				{
					fReadOnlyHelper = new JobConfigurationSelectorReadOnly(this);
				}

				return fReadOnlyHelper;
			}
		}
		JobConfigurationSelectorReadOnly fReadOnlyHelper;

		IJobConfigurationSelector[] IJobConfigurationSelector.ParentCollectionForValidation
		{
			get { return ParentCollectionForValidation; }
		}

		public ISupplyTypeSelector[] ParentCollectionForValidation
		{
			get
			{
				return Factory.Load<AccChargeSupplyTypeOverride>(new ZQuery(AccChargeSupplyTypeOverrideSchema.ACS_ParentID, ACS_ParentID));
			}
		}

		#endregion
	}
}
