using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GenRegCertAccredMaintListLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCertificatesTypesList()
		{
			GenRegCertAccredMaintList certificate = Factory.New<GenRegCertAccredMaintList>();
			AssertType(typeof(CodeDescriptionPairList), certificate.Lookups.CertificateTypes);

			DummyObject dummy = Factory.New<DummyObject>();
			dummy.NullCertificateTypeList = true;

			certificate = Factory.New<GenRegCertAccredMaintList>();
			certificate.MasterParent = dummy;
			AssertType(null, certificate.Lookups.CertificateTypes);

			dummy.NullCertificateTypeList = false;

			certificate = Factory.New<GenRegCertAccredMaintList>();
			certificate.MasterParent = dummy;
			AssertType("Factory value gets cached, so the method is not accessed again", null, certificate.Lookups.CertificateTypes);
		}

		public void TestCertificatesTypesListForDifferentMasterParentsShouldBeDifferent()
		{
			var staffCertificateTypeCollection = new OverrideImmuneCodeDescriptionBoolCollection()
			{
				{ "AKA", (NoResString)"Also Known As", true },
				{ "ASK", (NoResString)"Ask Jeeves", false }
			};
			var equipmentCertificateTypeCollection = new CertificateTypeCollection();
			equipmentCertificateTypeCollection.AddNew().SetupValues("NNA", (NoResString)"Not Known As", false, false, false, AlertTypeList.Codes.NoAlert);

			SystemDataRegistry.Instance.StaffCertificateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staffCertificateTypeCollection);
			ReferenceFilesDataRegistry.Instance.EquipmentCertificateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, equipmentCertificateTypeCollection);

			var staffMember = Factory.NewWithValidTestData<GlbStaff>();
			var equipment = Factory.NewWithValidTestData<RefEquipment>();

			var staffCertificate = Factory.New<GenRegCertAccredMaintList>();
			staffCertificate.MasterParent = staffMember;
			var equipmentCertificate = Factory.New<GenRegCertAccredMaintList>();
			equipmentCertificate.MasterParent = equipment;

			Assert("Should contain AKA since this is an active code in the StaffCertificateTypes registry", staffCertificate.Lookups.CertificateTypes.ContainsCode("AKA"));
			Assert("Should contain AKA since this is an active code in the StaffCertificateTypes registry", staffCertificate.Lookups.CertificateTypes_ActiveList.ContainsCode("AKA"));
			Assert("Should contain ASK since this is an inactive code in the StaffCertificateTypes registry", staffCertificate.Lookups.CertificateTypes.ContainsCode("ASK"));
			Assert("Should not contain ASK since this is an inactive code in the StaffCertificateTypes registry", !staffCertificate.Lookups.CertificateTypes_ActiveList.ContainsCode("ASK"));
			Assert("Should contain NNA since this is a code in the EquipmentCertificateTypes registry", equipmentCertificate.Lookups.CertificateTypes.ContainsCode("NNA"));
			Assert("Should contain NNA since this is a code in the EquipmentCertificateTypes registry", equipmentCertificate.Lookups.CertificateTypes_ActiveList.ContainsCode("NNA"));
		}

		public void TestStatesOrProvinces()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "X7";
			var state1 = Factory.New<RefCountryStates>();
			var state2 = Factory.New<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state2.RW_RN_NKCountryCode = country.RN_Code;

			var cert = Factory.New<GenRegCertAccredMaintList>();
			AssertEquals("StatesOrProvinces should have no elements.", 0, cert.Lookups.StatesOrProvinces.Count);

			cert.XZ_RN_NKCountryOfIssuance = "X7";
			AssertEquals("StatesOrProvinces should have 2 elements.", 2, cert.Lookups.StatesOrProvinces.Count);
		}

		#region Helper Classes

		class DummyObject : DummyEnterpriseBusinessObject, ICertificatesProvider
		{
			public DummyObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool NullCertificateTypeList { get; set; }

			#region ICertificatesProvider Members

			public GenRegCertAccredMaintListCollection Certificates
			{
				get { return null; }
			}

			public ICodeDescriptionPairList GetCertificateTypeList()
			{
				if (NullCertificateTypeList)
				{
					return null;
				}

				return new DummyCodeDescriptionPairList();
			}

			public ICodeDescriptionPairList GetActiveCertificateTypeList()
			{
				return GetCertificateTypeList();
			}

			public ZString GetDefaultDescription(ZString code)
			{
				return ZString.Empty;
			}

			#endregion
		}

		class DummyCodeDescriptionPairList : CodeDescriptionPairList
		{
		}

		#endregion
	}
}
