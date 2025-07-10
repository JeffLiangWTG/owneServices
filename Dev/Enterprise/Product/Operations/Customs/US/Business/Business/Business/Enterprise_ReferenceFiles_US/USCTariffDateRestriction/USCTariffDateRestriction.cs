using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCTariffDateRestriction : AutoUSCTariffDateRestriction
	{
		public USCTariffDateRestriction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public USCTariff Tariff
		{
			get { return Factory.Load<USCTariff>(UF_UE); }
		}

		public ZString RestrictionDateDescription
		{
			get
			{
				MonthList list = new MonthList();

				ZString fromMonth = list.GetDescriptionFromCode(RestrictionFromMonth);
				ZString toMonth = list.GetDescriptionFromCode(RestrictionToMonth);

				return fromMonth.Left(3) + "-" + RestrictionFromDay + " to " + toMonth.Left(3) + "-" + RestrictionToDay;
			}
		}

		public bool IsPassedDateWithinRestrictedDates(ZDate date)
		{
			bool result = true;

			if (HasValidData)
			{
				ZInt fromMonth = ZInt.ParseSafe(RestrictionFromMonth, 0);
				ZInt fromDay = ZInt.ParseSafe(RestrictionFromDay, 0);

				ZInt toMonth = ZInt.ParseSafe(RestrictionToMonth, 0);
				ZInt toDay = ZInt.ParseSafe(RestrictionToDay, 0);

				if (date.Month == fromMonth && date.Day >= fromDay || date.Month == toMonth && date.Day <= toDay)
				{
					result = true;
				}
				else
				{
					result = date.Month > fromMonth;

					bool isLessThanToMonth = date.Month < toMonth;

					if (UF_EntryDateRestrictionCode == "2")//Nov-15 to Feb-01
					{
						result |= isLessThanToMonth;
					}
					else
					{
						result &= isLessThanToMonth;
					}
				}
			}

			return result;
		}

		#region Implementation

		ZString RestrictionFromMonth
		{
			get { return UF_EntryDateRestrictionFromStringPadded.Left(2); }
		}

		ZString RestrictionFromDay
		{
			get { return UF_EntryDateRestrictionFromStringPadded.Right(2); }
		}

		ZString RestrictionToMonth
		{
			get { return UF_EntryDateRestrictionToStringPadded.Left(2); }
		}

		ZString RestrictionToDay
		{
			get { return UF_EntryDateRestrictionToStringPadded.Right(2); }
		}

		ZString UF_EntryDateRestrictionFromStringPadded
		{
			get
			{
				if (!fUF_EntryDateRestrictionFromString.HasValue)
				{
					fUF_EntryDateRestrictionFromString = UF_EntryDateRestrictionFrom.ToString().PadLeft(4, '0');
				}
				return fUF_EntryDateRestrictionFromString.Value;
			}
		}
		ZString? fUF_EntryDateRestrictionFromString;

		ZString UF_EntryDateRestrictionToStringPadded
		{
			get
			{
				if (!fUF_EntryDateRestrictionToString.HasValue)
				{
					fUF_EntryDateRestrictionToString = UF_EntryDateRestrictionTo.ToString().PadLeft(4, '0');
				}
				return fUF_EntryDateRestrictionToString.Value;
			}
		}
		ZString? fUF_EntryDateRestrictionToString;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(USCTariffDateRestriction tariffDateRestriction)
				: base(tariffDateRestriction)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				USCTariffDateRestriction tariffDateRestriction = (USCTariffDateRestriction)BusinessObject;
				Factory.AddFetchHint(typeof(USCTariff), USCTariffSchema.PK, tariffDateRestriction.UF_UE);
			}
		}

		#endregion

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			}
			base.Delete();
		}
		public override void OnSaving()
		{
			SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			if (!IsDeleted && !HasValidData)
			{
				Delete();
			}
			base.OnSaving();
		}

		public override bool IsSavedByFactory
		{
			get { return IsInDatabase || HasValidData; }
		}

		bool HasValidData
		{
			get
			{
				return
					!UF_EntryDateRestrictionCode.IsEmpty &&
					!UF_EntryDateRestrictionFrom.IsEmpty &&
					!UF_EntryDateRestrictionTo.IsEmpty;
			}
		}
	}
}
