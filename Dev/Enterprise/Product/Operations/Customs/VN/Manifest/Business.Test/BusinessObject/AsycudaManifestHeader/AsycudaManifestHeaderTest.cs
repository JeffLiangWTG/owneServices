using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.VN.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestGetBillType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("Default Bill Type", typeof(AsycudaBill), header.GetBillType());
		}

		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(header.Bills);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObjectForDeleteTest(Factory);
			Factory.Save();
			AssertType<AsycudaManifestHeader>("Load using Integration.Customs.ASYCUDA.VNManifest.IAsycudaManifestHeader", new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.VNManifest.IAsycudaManifestHeader>(bizObj.PK));
			AssertType<AsycudaManifestHeader>("Load using AsycudaManifestHeader", new BusinessObjectFactory().Load<AsycudaManifestHeader>(bizObj.PK));
			AssertType<AsycudaManifestHeader>("Load using ASYCUDA.Business.AsycudaManifestHeader", new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK));
			AssertType<AsycudaManifestHeader>("Load using ManifestBase.AsycudaManifestHeader", new BusinessObjectFactory().Load<ManifestBase.AsycudaManifestHeader>(bizObj.PK));
		}

		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaManifestHeaderValidation>(header.Validation);
		}

		public void TestDefaultValues()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(VNManifestTypes.Codes.VSW, header.AMA_ManifestType);
			AssertEquals(Core.Constants.CountryCodes.VietNam, header.AMA_RN_NKCountry);
			AssertEquals(MessageStatusCodeList.Codes.NotSent, header.AMA_MessageStatus);
		}

		public void TestAMA_MessageStatus_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("VN AsycudaManifestHeader.AMA_MessageStatus should NOT be read-only", !header.AMA_MessageStatusInfo.ReadOnly);
		}

		public void TestRegistrationDetails_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("VN AsycudaManifestHeader.RegistrationDate should NOT be read-only", !header.RegistrationDateInfo.ReadOnly);
			Assert("VN AsycudaManifestHeader.RegistrationNumber should NOT be read-only", !header.RegistrationNumberInfo.ReadOnly);
			Assert("VN AsycudaManifestHeader.RegistrationYear should NOT be read-only", !header.RegistrationYearInfo.ReadOnly);
			Assert("VN AsycudaManifestHeader.RegistrationStatus should NOT be read-only", !header.RegistrationStatusInfo.ReadOnly);
		}

		public void TestRegistrationNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("RegistrationNumber Caption should be Document Number", "Document Number",
				DataBoundResourceStrings.GetDataForProperty(header.RegistrationNumberInfo).Caption);
			AssertEquals("RegistrationNumber ShortCaption should be Document No", "Document No",
				DataBoundResourceStrings.GetDataForProperty(header.RegistrationNumberInfo).ShortCaption);
		}

		public void TestRegistrationYear()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("RegistrationYear Caption should be Document Year", "Document Year",
				DataBoundResourceStrings.GetDataForProperty(header.RegistrationYearInfo).Caption);
			AssertEquals("RegistrationYear ShortCaption should be Year", "Year",
				DataBoundResourceStrings.GetDataForProperty(header.RegistrationYearInfo).ShortCaption);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header;
		}
	}
}
