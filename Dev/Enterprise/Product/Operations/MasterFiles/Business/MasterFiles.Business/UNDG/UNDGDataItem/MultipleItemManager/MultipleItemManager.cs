using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.MultipleItemExtensions;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MultipleItemManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static string Many
		{
			get
			{
				return Res.GetString("852ca680-10e7-478a-a207-e830cffe7abb", "Many");
			}
		}

		public MultipleItemManager(IBusinessObjectCollection collection, SchemaColumn column, ZBool hasList, ZBool useNaturalKey)
		{
			this.Collection = collection;
			this.Column = column;
			this.hasList = hasList;
			this.useNaturalKey = useNaturalKey;
		}

		public MultipleItemManager(IBusinessObjectCollection collection, SchemaColumn column, ZBool hasList)
			: this(collection, column, hasList, false)
		{ }

		public IBusinessObjectCollection Collection { get; private set; }
		readonly SchemaColumn Column;

		#region Value

		public ZString Value
		{
			get
			{
				if (Collection.Count == 0)
				{
					return (ZString)ZDataType.ZTypeToEmptyValue(typeof(ZString));
				}
				else if (Collection.Count == 1)
				{
					return GetItemValue((BusinessObject)Collection[0]);
				}
				else
				{
					return Many;
				}
			}
			set
			{
				if (Many.Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
				if (Collection.Count == 1)
				{
					BusinessObject item = (BusinessObject)Collection[0];
					SetItemValue(item, value);
					if (item.ItemIsEmpty())
					{
						item.Delete();
					}
				}
				else if (Collection.Count == 0 && value != (ZString)ZDataType.ZTypeToEmptyValue(Column.GetEquivalentZType()).ToString())
				{
					BusinessObject item = Collection.AddNew();
					SetItemValue(item, value);
				}

				ValueInfo.RefreshBinding();
			}
		}

		void SetItemValue(BusinessObject item, ZString value)
		{
			if (Column.ColumnType == SchemaColumnType.String)
			{
				item[Column] = value;
			}
			else if (Column.ColumnType == SchemaColumnType.Decimal)
			{
				ZDecimal convertedValue = ZDecimal.Zero;
				try
				{
					convertedValue = ZDecimal.Parse(value, Culture.CurrentCompanyCountryCulture.NumberFormat);
				}
				catch (ArgumentNullException) { }
				catch (FormatException) { }
				catch (OverflowException) { }
				item[Column] = convertedValue;
			}
			else if (Column.ColumnType == SchemaColumnType.Int)
			{
				ZInt convertedValue;
				if (!ZInt.TryParse(value, out convertedValue))
				{
					convertedValue = ZInt.Zero;
				}
				item[Column] = convertedValue;
			}
			else if (Column.ColumnType == SchemaColumnType.Guid)
			{
				ZGuid convertedValue;
				if (ZGuid.TryParse(value, out convertedValue))
				{
					item[Column] = convertedValue;
				}
			}
			else if (Column.ColumnType == SchemaColumnType.Bool)
			{
				ZBool convertedValue;
				if (ZBool.TryParse(value, out convertedValue))
				{
					item[Column] = convertedValue;
				}
			}
			else if (Column.ColumnType == SchemaColumnType.Date)
			{
				ZDate convertedValue;
				try
				{
					convertedValue = (ZDate)Convert.ToDateTime(value, Culture.CurrentCompanyCountryCulture).Date;
				}
				catch (FormatException)
				{
					return;
				}
				if (convertedValue.IsValid)
				{
					item[Column] = convertedValue;
				}
			}
		}

		string GetItemValue(BusinessObject item)
		{
			string result;
			if (Column.ColumnType == SchemaColumnType.Decimal)
			{
				result = ((ZDecimal)item[Column]).ToString(null, Culture.CurrentCompanyCountryCulture);
			}
			else
			{
				result = item[Column].ToString();
			}

			return result;
		}

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(nameof(Value)); }
		}

		public int Value_MaxLength
		{
			get { return Column != null ? Column.MaxLength : 20; }
		}

		public bool Value_ReadOnly
		{
			get { return (ReadOnlyWhenMany && Collection.Count > 1) || ReadOnlyFunction(); }
		}

		public bool ReadOnlyWhenMany { get; set; }

		public Func<bool> ReadOnlyFunction
		{
			get => readOnlyFunction;
			set => readOnlyFunction = value;
		}
		Func<bool> readOnlyFunction = () => false;

		#endregion

		#region Field Type

		public ZString FieldColumnType
		{
			get
			{
				FieldType result = FieldType.Text;
				if (Collection.Count > 1)
				{
					result = FieldType.LinkLabel;
				}
				else
				{
					if (Column.ColumnType == SchemaColumnType.String)
					{
						if (!hasList)
						{
							result = FieldType.Text;
						}
						else if (useNaturalKey)
						{
							result = FieldType.TextDropEdit;
						}
						else
						{
							result = FieldType.TextCodeFindBox;
						}
					}
					else if (Column.ColumnType == SchemaColumnType.Int)
					{
						result = FieldType.Integer;
					}
					else if (Column.ColumnType == SchemaColumnType.Decimal)
					{
						result = FieldType.Decimal;
					}
					else if (Column.ColumnType == SchemaColumnType.Guid)
					{
						result = FieldType.Guid;
					}
					else if (Column.ColumnType == SchemaColumnType.Bool)
					{
						result = FieldType.Boolean;
					}
					else if (Column.ColumnType == SchemaColumnType.Date)
					{
						result = FieldType.Date;
					}
				}

				return result.ToString();
			}
		}

		readonly ZBool hasList;

		readonly ZBool useNaturalKey;

		#endregion
	}
}
