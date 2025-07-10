using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class MarkProductsAsBarcodedMethodApplicator : OperationalActionMethodApplicator
	{
		public MarkProductsAsBarcodedMethodApplicator(string name)
			: base(name)
		{
		}

		public MarkProductsAsBarcodedMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] products)
		{
			log.SetSectionProgressMax(products.Length);
			foreach (OrgSupplierPart product in products)
			{
				log.BumpSectionProgress();
				if (!product.OP_IsBarcoded)
				{
					TryToSetProductToBeBarcoded(product, log);
				}
				else
				{
					log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("ef2de452-a7d7-4ae1-89b0-9609432bdb86", "Product {0} skipped - it is already barcoded.", product.OP_PartNum));
				}
			}
		}

		#region TryToSetProductToBeBarcoded

		void TryToSetProductToBeBarcoded(OrgSupplierPart product, IOperationalActionSectionLog log)
		{
			product.OP_IsBarcoded = true;
			product.Validation.ValidateOP_IsBarcoded();

			if (product.HasErrors)
			{
				string error = product.GetErrors().ToUniqueMessageListString(System.Environment.NewLine);
				log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("c5b89332-8a7e-41de-9dd7-8b6abd04e4e8", "Product {0} could not be marked as barcoded because of the following error(s):{1}{2}", product.OP_PartNum, System.Environment.NewLine, error));

				product.OP_IsBarcoded = false;
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("b75aecde-5536-4ca2-b838-142a32524e29", "Product {0} marked as barcoded successfully.", product.OP_PartNum));
			}
		}

		#endregion
	}
}
