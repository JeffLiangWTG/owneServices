using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.AMS.Module
{
	class CusInBondBillController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.US.AMSBill; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.US.AMSBill; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusInBondBill); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var header = (CusInBondHeader)businessEntity;
			var consol = header.Consol;
			return consol != null ? CreateConsolForm(consol) : CreateUSAMSForm(header);
		}

		IZForm CreateConsolForm(ForwardingConsol consol)
		{
			var result = new ConsolForm(consol);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.US.AMS;
			return result;
		}

		IZForm CreateUSAMSForm(CusInBondHeader header) => new USAMSForm(header);

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var bill = (CusInBondBill)sourceEntity;
			return Factory.Load<CusInBondHeader>(bill.B0_BH);
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.Customs.US.AMS.ToString();
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			ZForm form = null;
			if (sourceEntity is CusInBondHeader header)
			{
				var reloadHeader = Factory.Load<CusInBondHeader>(header.PK);
				form = (ZForm)CreateUSAMSForm(reloadHeader);
				ShowForm(form);
			}
			else if (sourceEntity is ForwardingConsol consol)
			{
				var reloadConsol = Factory.Load<ForwardingConsol>(consol.PK);
				form = (ZForm)CreateConsolForm(reloadConsol);
				ShowForm(form);
			}
			else if (sourceEntity is CusInBondBill bill)
			{
				form = (ZForm)base.ShowLoadedForm(bill, action);
				SelectAndShowBill(form, bill);
			}

			form?.DisableNewAction();
			return form;
		}

		void SelectAndShowBill(ZForm form, CusInBondBill bill)
		{
			var consolForm = form as ConsolForm;
			if (consolForm != null)
			{
				var plugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.US.AMS);
				if (plugIn.Enabled)
				{
					var userControl = plugIn.UserControl as USAMSConsolManifestUserControl;
					if (userControl != null)
					{
						userControl.SelectAndShowBill(bill.PK);
					}
				}
			}
			else
			{
				var amsForm = form as USAMSForm;
				if (amsForm != null)
				{
					amsForm.SelectAndShowBill(bill.PK);
				}
			}
		}

		#region Show Form

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Delete not supported from this AMS Bill.");
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Template copy not supported from AMS Bill.");
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			ZForm formToShow = null;
			if (businessEntity is CusInBondHeader header)
			{
				formToShow = (ZForm)CreateUSAMSForm(header);
			}
			else if (businessEntity is ForwardingConsol consol)
			{
				formToShow = (ZForm)CreateConsolForm(consol);
			}

			if (formToShow != null)
			{
				SetControllerID(formToShow, ID);
				formToShow.DisableNewAction();
				return formToShow;
			}
			else
			{
				throw new ModuleGuiNotSupportedException("New not supported from AMS Bill.");
			}
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ConsolAMSReporting; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ConsolAMSReporting; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ConsolAMSReporting; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ConsolAMSReporting; }
		}

		#endregion
	}
}
