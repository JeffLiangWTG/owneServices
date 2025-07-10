using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.NZManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestHeader()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection<AsycudaPack, AsycudaBill>>(bill.Packs);
		}

		public void TestGetCountryCode()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(Core.Constants.CountryCodes.NewZealand, bill.GetCountryCode());
		}

		public void TestCreateNewAsycudaPackCollection()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertType<AsycudaPackCollection<AsycudaPack, AsycudaBill>>(bill.CreateNewAsycudaPackCollection());
		}

		public void TestGetPackTypeCore()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(typeof(AsycudaPack), bill.GetPackTypeCore());
		}

		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var child = header.MasterBill;
			var realBill = header.Bills.AddNew();
			AssertType<AsycudaBillValidationForRegularBill>(realBill.Validation);
			AssertType<AsycudaBillValidationForMasterChild>(child.Validation);
		}

		public void TestShipmentTypeDefaultValue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			var bill = header.Bills.AddNew();
			AssertEquals("Bill Shipment Type should be EXP by default for OCR manifest type", ShipmentTypeList.Codes.Export22, bill.ABL_ShipmentType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		sealed class AsycudaBillForTest : AsycudaBill
		{
			public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new ZString GetCountryCode() => base.GetCountryCode();
			public new ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => base.CreateNewAsycudaPackCollection();
			public new Type GetPackTypeCore() => base.GetPackTypeCore();
		}
	}
}
