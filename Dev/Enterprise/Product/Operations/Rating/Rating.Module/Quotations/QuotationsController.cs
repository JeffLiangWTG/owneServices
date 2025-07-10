using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static System.FormattableString;
using static Enterprise.Rating.Business.Quote;

namespace Enterprise.Rating.Module
{
	public class QuotationsController : RatingController<Quote>, IQuotationsController
	{
		#region Standard Controller Overrides

		public override ControllerID ID => ControllerIDs.Quotations;

		public override ModuleIdentifier ModuleID => ModuleIDs.Quotations;

		protected override IZForm GetForm(IBusiness businessEntity) =>
			new QuotationForm((Quote)businessEntity);

		protected virtual IClientRateForm GetClientRatesForm(ClientRate rate) =>
			new ActiveRatesForm(rate, false);

		#endregion

		#region Accept / Cancel Forms

		public void ShowCancelForm(IBusiness businessEntity)
		{
			if (CheckPointForCancel.IsAllowed)
			{
				Quote quoteToCancel = (Quote)Factory.Load(TypeOfTopLevelBusinessObject, businessEntity.Identifier);
				if (quoteToCancel == null)
				{
					LastShownForm = null;
					ShowAlreadyDeletedOrIrreversiblyChangedMessage();
					return;
				}
				IQuotationForm cancelForm = (IQuotationForm)GetForm(quoteToCancel);
				ShowForm(cancelForm);
				cancelForm.SetupFormForQuoteCancellation();
			}
			else
			{
				CheckPointForCancel.ShowError();
			}
		}

		public IZForm ShowAcceptForm(IBusiness businessEntity)
		{
			if (CheckPointForAccept.IsAllowed)
			{
				Quote quoteToAccept = (Quote)Factory.Load(TypeOfTopLevelBusinessObject, businessEntity.Identifier);
				if (quoteToAccept == null)
				{
					LastShownForm = null;
					ShowAlreadyDeletedOrIrreversiblyChangedMessage();
					return null;
				}

				if (quoteToAccept.HasOverlappingClientRate())
				{
					var message = Res.GetString("1DFAB30B-CBF3-4A75-BE5C-91AE1E202897", "During this operation, overlapping rates were found in the Client Rates of '{0}'.\r\n\r\nThe overlapping rates will be expired one day before the Start Date of the Quotation.\r\n\r\nDo you wish to continue?", quoteToAccept.Header.OH_Code);
					var caption = Res.GetString("676967DD-9E0F-4ED1-9441-8F008872200D", "Accepting Quote");
					if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes)
					{
						return null;
					}
				}

				ClientRate clientRate;
				using (RateEntryCreator.CreateFromQuotation().Push())
				{
					if (!quoteToAccept.TryAcceptQuote(out clientRate, reload: false))
					{
						Globals.Message.ShowError(Res.GetString("0b48fe6f-f84f-455d-8e40-5e1724010a31",
							"The Quotation has to be Approved before Accepting."));
						return null;
					}

					IClientRateForm acceptForm = GetClientRatesForm(clientRate);

					ShowForm(acceptForm);
					acceptForm.SetupFormForQuoteAcceptance(quoteToAccept.TH_QuoteNumber);

					return acceptForm;
				}
			}
			else
			{
				CheckPointForAccept.ShowError();
			}

			return null;
		}

		#endregion

