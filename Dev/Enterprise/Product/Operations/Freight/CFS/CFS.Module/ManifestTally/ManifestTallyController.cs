using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	/// <summary>
	/// Module Controller for ManifestTally.
	/// </summary>
	public class ManifestTallyController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public ManifestTallyController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ManifestTally; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ManifestTally; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(TallyContainer); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ManifestTallyForm((TallyContainer)businessEntity);
		}

		public override IZForm ShowNewForm()
		{
			throw new ModuleGuiNotSupportedException("Tally does not allow creation of new records");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CFSTally; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CFSTallyModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CFSTallyModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CFSTallyModify; }
		}

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.TallyContainer);
			factory.SetFreightDomainContext(FreightDomainContext.CFS);
			base.SetStrategyProvider(factory);
		}
	}
}
