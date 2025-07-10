using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MarketingManager.Module
{
	public class TradeProfileForOrgPluginController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ID

		public override ControllerID ID
		{
			get { return ControllerIDs.TradeProfileForOrgPlugin; }
		}

		#endregion

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("not supported");
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("not supported"); }
		}

		#endregion

		#region PlugIn

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new TradeProfileForOrgPlugin((OrgHeader)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("67caaae7-dfdb-4a31-89d0-f0060c128860", "Value Analysis"); }
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return null; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ClientIntelligenceModifyTradeProfile; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return null; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ClientIntelligenceViewTradeProfile; }
		}

		#endregion
	}
}
