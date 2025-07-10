using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class DefaultOrgTimetableCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new DefaultOrgTimetable this[int x]
		{
			get { return (DefaultOrgTimetable)Elements[x]; }
		}

		public DefaultOrgTimetableCollection()
			: base()
		{
		}

		public DefaultOrgTimetableCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DefaultOrgTimetableCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		internal DefaultOrgTimetable AddNewWithProperty(string type, ZDateTime from, ZDateTime to, string day, DefaultOrgTimetableCollection parent)
		{
			var newItem = new DefaultOrgTimetable();
			newItem.Type = type;
			newItem.From = from;
			newItem.To = to;
			newItem.Day = day;
			newItem.SetParent(parent);
			Add(newItem);
			return newItem;
		}

		public new DefaultOrgTimetable AddNew()
		{
			var newItem = (DefaultOrgTimetable)base.AddNew();
			newItem.SetParent(this);
			return newItem;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var newItem = new DefaultOrgTimetable(CurrentFallbackLevel, CurrentFactory);
			newItem.SetParent(this);
			return newItem;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultOrgTimetableCollection(fallbackLevel, factory);
		}

		protected override void PerformPostCloneAction(IRegistryBusiness registryBusiness)
		{
			var parent = (DefaultOrgTimetableCollection)registryBusiness;

			foreach (DefaultOrgTimetable element in registryBusiness.OfType<DefaultOrgTimetable>())
			{
				element.SetParent(parent);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == "Day")
			{
				return new DefaultOrgTimetableDayOfWeekComparer(property, direction);
			}
			return base.GetComparerForSort(property, direction);
		}

		internal DefaultOrgTimetableSettings ParentSettings
		{
			get;
			set;
		}

		class DefaultOrgTimetableDayOfWeekComparer : PropertyComparer
		{
			internal DefaultOrgTimetableDayOfWeekComparer(PropertyDescriptor property, ListSortDirection direction)
				: base(property, direction)
			{
			}

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				DefaultOrgTimetable data1 = (DefaultOrgTimetable)x;
				DefaultOrgTimetable data2 = (DefaultOrgTimetable)y;

				int result = new DefaultOrgTimetableComparer().Compare(data1, data2);
				if (Direction == System.ComponentModel.ListSortDirection.Ascending)
				{
					return result;
				}
				else
				{
					return result * -1;
				}
			}
		}
	}

	public class DefaultOrgTimetableComparer : IComparer<DefaultOrgTimetable>, IComparer
	{
		public int Compare(DefaultOrgTimetable timetable1, DefaultOrgTimetable timetable2)
		{
			int result = 0;
			DayOfWeek day1, day2;
			if (DayOfWeekCodeList.Mappping.TryGetValue(timetable1.Day.ToString(), out day1) && DayOfWeekCodeList.Mappping.TryGetValue(timetable2.Day.ToString(), out day2))
			{
				result = day1.CompareTo(day2);
			}
			if (result == 0)
			{
				result = timetable1.Type.CompareTo(timetable2.Type);
			}
			if (result == 0)
			{
				result = timetable1.From.CompareTo(timetable2.From);
			}
			return result;
		}

		int IComparer.Compare(object x, object y)
		{
			var timeTable1 = (DefaultOrgTimetable)x;
			var timeTable2 = (DefaultOrgTimetable)y;

			return Compare(timeTable1, timeTable2);
		}
	}
}
