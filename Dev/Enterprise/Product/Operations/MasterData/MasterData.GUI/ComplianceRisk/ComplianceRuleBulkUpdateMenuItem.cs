using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI
{
	public class ComplianceRuleBulkUpdateMenuItem
	{
		readonly ZGrid zGrid;
		ZMenuItem menuItem;

		public ComplianceRuleBulkUpdateMenuItem(ZGrid grid)
		{
			zGrid = grid;
		}

		public void AddMenuItem()
		{
			menuItem = new ZMenuItem(ResString.GetMultilingualString("CA06E500-39D6-49DF-A47D-8749DADE5C23", "Copy To Other Countries/Regions"));
			menuItem.Enabled = false;
			menuItem.Click += BulkUpdateComplianceRules;

			zGrid.ContextMenu.MenuItems.Add(menuItem);
			zGrid.SelectedRowsChangedInMouseDown += UpdateMenuItem_OnSelectedElements;
		}

		void UpdateMenuItem_OnSelectedElements(object sender, EventArgs e)
		{
			if (zGrid.SelectedElements.Length > 0)
			{
				menuItem.Enabled = true;
			}
			else
			{
				menuItem.Enabled = false;
			}
		}

		void BulkUpdateComplianceRules(object sender, EventArgs e)
		{
			var selectedRules = zGrid.SelectedElements.Cast<ComplianceRule>().ToArray();

			if (selectedRules.Length == 0)
			{
				return;
			}

			if (selectedRules.Any(rule => rule.HasChanges || (rule.CurrentCountry?.HasChanges ?? true)))
			{
				Globals.Message.ShowWarning(Res.GetString("D293F76D-C307-41C4-BC3C-B6AFD64ED525", "Please save the form before copying the compliance rules."));
				return;
			}

			var countries = new ComplianceRuleTargetCountryCollection();
			var recordChooser = new ZRecordChooser<RefCountry>(ModuleIDs.RefCountry);

			recordChooser.ShowModal(zGrid.FindForm(), (RefCountry[] selectedCountries) =>
			{
				if (selectedCountries?.Length == 0)
				{
					return;
				}

				countries.AddAll(selectedCountries);

				var rules = new ComplianceRuleCandidateCollection();
				rules.AddAll(selectedRules);

				var complianceWrapper = new ComplianceRuleWrapper(rules, countries);

				if (ZFormModaliser.ShowDialogAndDispose(new ComplianceRuleBulkUpdateConfirmForm(complianceWrapper)) == DialogResult.OK)
				{
					CopyComplianceRules(selectedRules, countries);
				}
			});
		}

		protected void CopyComplianceRules(ComplianceRule[] selectedRules, ComplianceRuleTargetCountryCollection countries)
		{
			var factory = new BusinessObjectFactory();
			var origins = countries.Select(x => x.Code).ToList();
			var query = new ZQuery(ComplianceRuleSchema.CRU_Origin, origins);
			var existRules = factory.Load<ComplianceRule>(query).Select(x => (Origins: x.CRU_Origin, Destination: x.CRU_Destination, Code: x.CRU_HarmonizedCode)).ToHashSet();
			var addedRules = new List<ComplianceRule>();
			var copiedRules = selectedRules
				.GroupBy(x => (Destination: x.CRU_Destination, Code: x.CRU_HarmonizedCode))
				.Select(y => (y.Key.Destination, y.Key.Code, RiskStatus: y.First().CRU_RiskStatus)).ToList();

			foreach (var rule in copiedRules)
			{
				foreach (var country in origins)
				{
					if (!existRules.Contains((country, rule.Destination, rule.Code)) && country != rule.Destination)
					{
						var newRule = factory.New<ComplianceRule>();
						newRule.CRU_Origin = country;
						newRule.CRU_Destination = rule.Destination;
						newRule.CRU_HarmonizedCode = rule.Code;
						newRule.CRU_RiskStatus = rule.RiskStatus;

						addedRules.Add(newRule);
					}
				}
			}

			if (addedRules.Count > 0)
			{
				AddEditLog(addedRules, factory);
				factory.Save();
			}

			Globals.Message.ShowInformation(Res.GetString("6141F93C-C000-4592-94A5-8B7C53D0FC24", "Selected Compliance Rules have been copied successfully."));
		}

		void AddEditLog(List<ComplianceRule> rules, BusinessObjectFactory factory)
		{
			var origins = rules.Select(x => x.CRU_Origin).Distinct().ToList();
			var query = new ZQuery(RefCountrySchema.RN_Code, origins);
			var updateCountries = factory.Load<RefCountry>(query);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			updateCountries.ForEach(x => x.GetLogs().AddNew(AutoEvents.EditedARecord));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}
	}
}
