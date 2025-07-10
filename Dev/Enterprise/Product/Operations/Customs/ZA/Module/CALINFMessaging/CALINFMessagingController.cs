using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.ZA.Module
{
	public class CALINFMessagingController : ZController
	{
		public override ControllerID ID => ZAControllerIDs.CALINFMessagingPlugin;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption { get { return Res.GetData("32AF9389-CF98-4897-9EF6-9B8053EE2537", "CALINF Messages"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ZPlugIn result = null;
			var voyage = businessEntity as JobVoyage;

			if (voyage != null)
			{
				result = new CALINFMessagingPlugin(voyage);
			}

			return result;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
		#endregion

		#region Not Supported
		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");
		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("Not supported");
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not supported");
		}
		#endregion
	}
}
