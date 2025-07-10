using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	class UniversalShipmentExtensionTest : TestCaseWithFactory
	{
		public void TestContainsRecipientRoleAndServiceCode()
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();

			universalShipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAR, ServiceCode = ServiceCodeType.VGM } };
			Assert(universalShipment.ContainsRecipientRoleAndServiceCode(RecipientRoleType.CAR, ServiceCodeType.VGM));

			universalShipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAR, ServiceCode = ServiceCodeType.BRQ }, new RecipientRole { Code = RecipientRoleType.CAP, ServiceCode = ServiceCodeType.VGM } };
			Assert(!universalShipment.ContainsRecipientRoleAndServiceCode(RecipientRoleType.CAR, ServiceCodeType.VGM));
		}
	}
}
