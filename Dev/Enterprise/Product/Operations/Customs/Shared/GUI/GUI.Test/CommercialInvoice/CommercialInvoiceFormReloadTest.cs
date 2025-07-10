using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormReloadTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			Factory.Save();

			var secondFactory = new BusinessObjectFactory();
			var result = new CommercialInvoiceForm(secondFactory.Load<BaseJobComInvoiceHeader>(invoiceHeader.PK));
			result.ControllerID = ControllerIDs.CommercialInvoice;
			return result;
		}
	}
}
