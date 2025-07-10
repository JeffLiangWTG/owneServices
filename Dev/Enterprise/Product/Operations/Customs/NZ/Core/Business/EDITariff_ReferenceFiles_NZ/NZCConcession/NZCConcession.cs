using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCConcession : AutoNZCConcession
	{
		#region Schema

		public new class Schema : AutoNZCConcession.Schema
		{
			public const string U2_Calc_Tariffs = "U2_Calc_Tariffs";
		}

		#endregion

		public NZCConcession(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static NZCConcession New(BusinessObjectFactory factory)
		{
			return factory.New<NZCConcession>();
		}

		public NZCConcessionClassificationLinkCollection TariffsApplicable
		{
			get
			{
				if (fTariffsApplicable == null)
				{
					fTariffsApplicable = new NZCConcessionClassificationLinkCollection(this);
					fTariffsApplicable.Load();
				}
				return fTariffsApplicable;
			}
		}

		public NZCConcessionDutyRateCollection DutyRates
		{
			get
			{
				SetupDutyRatesIfNull(true);
				return fDutyRates;
			}
		}

		public void LoadDutyRates(ZString countryGroup)
		{
			ZQuery specialFilter = new ZQuery(NZCConcessionDutyRateSchema.U5_PreferentialCountryGroup, "NML");
			if (!countryGroup.IsEmpty)
			{
				specialFilter.AddToFilter(JoinCondition.Or, NZCConcessionDutyRateSchema.U5_PreferentialCountryGroup, SQLComparisonOperator.Equal, countryGroup);
			}
			LoadDutyRates(specialFilter);
		}

		public void LoadDutyRates()
		{
			LoadDutyRates(new ZQuery());
		}

		void LoadDutyRates(ZQuery additionalFilter)
		{
			SetupDutyRatesIfNull(false);
			fDutyRates.Load(additionalFilter);
		}

		void SetupDutyRatesIfNull(bool loadIfNotAlreadySetup)
		{
			if (fDutyRates == null)
			{
				fDutyRates = new NZCConcessionDutyRateCollection(this);
				fDutyRates.SetReadOnlyIncludingChildren(ReadOnly);
				if (loadIfNotAlreadySetup)
				{
					fDutyRates.Load();
				}
			}
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		#region New Properties

		public ZString U2_Calc_Tariffs
		{
			get
			{
				StringBuilder result = new StringBuilder();
				foreach (NZCConcessionClassificationLink tariffApplicable in TariffsApplicable)
				{
					result.Append(tariffApplicable.U3_TariffPortion + ",");
				}
				return result.ToString().TrimEnd(',');
			}
		}

		public ZPropertyInfo U2_Calc_TariffsInfo
		{
			get { return GetZPropertyInfo(Schema.U2_Calc_Tariffs); }
		}

		#endregion

		#region Implementation

		protected NZCConcessionClassificationLinkCollection fTariffsApplicable;
		protected NZCConcessionDutyRateCollection fDutyRates;

		protected override ZString HumanReadableNameCore
		{
			get { return "Concession"; }
		}

		#endregion
	}
}
