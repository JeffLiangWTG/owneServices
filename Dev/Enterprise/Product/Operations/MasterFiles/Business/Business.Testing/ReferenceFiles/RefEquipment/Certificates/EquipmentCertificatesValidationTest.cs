using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EquipmentCertificatesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestXZ_Type()
		{
			var registryItem = ReferenceFilesDataRegistry.Instance.EquipmentCertificateTypes;
			var registryPath = ((IRegistryItemInternals)registryItem).Location;

			var expectedUnknown = string.Format("Unknown type 'AAA'. Check the Certificate Type in the following Registry: {0}", registryPath);
			var expectedUnique = string.Format("Equipment Certificate Type 'UNI (Unique type)' must be unique according to the Registry: {0}", registryPath);
			var expectedMandatory = string.Format("Equipment Certificate Type 'MAN (Mandatory type)' is Mandatory according to the registry and thus must have its Reference Number filled out: {0}", registryPath);

			var certificate = Equipment.Certificates.AddNew();
			certificate.XZ_Type = "UNI";
			AssertNoNotifications("Should be NO Errors", certificate.XZ_TypeInfo);

			certificate = Equipment.Certificates.AddNew();

			certificate.XZ_Type = "AAA";
			AssertHasError("Should be Error", certificate.XZ_TypeInfo, expectedUnknown);

			certificate.XZ_Type = "UNI";
			AssertHasError("Should be Error", certificate.XZ_TypeInfo, expectedUnique);

			certificate.XZ_Type = EquipmentCertificateTypeList.Codes.Service;
			AssertNoNotifications("Should be NO Errors", certificate.XZ_TypeInfo);

			certificate.XZ_Type = "MAN";
			AssertHasError("Should be Error", certificate.XZ_TypeInfo, expectedMandatory);
			certificate.XZ_RefNumber = "01234";
			AssertNoNotifications("Should be NO Errors", certificate.XZ_TypeInfo);
			certificate.XZ_RefNumber = "";
			AssertHasError("Should be Error", certificate.XZ_TypeInfo, expectedMandatory);
		}

		#region Implementation

		void SetupRegistry()
		{
			var collection = ReferenceFilesDataRegistry.Instance.EquipmentCertificateTypes.Value;
			var type = collection.AddNew();
			type.SetupValues("UNI", "Unique type", false, true, false, AlertTypeList.Codes.NoAlert);
			var type2 = collection.AddNew();
			type2.SetupValues("MAN", "Mandatory type", true, false, false, AlertTypeList.Codes.NoAlert);

			ReferenceFilesDataRegistry.Instance.EquipmentCertificateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		RefEquipment Equipment;

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();

			SetupRegistry();
			Equipment = Factory.NewWithValidTestData<RefEquipment>();
		}

		#endregion

		#endregion
	}
}
