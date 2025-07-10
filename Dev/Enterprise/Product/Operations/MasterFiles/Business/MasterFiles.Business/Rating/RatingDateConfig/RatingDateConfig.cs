using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Rating
{
	public class RatingDateConfig : AutoRatingDateConfig, IAutoRateDate
	{
		public RatingDateConfig(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.JobTypeList")]
		public override ZString RDT_JobType
		{
			get => base.RDT_JobType;
			set
			{
				base.RDT_JobType = value;

				JobConfigurationSelectorReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(RDT_DirectionInfo, ref directionCodeToRestore);
				JobConfigurationSelectorReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(RDT_TransportModeInfo, ref modeToRestore);
				AutoRateDateReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(RDT_Location_ReadOnly, RDT_LocationInfo);
				AutoRateDateReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(RDT_ContainerMode_ReadOnly, RDT_ContainerModeInfo);
			}
		}

		ZString directionCodeToRestore;
		ZString modeToRestore;

		[List("Lookups.DirectionList")]
		public override ZString RDT_Direction
		{
			get => base.RDT_Direction;
			set
			{
				base.RDT_Direction = value;
				AutoRateDateReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(RDT_Location_ReadOnly, RDT_LocationInfo);
			}
		}

		public bool RDT_Direction_ReadOnly => JobConfigurationSelectorReadOnlyHelper.DirectionCode_ReadOnly(RDT_JobType);

		[List("Lookups.TransportModeList")]
		public override ZString RDT_TransportMode
		{
			get => base.RDT_TransportMode;
			set
			{
				base.RDT_TransportMode = value;
				AutoRateDateReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(RDT_ContainerMode_ReadOnly, RDT_ContainerModeInfo);
			}
		}

		public virtual bool RDT_TransportMode_ReadOnly => JobConfigurationSelectorReadOnlyHelper.DirectionCode_ReadOnly(RDT_JobType);

		[List("Lookups.DateTypeList")]
		public override ZString RDT_AutoratingDate
		{
			get => base.RDT_AutoratingDate;
			set
			{
				base.RDT_AutoratingDate = value;

				if (value != JobDateTypes.Codes.HouseBillIssueDate)
				{
					RDT_NoFallback = false;
				}
			}
		}

		[List("Lookups.RateTypeList")]
		public override ZString RDT_RateType
		{
			get => base.RDT_RateType;
			set => base.RDT_RateType = value;
		}

		[List("Lookups.ContainerModeList")]
		public override ZString RDT_ContainerMode
		{
			get => base.RDT_ContainerMode;
			set => base.RDT_ContainerMode = value;
		}

		public bool RDT_ContainerMode_ReadOnly => AutoRateDateReadOnlyHelper.ContainerMode_ReadOnly(RDT_JobType, RDT_TransportMode);

		[List("Lookups.AutoRatingLocationCollection")]
		public override ZString RDT_Location
		{
			get => base.RDT_Location;
			set => base.RDT_Location = value;
		}

		public bool RDT_Location_ReadOnly => AutoRateDateReadOnlyHelper.Location_ReadOnly(RDT_DirectionInfo);

		#region RDT_NoFallback

		public bool RDT_NoFallback_ReadOnly => RDT_AutoratingDate != JobDateTypes.Codes.HouseBillIssueDate;

		#endregion

		#region IAutoRateDate

		ZString IAutoRateDate.JobType => RDT_JobType;
		ZString IAutoRateDate.DirectionCode => RDT_Direction;
		ZString IAutoRateDate.Mode => RDT_TransportMode;
		ZString IAutoRateDate.DateType => RDT_AutoratingDate;
		ZString IAutoRateDate.RateType => RDT_RateType;
		ZString IAutoRateDate.ContainerMode => RDT_ContainerMode;
		ZString IAutoRateDate.Location => RDT_Location;
		ZBool IAutoRateDate.IsFallbackDisabled => RDT_NoFallback;

		#endregion

		public CodeDescriptionPairList ChargeCodeGroupList
		{
			get
			{
				if (chargeCodeGroupList == null)
				{
					var chargeCodeGroupList = new CodeDescriptionPairList();
					var chargeCodeGroups = new ChargeCodeGroupList();
					foreach (var chargeCodeGroup in chargeCodeGroups.Cast<CodeDescriptionPair>().Where(x => !NonApplicableChargeGroups.Contains(x.Code)))
					{
						chargeCodeGroupList.Add(chargeCodeGroup);
					}
				}
				return chargeCodeGroupList;
			}
		}
		readonly CodeDescriptionPairList chargeCodeGroupList;

		HashSet<string> NonApplicableChargeGroups
		{
			get
			{
				if (nonApplicableChargeGroups == null)
				{
					nonApplicableChargeGroups = new HashSet<string>();
					nonApplicableChargeGroups.Add(Enterprise.MasterFiles.Business.ChargeCodeGroupList.Codes.CustomsDuty);
					nonApplicableChargeGroups.Add(Enterprise.MasterFiles.Business.ChargeCodeGroupList.Codes.NonJobRelated);
					nonApplicableChargeGroups.Add(Enterprise.MasterFiles.Business.ChargeCodeGroupList.Codes.NotGrouped);
				}

				return nonApplicableChargeGroups;
			}
		}

		HashSet<string> nonApplicableChargeGroups;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateRDT_JobType();
			Validation.ValidateRDT_Direction();
			Validation.ValidateRDT_TransportMode();
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			RDT_GC_Company = GlbCompany.CurrentCompany.PK;

			RDT_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			RDT_ParentID = orgHeader.PK;

			RDT_ChargeGroup = Enterprise.MasterFiles.Business.ChargeCodeGroupList.Codes.Origin;
			RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			RDT_Direction = Enterprise.Core.Constants.FreightShipmentDirection.Code.All;
			RDT_TransportMode = Core.Constants.TransportModes.Sea;
			RDT_RateType = JobRateTypes.Codes.Cost;
			RDT_ContainerMode = Core.Constants.ContainerModes.All;
			RDT_AutoratingDate = JobDateTypes.Codes.ArrivalDate;
		}

#endif
	}
}
