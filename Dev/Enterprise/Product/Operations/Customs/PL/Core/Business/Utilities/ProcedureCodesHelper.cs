using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public static class ProcedureCodesHelper
{
	public static void CheckForRuleR407(ZString procedureCode, ZString concessionCode, ZPropertyInfo propertyInfo, Action<ZPropertyInfo, ZString> howToAddNotification = null)
	{
		if (!ValidateForRuleR407(procedureCode, concessionCode))
		{
			(howToAddNotification ?? AddMessageError).Invoke(propertyInfo, Res.GetString("PLImportJobComInvoiceLineValidation|R407", "R407 - Invalid Procedure/Add.Procedure Code combination."));
		}
	}

	static bool ValidateForRuleR407(ZString procedureCode, ZString concessionCode)
	{
		return IsProcedureCodeValid() || IsConcessionCodeValid();

		bool IsProcedureCodeValid()
		{
			var firstCharacter = procedureCode.Left(1);
			return firstCharacter == "4" || firstCharacter == "6";
		}

		bool IsConcessionCodeValid()
		{
			var itemList = new List<ZString>
			{
				"C01", "C02", "C03", "C04", "C06", "C07", "C08", "C09",
				"C10", "C11", "C12", "C13", "C14", "C15", "C16", "C17", "C18", "C19",
				"C20", "C21", "C22", "C23", "C24", "C25", "C26", "C27", "C28", "C29",
				"C30", "C31", "C32", "C33", "C34", "C35", "C36", "C37", "C38", "C39",
				"C40", "C41",
				"B02", "B03",
				"F01", "F02", "F03", "F21", "F22",
				"3PL", "4PL", "5PL", "6PL", "7PL",
				"1C1"
			};
			return !itemList.Contains(concessionCode);
		}
	}

	public static void CheckForRuleR414(ZString procedureCode, ZString concessionCode, ZPropertyInfo propertyInfo, Action<ZPropertyInfo, ZString> howToAddNotification = null)
	{
		if (!ValidateForRuleR414(procedureCode, concessionCode))
		{
			(howToAddNotification ?? AddMessageError).Invoke(propertyInfo, Res.GetString("PLImportJobComInvoiceLineValidation|R414", "R414 - Invalid Procedure/Add.Procedure Code combination."));
		}
	}

	static bool ValidateForRuleR414(ZString procedureCode, ZString concessionCode)
	{
		return concessionCode != "B07" || procedureCode == "48";
	}

	public static void CheckForRuleR419(ZString concessionCode, ZPropertyInfo propertyInfo, ZString tariff, Action<ZPropertyInfo, ZString> howToAddNotification = null)
	{
		if (!ValidateForRuleR419(concessionCode, tariff))
		{
			(howToAddNotification ?? AddMessageError).Invoke(propertyInfo, Res.GetString("PLImportJobComInvoiceLineValidation|R419", "R419 - Invalid CN Code/Add.Procedure Code C01."));
		}
	}

	static bool ValidateForRuleR419(ZString concessionCode, ZString tariff)
	{
		return concessionCode != "C01" || tariff == "9905";
	}

	public static void CheckForRuleR421(ZString concessionCode, ZPropertyInfo propertyInfo, ZString tariff, Action<ZPropertyInfo, ZString> howToAddNotification = null)
	{
		if (!ValidateForRuleR421(concessionCode, tariff))
		{
			(howToAddNotification ?? AddMessageError).Invoke(propertyInfo, Res.GetString("PLImportJobComInvoiceLineValidation|R421", "R421 - Invalid Procedure/Add.Procedure Code combination."));
		}
	}

	static bool ValidateForRuleR421(ZString concessionCode, ZString tariff)
	{
		return tariff == "9919" || IsConcessionCodeValid();

		bool IsConcessionCodeValid()
		{
			var itemList = new List<ZString> { "C02", "C03", "C04", "C06", "C41", "C20", "C26" };
			return !itemList.Contains(concessionCode);
		}
	}

	public static void CheckForRuleR424(ZString procedureCode, ZString concessionCode, ZPropertyInfo propertyInfo, Action<ZPropertyInfo, ZString> howToAddNotification = null)
	{
		if (!ValidateForRuleR424(procedureCode, concessionCode))
		{
			(howToAddNotification ?? AddMessageError).Invoke(propertyInfo, Res.GetString("PLImportJobComInvoiceLineValidation|R424", "R424 - Invalid Procedure/Add.Procedure Code combination."));
		}
	}

	static bool ValidateForRuleR424(ZString procedureCode, ZString concessionCode)
	{
		return IsProcedureCodeValid() || concessionCode.Left(1) != "D";

		bool IsProcedureCodeValid()
		{
			var firstCharacter = procedureCode.Left(1);
			return firstCharacter != "4" && firstCharacter != "6" && procedureCode != "51" && procedureCode != "71";
		}
	}

	public static void CheckForRuleR480(ZString procedureCode, ZString concessionCode, ZPropertyInfo propertyInfo, Action<ZPropertyInfo, ZString> howToAddNotification = null)
	{
		if (!ValidateForRuleR480(procedureCode, concessionCode))
		{
			(howToAddNotification ?? AddMessageError).Invoke(propertyInfo, Res.GetString("PLImportJobComInvoiceLineValidation|R480", "R480 - Invalid Procedure/Add.Procedure Code combination."));
		}
	}

	static bool ValidateForRuleR480(ZString procedureCode, ZString concessionCode)
	{
		return IsConcessionCodeValid() || IsProcedureCodeValid();

		bool IsConcessionCodeValid()
		{
			var itemList = new List<ZString> { "B01", "B02", "B03", "B04" };
			return !itemList.Contains(concessionCode);
		}

		bool IsProcedureCodeValid()
		{
			var itemList = new List<ZString> { "51", "61", "63" };
			return !itemList.Contains(procedureCode);
		}
	}

	public static void CheckForRuleR859(ZString procedureCode, ZString concessionCode, ZPropertyInfo propertyInfo, Action<ZPropertyInfo, ZString> howToAddNotification = null)
	{
		if (!ValidateForRuleR859(procedureCode, concessionCode))
		{
			(howToAddNotification ?? AddMessageError).Invoke(propertyInfo, Res.GetString("PLImportJobComInvoiceLineValidation|R859", "R859 - Invalid Procedure/Add.Procedure Code combination."));
		}
	}

	static bool ValidateForRuleR859(ZString procedureCode, ZString concessionCode)
	{
		var firstCharacter = procedureCode.Left(1);
		return concessionCode != "2PL" || firstCharacter == "4" || firstCharacter == "6";
	}

	public static void CheckForRuleR975(ZString previousProcedure, ZString concessionCode, ZPropertyInfo propertyInfo, Action<ZPropertyInfo, ZString> howToAddNotification = null)
	{
		if (!ValidateForRuleR975(previousProcedure, concessionCode))
		{
			(howToAddNotification ?? AddMessageError).Invoke(propertyInfo, Res.GetString("PLImportJobComInvoiceLineValidation|R975", "R975 - Invalid Procedure/Add.Procedure Code combination."));
		}
	}

	static bool ValidateForRuleR975(ZString previousProcedure, ZString concessionCode)
	{
		return concessionCode != "F44" || previousProcedure == "51" || previousProcedure == "54";
	}

	public static void CheckForRuleR1049(ZString procedureCode, ZString concessionCode, ZPropertyInfo propertyInfo, JobComInvoiceLine invoiceLine, Action<ZPropertyInfo, ZString> howToAddNotification = null)
	{
		if (!ValidateForRuleR1049(procedureCode, concessionCode, invoiceLine))
		{
			(howToAddNotification ?? AddMessageError).Invoke(propertyInfo, Res.GetString("PLImportJobComInvoiceLineValidation|R1049", "R1049 - Invalid Procedure/Add.Procedure Code combination."));
		}
	}

	static bool ValidateForRuleR1049(ZString procedureCode, ZString concessionCode, JobComInvoiceLine invoiceLine)
	{
		return IsProcedureCodeValid() || IsConcessionCodeValid();

		bool IsProcedureCodeValid()
		{
			return invoiceLine == null || (procedureCode != "45" && procedureCode != "68" && !DoesAnyProcedureCodeContainsItem("F06", invoiceLine));
		}

		bool IsConcessionCodeValid()
		{
			var itemList = new List<ZString>
			{
				"6A1", "6A2", "6A3", "6A4", "6A5", "6A6", "6A7", "6A8", "6A9",
				"7A1", "7A2", "7A3", "7A4", "7A5", "7A6", "7A7", "7A8", "7A9",
				"8A8"
			};
			return !itemList.Contains(concessionCode);
		}
	}

	static bool DoesAnyProcedureCodeContainsItem(ZString item, JobComInvoiceLine invoiceLine) => invoiceLine != null && invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Any(x => item == x.CY_Code);

	static void AddMessageError(ZPropertyInfo propertyInfo, ZString message)
	{
		propertyInfo.AddMessageError(message);
	}
}
