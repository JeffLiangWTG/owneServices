using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MarketingManager.Module
{
	public class TradeProfileForOrgPlugin : ZPlugIn
	{
		public TradeProfileForOrgPlugin(OrgHeader hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		#region Name

		public override string Name
		{
			get { return Res.GetString("67caaae7-dfdb-4a31-89d0-f0060c128860", "Value Analysis"); }
		}

		#endregion

		#region Form

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();
			IFileMenuItemsProvider menuItemsProvider = Form;
			menuItemsProvider.ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("2bcaa71e-64c1-4cb5-89f2-2ca9bb4315db", "Synchronize Value Analysis Data"), SynchronizeTradeProfileDataMenuItemClick));
		}

		protected virtual OrgHeader GetOrg()
		{
			return (OrgHeader)Form.BusinessEntity;
		}

		protected void SynchronizeTradeProfileDataMenuItemClick(object sender, EventArgs e)
		{
			if (!ObjectFactory.Get<IJCDServiceTaskStatusChecker>().IsJCDServiceTaskComplete)
			{
				Globals.Message.ShowError(
					Res.GetString("00e5bcca-5b84-4c0f-a48c-70f20523a2d7", "Before you can synchronize operational data, you must finish processing all Transaction Lines through the Job Costing Data Queue Service Task (https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20180319d.pdf)."),
					Res.GetString("87cf38c5-4ee6-43d4-a47c-a60537a1bc23", "Job Costing Data Queue Service Task Incomplete"));
				return;
			}

			var anotherFactory = new BusinessObjectFactory();
			var org = GetOrg();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);
			if (orgInOtherFactory == null || orgInOtherFactory.IsDeleted)
			{
				Globals.Message.ShowError(Res.GetString("bb0a0554-1d7d-4b98-a04d-8bdd6789987b", "This record has been deleted while you were editing it. This form will be closed."));
				Form.Close();
				return;
			}

			var dialogResult = Globals.Message.Show(
				Res.GetString("4d383eaa-19ad-4dcd-9e02-167c3c3b64c7", "Synchronizing Value Analysis Data with operational data may take a couple of minutes. Are you sure you want to continue?"),
				Res.GetString("01313a0a-d53c-4182-b65e-b6dcde58c4c5", "Synchronize Value Analysis Data"),
				MessageBoxButtons.YesNo,
				DialogResult.No);

			if (dialogResult == DialogResult.Yes)
			{
				try
				{
					using (new ZWaitCursorChanger())
					{
						Synchronise(orgInOtherFactory);
					}

					anotherFactory.Save();
					org.SalesCollection.Load();
				}
				catch (ZSaveConcurrencyException ex)
				{
					ZExceptionReporting.HandleSaveException(ex, Form);
				}
			}
		}
		protected virtual void Synchronise(OrgHeader org)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				SynchroniseCore(connection, org);
			}
		}

		protected void SynchroniseCore(DbConnection connection, OrgHeader org)
		{
			var now = ZDateTime.UtcNow;
			var currentPeriod = new ZDate(now.Year, now.Month, 1);
			var to = currentPeriod.AddMonths(1);

			using (var summary = new NonCachedTradeLinesSummaryProvider(connection))
			{
				var synchroniser = new TradeLinesSynchroniser(org.PK);
				synchroniser.Execute(summary, new ZDate(OrganisationRegistry.Instance.FullTradeLanesSyncFromDate), to);
			}
		}

		#endregion

		#region User Control

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			var control = new TradeProfileForOrgUserControl();
			control.Dock = DockStyle.Fill;
			return control;
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.SalesValueAnalysis; }
		}

		#endregion
	}
}
