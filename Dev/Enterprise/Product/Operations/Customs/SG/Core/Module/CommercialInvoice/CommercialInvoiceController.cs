using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.GUI;

namespace Enterprise.Customs.SG.V4.Module
{
	public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobComInvoiceHeader); }
		}

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
		}
	}
}
