using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeBranchOverride : AutoAccChargeBranchOverride
	{
		public AccChargeBranchOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (YA_JobType.IsEmpty)
			{
				YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			}
			if (YA_Direction.IsEmpty)
			{
				YA_Direction = Core.Constants.FreightShipmentDirection.Code.Other;
			}
			if (YA_TransportMode.IsEmpty)
			{
				YA_TransportMode = Core.Constants.TransportModes.All;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		public GlbBranch GetBranch(Func<ZString, OrgHeader> getOrganisationByBranchDefaultingRule)
		{
			GlbBranch resultBranch = null;
			if (YA_DefaultingRule == JobInvoicingConsumerType.ChargeBranchDefaultingRulesBase.SpecificBranchAlways.Code)
			{
				resultBranch = SpecificBranch;
			}
			else if (getOrganisationByBranchDefaultingRule != null)
			{
				var orgProxy = getOrganisationByBranchDefaultingRule(YA_DefaultingRule);
				if (orgProxy != null && ChargeCode != null && ChargeCode.Company != null)
				{
					var branchesByOrgProxy = (from branch in ChargeCode.Company.Branches
											  where branch.GB_OH_OrgProxy == orgProxy.PK
											  select branch).Take(2).ToArray();
					if (branchesByOrgProxy.Length == 1)
					{
						resultBranch = Factory.Load<GlbBranch>(branchesByOrgProxy[0].PK);
					}
				}
			}

			return resultBranch;
		}

		#region Properties

		#region YA_JobType

		[List("Lookups.JobTypeList")]
		public override ZString YA_JobType
		{
			get { return base.YA_JobType; }
			set
			{
				base.YA_JobType = value;
				UpdateYA_DirectionOnReadOnly();
				UpdateYA_TransportModeOnReadOnly();
			}
		}

		public JobInvoicingConsumerType JobType
		{
			get
			{
				JobInvoicingConsumerType resultJobType = null;
				if (!YA_JobType.IsEmpty && !YA_JobTypeInfo.HasErrors())
				{
					resultJobType = Lookups.JobTypeList[YA_JobType] as JobInvoicingConsumerType;
				}

				return resultJobType;
			}
		}

		#endregion

		#region YA_Direction

		[List("Lookups.DirectionList")]
		public override ZString YA_Direction
		{
			get { return base.YA_Direction; }
			set { base.YA_Direction = value; }
		}

		protected bool YA_Direction_ReadOnly
		{
			get { return JobType == null || !JobType.IsDirectionSupported; }
		}

		void UpdateYA_DirectionOnReadOnly()
		{
			if (YA_Direction_ReadOnly)
			{
				if (!YA_Direction.IsEmpty)
				{
					direction = YA_Direction;
					YA_Direction = ZString.Empty;
				}
			}
			else
			{
				if (!direction.IsEmpty && YA_Direction.IsEmpty)
				{
					YA_Direction = direction;
				}
			}
		}
		ZString direction;

		#endregion

		#region YA_TransportMode

		[List("Lookups.TransportModeList")]
		public override ZString YA_TransportMode
		{
			get
			{
				return base.YA_TransportMode;
			}
			set
			{
				base.YA_TransportMode = value;
			}
		}

		protected bool YA_TransportMode_ReadOnly
		{
			get { return JobType == null || !JobType.IsTransportModeSupported; }
		}

		void UpdateYA_TransportModeOnReadOnly()
		{
			if (YA_TransportMode_ReadOnly)
			{
				if (!YA_TransportMode.IsEmpty)
				{
					transportMode = YA_TransportMode;
					YA_TransportMode = ZString.Empty;
				}
			}
			else
			{
				if (!transportMode.IsEmpty && YA_TransportMode.IsEmpty)
				{
					YA_TransportMode = transportMode;
				}
			}
		}
		ZString transportMode;

		#endregion

		[List("Lookups.DefaultingRuleList")]
		public override ZString YA_DefaultingRule
		{
			get { return base.YA_DefaultingRule; }
			set
			{
				base.YA_DefaultingRule = value;
				UpdateYA_GB_SpecificBranchOnReadOnly();
			}
		}

		#region YA_GB_SpecificBranch

		protected bool YA_GB_SpecificBranch_ReadOnly
		{
			get { return YA_DefaultingRule != JobInvoicingConsumerType.ChargeBranchDefaultingRulesBase.SpecificBranchAlways.Code; }
		}

		void UpdateYA_GB_SpecificBranchOnReadOnly()
		{
			if (YA_GB_SpecificBranch_ReadOnly)
			{
				if (!YA_GB_SpecificBranch.IsEmpty)
				{
					secificBranch = YA_GB_SpecificBranch;
					YA_GB_SpecificBranch = ZGuid.Empty;
				}
			}
			else
			{
				if (!secificBranch.IsEmpty && YA_GB_SpecificBranch.IsEmpty)
				{
					YA_GB_SpecificBranch = secificBranch;
				}
			}
		}
		ZGuid secificBranch;

		#endregion

		#endregion

		internal JobTypeDirectionAndTransportInfoProvider JobTypeDirectionAndTransportListProvider
			=> jobTypeDirectionAndTransportListProvider ??= new JobTypeDirectionAndTransportInfoProvider(() => YA_JobTypeInfo, () => YA_DirectionInfo, () => YA_TransportModeInfo);
		JobTypeDirectionAndTransportInfoProvider jobTypeDirectionAndTransportListProvider;
	}
}
