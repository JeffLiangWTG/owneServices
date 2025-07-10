using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class EquipmentCertificateTypesProviderTest : TestCaseWithFactory
	{
		public void TestDefaultCertificateTypesWhenLicenseValid()
		{
			var certTypes = (CertificateTypeCollection)cert.Lookups.CertificateTypes;
			AssertCertificateType(certTypes, ConveyanceReferences.Codes.ACEId, ConveyanceReferences.Descriptions.ACEId, false, true, true, AlertTypeList.Codes.NoAlert);
			AssertCertificateType(certTypes, ConveyanceReferences.Codes.CarrierId, ConveyanceReferences.Descriptions.CarrierId, false, true, true, AlertTypeList.Codes.NoAlert);
		}

		public void AssertCertificateType(CertificateTypeCollection types, string code, string desc, bool mandatory, bool unique, bool system, string alertType)
		{
			var type = types.Find(code);
			AssertNotNull(string.Format("Certificate '{0}' should be added if license is valid ", code), type);
			AssertEquals("Description", desc, type.Description);
			AssertEquals("IsMandatory", mandatory, type.IsMandatory);
			AssertEquals("IsUnique", unique, type.IsUnique);
			AssertEquals("IsSystem", system, type.IsSystem);
			AssertEquals("AlertType", alertType, type.AlertType);
		}

		public void TestDefaultTypesSpecificValidation()
		{
			var provider = new EquipmentCertificateTypesProvider();
			var validationType = typeof(EquipmentCertificateTypesProvider).GetNestedTypes(System.Reflection.BindingFlags.NonPublic)[0];
			AssertEquals("Should return EquipmentCertificateTypesValidation if license is valid", validationType, provider.GetDefaultTypesSpecificValidation(cert).GetType());
		}

		public void TestCheckXZ_RefNumber()
		{
			cert.XZ_Type = ConveyanceReferences.Codes.ACEId;
			ValidationTestHelper.AssertRelatedObjectMaxLengthValidation(cert.XZ_RefNumberInfo, 10, ConveyanceReferences.Descriptions.ACEId);
			cert.XZ_Type = ConveyanceReferences.Codes.CarrierId;
			ValidationTestHelper.AssertRelatedObjectMaxLengthValidation(cert.XZ_RefNumberInfo, 23, ConveyanceReferences.Descriptions.CarrierId);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var refEquipment = Factory.New<RefEquipment>();
			cert = refEquipment.Certificates.AddNew();
		}

		GenRegCertAccredMaintList cert;
	}
}
