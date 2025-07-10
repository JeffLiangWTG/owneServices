using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public class DeliveryOrderPrintManager : Integration.Customs.US.IDeliveryOrderPrintManager
	{
		public DeliveryOrderPrintManager(JobDeclaration declarationToPrint)
		{
			this.declarationToPrint = declarationToPrint;
		}

		public bool IsOkToPrint()
		{
			bool result = false;
			JobDeclaration declaration = Factory.Load<JobDeclaration>(declarationToPrint.PK);
			if (declaration != null)
			{
				if (declaration.DeliveryOrderHeaders.Count == 0)
				{
					declaration.DeliveryOrderHeaders.AddNew();
				}
				result = ZFormModaliser.ShowDialogAndDispose(new CustomsDeliveryOrderForm(declaration)) == DialogResult.OK;
			}
			return result;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		readonly JobDeclaration declarationToPrint;
	}
}
