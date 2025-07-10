using System;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	public sealed class UniversalRateDataForTestWithData : UniversalRateDataForTest
	{
		public UniversalRateDataForTestWithData()
		{
			InitializeUOMList();
			InitializeCountrySpecificValueList();
		}

		void InitializeUOMList()
		{
			UnitOfMeasureValueList.Add("BG", 1000);
			UnitOfMeasureValueList.Add("GK", 1000);
			UnitOfMeasureValueList.Add("KG", 1000);
			UnitOfMeasureValueList.Add("KWH", 1000);
			UnitOfMeasureValueList.Add("KG NET", 1000);
			UnitOfMeasureValueList.Add("L", 1000);
			UnitOfMeasureValueList.Add("LA", 1000);
			UnitOfMeasureValueList.Add("M2", 1000);
			UnitOfMeasureValueList.Add("M3", 1000);
			UnitOfMeasureValueList.Add("NO", 1000);
			UnitOfMeasureValueList.Add("PR", 1000);
		}

		void InitializeCountrySpecificValueList()
		{
			CountrySpecificValueList.Add("1P1", 200);
			CountrySpecificValueList.Add("1P2A", 200);
			CountrySpecificValueList.Add("1P2B", 200);
			CountrySpecificValueList.Add("1P3A", 200);
			CountrySpecificValueList.Add("1P3B", 200);
			CountrySpecificValueList.Add("1P3C", 200);
			CountrySpecificValueList.Add("1P3D", 200);
			CountrySpecificValueList.Add("1P5A", 200);
			CountrySpecificValueList.Add("1P5B", 200);
		}

		public void AddDefaultFormulaSpecificAnswer(Action<ZString, decimal> addAnswer)
		{
			addAnswer("Days Owned where owned for less than 12 months", 50);
			addAnswer("Duty Paid On Entry", 50);
			addAnswer("Duty Payable on Value Calculated in Terms of Note 29", 50);
			addAnswer("Duty Payable on Value Calculated in Terms of Note 8.1", 50);
			addAnswer("Duty as calculated in terms of the IRCC", 50);
			addAnswer("Duty as calculated in terms of the notes to this rebate item", 50);
			addAnswer("Duty of Schedule 1 Part 1 as calculated in terms of the IRCC", 50);
			addAnswer("Duty on the Cost of Manufacture, Processing or Repair", 50);
			addAnswer("Duty payable per quarter for Excise duty purposes", 50);
			addAnswer("Maximum Rebate Amount", 50);
			addAnswer("Rebate Amount", 50);
			addAnswer("Rebate, Refund and Drawback previously granted", 50);
		}
	}
}
