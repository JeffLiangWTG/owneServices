using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Universal
{
	[CodeAlive("Under Development")]
	public class RefCusCodeListAttributeCollection : ActiveBusinessObjectCollection<RefCusCodeListAttribute>
	{
		public RefCusCodeListAttributeCollection(RefCusCodeList parent)
			: base(parent.Factory, parent, new ZQuery(), RefCusCodeListAttributeSchema.ZZE_ZZD_CodeList)
		{
		}

		public RefCusCodeListAttribute AddNew(ZString name, ZString value)
		{
			var result = this.FirstOrDefault(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(name) && x.ZZE_Value.EqualsIgnoringCase(value));
			if (result == null)
			{
				result = AddNew();
				result.ZZE_ZXE_NKName = name;
				result.ZZE_Value = value;
			}

			return result;
		}

		public RefCusCodeListAttribute AddNew(ZString name, ZString value, ZDateTime startDate, ZDateTime endDate)
		{
			var result = this.FirstOrDefault(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(name)
												  && x.ZZE_Value.EqualsIgnoringCase(value)
												  && x.ZZE_StartDate == startDate
												  && x.ZZE_EndDate == endDate);
			if (result == null)
			{
				result = AddNew();
				result.ZZE_ZXE_NKName = name;
				result.ZZE_Value = value;
				result.ZZE_StartDate = startDate;
				result.ZZE_EndDate = endDate;
			}

			return result;
		}
	}
}
