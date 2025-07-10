using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.DIS.Testing
{
	sealed class JobDeclarationWrapperTest : TestCaseWithFactory
	{
		public void TestDetails()
		{
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_DateOfArrival = new ZDateTime(2013, 3, 1);
			declaration.US_SchDArrival = "3901";
			declaration.US_SchDEntry = "3910";

			var ior = Factory.New<OrgHeader>();
			ior.OH_FullName = "Buy All Co.";
			ior.CustomsCodes.AddNew("EIN", "34-342892300");
			declaration.IOROrgPK = ior.PK;

			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "XJ6";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);

			var wrapper = new JobDeclarationWrapper(declaration);
			AssertEquals("XJ6", wrapper.PreparerID);
			AssertEquals(new ZDateTime(2013, 3, 1), wrapper.ArrivalDate);
			AssertEquals("3901", wrapper.PortOfUnlading);
			AssertEquals("3910", wrapper.PortOfEntry);
			AssertEquals("34-342892300", wrapper.ImporterOfRecordID);
			AssertEquals(MasterFiles.Business.DIS.TransactionCategory.SingleTransaction, wrapper.TransactionCategory);
			AssertEquals("8888", wrapper.PreparerSiteCode);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			wrapper = new JobDeclarationWrapper(declaration);
			AssertEquals("XJ6", wrapper.PreparerID);

			Assert(!wrapper.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(wrapper.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			AssertEquals(0, wrapper.DefaultBondData.Count());
			AssertEquals(0, wrapper.DefaultInvoiceData.Count());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals(0, wrapper.DefaultInvoiceData.Count());
		}

		public void TestPreparerSiteCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1001", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2786", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2720", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;

			var wrapper = new JobDeclarationWrapper(declaration);
			AssertEquals(ZString.Empty, wrapper.PreparerSiteCode);

			declaration.US_SchDEntry = "2786";
			AssertEquals("2786", wrapper.PreparerSiteCode);

			var mappingCollection = new EntryProcessingPortsMappingCollection();
			var mappingItem = mappingCollection.AddNew();
			mappingItem.EntryPort = "2786";
			mappingItem.ProcessingPort = "2720";
			declaration.US_EntryMode = ZString.Empty;
			DataRegistry.Business.USCustomsDataRegistry.Instance.EntryProcessingPortMappings.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, mappingCollection);
			DataRegistry.Business.USCustomsDataRegistry.Instance.StatementProcessingPortEqualPortofEntryNonRLF.SetValue(Guid.Empty, declaration.Branch.GB_GC.ToGuid(), Guid.Empty, true);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, declaration.Branch.GB_GC.ToGuid(), Guid.Empty, "2704");
			AssertEquals("2720", wrapper.PreparerSiteCode);

			mappingCollection.RemoveAndDeleteAll();
			DataRegistry.Business.USCustomsDataRegistry.Instance.EntryProcessingPortMappings.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, mappingCollection);
			AssertEquals("2786", wrapper.PreparerSiteCode);

			DataRegistry.Business.USCustomsDataRegistry.Instance.StatementProcessingPortEqualPortofEntryNonRLF.SetValue(Guid.Empty, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, false);
			AssertEquals("2704", wrapper.PreparerSiteCode);
		}
	}
}
