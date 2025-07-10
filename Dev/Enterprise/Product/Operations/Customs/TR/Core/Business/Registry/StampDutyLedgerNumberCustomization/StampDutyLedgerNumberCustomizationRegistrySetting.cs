using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.TR.Business.XmlSerializers")]
	public class StampDutyLedgerNumberCustomizationRegistrySetting : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
			public const string StartNumber = "StartNumber";
			public const string ExpiredYear = "ExpiredYear";
		}

		#endregion

		#region Constructors

		public StampDutyLedgerNumberCustomizationRegistrySetting() : base()
		{
		}

		public StampDutyLedgerNumberCustomizationRegistrySetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StampDutyLedgerNumberCustomizationRegistrySetting(fallbackLevel, factory);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			StartDate = new ZDateTime(reader.ReadElementString(Schema.StartDate));
			EndDate = new ZDateTime(reader.ReadElementString(Schema.EndDate));
			StartNumber = ZInt.ParseSafe(reader.ReadElementString(Schema.StartNumber), ZInt.Zero);
			ExpiredYear = ZInt.ParseSafe(reader.ReadElementString(Schema.ExpiredYear), ZInt.Zero);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.StartDate, StartDate.ToString());
			writer.WriteElementString(Schema.EndDate, EndDate.ToString());
			writer.WriteElementString(Schema.StartNumber, StartNumber.ToString());
			writer.WriteElementString(Schema.ExpiredYear, ExpiredYear.ToString());
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			StartNumber = 1;
		}

		#endregion

		#region Properties

		#region StartDate

		public ZDateTime StartDate
		{
			get { return startDate; }
			set
			{
				SetNonPersistentPropertyValue(StartDateInfo, ref startDate, value);

				if (!StartDate.IsEmpty && EndDate.IsEmpty)
				{
					EndDate = StartDate.AddYears(1).AddDays(-1);
				}

				if (!IsValidationSuspended)
				{
					ValidateStartDate();
				}
			}
		}
		ZDateTime startDate;

		public ZPropertyInfo StartDateInfo
		{
			get { return GetZPropertyInfo(Schema.StartDate); }
		}

		#endregion

		#region EndDate

		public ZDateTime EndDate
		{
			get { return endDate; }
			set
			{
				SetNonPersistentPropertyValue(EndDateInfo, ref endDate, value);
				if (!IsValidationSuspended)
				{
					ValidateEndDate();
				}
			}
		}
		ZDateTime endDate;

		public ZPropertyInfo EndDateInfo
		{
			get { return GetZPropertyInfo(Schema.EndDate); }
		}

		#endregion

		#region StartNumber

		public ZInt StartNumber
		{
			get { return startNumber; }
			set
			{
				SetNonPersistentPropertyValue(StartNumberInfo, ref startNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateStartNumber();
				}
			}
		}
		ZInt startNumber;

		public ZPropertyInfo StartNumberInfo
		{
			get { return GetZPropertyInfo(Schema.StartNumber); }
		}

		#endregion

		#region ExpiredYear

		public ZInt ExpiredYear
		{
			get { return expiredYear; }
			set
			{
				SetNonPersistentPropertyValue(ExpiredYearInfo, ref expiredYear, value);
				if (!IsValidationSuspended)
				{
					ValidateExpiredYear();
				}
			}
		}
		ZInt expiredYear;

		public ZPropertyInfo ExpiredYearInfo
		{
			get { return GetZPropertyInfo(Schema.ExpiredYear); }
		}

		#endregion

		#endregion

		#region Validation

		public void ValidateStartDate()
		{
			var targetInfo = StartDateInfo;
			targetInfo.ClearAllNotifications();

			if (!EndDate.IsEmpty)
			{
				if (StartDate > EndDate)
				{
					targetInfo.AddError(Res.GetString("0D7B2172-ACB4-464B-9EAC-0F510C4CE279", "Start Date should not be greater than End Date."));
				}
				else if (StartDate < EndDate.AddYears(-1).AddDays(1))
				{
					targetInfo.AddError(Res.GetString("37DF3736-1877-4876-BC8B-DC9DC61BDFA3", "Start Date should not be more than one year earlier than than End Date."));
				}
			}
		}

		public void ValidateEndDate()
		{
			var targetInfo = EndDateInfo;
			targetInfo.ClearAllNotifications();

			if (!StartDate.IsEmpty)
			{
				if (EndDate < StartDate)
				{
					targetInfo.AddError(Res.GetString("2ABF9D0D-A85A-4E29-B867-C58B34A3BD16", "End Date should not be less than Start Date."));
				}
				else if (EndDate > StartDate.AddYears(1).AddDays(-1))
				{
					targetInfo.AddError(Res.GetString("22B32C07-72E6-41CF-BF0E-1ACAF2543989", "End Date should not be more than one year later than Start Date."));
				}
			}
		}

		public void ValidateStartNumber()
		{
			var targetInfo = StartNumberInfo;
			targetInfo.ClearAllNotifications();

			if (StartNumber < 1 || StartNumber > 9999999)
			{
				targetInfo.AddError(Res.GetString("F2A954D8-2CE8-4F12-A483-F97E96C06E35", "Start Number should be between 1 and 9999999."));
			}
		}

		public void ValidateExpiredYear()
		{
			var targetInfo = ExpiredYearInfo;
			targetInfo.ClearAllNotifications();

			if (ExpiredYear > 0 && ExpiredYear < ZDateTime.Now.Year)
			{
				targetInfo.AddError(Res.GetString("87127D27-58E3-475B-BF21-34DC0476C2EB", "Expired End of Fiscal Year should be equal to or greater than current year."));
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateStartDate();
			ValidateEndDate();
			ValidateStartNumber();
			ValidateExpiredYear();
		}

		#endregion
	}
}
