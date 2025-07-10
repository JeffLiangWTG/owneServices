using System;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC015CProvider))]
sealed class CC015CProviderTest : DepartureHeaderProviderAbstractTest<CC015CProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC015CProvider(null));

	protected override string MessageType => "CC015C";

	protected override bool HasSendingActionParameter => true;

	public new void TestConsignment()
	{
		AssertType<CC015CConsignmentProvider>(Provider.Consignment);
	}

	public new void TestTransitOperation() => CombineAssertions(() =>
	{
		AssertNotNull("Not Null", Provider.TransitOperation);
		AssertType<TransitOperationWithBindingItineraryZeroProvider>("Type", Provider.TransitOperation);
	});
}
