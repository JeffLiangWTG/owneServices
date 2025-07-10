using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	sealed class WarehouseInBondDataEventParentFinderTest : DataTransfer.Universal.Testing.InBondHelperTest
	{
		public void TestFind()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "DKD32432";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_OA_Importer = importer.MainAddress.PK;
			header1.BH_FTZMove = true;
			var moveHeader1 = header1.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "INB3242";
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_OA_Importer = importer.MainAddress.PK;
			header2.BH_FTZMove = true;
			var moveHeader2 = header2.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "INB3242";
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_OA_Importer = importer.MainAddress.PK;
			header3.BH_FTZMove = true;
			var moveHeader3 = header3.MovementHeaders.AddNew();
			var inBondNumberObj3 = CusEntryNumber.New(moveHeader2, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates);
			inBondNumberObj3.CE_EntryNum = "INB3242";
			var inBondNumberObj4 = CusEntryNumber.New(moveHeader2, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.ReconEntry, Core.Constants.CountryCodes.UnitedStates);
			inBondNumberObj4.CE_EntryNum = "INB3242";
			Factory.SaveForTesting();
			var finder = new WarehouseInBondDataEventParentFinder(Factory.BOFactory, new WarehouseInBondDataContextManager(), Logger);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.WarehouseInBond, null);
			var eventXml = new UniversalEvent()
			{
				DataContext = dataContext,
				ContextCollection = new List<Context>(new[] { new Context()
			{ Type = "EntryNumber", Value = "INB3242" }, new Context()
			{ Type = "EntryNumberType", Value = "INB" }, new Context()
			{ Type = "EntryNumberCountryOfIssue", Value = "US" } })
			};
			var bizObjs = finder.GetLogParentsForEvent(eventXml);
			AssertEquals(2, bizObjs.Length);
			AssertCollectionContains(moveHeader1, bizObjs);
			AssertCollectionContains(moveHeader2, bizObjs);
		}
	}
}
