using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class InvoiceAmountBoundary : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
			public const string Amount = "Amount";
		}

		#endregion

		#region StartDate

		[ResourceStringData("dd5596e0-c8d1-4331-9414-4e33921c5b84", Caption = "Start Date")]
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

		[ResourceStringData("4677e5da-49e4-480c-8960-b46a76f5325d", Caption = "End Date")]
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
				EndDateInfo.AddError(Res.GetString("D0B42F0A-6115-4EF2-835B-4F8FAEEC9A9A",
					"End Date cannot be less than Start Date."));
			}
			ValidateDateNotOverlapOtherDate(EndDateInfo);
		}

		#endregion

		#region Amount

		[ResourceStringData("5f0fe2a6-7b18-44fa-8b79-0613f270b119", Caption = "Amount (NIS)")]
		public ZDecimal Amount
		{
			get { return amount; }
			set
			{
				SetNonPersistentPropertyValue(AmountInfo, ref amount, value);
				if (!IsValidationSuspended)
				{
					ValidateAmount();
				}
			}
		}
		ZDecimal amount;

		public ZPropertyInfo AmountInfo
		{
			get { return GetZPropertyInfo(Schema.Amount); }
		}

		public void ValidateAmount()
		{
			AmountInfo.ClearAllNotifications();
			if (Amount < 0)
			{
				AmountInfo.AddError(Res.GetString("3D302AF8-2B7D-4F1A-9282-08B65C285A26",
					"Amount must be greater or equal to 0."));
			}
		}

		#endregion

		public bool IsWithinDateAmount(ZDateTime checkDate)
		{
			return StartDate <= checkDate && checkDate <= EndDate;
		}

		void ValidateDateNotOverlapOtherDate(ZPropertyInfo dateInfo)
		{
			ZDateTime value = (ZDateTime)dateInfo.Value;
			if (value.IsValid)
			{
				foreach (BusinessObjectCollection collection in ParentCollections)
				{
					foreach (InvoiceAmountBoundary element in collection)
					{
						if (element != this && (element.IsWithinDateAmount(value) || IsWithinDateAmount(element.StartDate) || IsWithinDateAmount(element.EndDate)))
						{
							dateInfo.AddError(
								Res.GetString("52AA7E7B-E337-406D-BE98-906A77B6E765",
									"This amount's dates overlap another amount's dates. This amount's dates: '{0}' and '{1}'. Other amount's dates: '{2}' and '{3}'.",
									GetDateString(startDate), GetDateString(endDate), GetDateString(element.StartDate), GetDateString(element.EndDate)));
							return;
						}
					}
				}
			}
		}

		static string GetDateString(ZDateTime date)
		{
			return date.IsEmpty ? "EMPTY" : date.ToShortDateString();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateStartDate();
			ValidateEndDate();
			ValidateAmount();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new InvoiceAmountBoundary();
			result.StartDate = StartDate;
			result.EndDate = EndDate;
			result.Amount = Amount;
			return result;
		}

		const string DateFormat = "yyyyMMdd";
		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.StartDate, StartDate.ToString(DateFormat));
			writer.WriteElementString(Schema.EndDate, EndDate.ToString(DateFormat));
			writer.WriteElementString(Schema.Amount, Amount.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			StartDate = reader.ReadElementStringAsZDateTime(Schema.StartDate, DateFormat);
			EndDate = reader.ReadElementStringAsZDateTime(Schema.EndDate, DateFormat);
			Amount = reader.ReadElementStringAsZDecimal(Schema.Amount);
		}
	}
}
