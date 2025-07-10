using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(DeclarationFormLayoutProvider))]
sealed class DeclarationFormLayoutProviderTest : DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
{
	protected override Type ExpectedDeclarationDetailsLayoutType => null;

	protected override Type ExpectedDeclarationShipmentTypeLayoutType => typeof(ShipmentTypeLayout);

	protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(ExportTransportDetailsLayout) },
		{ JobMessageTypeList.Codes.Import, typeof(ImportTransportDetailsLayout) },
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(MiscOptionsLayouts) } };

	protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(OrganisationsLayout);

	protected override Type ExpectedDeclarationShipmentDetailsLayoutType => typeof(ShipmentDetailsLayout);

	protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(ExportInvoiceDetailsLayout) },
		{ JobMessageTypeList.Codes.Import, typeof(ImportInvoiceDetailsLayout) },
		{ JobMessageTypeList.Codes.MiscellaneousCustoms, typeof(ImportInvoiceDetailsLayout) },
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(EntryInstructionDetailsBasicUserControlLayout) },
		{ JobMessageTypeList.Codes.Import, typeof(EntryInstructionDetailsBasicUserControlLayout) },
		{ JobMessageTypeList.Codes.MiscellaneousCustoms, typeof(EntryInstructionDetailsBasicUserControlLayout) },
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(ExportInvoiceChargesGridColumLayout) },
		{ JobMessageTypeList.Codes.Import, null },
		{ JobMessageTypeList.Codes.MiscellaneousCustoms, null },
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(ExportInvoiceChargesGridColumLayout) },
		{ JobMessageTypeList.Codes.Import, null },
		{ JobMessageTypeList.Codes.MiscellaneousCustoms, null },
	};
	protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(ExportInvoiceChargesGridColumLayout) },
		{ JobMessageTypeList.Codes.Import, null },
		{ JobMessageTypeList.Codes.MiscellaneousCustoms, null },
	};
}
