using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBWAAddInfo : NonPersistentBusinessObject
	{
		public WhsBWAAddInfo(string key, string value)
			: base()
		{
			Argument.NotNullOrEmpty(key, "key");
			Argument.NotNull(value, "value");

			this.keyString = key;
			this.valueString = value;
		}

		public abstract class Schema
		{
			public const string Key = "KeyString";
			public const string Value = "ValueString";
		}

		#region Properties

		[BusinessObjectTestExclude]
		public ZString KeyString
		{
			get { return keyString; }
		}
		readonly ZString keyString;

		public ZPropertyInfo KeyStringInfo
		{
			get { return GetZPropertyInfo(Schema.Key); }
		}

		[BusinessObjectTestExclude]
		public ZString ValueString
		{
			get { return valueString; }
		}
		readonly ZString valueString;

		public ZPropertyInfo ValueStringInfo
		{
			get { return GetZPropertyInfo(Schema.Value); }
		}

		#endregion
	}
}
