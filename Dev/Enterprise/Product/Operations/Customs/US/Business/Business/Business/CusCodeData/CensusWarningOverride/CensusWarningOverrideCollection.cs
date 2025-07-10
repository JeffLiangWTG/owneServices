using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class CensusWarningOverrideCollection : Customs.Business.CusCodeDataCollection<CensusWarningOverride>
	{
		public CensusWarningOverrideCollection(BusinessObject parent)
			: base(parent, CusCodeDataTypeList.Codes.CensusWarningOverride)
		{
		}

		internal void CopyFrom(List<EntryCensusWarningOverride> passedList)
		{
			List<CensusWarningOverride> thisList = new List<CensusWarningOverride>(new TypedEnumerable<CensusWarningOverride>(this));

			foreach (var passed in passedList)
			{
				if (!passed.ConditionCode.IsEmpty)
				{
					var elements = thisList.FindAll(x => x.CY_Code == passed.ConditionCode);

					if (elements.Count == 0)
					{
						var newElement = AddNew();
						newElement.CY_Code = passed.ConditionCode;
						newElement.CY_Data = passed.OverrideCode;
					}
					else
					{
						foreach (var element in elements)
						{
							element.CY_Data = passed.OverrideCode;
						}

						elements.ForEach(x => thisList.Remove(x));
					}
				}
			}

			thisList.ForEach(x => x.Delete());
		}
	}
}
