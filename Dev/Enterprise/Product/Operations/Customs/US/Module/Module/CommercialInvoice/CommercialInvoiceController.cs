using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;

namespace Enterprise.Customs.US.Module
{
	public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(JobComInvoiceHeader);

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity) => new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
	}
}
