using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class OGAAgencyRequirement : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OGAAgencyRequirement(BusinessObjectFactory factory, Func<ZString> getRequirement, ZPropertyInfo indicatorInfo, Action validateIndicatorPassed,
									ZPropertyInfo disclaimReasonInfo, Action validateDisclaimReasonPassed, Func<CodeDescriptionPairList> getDisclaimReasonList)
		{
			validationIndicatorPassed = validateIndicatorPassed;
			validationDisclaimReasonPassed = validateDisclaimReasonPassed;
			indicatorInfoPassed = indicatorInfo;
			disclaimReasonInfoPassed = disclaimReasonInfo;
			getRequirementDescription = getRequirement;

			factoryPassed = factory;
			this.getDisclaimReasonList = getDisclaimReasonList;
		}

		readonly BusinessObjectFactory factoryPassed;
		readonly Action validationIndicatorPassed;
		readonly Action validationDisclaimReasonPassed;
		readonly ZPropertyInfo indicatorInfoPassed;
		readonly ZPropertyInfo disclaimReasonInfoPassed;
		readonly Func<CodeDescriptionPairList> getDisclaimReasonList;
		readonly Func<ZString> getRequirementDescription;

		protected override void AddToFactoryCache()
		{
			//do not add. memory consumption issue
		}

		[ReadOnly(true)]
		public ZString AgencyCodeWithDescription
		{
			get;
			set;
		}

		internal ZString AgencyCode
		{
			get;
			set;
		}

		internal ZBool CanDisclaim
		{
			get;
			set;
		}

		[ReadOnly(true)]
		public ZString Requirement
		{
			get
			{
				return getRequirementDescription();
			}
		}

		public ZPropertyInfo RequirementInfo
		{
			get { return GetZPropertyInfo(nameof(Requirement)); }
		}

		[BusinessObjectTestExclude]
		[List(nameof(PGADisclaimReasonList))]
		[MaxLength(1)]
		public ZString DisclaimedReason
		{
			get
			{
				return disclaimReasonInfoPassed != null ? (ZString)disclaimReasonInfoPassed.Value : ZString.Empty;
			}
			set
			{
				var oldValue = DisclaimedReason;
				if (oldValue != value && disclaimReasonInfoPassed != null)
				{
					if (!IsCopying)
					{
						disclaimReasonInfoPassed.Value = value;
					}
					if (!IsValidationSuspended)
					{
						ValidateDisclaimReason();
					}
				}
				DisclaimedReasonInfo.RefreshBinding(oldValue);
			}
		}

		public CodeDescriptionPairList PGADisclaimReasonList
		{
			get { return getDisclaimReasonList == null ? new CodeDescriptionPairList() : getDisclaimReasonList(); }
		}

		public ZPropertyInfo DisclaimedReasonInfo
		{
			get { return GetZPropertyInfo(nameof(DisclaimedReason)); }
		}

		public bool DisclaimedReason_ReadOnly
		{
			get
			{
				return Indicator != OGAIndicatorList.Codes.Disclaimed || disclaimReasonInfoPassed == null ||
					PGADisclaimReasonList == null || PGADisclaimReasonList.Count == 0;
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(US_OGAIndicatorList))]
		[MaxLength(1)]
		[ReadOnlyMember(nameof(Indicator_ReadOnly))]
		public ZString Indicator
		{
			get
			{
				return indicatorInfoPassed != null ? (ZString)indicatorInfoPassed.Value : ZString.Empty;
			}
			set
			{
				var oldValue = Indicator;
				if (oldValue != value && indicatorInfoPassed != null)
				{
					indicatorInfoPassed.Value = value;
					if (!IsCopying && value != OGAIndicatorList.Codes.Disclaimed)
					{
						DisclaimedReason = ZString.Empty;
					}
					if (!IsValidationSuspended)
					{
						ValidateIndicator();
					}
				}
				IndicatorInfo.RefreshBinding(oldValue);
			}
		}

		bool Indicator_ReadOnly => indicatorInfoPassed.ReadOnly;

		public ZPropertyInfo IndicatorInfo
		{
			get { return GetZPropertyInfo(nameof(Indicator)); }
		}

		public void ValidateIndicator()
		{
			IndicatorInfo.ClearAllNotifications();

			if (!IsValidationSuspended && indicatorInfoPassed != null)
			{
				validationIndicatorPassed();
				IndicatorInfo.AddAllNotificationsFrom(indicatorInfoPassed);
			}

			ValidateDisclaimReason();
		}

		public void ValidateDisclaimReason()
		{
			DisclaimedReasonInfo.ClearAllNotifications();

			if (!IsValidationSuspended && disclaimReasonInfoPassed != null)
			{
				validationDisclaimReasonPassed();
				DisclaimedReasonInfo.AddAllNotificationsFrom(disclaimReasonInfoPassed);
			}
		}

		public CodeDescriptionPairList US_OGAIndicatorList
		{
			get { return CanDisclaim ? factoryPassed.GetCachedValue<OGAIndicatorList>() : OGAIndicatorList.GetWithoutDisclaim(factoryPassed); }
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateIndicator();
			base.RunPreSaveValidationCore();
		}
	}
}
