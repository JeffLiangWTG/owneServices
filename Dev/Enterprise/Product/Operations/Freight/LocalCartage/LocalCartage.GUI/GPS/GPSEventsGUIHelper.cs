using System.Drawing;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.GPS;
using Enterprise.GPS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI.GPS
{
	public class GPSEventsGUIHelper
	{
		public readonly Color ColourForUsedGPSEvent = Color.DarkSeaGreen;
		public readonly Color ColourForUnusedGPSEvent = Color.IndianRed;
		public readonly Color ColourForOverwrittenGPSEvent = Color.LightGoldenrodYellow;

		public GPSEventsGUIHelper(CommonWorkSheet worksheet)
		{
			Master = worksheet;
		}
		CommonWorkSheet Master { get; set; }

		public Color GetEventColour(GPSEvent gpsEvent)
		{
			Color result = Color.FromArgb(0);
			if (gpsEvent.EventLeg == null)
			{
				result = ColourForUnusedGPSEvent;
			}
			else if (Master.CartageLegs.Contains(gpsEvent.EventLeg))
			{
				if (GPSEventMatchesLegTime(gpsEvent.EventLeg, gpsEvent, ""))
				{
					result = ColourForUsedGPSEvent;
				}
				else
				{
					result = ColourForOverwrittenGPSEvent;
				}
			}

			return result;
		}

		internal void GetEventFont(FontDecidingEventArgs e, Font defaultFont, ZGrid gPSLegsGrid, bool gpsPanelVisible)
		{
			if (!gpsPanelVisible)
			{
				e.Font = defaultFont;
			}
			else if (e.DataMember == "EventTime")
			{
				GPSEvent evnt = e.ObjectAtRow as GPSEvent;
				if (evnt != null)
				{
					var current = gPSLegsGrid.ListManager.GetCurrent() as CommonCartageLeg;

					if (gPSLegsGrid.SelectedElements.Length == 1 && current != null)
					{
						if (evnt.EventLeg != null && current != null && EventIsForLeg(current, evnt, ""))
						{
							Font cellFont = new Font(defaultFont, FontStyle.Bold | FontStyle.Underline);
							e.Font = cellFont;
						}
					}
					else
					{
						foreach (CommonCartageLeg leg in gPSLegsGrid.SelectedElements)
						{
							if (EventIsForLeg(leg, evnt, ""))
							{
								Font cellFont = new Font(defaultFont, FontStyle.Bold | FontStyle.Underline);
								e.Font = cellFont;
								break;
							}
						}
					}
				}
			}
		}

		internal void GetLegFont(FontDecidingEventArgs e, Font defaultFont, ZGrid gPSEventsGrid, bool gpsPanelVisible)
		{
			CommonCartageLeg leg = e.ObjectAtRow as CommonCartageLeg;

			if (!gpsPanelVisible)
			{
				e.Font = defaultFont;
				return;
			}
			bool cellIsATimeCell = (e.DataMember.Contains("TimeIn") || e.DataMember.Contains("TimeOut"));

			GPSEvent eventThatUpdatedSelectedLeg = null;

			if (cellIsATimeCell)
			{
				foreach (GPSEvent gpsEvent in Master.GPSEvents)
				{
					if (EventIsForLeg(leg, gpsEvent, e.DataMember))
					{
						eventThatUpdatedSelectedLeg = gpsEvent;
						break;
					}
				}

				var listManager = gPSEventsGrid.ListManager;
				if (eventThatUpdatedSelectedLeg != null && listManager != null && listManager.GetCurrent() != null)
				{
					e.Font = new Font(defaultFont, FontStyle.Bold);

					GPSEvent singleSelectedEvent = gPSEventsGrid.ListManager.GetCurrent() as GPSEvent;

					if (eventThatUpdatedSelectedLeg != null && singleSelectedEvent != null && EventIsForLeg(leg, singleSelectedEvent, e.DataMember))
					{
						var legTimeEqualsEvent = cellIsATimeCell && GPSEventMatchesLegTime(leg, singleSelectedEvent, e.DataMember);

						if (legTimeEqualsEvent)
						{
							var sameInOut = IsSameInOut(e, singleSelectedEvent);
							if (sameInOut)
							{
								e.Font = new Font(defaultFont, FontStyle.Bold | FontStyle.Underline);
							}
						}
					}
				}
			}
		}

		bool IsSameInOut(FontDecidingEventArgs e, GPSEvent singleSelectedEvent)
		{
			var sameInOut = e.DataMember.Contains("TimeIn") && (
				singleSelectedEvent.EventTypeCode.Equals(GPSConstants.GPSInOutActivityType.Codes.GIN));

			sameInOut |= e.DataMember.Contains("TimeOut") && (
				singleSelectedEvent.EventTypeCode.Equals(GPSConstants.GPSInOutActivityType.Codes.GOT));

			return sameInOut;
		}

		bool EventIsForLeg(CommonCartageLeg leg, GPSEvent gpsEvent, string dateColumn)
		{
			if (gpsEvent.EventLeg != null && gpsEvent.EventLeg.PK == leg.PK && GPSEventMatchesLegTime(leg, gpsEvent, dateColumn))
			{
				return true;
			}
			else
			{
				foreach (CommonCartageLeg groupLeg in leg.OtherLegsInSameGroup())
				{
					if (gpsEvent.EventLeg != null && gpsEvent.EventLeg.PK == groupLeg.PK && GPSEventMatchesLegTime(groupLeg, gpsEvent, dateColumn))
					{
						return true;
					}
				}
				return false;
			}
		}

		bool GPSEventMatchesLegTime(CommonCartageLeg leg, GPSEvent gpsEvent, string legDateColumn)
		{
			var eventTime = gpsEvent.EventTime;
			var eventIgnoreSeconds = GetShortTime(eventTime);
			var result = false;

			if (!legDateColumn.IsNullOrEmpty())
			{
				if (GetShortTime((ZDateTime)leg[legDateColumn]) == eventIgnoreSeconds)
				{
					result = true;
				}
			}
			else
			{
				if (gpsEvent.EventTypeCode == GPSConstants.GPSInOutActivityType.Codes.GIN)
				{
					result = GetShortTime(leg.JU_PickupTimeIn) == eventIgnoreSeconds || GetShortTime(leg.JU_WaitPointTimeIn) == eventIgnoreSeconds || GetShortTime(leg.JU_DeliverTimeIn) == eventIgnoreSeconds;
				}
				else if (gpsEvent.EventTypeCode == GPSConstants.GPSInOutActivityType.Codes.GOT)
				{
					result = GetShortTime(leg.JU_PickupTimeOut) == eventIgnoreSeconds || GetShortTime(leg.JU_WaitPointTimeOut) == eventIgnoreSeconds || GetShortTime(leg.JU_DeliverTimeOut) == eventIgnoreSeconds;
				}
			}

			return result;
		}

		ZDateTime GetShortTime(ZDateTime timeWithSeconds)
		{
			return !timeWithSeconds.IsValid ? ZDateTime.Empty : new ZDateTime(timeWithSeconds.Year, timeWithSeconds.Month, timeWithSeconds.Day, timeWithSeconds.Hour, timeWithSeconds.Minute, 0);
		}
	}
}
