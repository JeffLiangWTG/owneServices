using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class DefaultStatementPrintDate : RegistryBusinessObjectTemplate
	{
		#region Schema
		public static class Schema
		{
			public const string NumberOfDays = "NumberOfDays";
			public const string DoDefaultPrelimStatementPrintDate = "DoDefaultPrelimStatementPrintDate";
		}
		#endregion

		#region Bound Properties

		#region NumberOfDays
		public ZInt NumberOfDays
		{
			get { return numberOfDays; }
			set
			{
				SetNonPersistentPropertyValue(NumberOfDaysInfo, ref numberOfDays, value);
				if (!IsValidationSuspended)
				{
					ValidateNumberOfDays();
				}
			}
		}
		ZInt numberOfDays;

		public ZPropertyInfo NumberOfDaysInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.NumberOfDays);
			}
		}

		public void ValidateNumberOfDays()
		{
			NumberOfDaysInfo.ClearAllNotifications();
			if (!NumberOfDays.IsEmpty)
			{
				if (NumberOfDays < 0)
				{
					NumberOfDaysInfo.AddError(NumberOfDaysCannotBeNegative);
				}
				else if (NumberOfDays > 10)
				{
					NumberOfDaysInfo.AddError(NumberOfDaysCannotBeGreaterThan10);
				}
			}
		}
		internal const string NumberOfDaysCannotBeNegative = "Number of Days cannot be negative, as Statement Print Date cannot be less than Estimated Entry Date.";
		internal const string NumberOfDaysCannotBeGreaterThan10 = "Number of Days cannot be more than 10 days, as Statement would then be considered overdue.";

		#endregion

		#region DoDefaultPrelimStatementPrintDate
		public ZBool DoDefaultPrelimStatementPrintDate
		{
			get { return doDefaultPrelimStatementPrintDate; }
			set
			{
				SetNonPersistentPropertyValue(DoDefaultPrelimStatementPrintDateInfo, ref doDefaultPrelimStatementPrintDate, value);
			}
		}
		ZBool doDefaultPrelimStatementPrintDate;

		public ZPropertyInfo DoDefaultPrelimStatementPrintDateInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.DoDefaultPrelimStatementPrintDate);
				return result;
			}
		}
		#endregion

		#endregion

		#region Overrides

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			DoDefaultPrelimStatementPrintDate = false;
			numberOfDays = 0;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultStatementPrintDate();
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearRowNotifications();
			base.RunPreSaveValidationCore();
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DoDefaultPrelimStatementPrintDate, DoDefaultPrelimStatementPrintDate.ToString());
			writer.WriteElementString(Schema.NumberOfDays, NumberOfDays.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			using (GetValidationSuspender())
			{
				DoDefaultPrelimStatementPrintDate = reader.ReadElementStringAsZBool(Schema.DoDefaultPrelimStatementPrintDate);
				NumberOfDays = new ZInt(reader.ReadElementString(Schema.NumberOfDays));
			}
		}
		#endregion

	}
}
