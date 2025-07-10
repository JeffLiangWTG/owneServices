using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	[TestedType(typeof(OneOffQuoteModule))]
	public class OneOffQuoteModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OneOffQuotes;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestUniversalCopyInstanceTypeAttribute()
		{
			var instanceTypeAttribute = typeof(OneOffQuoteModule).GetCustomAttribute<UniversalCopyInstanceTypeAttribute>();
			CombineAssertions("UniversalCopyInstanceTypeAttribute", () =>
			{
				AssertEquals("InstanceType", typeof(QuotedBooking), instanceTypeAttribute.InstanceType);
				AssertNullOrEmpty("CreationMethod", instanceTypeAttribute.CreationMethod);
				AssertNullOrEmpty("GetSourceMethod", instanceTypeAttribute.GetSourceMethod);
				AssertEquals("ShouldSyncTreeNodes", false, instanceTypeAttribute.ShouldSyncTreeNodes);
			});
		}

		public void TestMergeIntoSelectedBookingActionMenuNotIncludedInOneOffQuotes()
		{
			using (var form = new ZForm())
			using (var module = new OneOffQuoteModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var mergeMenu = module.FormActionMenu.FindByText("Merge Into Selected Booking", true);
				AssertNull(mergeMenu);
			}
		}
		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			Factory.Save();
			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = quote.PK;
			collection.Add(viewQuotedBooking);
			base.AddTestObjects(collection);
		}

		public void TestCheckpoints()
		{
			using (OneOffQuoteModule module = new OneOffQuoteModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.OneOffQuote, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Booking, module.LicenceCheckPoint);
			}
		}

		public void TestOneOffQuoteModule_ColorContextKey()
		{
			using (var module = new OneOffQuoteModuleForTest())
			{
				var filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is QuotedBookingFilterControl);
				AssertEquals("OneOffQuote", (filterControl as QuotedBookingFilterControl).Grid.ColorContextKey);
				filterControl.Dispose();
			}
		}

		#region Copy

		public void TestCopyAsNewOneOffQuote_FinalPrintedQuote() => TestCopy(CreateOneOffQuoteFinalPrint(),"Copy as New One Off Quote",  AssertNewQuoteOpened);

		public void TestCopyAsNewOneOffQuote_BookingWithQuote() => TestCopy(CreateOneOffBookingWithQuote(),  "Copy as New One Off Quote", AssertNewQuoteOpened);

		public void TestCopyAsAmendment_FinalPrintedQuote() => TestCopy(CreateOneOffQuoteFinalPrint(), "Copy as Amendment", AssertNewQuoteOpened);

		public void TestCopyAsAmendment_BookingWithQuote() => TestCopy(CreateOneOffBookingWithQuote(), "Copy as Amendment", AssertQuoteCannotBeAmended);

		void TestCopy(QuotedBooking oneOffQuote, string copyMenuItemName, Action assert)
		{
			Factory.Save();

			using (var form = new ZForm())
			using (var module = new OneOffQuoteModuleForTest())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				Application.DoEvents();

				module.PerformSearch_ForTest();

				var grid = ((IFilterGridModuleInternalsForTesting)module).Grid;
				grid.SelectAllElements();

				var copyMenuItem = module.FormActionMenu
					.FindByText(copyMenuItemName, true);
				AssertNotNull(copyMenuItem);

				copyMenuItem.PerformClick();

				assert.Invoke();
			}

			AssertEquals(ZBool.False, oneOffQuote.OneOffQuoteIsAmended);
		}

		QuotedBooking CreateOneOffBookingWithQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			return quotedBooking;
		}

		QuotedBooking CreateOneOffQuoteFinalPrint()
		{
			var quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			quotedBooking.Quote.TH_IsLocked = true;
			return quotedBooking;
		}

		static void AssertNewQuoteOpened()
		{
			using (var quotedBookingForm = Application.OpenForms.OfType<QuotedBookingForm>().SingleOrDefault())
			{
				AssertNotNull("Should show the QuotedBookingForm", quotedBookingForm);
			}
		}

		static void AssertQuoteCannotBeAmended()
			=> AssertEquals("The quote cannot be copied as an amendment since it has already been used.", UnitTestUserNotification.Instance.LastMessage.Text);

		#endregion

		internal class OneOffQuoteModuleForTest : OneOffQuoteModule
		{
			public IFilterControl NewFilterControl
			{
				get { return GetNewFilterControl(); }
			}
		}
	}
}
