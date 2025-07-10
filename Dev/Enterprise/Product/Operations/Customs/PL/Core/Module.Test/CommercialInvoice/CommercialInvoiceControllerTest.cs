using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Module.Testing;

[TestedType(typeof(CommercialInvoiceController))]
public class CommercialInvoiceControllerTest : EU.Module.Testing.CommercialInvoiceControllerTest
{
	protected override string CountryCode => Core.Constants.CountryCodes.Poland;

	protected override BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

	protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);

	public override Type ControllerToBashType => typeof(CommercialInvoiceController);
}
