using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestsSubclassesOf(typeof(BaseCustomsDeclarationUserControl))]
	public abstract class BaseCustomsDeclarationUserControlAbstractTest<T, D> : TestCaseWithFactory
		where T : BaseCustomsDeclarationUserControl, new()
		where D : BaseJobDeclaration
	{
		protected virtual Type TestNumbersUserControl() => typeof(NumbersUserControl);

		public void TestOrgAddressControlsUnderOrgTab()
		{
			var declaration = GetDeclarationForOrgAddressControlsUnderOrgTab();
			using (var form = new ZForm(declaration))
			using (var control = GetCustomsDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				foreach (var declarationSetup in SetupDeclarationForTestScenarios(declaration))
				{
					declarationSetup();
					var organisationTabPage = control.OrganisationsTabPage;
					AssertTestOrgAddressControlsUnderOrgTab(organisationTabPage);
				}
			}
		}

		protected virtual IEnumerable<Action> SetupDeclarationForTestScenarios(BaseJobDeclaration declaration) => declaration.Lookups.MessageTypeList.GetAllCodes().Select(x => new Action(() => declaration.JE_MessageType = x));

		protected BaseJobDeclaration GetDeclarationForOrgAddressControlsUnderOrgTab() => Factory.New<D>();

		protected void AssertTestOrgAddressControlsUnderOrgTab(ZTabPage organizationTabPage)
		{
			var failedReason = new ZStringBuilder();
			var docOrgAddressControls = organizationTabPage.FindAll<ZDocAddressControl>(x => x.DisplayMode != ZDocAddressControlDisplayMode.ShowOverrideAndTabs && x.DisplayMode != ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox);
			foreach (var docOrgAddressControl in docOrgAddressControls)
			{
				failedReason.AppendLine($"Display mode of {docOrgAddressControl.Name} is {docOrgAddressControl.DisplayMode}. The mode should be either SingleLineNoOverrideNoGroupBox or ShowOverrideAndTabs.");
			}

			var orgAddressControls = organizationTabPage.FindAll<ZAddressControl>(x => x.StackControls);
			foreach (var orgAddressControl in orgAddressControls)
			{
				failedReason.AppendLine($"StackControls of {orgAddressControl.Name} is true. Please set it to false.");
			}

			var organisationControls = organizationTabPage.FindAll<ZOrganisationControl>(x => x.Parent.GetType() != typeof(ZDocAddressControl));
			if (organisationControls.Any())
			{
				foreach (var organisationControl in organisationControls)
				{
					failedReason.AppendLine($"The type of {organisationControl.Name} is ZOrganisationControl. You shouldn't add this type under this tab. Please change it to ZGuidFindBox.");
				}
			}

			if (failedReason.Length > 0)
			{
				Assert(failedReason.ToString(), false);
			}
			else
			{
				Assert(true);
			}
		}

		BaseCustomsDeclarationUserControl GetCustomsDeclarationUserControl() => new T();
	}
}
