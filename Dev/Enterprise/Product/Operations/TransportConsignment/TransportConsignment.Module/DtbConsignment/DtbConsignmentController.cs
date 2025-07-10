using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Common;
using Enterprise.TransportConsignment.GUI;
using Enterprise.TransportConsignment.Module.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbConsignmentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.DtbConsignment;

		public override ModuleIdentifier ModuleID => ModuleIDs.DtbConsignment;

		public override Type TypeOfTopLevelBusinessObject => typeof(DtbConsignment);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.DtbConsignmentView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.DtbConsignmentEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.DtbConsignmentDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new DtbConsignmentForm((DtbConsignment)businessEntity);
		}

		public override IZForm ShowNewForm()
		{
			if (TransportRegistry.Instance.EnableLandTransport.Value)
			{
				var urlResult = GlowHelper.GenerateGotoGlowUrlForNewEntityAsync(GlowHelper.EntityName.Consignment).Result;

				if (!string.IsNullOrWhiteSpace(urlResult.ErrorMessage))
				{
					var errorMessage = ResString.GetMultilingualString("76ba2d4a-a1c6-4f68-94b2-7445ffabbe73", "Cannot create new Consignment in a browser.");
					Globals.Message.ShowError(errorMessage + System.Environment.NewLine + urlResult.ErrorMessage);
					return null;
				}

				WebUrlLauncher.Launch(urlResult.Uri.ToString());
			}
			else
			{
				LandTransportInformationDisplayer.ShowLandTransportDisabledInformation();
			}

			return null;
		}
	}
}
