using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[System.CodeDom.Compiler.GeneratedCode("CargoWise.EntityFramework", "1.0")]
	public class AccChargeGovtChargeCodeOverride : AutoAccChargeGovtChargeCodeOverride, IJobConfiguration
	{
		public AccChargeGovtChargeCodeOverride(BusinessObjectFactory Factory, DataRow Row) : base(Factory, Row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ACG_Direction = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			ACG_TransportMode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
		}

		#region CostSellAll

		[List("Lookups.CostSellList")]
		public override ZString ACG_CostSellAll
		{
			get => base.ACG_CostSellAll;
			set => base.ACG_CostSellAll = value;
		}

		#endregion

		#region JobType

		[List("Lookups.JobTypes")]
		public override ZString ACG_JobType
		{
			get => base.ACG_JobType;
			set => base.ACG_JobType = value;
		}

		public JobInvoicingConsumerType JobType
		{
			get
			{
				JobInvoicingConsumerType resultJobType = null;
				if (!base.ACG_JobType.IsEmpty && !base.ACG_JobTypeInfo.HasErrors())
				{
					resultJobType = Lookups.JobTypes[ACG_JobType] as JobInvoicingConsumerType;
				}
				return resultJobType;
			}
		}

		#endregion

		#region Direction

		[List("Lookups.DirectionList")]
		[BusinessObjectEmptyStringTestExcludeAttribute]
		public override ZString ACG_Direction
		{
			get => base.ACG_Direction;
			set => base.ACG_Direction = string.IsNullOrWhiteSpace(value) ? (ZString)JobConfigurationSelectorLookups.ModeAdditionalCodes.All : value;
		}

		protected bool ACG_Direction_ReadOnly => !(JobType?.IsDirectionSupported ?? false);

		#endregion

		#region TransportMode

		[List("Lookups.TransportModeList")]
		[BusinessObjectEmptyStringTestExcludeAttribute]
		public override ZString ACG_TransportMode
		{
			get => base.ACG_TransportMode;
			set => base.ACG_TransportMode = string.IsNullOrWhiteSpace(value) ? (ZString)JobConfigurationSelectorLookups.ModeAdditionalCodes.All : value;
		}

		protected bool ACG_TransportMode_ReadOnly => !(JobType?.IsTransportModeSupported ?? false);

		#endregion

		[MaxLength(23)]
		public override ZString ACG_GovtChargeCode
		{
			get => base.ACG_GovtChargeCode;
			set
			{
				if (ACG_GovtChargeCode != value)
				{
					CheckMaximumLength(ACG_GovtChargeCodeInfo, value);
					base.ACG_GovtChargeCode = value;
				}
			}
		}

		public bool IsDuplicateOf(AccChargeGovtChargeCodeOverride GovChargeCodeConfig)
		{
			return ACG_AC == GovChargeCodeConfig.ACG_AC
				&& ACG_CostSellAll == GovChargeCodeConfig.ACG_CostSellAll
				&& ACG_JobType == GovChargeCodeConfig.ACG_JobType
				&& ACG_Direction == GovChargeCodeConfig.ACG_Direction
				&& ACG_TransportMode == GovChargeCodeConfig.ACG_TransportMode;
		}

		#region IJobConfiguration members

		ZString IJobConfiguration.JobType => ACG_JobType;
		ZString IJobConfiguration.ServiceDirection => ACG_Direction;
		ZString IJobConfiguration.TransportMode => ACG_TransportMode;
		bool IJobConfiguration.IncludeOptionsForAllJobTypes => false;

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			this.ACG_JobType = "ALL";
		}
#endif

		internal JobTypeDirectionAndTransportInfoProvider JobTypeDirectionAndTransportListProvider
			=> jobTypeDirectionAndTransportListProvider ??= new JobTypeDirectionAndTransportInfoProvider(() => ACG_JobTypeInfo, () => ACG_DirectionInfo, () => ACG_TransportModeInfo);
		JobTypeDirectionAndTransportInfoProvider jobTypeDirectionAndTransportListProvider;
	}
}
