using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class TariffToChange : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string OldTariffNum = "OldTariffNum";
			public const string FormattedOldTariff = "FormattedOldTariff";
			public const int OldTariffNumMaxLength = 12;
			public const string NewTariffNum = "NewTariffNum";
			public const string FormattedNewTariff = "FormattedNewTariff";
			public const int NewTariffNumMaxLength = 12;
		}

		public TariffToChange(USTariffBulkChange tariffBulkChange)
			: base(tariffBulkChange.Factory)
		{
			this.parent = tariffBulkChange;
		}
		readonly USTariffBulkChange parent;

		#region FormattedOldTariff

		[BusinessObjectTestExclude]
		[List(nameof(Tariffs))]
		public ZString FormattedOldTariff
		{
			get { return TariffFormatter.DisplayFormat(OldTariffNum); }
			set { OldTariffNum = value; }
		}

		public ZPropertyInfo FormattedOldTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.FormattedOldTariff, x => OldTariffNumInfo); }
		}

		#endregion

		#region OldTariffNum

		[BusinessObjectTestExclude]
		[MaxLength(Schema.OldTariffNumMaxLength)]
		public ZString OldTariffNum
		{
			get { return oldTariffNum; }
			set
			{
				CheckMaximumLength(OldTariffNumInfo, value);
				SetNonPersistentPropertyValue(OldTariffNumInfo, ref oldTariffNum, value.KeepNumericCharacters());
				ValidateOldTariffNum();
			}
		}
		ZString oldTariffNum;

		public ZPropertyInfo OldTariffNumInfo
		{
			get { return GetZPropertyInfo(Schema.OldTariffNum); }
		}

		public void ValidateOldTariffNum()
		{
			OldTariffNumInfo.ClearAllNotifications();
			if (!IsValidationSuspended)
			{
				if (OldTariffNum.IsEmpty)
				{
					OldTariffNumInfo.AddError(TariffIsEmpty);
				}
				else
				{
					foreach (TariffToChange trf in parent.Tariffs)
					{
						if (trf.OldTariffNum == OldTariffNum && trf != this)
						{
							OldTariffNumInfo.AddError(TariffExists);
						}
					}

					CheckTariffLengthAndListValidation(OldTariffNumInfo);
				}
			}
		}
		public const string TariffIsEmpty = "Tariff Number must not be blank.";
		public const string TariffExists = "The Old Tariff should be unique for tariffs update process.";

		void CheckTariffLengthAndListValidation(ZPropertyInfo tariffInfo)
		{
			ZString tariffNum = (ZString)tariffInfo.Value;

			bool tariffHasCorrectLength = TariffValidator.CorrectTariffLength(tariffNum);
			if (tariffHasCorrectLength)
			{
				ListValidation.WarnIfInvalidCode(tariffInfo, Tariffs);
			}
			else
			{
				tariffInfo.AddError(TariffValidator.InvalidLength);
			}
		}

		#endregion

		#region FormattedNewTariff

		[BusinessObjectTestExclude]
		[List(nameof(Tariffs))]
		public ZString FormattedNewTariff
		{
			get { return TariffFormatter.DisplayFormat(NewTariffNum); }
			set { NewTariffNum = value; }
		}

		public ZPropertyInfo FormattedNewTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.FormattedNewTariff, x => NewTariffNumInfo); }
		}

		#endregion

		#region NewTariffNum

		[BusinessObjectTestExclude]
		[MaxLength(Schema.NewTariffNumMaxLength)]
		public ZString NewTariffNum
		{
			get { return newTariffNum; }
			set
			{
				CheckMaximumLength(NewTariffNumInfo, value);
				SetNonPersistentPropertyValue(NewTariffNumInfo, ref newTariffNum, value.KeepNumericCharacters());
				ValidateNewTariffNum();
			}
		}
		ZString newTariffNum;

		public ZPropertyInfo NewTariffNumInfo
		{
			get { return GetZPropertyInfo(Schema.NewTariffNum); }
		}

		public void ValidateNewTariffNum()
		{
			NewTariffNumInfo.ClearAllNotifications();
			if (!IsValidationSuspended)
			{
				if (NewTariffNum.IsEmpty)
				{
					NewTariffNumInfo.AddError(TariffIsEmpty);
				}
				else
				{
					if (NewTariffNum == OldTariffNum)
					{
						NewTariffNumInfo.AddError(UpdateToTheSameTariffNotAllowed);
					}

					CheckTariffLengthAndListValidation(NewTariffNumInfo);
				}
			}
		}
		public const string UpdateToTheSameTariffNotAllowed = "New Tariff may not be equal to Old Tariff.";

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			ValidateOldTariffNum();
			ValidateNewTariffNum();
			base.RunPreSaveValidationCore();
		}

		public BusinessObjectCollection Tariffs
		{
			get
			{
				BusinessObjectCollection result;
				if (parent.SHBTariffFlag)
				{
					result = new Universal.TariffViewCollection(Factory);
				}
				else
				{
					result = new USCTariffCollection(Factory);
				}
				return result;
			}
		}

		public TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter tariffFormatter;
	}
}
