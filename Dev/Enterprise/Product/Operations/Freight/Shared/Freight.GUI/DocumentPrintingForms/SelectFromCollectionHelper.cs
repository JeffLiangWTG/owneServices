using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Freight.Business
{
	public class SelectFromCollectionHelper : IFindBox
	{
		public SelectFromCollectionHelper(BusinessObjectCollection bOs, ModuleIdentifier moduleID)
			: this(bOs, CreateModule(moduleID))
		{
		}

		public SelectFromCollectionHelper(BusinessObjectCollection bOs, ZFilterGridModule module)
		{
			fList = bOs;
			this.Module = module;
		}

		public void Select(Form parentFormForShowModal)
		{
			Module.OverrideModuleDecisionProvider(new SelectFromCollectionModuleDecisionProvider(this));

			LastShownPopup = new SelectorEmbeddedModulePopup(this, Module);
			LastShownPopup.Closed += new EventHandler(LastShownPopup_Closed);
			LastShownPopup.ShowModal(this, parentFormForShowModal);
		}

		public EmbeddedModulePopup LastShownPopup;

		#region Events

		public delegate void BusinessObjectSelectedHandler(SelectFromCollectionHelper sender, BusinessObject[] businessObjects);
		public event BusinessObjectSelectedHandler BusinessObjectSelected;

		protected void OnBusinessObjectSelected(BusinessObject[] businessObjects)
		{
			if (BusinessObjectSelected != null)
			{
				BusinessObjectSelected(this, businessObjects);
			}
		}

		#endregion

		#region IFindBox Members

		public IFindBoxPopup PopupForm
		{
			get { return LastShownPopup; }
		}

		IFindBoxListProvider IFindBox.ListProvider
		{
			get { return fList; }
		}

		string fDescription = "";
		string IFindBox.Description
		{
			get { return fDescription; }
			set { fDescription = null; }
		}

		string fCode = "";
		string IFindBox.Code
		{
			get { return fCode; }
			set { fCode = value; }
		}

		#endregion

		#region SelectorEmbeddedModulePopup

		internal class SelectorEmbeddedModulePopup : EmbeddedModulePopup
		{
			readonly SelectFromCollectionHelper Helper;

			public SelectorEmbeddedModulePopup(SelectFromCollectionHelper helper, ZFilterGridModule module) : base(module)
			{
				this.Helper = helper;
			}

			protected override void HandleSelection(BusinessObject[] selectedBizObjs)
			{
				base.HandleSelection(selectedBizObjs);

				if (selectedBizObjs.Length > 0)
				{
					Helper.OnBusinessObjectSelected(selectedBizObjs);
				}
			}
		}

		#endregion

		#region SelectFromCollectionModuleDecisionProvider

		internal class SelectFromCollectionModuleDecisionProvider : ModuleDecisionProvider
		{
			public SelectFromCollectionModuleDecisionProvider(IFindBox findBox) : base(findBox, null)
			{
			}

			public override bool AllowExcelExport
			{
				get { return false; }
			}
		}

		#endregion

		#region Implementation

		readonly BusinessObjectCollection fList;
		ZFilterGridModule Module;

		static ZFilterGridModule CreateModule(ModuleIdentifier moduleID)
		{
			return (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleID);
		}

		void LastShownPopup_Closed(object sender, EventArgs e)
		{
			if (LastShownPopup != null)
			{
				LastShownPopup.Dispose();
				LastShownPopup = null;
			}

			if (Module != null)
			{
				Module.Dispose();
				Module = null;
			}
		}

		#endregion
	}
}
