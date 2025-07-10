using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class TradeDetailCommitmentUpdaterGUIManager : ITradeDetailCommitmentUpdaterGUIManager
	{
		public ZDialogResult ShowForm(IOrgOpportunity opportunity, EventArgs e)
		{
			var convertor = new TradeDetailCommitmentUpdater(opportunity);
			var result = DialogResult.OK;

			if (convertor.TradeDetailItems.Count > 0)
			{
				result = ZFormModaliser.ShowDialogAndDispose(new TradeDetailCommitmentUpdaterForm(convertor));
			}

			var valueChangedEventArgs = e as ValueChangedEventArgs;

			if (result != DialogResult.OK && valueChangedEventArgs != null)
			{
				var previousStatus = (ZString)valueChangedEventArgs.OldValue;
				var orgOpportunity = ((OrgOpportunity)opportunity);
				orgOpportunity.P8_Status = previousStatus;
			}

			return (ZDialogResult)result;
		}

		public static DialogResult ShowSupercedingConfirmation(string itemsWarningMessage)
		{
			var warningMessage = Res.GetString("8fd7291a-f73c-4b2c-83ce-d095ac25f44b",
@"The following Committed Values will be superseded by the new successful estimates entered under this Opportunity. Old Superseded estimate values on the related Opportunity will be locked from further editing.

{0}
Do you wish to continue?", itemsWarningMessage);

			return Globals.Message.Show(warningMessage, Res.GetString("d20da75b-dce2-4e0c-93b2-96f6fbc7d088", "Supersede Estimated Values"), MessageBoxButtons.YesNo, DialogResult.No);
		}
	}
}
