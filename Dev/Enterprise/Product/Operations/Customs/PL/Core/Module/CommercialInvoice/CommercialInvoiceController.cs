using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.Module;

public class CommercialInvoiceController : EU.Module.CommercialInvoiceController
{
	public CommercialInvoiceController()
	{
	}

	public override Type TypeOfTopLevelBusinessObject => typeof(JobComInvoiceHeader);

	protected override IZForm GetForm(IBusiness businessEntity)
	{
		return new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
	}
}
