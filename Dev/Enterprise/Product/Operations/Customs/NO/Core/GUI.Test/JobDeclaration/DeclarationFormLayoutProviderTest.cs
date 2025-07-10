using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.NO.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(DeclarationFormLayoutProvider))]
sealed class DeclarationFormLayoutProviderTest : Customs.GUI.Testing.DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
{
	protected override Type ExpectedDeclarationDetailsLayoutType => typeof(DeclarationDetailsLayouts);

	protected override Type ExpectedDeclarationShipmentDetailsLayoutType => typeof(ShipmentDetailsLayouts);

	protected override Type ExpectedDeclarationShipmentTypeLayoutType => typeof(ShipmentTypeLayouts);

	protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(MiscOptionsLayouts) } };

	protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(TransportDetailsLayout) } };

	protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(OrganisationsLayout);

	protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(CommercialInvoiceDetailsLayout) } };

	protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(EntryInstructionDetailsBasicLayout) } };

	protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
}
