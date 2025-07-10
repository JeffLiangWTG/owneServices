using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Module
{
	class CusInBondHeaderController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.US.AMS; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.US.AMS; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusInBondHeader); }
		}

		internal bool CreateNVOCCAMS;

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = (CusInBondHeader)base.GetNewBusinessEntityInLocalFactory();
			if (CreateNVOCCAMS)
			{
				result.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			}
			return result;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			ZForm result = null;
			var header = (CusInBondHeader)businessEntity;
			result = new USAMSForm(header);
			return result;
		}

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

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.US.AMS.Module.Res.GetData("PlugInTabPage|USAMS", "AMS"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new USAMSPlugIn((ForwardingConsol)businessEntity);
		}
	}
}
