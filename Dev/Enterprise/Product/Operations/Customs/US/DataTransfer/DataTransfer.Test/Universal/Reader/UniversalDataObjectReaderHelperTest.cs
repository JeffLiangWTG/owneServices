using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class UniversalDataObjectReaderHelperTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetCustomsUnitForPackType()
		{
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates);
			var mappings = USCustomsDataRegistry.Instance.USPackageTypesMapping.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			mappings.RemoveAndDeleteAll();
			USCustomsDataRegistry.Instance.USPackageTypesMapping.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, mappings);
			AssertNull(helper.GetCustomsUnitForPackType(null));
			var packType = new PackageType();
			AssertNull(helper.GetCustomsUnitForPackType(packType));
			packType.Code = Core.Constants.PkgUnit.Bag;
			AssertEquals(Core.Constants.PkgUnit.Bag, helper.GetCustomsUnitForPackType(packType));
			mappings.AddNew("BG", Core.Constants.PkgUnit.Bag);
			mappings.AddNew("BC", Core.Constants.PkgUnit.Bottle);
			USCustomsDataRegistry.Instance.USPackageTypesMapping.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, mappings);
			AssertEquals("BG", helper.GetCustomsUnitForPackType(packType));
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "BJ001232";
			declaration.JE_MasterBill = "MB1";
			var dataObject = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			dataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, "BJ001232");
			dataObject.TotalNoOfPacks = 10;
			dataObject.TotalNoOfPacksPackageType = packType;
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB1",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				PackType = packType,
				PackQty = 4
			};
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB1",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				PackType = new PackageType() { Code = Core.Constants.PkgUnit.Bottle },
				PackQty = 6
			};
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLine1, packingLine2 }));
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;
			var logger = new TestErrorLogger();
			ITopLevelDataObjectReader reader = new JobDeclarationDataObjectReader(dataObject, logger, Factory);
			BusinessObject targetBO = declaration;
			reader.ReadIntoBusinessObject(ref targetBO);
			AssertEquals("declaration.JE_TotalNoOfPacks", 10, declaration.JE_TotalNoOfPacks);
			AssertEquals("declaration.JE_TotalNoOfPacksPackType", "BG", declaration.JE_TotalNoOfPacksPackType);
			declaration.Packages.Load();
			AssertEquals(2, declaration.Packages.Count);
			var package1 = declaration.Packages[0];
			var package2 = declaration.Packages[1];
			if (package2.CW_PackType == Core.Constants.PkgUnit.Bag)
			{
				package1 = declaration.Packages[1];
				package2 = declaration.Packages[0];
			}

			AssertEquals("package1.CW_PackQty", 4, package1.CW_PackQty);
			AssertEquals("package1.CW_PackType", "BG", package1.CW_PackType);
			AssertEquals("package2.CW_PackQty", 6, package2.CW_PackQty);
			AssertEquals("package2.CW_PackType", "BC", package2.CW_PackType);
		}

		public void TestGetFreightUnitForPackType()
		{
			var refPack = Factory.New<BaseRefPacks>();
			refPack.RP_Type = RPTypeList.Codes.PackingDeclaration;
			refPack.RP_CustomsCountry = Core.Constants.CountryCodes.Australia;
			refPack.RP_CustomsPack = "XST";
			refPack.RP_CommercialPack = "CTN";

			var readerHelper = new UniversalDataObjectReaderHelper(Factory, "AU");
			AssertEquals("PKG", readerHelper.GetFreightUnitForPackType("PK"));
			AssertEquals("XYZ", readerHelper.GetFreightUnitForPackType("XYZ"));
			AssertEquals(null, readerHelper.GetFreightUnitForPackType(null));
			AssertEquals("CTN", readerHelper.GetFreightUnitForPackType("XST"));
		}

		public void TestGetSupportedAddInfoList()
		{
			var currentCompanyOrg = Factory.New<OrgHeader>();
			currentCompanyOrg.OH_Code = "~OC";
			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			currentCompany.GC_OH_OrgProxy = currentCompanyOrg.PK;
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = currentCompany.PK;
			Factory.SaveForTesting();
			using (DisposableEnvironment.ForBranch(currentBranch.PK.ToGuid()))
			{
				var australia = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Australia);
				UniversalDataObjectReaderHelperAbstractTest.CreateOrgPatternMatchOverrideInSource(Factory.BOFactory, Env.CurrentCompany.OrganisationPK, "$A1", Core.Constants.OrgPatternMatchOverrideRelationships.Country, australia.PK, "");
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.NewZealand;
				invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Vanuatu;
				invoice.US_StateOfOrigin = "DG";
				Factory.SaveForTesting();
				Factory.BOFactory.ClearQueryCache();
				var helper = new UniversalDataObjectReaderHelper(Factory, "AU");
				var list1 = helper.GetSupportedAddInfoList<JobComInvoiceHeader>(USAddInfoSchema.Instance);
				var list2 = helper.GetSupportedAddInfoList<JobComInvoiceHeader>(USAddInfoSchema.Instance);
				AssertEquals(true, Object.ReferenceEquals(list1, list2));
				foreach (var addInfoField in new[] { USAddInfoSchema.US_UC_NKCountryOfExport, USAddInfoSchema.US_UC_NKCountryOfOrigin })
				{
					AddInfoPropertyNameAndValueParser data;
					var name = addInfoField.Name.Substring(3);
					AssertEquals(true, list1.TryGetValue(name, out data));
					AssertNotNull(data);
					var fuction = data.Parser;
					var logger = new TestErrorLogger();
					var value = "H".PadRight(addInfoField.MaxLength + 1, '1');
					AssertEquals("Data should be truncated", "H".PadRight(addInfoField.MaxLength, '1'), fuction(logger, value));
					AssertMultilineASCIIEquals("logger.Logs", "Warning - " + BaseAddInfo.GetMaximumLengthTruncateMessage(name, addInfoField.MaxLength, value), logger.Logs);
					logger.ClearLogs();
					value = "H".PadRight(addInfoField.MaxLength, '1');
					AssertEquals(value, fuction(logger, value));
					AssertMultilineASCIIEquals("logger.Logs", "", logger.Logs);
					logger.ClearLogs();
					value = "$A1";
					AssertEquals("AU", fuction(logger, value));
					AssertMultilineASCIIEquals("logger.Logs", @"
Information - Used code mapping defined in Organization(Code: ~OC) > Config > EDI Code Mapping.
Information - Mapped Country/Region code '$A1' to 'AU'.
				".Trim(), logger.Logs);
				}
			}
		}
	}
}
