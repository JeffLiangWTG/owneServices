using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingConsolidationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.DtbBookingConsolidation; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbBookingConsolidation; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DtbBookingConsolidation); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var consolidation = (DtbBookingConsolidation)base.GetNewBusinessEntityInLocalFactory();

			using (consolidation.SuspendSettingHasChanges())
			{
				consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			}

			return consolidation;
		}

		protected override void ShowModelessForm(IZForm form)
		{
			base.ShowModelessForm(form);

			var transportBookingMultiForm = form as TransportBookingMultiForm;
			transportBookingMultiForm.SetView(DtbDeliveryManager.DefaultBookingView);
		}

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Consolidation);
			DtbFormStateService.SetState(factory, DtbFormState.Booking);
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new DtbBookingConsolidationPlugIn(businessEntity);
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TransportBookingMultiForm((DtbBookingConsolidation)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DtbBookingConsolidationView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.DtbBookingConsolidationEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.DtbBookingConsolidationNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DtbBookingConsolidationDelete; }
		}
	}
}
