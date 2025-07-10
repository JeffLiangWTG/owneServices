using System;

using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public class PackContainersController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ControllerID ID
		{
			get { return ControllerIDs.PackContainers; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobSailing); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			JobSailing selectedSailing = (JobSailing)businessEntity;
			businessEntity.Factory.AllowMultipleBusinessObjectsAroundOneRow = false;
			PackContainerHelper sailingHelper = new PackContainerHelper(selectedSailing);
			return new PackContainerForm(sailingHelper);
		}

		public override IZForm ShowNewForm()
		{
			throw new ModuleGuiNotSupportedException("New not supported for pack containers.");
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.SailingSchedule; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SailingScheduleNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.SailingScheduleEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.SailingScheduleDelete; }
		}

		#endregion
	}
}
