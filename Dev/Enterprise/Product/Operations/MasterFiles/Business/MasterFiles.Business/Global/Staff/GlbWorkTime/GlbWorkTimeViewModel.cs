using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbWorkTimeViewModel : NonPersistentBusinessObject
	{
		readonly GlbWorkTimeCollection workTimes;

		public GlbWorkTimeViewModel(GlbWorkTimeCollection glbWorkTimeCollection, bool isReadOnly)
		{
			workTimes = glbWorkTimeCollection;
			this.IsReadOnly = isReadOnly;
		}

		[ReadOnlyMember(nameof(IsReadOnly))]
		public ZString MondayWorkingHours
		{
			get { return workTimes.MondayWorkingHours; }
			set { workTimes.MondayWorkingHours = value; }
		}

		[ReadOnlyMember(nameof(IsReadOnly))]
		public ZString TuesdayWorkingHours
		{
			get { return workTimes.TuesdayWorkingHours; }
			set { workTimes.TuesdayWorkingHours = value; }
		}

		[ReadOnlyMember(nameof(IsReadOnly))]
		public ZString WednesdayWorkingHours
		{
			get { return workTimes.WednesdayWorkingHours; }
			set { workTimes.WednesdayWorkingHours = value; }
		}

		[ReadOnlyMember(nameof(IsReadOnly))]
		public ZString ThursdayWorkingHours
		{
			get { return workTimes.ThursdayWorkingHours; }
			set { workTimes.ThursdayWorkingHours = value; }
		}

		[ReadOnlyMember(nameof(IsReadOnly))]
		public ZString FridayWorkingHours
		{
			get { return workTimes.FridayWorkingHours; }
			set { workTimes.FridayWorkingHours = value; }
		}

		[ReadOnlyMember(nameof(IsReadOnly))]
		public ZString SaturdayWorkingHours
		{
			get { return workTimes.SaturdayWorkingHours; }
			set { workTimes.SaturdayWorkingHours = value; }
		}

		[ReadOnlyMember(nameof(IsReadOnly))]
		public ZString SundayWorkingHours
		{
			get { return workTimes.SundayWorkingHours; }
			set { workTimes.SundayWorkingHours = value; }
		}

		public ZBool IsReadOnly { get; private set; }
	}
}
