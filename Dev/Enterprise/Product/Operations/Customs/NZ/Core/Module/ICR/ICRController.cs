using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NZ.Module
{
	public class ICRController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.NZ.InwardCargoReport; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusEntryNumber); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This controller does not have GUI.");
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.NZCustomsInwardCargoReport; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NZCustomsInwardCargoReport; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.NZCustomsInwardCargoReport; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.NZ.Module.Res.GetData("PlugInTabPage|InwardCargoReport", "Inward Cargo Report"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			/*
			 *	Currently disabled - ICR from Consols is still under development and to be determined - Gary / BKG.
			 *	Access still to be determined
			 */
			return null;
			//ForwardingConsol consolEntity = BusinessEntity as ForwardingConsol;
			//if (consolEntity != null)
			//{
			//	return new ICRPluginToConsol((ForwardingConsol)BusinessEntity);
			//}
			//else
			//{
			//	return null;
			//	//return new ICRPluginToBrokerage((JobDeclaration)BusinessEntity);
			//}
		}
	}
}
