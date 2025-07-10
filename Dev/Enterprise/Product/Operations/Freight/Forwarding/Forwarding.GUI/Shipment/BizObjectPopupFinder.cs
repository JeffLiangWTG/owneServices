using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class BizObjectPopupFinder : IFindBox
	{
		#region Nested Types

		public class ModulePopup : EmbeddedModulePopup
		{
			public ModulePopup(ZFilterGridModule module)
				: base(module)
			{
			}

			public Action<BusinessObject[]> SelectedBizObjAction
			{
				get; set;
			}

			protected override void HandleSelection(BusinessObject[] selectedBizObjs)
			{
				base.HandleSelection(selectedBizObjs);

				if (selectedBizObjs.Length > 0 && SelectedBizObjAction != null)
				{
					SelectedBizObjAction(selectedBizObjs);
				}
			}
		}

		public class PopupDecisionProvider : ModuleDecisionProvider
		{
			readonly bool? allowExcelExport;
			readonly bool? allowMultiSelect;
			readonly bool? shouldIgnoreAdditionalFilter;

			public PopupDecisionProvider(IFindBox findBox, bool? allowExcelExport = null, bool? allowMultiSelect = null, bool? shouldIgnoreAdditionalFilter = null)
				: base(findBox, null)
			{
				this.allowExcelExport = allowExcelExport;
				this.allowMultiSelect = allowMultiSelect;
				this.shouldIgnoreAdditionalFilter = shouldIgnoreAdditionalFilter;
			}

			public override bool AllowExcelExport
			{
				get { return allowExcelExport ?? base.AllowExcelExport; }
			}

			public override bool AllowMultiSelect
			{
				get { return allowMultiSelect ?? base.AllowMultiSelect; }
			}

			public override bool ShouldIgnoreAdditionalFilter
			{
				get { return shouldIgnoreAdditionalFilter ?? base.ShouldIgnoreAdditionalFilter; }
			}
		}

		#endregion

		#region Ctor

		public BizObjectPopupFinder(ZFilterGridModule module, BusinessObjectCollection listProvider)
		{
			Module = module;
			Code = Description = string.Empty;
			ListProvider = listProvider;
		}

		#endregion

		#region IFindBox Members

		public string Code
		{
			get; set;
		}

		public string Description
		{
			get; set;
		}

		public IFindBoxListProvider ListProvider
		{
			get; private set;
		}

		public IFindBoxPopup PopupForm
		{
			get; private set;
		}

		#endregion

		#region Properties

		public Action<BusinessObject[]> SelectedBizObjAction
		{
			get; set;
		}

		public PopupDecisionProvider DecisionProvider
		{
			get; set;
		}

		public ZFilterGridModule Module
		{
			get; private set;
		}

		#endregion

		#region Implementation

		public void ShowModal(Form form)
		{
			if (DecisionProvider == null)
			{
				DecisionProvider = new PopupDecisionProvider(this, false, false, false);
			}

			Module.OverrideModuleDecisionProvider(DecisionProvider);

			ModulePopup popup = new ModulePopup(Module);
			popup.SelectedBizObjAction = (bizObjArray) =>
				{
					if (SelectedBizObjAction != null && bizObjArray != null && bizObjArray.Length > 0)
					{
						SelectedBizObjAction(bizObjArray);
					}
				};

			PopupForm = popup;
			PopupForm.ShowModal(this, form);
		}

		#endregion
	}
}
