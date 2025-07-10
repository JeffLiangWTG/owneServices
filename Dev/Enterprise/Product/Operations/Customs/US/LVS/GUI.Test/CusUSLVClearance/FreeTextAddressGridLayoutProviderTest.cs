using System;
using System.Collections.Generic;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing;

[TestedType(typeof(FreeTextAddressGridLayoutProvider))]
public class FreeTextAddressGridLayoutProviderTest : GridColumnLayoutProviderAbstractTest<FreeTextAddressGridLayoutProvider>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns =>
	[
		(FreeTextAddress<CusUSLVConsignment>.Schema.WaybillNumber, typeof(ZTextBoxColumnStyleInfo), 200),
		(FreeTextAddress<CusUSLVConsignment>.Schema.PartyName, typeof(ZTextBoxColumnStyleInfo), 160),
		(FreeTextAddress<CusUSLVConsignment>.Schema.Address1, typeof(ZTextBoxColumnStyleInfo), 200),
		(FreeTextAddress<CusUSLVConsignment>.Schema.Address2, typeof(ZTextBoxColumnStyleInfo), 160),
		(FreeTextAddress<CusUSLVConsignment>.Schema.City, typeof(ZTextBoxColumnStyleInfo), 140),
		(FreeTextAddress<CusUSLVConsignment>.Schema.State, typeof(ZTextBoxColumnStyleInfo), 140),
		(FreeTextAddress<CusUSLVConsignment>.Schema.Postcode, typeof(ZTextBoxColumnStyleInfo), 140),
		(FreeTextAddress<CusUSLVConsignment>.Schema.Country, typeof(ZTextBoxColumnStyleInfo), 140)
	];

	protected override Type GridBoundEntityType => typeof(FreeTextAddress<CusUSLVConsignment>);
}
