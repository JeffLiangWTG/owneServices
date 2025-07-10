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
	public class DtbConsignmentRunSheetController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public DtbConsignmentRunSheetController()
		{
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = (DtbConsignmentRunSheet)base.GetNewBusinessEntityInLocalFactory();

			return result;
		}

		#region Controller / Module ID

		public override ControllerID ID
		{
			get { return ControllerIDs.DtbConsignmentRunSheet; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbConsignmentRunSheet; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DtbConsignmentRunSheet); }
		}

		#endregion Controller / Module ID

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new DtbConsignmentRunSheetForm((DtbConsignmentRunSheet)businessEntity);
		}

		#endregion Form

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DtbConsignmentRunSheetDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.DtbConsignmentRunSheetEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DtbConsignmentRunSheetView; }
		}

		#endregion Security

		public override IZForm ShowNewForm()
		{
			if (TransportRegistry.Instance.EnableLandTransport.Value)
			{
				var urlResult = GlowHelper.GenerateGotoGlowUrlForNewEntityAsync(GlowHelper.EntityName.RunSheet).Result;

				if (!string.IsNullOrWhiteSpace(urlResult.ErrorMessage))
				{
					var errorMessage = ResString.GetMultilingualString("71ba2d9a-a1c3-2f68-94b2-7445ffabbe73", "Cannot create new Run sheet in a browser.");
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
