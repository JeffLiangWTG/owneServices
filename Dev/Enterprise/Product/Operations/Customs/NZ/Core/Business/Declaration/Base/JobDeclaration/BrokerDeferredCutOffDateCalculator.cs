using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class BrokerDeferredCutOffDateCalculator
	{
		public BrokerDeferredCutOffDateCalculator(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public ZString GetWarningMessageIfWarningRequired()
		{
			ZString result = ZString.Empty;
			if (CutoffDateWarningIsEnabled && CloseToTheCutOffDate && DeclarationIsBeingPaidFromBrokersAccount && AmountPayableIsOverMinimumForWarning)
			{
				result = MessageToShowIfWarningIsRequired;
			}
			return result;
		}

		ZString MessageToShowIfWarningIsRequired
		{
			get
			{
				return "This entry is of a high value and the next cutoff date for your Broker Deferred account is close.\r\n" +
						"\r\n" +
						"Total Amount Payable: " + TotalAmountPayable + "\r\n" +
						"Next Cutoff Date: " + NextCutoffDate.ToShortDateString() + "\r\n" +
						"\r\n" +
						"Are you sure you still want to submit this entry?";
			}
		}

		bool DeclarationIsBeingPaidFromBrokersAccount
		{
			get { return declaration.CusEntryHeader.IsFeePaidByBroker(null, "", null); }
		}

		decimal TotalAmountPayable
		{
			get { return declaration.CusEntryHeader.TotalAmountPayable; }
		}

		bool AmountPayableIsOverMinimumForWarning
		{
			get { return TotalAmountPayable >= MinimumAmountPayableToWarnOn; }
		}

		bool CloseToTheCutOffDate
		{
			get { return ZDateTime.Today.AddDays(DaysToHaveWarningFor) > NextCutoffDate; }
		}

		ZDateTime NextCutoffDate
		{
			get
			{
				var today = ZDateTime.Today;

				var currentCutOffDate = new ZDateTime(today.Year, today.Month, 20);

				if (today > currentCutOffDate)
				{
					currentCutOffDate = currentCutOffDate.AddMonths(1);
				}

				return currentCutOffDate;
			}
		}

		#region Registry Accessors

		bool CutoffDateWarningIsEnabled
		{
			get { return NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateEnabled.Value; }
		}

		decimal MinimumAmountPayableToWarnOn
		{
			get { return NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateMinimumDutyWarning.Value; }
		}

		int DaysToHaveWarningFor
		{
			get { return NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateDaysBeforeWarning.Value; }
		}

		#endregion
	}
}
