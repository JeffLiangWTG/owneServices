
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NonDependentNZCClassificationCollection : BusinessObjectCollection<NZCClassification>
	{
		public NonDependentNZCClassificationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public NonDependentNZCClassificationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		#region Indexer
		#endregion

		#region AddNew
		#endregion

		#region Load
		public void Load(ZString tariffCode)
		{
			Load(new ZQuery(NZCClassificationSchema.U0_Tariff, tariffCode));
			fDateRange = null;
		}
		#endregion

		#region DateRange

		public DateRangeGetter DateRange
		{
			get
			{
				if (fDateRange == null)
				{
					fDateRange = new DateRangeGetter(this);
				}
				return fDateRange;
			}
		}
		DateRangeGetter fDateRange;

		public class DateRangeGetter
		{
			public DateRangeGetter(NonDependentNZCClassificationCollection classifications)
			{
				DateActiveFrom = ZDateTime.Empty;
				DateActiveTo = ZDateTime.Empty;
				foreach (NZCClassification classification in classifications)
				{
					if (classification.U0_DateActiveFrom < DateActiveFrom || DateActiveFrom.IsEmpty)
					{
						DateActiveFrom = classification.U0_DateActiveFrom.Date;
					}
					if (classification.U0_DateActiveTo.IsEmpty)
					{
						DateActiveTo = new ZDateTime(3000, 12, 31);
					}
					else if (classification.U0_DateActiveTo > DateActiveTo || DateActiveTo.IsEmpty)
					{
						DateActiveTo = classification.U0_DateActiveTo.Date;
					}
				}
			}
			public readonly ZDateTime DateActiveFrom;
			public readonly ZDateTime DateActiveTo;
		}
		#endregion
	}
}
