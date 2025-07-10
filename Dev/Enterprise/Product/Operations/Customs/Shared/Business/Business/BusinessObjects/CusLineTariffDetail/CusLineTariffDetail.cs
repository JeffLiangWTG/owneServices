using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(BaseJobComInvoiceLine), nameof(BaseJobComInvoiceLine.CusLineTariffDetails))]
	[SingleObjectAroundARow]
	public class CusLineTariffDetail : AutoCusLineTariffDetail, Integration.Customs.ICusLineTariffDetail, IDataModelSupporter
	{
		public CusLineTariffDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString BZ_ParentTableCode
		{
			get => base.BZ_ParentTableCode;
			set => base.BZ_ParentTableCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.TariffTypeList))]
		public override ZString BZ_Type
		{
			get => base.BZ_Type;
			set => base.BZ_Type = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.QuantityUnitList))]
		public override ZString BZ_UQ1
		{
			get => base.BZ_UQ1;
			set => base.BZ_UQ1 = value;
		}

		public override ZString BZ_DataModel
		{
			get { return base.BZ_DataModel; }
			set
			{
				this.ReportDataModelErrorIfNeeded(BZ_DataModelInfo, value);
				base.BZ_DataModel = value;
			}
		}

		protected override bool SupportsCloneCore() => true;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				CusLineTariffDetailSchema.Constants.BZ_ParentID,
				CusLineTariffDetailSchema.Constants.BZ_ParentTableCode
			};

			return result;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateDataModelIfNeeded();
		}

		public ICusLineTariffDetailParent Parent => GetParent();

		ICusLineTariffDetailParent GetParent()
		{
			ICusLineTariffDetailParent result = null;

			if (!IsDeleted)
			{
				switch (BZ_ParentTableCode)
				{
					case JobComInvoiceLineSchema.Constants.Prefix:
						result = InvoiceLine;
						break;
					case CusClassPartPivotSchema.Constants.Prefix:
						result = PartPivot;
						break;
				}
			}

			return result;
		}

		public BaseJobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null || invoiceLine.IsDeleted)
				{
					invoiceLine = GetParentBizObj<BaseJobComInvoiceLine>(JobComInvoiceLineSchema.Constants.Prefix);
				}
				return invoiceLine;
			}
		}
		BaseJobComInvoiceLine invoiceLine;

		public BaseCusClassPartPivot PartPivot
		{
			get
			{
				if (partPivot == null || partPivot.IsDeleted)
				{
					partPivot = GetParentBizObj<BaseCusClassPartPivot>(CusClassPartPivotSchema.Constants.Prefix);
				}
				return partPivot;
			}
		}
		BaseCusClassPartPivot partPivot;

		T GetParentBizObj<T>(ZString prefix)
			where T : BusinessObject
		{
			T result = null;

			if (!IsDeleted)
			{
				var parentPk = BZ_ParentID;
				if (parentPk.IsValid && BZ_ParentTableCode == prefix)
				{
					result = Factory.Load<T>(parentPk);
				}
			}

			return result;
		}

		public virtual RefCusTariffType UniversalTariffType
		{
			get
			{
				var tariffType = BZ_Type;
				return !tariffType.IsEmpty ? GetAdditionalDutiesTariffTypeList()[tariffType] : null;
			}
		}

		public virtual TariffView UniversalTariff
		{
			get
			{
				var type = BZ_Type;
				var tariff = BZ_Tariff;
				return !type.IsEmpty && !tariff.IsEmpty ? new TariffView.Loader(Factory).LoadMostRecentCachedTariff(CustomsCountryCode, type, tariff, EffectiveAssessmentDate) : null;
			}
		}

		protected internal virtual AdditionalDutiesTariffTypeList GetAdditionalDutiesTariffTypeList()
			=> Factory.GetCachedValue($"{CustomsCountryCode}.CusLineTariffDetailLookups.GetAdditionalDutiesTariffTypeList", () => new AdditionalDutiesTariffTypeList(Factory, CustomsCountryCode));

		public virtual ZString CustomsCountryCode => Parent?.CustomsCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public virtual ZDateTime EffectiveAssessmentDate => Parent?.EffectiveAssessmentDate ?? ZDateTime.Today;

		#region IDataModelSupporter

		public void PopulateDataModelIfNeeded()
		{
			if (!IsDeleted)
			{
				if (GetParent() is IDataModelSupporter parent)
				{
					this.PopulateDataModelFromParentIfNeeded(parent);
				}
				else
				{
					this.PopulateDataModelFromCountryCodeIfNeeded(CustomsCountryCode);
				}
			}
		}

		ZString IDataModelSupporter.DataModel { get => BZ_DataModel; set => BZ_DataModel = value; }

		#endregion
	}
}
