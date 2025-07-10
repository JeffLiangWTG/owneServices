using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module.Testing
{
	abstract class JobSailingFilterControlForTest : TestCaseWithFactory
	{
		public JobVoyageCollection Voyages => voyages ?? (voyages = GetNewVoyages());
		JobVoyageCollection voyages;

		public virtual JobVoyageCollection GetNewVoyages() => new JobVoyageCollection(Factory);

		public FilterStripBusinessObject FilterBO => filterBO ?? (filterBO = GetNewFilterStripBusinessObject());
		FilterStripBusinessObject filterBO;

		public abstract FilterStripBusinessObject GetNewFilterStripBusinessObject();

		public JobSailingFilterControl FilterControl => filterControl ?? (filterControl = GetNewJobSailingFilterControl(Voyages, FilterBO));
		JobSailingFilterControl filterControl;

		public abstract JobSailingFilterControl GetNewJobSailingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject);

		protected abstract IEnumerable<string> DateColumnStylesShouldHaveLongFormat { get; }

		protected abstract IEnumerable<string> DateColumnStylesShouldHaveShortFormat { get; }

		public void TestToEnsureAllDateColumnsAreTestedForFormat()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(FilterControl);
				form.Show();

				CombineAssertions("All date columns should be tested for their format - either long or short.", () =>
				{
					FilterControl.FilteredGrid.ColumnStyles.ToArray().Where(s => s is ZDateEditColumnStyleInfo).Cast<ZDateEditColumnStyleInfo>().ForEach(s =>
					{
						if (s.ColumnName.IndexOf("System", StringComparison.InvariantCultureIgnoreCase) < 0)
						{
							var dateColumnStyleShouldBeLong = DateColumnStylesShouldHaveLongFormat.Contains(s.ColumnName);
							var dateColumnStyleShouldBeShort = DateColumnStylesShouldHaveShortFormat.Contains(s.ColumnName);

							Assert($"Date column '{s.ColumnName}' should be tested for its format - either long or short.", dateColumnStyleShouldBeLong || dateColumnStyleShouldBeShort);
							Assert($"Date column '{s.ColumnName}' should not be tested for its format for both long and short.", !(dateColumnStyleShouldBeLong && dateColumnStyleShouldBeShort));
						}
					});
				});
			}
		}

		public virtual void TestDateEditColumnsStyleFormat()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(FilterControl);
				form.Show();

				CombineAssertions("All the date columns should show in long format.", () =>
				{
					DateColumnStylesShouldHaveLongFormat.ForEach(s => AssertDateTimeFormatIsLong(s));
				});

				CombineAssertions("All the date columns should show in short format.", () =>
				{
					DateColumnStylesShouldHaveShortFormat.ForEach(s => AssertDateTimeFormatIsShort(s));
				});
			}

			void AssertDateTimeFormatIsLong(string dateColumnName)
						=> AssertDateTimeFormat(dateColumnName, ZDateTimePickerFormat.Long);

			void AssertDateTimeFormatIsShort(string dateColumnName)
				=> AssertDateTimeFormat(dateColumnName, ZDateTimePickerFormat.Short);

			void AssertDateTimeFormat(string dateColumnName, ZDateTimePickerFormat format)
				=> AssertEquals(format, ((ZDateEditColumnStyle)FilterControl.FilteredGrid.Columns[dateColumnName].ColumnStyle).DateTimeFormat);
		}
	}
}
