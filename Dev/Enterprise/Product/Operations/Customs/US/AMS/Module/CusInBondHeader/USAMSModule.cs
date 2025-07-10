using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Module
{
	public class USAMSModule : ZFilterGridModule
	{
		public MenuItem NewVOCCItem { get; private set; }
		public MenuItem NewNVOCCItem { get; private set; }

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewStandardMenuItems());
			result.Insert(0, NewVOCCItem = new ZMenuItem(Res.GetString("ModuleGrid.NewVOCC", "New &VOCC"), HandleNewClick));
			result.Insert(1, NewNVOCCItem = new ZMenuItem(Res.GetString("ModuleGrid.NewNVOCC", "New &NVOCC"), ShowNewNVOCCForm));
			return result.ToArray();
		}

		protected override void SetupButtonDetailForItem(MenuItem item, ref IconTypes buttonImage, ref IconTypes buttonImageActive, ref string buttonToolTip)
		{
			if (item == NewVOCCItem || item == NewNVOCCItem)
			{
				buttonImage = IconTypes.NewButtonRest;
				buttonImageActive = IconTypes.NewButtonActive;
			}
			else
			{
				base.SetupButtonDetailForItem(item, ref buttonImage, ref buttonImageActive, ref buttonToolTip);
			}
		}

		void ShowNewNVOCCForm(object sender, EventArgs e)
		{
			try
			{
				createNVOCCAMS = true;
				ShowNewForm();
			}
			finally
			{
				createNVOCCAMS = false;
			}
		}
		bool createNVOCCAMS;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController result = null;
			var aMS = selectedBusinessObject as CusInBondHeader;
			var consol = aMS?.Consol;
			if (consol == null)
			{
				result = ZControllerFactory.Create(ControllerIDs.Customs.US.AMS);
				((CusInBondHeaderController)result).CreateNVOCCAMS = createNVOCCAMS;
			}
			else
			{
				result = ZControllerFactory.Create(ControllerIDs.Customs.US.AMSPluggedIntoConsol);
			}

			return result;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new USAMSFilterStrip();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new USAMSFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ActiveBusinessObjectCollection<CusInBondHeader>(Factory, new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.AMS));
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.US.AMS; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ConsolAMSReporting; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.USAMSWorkflowDescriptorCode; }
		}
	}
}
