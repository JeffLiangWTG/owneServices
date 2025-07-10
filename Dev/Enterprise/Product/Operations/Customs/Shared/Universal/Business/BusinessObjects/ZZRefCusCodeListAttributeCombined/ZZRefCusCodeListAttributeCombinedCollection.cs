using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusCodeListAttributeCombinedCollection : DependentBusinessObjectCollection<ZZRefCusCodeListAttributeCombined, ZZRefCusCodeListCombined>
	{
		public ZZRefCusCodeListAttributeCombinedCollection(ZZRefCusCodeListCombined cusCodeList)
			: base(cusCodeList)
		{
		}

		/// <summary>
		/// This method returns true when there is an attribute matching both name and value
		/// </summary>
		public bool HasAttribute(ZString name, ZString value)
		{
			return this.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(name) && x.ZZE_Value.EqualsIgnoringCase(value));
		}

		public bool HasAttribute(ZString name)
		{
			return this.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(name));
		}

		public ZString GetAttributeValue(ZString name)
		{
			var attribute = this.Cast<ZZRefCusCodeListAttributeCombined>().Where(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(name))?.FirstOrDefault();
			return attribute?.ZZE_Value ?? ZString.Empty;
		}

		public ZZRefCusCodeListAttributeCombined AddNew(ZString name, ZString value, ZDateTime? startDate = null, ZDateTime? endtDate = null)
		{
			var result = AddNew();
			result.ZZE_ZXE_NKName = name;
			result.ZZE_Value = value;
			result.ZZE_StartDate = startDate ?? ZDateTime.MinSmallDateTimeValue;
			result.ZZE_EndDate = endtDate ?? ZDateTime.MaxSmallDateTimeValue;
			return result;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList; }
		}

		protected override bool AllowNewCore
		{
			get { return !Master.ZZD_IsSystem; }
		}
	}
}
