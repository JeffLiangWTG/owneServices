using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(DeclarationFormLayoutProvider))]
sealed class DeclarationFormLayoutProviderTest : DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
{
	protected override Type ExpectedDeclarationDetailsLayoutType => null;

	protected override Type ExpectedDeclarationShipmentTypeLayoutType => typeof(ShipmentTypeLayout);

	protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(TransportDetailsLayout) } };

	protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(MiscOptionsLayouts) } };

	protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(OrganisationsLayout);

	protected override Type ExpectedDeclarationShipmentDetailsLayoutType => typeof(ShipmentDetailsLayouts);

	protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(ExportInvoiceDetailsLayout) },
		{ JobMessageTypeList.Codes.Import, typeof(ImportInvoiceDetailsLayout) }
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
}
