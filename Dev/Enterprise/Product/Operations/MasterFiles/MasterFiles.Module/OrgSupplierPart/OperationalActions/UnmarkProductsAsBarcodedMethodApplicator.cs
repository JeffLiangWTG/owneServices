using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class UnmarkProductsAsBarcodedMethodApplicator : OperationalActionMethodApplicator
	{
		public UnmarkProductsAsBarcodedMethodApplicator(string name)
			: base(name)
		{
		}

		public UnmarkProductsAsBarcodedMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] products)
		{
			log.SetSectionProgressMax(products.Length);
			foreach (OrgSupplierPart product in products)
			{
				log.BumpSectionProgress();
				if (product.OP_IsBarcoded)
				{
					TryToSetProductNotBarcoded(product, log);
				}
				else
				{
					log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("5535fc72-c684-4fb0-8d56-0c804cb4155d", "Product {0} skipped - it is already non-barcoded.", product.OP_PartNum));
				}
			}
		}

		#region TryToSetProductNotBarcoded

		void TryToSetProductNotBarcoded(OrgSupplierPart product, IOperationalActionSectionLog log)
		{
			product.OP_IsBarcoded = false;
			product.Validation.ValidateOP_IsBarcoded();
			if (product.HasErrors)
			{
				string error = product.GetErrors().ToUniqueMessageListString(System.Environment.NewLine);
				log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("d6f4fa0c-e989-4062-a77b-4cf01aebe158", "Product {0} could not be unmarked as barcoded because of the following error(s):{1}{2}", product.OP_PartNum, System.Environment.NewLine, error));
				product.OP_IsBarcoded = true;
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("9db27369-2734-45f2-8bb3-d3c642ec3906", "Product {0} successfully unmarked as barcoded.", product.OP_PartNum));
			}
		}

		#endregion
	}
}
