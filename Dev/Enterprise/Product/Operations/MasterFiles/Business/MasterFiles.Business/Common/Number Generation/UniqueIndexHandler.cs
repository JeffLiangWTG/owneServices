using System;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class UniqueIndexHandler
	{
		public UniqueIndexHandler(INumberFountainProxy numberFountain)
		{
			this.numberFountain = numberFountain;
		}

		public UniqueIndexHandler(NumberGeneratorTarget target)
		{
			if (target != null)
			{
				numberFountain = target.FountainUsedForGeneration;
				prefix = target.ValuePrefix;
				suffix = target.ValueSuffix;
				fountainValue = target.FountainValue;
				value = target.Value;
			}
		}

		readonly INumberFountainProxy numberFountain;
		readonly ZString prefix;
		readonly ZString suffix;
		readonly ZString fountainValue;
		readonly ZString value;

		public INumberFountainProxy NumberFountain
		{
			get { return numberFountain; }
		}

		public DbCommand FindMaxValueInDatabase(DbConnection connection, SchemaColumn columnSchema)
		{
			string columnReplacement = string.Format("substring({0}, {1}, {2})",
				columnSchema.Name,
				1 + prefix.Length,
				fountainValue.Length);
			string prefixPart = string.Empty;
			if (!prefix.IsEmpty)
			{
				prefixPart = string.Format((NoResString)"AND left({0}, {1}) = '{2}'",
					columnSchema.Name,
					prefix.Length,
					prefix);
			}

			string suffixPart = string.Empty;
			if (!suffix.IsEmpty)
			{
				suffixPart = string.Format("AND substring({0}, {1}, {2}) = '{3}'",
					columnSchema.Name,
					prefix.Length + fountainValue.Length + 1,
					suffix.Length,
					suffix);
			}

			string sqlText = String.Format("SELECT max({0}) FROM {1}.{2} WHERE isnumeric({0}) = 1 AND len({3}) = {4} {5} {6}",
				columnReplacement,
				columnSchema.TableSchema.SqlSchemaName,
				columnSchema.TableName,
				columnSchema.Name,
				value.Length,
				prefixPart,
				suffixPart);
			return connection.Command(sqlText);
		}
	}
}
