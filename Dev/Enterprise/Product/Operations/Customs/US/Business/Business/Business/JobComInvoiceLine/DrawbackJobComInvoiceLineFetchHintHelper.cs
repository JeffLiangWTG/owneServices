using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class DrawbackJobComInvoiceLineFetchHintHelper
	{
		public static void AddFetchHintForUSImportEntryLineTable(BusinessObjectFactory factory, ZString importEntryNo, ZShort drwImportDecLine)
		{
			var entryFilerCode = importEntryNo.Left(AddInfoJobDeclaration.Schema.US_EntryFilerCodeMaxLength);
			var entryNum = importEntryNo.SubstringSafe(USAddInfoSchema.US_EntryFilerCode.MaxLength);

			var query = new ZQuery(USImportEntryLineSchema.USE_EntryFilerCode, entryFilerCode);
			query.AddToFilter(USImportEntryLineSchema.USE_EntryNum, entryNum);
			query.AddToFilter(USImportEntryLineSchema.USE_LineNumber, drwImportDecLine);
			query.AddToFilter(USImportEntryLineSchema.USE_SupLine, ZBool.False);
			factory.AddFetchHint(USImportEntryLineSchema.Instance, query);
		}

		public static USImportEntryLine GetViewDrawbackImportEntryLineData(Func<ZQuery, USImportEntryLine[]> usImportEntryLineGetter, ZString importEntryNo, ZShort drwImportDecLine)
		{
			USImportEntryLine result = null;
			USImportEntryLine resultWithoutTariff = null;
			var entryFilerCode = importEntryNo.Left(AddInfoJobDeclaration.Schema.US_EntryFilerCodeMaxLength);
			var entryNum = importEntryNo.SubstringSafe(USAddInfoSchema.US_EntryFilerCode.MaxLength);

			var query = new ZQuery(USImportEntryLineSchema.USE_EntryNum, entryNum);
			query.FetchOnlyFromLocalCache = true; // Row matching using query with lots of 'AND' statement is slower than system comparison.

			foreach (var uSImportEntryLine in usImportEntryLineGetter(query)
				.Where(x => x.USE_EntryFilerCode == entryFilerCode && x.USE_LineNumber == drwImportDecLine && !x.USE_SupLine)
				.OrderBy(x => x.USE_ChildLineNum))
			{
				if (uSImportEntryLine.USE_CL_ParentLine.IsEmpty)
				{
					result = uSImportEntryLine;
					break;
				}

				if (!uSImportEntryLine.USE_AdValoremTariff.IsEmpty)
				{
					result = uSImportEntryLine;
					break;
				}

				if (resultWithoutTariff == null)
				{
					resultWithoutTariff = uSImportEntryLine;
				}
			}
			return result ?? resultWithoutTariff;
		}
	}
}
