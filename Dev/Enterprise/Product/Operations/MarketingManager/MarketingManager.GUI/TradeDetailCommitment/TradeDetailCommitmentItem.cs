using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.GUI
{
	public class TradeDetailCommitmentItem : TradeDetailSelectionItem
	{
		public TradeDetailCommitmentItem(OrgTradeDetail tradeDetail, ZString opportunityStatus)
			: base(tradeDetail)
		{
			Argument.NotNull(tradeDetail, nameof(tradeDetail));
			this.opportunityStatus = opportunityStatus;
			Init(tradeDetail, opportunityStatus);
			TradeDetail.ProspectPeriodEndTypeInfo.ValueChanged += (s, e) => { ProspectPeriodEndTypeInfo.RefreshBinding(); };
		}

		readonly ZString opportunityStatus;

		void Init(OrgTradeDetail tradeDetail, ZString oppStatus)
		{
			if (TradeDetailStatusList.ContainsCode(tradeDetail.PA_Status))
			{
				TradeDetailStatus = tradeDetail.PA_Status;
			}
			else
			{
				TradeDetailStatus = OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetTradeStatusFromCode(oppStatus);
			}

			ExpectedTradeStartDate = tradeDetail.ProspectDetail.PAP_ExpectedTradeStartDate;

			if (tradeDetail.ProspectPeriodStart.IsValid)
			{
				ProspectPeriodStart = tradeDetail.ProspectPeriodStart;
			}

			if (tradeDetail.ProspectPeriodEnd.IsValid)
			{
				ProspectPeriodEnd = tradeDetail.ProspectPeriodEnd;
			}

			if (!tradeDetail.ProspectPeriodEndType.IsEmpty)
			{
				ProspectPeriodEndType = tradeDetail.ProspectPeriodEndType;
			}
			else
			{
				SetDefaultProspectPeriodEndType();
			}

			if (TradeDetailStatus == OpportunityTradeStatus.Codes.Successful)
			{
				ForecastType = OrgTradeProspectForecastTypeList.Codes.Static;
			}
		}

		#region Properties

		#region TradeDetailStatus

		[MaxLength(30)]
		[ResourceStringData("Enterprise.MarketingManager.GUI.OpportunityTradeDetailItem|TradeDetailStatus", Caption = "Status")]
		[List("TradeDetailStatusList")]
		public ZString TradeDetailStatusDescription
		{
			get => !TradeDetailStatusDescription_ReadOnly ? (ZString)TradeDetailStatusList.GetDescriptionFromCode(TradeDetailStatus) : TradeDetail.PA_StatusDescription;
			set
			{
				if (!TradeDetailStatusDescription_ReadOnly)
				{
					TradeDetailStatus = TradeDetailStatusList.GetCodeFromDescription(value) ?? ZString.Empty;
				}
			}
		}

		public ZWrappedPropertyInfo TradeDetailStatusDescriptionInfo => GetWrappedZPropertyInfo(nameof(TradeDetailStatus), x => TradeDetailStatusInfo);

		public bool TradeDetailStatusDescription_ReadOnly => IsSuperceded || IsExpired;

		[MaxLength(3)]
		[List("TradeDetailStatusList")]
		public ZString TradeDetailStatus
		{
			get => tradeDetailStatus;
			set
			{
				SetNonPersistentPropertyValue(TradeDetailStatusInfo, ref tradeDetailStatus, value);
				ValidateTradeDetailStatus();

				if (value != OpportunityTradeStatus.Codes.Successful)
				{
					ProspectPeriodEndType = ZString.Empty;
					ProspectPeriodStart = ZDate.Empty;
					ProspectPeriodEnd = ZDate.Empty;
					ForecastType = ZString.Empty;
					ValidateExpectedTradeStartDate();
				}
				else if (ExpectedTradeStartDate.IsValid)
				{
					ProspectPeriodStart = new ZDate(ExpectedTradeStartDate.Year, ExpectedTradeStartDate.Month, 1);
					SetDefaultProspectPeriodEndType();
					UpdateProspectPeriodEnd();
				}
			}
		}
		ZString tradeDetailStatus;

		public ZPropertyInfo TradeDetailStatusInfo => GetZPropertyInfo(nameof(TradeDetailStatus));

		public CodeDescriptionPairList TradeDetailStatusList
		{
			get
			{
				return Factory.GetCachedValue("OpportunityTradeDetailSelectionItem.TradeDetailStatusList_" + opportunityStatus, () =>
				{
					var statusList = new CodeDescriptionPairList();

					var tradeStatus = OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetTradeStatusFromCode(opportunityStatus);

					if (tradeStatus == OpportunityTradeStatus.Codes.Successful || tradeStatus == OpportunityTradeStatus.Codes.Unsuccessful)
					{
						statusList.AddPair(OpportunityTradeStatus.Codes.Successful, OpportunityTradeStatus.Descriptions.Successful);
						statusList.AddPair(OpportunityTradeStatus.Codes.Unsuccessful, OpportunityTradeStatus.Descriptions.Unsuccessful);
					}
					else if (tradeStatus == OpportunityTradeStatus.Codes.Active)
					{
						statusList.AddPair(OpportunityTradeStatus.Codes.Active, OpportunityTradeStatus.Descriptions.Active);
					}

					return statusList;
				});
			}
		}

		#endregion

		#region ExpectedTradeStartDate

		[ResourceStringData("Enterprise.MarketingManager.GUI.OpportunityTradeDetailItem|ExpectedTradeStartDate", Caption = "Expected Start Date")]
		public ZDate ExpectedTradeStartDate
		{
			get => expectedTradeStartDate;
			set
			{
				SetNonPersistentPropertyValue(ExpectedTradeStartDateInfo, ref expectedTradeStartDate, value);
				ValidateExpectedTradeStartDate();
				if (value.IsValid && TradeDetailStatus == OpportunityTradeStatus.Codes.Successful)
				{
					ProspectPeriodStart = new ZDate(value.Year, value.Month, 1);
					UpdateProspectPeriodEnd();
				}
			}
		}
		ZDate expectedTradeStartDate;

		public bool ExpectedTradeStartDate_ReadOnly => TradeDetailStatus != OpportunityTradeStatus.Codes.Successful || TradeDetailStatusDescription_ReadOnly;

		public ZPropertyInfo ExpectedTradeStartDateInfo => GetZPropertyInfo(nameof(ExpectedTradeStartDate));

		#endregion

		#region ProspectPeriodEndType

		[ResourceStringData("Enterprise.MarketingManager.GUI.OpportunityTradeDetailItem|ProspectPeriodEndType", Caption = "Trade Commitment")]
		[List("OrgTradeProspectPeriodEndTypes")]
		public ZString ProspectPeriodEndType
		{
			get => prospectPeriodEndType;
			set
			{
				SetNonPersistentPropertyValue(ProspectPeriodEndTypeInfo, ref prospectPeriodEndType, value);
				ValidateProspectPeriodEndType();
				UpdateProspectPeriodEnd();
			}
		}
		ZString prospectPeriodEndType;

		public ICodeDescriptionPairList OrgTradeProspectPeriodEndTypes
		{
			get { return new OrgTradeProspectPeriodEndTypeList(); }
		}

		public ZPropertyInfo ProspectPeriodEndTypeInfo => GetZPropertyInfo(nameof(ProspectPeriodEndType));

		public bool ProspectPeriodEndType_ReadOnly => TradeDetailStatus != OpportunityTradeStatus.Codes.Successful || TradeDetailStatusDescription_ReadOnly;

		void SetDefaultProspectPeriodEndType()
		{
			if (TradeDetailStatus == OpportunityTradeStatus.Codes.Successful)
			{
				ProspectPeriodEndType = tradeDetail.ProspectDetail.PAP_RecurrenceType == OrgTradeProspectRecurrenceTypeList.Codes.OneOff
											? OrgTradeProspectPeriodEndTypeList.Codes._1Month
											: OrgTradeProspectPeriodEndTypeList.Codes._12Months;
			}
		}

		void UpdateProspectPeriodEnd()
		{
			if (ProspectPeriodStart.IsValid && !ProspectPeriodEndType.IsEmpty && TradeDetailStatus == OpportunityTradeStatus.Codes.Successful)
			{
				switch (ProspectPeriodEndType)
				{
					case OrgTradeProspectPeriodEndTypeList.Codes._1Month:
						ProspectPeriodEnd = ProspectPeriodStart;
						break;
					case OrgTradeProspectPeriodEndTypeList.Codes._3Months:
						ProspectPeriodEnd = ProspectPeriodStart.AddMonths(2);
						break;
					case OrgTradeProspectPeriodEndTypeList.Codes._6Months:
						ProspectPeriodEnd = ProspectPeriodStart.AddMonths(5);
						break;
					case OrgTradeProspectPeriodEndTypeList.Codes._12Months:
						ProspectPeriodEnd = ProspectPeriodStart.AddMonths(11);
						break;
					case OrgTradeProspectPeriodEndTypeList.Codes.EndOfCurrentFinancialYear:
						ProspectPeriodEnd = GetFirstDayOfCurrentFinancialYearEndPeriod();
						break;
					case OrgTradeProspectPeriodEndTypeList.Codes.EndOfNextFinancialYear:
						ProspectPeriodEnd = GetFirstDayOfCurrentFinancialYearEndPeriod().AddYears(1);
						break;
				}
			}
		}

		ZDate GetFirstDayOfCurrentFinancialYearEndPeriod()
		{
			var result = ZDate.Empty;

			var today = ZDate.Today;
			var firstDayOfCurrentCalendarMonth = new ZDate(today.Year, today.Month, 1);
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentFinancialPeriod = periodCalculator.GetPeriodFromDate(today);
			if (currentFinancialPeriod > 0)
			{
				result = firstDayOfCurrentCalendarMonth.AddMonths(12 - currentFinancialPeriod % 100);
			}
			return result;
		}

		#endregion

		#region ProspectPeriodStart

		[ResourceStringData("Enterprise.MarketingManager.GUI.OpportunityTradeDetailItem|ProspectPeriodStart", Caption = "Start Period")]
		public ZDate ProspectPeriodStart
		{
			get => prospectPeriodStart;
			set
			{
				SetNonPersistentPropertyValue(ProspectPeriodStartInfo, ref prospectPeriodStart, value);
				if (!IsValidationSuspended)
				{
					ValidateProspectPeriodStartDate();
				}
			}
		}

		ZDate prospectPeriodStart;

		public ZPropertyInfo ProspectPeriodStartInfo => GetZPropertyInfo(nameof(ProspectPeriodStart));

		public bool ProspectPeriodStart_ReadOnly => TradeDetailStatus != OpportunityTradeStatus.Codes.Successful || ProspectPeriodEndType != OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter || TradeDetailStatusDescription_ReadOnly;

		#endregion

		#region ProspectPeriodEnd

		[ResourceStringData("Enterprise.MarketingManager.GUI.OpportunityTradeDetailItem|ProspectPeriodEnd", Caption = "End Period")]
		public ZDate ProspectPeriodEnd
		{
			get => prospectPeriodEnd;
			set
			{
				SetNonPersistentPropertyValue(ProspectPeriodEndInfo, ref prospectPeriodEnd, value);
				if (!IsValidationSuspended)
				{
					ValidateProspectPeriodEndDate();
				}
			}
		}

		ZDate prospectPeriodEnd;

		public ZPropertyInfo ProspectPeriodEndInfo => GetZPropertyInfo(nameof(ProspectPeriodEnd));

		public bool ProspectPeriodEnd_ReadOnly => TradeDetailStatus != OpportunityTradeStatus.Codes.Successful || ProspectPeriodEndType != OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter || TradeDetailStatusDescription_ReadOnly;

		#endregion

		#region ForecastType

		ZString ForecastType { get; set; }

		[ResourceStringData("Enterprise.MarketingManager.GUI.OpportunityTradeDetailItem|ForecastTypeDescription", Caption = "Rolling Forecast")]
		public ZString ForecastTypeDescription => ForecastTypeList.GetDescriptionFromCode(ForecastType);

		public ZPropertyInfo ForecastTypeDescriptionInfo => GetZPropertyInfo(nameof(ForecastTypeDescription));

		public CodeDescriptionPairList ForecastTypeList
		{
			get
			{
				return Factory.GetCachedValue("OpportunityTradeDetailSelectionItem.ForecastTypeList", () =>
				{
					return new OrgTradeProspectForecastTypeList();
				});
			}
		}

		#endregion

		#region IsSuperceded / IsExpired

		public ZBool IsSuperceded => TradeDetail.IsSuperceded;

		public ZBool IsExpired => TradeDetail.IsExpired;

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateTradeDetailStatus();
			ValidateExpectedTradeStartDate();
			ValidateProspectPeriodEndType();
			ValidateProspectPeriodStartDate();
			ValidateProspectPeriodEndDate();
		}

		void ValidateTradeDetailStatus()
		{
			TradeDetailStatusInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TradeDetailStatusInfo);
			ListValidation.ErrorIfInvalidCode(TradeDetailStatusInfo);
		}

		void ValidateExpectedTradeStartDate()
		{
			ExpectedTradeStartDateInfo.ClearAllNotifications();
			if (TradeDetailStatus == OpportunityTradeStatus.Codes.Successful)
			{
				MandatoryValidation.CheckEntered(ExpectedTradeStartDateInfo);
			}
		}

		void ValidateProspectPeriodEndType()
		{
			ProspectPeriodEndTypeInfo.ClearAllNotifications();
			if (TradeDetailStatus == OpportunityTradeStatus.Codes.Successful)
			{
				MandatoryValidation.CheckEntered(ProspectPeriodEndTypeInfo);
				ListValidation.ErrorIfInvalidCode(ProspectPeriodEndTypeInfo);
			}
		}

		void ValidateProspectPeriodStartDate()
		{
			ProspectPeriodStartInfo.ClearAllNotifications();
			if (ProspectPeriodEndType == OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter)
			{
				MandatoryValidation.CheckEntered(ProspectPeriodStartInfo);

				if (ProspectPeriodStart > ProspectPeriodEnd)
				{
					ProspectPeriodStartInfo.AddError(Res.GetString("47b08b0e-1664-40bc-b8e9-2f48714ce369", "The Start Period must be earlier than the End Period"));
				}
			}
		}

		void ValidateProspectPeriodEndDate()
		{
			ProspectPeriodEndInfo.ClearAllNotifications();
			if (ProspectPeriodEndType == OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter)
			{
				MandatoryValidation.CheckEntered(ProspectPeriodEndInfo);

				if (ProspectPeriodStart > ProspectPeriodEnd)
				{
					ProspectPeriodEndInfo.AddError(Res.GetString("f761ab83-b41f-425a-8218-81ee66f6e210", "The End Period must be later than the Start Period"));
				}
			}
		}

		#endregion

		#region Confirm Change

		public void ConfirmChange()
		{
			TradeDetail.PA_Status = TradeDetailStatus;
			TradeDetail.ProspectDetail.PAP_ExpectedTradeStartDate = ExpectedTradeStartDate;
			TradeDetail.ProspectPeriodEndType = ProspectPeriodEndType;
			TradeDetail.ProspectPeriodStart = ProspectPeriodStart;
			TradeDetail.ProspectPeriodEnd = ProspectPeriodEnd;
			TradeDetail.ProspectDetail.PAP_ForecastType = ForecastType;
		}

		#endregion

		#region Check For Superceding

		public ZString SupercedingWarningMessage
		{
			get
			{
				var superceder = new OrgTradeDetailValueSuperceder(TradeDetail, ProspectPeriodStart);
				return superceder.SupercedingWarningMessage;
			}
		}

		#endregion
	}
}
