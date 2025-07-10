using System.Collections.Generic;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure
{
	public class ProcedureCodeProcessorTest
	{
		[Test]
		public void TestProcessor()
		{
			var procedureCodes = new List<ProcedureCodeRaw>
			{
				new ProcedureCodeRaw() { Code = "0100", Description = descripton0100, ShipmentType = "IMP" },
				new ProcedureCodeRaw() { Code = "0121", Description = descripton0121, ShipmentType = "IMP" },
				new ProcedureCodeRaw() { Code = "0012", Description = descripton0012, ShipmentType = "EXP" },
			};

			var additionalProcedureCodes = new List<AdditionalProcedureCodeRaw>
			{
				new AdditionalProcedureCodeRaw() { Code = "E01", Description = descriptionE01 },
				new AdditionalProcedureCodeRaw() { Code = "E02", Description = descriptionE02 },
				new AdditionalProcedureCodeRaw() { Code = "F06", Description = descriptionF06 },
				new AdditionalProcedureCodeRaw() { Code = "F15", Description = descriptionF15 },
				new AdditionalProcedureCodeRaw() { Code = "1CD", Description = description1CD },
				new AdditionalProcedureCodeRaw() { Code = "000", Description = description000 },
				new AdditionalProcedureCodeRaw() { Code = "10C", Description = description10C },
				new AdditionalProcedureCodeRaw() { Code = "14C", Description = description14C },
			};

			var additionalProcedureMappings = new List<AdditionalProcedureMapping>
			{
				new AdditionalProcedureMapping() { ProcedureCode = "0100", AdditionalProcedureCodes = new List<string> { "E01", "E02", "F06" }},
				new AdditionalProcedureMapping() { ProcedureCode = "0121", AdditionalProcedureCodes = new List<string> { "F06", "F15", "1CD" }},
				new AdditionalProcedureMapping() { ProcedureCode = "0012", AdditionalProcedureCodes = new List<string> { "000", "10C", "14C" }},
			};

			var data = ProcedureCodeProcessor.PopulateProcedureCodeData(procedureCodes, additionalProcedureCodes, additionalProcedureMappings);

			Assert.IsInstanceOf<List<ProcedureCodeData>>(data);
			Assert.AreEqual(9, data.Count);
			AssertAdditionalProcedureCodeData(data[0], "0100", descripton0100, "IMP", "E01", descriptionE01);
			AssertAdditionalProcedureCodeData(data[1], "0100", descripton0100, "IMP", "E02", descriptionE02);
			AssertAdditionalProcedureCodeData(data[2], "0100", descripton0100, "IMP", "F06", descriptionF06);
			AssertAdditionalProcedureCodeData(data[3], "0121", descripton0121, "IMP", "F06", descriptionF06);
			AssertAdditionalProcedureCodeData(data[4], "0121", descripton0121, "IMP", "F15", descriptionF15);
			AssertAdditionalProcedureCodeData(data[5], "0121", descripton0121, "IMP", "1CD", description1CD);
			AssertAdditionalProcedureCodeData(data[6], "0012", descripton0012, "EXP", "000", description000);
			AssertAdditionalProcedureCodeData(data[7], "0012", descripton0012, "EXP", "10C", description10C);
			AssertAdditionalProcedureCodeData(data[8], "0012", descripton0012, "EXP", "14C", description14C);
		}

		void AssertAdditionalProcedureCodeData(ProcedureCodeData data, string procedureCode, string procedureCodeDescription, string shipmentType, string additionalProcedureCode, string additionalProcedureCodeDescription)
		{
			Assert.AreEqual(data.ProcedureCode, procedureCode);
			Assert.AreEqual(data.Description, $"{procedureCodeDescription} - {additionalProcedureCodeDescription}");
			Assert.AreEqual(data.ShipmentType, shipmentType);
			Assert.AreEqual(data.AdditionalProcedureCode, additionalProcedureCode);
		}

		const string descripton0100 = "Free circulation with onward dispatch";
		const string descripton0121 = "Re-import after outward processing (OP) with onward dispatch";
		const string descripton0012 = "Permanent Export of Union Goods";

		const string descriptionE01 = "Use of the Unit Price for the Determination of the Customs Value for Certain Perishable Goods (Article 74(2)(c) of the Code and Article 142(6) of Commission Implementing Regulation No. (EU) 2015/2447 )";
		const string descriptionE02 = "Standard Import Values (SIV) as published in the EU Official Journal (OJ)";
		const string descriptionF06 = "A movement of excise goods under an excise duty suspension arrangement from the place of importation in accordance with Article 17(1)(b) of Directive 2008/118/EC";
		const string descriptionF15 = "Goods introduced in the context of trade with Special Fiscal Territories or territories with which the EU has formed a Customs Union (Article 1(3) of the Code)";
		const string description1CD = "Controlled drugs using Simplified Procedures";
		const string description000 = "No Additional Conditions";
		const string description10C = "Items of correspondence";
		const string description14C = "Continental Shelf";
	}
}
