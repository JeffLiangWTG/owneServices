using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MarketingManager.ServiceTask.GUI
{
	public class SalesTradeLanesSyncTaskConfig : AutoSalesTradeLanesSyncTaskConfig
	{
		readonly IServiceTaskSchedule parent;

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public SalesTradeLanesSyncTaskConfig(IServiceTaskSchedule parent)
			: base(((IBusiness)parent).Factory)
		{
			this.parent = parent;
			((BusinessObject)parent).RegisterEditableChildObject(this);
			Parse(parent.ConfigString);
		}

		public SalesTradeLanesSyncTaskConfig(string configString)
		{
			using (SuspendSettingHasChanges())
			{
				Parse(configString);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SyncMonthsOnMainSchedule = 3;
			SyncYearly = true;
			SyncYearlyEveryMonths = 3;
		}

		public bool SyncYearlyEveryMonths_ReadOnly
		{
			get { return !SyncYearly; }
		}

		public bool RunYearlyOnDayOfMonth_ReadOnly
		{
			get { return !SyncYearly; }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (parent != null)
			{
				parent.ConfigString = ConfigString;
			}
		}

		internal string ConfigString
		{
			get { return SyncMonthsOnMainSchedule + "," + (SyncYearly ? "Y" : "N") + "," + SyncYearlyEveryMonths + "," + RunYearlyOnDayOfMonth; }
		}

		void Parse(string configString)
		{
			if (!string.IsNullOrEmpty(configString))
			{
				string[] parts = configString.Split(',');

				if (parts.Length > 0)
				{
					int result;
					if (int.TryParse(parts[0], out result))
					{
						SyncMonthsOnMainSchedule = result;
					}
				}

				if (parts.Length > 1)
				{
					SyncYearly = parts[1] == "Y";
				}

				if (parts.Length > 2)
				{
					int result;
					if (int.TryParse(parts[2], out result))
					{
						SyncYearlyEveryMonths = result;
					}
				}

				if (parts.Length > 3)
				{
					int result;
					if (int.TryParse(parts[3], out result))
					{
						RunYearlyOnDayOfMonth = result;
					}
				}
			}
		}
	}
}
