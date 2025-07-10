using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.US.GUI
{
	public class USCustomsNumberViewStmNumsCompanyProvider : CustomsNumberViewStmNumsCompanyProvider, ICustomsNumberViewStmNumsGuiProvider
	{
		public USCustomsNumberViewStmNumsCompanyProvider(BusinessObjectFactory factory, ZGuid ownerPk)
			: base(factory, Core.Constants.CountryCodes.UnitedStates, ownerPk)
		{ }

		public override bool EnableBranchLevel => true;
		public override bool EnableCompanyLevel => true;
		public Form GetEditorForm(CustomsNumberViewStmNumsWrapper wrapper)
		{
			return new USCustomsNumberViewStmNumsEditorForm((USCustomsNumberViewStmNumsWrapper)wrapper);
		}

		public new USCustomsNumberViewStmNumsWrapperCollection CustomsNumberWrappers => (USCustomsNumberViewStmNumsWrapperCollection)base.CustomsNumberWrappers;

		protected override CustomsNumberViewStmNumsWrapperCollection NewCustomsNumberWrappers()
		{
			return new USCustomsNumberViewStmNumsWrapperCollection(CustomsNumbers);
		}

		public Control GetUserControl()
		{
			return new USCustomsNumberViewStmNumsUserControl(CustomsNumberWrappers);
		}

		protected override CustomsNumberViewStmNumsSetting GetSettingCore(ZString rangeType)
		{
			return new USCustomsNumberViewStmNumsSetting(Company, rangeType);
		}

		protected override CustomsNumberViewStmNumsLookups GetNewLookups(CustomsNumberViewStmNums stmNums)
		{
			return new USCustomsNumberViewStmNumsLookups(stmNums);
		}

		protected override CustomsNumberViewStmNumsValidation GetNewValidation(CustomsNumberViewStmNums stmNums)
		{
			return new USCustomsNumberViewStmNumsValidation(stmNums);
		}

		protected override Type WrapperType => typeof(USCustomsNumberViewStmNumsWrapper);

		protected override CustomsNumberViewStmNumsWrapper CreateWrapperCore(CustomsNumberViewStmNums stmNums)
		{
			return new USCustomsNumberViewStmNumsWrapper(stmNums);
		}

		protected override ZString GetDetailCore(CustomsNumberViewStmNumsWrapper wrapper)
		{
			var usWrapper = (USCustomsNumberViewStmNumsWrapper)wrapper;
			return FormattableString.Invariant($"Owner: {GetOwnerType(usWrapper.StmNums)} - {usWrapper.SN_OwnerForDisplay}, Range Type: {usWrapper.SN_Type}, Check Digit Addition: {usWrapper.CheckDigitAddition}, Applies To: {usWrapper.AppliesTo}");
		}
	}
}
