using System.Collections.Generic;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USTariffBulkChange : NonPersistentBusinessObject, IObsoleteValidation
	{
		public USTariffBulkChange(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public event TariffBulkChangedEventHandler ProgressChanged;

		public ZBool HTSTariffFlag
		{
			get { return htsTariffFlag; }
			set { htsTariffFlag = value; }
		}
		ZBool htsTariffFlag;

		public ZBool SHBTariffFlag
		{
			get { return shbTariffFlag; }
			set { shbTariffFlag = value; }
		}
		ZBool shbTariffFlag;

		public int ProductsChanged
		{
			get { return listOfChangedProducts != null ? listOfChangedProducts.Count : 0; }
		}
		List<ZGuid> listOfChangedProducts;
		int productsFound;

		public int ClassificationsChanged
		{
			get { return listOfChangedClassifications != null ? listOfChangedClassifications.Count : 0; }
		}
		List<ZGuid> listOfChangedClassifications;
		int classificationsFound;

		public bool Cancelled { get; private set; }

		public ZString Status
		{
			get { return status; }
		}
		ZString status;

		public void Cancel()
		{
			Cancelled = true;
		}

		public TariffToChangeCollection Tariffs
		{
			get
			{
				if (tariffs == null)
				{
					tariffs = new TariffToChangeCollection(this);
					RegisterEditableChildObject(tariffs);
				}
				return tariffs;
			}
		}
		TariffToChangeCollection tariffs;

		public void Update()
		{
			listOfChangedProducts = new List<ZGuid>();
			listOfChangedClassifications = new List<ZGuid>();
			foreach (TariffToChange tariffToChange in Tariffs)
			{
				status = string.Format("Updating Products and Classifications for Tariff {0}...", tariffToChange.OldTariffNum);
				OnProgressChanged(0);

				countOfObjectsChanged = 0;
				ZQuery partQuery = GetPartQuery(tariffToChange);
				productsFound = Factory.GetDatabaseCount(typeof(OrgSupplierPart), partQuery);

				ZQuery classQuery = GetClassificationQuery(tariffToChange.OldTariffNum);
				classificationsFound = Factory.GetDatabaseCount(typeof(CusClassification), classQuery);

				do
				{
					BusinessObjectFactory factoryForSaving = new BusinessObjectFactory();
					factoryForSaving.SuspendValidation();
					var partCollection = new DynamicBusinessObjectCollection(factoryForSaving);

					string sql = "select top " + step + " OP_PK from dbo.OrgSupplierPart";
					partCollection.Load(sql + partQuery.GetAsWhereAndOrderByClause(false), partQuery.Params);

					UpdateProducts(partCollection, factoryForSaving, tariffToChange);

					var lookupsCollection = new DynamicBusinessObjectCollection(factoryForSaving);

					if (!Cancelled)
					{
						string sql1 = "select top " + step + " CC_PK from dbo.CusClassification";
						lookupsCollection.Load(sql1 + classQuery.GetAsWhereAndOrderByClause(false), classQuery.Params);

						UpdateClassifications(lookupsCollection, factoryForSaving, tariffToChange);
					}

					factoryForSaving.Save();
					if ((partCollection.Count < step && lookupsCollection.Count < step) || Cancelled)
					{
						break;
					}
				}
				while (true);

				if (Cancelled)
				{
					break;
				}
			}
		}
		readonly int step = 100;
		int countOfObjectsChanged;

		void UpdateProducts(DynamicBusinessObjectCollection collection, BusinessObjectFactory factoryForSaving, TariffToChange tariffToChange)
		{
			foreach (DynamicBusinessObject dynamicObject in collection)
			{
				if (Cancelled)
				{
					break;
				}

				OrgSupplierPart orgSupplierPart = factoryForSaving.Load<OrgSupplierPart>((ZGuid)dynamicObject[OrgSupplierPartSchema.PK]);
				orgSupplierPart.IsLightSaving = true;
				foreach (CusClassPartPivot pivot in orgSupplierPart.GetUSPivots())
				{
					bool shouldUpdate = (HTSTariffFlag && pivot.UseHTSClassification) ||
										(SHBTariffFlag && pivot.UseSCHBClassification);

					if (shouldUpdate)
					{
						if (pivot.CI_TariffNum.ExcludeChars(" .") == tariffToChange.OldTariffNum)
						{
							pivot.CI_TariffNum = tariffToChange.NewTariffNum;
						}

						if (pivot.CI_SupplementalTariff.ExcludeChars(" .") == tariffToChange.OldTariffNum)
						{
							pivot.CI_SupplementalTariff = tariffToChange.NewTariffNum;
						}

						foreach (CusClassPartPivot child in pivot.Children)
						{
							if (child.CI_TariffNum.ExcludeChars(" .") == tariffToChange.OldTariffNum)
							{
								child.CI_TariffNum = tariffToChange.NewTariffNum;
							}
						}
					}
				}

				if (!listOfChangedProducts.Contains(orgSupplierPart.PK))
				{
					listOfChangedProducts.Add(orgSupplierPart.PK);
				}
				countOfObjectsChanged++;
				if (countOfObjectsChanged % 20 == 0)
				{
					OnProgressChanged(countOfObjectsChanged);
				}
			}
		}

		void UpdateClassifications(DynamicBusinessObjectCollection collection1, BusinessObjectFactory factoryForSaving, TariffToChange tariffToChange)
		{
			foreach (DynamicBusinessObject dynamicObject in collection1)
			{
				if (Cancelled)
				{
					break;
				}

				CusClassification classification = factoryForSaving.Load<CusClassification>((ZGuid)dynamicObject[Enterprise.ZArchitecture.Schema.CusClassificationSchema.PK]);
				if (classification.CC_TariffNum == tariffToChange.OldTariffNum)
				{
					classification.CC_TariffNum = tariffToChange.NewTariffNum;

					if (!listOfChangedClassifications.Contains(classification.PK))
					{
						listOfChangedClassifications.Add(classification.PK);
					}
					countOfObjectsChanged++;
				}
				if (countOfObjectsChanged % 20 == 0)
				{
					OnProgressChanged(countOfObjectsChanged);
				}
			}
		}

		public void OnProgressChanged(decimal count)
		{
			if (ProgressChanged != null)
			{
				int percentage = productsFound + classificationsFound > 0 ?
								(int)(count / (productsFound + classificationsFound) * 100)
								: 0;

				ProgressChanged(percentage > 100 ? 100 : percentage);
			}
		}

		protected ZQuery GetPartQuery(TariffToChange tariffToChange)
		{
			ZDBOnlyQuery partQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			partQuery.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);

			ZDBOnlySubQuery partPivotQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);

			ZQuery subQuery1 = new ZQuery();
			if (HTSTariffFlag)
			{
				subQuery1.AddToFilter(CusClassPartPivotSchema.CI_ChildType, ClassificationTypeList.Codes.HTI);
				subQuery1.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_ChildType, ClassificationTypeList.Codes.HTE);
				subQuery1.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_ChildType, ClassificationChildTypeList.Codes.Related);
				subQuery1.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_ChildType, ClassificationChildTypeList.Codes.COMPONENT);
			}
			else
			{
				subQuery1.AddToFilter(CusClassPartPivotSchema.CI_ChildType, ClassificationTypeList.Codes.SHB);
			}
			partPivotQuery.AddToFilter(subQuery1);

			partPivotQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);

			ZQuery subQuery = new ZQuery(CusClassPartPivotSchema.CI_TariffNum, tariffToChange.OldTariffNum);
			subQuery.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_SupplementalTariff, tariffToChange.OldTariffNum);
			subQuery.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_TariffNum, tariffToChange.FormattedOldTariff);
			subQuery.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_SupplementalTariff, tariffToChange.FormattedOldTariff);

			partPivotQuery.AddToFilter(subQuery, JoinCondition.And);

			partQuery.AddSubQuery(partPivotQuery, JoinCondition.And);
			return partQuery;
		}

		protected ZQuery GetClassificationQuery(ZString oldTariffNum)
		{
			ZQuery classificationQuery = new ZQuery(CusClassificationSchema.CC_IsActive, true);
			classificationQuery.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			classificationQuery.AddToFilter(CusClassificationSchema.CC_TariffNum, oldTariffNum);

			ZString classType = HTSTariffFlag ? CusClassification.ClassificationType.IMP : CusClassification.ClassificationType.EXP;
			classificationQuery.AddToFilter(CusClassificationSchema.CC_ClassificationType, classType);
			return classificationQuery;
		}

		public delegate void TariffBulkChangedEventHandler(int percentage);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			htsTariffFlag = true;
		}
	}
}