		#region Showing Forms

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			if (!RunAdditionalShowFormChecks(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowViewForm(sourceEntity);
		}

		public override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			if (!RunAdditionalShowFormChecks(selectedBusinessObject))
			{
				LastShownForm = null;
				return null;
			}

			if (selectedBusinessObject is RateEntry)
			{
				return base.ShowEditForm(selectedBusinessObject);
			}

			var quote = (Quote)selectedBusinessObject;

			string caption = Res.GetString("2294e5d8-3d53-4b69-8364-dd7932b58fff", "Can't Edit");
			string message;

			if (quote.QuoteStatus == QuoteStatusOptions.Active || quote.QuoteStatus == QuoteStatusOptions.Approved)
			{
				return base.ShowEditForm(selectedBusinessObject);
			}

			if (quote.QuoteStatus == QuoteStatusOptions.ClientAccepted)
			{
				var unlockedAmendedQuote = quote.GetMostRecentAmendment(false);
				var msgExistingAmmendment = Res.GetString("06d0b45a-b2e8-4a82-96cb-98e925484177", "The selected quote has already been accepted by the client and cannot be edited.\r\nHowever, an un-printed Amended quote exists (Quote Number: {0}).\r\n\r\nWould you like to edit this one instead?", unlockedAmendedQuote?.TH_QuoteNumber ?? string.Empty);
				var msgNewAmmendment = Res.GetString("26c4d3e6-b664-4b51-8350-c68ef1bce2b0", "The selected quote has already been accepted by the client and cannot be edited.  Would you like to create an Amendment / copy instead?");

				var form = ShowAmendment(quote, unlockedAmendedQuote, caption, msgExistingAmmendment, msgNewAmmendment);
				if (form != null)
				{
					return form;
				}
			}

			if (quote.QuoteStatus == QuoteStatusOptions.Finalized)
			{
				var unlockedAmendedQuote = quote.GetMostRecentAmendment(false);
				var msgExistingAmmendment = Res.GetString("74fd1ee0-09b9-44b2-b842-4928f2926ef9", "The selected quote has already been printed in Final mode and cannot be edited.\r\nHowever, an un-printed Amended quote exists (Quote Number: {0}).\r\n\r\nWould you like to edit this one instead?", unlockedAmendedQuote?.TH_QuoteNumber ?? string.Empty);
				var msgNewAmmendment = Res.GetString("3e8e932e-dff8-4e5b-9841-4c340e508433", "The selected quote has already been printed in Final mode and cannot be edited.  Would you like to create an Amendment / copy instead?");

				var form = ShowAmendment(quote, unlockedAmendedQuote, caption, msgExistingAmmendment, msgNewAmmendment);
				if (form != null)
				{
					return form;
				}
			}

			if (quote.QuoteStatus == QuoteStatusOptions.Accepted)
			{
				message = Res.GetString("754ccc24-3a6d-4aa7-87c0-d6ddad0fc892", "The selected quote has already been accepted and cannot be edited.  Would you like to view it instead?");
				if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
				{
					return base.ShowViewForm(selectedBusinessObject);
				}
			}

			if (quote.QuoteStatus == QuoteStatusOptions.Cancelled)
			{
				message = Res.GetString("2ef8b47b-ee40-4c03-89a0-ab2554f6efdf", "The selected quote has already been canceled and cannot be edited.  Would you like to view it instead?");
				if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
				{
					return base.ShowViewForm(selectedBusinessObject);
				}
			}

			if (quote.QuoteStatus == QuoteStatusOptions.Expired)
			{
				message = Res.GetString("214f60b8-5f70-407e-9303-27d61149b0a9", "The selected quote has already expired and cannot be edited.  Would you like to view it instead?");
				if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
				{
					return base.ShowViewForm(selectedBusinessObject);
				}
			}
			return null;
		}

