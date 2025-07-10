using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	[DefaultDataSourceBindingMember(null)] //Don't automatically bind!
	public partial class CartageLegTile : ZGroupBoxTile
	{
		public CartageLegTile()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null && !typeof(CommonCartageLeg).IsAssignableFrom(dataSource.GetType()))
			{
				throw new ArgumentException("dataSource type should be CommonCartageLeg, but was " + dataSource.GetType().Name);
			}

			if (CartageLeg != null && !CartageLeg.IsDeleted)
			{
				CartageLeg.JU_E2WaitPointAddressIDInfo.ValueChanged -= new EventHandler(JU_E2WaitPointAddressIDInfo_ValueChanged);
			}

			base.SetDataBinding(dataSource, dataMember);

			if (CartageLeg != null && !CartageLeg.IsDeleted)
			{
				CartageLeg.JU_E2WaitPointAddressIDInfo.ValueChanged += new EventHandler(JU_E2WaitPointAddressIDInfo_ValueChanged);

				PositionLabels(Width, Height);
			}
		}

		void JU_E2WaitPointAddressIDInfo_ValueChanged(object sender, EventArgs e)
		{
			PositionLabels(Width, Height);
		}

		protected override float[] TileColorPositionsCore
		{
			get
			{
				float[] result;
				if (CartageLeg == null || CartageLeg.IsDeleted)
				{
					result = base.TileColorPositionsCore;
				}
				else if (CartageLeg.JU_E2WaitPointAddressID.IsValid)
				{
					result = new float[] { 0.5f, 0.75f };
				}
				else
				{
					result = new float[] { 0.67f };
				}
				return result;
			}
		}

		protected override Color[] TileColorsCore
		{
			get
			{
				Color[] result;
				if (CartageLeg == null || CartageLeg.IsDeleted)
				{
					result = base.TileColorsCore;
				}
				else if (CartageLeg.JU_E2WaitPointAddressID.IsValid)
				{
					result = new Color[3];
					result[0] = GetColour(CartageLeg.PickupStatus);
					result[1] = GetColour(CartageLeg.WaitPointStatus);
					result[2] = GetColour(CartageLeg.DeliveryStatus);
				}
				else
				{
					result = new Color[2];
					result[0] = GetColour(CartageLeg.PickupStatus);
					result[1] = GetColour(CartageLeg.DeliveryStatus);
				}

				return result;
			}
		}

		protected override float BlendDistance
		{
			get { return 0.05f; }
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore(x, y, width, height, specified);

			PositionLabels(width, height);
		}

		void PositionLabels(int width, int height)
		{
			if (!positioningLabels)
			{
				try
				{
					positioningLabels = true;

					var cartageLeg = CartageLeg;
					if (PickupOrgLabel != null && cartageLeg != null && !cartageLeg.IsDeleted)
					{
						var hasWaitPoint = cartageLeg.JU_E2WaitPointAddressID.IsValid;
						if (lastWidth != width || lastHeight != height || lastHasWaitPoint != hasWaitPoint)
						{
							using (SuspendRefresh())
							{
								//padding and stuff?
								int middle = width / 2;

								ControlDpiScalingHelper.SetTop(ref SequenceLabel, (int)BorderThickness + 2, true);
								ControlDpiScalingHelper.SetWidth(ref SequenceLabel, 34, true);
								ControlDpiScalingHelper.SetLeft(ref SequenceLabel, this.Width - SequenceLabel.Width, false);

								ControlDpiScalingHelper.SetTop(ref WhatLabel, (int)BorderThickness + 2, true);
								ControlDpiScalingHelper.SetLeft(ref WhatLabel, (int)BorderThickness, true);
								ControlDpiScalingHelper.SetWidth(ref WhatLabel, SequenceLabel.Left - WhatLabel.Left, false);

								ControlDpiScalingHelper.SetTop(ref PickupOrgLabel, WhatLabel.Top + WhatLabel.Height, false);
								ControlDpiScalingHelper.SetLeft(ref PickupOrgLabel, (int)BorderThickness, true);
								ControlDpiScalingHelper.SetWidth(ref PickupOrgLabel, middle, false);

								ControlDpiScalingHelper.SetTop(ref PickupDateLabel, PickupOrgLabel.Top, false);
								ControlDpiScalingHelper.SetLeft(ref PickupDateLabel, PickupOrgLabel.Left + PickupOrgLabel.Width, false);
								ControlDpiScalingHelper.SetWidth(ref PickupDateLabel, middle, false);

								if (hasWaitPoint)
								{
									WaitPointOrgLabel.Visible = true;
									ControlDpiScalingHelper.SetTop(ref WaitPointOrgLabel, PickupOrgLabel.Top + PickupOrgLabel.Height, false);
									ControlDpiScalingHelper.SetLeft(ref WaitPointOrgLabel, (int)BorderThickness, true);
									ControlDpiScalingHelper.SetWidth(ref WaitPointOrgLabel, middle, false);

									WaitPointDateLabel.Visible = true;
									ControlDpiScalingHelper.SetTop(ref WaitPointDateLabel, WaitPointOrgLabel.Top, false);
									ControlDpiScalingHelper.SetLeft(ref WaitPointDateLabel, WaitPointOrgLabel.Left + WaitPointOrgLabel.Width, false);
									ControlDpiScalingHelper.SetWidth(ref WaitPointDateLabel, middle, false);
								}
								else
								{
									WaitPointOrgLabel.Visible = false;
									WaitPointDateLabel.Visible = false;
								}

								ControlDpiScalingHelper.SetTop(ref DeliveryOrgLabel, hasWaitPoint ? WaitPointOrgLabel.Top + WaitPointOrgLabel.Height : PickupOrgLabel.Top + PickupOrgLabel.Height, false);
								ControlDpiScalingHelper.SetLeft(ref DeliveryOrgLabel, (int)BorderThickness, true);
								ControlDpiScalingHelper.SetWidth(ref DeliveryOrgLabel, middle, false);

								ControlDpiScalingHelper.SetTop(ref DeliveryDateLabel, DeliveryOrgLabel.Top, false);
								ControlDpiScalingHelper.SetLeft(ref DeliveryDateLabel, DeliveryOrgLabel.Left + DeliveryOrgLabel.Width, false);
								ControlDpiScalingHelper.SetWidth(ref DeliveryDateLabel, middle, false);

								ControlDpiScalingHelper.SetHeight(this, DeliveryDateLabel.Top + DeliveryDateLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);

								lastWidth = width;
								lastHeight = this.Height;
								lastHasWaitPoint = hasWaitPoint;
							}

							Refresh();
							//invok
						}
					}
				}
				finally
				{
					positioningLabels = false;
				}
			}
		}

		int lastWidth;
		int lastHeight;
		bool lastHasWaitPoint;
		bool positioningLabels;

		Color GetColour(string status)
		{
			if (status == nameof(CommonCartageLeg.LegStatuses.None))
			{
				return Color.White;
			}
			else if (status == nameof(CommonCartageLeg.LegStatuses.Dispatched))  // see LegStatus in COmmonCartageLeg.cs
			{
				return Color.FromArgb(255, 255, 192); // Light yellow
			}
			else if (status == nameof(CommonCartageLeg.LegStatuses.TimeIn))
			{
				return Color.FromArgb(192, 255, 255); // Light Aqua Blue
			}
			else if (status == nameof(CommonCartageLeg.LegStatuses.TimeOut))
			{
				return Color.FromArgb(215, 255, 215); // Light Green
			}
			else if (status == nameof(CommonCartageLeg.LegStatuses.Error))
			{
				return Color.FromArgb(255, 192, 192); // Light Red
			}
			else
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "GetColour: Leg Status {0} not supported", status));
			}
		}

		CommonCartageLeg CartageLeg
		{
			get { return (CommonCartageLeg)DataSource; }
		}
	}
}
