using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business
{
	public class LCDistributionStatusChecker
	{
		public static string ErrorMessage
		{
			get { return Res.GetString("1be8032e-f962-4202-a046-41028d3ea1dc", "There are errors. Please fix the errors first."); }
		}
		public static string LandedCostingNoChargeRowWarningMessage
		{
			get { return Res.GetString("274f0549-6423-434f-baa5-7afe96151065", "You have not entered any Transport and Logistics costs."); }
		}

		public static string DistributionFieldErrorMessage
		{
			get { return Res.GetString("ead84736-3e83-4bc8-90cb-8c3ae9c559ae", "The Transport and Logistics costs cannot be distributed by the selected method, as none of the lines have the appropriate values entered. Please distribute costs by another method, or enter the weight/volume/quantity against the lines."); }
		}
		public static string DistributionFieldWarningMessage
		{
			get { return Res.GetString("876a8c62-6e35-4776-b7d1-62862e8e05db", "Some of lines do not have weight/volume/quantity Transport and Logistics cost can be distributed by."); }
		}

		public static string ExistingLandedCostJob
		{
			get { return Res.GetString("f19bb6cc-0fde-42c0-85d5-69a07c01ff48", "You have selected Run Landed Costing. Previous Runs for LC have been made. You will lose this data by running Landed Costing again"); }
		}

		public LCDistributionStatusChecker(LandedCostHeader lCHeader)
		{
			this.LCHeader = lCHeader;
		}

		public StatusCheckResult GetStatus()
		{
			StatusCheckResult result = new StatusCheckResult();

			string preconditionError = CheckPreConditionsForLandedCostDistribution();
			if (!string.IsNullOrEmpty(preconditionError))
			{
				result.Message = preconditionError;
				result.NotificationType = NotificationTypes.Error;
			}
			else
			{
				string warning = CheckWarningsForLandedCostDistribution();
				if (!string.IsNullOrEmpty(warning))
				{
					result.Message = warning;
					result.NotificationType = NotificationTypes.Warning;
				}
			}

			return result;
		}

		string CheckPreConditionsForLandedCostDistribution()
		{
			string result = "";

			CalculateMissingDistributionByFieldError();

			if (!string.IsNullOrEmpty(MissingDistributionByFieldError))
			{
				result = MissingDistributionByFieldError;
			}
			else
			{
				LCHeader.ExchangeRates.LoadFromLCHeaderHost();
				LCHeader.RunPreSaveValidation();

				if (LCHeader.HasErrors)
				{
					result = ErrorMessage + "\r\n" + LCHeader.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString();
				}
			}
			return result;
		}

		string CheckWarningsForLandedCostDistribution()
		{
			StringBuilder result = new StringBuilder();

			if (LCHeader.CostInputs.Count == 0)
			{
				result.Append(LandedCostingNoChargeRowWarningMessage);
			}
			if (!string.IsNullOrEmpty(MissingDistributionByFieldWarning))
			{
				result.Append(MissingDistributionByFieldWarning);
			}
			if (LCHeader.Histories.Count > 0)
			{
				result.Append("\r\n" + ExistingLandedCostJob);
			}
			return result.ToString();
		}

		#region Implementation

		void CalculateMissingDistributionByFieldError()
		{
			MissingDistributionByFieldError = "";
			MissingDistributionByFieldWarning = "";

			bool actualExist = false, weightExist = false, volumeExist = false, itemCountExist = false;

			foreach (LandCostInput lCInput in LCHeader.CostInputs)
			{
				actualExist |= lCInput.LI_DistributeCostBy == CostDistributionMechanismList.Codes.Actual;
				weightExist |= lCInput.LI_DistributeCostBy == CostDistributionMechanismList.Codes.ActualWeight;
				volumeExist |= lCInput.LI_DistributeCostBy == CostDistributionMechanismList.Codes.ActualVolume;
				itemCountExist |= lCInput.LI_DistributeCostBy == CostDistributionMechanismList.Codes.Item;
			}

			if (actualExist || weightExist || volumeExist || itemCountExist)
			{
				bool hasDistributeeWithActual = false, hasDistributeeWithWeight = false, hasDistributeeWithVolume = false, hasDistributeeWithItemCount = false;
				bool hasDistributeeWithoutActual = false, hasDistributeeWithoutWeight = false, hasDistributeeWithoutVolume = false, hasDistributeeWithoutItemCount = false;

				foreach (IUltimateDistributee distributee in LCHeader.Parent.UltimateDistributees)
				{
					hasDistributeeWithActual |= distributee.Actual > 0;
					hasDistributeeWithoutActual |= distributee.Actual == 0;

					hasDistributeeWithWeight |= distributee.Weight > 0;
					hasDistributeeWithoutWeight |= distributee.Weight == 0;

					hasDistributeeWithVolume |= distributee.Volume > 0;
					hasDistributeeWithoutVolume |= distributee.Volume == 0;

					hasDistributeeWithItemCount |= distributee.ItemCount > 0;
					hasDistributeeWithoutItemCount |= distributee.ItemCount == 0;
				}

				if (actualExist && !hasDistributeeWithActual || weightExist && !hasDistributeeWithWeight ||
					volumeExist && !hasDistributeeWithVolume || itemCountExist && !hasDistributeeWithItemCount)
				{
					MissingDistributionByFieldError = DistributionFieldErrorMessage;
				}
				else
				{
					if (actualExist && hasDistributeeWithoutActual || weightExist && hasDistributeeWithoutWeight ||
						volumeExist && hasDistributeeWithoutVolume || itemCountExist && hasDistributeeWithoutItemCount)
					{
						MissingDistributionByFieldWarning = DistributionFieldWarningMessage;
					}
				}
			}
		}

		string MissingDistributionByFieldWarning;
		string MissingDistributionByFieldError;

		readonly LandedCostHeader LCHeader;

		#endregion

	}

	public struct StatusCheckResult
	{
		public bool IsError
		{
			get { return notificationType == NotificationTypes.Error; }
		}

		public bool IsWarning
		{
			get { return notificationType == NotificationTypes.Warning; }
		}

		public string Message
		{
			get { return message; }
			internal set { message = value; }
		}
		string message;

		public NotificationTypes NotificationType
		{
			get { return notificationType; }
			internal set { notificationType = value; }
		}
		NotificationTypes notificationType;
	}
}
