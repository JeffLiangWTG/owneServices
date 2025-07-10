using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class EntryStartEndDates : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EntryStartEndDates(ZDate start, ZDate end)
		{
			this.Start = start;
			this.End = end;
		}

		#region Constants

		public abstract class Schema
		{
			public const string UpdateStart = "UpdateStart";
			public const string UpdateEnd = "UpdateEnd";
			public const string Start = "Start";
			public const string End = "End";
		}

		#endregion

		#region Start

		public ZDate Start
		{
			get { return fStart; }
			set
			{
				fStart = value;
				if (!IsValidationSuspended)
				{
					ValidateStart();
				}
				StartInfo.RefreshBinding();
			}
		}

		ZDate fStart;

		public ZPropertyInfo StartInfo
		{
			get { return GetZPropertyInfo(EntryStartEndDates.Schema.Start); }
		}

		protected bool Start_ReadOnly
		{
			get { return !UpdateStart; }
		}

		public void ValidateStart()
		{
			StartInfo.ClearAllNotifications();

			TypeValidation.CheckValidZDateTimeWithoutRange(StartInfo);
			TypeValidation.CheckValidZDateTimeRange(StartInfo);
		}

		#endregion

		#region End

		public ZDate End
		{
			get { return fEnd; }
			set
			{
				fEnd = value;
				if (!IsValidationSuspended)
				{
					ValidateEnd();
				}
				EndInfo.RefreshBinding();
			}
		}

		ZDate fEnd;

		public ZPropertyInfo EndInfo
		{
			get { return GetZPropertyInfo(EntryStartEndDates.Schema.End); }
		}

		protected bool End_ReadOnly
		{
			get { return !UpdateEnd; }
		}

		public void ValidateEnd()
		{
			EndInfo.ClearAllNotifications();

			TypeValidation.CheckValidZDateTimeWithoutRange(EndInfo);
			TypeValidation.CheckValidZDateTimeRange(EndInfo);
		}

		#endregion

		#region UpdateStart

		public ZBool UpdateStart
		{
			get { return fUpdateStart; }
			set
			{
				fUpdateStart = value;
				if (!IsValidationSuspended)
				{
					ValidateUpdateStart();
				}
				UpdateStartInfo.RefreshBinding();
			}
		}

		ZBool fUpdateStart = ZBool.True;

		public ZPropertyInfo UpdateStartInfo
		{
			get { return GetZPropertyInfo(EntryStartEndDates.Schema.UpdateStart); }
		}

		public void ValidateUpdateStart()
		{
			UpdateStartInfo.ClearAllNotifications();
		}

		#endregion

		#region UpdateEnd

		public ZBool UpdateEnd
		{
			get { return fUpdateEnd; }
			set
			{
				fUpdateEnd = value;
				if (!IsValidationSuspended)
				{
					ValidateUpdateEnd();
				}
				UpdateEndInfo.RefreshBinding();
			}
		}

		ZBool fUpdateEnd = ZBool.True;

		public ZPropertyInfo UpdateEndInfo
		{
			get { return GetZPropertyInfo(EntryStartEndDates.Schema.UpdateEnd); }
		}

		public void ValidateUpdateEnd()
		{
			UpdateEndInfo.ClearAllNotifications();
		}

		#endregion
	}
}

