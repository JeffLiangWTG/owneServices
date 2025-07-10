using System;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class QueryTariff : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string FromTariff = "FromTariff";
			public const string ToTariff = "ToTariff";
			public const string AsOfDate = "AsOfDate";
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateFromTariff();
			ValidateToTariff();
		}

		#region From Tariff

		[BusinessObjectTestExclude]
		[List(nameof(Tariffs))]
		[MaxLength(12)]
		public ZString FromTariff
		{
			get { return TariffFormatter.DisplayFormat(fromTariff); }
			set
			{
				SetNonPersistentPropertyValue(FromTariffInfo, ref fromTariff, value.KeepNumericCharacters());

				if (!IsValidationSuspended)
				{
					ValidateFromTariff();
				}
			}
		}
		ZString fromTariff;

		public ZPropertyInfo FromTariffInfo
		{
			get { return GetZPropertyInfo(Schema.FromTariff); }
		}

		public void ValidateFromTariff()
		{
			FromTariffInfo.ClearAllNotifications();

			if (FromTariff.IsEmpty)
			{
				FromTariffInfo.AddError(FromTariffMandatory);
			}
		}
		public const string FromTariffMandatory = "From Tariff number is mandatory.";

		#endregion

		#region To Tariff

		[BusinessObjectTestExclude]
		[List(nameof(Tariffs))]
		[MaxLength(12)]
		public ZString ToTariff
		{
			get { return TariffFormatter.DisplayFormat(toTariff); }
			set
			{
				SetNonPersistentPropertyValue(ToTariffInfo, ref toTariff, value.KeepNumericCharacters());

				if (!IsValidationSuspended)
				{
					ValidateToTariff();
				}
			}
		}
		ZString toTariff;

		public ZPropertyInfo ToTariffInfo
		{
			get { return GetZPropertyInfo(Schema.ToTariff); }
		}

		public void ValidateToTariff()
		{
			ToTariffInfo.ClearAllNotifications();

			if (!toTariff.IsEmpty)
			{
				Int64 fromTariffAsInt;
				Int64.TryParse(fromTariff.PadRight(12, '0'), out fromTariffAsInt);

				Int64 toTariffAsInt;
				Int64.TryParse(toTariff.PadRight(12, '9'), out toTariffAsInt);

				if (fromTariffAsInt > toTariffAsInt)
				{
					ToTariffInfo.AddMessageError(ToTariffShouldBeGreaterThanFromTariff);
				}
				else
				{
					ValidateRange();
				}
			}
		}
		public const string ToTariffShouldBeGreaterThanFromTariff = "To Tariff should be greater than From Tariff.";

		void ValidateRange()
		{
			using (DbConnection anotherConn = Db.NewExtraConnectionToMainDb())
			{
				string sqlText =
					"select count(distinct " + USCTariffSchema.UE_Tariff.Name + ") " +
					"	from " + USCTariffSchema.Constants.TableName +
					" where substring(" + USCTariffSchema.UE_Tariff.Name + ", 1, " + fromTariff.Length + ") >= @FromTariff" +
					" and  substring(" + USCTariffSchema.UE_Tariff.Name + ", 1, " + toTariff.Length + ") <= @ToTariff" +
					" and " + USCTariffSchema.UE_DateFrom.Name + " <= @AsOfDate" +
					" and " + USCTariffSchema.UE_DateTo.Name + " >= @AsOfDate";

				DbCommand command = anotherConn.Command(sqlText);
				command.AddParameterBasedOnDbColumn("@FromTariff", TariffFormatter.Format(FromTariff).ToString(), USCTariffSchema.UE_Tariff);
				command.AddParameterBasedOnDbColumn("@ToTariff", TariffFormatter.Format(ToTariff).ToString(), USCTariffSchema.UE_Tariff);
				command.AddParameterBasedOnDbColumn("@AsOfDate", (AsOfDate.IsValid ? AsOfDate.Date : ZDate.Today).ToDateTime(), USCTariffSchema.UE_DateFrom);

				if (ZArchitecture.Core.Utilities.ConvertToInt32(command.ExecuteScalar()) > MaxNumberOfTariffs)
				{
					ToTariffInfo.AddWarning(RangeMayExceed100Tariffs);
				}
			}
		}
		public const string RangeMayExceed100Tariffs = "Number of Tariffs within the specified range exceed 100 numbers. Customs will only respond with the first 100 tariffs in the range.";

		protected virtual int MaxNumberOfTariffs => 100;

		#endregion

		#region AsOfDate

		public ZDateTime AsOfDate
		{
			get { return asOfDate; }
			set { SetNonPersistentPropertyValue(AsOfDateInfo, ref asOfDate, value); }
		}
		ZDateTime asOfDate;

		public ZPropertyInfo AsOfDateInfo
		{
			get { return GetZPropertyInfo(Schema.AsOfDate); }
		}

		#endregion

		#region Lookups

		public BusinessObjectCollection Tariffs
		{
			get { return new USCTariffCollection(new BusinessObjectFactory()); }
		}

		#endregion

		#region Implementation

		TariffFormatter TariffFormatter
		{
			get { return new TariffFormatter(); }
		}

		#endregion
	}
}
