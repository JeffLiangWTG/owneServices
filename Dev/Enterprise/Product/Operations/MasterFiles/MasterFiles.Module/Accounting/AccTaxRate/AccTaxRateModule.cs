using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Accounting.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccTaxRateModule : ZFilterGridModule
	{
		public AccTaxRateModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccTaxRate; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems())
			{
				new ZMenuItem(CreateApplicableTaxIdsMenuText, HandleCreateApplicableTaxIds)
			};

			if (AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				menuItems.Add(new ZMenuItem(LoadDefaultPostingGroupsMenuText, HandleLoadDefaultPostingGroups));
			}

			if (Env.CurrentUser.IsDeveloper)
			{
				menuItems.Add(new ZMenuItem(CountryComplianceInfoMenuText, HandleCountryComplianceInfoDisplayForm));
			}

			if (Env.CurrentUser.IsDeveloper)
			{
				menuItems.Add(new ZMenuItem(UpdateTaxRateForTaxFramework, HandleUpdateTaxRateForTaxFramework));
			}

			return menuItems.ToArray();
		}

#if DEBUG
		public MenuItem[] GetNewActionMenuItems_Test()
		{
			return GetNewActionMenuItems();
		}
#endif

		protected MultilingualString CreateApplicableTaxIdsMenuText
		{
			get { return ResString.GetMultilingualString("b7df8a49-dcbf-4f63-ad1b-be8bcefa6484", "Create Applicable Tax IDs"); }
		}

		protected MultilingualString CountryComplianceInfoMenuText
		{
			get { return ResString.GetMultilingualString("DB386D70-B91C-4D03-9612-61CACE58A69E", "Country/Region Compliance Info"); }
		}

		protected MultilingualString UpdateTaxRateForTaxFramework
		{
			get { return ResString.GetMultilingualString("9D16CCD0-5971-49C3-A493-535E0F8C6EA4", "Update Tax Rate for Tax Framework"); }
		}

		void HandleCreateApplicableTaxIds(object sender, EventArgs e)
		{
			if (Env.Security.GSTTaxRatesAllowCreateApplicableTaxIDs.IsAllowed)
			{
				var newFactory = new BusinessObjectFactory();
				var errMsg = AccTaxRate.CreateApplicableTaxIds(newFactory);
				if (!string.IsNullOrEmpty(errMsg))
				{
					Globals.Message.Show(errMsg);
				}
				else
				{
					newFactory.Save();
					Globals.Message.Show(Res.GetString("77cdbb8b-e57b-4126-b722-d1accc48b064", "The Tax ID Set for the current Login Country/Region has successfully updated."));
				}
			}
			else
			{
				Globals.Message.Show(Env.Security.GSTTaxRatesAllowCreateApplicableTaxIDs.ErrorMessageForNotAllowed);
			}
		}

		void HandleCountryComplianceInfoDisplayForm(object sender, EventArgs e)
		{
			var form = new CountryComplianceInfoDisplayForm(new CountryComplianceInfoDisplay());
			form.Show();
		}

		void HandleUpdateTaxRateForTaxFramework(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new TaxFrameworkAccTaxRateForm(new TaxFrameworkAccTaxRateLoader()), Grid.FindForm());
		}

		protected MultilingualString LoadDefaultPostingGroupsMenuText
		{
			get { return ResString.GetMultilingualString("5377b5af-f24e-4ed3-9f94-44399c5e1de3", "Load Default Posting Groups"); }
		}

		void HandleLoadDefaultPostingGroups(object sender, EventArgs e)
		{
			if (Env.Security.GSTTaxRatesAllowLoadDefaultPostingGroups.IsAllowed)
			{
				var result = Globals.Message.Show(Res.GetString("84e77a1b-1fa9-4964-aa20-e89306ef8d53", @"The 'Load Default Posting Groups' function will re-default Posting Group values on Tax IDs.
Are you sure you want to update your Tax ID Posting Groups to their default values?"), string.Empty,
							 MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					var newFactory = new BusinessObjectFactory();
					var importer = new TaxRateImporter(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.PK, newFactory);
					if (importer.UpdatePostingGroupsFromXMLFile() > 0)
					{
						newFactory.Save();
						Globals.Message.Show(Res.GetString("0fbae247-82a1-457a-88e5-a1c48b9e8163", "The Tax ID Default Posting Group values for the current Login Country/Region have successfully updated."));
					}
					else
					{
						Globals.Message.Show(Res.GetString("2f1f8bc7-47ed-48bb-ac52-82846e0b91f8", "The current Login Country/Region's Tax ID Posting Groups are already set to the correct default value. No changes have been made."));
					}
				}
			}
			else
			{
				Globals.Message.Show(Env.Security.GSTTaxRatesAllowLoadDefaultPostingGroups.ErrorMessageForNotAllowed);
			}
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("74713010-CF83-4c45-ABC2-BFF7B8F35A3D", "Deactivate", "Deletes the selected item after viewing its details read-only (shortcut Del)");
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccTaxRate);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccTaxRateFilterControl(GridCollection, (AccTaxRateFilterBusinessObject)FilterBusinessObject, IsAuxiliaryRateGridColumnsVisible);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccTaxRateCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccTaxRateFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GSTTaxRates; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override bool AllowUniversalCopy
		{
			get { return GlbStaff.CurrentUser.IsSupportUser; }
		}

		bool IsAuxiliaryRateGridColumnsVisible
		{
			get
			{
				var extraTaxRateFilter = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				extraTaxRateFilter.AddToFilter(AccTaxRateSchema.AT_ExtraTaxRateType, SQLComparisonOperator.NotEqual, ZString.Empty);
				return Factory.ExistsInDatabase(AccTaxRateSchema.Constants.TableName, extraTaxRateFilter);
			}
		}
	}
}
