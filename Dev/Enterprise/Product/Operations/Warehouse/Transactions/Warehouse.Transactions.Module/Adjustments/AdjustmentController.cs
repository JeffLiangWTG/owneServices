using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class AdjustmentController : WhsControllerBase
	{
		public AdjustmentController()
			: this(AdjustmentType.Codes.Adjustment)
		{
		}

		public AdjustmentController(string adjustmentType)
		{
			docketSubType = adjustmentType;
		}
		readonly string docketSubType;

		public override ControllerID ID
		{
			get { return ControllerIDs.WhsAdjustment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsAdjustment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsAdjustment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			return new AdjustmentEntryForm((WhsAdjustment)businessEntity, whsNotificationSubscriberGuiHelper);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WhsAdjustmentView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WhsAdjustmentNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WhsAdjustmentEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WhsAdjustmentDelete; }
		}

		public override IZForm ShowNewForm()
		{
			var form = (AdjustmentEntryForm)base.ShowNewForm();
			if (form != null) // form will be null if security denied
			{
				form.SetupInventoryFilterStripUserControl();
			}
			return form;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			var form = (AdjustmentEntryForm)base.ShowEditForm(sourceEntity);
			if (!((WhsDocket)sourceEntity).IsFinalised)
			{
				if (form != null) // form can be null if source entity is not saved in DB
				{
					form.SetupInventoryFilterStripUserControl();
				}
			}
			return form;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			using (adjustment.GetValidationSuspender())
			{
				adjustment.WD_DocketSubType = docketSubType;
				if (docketSubType == AdjustmentType.Codes.OwnershipAdjustment)
				{
					var childAdjustment = Factory.New<WhsAdjustment>();
					childAdjustment.WD_WD_ParentDocket = adjustment.PK;
					childAdjustment.WD_DocketSubType = docketSubType;
				}
			}

			return adjustment;
		}
	}
}
