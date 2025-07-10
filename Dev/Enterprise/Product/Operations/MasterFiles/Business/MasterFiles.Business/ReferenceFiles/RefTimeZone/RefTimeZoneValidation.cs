using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefTimeZoneValidation : AutoRefTimeZoneValidation
	{
		public RefTimeZoneValidation(AutoRefTimeZone parent) : base(parent)
		{
		}

		new internal RefTimeZone Parent
		{
			get { return (RefTimeZone)base.Parent; }
		}

		#region R2_CivilianTimeZoneCode

		protected override void CheckR2_CivilianTimeZoneCode()
		{
			base.CheckR2_CivilianTimeZoneCode();
			MandatoryValidation.CheckEntered(Parent.R2_CivilianTimeZoneCodeInfo);
		}

		#endregion

		#region R2_OffsetMinutesFromUTC

		protected override void CheckR2_OffsetMinutesFromUTC()
		{
			base.CheckR2_OffsetMinutesFromUTC();
			if (Parent.R2_OffsetMinutesFromUTC < -720 || Parent.R2_OffsetMinutesFromUTC > 840)
			{
				Parent.R2_OffsetMinutesFromUTCInfo.AddError(Res.GetString("8bb0b6d7-5319-494a-adae-c9626542a436", "The UTC Offset is out of Range - it must be between -720 and +840 minutes."));
			}
		}

		#endregion
	}
}
