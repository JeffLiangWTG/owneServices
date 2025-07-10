using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.DataTransfer.Universal;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Registry.Business.eServices;
	using Enterprise.UniversalDataBuss.Core.Testing;
	using Enterprise.UniversalDataBuss.DataObjects;
	using Enterprise.UniversalDataBuss.DataObjects.Universal;
	using Enterprise.UniversalDataBuss.Integration;

	class UniversalDataObjectReaderHelperTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetCustomsUnitForPackType()
		{
			var helper = new Universal.UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.NewZealand);
			AssertNull(helper.GetCustomsUnitForPackType(null));
			var packageType = new PackageType();
			AssertNull(helper.GetCustomsUnitForPackType(packageType));
			packageType.Code = "Z@";
			AssertEquals("Z@", helper.GetCustomsUnitForPackType(packageType));
			packageType.Code = Core.Constants.PkgUnit.Skid;
			AssertEquals("SI", helper.GetCustomsUnitForPackType(packageType));

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "BJ001232";
			declaration.JE_MasterBill = "MB1";
			var dataObject = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			dataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, "BJ001232");
			dataObject.OuterPacks = 10;
			dataObject.OuterPacksPackageType = packageType;
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB1",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				PackType = packageType,
				PackQty = 4
			};
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB1",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				PackType = new PackageType() { Code = Core.Constants.PkgUnit.Tube },
				PackQty = 6
			};
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLine1, packingLine2 }));
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var logger = new TestErrorLogger();
			ITopLevelDataObjectReader reader = new JobDeclarationDataObjectReader(dataObject, logger, Factory);
			BusinessObject targetBO = declaration;
			reader.ReadIntoBusinessObject(ref targetBO);
			AssertEquals("declaration.JE_TotalNoOfPacks", 10, declaration.JE_TotalNoOfPacks);
			AssertEquals("declaration.JE_TotalNoOfPacksPackType", "SI", declaration.JE_TotalNoOfPacksPackType);
			declaration.Packages.Load();
			AssertEquals(2, declaration.Packages.Count);
			var package1 = declaration.Packages[0];
			var package2 = declaration.Packages[1];
			if (package2.CW_PackType == "SI")
			{
				package1 = declaration.Packages[1];
				package2 = declaration.Packages[0];
			}
			AssertEquals("package1.CW_PackQty", 4, package1.CW_PackQty);
			AssertEquals("package1.CW_PackType", "SI", package1.CW_PackType);
			AssertEquals("package2.CW_PackQty", 6, package2.CW_PackQty);
			AssertEquals("package2.CW_PackType", "TU", package2.CW_PackType);
		}

		public void TestGetFreightUnitForPackType()
		{
			var readerHelper = new Universal.UniversalDataObjectReaderHelper(Factory, "AU");

			AssertEquals("PKG", readerHelper.GetFreightUnitForPackType("PK"));
			AssertEquals("XYZ", readerHelper.GetFreightUnitForPackType("XYZ"));
			AssertEquals(null, readerHelper.GetFreightUnitForPackType(null));
		}
	}

	public class DeclarationDataObjectReaderTest : DataTransfer.Universal.Testing.DataObjectReaderTest
	{
		public void TestCanImportDataWhenNoCommercialInvoiceDetails()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_EDITransmitDate = new ZDateTime(2015, 11, 01);
			declaration.JE_DeclarationReference = "BJ001232";
			declaration.JE_MasterBill = "MB2343";
			declaration.JE_GoodsDescription = "ABCD";
			AssertNotNull(declaration.CusEntryHeader);
			declaration.CusEntryHeader.EntryNumber = "1942482";
			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.GoodsDescription = "TEST NZ";
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			reader.ReadIntoBusinessObject();

			var declaration2 = Factory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("The Job updated", declaration2.JE_GoodsDescription, "TEST NZ");
			Assert(!declaration2.IsMergeDone);
		}
	}
}
