using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public class ContainerMoveController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ControllerID ID
		{
			get { return ControllerIDs.AgencyContainerMove; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AgencyContainerMove; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ContainerMovement); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ContainerManagerForm((RefContainerStock)businessEntity);
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var movement = (ContainerMovement)sourceEntity;

			var refContainerStock = Factory.Load<RefContainerStock>(movement.E9_R6);
			refContainerStock.CurrentMovement = movement;

			return refContainerStock;
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.AgencyContainerManager.ToString();
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			var movement = sourceEntity as ContainerMovement;

			if (sourceEntity is RefContainerStock refContainerStock)
			{
				movement = refContainerStock.CurrentMovement;
			}

			var form = (ContainerManagerForm)base.ShowLoadedForm(movement, action);
			if (form != null)
			{
				form.SelectAndShowMovement(movement.PK);
			}
			return form;
		}

		#region Show Form

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Delete not supported for container movements.");
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Template copy not supported for container movements.");
		}

		public override IZForm ShowNewForm()
		{
			if (CheckPointForNew.IsAllowed)
			{
				BulkMovementsHeader header = new BulkMovementsHeader(Factory);
				BulkMovementsForm form = new BulkMovementsForm(header);
				ShowForm(form);
				return form;
			}
			else
			{
				CheckPointForNew.ShowError();
				return null;
			}
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("New not supported for container movements.");
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AgencyContainerManager; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AgencyContainerManagerEdit; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AgencyContainerManagerEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AgencyContainerManagerEdit; }
		}

		#endregion
	}
}


