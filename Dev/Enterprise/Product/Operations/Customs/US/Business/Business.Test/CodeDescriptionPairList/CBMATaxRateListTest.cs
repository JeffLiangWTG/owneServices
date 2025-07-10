using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CBMATaxRateListTest : TestCaseWithFactory
	{
		public void TestGetList()
		{
			var list1 = CBMATaxRateList.GetList(Factory, "016");
			var list2 = CBMATaxRateList.GetList(Factory, "016");
			AssertSame(list1, list2);
			var expectedRecords = new List<(string taxCode, string code, decimal rate, decimal ttbConfirmationRate)>(new[]
			{
				("016", "S01010", 0.71326450m, 2.7m),
				("016", "S01020", 3.52405520m, 13.34m)
			});
			AssertEquals(string.Join("\r\n", expectedRecords.Select(x => $"TaxCode='{x.taxCode}', Code='{x.code}', Rate='{x.rate}', TTB Confirmation Rate='{x.ttbConfirmationRate}'")),
				string.Join("\r\n", list1.Cast<CBMATaxRate>().Select(x => $"TaxCode='{x.TaxCode}', Code='{x.Code}', Rate='{x.Rate}', TTB Confirmation Rate='{x.TTBConfirmationRate}'")));

			list1 = CBMATaxRateList.GetList(Factory, "016", AppendixBTaxRateList.Codes.Wines_2);
			list2 = CBMATaxRateList.GetList(Factory, "016", AppendixBTaxRateList.Codes.Wines_2);
			AssertSame(list1, list2);
			AssertArrayEqualsByElements(new string[]
			{
				"W01010", "W01020", "W01030", "W04010", "W04020", "W04030", "W05010", "W05020", "W05030"
			}, list1.Cast<CBMATaxRate>().Select(x => x.Code).ToArray());

			list2 = CBMATaxRateList.GetList(Factory, "018", AppendixBTaxRateList.Codes.Wines_2);
			AssertArrayEqualsByElements(list1.Cast<CBMATaxRate>().Select(x => x.Code).ToArray(), list2.Cast<CBMATaxRate>().Select(x => x.Code).ToArray());

			list1 = CBMATaxRateList.GetList(Factory, "017", AppendixBTaxRateList.Codes.Wines_3);
			AssertArrayEqualsByElements(new string[]
			{
				"W02010", "W02020", "W02030"
			}, list1.Cast<CBMATaxRate>().Select(x => x.Code).ToArray());

			list1 = CBMATaxRateList.GetList(Factory, "018", AppendixBTaxRateList.Codes.Wines_4);
			AssertArrayEqualsByElements(new string[]
			{
				"W03010", "W03020", "W03030"
			}, list1.Cast<CBMATaxRate>().Select(x => x.Code).ToArray());

			list1 = CBMATaxRateList.GetList(Factory, "022", AppendixBTaxRateList.Codes.Wines_6);
			AssertArrayEqualsByElements(new string[]
			{
				"W06010", "W06020", "W06030"
			}, list1.Cast<CBMATaxRate>().Select(x => x.Code).ToArray());

			list1 = CBMATaxRateList.GetList(Factory, "016", AppendixBTaxRateList.Codes.Wines_5);
			AssertArrayEqualsByElements(new string[]
			{
				"W07010", "W07020", "W07030"
			}, list1.Cast<CBMATaxRate>().Select(x => x.Code).ToArray());

			list1 = CBMATaxRateList.GetList(Factory, "017", AppendixBTaxRateList.Codes.Other_4);
			AssertArrayEqualsByElements(new string[]
			{
				"S01010", "S01020"
			}, list1.Cast<CBMATaxRate>().Select(x => x.Code).ToArray());

			list1 = CBMATaxRateList.GetList(Factory, "018", AppendixBTaxRateList.Codes.Other_3);
			AssertArrayEqualsByElements(new string[]
			{
				"B01010"
			}, list1.Cast<CBMATaxRate>().Select(x => x.Code).ToArray());

			list1 = CBMATaxRateList.GetList(Factory, "016", AppendixBTaxRateList.Codes.Specify);
			list2 = CBMATaxRateList.GetList(Factory, "016");
			AssertArrayEqualsByElements(list1.Cast<CBMATaxRate>().Select(x => x.Code).ToArray(), list2.Cast<CBMATaxRate>().Select(x => x.Code).ToArray());
		}

		public void TestFullList()
		{
			CombineAssertions(() =>
			{
				var list = new CBMATaxRateList();
				var expectedRecords = new List<(string taxCode, string code, decimal rate, decimal ttbConfirmationRate)>(new[]
				{
					("022", "B01010", 0.1363469m, 16m),
					("017", "W01010", 0.01849200m, 0.07m),
					("017", "W01020", 0.04490920m, 0.17m),
					("017", "W01030", 0.14133200m, 0.535m),
					("017", "W02010", 0.15057810m, 0.57m),
					("017", "W02020", 0.17699530m, 0.67m),
					("017", "W02030", 0.27341800m, 1.035m),
					("017", "W03010", 0.56796990m, 2.15m),
					("017", "W03020", 0.59438710m, 2.25m),
					("017", "W03030", 0.69081000m, 2.615m),
					("017", "W04010", 0.01849200m, 0.07m),
					("017", "W04020", 0.04490920m, 0.17m),
					("017", "W04030", 0.14133200m, 0.535m),
					("017", "W05010", 0.01849200m, 0.07m),
					("017", "W05020", 0.04490920m, 0.17m),
					("017", "W05030", 0.14133200m, 0.535m),
					("017", "W06010", 0.60759570m, 2.3m),
					("017", "W06020", 0.63401290m, 2.4m),
					("017", "W06030", 0.73043570m, 2.765m),
					("017", "W07010", 0.63401290m, 2.4m),
					("017", "W07020", 0.66043010m, 2.5m),
					("017", "W07030", 0.75685290m, 2.865m),
					("017", "W08010", 0.04332420m, 0.164m),
					("017", "W08020", 0.04490920m, 0.17m),
					("017", "W08030", 0.05098520m, 0.193m),
					("016", "S01010", 0.71326450m, 2.7m),
					("016", "S01020", 3.52405520m, 13.34m)
				});
				var missingData = new ZStringBuilder();
				foreach (var record in expectedRecords)
				{
					var recordValue = $"TaxCode='{record.taxCode}', Code='{record.code}', Rate='{record.rate}', TTB Confirmation Rate='{record.ttbConfirmationRate}'";
					var actual = (CBMATaxRate)list[record.code];
					if (actual == null)
					{
						missingData.Append(recordValue);
					}
					else
					{
						AssertEquals(recordValue, $"TaxCode='{actual.TaxCode}', Code='{actual.Code}', Rate='{actual.Rate}', TTB Confirmation Rate='{actual.TTBConfirmationRate}'");
						list.Remove(actual);
					}
				}

				if (!missingData.IsEmpty)
				{
					Fail($"The following record are no longer supported:\r\n{missingData.ToStringWithNewLineBetweenAppends()}");
				}

				if (list.Count > 0)
				{
					var unexpctedData = string.Join("\r\n",
						list.OfType<CBMATaxRate>().Select(x =>
							$"TaxCode='{x.TaxCode}', Code='{x.Code}', Rate='{x.Rate}', TTB Confirmation Rate='{x.TTBConfirmationRate}', Description={x.Description}"));
					Fail($"The following codes are unexpected:\r\n{unexpctedData}");
				}
			});
		}

		public void Testvw_CBMATaxRateCode()
		{
			var list = new CBMATaxRateList();

			var sql = @"SELECT Code FROM dbo.vw_CBMATaxRateCode";
			var expectedTaxRateCodeList = new List<string>();
			using (var command = Db.Connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						expectedTaxRateCodeList.Add(reader.GetString(0));
					}
				}
			}

			var actualTaxRateCodeList = list.Cast<CBMATaxRate>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder("CBMA Tax Rate Code List and vw_CBMATaxRateCode should be in sync", actualTaxRateCodeList, expectedTaxRateCodeList);
		}
	}
}
