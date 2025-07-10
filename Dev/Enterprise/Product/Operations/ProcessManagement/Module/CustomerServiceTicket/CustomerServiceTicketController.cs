using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Module
{
	public class CustomerServiceTicketController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CustomerServiceTicketForm((WorkRequest)businessEntity);
		}

		public override ControllerID ID => ControllerIDs.CustomerServiceTicket;

		public override ModuleIdentifier ModuleID => ModuleIDs.CustomerServiceTicket;

		public override Type TypeOfTopLevelBusinessObject => typeof(WorkRequest);

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomerServiceTicketNew;
		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomerServiceTicketView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomerServiceTicketEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomerServiceTicketDelete;
	}
}
