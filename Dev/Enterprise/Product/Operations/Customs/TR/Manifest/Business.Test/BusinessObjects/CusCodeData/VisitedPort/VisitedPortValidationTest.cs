using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	class VisitedPortValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			var port = header.VisitedPorts.AddNew();
			AssertNoMessageErrorContaining(port.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			port.CY_Code = "XXXXX";
			AssertHasMessageErrorContaining(port.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			port.CY_Code = "TR092-001";
			AssertNoMessageErrorContaining(port.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			var port2 = header.VisitedPorts.AddNew();
			port2.CY_Code = "TR092-001";
			AssertHasErrorContaining(port2.CY_CodeInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(port2.CY_CodeInfo.Description));
			port2.CY_Code = "TRIST-001";
			AssertNoMessageErrorContaining(port.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			var port3 = header.VisitedPorts.AddNew();
			port3.CY_Data = "TRIST";
			AssertHasErrorContaining(port3.CY_CodeInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(port3.CY_CodeInfo.Description));
			port3.CY_Code = "TRIST-002";
			AssertNoMessageErrorContaining(port.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			port3.CY_Data = "SGSIN";
			port3.Validation.ValidateCY_Code();
			AssertHasMessageErrorContaining(port3.CY_CodeInfo, "\"Local Code\" on the Port(UNLOCO) UNLOCO record.");
			header.AMA_TransportMode = "AIR";
			port3.Validation.ValidateCY_Code();
			AssertHasWarningContaining(port3.CY_CodeInfo, "The Port(TR) code will not be sent to customs if the transport mode is not sea.");
		}

		public void TestCheckCY_Data()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_TransportMode = "SEA";
			var port = header.VisitedPorts.AddNew();
			port.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining("After set value, there should not be message error on AMA_DateAtCustomsOffice", port.CY_DataInfo, "You have not entered a Port(UNLOCO) or Port(TR).");
			port.CY_Data = "XXXXX";
			port.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(port.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			port.CY_Data = "TRAJI";
			AssertNoMessageErrorContaining(port.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			var port2 = header.VisitedPorts.AddNew();
			port2.CY_Data = "TRAJI";
			AssertHasErrorContaining(port2.CY_DataInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(port2.CY_DataInfo.Description));
			port2.CY_Data = "TRIST";
			AssertNoMessageErrorContaining(port.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			var port3 = header.VisitedPorts.AddNew();
			port3.CY_Data = "SGSIN";
			AssertNoMessageErrorContaining(port.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			var port4 = header.VisitedPorts.AddNew();
			port4.CY_Code = "TRIZT-001";
			AssertNoMessageErrorContaining(port.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			var port5 = header.VisitedPorts.AddNew();
			port5.CY_Code = "TRIZT-002";
			AssertNoMessageErrorContaining(port.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			var port6 = header.VisitedPorts.AddNew();
			port6.CY_Code = "TR092-002";
			AssertNoMessageErrorContaining(port.CY_DataInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.Port, "TRIST-001", "AHIRKAPI DEMİR MEVKİİ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.Port, "TRIST-002", "ATAKÖY MARİNA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.Port, "TR092-001", "ATAKÖY MARİNA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.Port, "TR092-002", "ATAKÖY MARİNA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.Port, "TRIZT-001", "ATAKÖY MARİNA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.Port, "TRIZT-002", "ATAKÖY MARİNA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var map1 = Factory.New<RefLocoMap>();
			map1.RY_IsSystem = true;
			map1.RY_LocalPortCode = "TRIST-001";
			map1.RY_RL_NKLocoPort = "TRIST";
			map1.RY_RN = new ZGuid("A12B3D34-08FE-4AF5-A8D6-5B99E9AAB655");
			map1.RY_IsSystem = true;
			map1.RY_SystemUsage = "CUS";
			var map2 = Factory.New<RefLocoMap>();
			map2.RY_IsSystem = true;
			map2.RY_LocalPortCode = "TRIST-002";
			map2.RY_RL_NKLocoPort = "TRIST";
			map2.RY_RN = new ZGuid("A12B3D34-08FE-4AF5-A8D6-5B99E9AAB655");
			map2.RY_IsSystem = true;
			map2.RY_SystemUsage = "CUS";
			var map3 = Factory.New<RefLocoMap>();
			map3.RY_IsSystem = true;
			map3.RY_LocalPortCode = "TRIZT-001";
			map3.RY_RL_NKLocoPort = "TRIZT";
			map3.RY_RN = new ZGuid("A12B3D34-08FE-4AF5-A8D6-5B99E9AAB655");
			map3.RY_IsSystem = true;
			map3.RY_SystemUsage = "CUS";
			var map4 = Factory.New<RefLocoMap>();
			map4.RY_IsSystem = true;
			map4.RY_LocalPortCode = "TRIZT-002";
			map4.RY_RL_NKLocoPort = "TRIZT";
			map4.RY_RN = new ZGuid("A12B3D34-08FE-4AF5-A8D6-5B99E9AAB655");
			map4.RY_IsSystem = true;
			map4.RY_SystemUsage = "CUS";
			Factory.Save();
		}
	}
}
