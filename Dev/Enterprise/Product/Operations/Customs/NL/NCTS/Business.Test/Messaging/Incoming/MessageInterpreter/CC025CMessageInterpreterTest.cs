using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC025CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var mockCC025C = new Mock<ICC025CDataProvider>();
		var interpreter = new CC025CMessageInterpreter();

		var packagingProviderMock1 = new Mock<INCTSPackagingProvider>();
		packagingProviderMock1.Setup(m => m.NumberOfPackages).Returns(5);
		packagingProviderMock1.Setup(m => m.TypeOfPackages).Returns("PKG");
		packagingProviderMock1.Setup(m => m.ShippingMarks).Returns("marksandnumbers");
		var packagingProvider1 = packagingProviderMock1.Object;

		var consignmentItemProvider3Mock = new Mock<INCTSConsignmentItemProvider>();
		consignmentItemProvider3Mock.Setup(m => m.DeclarationGoodsItemNumber).Returns(3);
		consignmentItemProvider3Mock.Setup(m => m.ReleaseType).Returns(2);
		consignmentItemProvider3Mock.Setup(m => m.Packagings).Returns(new ReadOnlyCollection<INCTSPackagingProvider>(new List<INCTSPackagingProvider>() { packagingProvider1 }));
		var consignmentItemProvider3 = consignmentItemProvider3Mock.Object;

		var packagingProviderMock2 = new Mock<INCTSPackagingProvider>();
		packagingProviderMock2.Setup(m => m.NumberOfPackages).Returns(5);
		packagingProviderMock2.Setup(m => m.TypeOfPackages).Returns("PKG");
		packagingProviderMock2.Setup(m => m.ShippingMarks).Returns("marksandnumbers2");
		var packagingProvider2 = packagingProviderMock2.Object;

		var packagingProviderMock3 = new Mock<INCTSPackagingProvider>();
		packagingProviderMock3.Setup(m => m.NumberOfPackages).Returns(10);
		packagingProviderMock3.Setup(m => m.TypeOfPackages).Returns("BOX");
		packagingProviderMock3.Setup(m => m.ShippingMarks).Returns("marksandnumbers3");
		var packagingProvider3 = packagingProviderMock3.Object;

		var consignmentItemProvider4Mock = new Mock<INCTSConsignmentItemProvider>();
		consignmentItemProvider4Mock.Setup(m => m.DeclarationGoodsItemNumber).Returns(4);
		consignmentItemProvider4Mock.Setup(m => m.ReleaseType).Returns(2);
		consignmentItemProvider4Mock.Setup(m => m.Packagings).Returns(new ReadOnlyCollection<INCTSPackagingProvider>(new List<INCTSPackagingProvider>() { packagingProvider2, packagingProvider3 }));
		var consignmentItemProvider4 = consignmentItemProvider4Mock.Object;

		var houseConsignmentProviderMock = new Mock<INCTSHouseConsignmentProvider>();
		houseConsignmentProviderMock.Setup(m => m.SequenceNumeric).Returns(1);
		houseConsignmentProviderMock.Setup(m => m.ReleaseType).Returns(2);
		houseConsignmentProviderMock.Setup(m => m.ConsignmentItems).Returns(new ReadOnlyCollection<INCTSConsignmentItemProvider>(new List<INCTSConsignmentItemProvider>() { consignmentItemProvider3, consignmentItemProvider4 }));
		var houseConsignmentProvider = houseConsignmentProviderMock.Object;

		mockCC025C.Setup(x => x.HouseConsignments).Returns(new ReadOnlyCollection<INCTSHouseConsignmentProvider>(new List<INCTSHouseConsignmentProvider>() { houseConsignmentProvider }));

		CombineAssertions(() =>
		{
			mockCC025C.Setup(m => m.ReleaseIndicator).Returns(NLNctsConstants.ReleaseIndicator.FullRelease);
			var result = interpreter.Interpret(mockCC025C.Object);
			AssertEquals("release type 1", "All Goods are released for transit upon arrival. The movement is closed.</br>1) House Bill: full release</br>3) Item: full release</br>-- 5 PKG with the marks and numbers 'marksandnumbers' are released</br>4) Item: full release</br>-- 5 PKG with the marks and numbers 'marksandnumbers2' are released</br>-- 10 BOX with the marks and numbers 'marksandnumbers3' are released", result);
			mockCC025C.Setup(m => m.ReleaseIndicator).Returns(NLNctsConstants.ReleaseIndicator.PartialRelease);
			consignmentItemProvider3Mock.Setup(m => m.ReleaseType).Returns(2);
			consignmentItemProvider4Mock.Setup(m => m.ReleaseType).Returns(1);
			houseConsignmentProviderMock.Setup(m => m.ReleaseType).Returns(1);
			result = interpreter.Interpret(mockCC025C.Object);
			AssertEquals("release type 2", "Goods are partially released.</br>1) House Bill: partial release</br>3) Item: full release</br>-- 5 PKG with the marks and numbers 'marksandnumbers' are released</br>4) Item: partial release</br>-- 5 PKG with the marks and numbers 'marksandnumbers2' are released</br>-- 10 BOX with the marks and numbers 'marksandnumbers3' are released", result);
			mockCC025C.Setup(m => m.ReleaseIndicator).Returns(NLNctsConstants.ReleaseIndicator.PartialReleaseClosed);
			result = interpreter.Interpret(mockCC025C.Object);
			AssertEquals("release type 3", "Goods are partially released. The movement is closed.</br>1) House Bill: partial release</br>3) Item: full release</br>-- 5 PKG with the marks and numbers 'marksandnumbers' are released</br>4) Item: partial release</br>-- 5 PKG with the marks and numbers 'marksandnumbers2' are released</br>-- 10 BOX with the marks and numbers 'marksandnumbers3' are released", result);
			mockCC025C.Setup(m => m.ReleaseIndicator).Returns(NLNctsConstants.ReleaseIndicator.NoRelease);
			result = interpreter.Interpret(mockCC025C.Object);
			AssertEquals("release type 4", "No release of Goods.", result);
		});
	}
}
