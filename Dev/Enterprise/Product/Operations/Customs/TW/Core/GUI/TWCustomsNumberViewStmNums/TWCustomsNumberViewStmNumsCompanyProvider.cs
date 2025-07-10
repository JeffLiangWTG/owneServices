using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public class TWCustomsNumberViewStmNumsCompanyProvider : CustomsNumberViewStmNumsCompanyProvider, ICustomsNumberViewStmNumsGuiProvider
	{
		public TWCustomsNumberViewStmNumsCompanyProvider(BusinessObjectFactory factory, ZGuid ownerPk)
			: base(factory, Core.Constants.CountryCodes.Taiwan, ownerPk)
		{ }

		public override bool EnableBranchLevel => false;
		public override bool EnableCompanyLevel => true;
		public Form GetEditorForm(CustomsNumberViewStmNumsWrapper wrapper)
		{
			return new TWCustomsNumberViewStmNumsEditorForm((TWCustomsNumberViewStmNumsWrapper)wrapper);
		}

		public new TWCustomsNumberViewStmNumsWrapperCollection CustomsNumberWrappers => (TWCustomsNumberViewStmNumsWrapperCollection)base.CustomsNumberWrappers;

		protected override CustomsNumberViewStmNumsWrapperCollection NewCustomsNumberWrappers()
		{
			return new TWCustomsNumberViewStmNumsWrapperCollection(CustomsNumbers);
		}

		public Control GetUserControl()
		{
			return new TWCustomsNumberViewStmNumsUserControl(CustomsNumberWrappers);
		}

		protected override CustomsNumberViewStmNumsSetting GetSettingCore(ZString rangeType)
		{
			return new TWCustomsNumberViewStmNumsSetting(Company, rangeType);
		}

		protected override CustomsNumberViewStmNumsLookups GetNewLookups(CustomsNumberViewStmNums stmNums)
		{
			return new TWCustomsNumberViewStmNumsLookups(stmNums);
		}

		protected override CustomsNumberViewStmNumsValidation GetNewValidation(CustomsNumberViewStmNums stmNums)
		{
			return new TWCustomsNumberViewStmNumsValidation(stmNums);
		}

		protected override Type WrapperType => typeof(TWCustomsNumberViewStmNumsWrapper);

		protected override CustomsNumberViewStmNumsWrapper CreateWrapperCore(CustomsNumberViewStmNums stmNums)
		{
			return new TWCustomsNumberViewStmNumsWrapper(stmNums);
		}

		protected override ZString GetDetailCore(CustomsNumberViewStmNumsWrapper wrapper)
		{
			var twWrapper = (TWCustomsNumberViewStmNumsWrapper)wrapper;
			return FormattableString.Invariant($"Owner: {GetOwnerType(twWrapper.StmNums)} - {twWrapper.SN_OwnerForDisplay}, Range Type: {twWrapper.RangeType}, MessageType: {twWrapper.MessageType}");
		}
	}
}
