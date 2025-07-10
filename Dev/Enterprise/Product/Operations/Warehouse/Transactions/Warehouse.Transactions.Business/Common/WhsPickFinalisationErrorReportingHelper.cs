using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickFinalisationErrorReportingHelper
	{
		public WhsPickFinalisationErrorReportingHelper(WhsPick pick)
		{
			Pick = Argument.NotNull(pick, nameof(pick));
		}

		readonly WhsPick Pick;

		public string ReportPickFinalisationErrorMessage()
		{
			var errorMessageBuilder = new ZStringBuilder();
			errorMessageBuilder.Append(Res.GetString("427d8b9f-d4d1-4ed1-a814-aab18f04f247", "Failed to Finalize Pick."));

			AppendPickUnfinalisableReason(errorMessageBuilder);
			foreach (WhsPickableDocket order in Pick.Orders)
			{
				AppendProductsWithErrors(errorMessageBuilder, order);
			}
			AppendPickValidationErrors(errorMessageBuilder, Pick);

			return errorMessageBuilder.ToStringWithNewLineBetweenAppends();
		}

		void AppendPickUnfinalisableReason(ZStringBuilder stringBuilder)
		{
			var finalisePickStatus = Pick.GetFinalisableStatus();
			if (!finalisePickStatus.IsFinalisable)
			{
				if (!finalisePickStatus.FinaliseCheckpoint.IsAllowed)
				{
					stringBuilder.Append(Res.GetString("43fb88a0-51bc-45f7-a218-50df1c0b04f5", "Login user is not allowed to finalize Warehouse Pick. Please check user's security rights settings for Warehouse Release Finalize"));
				}

				if (!finalisePickStatus.ErrorMessage.IsEmpty)
				{
					stringBuilder.Append(Res.GetString("82dd03e4-e705-41fd-8a41-435d8f9eae45", "Error Message: {0}", finalisePickStatus.ErrorMessage));
				}
			}
		}

		void AppendProductsWithErrors(ZStringBuilder stringBuilder, WhsPickableDocket order)
		{
			foreach (var orderLine in order.Lines.Where(l => l.HasErrors))
			{
				stringBuilder.Append(Res.GetString("53d7dc17-c48b-4645-ac13-182dc9d4b221", "Order Line Product: {0}", orderLine.ProductCode));
			}
		}

		void AppendPickValidationErrors(ZStringBuilder stringBuilder, WhsPick pick)
		{
			var uniqueErrorMessageList = pick.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList();
			foreach (var errorMessage in uniqueErrorMessageList)
			{
				stringBuilder.Append(errorMessage);
			}
		}
	}
}
