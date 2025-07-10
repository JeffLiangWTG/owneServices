using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Netting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.GUI.Accounting.Netting;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbCompanyModule : ZFilterGridModule
	{
		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbCompany; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbCompany);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbCompanyFilterControl(GridCollection, (GlbCompanyFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlbCompanyCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbCompanyFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Companies; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; } //HY: this should not have any license checks, otherwise we will find it difficult to load a new license key to them if their current one expired.
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());

			if (AccountingMasterFilesRegistry.Instance.EnableNetting.Value)
			{
				menuItems.Add(new ZMenuItem("-"));
				menuItems.Add(new ZMenuItem(Res.GetString("E284B950-AD17-4C4D-80F3-3CD14A0134EC", "Setup Netting"), new EventHandler(HandleNettingSetup)));
				menuItems.Add(new ZMenuItem(Res.GetString("E67503CF-B2E8-48A2-A5A1-CD57BE244222", "Import Transactions to Netting"), new EventHandler(HandleQueueTransactions)));
			}

			if (EInvoiceCredentialsProvider != null)
			{
				menuItems.Add(new ZMenuItem(Res.GetString("F2B1A49B-EC30-4E8A-9CB5-25207712C561", "Authorize for E-Invoicing"), new EventHandler(AuthorizeForEInvoicing)));
			}

			return menuItems.ToArray();
		}

		#endregion

		void HandleNettingSetup(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				ZFormModaliser.Show(new NettingSetupForm(new NettingSetupManager(new BusinessObjectFactory(), SelectedBusinessObjects.Select(x => x.PK).ToArray())), Grid.FindForm());
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		void HandleQueueTransactions(object sender, EventArgs e)
		{
			var nettingSystem = Factory.LoadTop1<NettingSystem>(new ZQuery());
			if (nettingSystem != null)
			{
				ZFormModaliser.Show(new NettingImportInitiatorForm(new NettingImportInitiator(new BusinessObjectFactory())), Grid.FindForm());
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("D4ACD72C-D57D-4BC3-BD95-7A56082D05E6", "There is no Netting System setup. Please setup a Netting System first before queuing transactions for Netting."));
			}
		}

		void AuthorizeForEInvoicing(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				ShowNoSelectedMessage();
				return;
			}

			if (SelectedBusinessObjects.Length > 1)
			{
				Globals.Message.ShowError(Res.GetString("0103E6F4-1546-459B-9704-28C22C944073", "You can only authorize one company at a time."));
				return;
			}

			var company = (GlbCompany)SelectedBusinessObjects[0];
			if (company.GC_RN_NKCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode || !EInvoiceCredentialsProvider.ShouldShowCredentialsTab(company))
			{
				Globals.Message.ShowError(Res.GetString("6326835D-AC5D-4AA6-BB32-282A316ACB37", "Action is unavailable for this company."));
				return;
			}

			var launchUrl = EInvoiceCredentialsProvider.GetAuthorizationURL(company) ?? "";
			WebUrlLauncher.Launch(launchUrl);

			EInvoiceCredentialsProvider.CreateOrUpdateEInvoicingCredential(company);
		}

		IEInvoiceCredentialsProvider EInvoiceCredentialsProvider => eInvoiceCredentialsProvider ??= ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IEInvoiceCredentialsProvider;
		IEInvoiceCredentialsProvider eInvoiceCredentialsProvider;
	}
}
