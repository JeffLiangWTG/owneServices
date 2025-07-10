using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	abstract class JobDeclarationFormAbstractTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		protected override IEnumerable<string> GetMessageSubTypesForFormBashingTest(CodeDescriptionPairList messageSubTypeList)
		{
			if (messageSubTypeList.Count > 0)
			{
				yield return messageSubTypeList[0].Code;
			}
		}

		protected override Customs.Business.BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(Customs.Business.BaseJobComInvoiceHeader invoiceHeader, int index, int invoiceIndex)
		{
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = Tariffs[(index + (invoiceIndex * 10)) % Tariffs.Length].ZZ1_TariffCode;
			return invoiceLine;
		}

		TariffView[] Tariffs
		{
			get
			{
				if (tariffs == null)
				{
					tariffs = Factory.Load<TariffView>(new ZQuery(TariffViewSchema.ZZ1_TariffCode, TariffsToLoad));
				}

				return tariffs;
			}
		}

		TariffView[] tariffs;
		ZString[] TariffsToLoad
		{
			get
			{
				return new ZString[] { "01041090", "02071491", "03028913", "04062090", "05080020", "06042090", "07123100", "08109010", "09092200", "10062010", "11042920", "12119019", "13021930", "14012030", "15149190", "16042029", "17022000", "18069010", "19030000", "20086010", "21069053", "22089080", "23099013", "24039990", "25010010", "26169000", "27101983", "28261900", "29071500", "30049063", "31023000", "32089021", "33079010", "34070020", "35030019", "36069020", "37031010", "38063010", "39033040", "40023110", "41133000", "42032910", "43018000", "44072981", "45049000", "46012100", "47079000", "48010010", "49119930", "50072090", "51053100", "52051200", "53071000", "54024900", "55020000", "56079030", "57029911", "58042910", "59113200", "60053410", "61022000", "62101011", "63029900", "64029990", "65050090", "66039010", "67041100", "68099090", "69101000", "70031290", "71151000", "72024100", "73061990", "74111000", "75051100", "76061239", "76169940", "78042000", "79012000", "80070091", "81129200", "82023900", "83023010", "84022020", "85271999", "86071900", "87039011", "88040090", "89020094", "90151010", "91021200", "92089010", "93033000", "94051090", "95061200", "96035000", "97030030", "98920022" };
			}
		}

		protected UniversalReferenceTestDataHelper helper;
		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			helper.LoadOrCreateNewTariff(tariffType, "01041090");
			helper.LoadOrCreateNewTariff(tariffType, "02071491");
			helper.LoadOrCreateNewTariff(tariffType, "03028913");
			helper.LoadOrCreateNewTariff(tariffType, "04062090");
			helper.LoadOrCreateNewTariff(tariffType, "05080020");
			helper.LoadOrCreateNewTariff(tariffType, "06042090");
			helper.LoadOrCreateNewTariff(tariffType, "07123100");
			helper.LoadOrCreateNewTariff(tariffType, "08109010");
			helper.LoadOrCreateNewTariff(tariffType, "09092200");
			helper.LoadOrCreateNewTariff(tariffType, "10062010");
			helper.LoadOrCreateNewTariff(tariffType, "11042920");
			helper.LoadOrCreateNewTariff(tariffType, "12119019");
			helper.LoadOrCreateNewTariff(tariffType, "13021930");
			helper.LoadOrCreateNewTariff(tariffType, "14012030");
			helper.LoadOrCreateNewTariff(tariffType, "15149190");
			helper.LoadOrCreateNewTariff(tariffType, "16042029");
			helper.LoadOrCreateNewTariff(tariffType, "17022000");
			helper.LoadOrCreateNewTariff(tariffType, "18069010");
			helper.LoadOrCreateNewTariff(tariffType, "19030000");
			helper.LoadOrCreateNewTariff(tariffType, "20086010");
			helper.LoadOrCreateNewTariff(tariffType, "21069053");
			helper.LoadOrCreateNewTariff(tariffType, "22089080");
			helper.LoadOrCreateNewTariff(tariffType, "23099013");
			helper.LoadOrCreateNewTariff(tariffType, "24039990");
			helper.LoadOrCreateNewTariff(tariffType, "25010010");
			helper.LoadOrCreateNewTariff(tariffType, "26169000");
			helper.LoadOrCreateNewTariff(tariffType, "27101983");
			helper.LoadOrCreateNewTariff(tariffType, "28261900");
			helper.LoadOrCreateNewTariff(tariffType, "29071500");
			helper.LoadOrCreateNewTariff(tariffType, "30049063");
			helper.LoadOrCreateNewTariff(tariffType, "31023000");
			helper.LoadOrCreateNewTariff(tariffType, "32089021");
			helper.LoadOrCreateNewTariff(tariffType, "33079010");
			helper.LoadOrCreateNewTariff(tariffType, "34070020");
			helper.LoadOrCreateNewTariff(tariffType, "35030019");
			helper.LoadOrCreateNewTariff(tariffType, "36069020");
			helper.LoadOrCreateNewTariff(tariffType, "37031010");
			helper.LoadOrCreateNewTariff(tariffType, "38063010");
			helper.LoadOrCreateNewTariff(tariffType, "39033040");
			helper.LoadOrCreateNewTariff(tariffType, "40023110");
			helper.LoadOrCreateNewTariff(tariffType, "41133000");
			helper.LoadOrCreateNewTariff(tariffType, "42032910");
			helper.LoadOrCreateNewTariff(tariffType, "43018000");
			helper.LoadOrCreateNewTariff(tariffType, "44072981");
			helper.LoadOrCreateNewTariff(tariffType, "45049000");
			helper.LoadOrCreateNewTariff(tariffType, "46012100");
			helper.LoadOrCreateNewTariff(tariffType, "47079000");
			helper.LoadOrCreateNewTariff(tariffType, "48010010");
			helper.LoadOrCreateNewTariff(tariffType, "49119930");
			helper.LoadOrCreateNewTariff(tariffType, "50072090");
			helper.LoadOrCreateNewTariff(tariffType, "51053100");
			helper.LoadOrCreateNewTariff(tariffType, "52051200");
			helper.LoadOrCreateNewTariff(tariffType, "53071000");
			helper.LoadOrCreateNewTariff(tariffType, "54024900");
			helper.LoadOrCreateNewTariff(tariffType, "55020000");
			helper.LoadOrCreateNewTariff(tariffType, "56079030");
			helper.LoadOrCreateNewTariff(tariffType, "57029911");
			helper.LoadOrCreateNewTariff(tariffType, "58042910");
			helper.LoadOrCreateNewTariff(tariffType, "59113200");
			helper.LoadOrCreateNewTariff(tariffType, "60053410");
			helper.LoadOrCreateNewTariff(tariffType, "61022000");
			helper.LoadOrCreateNewTariff(tariffType, "62101011");
			helper.LoadOrCreateNewTariff(tariffType, "63029900");
			helper.LoadOrCreateNewTariff(tariffType, "64029990");
			helper.LoadOrCreateNewTariff(tariffType, "65050090");
			helper.LoadOrCreateNewTariff(tariffType, "66039010");
			helper.LoadOrCreateNewTariff(tariffType, "67041100");
			helper.LoadOrCreateNewTariff(tariffType, "68099090");
			helper.LoadOrCreateNewTariff(tariffType, "69101000");
			helper.LoadOrCreateNewTariff(tariffType, "70031290");
			helper.LoadOrCreateNewTariff(tariffType, "71151000");
			helper.LoadOrCreateNewTariff(tariffType, "72024100");
			helper.LoadOrCreateNewTariff(tariffType, "73061990");
			helper.LoadOrCreateNewTariff(tariffType, "74111000");
			helper.LoadOrCreateNewTariff(tariffType, "75051100");
			helper.LoadOrCreateNewTariff(tariffType, "76061239");
			helper.LoadOrCreateNewTariff(tariffType, "76169940");
			helper.LoadOrCreateNewTariff(tariffType, "78042000");
			helper.LoadOrCreateNewTariff(tariffType, "79012000");
			helper.LoadOrCreateNewTariff(tariffType, "80070091");
			helper.LoadOrCreateNewTariff(tariffType, "81129200");
			helper.LoadOrCreateNewTariff(tariffType, "82023900");
			helper.LoadOrCreateNewTariff(tariffType, "83023010");
			helper.LoadOrCreateNewTariff(tariffType, "84022020");
			helper.LoadOrCreateNewTariff(tariffType, "85271999");
			helper.LoadOrCreateNewTariff(tariffType, "86071900");
			helper.LoadOrCreateNewTariff(tariffType, "87039011");
			helper.LoadOrCreateNewTariff(tariffType, "88040090");
			helper.LoadOrCreateNewTariff(tariffType, "89020094");
			helper.LoadOrCreateNewTariff(tariffType, "90151010");
			helper.LoadOrCreateNewTariff(tariffType, "91021200");
			helper.LoadOrCreateNewTariff(tariffType, "92089010");
			helper.LoadOrCreateNewTariff(tariffType, "93033000");
			helper.LoadOrCreateNewTariff(tariffType, "94051090");
			helper.LoadOrCreateNewTariff(tariffType, "95061200");
			helper.LoadOrCreateNewTariff(tariffType, "96035000");
			helper.LoadOrCreateNewTariff(tariffType, "97030030");
			helper.LoadOrCreateNewTariff(tariffType, "98920022");
			Factory.Save();
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}
	}
}
