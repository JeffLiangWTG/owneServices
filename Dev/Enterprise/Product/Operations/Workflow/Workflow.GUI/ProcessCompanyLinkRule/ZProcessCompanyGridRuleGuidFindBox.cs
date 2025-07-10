using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.MasterFiles.GUI
{
	public class ZProcessCompanyGridRuleGuidFindBox : ZGridGuidFindBox
	{
		public ZProcessCompanyGridRuleGuidFindBox() : base() { }

		protected sealed override IFindBoxPopup GetNewPopupForm()
		{
			var zFilterModule = NewModuleFromModuleID();
			zFilterModule.OverrideModuleDecisionProvider(new ProcessCompanyGridRuleModuleDecisionProvider(this, master));
			zFilterModule.FilterBusinessObject.ParentModuleID = ParentModuleID;
			zFilterModule.FilterBusinessObject.ParentType = ParentType;

			var result = CreateEmbeddedPopup(zFilterModule);
			result.Selected += HandleSelect;
			return result;
		}

		protected sealed override void OnPopupFormClosed(IFindBoxPopup popupForm)
		{
			if (popupForm is EmbeddedModulePopup popup)
			{
				popup.Selected -= HandleSelect;
			}
			base.OnPopupFormClosed(popupForm);
		}

		void HandleSelect(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects.Length > 1)
			{
				var first = e.SelectedBusinessObjects[0] as GlbCompany;
				this.SetCodeDescription(first);

				foreach (GlbCompany company in e.SelectedBusinessObjects.Skip(1))
				{
					var rule = master.RulesForBinding.AddNew();
					rule.PCR_GC_Company = company.PK;
				}
			}
		}

		ProcessCompanyLinkRule master => (ProcessCompanyLinkRule)((CargoWise.Windows.UI.KForm)ParentForm).DataSource;
	}

	public class ZProcessCompanyGridRuleGuidFindBoxColumnStyle : ZGuidFindBoxColumnStyle
	{
		public ZProcessCompanyGridRuleGuidFindBoxColumnStyle(ZProcessCompanyGridRuleGuidFindBoxColumnStyleInfo columnInfo) : this(() => new ZProcessCompanyGridRuleGuidFindBox(), columnInfo) { }

		protected ZProcessCompanyGridRuleGuidFindBoxColumnStyle(Func<ZProcessCompanyGridRuleGuidFindBox> gridFindBox1, ZProcessCompanyGridRuleGuidFindBoxColumnStyleInfo columnInfo) : base(gridFindBox1, columnInfo) { }
	}

	public class ZProcessCompanyGridRuleGuidFindBoxColumnStyleInfo : ZGuidFindBoxColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZProcessCompanyGridRuleGuidFindBoxColumnStyle); }
		}
	}

	class ProcessCompanyGridRuleModuleDecisionProvider : PopupModuleDecisionProviderWithMultipleSelect
	{
		public ProcessCompanyGridRuleModuleDecisionProvider(IFindBox findBox, ProcessCompanyLinkRule master) : base(findBox)
		{
			this.master = master;
		}

		protected override void HandleDefaultActionCore(BusinessObject[] selectedBusinessObjects)
		{
			base.HandleDefaultActionCore(selectedBusinessObjects.Where(o => o is GlbCompany company && !master.RulesForBinding.Find(rule => rule.PCR_GC_Company == company.PK).Any()).ToArray());
		}

		readonly ProcessCompanyLinkRule master;
	}
}
