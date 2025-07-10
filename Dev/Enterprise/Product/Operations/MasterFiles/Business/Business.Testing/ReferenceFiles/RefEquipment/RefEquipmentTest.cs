using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefEquipment))]
	sealed class RefEquipmentTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		public void TestRQ_RN_NKRegoCountry()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "KNZTest";
			organisation.OH_RL_NKClosestPort = "AUSYD";

			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.OH_Code = "ABCTest";
			organisation2.OH_RL_NKClosestPort = "CHSAH";

			Factory.Save();

			BO.RQ_OH_Owner = organisation.PK;
			AssertEquals("Should set default value", "AU", BO.RQ_RN_NKRegistrationCountry);
			AssertNotNull(BO.Lookups.RefCountryStatesList);

			BO.RQ_OH_Owner = organisation2.PK;
			AssertEquals("If it has value don't set default value", "AU", BO.RQ_RN_NKRegistrationCountry);
		}

		public void TestRQ_GS_NKPreferredDriverList()
		{
			AssertEquals(typeof(StaffDriverCollection), Factory.New<RefEquipment>().RQ_GS_NKPreferredDriverList.GetType());
		}

		public void TestPopulateDescriptionFromRoadContainerType()
		{
			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "ABCD";
			container.RC_Description = "Zubin Truck";

			var container2 = Factory.NewWithValidTestData<RefContainer>();
			container2.RC_Code = "DEFG";
			container2.RC_Description = "Rakhsh Truck";

			Factory.Save();

			BO.RQ_Description = "";
			BO.RQ_RC_RoadContainerType = container.PK;
			AssertEquals("Description should be populated with equipment type", "Zubin Truck", BO.RQ_Description);

			BO.RQ_RC_RoadContainerType = container2.PK;
			AssertEquals("Description should be populated with new equipment type", "Rakhsh Truck", BO.RQ_Description);

			BO.RQ_Description = "xxx";

			BO.RQ_RC_RoadContainerType = ZGuid.Empty;
			BO.RQ_RC_RoadContainerType = container2.PK;
			AssertEquals("Description changed, shouldn't populate now", "xxx", BO.RQ_Description);
		}

		public void TestPopulateCertificateDescriptionFromTypeIfCertificateIsUnique()
		{
			var cert = BO.Certificates.AddNew();
			cert.XZ_Type = EquipmentCertificateTypeList.Codes.Service;
			AssertEquals("Description should be populated from certificate type", "Maintenance Service", cert.XZ_Comment);

			cert.XZ_Type = "ACE";
			AssertEquals("Description should be populated from certificate type", "ACE ID", cert.XZ_Comment);

			cert.XZ_Type = "CID";
			AssertEquals("Description should be populated from new certificate type", "Carrier ID of conveyance", cert.XZ_Comment);

			cert.XZ_Comment = "xxx";
			cert.XZ_Type = "ACI";
			AssertEquals("Description changed, shouldn't populate now", "xxx", cert.XZ_Comment);
		}

		public void TestPopulateRQ_RegistrationFromRQ_ShortCode()
		{
			BO.RQ_ShortCode = "x";
			AssertEquals("Registration should be populated with short code", "x", BO.RQ_Registration);

			BO.RQ_Registration = "splat";
			BO.RQ_ShortCode = "a";
			AssertEquals("Registration not empty, shouldn't populate now", "splat", BO.RQ_Registration);
		}

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(BO.GetType()));
		}

		public void TestHasTelematics()
		{
			AssertEquals("Precondition: No Divot found Has Telematics should be false.", false, BO.HasTelematics);

			var devicePivot = (IDeviceAssignmentDivot)Factory.NewWithValidTestData(ObjectFactory.GetType<IDeviceAssignmentDivot>());
			devicePivot.V7_ParentID = BO.PK;
			devicePivot.V7_ParentTableCode = BO.TablePrefix;
			Factory.Save();

			AssertEquals("Divot found Has Telematics should be true.", true, BO.HasTelematics);
		}

		#endregion

		public void TestCertificates()
		{
			AssertType(typeof(GenRegCertAccredMaintListCollection), BO.Certificates);
		}

		public void TestDelete_ClearsWhsRFRegistryForeignKeys()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whsSyd = (IWhsWarehouse)helper.CreateWarehouse("SYD", "S");

			var refEq = Factory.New<RefEquipment>();
			refEq.RQ_ShortCode = "Ref";

			var rfReg1 = Factory.New<IWhsRFRegistry>();
			rfReg1.WRR_WW_Whs = whsSyd.PK;
			rfReg1.WRR_RQ_LastUsedEquipment = refEq.PK;
			rfReg1.WRR_GS_NKAssignedTo = "E";
			rfReg1.WRR_PickGroupSequence = 1;
			rfReg1.WRR_PickMethodCode = "ANY";

			var rfReg2 = Factory.New<IWhsRFRegistry>();
			rfReg2.WRR_WW_Whs = whsSyd.PK;
			rfReg2.WRR_RQ_LastUsedEquipment = refEq.PK;
			rfReg2.WRR_GS_NKAssignedTo = "D";
			rfReg2.WRR_PickGroupSequence = 2;
			rfReg2.WRR_PickMethodCode = "ANY";

			Factory.Save();

			AssertEquals("Precondition: FK exists", true, rfReg1.WRR_RQ_LastUsedEquipment.IsValid);
			AssertEquals("Precondition: FK exists", true, rfReg2.WRR_RQ_LastUsedEquipment.IsValid);

			refEq.Delete();

			AssertEquals("Equipment deleted gone", true, refEq.IsDeleted);
			AssertEquals("FK1 gone", ZGuid.Empty, rfReg1.WRR_RQ_LastUsedEquipment);
			AssertEquals("FK2 gone", ZGuid.Empty, rfReg2.WRR_RQ_LastUsedEquipment);
		}

		#region ICertificatesProvider Members

		public void TestCertificateTypeList()
		{
			AssertEquals("SER Code", true, ((ICertificatesProvider)BO).GetCertificateTypeList().ContainsCode(EquipmentCertificateTypeList.Codes.Service));
			AssertEquals("SER Description", EquipmentCertificateTypeList.Descriptions.Service, ((ICertificatesProvider)BO).GetCertificateTypeList().GetDescriptionFromCode(EquipmentCertificateTypeList.Codes.Service));
		}

		public void TestDefaultDescriptionProvider()
		{
			AssertEquals("Unique description", "Carrier ID of conveyance", ((ICertificatesProvider)BO).GetDefaultDescription("CID"));
			AssertEquals("Non-unique description", EquipmentCertificateTypeList.Descriptions.Service, ((ICertificatesProvider)BO).GetDefaultDescription("SER"));
		}

		#endregion

		#region IRefEquipment Members

		public void TestIRefEquipmentMembers()
		{
			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_EquipmentGroup = "ABC";

			var iRefEquipment = (Integration.IRefEquipment)equipment;
			AssertEquals(nameof(Integration.IRefEquipment.PK), equipment.PK, iRefEquipment.PK);
			AssertEquals(nameof(Integration.IRefEquipment.RQ_EquipmentGroup), equipment.RQ_EquipmentGroup, iRefEquipment.RQ_EquipmentGroup);
		}

		#endregion

		#region Implementation

		RefEquipment BO;

		protected override void SetUp()
		{
			base.SetUp();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BO = factory.New<RefEquipment>();
		}

		#endregion
	}
}
