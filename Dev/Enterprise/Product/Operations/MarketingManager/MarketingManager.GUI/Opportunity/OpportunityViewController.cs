using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	/// <summary>
	/// Implementation of IOpportunityViewControllerProvider, which is defined in MasterFiles.GUI assembly and configured via Spring.
	/// Allows OpportunityForm, from MasterFiles.GUI assembly, to call into MarketingManager.GUI assembly
	/// to handle events like the opportunity organization changing.
	/// </summary>
	public class OpportunityViewControllerProvider : IOpportunityViewControllerProvider
	{
		public IOpportunityViewController CreateController(OpportunityForm opportunityView)
		{
			return new OpportunityViewController(opportunityView);
		}
	}

	public class OpportunityViewController : IOpportunityViewController
	{
		public OpportunityViewController(OpportunityForm opportunityView)
		{
			view = opportunityView;
			opportunityView.ValidatingForSave += OpportunityView_ValidatingForSave;
			opportunityView.Saved += OpportunityView_Saved;
		}

		readonly OpportunityForm view;
		ZGuid originalOrgPk;
		IDisposable hasChangesSuspender;

		void OpportunityView_ValidatingForSave(object sender, ValidatingForSaveEventArgs e)
		{
			originalOrgPk = ZGuid.Empty;
			var hasOrgChanged = false;

			var opp = view.BusinessEntity;
			if (opp == null)
			{
				return;
			}

			if (opp.IsInDatabase)
			{
				originalOrgPk = (ZGuid)opp.P8_OHInfo.OriginalValue;
				hasOrgChanged = opp.P8_OHInfo.HasChanges;
			}
			else
			{
				originalOrgPk = opp.SourceOpportunityOrgPK;
				hasOrgChanged = !opp.SourceOpportunityOrgPK.IsEmpty && opp.P8_OH != opp.SourceOpportunityOrgPK;
			}

			if (!hasOrgChanged || (opp.ProspectiveSalesHeaderCollection.Count == 0 && !opp.HasUnsavedCopiedOrgTradePeriods))
			{
				return;
			}

			var result = Globals.Message.Show(Res.GetString("6367BC46-87AD-4F81-BCFC-E09158BE4D0A"
				, "This opportunity has estimated values that will need to be migrated to the new Organization. Any values that are shared with other records will be copied and the shared association removed. Are you sure you wish to change the Organization and begin the migration?")
				, view.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, DialogResult.OK);

			if (result == DialogResult.Cancel)
			{
				e.ContinueWithSave = ContinueWithSave.No;
			}
			else
			{
				opp.DuplicateSharedProspectiveSales();
			}
		}

		void OpportunityView_Saved(object sender, EventArgs e)
		{
			var opp = view.BusinessEntity;
			if (opp == null
				|| originalOrgPk.IsEmpty
				|| originalOrgPk == opp.P8_OH
				|| opp.ProspectiveSalesHeaderCollection.Count == 0)
			{
				return;
			}

			var newFactory = new BusinessObjectFactory();
			var oppInNewFactory = newFactory.Load<OrgOpportunity>(opp.PK);
			if (HasMatchingSales(oppInNewFactory))
			{
				ShowMessageForMatch();
				var form = new OpportunityProspectiveTradeProfileForm(oppInNewFactory);
				form.Shown += ProspectiveTradeProfilesForm_Shown;
				form.FormClosed += ProspectiveTradeProfilesForm_FormClosed;
				hasChangesSuspender = opp.SuspendSettingHasChangesIncludingChildren();
				ZFormModaliser.Show(form, view);
			}
		}

		static IEnumerable<SalesMatching> GetMatchings(OrgOpportunity opp)
		{
			foreach (var salesHeader in ((SalesHeaderCollection)opp.ProspectiveSalesHeaderCollection).Cast<SalesHeader>().ToArray())
			{
				var product = salesHeader.SalesProduct;
				foreach (var sale in salesHeader.EntitySales.ToArray())
				{
					if (product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgTradeDetail))
					{
						foreach (var tradeDetailWrapper in sale.EntityTradeDetails.ToArray())
						{
							var matching = new SalesMatching(opp.Header, null, tradeDetailWrapper, product.SalesMatchingOptions);
							if (matching.MatchedSalesCollection.Count > 0)
							{
								yield return matching;
							}
						}
					}
					else
					{
						var matching = new SalesMatching(opp.Header, sale, null, product.SalesMatchingOptions);
						if (matching.MatchedSalesCollection.Count > 0)
						{
							yield return matching;
						}
					}
				}
			}
		}

		bool HasMatchingSales(OrgOpportunity opp)
		{
			return GetMatchings(opp).Any();
		}

		void ProspectiveTradeProfilesForm_Shown(object sender, EventArgs e)
		{
			var form = (OpportunityProspectiveTradeProfileForm)sender;
			var opp = (OrgOpportunity)form.BusinessEntity;

			foreach (var matching in GetMatchings(opp))
			{
				Process(matching);
			}
		}

		void ProspectiveTradeProfilesForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			((ZForm)sender).FormClosed -= ProspectiveTradeProfilesForm_FormClosed;

			var opp = view.BusinessEntity;
			var headers = (SalesHeaderCollection)opp.ProspectiveSalesHeaderCollection;
			headers.Rebuild();

			var plugin = view.DetailsControl.TradeProfilePlugin;
			var tradeControl = plugin.UserControl as TradeProfileForRelatedUserControl;
			if (tradeControl != null && tradeControl.SalesValueAnalysisCollection != null)
			{
				tradeControl.SetDataBinding(opp, "");
			}

			if (hasChangesSuspender != null)
			{
				var tmp = hasChangesSuspender;
				hasChangesSuspender = null;
				tmp.Dispose();
				opp.HasChanges = false;
			}
		}

		void Process(SalesMatching matching)
		{
			var response = PromptUser(matching);
			if (response.UserResult == SalesMigrationForm.MigrationResult.Append)
			{
				matching.Append();
			}
			else if (response.UserResult == SalesMigrationForm.MigrationResult.Overwrite)
			{
				matching.MigrateToExisting(response.SelectedSalesMatchingData);
			}
			else if (response.UserResult == SalesMigrationForm.MigrationResult.UseMatchingOnly)
			{
				matching.UseExisting(response.SelectedSalesMatchingData);
			}
		}

		public class SalesMigrationResponse
		{
			public SalesMigrationForm.MigrationResult UserResult;
			public SalesMatchingData SelectedSalesMatchingData;
		}

		protected virtual SalesMigrationResponse PromptUser(SalesMatching matching)
		{
			using (var form = new SalesMigrationForm(matching))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				return new SalesMigrationResponse()
				{
					UserResult = form.UserResult,
					SelectedSalesMatchingData = form.SelectedSalesMatchingData
				};
			}
		}

		void ShowMessageForMatch()
		{
			var msg = Res.GetString("0D7EAC40-C6F4-4522-8F7E-5E6DC2E07BCC",
				"Some value analysis associated to this Opportunity matches with those existing on the new Organization and will need to be migrated. Add will create a duplicate entry...");
			Globals.Message.Show(msg);
		}
	}
}
