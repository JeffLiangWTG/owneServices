using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NZ.Module
{
	public class OutwardReportController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.NZ.OutwardReport; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusEntryNumber); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This controller does not have GUI.");
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.NZCustomsOutwardReport; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NZCustomsOutwardReport; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.NZCustomsOutwardReport; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.NZ.Module.Res.GetData("PlugInTabPage|OutwardReport", "Outward Report"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OutwardReportPlugInToConsol((ForwardingConsol)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
