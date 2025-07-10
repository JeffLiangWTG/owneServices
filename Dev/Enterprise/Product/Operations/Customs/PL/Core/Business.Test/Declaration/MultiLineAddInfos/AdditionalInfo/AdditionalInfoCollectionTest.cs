using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(AdditionalInfoCollection))]
class AdditionalInfoCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.AdditionalInfoCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.Invoices.AddNew();
		var line = header.InvoiceLines.AddNew();
		return line.AdditionalInfos;
	}

	protected override Type GetExpectedCollectionType() => typeof(AdditionalInfoCollection);
}
