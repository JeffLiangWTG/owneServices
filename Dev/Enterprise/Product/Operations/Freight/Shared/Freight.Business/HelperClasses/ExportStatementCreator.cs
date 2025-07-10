using System;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public interface IExportStatement
	{
		ZString GetExportStatement(ExportStatementSetting exportStatementSetting);
	}

	public class ExportStatementCreator
	{
		public ExportStatementCreator(ExportStatementSetting exportStatementSetting)
		{
			if (exportStatementSetting == null)
			{
				throw new ArgumentNullException("ExportStatementSetting");
			}
			this.ExportStatementSetting = exportStatementSetting;
		}

		public ZString ExportStatement
		{
			get { return ExportStatementCore; }
		}

		protected virtual ZString ExportStatementCore
		{
			get
			{
				ZString result = ExportStatementSetting.Statement;
				ZString countryCode = ExportStatementSetting.Parent.CountryCode;
				if (!ExportStatementSetting.Field1.IsEmpty)
				{
					result += GetFieldSeparator(countryCode) + GetStatementFieldTypeMessage(ExportStatementSetting.Field1, countryCode);
				}

				if (!ExportStatementSetting.Field2.IsEmpty)
				{
					result += GetFieldSeparator(countryCode) + GetStatementFieldTypeMessage(ExportStatementSetting.Field2, countryCode);
				}
				return result;
			}
		}

		public ZString ExportStatementFields
		{
			get
			{
				var countryCode = ExportStatementSetting.Parent.CountryCode;
				return ZString.Join(GetFieldSeparator(countryCode), new ZString[] { GetStatementFieldTypeMessage(ExportStatementSetting.Field1, countryCode), GetStatementFieldTypeMessage(ExportStatementSetting.Field2, countryCode) });
			}
		}

		protected virtual string GetFieldSeparator(string countryCode)
		{
			return "-";
		}

		protected virtual ZString GetStatementFieldTypeMessage(ZString fieldType, string countryCode)
		{
			return fieldType;
		}

		protected readonly ExportStatementSetting ExportStatementSetting;
	}
}
