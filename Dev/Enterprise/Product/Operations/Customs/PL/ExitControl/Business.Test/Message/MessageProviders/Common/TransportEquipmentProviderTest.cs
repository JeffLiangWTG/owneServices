using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class TransportEquipmentProviderTest : DataProviderTestCase<TransportEquipmentProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var expectedMessage = string.Empty;

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitContainer";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitContainer')";
#endif

		AssertExceptionThrown<ArgumentNullException>("Null CusExitContainer", expectedMessage,
			() => new TransportEquipmentProvider(1, null, null));

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitConsignment";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitConsignment')";
#endif

		AssertExceptionThrown<ArgumentNullException>("Null CusExitConsignment", expectedMessage,
			() => new TransportEquipmentProvider(1, cusExitContainer, null));
	});

	public void TestSequenceNumber() => AssertEquals(999, Provider.SequenceNumber);

	public void TestContainerIdentificationNumber() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("CXN_ContainerNumber is not set", GetProvider().ContainerIdentificationNumber);

		cusExitContainer.CXN_ContainerNumber = "ABCD";
		AssertEquals("CXN_ContainerNumber is not empty", "ABCD", GetProvider().ContainerIdentificationNumber);
	});

	public void TestNumberOfSeals() => CombineAssertions(() =>
	{
		AssertEquals("AllSealNumbers is not set", ZInt.Zero, GetProvider().NumberOfSeals);

		var seal1 = cusExitContainer.AllSealNumbers.AddNew();
		seal1.BK_SealNumber = "11";
		var seal2 = cusExitContainer.AllSealNumbers.AddNew();
		seal2.BK_SealNumber = "22";
		var seal3 = cusExitContainer.AllSealNumbers.AddNew();
		seal3.BK_SealNumber = "33";
		AssertEquals("3 Seals exist", 3, GetProvider().NumberOfSeals);
	});

	public void TestSeals() => CombineAssertions(() =>
	{
		AssertEquals("AllSealNumbers is not set", ZInt.Zero, GetProvider().Seals.Count);

		var seal1 = cusExitContainer.AllSealNumbers.AddNew();
		seal1.BK_SealNumber = "11";
		var seal2 = cusExitContainer.AllSealNumbers.AddNew();
		seal2.BK_SealNumber = "22";
		var providerSeals = GetProvider().Seals;
		AssertEquals("2 Seals exist", 2, providerSeals.Count);
		AssertEquals("Seals sequence number should start from 1", 1, providerSeals.First().SequenceNumber);
		AssertEquals("2nd seal sequence number should start be 2", 2, providerSeals.Last().SequenceNumber);
	});

	public void TestGoodsReferences() => CombineAssertions(() =>
	{
		AssertEquals("GoodsReferences is not set", ZInt.Zero, GetProvider().GoodsReferences.Count);

		var cusExitConsignmentItem1 = cusExitConsignment.CusExitConsignmentItems.AddNew();
		cusExitConsignmentItem1.CCI_LineNumber = 11;
		cusExitConsignmentItem1.CusExitConsignmentPivots.AddNew();
		var item1pivot2 = cusExitConsignmentItem1.CusExitConsignmentPivots.AddNew();
		item1pivot2.CNP_CXN_Container = cusExitContainer.PK;
		var cusExitConsignmentItem2 = cusExitConsignment.CusExitConsignmentItems.AddNew();
		cusExitConsignmentItem2.CCI_LineNumber = 22;
		var item2pivot1 = cusExitConsignmentItem2.CusExitConsignmentPivots.AddNew();
		item2pivot1.CNP_CXN_Container = cusExitContainer.PK;
		var cusExitConsignmentItem3 = cusExitConsignment.CusExitConsignmentItems.AddNew();
		cusExitConsignmentItem3.CCI_LineNumber = 33;
		cusExitConsignmentItem3.CusExitConsignmentPivots.AddNew();
		cusExitContainer.CXN_Sequence = 5;
		var goodsReferences = GetProvider().GoodsReferences;
		AssertEquals("2 CusExitConsignmentItems out of 3 contain searched container", 2, goodsReferences.Count);
		var firstItem = goodsReferences.First();
		AssertEquals("first DeclarationGoodsItemNumber should have cusExitConsignmentItem1.CCI_LineNumber", 11, firstItem.DeclarationGoodsItemNumber);
		AssertEquals("first DeclarationGoodsItemNumber should have exitContainer.CXN_Sequence", 5, firstItem.SequenceNumber);
		var lastItem = goodsReferences.Last();
		AssertEquals("2nd DeclarationGoodsItemNumber should have cusExitConsignmentItem2.CCI_LineNumber", 22, lastItem.DeclarationGoodsItemNumber);
		AssertEquals("2nd DeclarationGoodsItemNumber should have exitContainer.CXN_Sequence", 5, lastItem.SequenceNumber);
	});

	protected override TransportEquipmentProvider GetProvider() => new TransportEquipmentProvider(999, cusExitContainer, cusExitConsignment);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<CusExitHeader>();
		cusExitConsignment = header.CusExitConsignments.AddNew();
		cusExitContainer = header.CusExitContainers.AddNew();
	}

	CusExitContainer cusExitContainer;
	CusExitConsignment cusExitConsignment;
}
