using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.GUI.Forms
{
	public partial class GlbDeviceForm : ZTemplateForm
	{
		public GlbDeviceForm(GlbDevice device)
			: this(device, withUIUpdateTimer: true)
		{
		}

		public GlbDeviceForm(GlbDevice device, bool withUIUpdateTimer)
			: base(device)
		{
			lazyMostRecentLocation = new Lazy<GlbDeviceLocation>(GetMostRecentDeviceLocation);

			InitializeComponent();

			UpdateLocationInfo();

			if (withUIUpdateTimer)
			{
				locationInfoTimer.Start();
			}
		}

		readonly Lazy<GlbDeviceLocation> lazyMostRecentLocation;

		public string LocationInfoText => locationInfoLabel.Text;

		#region ZForm

		protected override bool AllowNew => false;

		public override string FormCaption => BusinessEntity.HumanReadableName;

		protected override void OnClosed(EventArgs e)
		{
			locationInfoTimer?.Dispose();
			base.OnClosed(e);
		}

		#endregion

		#region ZTemplateForm

		protected override bool SupportsEDocs => false;

		#endregion

		GlbDeviceLocation GetMostRecentDeviceLocation()
		{
			var device = (GlbDevice)BusinessEntity;

			var locationPks = new DynamicBusinessObjectCollection(BusinessEntity.Factory);
			locationPks.Load($@"
SELECT TOP 1
	{GlbDeviceLocationSchema.Constants.PK}
FROM
	{GlbDeviceLocationSchema.Constants.SqlSchemaName}.{GlbDeviceLocationSchema.Constants.TableName}
WHERE
	{GlbDeviceLocationSchema.Constants.V2_V3_Device} = @deviceId
ORDER BY
	{GlbDeviceLocationSchema.Constants.V2_MeasurementTimeUtc} DESC
",
				new[]
				{
					ZSqlParameter.New("@deviceId", device.PK, GlbDeviceLocationSchema.V2_V3_Device),
				});

			var locationPk = new ZGuid(locationPks
				.Select(dbo => dbo[GlbDeviceLocationSchema.Constants.PK])
				.SingleOrDefault());

			return locationPk.IsEmpty
				? null
				: BusinessEntity.Factory.Load<GlbDeviceLocation>(locationPk);
		}

		void OnClearAssignmentButtonClick(object sender, EventArgs e)
		{
			var device = (GlbDevice)BusinessEntity;
			if (device.IsRimRegistered)
			{
				Globals.Message.Show(Res.GetString("F6E626FF-9C89-440C-9D27-7D2CBB95E0D7", "Device is registered to a vehicle in the TCA RIM program. Cannot change assignment details"));
				return;
			}
			device.ClearAssignedParent();
		}

		void UpdateLocationInfo()
		{
			var location = lazyMostRecentLocation.Value;
			var mostRecentLocationTime = location?.V2_MeasurementTimeUtc;

			if (!mostRecentLocationTime.HasValue)
			{
				locationInfoLabel.Text = Res.GetString("9148466A-264A-45BC-A025-8A7C32A90768", "This device has not reported location data.");
			}
			else
			{
				var age = ZDateTime.UtcNow - mostRecentLocationTime.Value;

				if (age.Days > 1)
				{
					locationInfoLabel.Text = Res.GetString("03FD9E11-F035-4180-9A2E-30B9E69D13D4", "This device was last seen {0} days ago at ({1}, {2}).", age.Days, location.V2_Location.Latitude, location.V2_Location.Longitude);
				}
				else if (age.Days > 0)
				{
					locationInfoLabel.Text = Res.GetString("1A957ABB-D9E5-4B2B-AD82-4E81FCA46F75", "This device was last seen 1 day ago at ({0}, {1}).", location.V2_Location.Latitude, location.V2_Location.Longitude);
					locationInfoTimer.Interval = (int)TimeSpan.FromDays(1).TotalMilliseconds;
				}
				else if (age.Hours > 1)
				{
					locationInfoLabel.Text = Res.GetString("6F176A28-5EED-47B8-B7F6-22B5A3B36351", "This device was last seen {0} hours ago at ({1}, {2}).", age.Hours, location.V2_Location.Latitude, location.V2_Location.Longitude);
				}
				else if (age.Hours > 0)
				{
					locationInfoLabel.Text = Res.GetString("2A0F83D5-D62E-4F02-A0D9-A30F81FF7F98", "This device was last seen 1 hour ago at ({0}, {1}).", location.V2_Location.Latitude, location.V2_Location.Longitude);
					locationInfoTimer.Interval = (int)TimeSpan.FromHours(1).TotalMilliseconds;
				}
				else if (age.Minutes > 1)
				{
					locationInfoLabel.Text = Res.GetString("51AC520A-84E1-4991-BE30-56F537E2A05C", "This device was last seen {0} minutes ago at ({1}, {2}).", age.Minutes, location.V2_Location.Latitude, location.V2_Location.Longitude);
				}
				else if (age.Minutes > 0)
				{
					locationInfoLabel.Text = Res.GetString("20E064C6-8AD6-4B4B-820E-FC216D188330", "This device was last seen 1 minute ago at ({0}, {1}).", location.V2_Location.Latitude, location.V2_Location.Longitude);
					locationInfoTimer.Interval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
				}
				else if (age.Seconds > 1)
				{
					locationInfoLabel.Text = Res.GetString("A1E810C4-9BAD-42FE-BA5D-D4C414D9BA85", "This device was last seen {0} seconds ago at ({1}, {2}).", age.Seconds, location.V2_Location.Latitude, location.V2_Location.Longitude);
				}
				else
				{
					locationInfoLabel.Text = Res.GetString("4C9778A5-7B18-4693-A55E-E907485221C8", "This device was last seen just now at ({0}, {1}).", location.V2_Location.Latitude, location.V2_Location.Longitude);
				}
			}
		}

		void OnLocationInfoTimerTick(object sender, EventArgs e)
			=> UpdateLocationInfo();
	}
}
