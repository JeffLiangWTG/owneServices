using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusTariffBRCharacteristic : AutoRefCusTariffBRCharacteristic
	{
		public RefCusTariffBRCharacteristic(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Tariff))]
		public override ZGuid ZB1_ZZ1_Tariff
		{
			get => base.ZB1_ZZ1_Tariff;
			set => base.ZB1_ZZ1_Tariff = value;
		}

		public TariffView Tariff => Factory.Load<TariffView>(ZB1_ZZ1_Tariff);

		[RelatedBusinessObject(nameof(NomenclatureGroup))]
		public override ZGuid ZB1_ZZ5_Nomenclature
		{
			get => base.ZB1_ZZ5_Nomenclature;
			set => base.ZB1_ZZ5_Nomenclature = value;
		}

		public RefCusNomenclatureGroup NomenclatureGroup => Factory.Load<RefCusNomenclatureGroup>(ZB1_ZZ5_Nomenclature);

		#region Values

		[ChildEditable(true)]
		public RefCusTariffBRCharacteristicValueCollection Values
		{
			get
			{
				if (fValues == null)
				{
					fValues = new RefCusTariffBRCharacteristicValueCollection(this);
					RegisterEditableChildObject(fValues);
				}
				return fValues;
			}
		}

		RefCusTariffBRCharacteristicValueCollection fValues;

		#endregion

		#region Attributes

		[ChildEditable(true)]
		public RefCusTariffBRCharacteristicAttributeCollection Attributes
		{
			get
			{
				if (fAttributes == null)
				{
					fAttributes = new RefCusTariffBRCharacteristicAttributeCollection(this);
					RegisterEditableChildObject(fAttributes);
				}
				return fAttributes;
			}
		}

		RefCusTariffBRCharacteristicAttributeCollection fAttributes;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(RefCusTariffBRCharacteristic profile)
				: base(profile)
			{
			}

			protected new RefCusTariffBRCharacteristic BusinessObject
			{
				get { return (RefCusTariffBRCharacteristic)base.BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(RefCusTariffBRCharacteristicValueSchema.ZB2_ZB1_Characteristic, BusinessObject.PK);
				Factory.AddFetchHint(RefCusTariffBRCharacteristicAttributeSchema.ZB3_ZB1_Characteristic, BusinessObject.PK);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusTariffBRCharacteristic.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefCusTariffBRCharacteristic);

			public RefCusTariffBRCharacteristic[] LoadCharacteristics(TariffView tariffView, ZString characteristicType, CharacteristicDirection direction, ZDateTime valuationDate, bool includeForNomenclatureGroup = false)
			{
				Argument.NotNull(tariffView, nameof(tariffView));

				var filter = GetFilter(tariffView, characteristicType, valuationDate, includeForNomenclatureGroup);
				var directionFitler = new ZQuery();
				if (direction == CharacteristicDirection.Import || direction == CharacteristicDirection.Both)
				{
					directionFitler.AddToFilter(JoinCondition.Or, RefCusTariffBRCharacteristicSchema.ZB1_IsImport, true);
				}
				if (direction == CharacteristicDirection.Export || direction == CharacteristicDirection.Both)
				{
					directionFitler.AddToFilter(JoinCondition.Or, RefCusTariffBRCharacteristicSchema.ZB1_IsExport, true);
				}
				filter.AddToFilter(directionFitler);

				return Factory.Load<RefCusTariffBRCharacteristic>(filter);
			}

			ZQuery GetFilter(TariffView tariffView, ZString characteristicType, ZDateTime valuationDate, bool includeForNomenclatureGroup)
			{
				var query = new ZQuery();

				query.AddToFilter(RefCusTariffBRCharacteristicSchema.ZB1_CharacteristicType, characteristicType);
				query.AddToFilter(RefCusTariffBRCharacteristicSchema.ZB1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, valuationDate);
				query.AddToFilter(RefCusTariffBRCharacteristicSchema.ZB1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, valuationDate);

				if (includeForNomenclatureGroup)
				{
					var subQuery = new ZDBOnlyQuery(typeof(RefCusTariffBRCharacteristic));
					subQuery.AddToFilter(new ZQuery(RefCusTariffBRCharacteristicSchema.ZB1_ZZ1_Tariff, tariffView.PK), JoinCondition.Or);

					var nomenclatureQuery = new ZDBOnlySubQuery(typeof(RefCusNomenclatureGroup), RefCusTariffBRCharacteristicSchema.ZB1_ZZ5_Nomenclature);
					nomenclatureQuery.AddToFilter(RefCusNomenclatureGroupCollection.GetQuery(tariffView, valuationDate));
					subQuery.AddSubQuery(nomenclatureQuery, JoinCondition.Or);
					query.AddToFilter(subQuery);
				}
				else
				{
					query.AddToFilter(RefCusTariffBRCharacteristicSchema.ZB1_ZZ1_Tariff, tariffView.PK);
				}

				query.OrderBy = RefCusTariffBRCharacteristicSchema.Constants.ZB1_Code;
				return query;
			}
		}
	}

	public enum CharacteristicDirection
	{
		Both = 0,
		Import = 1,
		Export = 2,
	}
}
