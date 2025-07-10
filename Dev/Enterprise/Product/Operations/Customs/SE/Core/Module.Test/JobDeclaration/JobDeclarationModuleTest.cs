using System;
using Enterprise.Customs.SE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	public sealed class JobDeclarationModuleTest : EU.Module.Testing.JobDeclarationModuleTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Sweden;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);
	}
}
