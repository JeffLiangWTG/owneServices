using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Moq;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class RefCusTradeGroupParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "RefCusTradeGroup";

		protected override string TextFileName => "PRSPSNAP-P1-EDMAIN-2306090141.txt";

		protected override string XMLFileName => "AU CMR Trade Groups.xml";

		protected override DateTime PublishedDate => new DateTime(2022, 08, 05, 01, 43, 00);

		protected override ICMRDataParser Parser
		{
			get
			{
				var mockParser = new Mock<RefCusTradeGroupParser>() { CallBase = true };
				mockParser.Setup(x => x.TradeGroupCountriesByTradeGroup).Returns(CountryAttribute);
				return mockParser.Object;
			}
		}

		Dictionary<string, List<RefCusTradeGroupCountry>> CountryAttribute
		{
			get
			{
				return new Dictionary<string, List<RefCusTradeGroupCountry>>
				{
					{ "AANZ", new List<RefCusTradeGroupCountry>(){ new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "BN", ZZB_StartDate = new DateTime(2021, 11, 04) }, new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "ID", ZZB_StartDate = new DateTime(2021, 11, 04) } }},
					{ "CA", new List<RefCusTradeGroupCountry>(){ new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "CA", ZZB_StartDate = new DateTime(2021, 09, 07) } }},
					{ "DC", new List<RefCusTradeGroupCountry>(){ new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "AF", ZZB_StartDate = new DateTime(2021, 09, 07) }, new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "AO", ZZB_StartDate = new DateTime(2021, 09, 07) }, new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "AS", ZZB_StartDate = new DateTime(2021, 09, 07) } } },
				};
			}
		}
	}
}
