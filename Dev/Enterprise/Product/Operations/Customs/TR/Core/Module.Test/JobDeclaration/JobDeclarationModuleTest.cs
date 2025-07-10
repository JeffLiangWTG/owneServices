using System;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	public class JobDeclarationModuleTest : EU.Module.Testing.JobDeclarationModuleTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Turkey;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);
	}
}
