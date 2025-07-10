using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(PlugInLayoutProvider))]
sealed class PlugInLayoutProviderTest : EU.GUI.PlugIn.Testing.PlugInLayoutProviderAbstractTest
{
	protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Poland;

	protected override Type GetExpectedInvoiceHeaderPreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => typeof(PreviousDocumentsFieldsLayout);

	protected override Type GetExpectedInvoiceLinePreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => typeof(PreviousDocumentsFieldsLayout);

	protected override Type GetExpectedDeclarationPreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => typeof(PreviousDocumentsFieldsLayout);

	protected override Type GetExpectedEntryInstructionPreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => typeof(PreviousDocumentsFieldsLayout);
}
