using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// For duty, tax and fee computations (except HMF) on quota entries and warehouse withdrawals, 
	/// ACS ignores the IT date and begins the sequence at the entry date continuing through to the system date.
	/// --------------------------------------------------------------------------------------------------------
	///	For MPF (Merchandise Processing Fee) calculations, ACS ignores the IT date and 
	/// begins the sequence at the entry date continuing through to the system date.
	/// --------------------------------------------------------------------------------------------------------
	///	For HMF (Harbor Maintenance Fee) computations, the rate in effect on the date of importation is used.
	/// --------------------------------------------------------------------------------------------------------
	/// Note: Entry Date and Cargo Release Date are the same for normal entries, differ for border shipments
	/// 
	/// IT Date								Record Identifier 22
	/// Entry Date							Determined by CBP
	/// Estimated Entry Date				Record Identifier 10
	/// Cargo Release Date					Determined by CBP
	/// Preliminary Statement Print Date	Record Identifier 30
	/// Estimated Date of Arrival			Record Identifiers 20 & H1
	///	System Date							ACS processing date
	/// </summary>
	public class DutyFeeDateCalculator
	{
		public ZDate GetDutyFeeDateForHMF(JobDeclaration declaration)
		{
			ZDate result = declaration.JE_DateOfArrival.Date;

			if (result.IsEmpty)
			{
				result = declaration.US_ITDate.Date;
			}

			if (result.IsEmpty)
			{
				result = declaration.US_EntryDate.Date;
			}

			return TodayIfEmpty(result);
		}

		public ZDate GetDutyFeeDateForMPF(JobDeclaration declaration)
		{
			var result = ZDate.Empty;
			if (declaration != null)
			{
				result = declaration.US_MPFCalcDate;
				if (result.IsEmpty || declaration.ShouldCalculateMPFAndDutyDate)
				{
					result = GetDateFromEntryDateSequence(declaration);
				}
			}
			return TodayIfEmpty(result);
		}

		public ZDate GetDutyFeeDate(IDeclaration declaration)
		{
			var result = ZDate.Empty;

			if (declaration != null)
			{
				result = declaration.US_DutyCalcDate;
				if (result.IsEmpty || declaration.ShouldCalculateMPFAndDutyDate)
				{
					result = ZDate.Empty;
					if (!declaration.IsExWarehouse && !declaration.IsQuota)
					{
						result = declaration.US_ITDate.Date;
					}

					if (result.IsEmpty)
					{
						result = GetDateFromEntryDateSequence(declaration);
					}
				}
			}

			return TodayIfEmpty(result);
		}

		public ZDate GetDutyFeeDateForBond(JobDeclaration declaration)
		{
			ZDate result = GetCargoReleaseDate(declaration);

			if (result.IsEmpty)
			{
				result = GetCargoReleaseDate(declaration);
			}

			if (result.IsEmpty && declaration != null)
			{
				result = declaration.US_EstimatedEntryDate.Date;
			}

			if (result.IsEmpty && declaration != null)
			{
				result = declaration.US_PreliminaryStatementPrintDate.Date;
			}

			return TodayIfEmpty(result);
		}

		internal ZDate GetDateFromEntryDateSequence(IDeclaration declaration)
		{
			var result = declaration.US_EstimatedEntryDate.Date;

			if (result.IsEmpty)
			{
				result = GetCargoReleaseDate(declaration);
			}

			if (result.IsEmpty)
			{
				result = declaration.US_PreliminaryStatementPrintDate.Date;
			}

			if (result.IsEmpty && declaration.US_EntryDate.Date > ZDate.Today)
			{
				result = declaration.US_EntryDate.Date;
			}

			return result;
		}

		public ZDate GetDutyFeeDateForADD_CVD(JobDeclaration declaration)
		{
			ZDate result = declaration != null ? declaration.US_EstimatedEntryDate.Date : ZDate.Empty;

			if (result.IsEmpty && declaration != null)
			{
				result = GetCargoReleaseDate(declaration);
			}

			if (result.IsEmpty && declaration != null)
			{
				result = declaration.US_EntryDate.Date;
			}

			return TodayIfEmpty(result);
		}

		static ZDate TodayIfEmpty(ZDate value) => value.IsEmpty ? ZDate.Today : value;

		static ZDate GetCargoReleaseDate(IDeclaration declaration) => declaration != null ? declaration.JE_EntryAuthorisationDate.Date : ZDate.Empty;
	}
}
