using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ManufacturerDataGeneratorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestEmptyBL_ManufacturerDocAddressPKDoesNotThrowException()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			var manufacturerer = header.ManufacturerAddresses.AddNew();
			manufacturerer.OrganisationPK = Factory.New<OrgHeader>().PK;
			var line = header.Lines.AddNew();
			line.BL_ManufacturerDocAddressPK = ZGuid.Empty;
			var builder = new ImporterSecurityFilingMessageBuilder<ACEInputBlockControlGenerator, AABIInputB, AABIInputY>(header, Messaging.Business.UpdateActionCode.Add);
			builder.PopulateMessage();
		}

		public void TestGenerate()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			var manufacturer1 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			manufacturer1.E2_AddressOverride = true;
			manufacturer1.E2_CompanyName = "Company 1";
			JobDocAddress manufacturer2 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			manufacturer2.E2_AddressOverride = true;
			manufacturer2.E2_CompanyName = "Company 2";
			CusISFLine line1 = header.Lines.AddNew();
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line1.BL_HarmonisedNum = "1010101010";
			line1.BL_ManufacturerDocAddressPK = manufacturer1.PK;
			CusISFLine line2 = header.Lines.AddNew();
			line2.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewCaledonia;
			line2.BL_HarmonisedNum = "2010101010";
			line2.BL_ManufacturerDocAddressPK = ZGuid.Empty;
			CusISFLine line3 = header.Lines.AddNew();
			line3.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewZealand;
			line3.BL_HarmonisedNum = "3010101010";
			line3.BL_ManufacturerDocAddressPK = manufacturer2.PK;
			CusISFLine line4 = header.Lines.AddNew();
			line4.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewZealand;
			line4.BL_HarmonisedNum = "3010131010";
			line4.BL_ManufacturerDocAddressPK = manufacturer2.PK;
			CusISFLine line5 = header.Lines.AddNew();
			line5.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line5.BL_HarmonisedNum = "1010101020";
			line5.BL_ManufacturerDocAddressPK = manufacturer1.PK;
			CusISFLine line6 = header.Lines.AddNew();
			line6.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewCaledonia;
			line6.BL_HarmonisedNum = "2010102010";
			line6.BL_ManufacturerDocAddressPK = ZGuid.Empty;
			CusISFLine line7 = header.Lines.AddNew();
			line7.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line7.BL_HarmonisedNum = "1010101020";
			line7.BL_ManufacturerDocAddressPK = manufacturer1.PK;
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Six;
			List<IManufacturerData> datas = new List<IManufacturerData>(ManufacturerDataGenerator.Generate(header.Lines, TariffDataCollection.MergeStyle.Merge));
			AssertEquals(3, datas.Count);
			AssertManufacturerData(datas[0], manufacturer1, new List<TariffData>()
			{ GetTariffData(line1) });
			AssertManufacturerData(datas[1], null, new List<TariffData>()
			{ GetTariffData(line2) });
			AssertManufacturerData(datas[2], manufacturer2, new List<TariffData>()
			{ GetTariffData(line3), GetTariffData(line4) });
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Eight;
			datas = new List<IManufacturerData>(ManufacturerDataGenerator.Generate(header.Lines, TariffDataCollection.MergeStyle.Merge));
			AssertEquals(3, datas.Count);
			AssertManufacturerData(datas[0], manufacturer1, new List<TariffData>()
			{ GetTariffData(line1) });
			AssertManufacturerData(datas[1], null, new List<TariffData>()
			{ GetTariffData(line2), GetTariffData(line6) });
			AssertManufacturerData(datas[2], manufacturer2, new List<TariffData>()
			{ GetTariffData(line3), GetTariffData(line4) });
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Ten;
			datas = new List<IManufacturerData>(ManufacturerDataGenerator.Generate(header.Lines, TariffDataCollection.MergeStyle.Merge));
			AssertEquals(3, datas.Count);
			AssertManufacturerData(datas[0], manufacturer1, new List<TariffData>()
			{ GetTariffData(line1), GetTariffData(line5) });
			AssertManufacturerData(datas[1], null, new List<TariffData>()
			{ GetTariffData(line2), GetTariffData(line6) });
			AssertManufacturerData(datas[2], manufacturer2, new List<TariffData>()
			{ GetTariffData(line3), GetTariffData(line4) });
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Six;
			line6.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			datas = new List<IManufacturerData>(ManufacturerDataGenerator.Generate(header.Lines, TariffDataCollection.MergeStyle.Merge));
			AssertEquals(3, datas.Count);
			AssertManufacturerData(datas[0], manufacturer1, new List<TariffData>()
			{ GetTariffData(line1) });
			AssertManufacturerData(datas[1], null, new List<TariffData>()
			{ GetTariffData(line2), GetTariffData(line6) });
			AssertManufacturerData(datas[2], manufacturer2, new List<TariffData>()
			{ GetTariffData(line3), GetTariffData(line4) });
			datas = new List<IManufacturerData>(ManufacturerDataGenerator.Generate(header.Lines, TariffDataCollection.MergeStyle.NotMerge));
			AssertEquals(3, datas.Count);
			AssertManufacturerData(datas[0], manufacturer1, new List<TariffData>()
			{ GetTariffData(line1), GetTariffData(line5), GetTariffData(line7) });
			AssertManufacturerData(datas[1], null, new List<TariffData>()
			{ GetTariffData(line2), GetTariffData(line6) });
			AssertManufacturerData(datas[2], manufacturer2, new List<TariffData>()
			{ GetTariffData(line3), GetTariffData(line4) });
		}

		TariffData GetTariffData(CusISFLine line)
		{
			return new TariffData()
			{ CountryOfOrigin = line.BL_RN_NKGoodsOrigin, HarmonizedTariffNumber = line.HarmonisedNumToReportToCustoms };
		}

		void AssertManufacturerData(IManufacturerData manufacturerData, JobDocAddress manufacturer, List<TariffData> list)
		{
			if (manufacturer != null)
			{
				AssertEquals(manufacturer, ((SanitizedISFDocAddressWrapper)manufacturerData.Manufacturer).DocAddress);
			}
			else
			{
				AssertNull(manufacturerData.Manufacturer);
			}

			AssertContainsExactElementsInAnyOrder(manufacturerData.Tariffs, list);
		}
	}
}
