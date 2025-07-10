using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Module
{
	[UniversalCopyInstanceType(InstanceType = typeof(QuotedBooking))]
	public class OneOffQuoteModule : QuotedBookingModuleBase
	{
		protected override bool SupportTemplateRecords => false;

		public override ModuleIdentifier ID => ModuleIDs.OneOffQuotes;

		protected override void AddNewStandardMenuItemsCore()
		{
			if (NewMenuItem != null)
			{
				var spotQuoteMenuItemName = ResString.GetMultilingualString("BookingNewButtonLabelList|SpotQuote", "One Off Quote");
				var oneOffQuoteMenuItem = new ZMenuItem(spotQuoteMenuItemName, HandleQuoteOnly);

				oneOffQuoteMenuItem.Name = spotQuoteMenuItemName;

				NewMenuItem.MenuItems.Add(oneOffQuoteMenuItem);
			}
		}

		protected override void AddFinalCopySubMenuItems()
		{
			// When this is called, there may already exist some sub-menu items.
			// so we need to add them in reverse order, and into the zero index
			// so they show up as the first two options.
			CopyMenuItem.MenuItems.Add(0, new ZMenuItem(ResString.GetMultilingualString("d62070e4-8f26-4e54-9d4f-0cd3e41b549d", "Copy as Amendment"), HandleCopyAsAmendmentClick));
			CopyMenuItem.MenuItems.Add(0, new ZMenuItem(ResString.GetMultilingualString("6583490a-f227-4cf5-9513-4549012f6143", "Copy as New One Off Quote"), HandleTemplateCopyClick));
		}

		protected override IFilterControl GetNewFilterControl() => new QuotedBookingFilterControl(GridCollection, FilterBusinessObject, "OneOffQuote", true);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new OneOffQuoteFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.OneOffQuotes);

		protected override ZController GetNewController(QuotedBookingState state) => new OneOffQuoteController();

		protected override IBusinessObjectCollection GetNewGridCollectionCore() => new ViewOneOffQuoteCollection(Factory);

		#region Events

		void HandleQuoteOnly(object sender, EventArgs e)
		{
			new OneOffQuoteController().ShowNewForm();
		}

		void HandleCopyAsAmendmentClick(object sender, EventArgs e)
		{
			if (CurrentBusinessObjectInGrid == null)
			{
				ShowNoSelectedMessage();
				return;
			}

			if (CurrentBusinessObjectInGrid is ViewQuotedBooking viewQuotedBooking)
			{
				if (viewQuotedBooking.QuotedBooking.CanCopyAsOneOffQuoteAmendment)
				{
					viewQuotedBooking.QuotedBooking.Quote.AmendmentCopy = ZBool.True;
					try
					{
						ShowTemplateCopyForm(viewQuotedBooking);
					}
					finally
					{
						viewQuotedBooking.QuotedBooking.Quote.AmendmentCopy = ZBool.False;
					}
				}
				else
				{
					var message = Res.GetString("8eacf220-af97-41ed-aff6-89e46eeec3b7", "The quote cannot be copied as an amendment since it has already been used.");
					var caption = Res.GetString("00083981-40f5-4e02-8f87-c0e728f26a87", "Action cannot be completed");
					Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, DialogResult.OK);
				}
			}
			else
			{
				ShowIncorrectTypeErrorMessage();
			}
		}

		#endregion

		#region Security / Licence

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.OneOffQuote;

		#endregion
	}
}
