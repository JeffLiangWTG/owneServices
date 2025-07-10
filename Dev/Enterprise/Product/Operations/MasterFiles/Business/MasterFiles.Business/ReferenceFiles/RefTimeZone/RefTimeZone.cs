using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefTimeZone.Schema.R2_CivilianTimeZoneCode), DescriptionProperty(RefTimeZone.Schema.R2_CivilianTimeZoneFullName)]
	public abstract class RefTimeZone : AutoRefTimeZone
	{
		public RefTimeZone(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly RefTimeZoneTypeDecider TypeDecider = new RefTimeZoneTypeDecider();

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("cd67a5b0-dfd5-4b7c-9beb-800d611f114a", "Time Zone Details"); }
		}

		#endregion
	}
}
