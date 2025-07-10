using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Freight.GUI
{
	public class VoyageIFindBox : IFindBox
	{
		public static void SelectSeaVoyage(ZForm parentForm, ZPropertyInfo info)
		{
			if (parentForm == null)
			{
				throw new ArgumentNullException(nameof(parentForm));
			}

			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			SelectSeaVoyage(parentForm, info.BizObj.Factory, delegate(ZGuid value)
			{ info.Value = value; });
		}
		public static void SelectSeaVoyage(ZForm parentForm, BusinessObjectFactory factory, Action<ZGuid> propertySetter)
		{
			if (parentForm == null)
			{
				throw new ArgumentNullException(nameof(parentForm));
			}

			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (propertySetter == null)
			{
				throw new ArgumentNullException(nameof(propertySetter));
			}

			new VoyageIFindBox(parentForm, new JobVoyageCollection(factory), propertySetter).Show();
		}

		VoyageIFindBox(ZForm parentForm, IFindBoxListProvider listProvider, Action<ZGuid> propertySetter)
		{
			this.parentForm = parentForm;
			this.listProvider = listProvider;
			this.propertySetter = propertySetter;
		}

		void Show()
		{
			ZFilterGridModule module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.JobSeaVoyage);

			if (module.SecurityCheckpoint.IsAllowed)
			{
				module.OverrideModuleDecisionProvider(new PopupModuleDecisionProvider(this));

				popup = new VoyageModulePopup(module, propertySetter);
				popup.ShowModal(this, parentForm);
			}
			else
			{
				module.SecurityCheckpoint.ShowError();
				module.Dispose();
			}
		}

		#region IFindBox Members

		string IFindBox.Code
		{
			get { return ""; }
			set { }
		}

		string IFindBox.Description
		{
			get { return ""; }
			set { }
		}

		IFindBoxListProvider IFindBox.ListProvider
		{
			get { return listProvider; }
		}

		IFindBoxPopup IFindBox.PopupForm
		{
			get { return popup; }
		}

		#endregion

		#region VoyageModulePopup

		internal class VoyageModulePopup : EmbeddedModulePopup
		{
			public VoyageModulePopup(ZFilterModule module, Action<ZGuid> propertySetter)
				: base(module)
			{
				if (propertySetter == null)
				{
					throw new ArgumentNullException(nameof(propertySetter));
				}

				this.propertySetter = propertySetter;
			}

			protected override void HandleSelection(BusinessObject[] selectedBizObjs)
			{
				if (selectedBizObjs.Length > 0)
				{
					propertySetter(selectedBizObjs[0].PK);
				}

				Dispose();
			}

			readonly Action<ZGuid> propertySetter;
		}

		#endregion

		IFindBoxPopup popup;

		readonly ZForm parentForm;
		readonly IFindBoxListProvider listProvider;
		readonly Action<ZGuid> propertySetter;
	}
}
