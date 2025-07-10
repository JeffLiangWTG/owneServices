using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	[CodeProperty(AutoNZCClassification.Schema.U0_Tariff), DescriptionProperty(AutoNZCClassification.Schema.U0_Description)]
	public class NZCClassification : AutoNZCClassification, IFamilyMember, ITraversibleNode, ITariff
	{
		public NZCClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static NZCClassification New(BusinessObjectFactory factory)
		{
			return factory.New<NZCClassification>();
		}

		public bool IsPetrolClassification
		{
			get
			{
				return (U0_Tariff == "2710.19.11.11F"
					|| U0_Tariff == "2710.19.11.19A"
					|| U0_Tariff == "2710.19.29.11B"
					|| U0_Tariff == "2710.19.29.19H");
			}
		}

		public ZString U0_SupplementaryUnitForInvoiceLine
		{
			get { return IsPetrolClassification ? ZString.Empty : base.U0_SupplementaryUnit; }
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public static ZString GetDescriptionForCompleteCode(BusinessObjectFactory factory, ZString code)
		{
			NZCClassification classification = GetClassForCompleteCode(factory, code);
			ZString description = "";
			if (classification != null)
			{
				description = classification.U0_ComputerDumpDescr.IsEmpty ? classification.U0_Description : classification.U0_ComputerDumpDescr;
			}
			return description;
		}

		#region Static Loaders
		public static NZCClassification GetClassForPartialCode(BusinessObjectFactory factory, ZString code)
		{
			return GetClassForPartialCode(factory, code, ZDateTime.Empty);
		}

		public static NZCClassification GetClassForPartialCode(BusinessObjectFactory factory, ZString code, ZDateTime dateForDutyRate)
		{
			NZCClassification result = null;
			if (dateForDutyRate != ZDateTime.Invalid)
			{
				result = factory.LoadTop1<NZCClassification>(GetDateForDutyRateClassificationFilter(code, dateForDutyRate));
			}
			return result;
		}

		public static NZCClassification GetClassThatHasDutyRatesAgainstIt(BusinessObjectFactory factory, ZString code)
		{
			NZCClassification result = null;
			if (code.Length == 14)
			{
				result = factory.LoadTop1<NZCClassification>(GetClassThatHasDutyRatesAgainstItFilter(code));
			}
			return result;
		}

		public static ZQuery GetClassThatHasDutyRatesAgainstItFilter(ZString code)
		{
			ZQuery sQLFilter = new ZQuery(NZCClassificationSchema.U0_Tariff, code);
			sQLFilter.OrderBy = NZCClassification.Schema.U0_DateActiveFrom + OrderByClause.Descending;
			return sQLFilter;
		}

		public static NZCClassification GetClassForCompleteCode(BusinessObjectFactory factory, ZString code, ZDateTime dateForDutyRate)
		{
			return factory.GetCachedValue(code + dateForDutyRate.ToShortDateString(), delegate
			{
				NZCClassification result = null;
				if (code.Length == 14)
				{
					ZDateTime dateForDutyRateOrTodayIfEmpty = dateForDutyRate.IsEmpty ? ZDateTime.Today : dateForDutyRate;
					result = factory.LoadTop1<NZCClassification>(GetDateForDutyRateClassificationFilter(code, dateForDutyRateOrTodayIfEmpty));
				}
				return result;
			});
		}

		public static NZCClassification GetClassForCompleteCode(BusinessObjectFactory factory, ZString code)
		{
			NZCClassification result = null;
			if (code.Length == 14)
			{
				result = factory.LoadTop1<NZCClassification>(GetDateForDutyRateClassificationFilter(code, ZDateTime.Empty));
			}
			return result;
		}

		public static ZQuery GetDateForDutyRateClassificationFilter(ZString tariffCode, ZDateTime dateForDutyRate)
		{
			SQLComparisonOperator comparisionOperator = (tariffCode.Length == 14) ? SQLComparisonOperator.Equal : SQLComparisonOperator.Contains;
			ZQuery sQLFilter = new ZQuery(NZCClassificationSchema.U0_Tariff, comparisionOperator, tariffCode);
			if (!dateForDutyRate.IsEmpty)
			{
				sQLFilter.AddToFilter(NZCClassificationSchema.U0_DateActiveFrom, SQLComparisonOperator.LessThanOrEqualTo, dateForDutyRate);
				ZQuery wrappedSQLFilter = new ZQuery(NZCClassificationSchema.U0_DateActiveTo, SQLComparisonOperator.Equal, null);
				wrappedSQLFilter.AddToFilter(JoinCondition.Or, NZCClassificationSchema.U0_DateActiveTo, SQLComparisonOperator.GreaterThanOrEqualTo, dateForDutyRate);
				sQLFilter.AddToFilter(wrappedSQLFilter);
			}

			sQLFilter.OrderBy = NZCClassification.Schema.U0_Tariff + ", " + NZCClassification.Schema.U0_DateActiveFrom + OrderByClause.Descending;
			return sQLFilter;
		}

		public NZCClassificationChapter Chapter
		{
			get { return Factory.Load<NZCClassificationChapter>(U0_Q2_Chapter); }
		}
		#endregion

		#region IFamilyMember Members

		public ZString WrappedLongDescription
		{
			get { return U0_Description; }
		}

		public ZPropertyInfo WrappedLongDescriptionInfo
		{
			get { return U0_DescriptionInfo; }
		}

		public ZString StatUnit
		{
			get { return U0_StatisticalUnit; }
		}

		public ZPropertyInfo StatUnitInfo
		{
			get { return U0_StatisticalUnitInfo; }
		}

		public ZString SuppUnit
		{
			get { return U0_SupplementaryUnit; }
		}

		public ZPropertyInfo SuppUnitInfo
		{
			get { return U0_SupplementaryUnitInfo; }
		}

		public bool HasChildren
		{
			get { return false; }
		}

		public CargoWise.EntityFramework.IFamilyMember[] Children
		{
			get { return System.Array.Empty<CargoWise.EntityFramework.IFamilyMember>(); }
		}

		public ZPropertyInfo LongDescriptionInfo
		{
			get { return WrappedLongDescriptionInfo; }
		}

		public string ShortDescription
		{
			get { return U0_Tariff + " " + U0_Description; }
		}

		public ZString LongDescription
		{
			get { return WrappedLongDescription; }
		}

		#endregion

		#region ITraversibleNode Members

		public CargoWise.EntityFramework.IFamilyMember[] GetHierarchy()
		{
			ArrayList list = new ArrayList();
			list.Add(this);
			if (Chapter != null)
			{
				list.AddRange(Chapter.GetHierarchy());
			}
			return (IFamilyMember[])list.ToArray(typeof(IFamilyMember));
		}

		#endregion

		#region ITariff Members

		public ZString Code => U0_Tariff;
		public ZString Description => U0_Description;
		public ZString UQ1 => U0_StatisticalUnit;
		public ZString UQ2 => string.Empty;
		public ZString UQ3 => string.Empty;
		public ZString UQ4 => string.Empty;
		public ZString UQ5 => string.Empty;

		#endregion

		public NZCClassificationDutyRateCollection DutyRates
		{
			get
			{
				if (dutyRates == null)
				{
					dutyRates = new NZCClassificationDutyRateCollection(this, Factory);
					dutyRates.Load();
				}

				return dutyRates;
			}
		}
		NZCClassificationDutyRateCollection dutyRates;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new NZCClassificationFetchStrategy(this);
		}

		class NZCClassificationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public NZCClassificationFetchStrategy(NZCClassification parent)
				: base(parent)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(NZCClassificationDutyRateSchema.U1_U0_Classification, BusinessObject.PK);
			}
		}
	}
}
