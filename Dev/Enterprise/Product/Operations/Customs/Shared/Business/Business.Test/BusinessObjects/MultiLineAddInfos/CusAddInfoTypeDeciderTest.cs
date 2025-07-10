using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	sealed class CusAddInfoTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new CusAddInfoTypeDecider();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				var type = typeDecider.GetTypeForLoad(CusAddInfoTypeAttribute.Codes.NZMAFFiles, JobDeclarationSchema.Constants.Prefix, dec.PK, Factory);
				AssertEquals("CusAddInfoTypeDecider should always return type CusAddInfo - never BaseAddInfo", true, type.IsSubclassOf(typeof(CusAddInfo)));
			}

			using (Enterprise.ZArchitecture.Environment.Globals.SetIsUserInteractiveForTest(false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				AssertExceptionThrown<InvalidOperationException>("Not support should throw an error",
					"The 'TES' node should not be nested under the 'JobDeclaration' node. Please ensure that 'TES' is placed at the correct hierarchical level.",
					() => typeDecider.GetTypeForLoad("TES", JobDeclarationSchema.Constants.Prefix, dec.PK, Factory));

				var dispositionCode = Factory.New<Integration.Customs.US.IDispositionData>();
				dispositionCode.B7_ParentID = dec.PK;
				dispositionCode.B7_ParentTableCode = "JE";
				dispositionCode.US_Code = "3U";
				dispositionCode.US_DispositionDate = ZDateTime.Today;
				AssertExceptionThrown<InvalidOperationException>("Not support should throw an error",
					"The 'TES' node should not be nested under the 'UDP' node. Please ensure that 'TES' is placed at the correct hierarchical level.",
					() => typeDecider.GetTypeForLoad("TES", CusAddInfoSchema.Constants.Prefix, dispositionCode.PK, Factory));
			}
		}

		public void TestGetTypeForBinding()
		{
			CusAddInfoTypeDecider typeDecider = new CusAddInfoTypeDecider();
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			CusAddInfoTypeDecider typeDecider = new CusAddInfoTypeDecider();
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetCorrectTypeWarehouseCustomsAdditionalAddInfo()
		{
			var warehouseClient = Factory.New<OrgHeader>();
			warehouseClient.OH_Code = "SD";
			warehouseClient.MainAddress.OA_Address1 = "SDF";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "SD@#";
			part.OP_Desc = "PART DESC";
			part.RelatedOrganisations.AddOwner(warehouseClient);

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whsWarehouse = helper.CreateWarehouse("AD!", warehouseClient.MainAddress, GlbBranch.CurrentBranch);
			var whsReceive = Factory.New<IWhsReceive>();
			whsReceive.WD_WW_Whs = whsWarehouse.PK;
			whsReceive.WD_DocketID = "SD";
			whsReceive.WD_OH_Client = warehouseClient.PK;
			whsReceive.WD_DocketSubType = "CUS";
			var whsReceiveLine = Factory.New<IWhsReceiveLine>();
			whsReceiveLine.WE_WD = whsReceive.PK;
			whsReceiveLine.WE_TransactionQuantity = 10m;
			whsReceiveLine.WE_OP = part.PK;
			var attribute = Factory.New<IWhsBondedWarehouseAttribute>();
			attribute.WB_ParentID = whsReceiveLine.PK;
			attribute.WB_ParentTableCode = ((BusinessObject)whsReceiveLine).TablePrefix;
			attribute.WB_AddInfo = "SD";
			var addInfo = Factory.New<WarehouseCustomsAttributeAddInfo>();
			addInfo.B7_Type = "CCT";
			addInfo.Parent = (BusinessObject)attribute;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertType<WarehouseCustomsAttributeAddInfo>(newFactory.Load<CusAddInfo>(addInfo.PK));
		}

		public void TestDifferentB7_TypesAndDifferentParentsHaveDifferentReportKeys()
		{
			var typeDecider = new CusAddInfoTypeDecider();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				ErrorReporter.Clear();
				var dec = Factory.New<BaseJobDeclaration>();
				var type = typeDecider.GetTypeForLoad(CusAddInfoTypeAttribute.Codes.AUCLR, JobDeclarationSchema.Constants.Prefix, dec.PK, Factory);
				AssertEquals("CusAddInfoTypeDecider.GetTypeForLoad|ParentBO:Enterprise.Customs.US.Business.JobDeclaration|B7_Type:CLR", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				type = typeDecider.GetTypeForLoad(CusAddInfoTypeAttribute.Codes.AUREG, JobDeclarationSchema.Constants.Prefix, dec.PK, Factory);
				AssertEquals("CusAddInfoTypeDecider.GetTypeForLoad|ParentBO:Enterprise.Customs.US.Business.JobDeclaration|B7_Type:REG", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				var invoice = dec.Invoices.AddNew();
				type = typeDecider.GetTypeForLoad(CusAddInfoTypeAttribute.Codes.AUCLR, JobComInvoiceHeaderSchema.Constants.Prefix, invoice.PK, Factory);
				AssertEquals("CusAddInfoTypeDecider.GetTypeForLoad|ParentBO:Enterprise.Customs.US.Business.JobComInvoiceHeader|B7_Type:CLR", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}
	}
}
