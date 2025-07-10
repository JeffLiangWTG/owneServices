using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Customs.Universal
{
	public abstract class TariffRelatedFilteredCollection<T> : ActiveBusinessObjectCollection<T>
		where T : BusinessObject
	{
		protected TariffRelatedFilteredCollection(TariffView parentTariff, SchemaGuidColumn foreignKeyColumn)
			: this(parentTariff, GetQuery(parentTariff, foreignKeyColumn))
		{
		}

		protected TariffRelatedFilteredCollection(TariffView parentTariff, ZQuery filter)
			: base(parentTariff.Factory, filter)
		{
			this.parentTariff = parentTariff;
		}

		protected TariffRelatedFilteredCollection(TariffView parentTariff, ZQuery filter, SchemaGuidColumn relationshipColumn)
			: base(parentTariff.Factory, parentTariff, filter, relationshipColumn)
		{
			this.parentTariff = parentTariff;
		}

		readonly protected TariffView parentTariff;

		protected TariffRelatedFilteredCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		static ZQuery GetQuery(TariffView parentTariff, SchemaGuidColumn foreignKeyColumn)
		{
			var result = new ZQuery(foreignKeyColumn, parentTariff.PK);
			if (parentTariff.IsTariffNationalCode)
			{
				result.AddToFilter(JoinCondition.Or, foreignKeyColumn, parentTariff.ZZ1_ZZ1_Tariff);
			}
			return result;
		}

		protected override bool AllowNew => false;
	}

	public abstract class TariffDataGroupingRelatedFilteredCollection<T> : TariffRelatedFilteredCollection<T>
		where T : BusinessObject, ITariffDataGroupingRelatedBusinessObject
	{
		protected TariffDataGroupingRelatedFilteredCollection(TariffView parentTariff, SchemaGuidColumn foreignKeyColumn, bool enableEffectiveDataGrouping)
			: base(parentTariff, foreignKeyColumn)
		{
			this.enableEffectiveDataGrouping = enableEffectiveDataGrouping;
			SetupEventHandler();
		}

		protected TariffDataGroupingRelatedFilteredCollection(TariffView parentTariff, ZQuery filter, bool enableEffectiveDataGrouping)
			: base(parentTariff, filter)
		{
			this.enableEffectiveDataGrouping = enableEffectiveDataGrouping;
			SetupEventHandler();
		}

		protected TariffDataGroupingRelatedFilteredCollection(TariffView parentTariff, ZQuery filter, SchemaGuidColumn relationshipColumn, bool enableEffectiveDataGrouping)
			: base(parentTariff, filter, relationshipColumn)
		{
			this.enableEffectiveDataGrouping = enableEffectiveDataGrouping;
			SetupEventHandler();
		}

		void SetupEventHandler()
		{
			if (enableEffectiveDataGrouping)
			{
				parentTariff.Wrapper.EffectiveDataGroupingInfo.ValueChanged += EffectiveDateInfo_ValueChanged;
			}
		}

		void EffectiveDateInfo_ValueChanged(object sender, System.EventArgs e)
		{
			((IActiveBusinessObjectCollection)this).Refresh();
		}

		protected override bool MatchesFilterCore(T element, bool fetchOnlyFromLocalCache)
		{
			var result = false;
			if (base.MatchesFilterCore(element, fetchOnlyFromLocalCache))
			{
				result = (!enableEffectiveDataGrouping || (MatchesDataGroupingFilter(parentTariff, element.DataGrouping))) && AdditionalMatchesFilter(parentTariff, element);
			}
			return result;
		}

		protected virtual bool MatchesDataGroupingFilter(TariffView master, string dataGrouping)
		{
			return master.Wrapper.MatchEffectiveDataGrouping(dataGrouping) && master.MatchDataGrouping(dataGrouping);
		}

		protected virtual bool AdditionalMatchesFilter(TariffView master, T element) => true;
		readonly bool enableEffectiveDataGrouping;
	}

	public abstract class TariffEffectiveDatesRelatedFilteredCollection<T> : TariffDataGroupingRelatedFilteredCollection<T>
		where T : BusinessObject, ITariffEffectiveDatesRelatedBusinessObject
	{
		protected TariffEffectiveDatesRelatedFilteredCollection(TariffView parentTariff, SchemaGuidColumn foreignKeyColumn, bool enableEffectiveDataGrouping, bool enableEffectiveDateFilter)
			: base(parentTariff, foreignKeyColumn, enableEffectiveDataGrouping)
		{
			this.enableEffectiveDateFilter = enableEffectiveDateFilter;
			SetupEventHandler();
		}

		protected TariffEffectiveDatesRelatedFilteredCollection(TariffView parentTariff, ZQuery filter, bool enableEffectiveDataGrouping, bool enableEffectiveDateFilter)
			: base(parentTariff, filter, enableEffectiveDataGrouping)
		{
			this.enableEffectiveDateFilter = enableEffectiveDateFilter;
			SetupEventHandler();
		}

		protected TariffEffectiveDatesRelatedFilteredCollection(TariffView parentTariff, ZQuery filter, SchemaGuidColumn relationshipColumn, bool enableEffectiveDataGrouping, bool enableEffectiveDateFilter)
			: base(parentTariff, filter, relationshipColumn, enableEffectiveDataGrouping)
		{
			this.enableEffectiveDateFilter = enableEffectiveDateFilter;
			SetupEventHandler();
		}

		void SetupEventHandler()
		{
			if (enableEffectiveDateFilter)
			{
				parentTariff.Wrapper.EffectiveDateInfo.ValueChanged += EffectiveDataGroupingInfo_ValueChanged;
			}
		}

		protected override bool AdditionalMatchesFilter(TariffView master, T element)
		{
			return !enableEffectiveDateFilter ||
				master.Wrapper.IsWithInEffectiveDate(element.StartDate, element.EndDate);
		}
		void EffectiveDataGroupingInfo_ValueChanged(object sender, System.EventArgs e)
		{
			((IActiveBusinessObjectCollection)this).Refresh();
		}

		readonly bool enableEffectiveDateFilter;
	}
}
