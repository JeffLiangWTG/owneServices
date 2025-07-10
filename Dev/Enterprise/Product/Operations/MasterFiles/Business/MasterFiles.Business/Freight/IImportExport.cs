using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Implementated on business objects that relate to imports or exports.
	/// </summary>
	public interface IImportExport
	{
		Directions JobDirection { get; }
	}

	public static class IImportExportExtensions
	{
		public static ZBool IsImport(this IImportExport impExp)
		{
			return impExp.JobDirection == Directions.Import;
		}

		public static ZBool IsExport(this IImportExport impExp)
		{
			return impExp.JobDirection == Directions.Export;
		}

		public static ZBool IsCrossTrade(this IImportExport impExp)
		{
			return impExp.JobDirection == Directions.CrossTrade;
		}

		public static ZBool IsDomestic(this IImportExport impExp)
		{
			return impExp.JobDirection == Directions.Domestic;
		}

		public static ZBool IsUnknown(this IImportExport impExp)
		{
			return impExp.JobDirection == Directions.Unknown;
		}
	}

	public enum Directions
	{
		Unknown,    //default
		Import,
		Export,
		Domestic,
		CrossTrade
	}
}
