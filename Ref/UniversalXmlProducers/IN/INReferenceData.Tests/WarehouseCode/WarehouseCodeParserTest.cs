using System.Linq;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests;

[TestFixture]
public class WarehouseCodeParserTest
{
	[Test]
	public void TestParseResponse()
	{
		const string goodResponse = """
									<tbody id="tbodyId" style="display:none">
									<tr>
										<td class='tdDataNew'>MAA1U001</td>
										<td class='tdDataNew'>M/S APM TERMINAL(I) PVT. LTD</td>
										<td class='tdDataNew'>NO.78, ANNAUPPAMPARTTU VILLAGE,</td>
									</tr>
									<tr>
										<td class='tdDataNew'>MAA1U002</td>
										<td class='tdDataNew'>M/S LAKSHMI SAI LOGISTC P. LTD</td>
										<td class='tdDataNew'>RELIANCE ROAD, KONDEKARAI VALUUR</td>
									</tr>
									<tr>
										<td class='tdDataNew'>MAA1U002</td>
										<td class='tdDataNew'>N.A.</td>
										<td class='tdDataNew'>RELIANCE ROAD, KONDEKARAI VALUUR</td>
									</tr>
									<tr>
										<td class='tdDataNew'>maa1u002</td>
										<td class='tdDataNew'>M/S LAKSHMI SAI LOGISTC P. LTD</td>
										<td class='tdDataNew'>RELIANCE ROAD, KONDEKARAI VALUUR</td>
									</tr>
									""";
		var warehouseCodes = WarehouseCodeParser.ParseResponse(goodResponse);

		Assert.AreEqual(2, warehouseCodes.Count);

		var code = warehouseCodes.FirstOrDefault(x => x.ZZD_Code == "MAA1U001");
		Assert.NotNull(code);
		Assert.AreEqual(code.ZZD_Description, "M/S APM TERMINAL(I) PVT. LTD");
		Assert.AreEqual(code.RefCusCodeListAttributes[0].ZZE_Value, "NO.78, ANNAUPPAMPARTTU VILLAGE,");

		code = warehouseCodes.FirstOrDefault(x => x.ZZD_Code == "MAA1U002");
		Assert.NotNull(code);
		Assert.AreEqual(code.ZZD_Description, "M/S LAKSHMI SAI LOGISTC P. LTD");
		Assert.AreEqual(code.RefCusCodeListAttributes[0].ZZE_Value, "RELIANCE ROAD, KONDEKARAI VALUUR");

	}

	[Test]
	public void TestParseResponse_BadResponse()
	{
		const string badResponse1 = """
									<tbody id="tbodyId" style="display:none">
									<tr>
										<td class='tdDataNew'>MAA1U001</td>
										<td class='tdDataNew'>M/S APM TERMINAL(I) PVT. LTD</td>
									</tr>
									<tr>
										<td class='tdDataNew'>MAA1U002</td>
										<td class='tdDataNew'>M/S LAKSHMI SAI LOGISTC P. LTD</td>
									</tr>
									""";
		const string badResponse2 = """
									<tbody id="tbodyId" style="display:none">
									<tr>
										<td class='tdDataNew'>MAA1U001</td>
										<td class='tdDataNew'>M/S APM TERMINAL(I) PVT. LTD</td>
										<td class='tdDataNew'>NO.78, ANNAUPPAMPARTTU VILLAGE,</td>
										<td class='tdDataNew'>NO.78, ANNAUPPAMPARTTU VILLAGE,</td>
									</tr>
									<tr>
										<td class='tdDataNew'>MAA1U002</td>
										<td class='tdDataNew'>M/S LAKSHMI SAI LOGISTC P. LTD</td>
										<td class='tdDataNew'>RELIANCE ROAD, KONDEKARAI VALUUR</td>
										<td class='tdDataNew'>RELIANCE ROAD, KONDEKARAI VALUUR</td>
									</tr>
									""";
		const string badResponse3 = "";
		Assert.Throws<UnhandledApplicationException>(() => WarehouseCodeParser.ParseResponse(badResponse1));
		Assert.Throws<UnhandledApplicationException>(() => WarehouseCodeParser.ParseResponse(badResponse2));
		Assert.Throws<UnhandledApplicationException>(() => WarehouseCodeParser.ParseResponse(badResponse3));
	}
}
