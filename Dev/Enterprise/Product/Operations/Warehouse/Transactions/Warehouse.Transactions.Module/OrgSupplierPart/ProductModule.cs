using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ProductModule : OrgSupplierPartModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsConfigProduct; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.WhsConfigProduct; }
		}

		protected override OperationalActionSupporter GetOperationalActionSupporterCore()
		{
			return new ProductActionSupporter();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WhsOrgSupplierPartFilterStripBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsConfigProduct);
		}

		protected override IZForm ShowNewForm()
		{
			var form = (ZForm)base.ShowNewForm();
			if (form != null) // form will be null if security denied
			{
				SetDefaultsOffFilters(form);
			}

			return form;
		}

		void SetDefaultsOffFilters(ZForm form)
		{
			var part = (OrgSupplierPart)form.BusinessEntity;
			var filterBizO = (WhsOrgSupplierPartFilterStripBusinessObject)FilterBusinessObject;

			var productCode = (filterBizO[WhsOrgSupplierPartFilterStripBusinessObject.Descriptions.ProductCode] as ModuleTextFilter)?.Property;
			if (!string.IsNullOrEmpty(productCode) && part != null)
			{
				part.OP_PartNum = productCode.Value;
			}
		}
	}
}