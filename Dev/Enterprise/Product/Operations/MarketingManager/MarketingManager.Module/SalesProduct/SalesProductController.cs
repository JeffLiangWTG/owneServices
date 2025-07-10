using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	public class SalesProductController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Standard Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SalesProduct; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.SalesProduct; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgSalesProduct); }
		}

		#endregion

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var salesProduct = (OrgSalesProduct)businessEntity;
			return new SalesProductForm(salesProduct);
		}

		#endregion

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.SalesProductsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.SalesProductsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SalesProductsNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.SalesProductsView; }
		}

		#endregion
	}
}
