using System.ComponentModel;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageLegPlanner : AutoCartageLegPlanner
	{
		public CartageLegPlanner(BusinessObjectFactory factory)
			: base(factory) { }

		public static class Schema
		{
			public const string DriversWorkSheetSidePanelMode = "DriversWorkSheetSidePanelMode";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DriversWorkSheetSidePanelMode = CartageLegPlanner.DriversWorkSheetSidePanelMode_Drivers;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("3784f6ca-5ce7-4413-b5ba-722d712d0290", "Port Transport Leg Planner"); }
		}

		[ChildEditable()]
		public FilteredCartageLegCollection CartageLegs
		{
			get
			{
				if (cartageLegs == null)
				{
					cartageLegs = new FilteredCartageLegCollection(Factory);
					RegisterEditableChildObject(cartageLegs);
				}
				return cartageLegs;
			}
		}
		FilteredCartageLegCollection cartageLegs;

		public StaffDriverCollection Drivers
		{
			get
			{
				if (fDrivers == null)
				{
					fDrivers = new StaffDriverCollection(Factory);
				}

				return fDrivers;
			}
		}
		StaffDriverCollection fDrivers;

		public RefEquipmentCollection Vehicles
		{
			get
			{
				if (fVehicles == null)
				{
					fVehicles = new RefEquipmentCollection(Factory);
					fVehicles.AdditionalFilter = new ZQuery(RefEquipmentSchema.RQ_IsVehicle, true);
				}

				return fVehicles;
			}
		}
		RefEquipmentCollection fVehicles;

		public CommonWorkSheetCollection WorkSheets
		{
			get
			{
				if (fWorkSheets == null)
				{
					fWorkSheets = new CommonWorkSheetCollection(Factory);
					fWorkSheets.AdditionalFilter = ZQuery.NoResultQuery;
					RegisterEditableChildObject(fWorkSheets);
				}

				return fWorkSheets;
			}
		}
		CommonWorkSheetCollection fWorkSheets;

		[List("DriversWorkSheetSidePanelModes")]
		[MaxLength(25)]
		public ZString DriversWorkSheetSidePanelMode
		{
			get
			{
				return fDriversWorkSheetSidePanelMode;
			}
			set
			{
				CheckMaximumLength(DriversWorkSheetSidePanelModeInfo, value);
				fDriversWorkSheetSidePanelMode = value;

				if (value.StartsWith(CartageLegPlanner.DriversWorkSheetsSidePanelMode_WorkSheet, System.StringComparison.CurrentCulture))
				{
					ZDateTime workSheetDay;
					ZDateTime.TryParseExact(DriversWorkSheetSidePanelModes.GetDescriptionFromCode(value), out workSheetDay, ZDateTime.LongTimeFormat);

					var daysOffset = workSheetDay.IsValid ? (workSheetDay.Date - ZDateTime.Today.Date).Days : 0;
					var from = ZDateTime.Today.AddDays(daysOffset);
					var to = ZDateTime.Today.AddDays(daysOffset + 1);

					var query = new ZQuery(JobCartageRunSheetSchema.EY_EndTime, SQLComparisonOperator.GreaterThan, from);
					query.AddToFilter(JobCartageRunSheetSchema.EY_StartTime, SQLComparisonOperator.LessThan, to);

					WorkSheets.AdditionalFilter = query;
				}

				DriversWorkSheetSidePanelModeInfo.RefreshBinding();
			}
		}
		ZString fDriversWorkSheetSidePanelMode;

		public ZPropertyInfo DriversWorkSheetSidePanelModeInfo
		{
			get { return GetZPropertyInfo(Schema.DriversWorkSheetSidePanelMode); }
		}

		public CodeDescriptionPairList DriversWorkSheetSidePanelModes
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();

				result.AddPair(DriversWorkSheetSidePanelMode_Drivers, "");
				result.AddPair(DriversWorkSheetSidePanelMode_Vehicles, "");

				for (int i = -1; i <= 7; i++)
				{
					result.AddPair(DriversWorkSheetsSidePanelMode_WorkSheet + " " + GetDay(i), ZDateTime.Today.AddDays(i).ToString(ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture));
				}

				return result;
			}
		}

		public static ZString DriversWorkSheetSidePanelMode_Drivers { get { return Res.GetString("392d0d8f-94cd-472d-aa27-1c5daa45f065", "Drivers"); } }
		public static ZString DriversWorkSheetsSidePanelMode_WorkSheet { get { return Res.GetString("e3598bd7-c4e8-4758-9578-d61eff19569c", "Run Sheets -"); } }
		public static ZString DriversWorkSheetSidePanelMode_Vehicles { get { return Res.GetString("dea8d0df-a3e1-4dcc-b8ba-e84991e44723", "Vehicles"); } }

		public void SwapWith(CartageLegPlanner oldLegPlanner)
		{
			SwapCollection(oldLegPlanner.CartageLegs, CartageLegs);
			SwapCollection(oldLegPlanner.Drivers, Drivers);
			SwapCollection(oldLegPlanner.Vehicles, Vehicles);
			SwapCollection(oldLegPlanner.WorkSheets, WorkSheets);

			DriversWorkSheetSidePanelMode = oldLegPlanner.DriversWorkSheetSidePanelMode;
		}

		void SwapCollection(IBindingList oldList, IBindingList newList)
		{
			if (oldList.SortProperty != null)
			{
				newList.ApplySort(oldList.SortProperty, oldList.SortDirection);
			}
		}

		string GetDay(int i)
		{
			if (i == -1)
			{
				return Res.GetString("C5428FA4-3BBE-4592-8131-42BBF3B51655", "Yesterday");
			}
			else if (i == 0)
			{
				return Res.GetString("f0eeccc1-cd0c-419c-974d-1212d64be8d6", "Today");
			}
			else if (i == 1)
			{
				return Res.GetString("fd72de09-cda6-4b60-baf3-4a7f282d5454", "Tomorrow");
			}
			else
			{
				return ZDateTime.Today.AddDays(i).ToString("dddd", CultureInfo.CurrentCulture);
			}
		}
	}
}
