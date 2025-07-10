using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(AccChargeCode), "RevenueRecOverrides")]
	public class AccChargeRevRecOverride : AutoAccChargeRevRecOverride, IRevenueRecognition
	{
		public AccChargeRevRecOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			using (SuspendSettingHasChanges())
			{
				Offset = 0;
				OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Days;
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (AE_JobType.IsEmpty)
			{
				AE_JobType = "ALL";
			}
			if (AE_RecognitionType.IsEmpty)
			{
				AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		#region JobType

		[List("JobTypeList")]
		public override ZString AE_JobType
		{
			get
			{
				return base.AE_JobType;
			}
			set
			{
				base.AE_JobType = value;
				UpdateAE_DirectionOnReadOnly();
				UpdateAE_ModeOnReadOnly();
				UpdateAE_BrokerTypeOnReadOnly();
			}
		}

		public ZString JobType
		{
			get
			{
				return AE_JobType;
			}

			set
			{
				AE_JobType = value;
			}
		}

		public ZPropertyInfo JobTypeInfo
		{
			get { return AE_JobTypeInfo; }
		}

		public CodeDescriptionPairList JobTypeList
		{
			get { return RevenueRecognitionLookups.JobTypeList; }
		}

		public void ValidateJobType()
		{
			Validation.ValidateAE_JobType();
		}

		#endregion

		#region Direction

		[List("DirectionList")]
		public override ZString AE_Direction
		{
			get
			{
				return base.AE_Direction;
			}
			set
			{
				base.AE_Direction = value;
				UpdateAE_BrokerTypeOnReadOnly();
			}
		}

		ZString direction;

		public bool AE_Direction_ReadOnly
		{
			get { return ReadOnlyHelper.DirectionCode_ReadOnly; }
		}

		void UpdateAE_DirectionOnReadOnly()
		{
			if (AE_Direction_ReadOnly)
			{
				if (!AE_Direction.IsEmpty)
				{
					direction = AE_Direction;
					AE_Direction = ZString.Empty;
				}
			}
			else
			{
				if (!direction.IsEmpty && AE_Direction.IsEmpty)
				{
					AE_Direction = direction;
				}
			}
		}

		public ZString DirectionCode
		{
			get
			{
				return AE_Direction;
			}

			set
			{
				AE_Direction = value;
			}
		}

		public bool DirectionCode_ReadOnly
		{
			get { return ReadOnlyHelper.DirectionCode_ReadOnly; }
		}

		public ZPropertyInfo DirectionCodeInfo
		{
			get { return AE_DirectionInfo; }
		}

		public CodeDescriptionPairList DirectionList
		{
			get { return RevenueRecognitionLookups.DirectionList; }
		}

		public void ValidateDirectionCode()
		{
			Validation.ValidateAE_Direction();
		}

		#endregion

		#region Mode

		[List("ModeList")]
		public override ZString AE_Mode
		{
			get
			{
				return base.AE_Mode;
			}
			set
			{
				base.AE_Mode = value;
			}
		}

		ZString mode;

		public bool AE_Mode_ReadOnly
		{
			get { return ReadOnlyHelper.Mode_ReadOnly; }
		}

		void UpdateAE_ModeOnReadOnly()
		{
			if (AE_Mode_ReadOnly)
			{
				if (!AE_Mode.IsEmpty)
				{
					mode = AE_Mode;
					AE_Mode = ZString.Empty;
				}
			}
			else
			{
				if (!mode.IsEmpty && AE_Mode.IsEmpty)
				{
					AE_Mode = mode;
				}
			}
		}

		public ZString Mode
		{
			get
			{
				return AE_Mode;
			}

			set
			{
				AE_Mode = value;
			}
		}

		public bool Mode_ReadOnly
		{
			get { return ReadOnlyHelper.Mode_ReadOnly; }
		}

		public ZPropertyInfo ModeInfo
		{
			get { return AE_ModeInfo; }
		}

		public CodeDescriptionPairList ModeList
		{
			get { return RevenueRecognitionLookups.ModeList; }
		}

		public void ValidateMode()
		{
			Validation.ValidateAE_Mode();
		}

		#endregion

		#region RecognitionDateOption

		[List("RecognitionDateOptionList")]
		public override ZString AE_RecognitionType
		{
			get
			{
				return base.AE_RecognitionType;
			}
			set
			{
				base.AE_RecognitionType = value;
			}
		}

		public ZString RecognitionDateOptionCode
		{
			get
			{
				return AE_RecognitionType;
			}

			set
			{
				AE_RecognitionType = value;
			}
		}

		public ZPropertyInfo RecognitionDateOptionCodeInfo
		{
			get { return AE_RecognitionTypeInfo; }
		}

		public CodeDescriptionPairList RecognitionDateOptionList
		{
			get { return RevenueRecognitionLookups.RecognitionDateOptionList; }
		}

		public void ValidateRecognitionDateOptionCode()
		{
			Validation.ValidateAE_RecognitionType();
		}

		#endregion

		#region Broker

		[List("BrokerList")]
		public override ZString AE_BrokerType
		{
			get
			{
				return base.AE_BrokerType;
			}
			set
			{
				base.AE_BrokerType = value;
			}
		}

		ZString brokerType;

		public bool AE_BrokerType_ReadOnly
		{
			get { return ReadOnlyHelper.BrokerCode_ReadOnly; }
		}

		void UpdateAE_BrokerTypeOnReadOnly()
		{
			if (AE_BrokerType_ReadOnly)
			{
				if (!AE_BrokerType.IsEmpty)
				{
					brokerType = AE_BrokerType;
					AE_BrokerType = ZString.Empty;
				}
			}
			else
			{
				if (!brokerType.IsEmpty && AE_BrokerType.IsEmpty)
				{
					AE_BrokerType = brokerType;
				}
			}
		}

		public ZString BrokerCode
		{
			get
			{
				return AE_BrokerType;
			}

			set
			{
				AE_BrokerType = value;
			}
		}

		public bool BrokerCode_ReadOnly
		{
			get { return ReadOnlyHelper.BrokerCode_ReadOnly; }
		}

		public ZPropertyInfo BrokerCodeInfo
		{
			get { return AE_BrokerTypeInfo; }
		}

		public CodeDescriptionPairList BrokerList
		{
			get { return RevenueRecognitionLookups.BrokerList; }
		}

		public void ValidateBrokerCode()
		{
			Validation.ValidateAE_BrokerType();
		}

		#endregion

		#region Offset

		ZInt fOffset;
		public ZInt Offset
		{
			get
			{
				return fOffset;
			}
			set
			{
				SetNonPersistentPropertyValue(OffsetInfo, ref fOffset, value);
			}
		}

		public bool Offset_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo OffsetInfo
		{
			get { return GetZPropertyInfo(nameof(Offset)); }
		}

		public void ValidateOffset()
		{
		}

		#endregion

		#region OffsetType

		ZString fOffsetType;

		[MaxLength(3)]
		public ZString OffsetType
		{
			get
			{
				return fOffsetType;
			}
			set
			{
				CheckMaximumLength(OffsetTypeInfo, value);
				SetNonPersistentPropertyValue(OffsetTypeInfo, ref fOffsetType, value);
			}
		}

		public bool OffsetType_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo OffsetTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OffsetType)); }
		}

		public CodeDescriptionPairList OffsetTypeList
		{
			get { return RevenueRecognitionLookups.OffsetTypeList; }
		}

		public void ValidateOffsetType()
		{
		}

		#endregion

		#region Implementation

		IJobConfigurationSelector[] IJobConfigurationSelector.ParentCollectionForValidation
		{
			get { return ParentCollectionForValidation; }
		}

		public IRevenueRecognition[] ParentCollectionForValidation
		{
			get
			{
				return Factory.Load<AccChargeRevRecOverride>(new ZQuery(AccChargeRevRecOverrideSchema.AE_AC, AE_AC));
			}
		}

		JobConfigurationSelectorLookups IJobConfigurationSelector.Lookups
		{
			get { return RevenueRecognitionLookups; }
		}

		public RevenueRecognitionLookups RevenueRecognitionLookups
		{
			get
			{
				if (RevenueRecognitionLookups_internal == null)
				{
					RevenueRecognitionLookups_internal = new RevenueRecognitionLookups(this);
				}

				return RevenueRecognitionLookups_internal;
			}
		}
		RevenueRecognitionLookups RevenueRecognitionLookups_internal;

		public RevenueRecognitionReadOnly ReadOnlyHelper
		{
			get
			{
				if (ReadOnlyHelper_internal == null)
				{
					ReadOnlyHelper_internal = new RevenueRecognitionReadOnly(this);
				}

				return ReadOnlyHelper_internal;
			}
		}
		RevenueRecognitionReadOnly ReadOnlyHelper_internal;

		#endregion
	}
}
