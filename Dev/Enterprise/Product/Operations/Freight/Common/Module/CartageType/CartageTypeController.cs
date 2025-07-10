using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Common.Module
{
	public class CartageTypeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ControllerID ID
		{
			get { return ControllerIDs.CartageType; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CartageType; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CommonCartageType); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			IZForm result;
			result = new CartageTypeForm((CommonCartageType)businessEntity);
			return result;
		}

		#region Implementation

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.LocalCartageJobType; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.LocalCartageJobTypeEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.LocalCartageJobTypeNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.LocalCartageJobTypeDelete; }
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			var cartageType = sourceEntity as CommonCartageType;
			if (cartageType != null)
			{
				if (cartageType.CanDelete)
				{
					return base.ShowDeleteForm(sourceEntity);
				}
				else
				{
					Globals.Message.ShowError(cartageType.ReasonForNotAbleToDelete, Res.GetString("26addc48-f8fe-475c-b9d6-fc883e221999", "Cannot Delete Record"));
					return null;
				}
			}

			return null;
		}

		#endregion
	}
}
