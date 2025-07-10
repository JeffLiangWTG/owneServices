using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.SG.Access.Business.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(header.Bills);
		}

		public void TestMarkBillsApportionmentDirty()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ApportionmentDirty = false;
			header.AMA_ManifestType = "XXX";
			AssertEquals("Should be true as the ManifestType is changed.", true, bill.ApportionmentDirty);
		}

		public void TestValuationDate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(ZDate.Today, header.ValuationDate);
		}

		public void TestIsDeclarationCreationEnabled()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("SGAccess allows Declaration Creation", header.IsDeclarationCreationEnabled);
		}

		public void TestLockedBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("SGAccess bills are locked", header.LockedBills);
		}

		public void TestShowPackedItems()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("SGAccess has PackedItems", header.ShowPackedItems);
		}

		public void TestCustomsSystem()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("SGCustoms", header.CustomsSystem);
		}

		public void TestGetExtraMessageSendingNotification()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var company1 = Factory.New<GlbCompany>();
				company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
				company1.GC_Code = "DSG";
				var proxy1 = Factory.New<OrgHeader>();
				proxy1.OH_Code = "Proxy1";
				proxy1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "12345", Core.Constants.CountryCodes.Singapore);
				var branch1 = Factory.New<GlbBranch>();
				branch1.GB_GC = company1.PK;
				branch1.GB_Code = "SGC";
				branch1.GB_BranchName = "Singapore Corporate Office";
				branch1.GB_OH_OrgProxy = proxy1.PK;
				Factory.Save();
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_CustomsOffice = "office";
				header.AMA_ManifestType = "ASY";
				AssertEquals(ValidationConstants.AccessCredentialsNotSetUp, header.MessageSendingNotificationHelper.GetNotifications());
				var sgStaffWrapper = SGManifestTestHelper.SetUpSGAccessTestUser(Factory);
				sgStaffWrapper.AccessPassword.GP_PasswordStatus = "IID";
				Factory.Save();
				AssertEquals(ValidationConstants.InvalidSGAccessCredentials("IID"), header.MessageSendingNotificationHelper.GetNotifications());
				sgStaffWrapper.AccessPassword.GP_PasswordStatus = Core.Constants.PasswordOK;
				Factory.Save();
				AssertEquals(ZString.Empty, header.MessageSendingNotificationHelper.GetNotifications());
			}
		}

		public void TestTestMarkAsNeedingValidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("IsValid", false, header.LightValidationIsValid);
			AssertEquals("Should validate on save", true, header.ShouldValidateOnSave);
			bill.MarkLightValidationAsValidForTesting();
			Assert(bill.LightValidationIsValid);
		}

		public void TestIsImport()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Business.Constants.ManifestType.Export;
			AssertEquals(false, header.IsImport);
			header.AMA_ManifestType = Business.Constants.ManifestType.Import;
			AssertEquals(true, header.IsImport);
		}

		public void TestNatureIsDefaultedBasedOnManifestType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			AssertEquals("AMA_Nature should default", "IMP", header.AMA_Nature);
			header.AMA_ManifestType = Constants.ManifestType.Export;
			AssertEquals("AMA_Nature should default", "EXP", header.AMA_Nature);
		}

		public void TestDefaultCustomsDischargePort()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "SGSIN";
			AssertEquals("SGSIN", header.AMA_CustomsDischargePort);
			SetupPortWithMultipleRefLocoMaps("SGYYY", "SGAYC", "SGPAP");
			header.AMA_RL_NKPortOfDischarge = "SGYYY";
			AssertEquals("SGAYC", header.AMA_CustomsDischargePort);
		}

		public void TestDefaultCustomsLoadPort()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("AUSYD", header.AMA_CustomsLoadPort);
			SetupPortWithMultipleRefLocoMaps("AUZZZ", "AUMEL", "AUSYD");
			header.AMA_RL_NKPortOfLoading = "AUZZZ";
			AssertEquals("AUMEL", header.AMA_CustomsLoadPort);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestGetNewMessageChooser()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(typeof(MessageChooser), header.GetNewMessageChooser(new[] { bill }, string.Empty, false).GetType());
		}

		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(typeof(AsycudaManifestHeaderValidation), header.Validation.GetType());
		}

		public void TestIControllerIDProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var provider = header as IControllerIDProvider;

			CombineAssertions(() =>
			{
				AssertEquals("Expected ControllerID", ControllerIDs.Customs.ASYCUDA.SGAccess.Manifest, provider.ControllerID);
				AssertEquals("Expected PK", header.PK.ToGuid(), provider.BusinessObjectPK);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		void SetupPortWithMultipleRefLocoMaps(ZString portCode, ZString localPortCode1, ZString localPortCode2)
		{
			var port = Factory.New<RefUNLOCO>();
			port.RL_Code = portCode;
			var locoMap1 = port.RefLocoMaps.AddNew();
			locoMap1.RY_RN = Core.Constants.CountryGuids.Singapore;
			locoMap1.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			locoMap1.RY_LocalPortCode = localPortCode1;
			var locoMap2 = port.RefLocoMaps.AddNew();
			locoMap2.RY_RN = Core.Constants.CountryGuids.Singapore;
			locoMap2.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			locoMap2.RY_LocalPortCode = localPortCode2;
		}

		[TestDate(2022, 7, 1, 0, 0, 0)]
		public void TestIsOVRApplicable()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Business.Constants.ManifestType.Import;
			using (SGAccessRegistry.Instance.OVRLiveEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2022, 1, 1)))
			{
				AssertEquals(true, header.IsOVRApplicable);
			}

			using (SGAccessRegistry.Instance.OVRLiveEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				AssertEquals(false, header.IsOVRApplicable);
			}
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}
