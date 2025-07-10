using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CustomsNumberViewStmNumsGuiProviderForTesting : CustomsNumberViewStmNumsCompanyProviderForTest, ICustomsNumberViewStmNumsGuiProvider
	{
		public CustomsNumberViewStmNumsGuiProviderForTesting(BusinessObjectFactory factory, ZString countryCode, ZGuid companyPk, bool? enableCompanyLevelForTesting = true, bool? enableBranchLevelForTesting = true)
			: base(factory, countryCode, companyPk, enableCompanyLevelForTesting, enableBranchLevelForTesting)
		{
		}

		public Control GetUserControl()
		{
			return getUserControlForTesting?.Invoke(this);
		}
		public Func<CustomsNumberViewStmNumsCompanyProviderForTest, Control> getUserControlForTesting;

		public Form GetEditorForm(CustomsNumberViewStmNumsWrapper wrapper)
		{
			return getEditorFormForTesting?.Invoke(this, wrapper);
		}
		public Func<CustomsNumberViewStmNumsCompanyProviderForTest, CustomsNumberViewStmNumsWrapper, Form> getEditorFormForTesting;
	}
}
