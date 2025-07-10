using System;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	[DebuggerDisplay("SchemaColumnName = {SchemaColumnName}, IsActive = {IsActive}")]
	public class CustomFieldInfo
	{
		#region Ctor

		CustomFieldInfo()
		{
		}

		public static CustomFieldInfo New(string caption, string hint, SchemaColumn schemaColumn, int? position = null)
		{
			return New(!string.IsNullOrWhiteSpace(caption), caption, hint, schemaColumn, position);
		}

		public static CustomFieldInfo New(bool isActive, string caption, string hint, SchemaColumn schemaColumn, int? position = null)
		{
			Argument.NotNull(schemaColumn, "schemaColumn");

			return new CustomFieldInfo
			{
				IsActive = isActive,
				Caption = caption,
				Hint = hint,
				SchemaColumnName = schemaColumn.Name,
				ZDataType = schemaColumn.GetEquivalentZType(),
				Position = position
			};
		}

		public static CustomFieldInfo New<T>(string caption, string hint, string schemaColumnName, int? position = null)
			where T : IZType
		{
			return New<T>(!string.IsNullOrWhiteSpace(caption), caption, hint, schemaColumnName, position);
		}

		public static CustomFieldInfo New<T>(bool isActive, string caption, string hint, string schemaColumnName, int? position = null)
			where T : IZType
		{
			Argument.NotNullOrEmpty(schemaColumnName, "schemaColumnName");

			return new CustomFieldInfo
			{
				IsActive = isActive,
				Caption = caption,
				Hint = hint,
				SchemaColumnName = schemaColumnName,
				ZDataType = typeof(T),
				Position = position
			};
		}

		#endregion

		public bool IsActive { get; private set; }
		public string Caption { get; private set; }
		public string Hint { get; private set; }
		public string SchemaColumnName { get; private set; }
		public Type ZDataType { get; private set; }
		public int? Position { get; private set; }
	}
}
