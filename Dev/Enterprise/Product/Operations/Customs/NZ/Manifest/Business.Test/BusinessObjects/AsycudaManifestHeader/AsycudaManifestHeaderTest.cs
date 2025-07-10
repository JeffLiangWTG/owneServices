using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestDeliveryNotificationParty()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertNotNull("AsycudaManifestHeader.DeliveryNotificationParty", manifestHeader.DeliveryNotificationParty);
		}

		public void TestValidNatureDefaulsOnManifestTypeEntry()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			manifestHeader.AMA_ManifestType = NZManifestTypes.Codes.ICR;
			AssertEquals("Manifest Nature should default to IMP for ICR", "IMP", manifestHeader.AMA_Nature);
			manifestHeader.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			AssertEquals("Manifest Nature should default to EXP for OCR", "EXP", manifestHeader.AMA_Nature);
		}

		public void TestBillShipmentTypeDefaultValueForManifestTypeOCR()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			var bill = manifestHeader.Bills.AddNew();
			manifestHeader.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			AssertEquals("Bill Shipment Type should be EXP by default for OCR manifest type", ShipmentTypeList.Codes.Export22, bill.ABL_ShipmentType);
		}

		public void TestLookups()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<AsycudaManifestHeaderLookups>(manifestHeader.Lookups);
		}

		public void TestIsDeconsolidatorEnabled()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(true, manifestHeader.IsDeconsolidatorEnabled);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.NZManifest.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestBills()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(manifestHeader.Bills);
		}

		public void TestPersons()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<CusPersonCollection<CusPerson, AsycudaManifestHeader>>(manifestHeader.Persons);
		}

		public void TestCreateNewAsycudaContainerCollection()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
			AssertType<AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>>(manifestHeader.CreateNewAsycudaContainerCollection());
		}

		public void TestCreateNewCusPersonCollection()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
			AssertType<CusPersonCollection<CusPerson, AsycudaManifestHeader>>(manifestHeader.CreateNewCusPersonCollection());
		}

		public void TestGetDefaultCountryCode()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
			AssertEquals(Core.Constants.CountryCodes.NewZealand, manifestHeader.GetDefaultCountryCode());
		}

		public void TestGetBillTypeCore()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
			AssertEquals(typeof(AsycudaBill), manifestHeader.GetBillTypeCore());
		}

		public void TestGetContainerTypeCore()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
			AssertEquals(typeof(AsycudaContainer), manifestHeader.GetContainerTypeCore());
		}

		public void TestGetPersonTypeCore()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
			AssertEquals(typeof(CusPerson), manifestHeader.GetPersonTypeCore());
		}

		public void TestINZManifestHeader()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
			Assert("AsycudaManifestHeader should have implemented the interface IManifestHeader", manifestHeader is INZManifestHeader);
			manifestHeader.AMA_JobReference = "C00058927";
			manifestHeader.AMA_MasterBill = "BILL1234567";
			var iNZManifestHeader = (INZManifestHeader)manifestHeader;
			AssertEquals("IManifestHeader.JobName", "AsycudaManifestHeader", iNZManifestHeader.JobName);
			AssertEquals("IManifestHeader.DocumentParentType", "ASM", iNZManifestHeader.DocumentParentType);
			AssertSame("IManifestHeader.Logs", manifestHeader.Logs, iNZManifestHeader.Logs);
			AssertEquals("IManifestHeader.MasterBillNumber", "BILL1234567", iNZManifestHeader.MasterBillNumber);
			AssertEquals("IManifestHeader.JobNumber", "C00058927", iNZManifestHeader.JobNumber);
		}

		public void TestLoadAndCreateRegistrationNumber()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
			Assert("AsycudaManifestHeader should have implemented the interface IManifestHeader", manifestHeader is INZManifestHeader);
			var iNZManifestHeader = (INZManifestHeader)manifestHeader;
			var manifestEntryNo = iNZManifestHeader.LoadAndCreateRegistrationNumber();
			manifestEntryNo.CE_EntryNum = "63289015";
			AssertEquals("Manifest has recorded the OCR Entry No (RegistrationNumber)", "63289015", manifestHeader.RegistrationNumber);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = CreateBusinessObject(factory);
			header.SuspendCheckBusinessObjectType();
			return header;
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);

		AsycudaManifestHeader CreateBusinessObject(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "C1234";
			return manifestHeader;
		}

		sealed class AsycudaManifestHeaderForTest : AsycudaManifestHeader
		{
			public AsycudaManifestHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			internal new IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => base.CreateNewAsycudaContainerCollection();
			internal new CusPersonCollection CreateNewCusPersonCollection() => base.CreateNewCusPersonCollection();
			internal new ZString GetDefaultCountryCode() => base.GetDefaultCountryCode();
			internal new Type GetBillTypeCore() => base.GetBillTypeCore();
			internal new Type GetContainerTypeCore() => base.GetContainerTypeCore();
			internal new Type GetPersonTypeCore() => base.GetPersonTypeCore();
		}
		#endregion
	}
}
