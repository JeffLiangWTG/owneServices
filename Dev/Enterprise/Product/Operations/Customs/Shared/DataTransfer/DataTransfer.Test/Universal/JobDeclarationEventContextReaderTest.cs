using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class JobDeclarationEventContextReaderTest : TestCaseWithFactory
	{
		public void TestContextValueIncludeEntryInfo()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "entry1";
			entry1.CH_MessageType = "IMP";
			entry1.EntryNumber = "111";

			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "entry2";
			entry2.CH_MessageType = "EXP";
			entry2.EntryNumber = "222";

			var reader = new JobDeclarationEventContextReader(declaration);
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();
			reader.AddJobDeclarationContextValues(contextValues, entry1);

			CombineAssertions(() =>
			{
				AssertEquals("EntryReference", "entry1", contextValues.First(p => p.Key.Type == nameof(UniversalEvent.ContextTypes.EntryReference)).Value.ToString());
				AssertEquals("EntryType", "IMP", contextValues.First(p => p.Key.Type == nameof(UniversalEvent.ContextTypes.EntryType)).Value.ToString());
				AssertEquals("EntryNumber", "111", contextValues.First(p => p.Key.Type == nameof(UniversalEvent.ContextTypes.EntryNumber)).Value.ToString());
				AssertEquals("EntryCountry", GlbCompany.CurrentCompany.Country.Code, contextValues.First(p => p.Key.Type == nameof(UniversalEvent.ContextTypes.EntryCountry)).Value.ToString());
			});
		}

		public void TestContextValueIncludeEntryInfo_EntryCountry()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GC = ZGuid.Empty;
			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			var entry2 = Factory.New<CusEntryHeader>();

			var reader = new JobDeclarationEventContextReader(declaration);
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();
			reader.AddJobDeclarationContextValues(contextValues, entry1);

			CombineAssertions(() =>
			{
				AssertEquals("No EntryCountry when it is empty", 0, contextValues.Where(p => p.Key.Type == nameof(UniversalEvent.ContextTypes.EntryCountry)).Count());

				reader.AddJobDeclarationContextValues(contextValues, entry2);
				AssertEquals("EntryCountry: current country of entry", GlbCompany.CurrentCompany.Country.Code, contextValues.First(p => p.Key.Type == nameof(UniversalEvent.ContextTypes.EntryCountry)).Value.ToString());
			});
		}

		public void TestProcessingDataObjectShouldBeCalledOnceForEachReader()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB100031";
			declaration.JE_HouseBill = "HB1000460";
			declaration.JE_RL_NKFinalDestination = "CATOR";
			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "CCCB", "US");
			declaration.JE_OH_ShippingLine = carrier.PK;
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "C1CC", "US");
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var reader = new JobDeclarationEventContextReader(declaration);
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();
			reader.AddJobDeclarationContextValues(contextValues, null);
			AssertEquals("CCCB", contextValues.First(p => p.Key.Type == nameof(UniversalEvent.ContextTypes.CarrierCode)).Value);
			AssertEquals("C1CC", contextValues.First(p => p.Key.Type == nameof(UniversalEvent.ContextTypes.CarrierC1CCode)).Value);
		}
	}
}
