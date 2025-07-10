using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	[DefaultDataSourceBindingMember(null)] //Don't automatically bind!
	public partial class RunSheetTile : ZGroupBoxTile
	{
		public RunSheetTile()
		{
			InitializeComponent();
			SetDataSourceBinding(this, "TitleBarText", "Heading", false);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null && !dataSource.GetType().IsAssignableFrom(typeof(CommonWorkSheet)))
			{
				throw new ArgumentException("dataSource type should be CommonWorkSheet, but was " + dataSource.GetType().Name);
			}

			if (RunSheet != null)
			{
				RunSheet.StatusInfo.ValueChanged -= new EventHandler(StatusInfo_ValueChanged);
				RunSheet.ActiveLegChanged -= new EventHandler(RunSheet_ActiveLegChanged);
				RunSheet.CartageLegs.CountChanged -= new EventHandler(CartageLegs_CountChanged);
				RunSheet.OrderChanged -= new EventHandler(RunSheet_OrderChanged);
			}

			base.SetDataBinding(dataSource, dataMember);

			if (RunSheet != null)
			{
				RunSheet.StatusInfo.ValueChanged += new EventHandler(StatusInfo_ValueChanged);
				RunSheet.ActiveLegChanged += new EventHandler(RunSheet_ActiveLegChanged);
				RunSheet.CartageLegs.CountChanged += new EventHandler(CartageLegs_CountChanged);
				RunSheet.OrderChanged += new EventHandler(RunSheet_OrderChanged);

				using (SuspendRefresh())
				{
					RefreshCartageLegs();
					RefreshTileColors();
				}
			}
		}

		void RefreshCartageLegs()
		{
			var legs = RunSheet.CartageLegs.ToArray();

			// WI00061320: should remove deleted CartageLegTile first, then add.
			// remove leg tiles that have been detached from the RunSheet
			var legPKs = legs.Select(l => l.PK);
			foreach (var legPK in CartageLegTiles.Keys.ToArray())
			{
				if (!legPKs.Contains(legPK))
				{
					RemoveCartageLeg(legPK);
				}
			}

			// add Leg tiles that have been attached to the RunSheet
			// refresh existing leg tiles as they details may have changed
			foreach (CommonCartageLeg leg in legs)
			{
				CartageLegTile legTile;

				if (!CartageLegTiles.TryGetValue(leg.PK, out legTile))
				{
					AddCartageLeg(leg);
				}
				else
				{
					legTile.SetDataBinding(leg, "");
				}
			}

			OrderTiles();
			ScrollToActiveLeg();
		}

		void AddCartageLeg(CommonCartageLeg leg)
		{
			var tile = new CartageLegTile();
			tile.Dock = DockStyle.Top;
			CartageLegTiles.Add(leg.PK, tile);
			BodyPanel.Controls.Add(tile);
			tile.SetDataBinding(leg, "");
		}

		void RemoveCartageLeg(ZGuid legPK)
		{
			ZGroupBoxTile tile = CartageLegTiles[legPK];
			CartageLegTiles.Remove(legPK);
			tile.Dispose();
		}

		void OrderTiles()
		{
			var legs = RunSheet.GetCartageLegsInOrder(ListSortDirection.Descending);
			for (int i = 0; i < legs.Length; i++)
			{
				CartageLegTile legTile;
				if (CartageLegTiles.TryGetValue(legs[i].PK, out legTile))
				{
					BodyPanel.Controls.SetChildIndex(legTile, i);
				}
			}
		}

		void ScrollToActiveLeg()
		{
			var activeLeg = RunSheet.PrimaryActiveLeg;
			CartageLegTile legTile;
			if (activeLeg != null && CartageLegTiles.TryGetValue(activeLeg.PK, out legTile))
			{
				BodyPanel.ScrollControlIntoView(legTile); //BodyPanel.AutoScrollPosition = new Point(x,y)
			}
		}

		void RefreshTileColors()
		{
			TileColor = GetColour(RunSheet.Status, -3);
			BorderColor = GetColour(RunSheet.Status, -5);
			PanelBorderColor = GetColour(RunSheet.Status, -4);
			TileColorGradient = GetColour(RunSheet.Status, 8);
		}

		/// <summary>
		/// Get Color
		/// </summary>
		/// <param name="status">CommonWorkSheet.RunSheetStatuses</param>
		/// <param name="lightDark">Dark: Negative, Light: Positive</param>
		/// <returns></returns>
		Color GetColour(string status, int lightDark)
		{
			Color middle;
			int step = 25;

			if (status == nameof(CommonWorkSheet.RunSheetStatuses.None))
			{
				middle = Color.Silver;
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.OK))
			{
				middle = Color.Blue;
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.NearCompletion))
			{
				middle = Color.Aqua;
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.Completed))
			{
				middle = Color.Lime;
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.Warning))
			{
				middle = Color.FromArgb(255, 172, 0); //Orange
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.LegError))
			{
				middle = Color.Red;
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.RunSheetError))
			{
				middle = Color.Red;
			}
			else
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "GetColour: Leg Status {0} not supported", status));
			}

			int red = step * lightDark + middle.R;
			red = red > 255 ? 255 : red < 0 ? 0 : red;

			int green = step * lightDark + middle.G;
			green = green > 255 ? 255 : green < 0 ? 0 : green;

			int blue = step * lightDark + middle.B;
			blue = blue > 255 ? 255 : blue < 0 ? 0 : blue;

			return Color.FromArgb(red, green, blue);
		}

		void StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshTileColors();
		}

		void RunSheet_ActiveLegChanged(object sender, EventArgs e)
		{
			ScrollToActiveLeg();
		}

		void OpenRunSheetButton_Click(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.CartageWorkSheet);
			controller.ShowEditForm(RunSheet);
		}

		void CartageLegs_CountChanged(object sender, EventArgs e)
		{
			RefreshCartageLegs();
			RefreshTileColors();
			ScrollToActiveLeg();
		}

		void RunSheet_OrderChanged(object sender, EventArgs e)
		{
			OrderTiles();
		}

		//CommonCartageLeg[] CartageLegsAscending
		//{
		//	get { return RunSheet != null ? RunSheet.GetCartageLegsInOrder(ListSortDirection.Ascending) : new CommonCartageLeg[] { }; }
		//}

		//CommonCartageLeg[] CartageLegsDescending
		//{
		//	get { return RunSheet != null ? RunSheet.GetCartageLegsInOrder(ListSortDirection.Descending) : new CommonCartageLeg[] { }; }
		//}

		CommonWorkSheet RunSheet
		{
			get { return (CommonWorkSheet)DataSource; }
		}

		internal Dictionary<ZGuid, CartageLegTile> CartageLegTiles
		{
			get { return cartageLegTiles ?? (cartageLegTiles = new Dictionary<ZGuid, CartageLegTile>()); }
		}
		Dictionary<ZGuid, CartageLegTile> cartageLegTiles;
	}
}
