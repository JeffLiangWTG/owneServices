using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	internal class CarrierServiceLevelDataObjectWriterTest : TestCaseWithFactory
	{
		#region TestCarrierServiceLevelFieldMappings

		public void TestCarrierServiceLevelFieldMappings()
		{
			var serviceLevelBO = GetCarrierServiceLevel(Factory);
			var serviceLevelData = new CarrierServiceLevelDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, serviceLevelBO))).GetDataObject(serviceLevelBO);

			AssertNotNull("serviceLevelData", serviceLevelData);

			CombineAssertions(() => AssertContents(serviceLevelData));
		}

		#endregion

		#region Implementation

		internal static OrgCarrierServiceLevel GetCarrierServiceLevel(BusinessObjectFactory factory)
		{
			var carrierServiceLevel = factory.New<OrgCarrierServiceLevel>();
			carrierServiceLevel.PL_Code = "ABC";
			carrierServiceLevel.PL_CarrierServiceLevelDescription = "service level description";
			carrierServiceLevel.PL_CarrierServiceCode = "1234";
			carrierServiceLevel.PL_ChargeCode = "555555";
			carrierServiceLevel.PL_ProductCode = "1212121212";
			carrierServiceLevel.PL_APProfileID = "AP3333";
			carrierServiceLevel.PL_IsSignatureRequired = true;

			return carrierServiceLevel;
		}

		internal static void AssertContents(ServiceLevel serviceLevelDataObject)
		{
			AssertEquals("serviceLevelDataObject.Code", "ABC", serviceLevelDataObject.Code);
			AssertEquals("serviceLevelDataObject.Description", "service level description", serviceLevelDataObject.Description);
			AssertEquals("serviceLevelDataObject.CarrierServiceCode", "1234", serviceLevelDataObject.CarrierServiceCode);
			AssertEquals("serviceLevelDataObject.CarrierProductCode", "1212121212", serviceLevelDataObject.CarrierProductCode);
			AssertEquals("serviceLevelDataObject.CarrierChargeCode", "555555", serviceLevelDataObject.CarrierChargeCode);
			AssertEquals("serviceLevelDataObject.CarrierProfileID", "AP3333", serviceLevelDataObject.CarrierProfileID);
		}

		#endregion
	}
}
