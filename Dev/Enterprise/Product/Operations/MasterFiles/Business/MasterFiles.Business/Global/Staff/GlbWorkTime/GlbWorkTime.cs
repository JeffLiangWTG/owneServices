using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbWorkTime : AutoGlbWorkTime
	{
		public GlbWorkTime(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZDateTime GW_StartTime
		{
			get => base.GW_StartTime;
			set
			{
				var correctedStartTime = value;

				if (value < new ZDateTime(1900, 1, 1) || value >= new ZDateTime(1900, 1, 2))
				{
					correctedStartTime = new ZDateTime(1900, 1, 1, value.Hour, value.Minute, 0);
				}

				base.GW_StartTime = correctedStartTime;

				if (!IsValidationSuspended)
				{
					Validation.ValidateGW_EndTime();
				}
			}
		}

		public override ZDateTime GW_EndTime
		{
			get => base.GW_EndTime;
			set
			{
				var correctedEndTime = value;

				if (value < new ZDateTime(1900, 1, 1) || value >= new ZDateTime(1900, 1, 3))
				{
					correctedEndTime = new ZDateTime(1900, 1, 1, value.Hour, value.Minute, 0);
				}

				base.GW_EndTime = correctedEndTime;

				if (!IsValidationSuspended)
				{
					Validation.ValidateGW_StartTime();
				}
			}
		}

		public static ZDateTime CreateTime(int hour, int minute, bool isNextDay = false)
		{
			var day = isNextDay ? 2 : 1;
			return new ZDateTime(1900, 1, day, hour, minute, 0);
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			GW_StartTime = CreateTime(8, 30);
			GW_EndTime = CreateTime(18, 0);
			GW_DayOfWeek = "MON";
			GW_ParentTableCode = "GS";
		}

#endif
	}
}
