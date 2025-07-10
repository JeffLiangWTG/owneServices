using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingTmplController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.DtbBookingTmpl; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbBookingTmpl; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DtbBookingTmpl); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TransportBookingTemplateForm((DtbBookingTmpl)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DtbBookingTemplateView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.DtbBookingTemplateEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.DtbBookingTemplateNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DtbBookingTemplateDelete; }
		}
	}
}
