using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module
{
	class ClearExpiredStockOperationalActionMethod : OperationalActionMethod
	{
		public ClearExpiredStockOperationalActionMethod() : base(new ZGuid("E64F0570-C66B-49D7-96A0-562CE71F347C"))
		{
		}

		public override string Name => "Clear Expired Stock";

		public override string Description => "Clear Expired Stock";

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ClearExpiredStockApplicator(factory, PromptForExpiryCutoffDateAndConfirm);
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl()
		{
			return new ClearExpiredStockOperationalActionUserControl();
		}

		ZDate PromptForExpiryCutoffDateAndConfirm()
		{
			var dateObject = new NonPersistentExpiryDateObject
			{
				Date = ZDate.Today.AddDays(-7 * 102)
			};

			using (var form = new ExpiryDateConfirmationForm(dateObject))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) != DialogResult.OK)
				{
					dateObject.Date = ZDateTime.Empty;
				}
			}

			if (dateObject.Date.IsValid)
			{
				var thresholdDate = ZDate.Today.AddMonths(-22);
				if (dateObject.Date <= thresholdDate)
				{
					var ageMonths = ZArchitecture.Core.Utilities.Round((decimal)((ZDate.Today - dateObject.Date.Date).TotalDays / 30.43), 0);
					if (Globals.Message.ShowConfirmation($"Date selected is {ageMonths} months prior to current date, do you want to continue?", "Clear Expired Stock", "yes", System.Windows.Forms.MessageBoxIcon.Warning) != System.Windows.Forms.DialogResult.OK)
					{
						dateObject.Date = ZDate.Empty;
					}
				}
				else
				{
					Globals.Message.Show("Goods that are less than 22 months old cannot be cleared via the Expiry option", "Clear Expired Stock", ZMessageBoxButtons.OK, ZMessageBoxIcon.Error, ZDialogResult.OK);
				}
			}

			return dateObject.Date.Date;
		}
	}
}
