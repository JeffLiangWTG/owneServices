using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class ReconInterestRate : RegistryBusinessObjectTemplate
	{
		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			startDate = ZDateTime.Empty;
			endDate = ZDateTime.Empty;
			rate = ZDecimal.Zero;
		}

		#region Schema

		public static class Schema
		{
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
			public const string Rate = "Rate";
		}

		#endregion

		#region Bound Properties

		#region StartDate

		public ZDateTime StartDate
		{
			get { return startDate; }
			set
			{
				SetNonPersistentPropertyValue(StartDateInfo, ref startDate, value, true);
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

		public void ValidateStartDate()
		{
			StartDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(StartDateInfo);
			ValidateDateNotOverlapOtherDate(StartDateInfo);
			ValidateEndDate();
		}

		#endregion

		#region EndDate

		public ZDateTime EndDate
		{
			get { return endDate; }
			set
			{
				SetNonPersistentPropertyValue(EndDateInfo, ref endDate, value, true);
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

		public void ValidateEndDate()
		{
			EndDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EndDateInfo);
			if (StartDate > EndDate)
			{
				EndDateInfo.AddError("End Date cannot be less than Start Date.");
			}
			ValidateDateNotOverlapOtherDate(EndDateInfo);
		}

		#endregion

		#region Rate

		public ZDecimal Rate
		{
			get { return rate; }
			set
			{
				SetNonPersistentPropertyValue(RateInfo, ref rate, value);
				if (!IsValidationSuspended)
				{
					ValidateRate();
				}
			}
		}
		ZDecimal rate;

		public ZPropertyInfo RateInfo
		{
			get { return GetZPropertyInfo(Schema.Rate); }
		}

		public void ValidateRate()
		{
			RateInfo.ClearAllNotifications();
			if (!(Rate > 0 && Rate < 50))
			{
				RateInfo.AddError("Rate must be greater than 0 and less than 50.");
			}
		}

		#endregion

		#endregion

		public bool IsWithinDateRate(ZDateTime checkDate)
		{
			return StartDate <= checkDate && checkDate <= EndDate;
		}

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateStartDate();
			ValidateEndDate();
			ValidateRate();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ReconInterestRate result = new ReconInterestRate();
			result.EndDate = EndDate;
			result.StartDate = StartDate;
			result.Rate = Rate;
			return result;
		}

		const string DateFormat = "yyyyMMdd";
		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.StartDate, StartDate.ToString(DateFormat));
			writer.WriteElementString(Schema.EndDate, EndDate.ToString(DateFormat));
			writer.WriteElementString(Schema.Rate, Rate.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ZDateTime startDate = reader.ReadElementStringAsZDateTime(Schema.StartDate, DateFormat);
			EndDate = reader.ReadElementStringAsZDateTime(Schema.EndDate, DateFormat);
			StartDate = startDate;
			Rate = reader.ReadElementStringAsZDecimal(Schema.Rate);
		}

		#endregion

		void ValidateDateNotOverlapOtherDate(ZPropertyInfo dateInfo)
		{
			ZDateTime value = (ZDateTime)dateInfo.Value;
			if (value.IsValid)
			{
				foreach (BusinessObjectCollection collection in ParentCollections)
				{
					foreach (ReconInterestRate element in collection)
					{
						if (element != this && (element.IsWithinDateRate(value) || IsWithinDateRate(element.StartDate) || IsWithinDateRate(element.EndDate)))
						{
							dateInfo.AddError(string.Format("This rate's dates overlap another rate's dates.\r\nThis rate's dates: '{0}' and '{1}'\r\nOther rate's dates: '{2}' and '{3}'.", GetDateString(startDate), GetDateString(endDate), GetDateString(element.StartDate), GetDateString(element.EndDate)));
							return;
						}
					}
				}
			}
		}

		string GetDateString(ZDateTime date)
		{
			return date.IsEmpty ? "EMPTY" : date.ToShortDateString();
		}
	}
}
