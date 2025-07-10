using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Confirmations.Business;
using Enterprise.Freight.Confirmations.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Confirmations.Module
{
	public class QuickPODController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.PickupDeliveryConfirm; } //It doesn't need this?
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.QuickPOD; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(QuickPODs); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			QuickPODs quickPODs = (QuickPODs)businessEntity;
			return new QuickPODForm(quickPODs);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new QuickPODs(Factory);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowEditFormNotSupportedException("Not supported");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowViewFormNotSupportedException("Not supported");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowDeleteFormNotSupportedException("Not supported");
		}

		//Change security to use Quick POD security?

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PickupDeliveryConfirmations; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PickupDeliveryConfirmationsNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PickupDeliveryConfirmationsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.PickupDeliveryConfirmationsDelete; }
		}

		#endregion
	}
}