		IZForm ShowAmendment(Quote currentQuote, Quote existingAmendment, string caption, string messageForPreviousAmmendmentFound, string messageForCreateNewAmmendment)
		{
			// If there are other Amendments for this quote that are unlocked, open them for editing instead.
			if (existingAmendment != null)
			{
				if (Globals.Message.Show(messageForPreviousAmmendmentFound, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
				{
					return base.ShowEditForm(existingAmendment);
				}
			}
			else // if there aren't any unlocked Amendments, then create an Amendment
			{
				if (Globals.Message.Show(messageForCreateNewAmmendment, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
				{
					return ShowCopyFormSameClient(currentQuote, true);
				}
			}

			return null;
		}

		IZForm ShowCopyFormSameClient(Quote selectedQuote, bool isAmendment)
		{
			if (selectedQuote != null)
			{
				selectedQuote.SameClientCopy = true;
				selectedQuote.AmendmentCopy = isAmendment;

				try
				{
					return ShowTemplateCopyForm(selectedQuote);
				}
				finally
				{
					selectedQuote.AmendmentCopy = false;
					selectedQuote.SameClientCopy = false;
				}
			}

			return null;
		}

		protected override IZForm ShowCopyForm(BusinessObject inMemorySourceEntity, CopyOfBusinessObject returnsNewBusinessEntity)
		{
			if (!RunAdditionalShowFormChecks(inMemorySourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowCopyForm(inMemorySourceEntity, returnsNewBusinessEntity);
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (!RunAdditionalShowFormChecks(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowDeleteForm(sourceEntity);
		}

		#region Additional Show Form Checks

		static bool RunAdditionalShowFormChecks(BusinessObject sourceEntity)
		{
			var quote = sourceEntity as Quote;
			if (quote != null)
			{
				if (!CheckLoginCompanyMatches(quote))
				{
					return false;
				}

				if (!Env.Security.QuotationShowAllQuotes.IsAllowed)
				{
					if (!Env.Security.QuotationShowBranchQuotes.IsAllowed)
					{
						return CheckLoginUserMatches(quote);
					}
					else
					{
						return CheckLoginBranchMatches(quote);
					}
				}
			}

			return true;
		}

		static bool CheckLoginCompanyMatches(Quote quote)
		{
			var company = quote.Company;
			if (company != null && company.PK != Env.CurrentCompany.PK)
			{
				var countryDescription = company.Country != null ? company.Country.Description : ZString.Empty;
				var message = ResString.GetMultilingualString("a1c6f2cf-014a-4602-8470-459ea75e30b6", @"The Quotation is for login users in {0} ({1}).
Please login to the relevant company to view the quotation.", countryDescription, company.GC_Code);
				var caption = ResString.GetMultilingualString("6a7b535b-0a7a-4c3c-8184-e3a792c7bd56", "Access Denied: Incorrect login company");
				Globals.Message.ShowError(message, caption);

				return false;
			}

			return true;
		}

		static bool CheckLoginUserMatches(Quote quote)
		{
			var signatories = new[] { quote.FirstSignatory, quote.SecondSignatory }.Where(x => x != null);
			if (signatories.Any(x => x.GS_Code == GlbStaff.CurrentUser.GS_Code))
			{
				return true;
			}

			var branchMatches = signatories.Any(x => x.GS_GB_HomeBranch == GlbStaff.CurrentUser.GS_GB_HomeBranch);
			var requiredSecurityCheckpoint = branchMatches ? Env.Security.QuotationShowBranchQuotes : Env.Security.QuotationShowAllQuotes;
			Globals.Message.ShowError(
					ResString.GetMultilingualString("154bce8e-fb54-4ced-b91a-21ab15c6107f", @"You do not have the appropriate security rights to view {0}. You are only allowed to view quotations where you are one of its signatories.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{1}",
						quote.HumanReadableName,
						requiredSecurityCheckpoint.DisplayTextPathToSecurityRight,
						ResString.GetMultilingualString("ab428af6-a3f0-4215-a879-4948153da510", "Access Denied: {0}", requiredSecurityCheckpoint.DisplayText)),
					requiredSecurityCheckpoint.DisplayText
				);

			return false;
		}

		static bool CheckLoginBranchMatches(Quote quote)
		{
			var signatories = new[] { quote.FirstSignatory, quote.SecondSignatory }.Where(x => x != null);
			var branchMatches = signatories.Any(x => x.GS_GB_HomeBranch == GlbStaff.CurrentUser.GS_GB_HomeBranch);
			if (branchMatches)
			{
				return true;
			}

			var branches = signatories.Select(x => x.HomeBranch).Where(x => x != null).Distinct().ToArray();
			var branchDescription =
				branches.Length == 0 ? ResString.GetMultilingualString("33aabb7c-4123-43ab-b113-6eb4593a868a", "relevant branch") :
				branches.Length == 1 ? ResString.GetMultilingualString("665ead81-afe8-46e2-84a1-51f039bd9fda", "relevant branch ({0})", GetFormattedBranch(branches[0])) :
				ResString.GetMultilingualString("52feb374-22a4-4a45-a391-eea0f2beb4ed", "relevant branch ({0} or {1})", GetFormattedBranch(branches[0]), GetFormattedBranch(branches[1]));

			Globals.Message.ShowError(
					ResString.GetMultilingualString("67c1ee39-ddef-429f-89af-5fa6b735ba19", @"You do not have the appropriate security rights to view {0}. You are only allowed to view quotations for your login branch.

If you require access to this function please login to the {1}, or ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{2}",
						quote.HumanReadableName,
						branchDescription,
						Env.Security.QuotationShowAllQuotes.DisplayTextPathToSecurityRight),
					Env.Security.QuotationShowAllQuotes.DisplayText
				);

			return false;
		}

		static string GetFormattedBranch(GlbBranch branch)
		{
			return Invariant($"{branch.GB_BranchName} ({branch.GB_Code})");
		}

		#endregion

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.QuotationView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.QuotationNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.QuotationEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.QuotationDelete;

		protected virtual SecurityCheckpoint CheckPointForCancel => Env.Security.QuotationCancel;

		protected virtual SecurityCheckpoint CheckPointForAccept => Env.Security.QuotationAccept;

		protected override SecurityCheckpoint GetCheckPointForCopy(BusinessObject inMemorySourceEntity)
		{
			var baseResult = base.GetCheckPointForCopy(inMemorySourceEntity);
			return baseResult != null && !baseResult.IsAllowed ? baseResult : Env.Security.QuotationCopy;
		}

		bool IQuotationsController.CheckIsCopyAllowed(BusinessObject inMemorySourceEntity)
		{
			var checkPoint = GetCheckPointForCopy(inMemorySourceEntity);
			if (checkPoint.IsAllowed)
			{
				return true;
			}
			else
			{
				checkPoint.ShowError();
				return false;
			}
		}

		protected override CRMSecurityProvider<Quote> SecurityProvider =>
			securityProvider ?? (securityProvider = new QuotationsCRMSecurityProvider());
		QuotationsCRMSecurityProvider securityProvider;

		#endregion
	}
}

