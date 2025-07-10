using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondMoveDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB9_B0()
		{
			var moveDetailMock = Factory.NewMoq<CusInBondMoveDetailForTesting>();
			var moveDetail = moveDetailMock.Object;
			moveDetail.B9_B0 = ZGuid.Empty;
			AssertHasError(moveDetail.B9_B0Info, "Bill number cannot be empty.");
			moveDetail.B9_B0 = ZGuid.NewZGuid();
			AssertNoError(moveDetail.B9_B0Info, "Bill number cannot be empty.");

			var validationMock = new Mock<CusInBondMoveDetailValidation>(moveDetail);
			moveDetailMock.Protected().Setup<CusInBondMoveDetailValidation>("GetNewValidation").Returns(validationMock.Object);
			validationMock.Protected().Setup<bool>("IsBillNumberMandatory").Returns(false);
			moveDetail.B9_B0 = ZGuid.Empty;
			AssertNoError(moveDetail.B9_B0Info, "Bill number cannot be empty.");
		}

		public class CusInBondMoveDetailForTesting : CusInBondMoveDetail
		{
			public CusInBondMoveDetailForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			protected override Type ContainerTypeCore => throw new NotImplementedException();
			protected override Type MoveLineItemTypeCore => throw new NotImplementedException();
			protected override Type PackTypeCore => throw new NotImplementedException();
			protected override ICusInBondContainerCollection GetContainersCollection()
			{
				throw new NotImplementedException();
			}
		}
	}
}
