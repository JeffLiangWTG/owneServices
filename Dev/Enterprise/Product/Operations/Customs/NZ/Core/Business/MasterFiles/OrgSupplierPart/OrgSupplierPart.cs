using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.MasterFiles
{
	public class OrgSupplierPart : Customs.Business.OrgSupplierPart, Integration.Customs.NZ.IOrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static OrgSupplierPart New(BusinessObjectFactory factory)
		{
			return factory.New<OrgSupplierPart>();
		}

		[ChildEditable(true)]
		public new ClassificationCollection<CusClassification> ClassificationsForBinding => (ClassificationCollection<CusClassification>)base.ClassificationsForBinding;

		public new OrgSupplierPartValidation Validation => (OrgSupplierPartValidation)base.Validation;

		public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)base.PivotsForBinding;

		public ZString ExportClassificationLookup
		{
			get
			{
				var exportClassCodes = GetExpClassCodesFromAllPivots();
				return !exportClassCodes.Any() ? "" : exportClassCodes.Count() > 1 ? MultipleValues : exportClassCodes.FirstOrDefault().ToString();
			}
		}

		public ZString ImportClassificationLookup
		{
			get
			{
				var importClassCodes = GetImpClassCodesFromAllPivots();
				return !importClassCodes.Any() ? "" : importClassCodes.Count() > 1 ? MultipleValues : importClassCodes.FirstOrDefault().ToString();
			}
		}

		IBusinessObjectCollection<Integration.Customs.NZ.ICusClassPartPivot> Integration.Customs.NZ.IOrgSupplierPart.PivotsForBinding => PivotsForBinding;

		IEnumerable<ZString> GetImpClassCodesFromAllPivots()
		{
			foreach (var pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.NewZealand))
			{
				if (pivot.Classification != null && pivot.IsImportClassification)
				{
					yield return pivot.Classification.CC_LookupCode;
				}
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new NZPartFetchStrategy(this);

		protected override bool IsImportChildType(BaseCusClassPartPivot pivot) => pivot.CI_ChildType == ClassificationTypeList.Codes.HTI || pivot.CI_ChildType == ClassificationTypeList.Codes.HTB;

		protected override bool IsExportChildType(BaseCusClassPartPivot pivot) => pivot.CI_ChildType == ClassificationTypeList.Codes.HTE || pivot.CI_ChildType == ClassificationTypeList.Codes.HTB;

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<CusClassification>(this, Core.Constants.CountryCodes.NewZealand);

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.NewZealand);

		protected override Enterprise.MasterFiles.Business.OrgSupplierPartValidation GetNewValidation() => new OrgSupplierPartValidation(this);

		IEnumerable<ZString> GetExpClassCodesFromAllPivots()
		{
			foreach (var pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.NewZealand))
			{
				if (pivot.Classification != null && pivot.IsExportClassification)
				{
					yield return pivot.Classification.CC_LookupCode;
				}
			}
		}

		class NZPartFetchStrategy : OrgSupplierPartFetchStrategy
		{
			public NZPartFetchStrategy(OrgSupplierPart part)
				: base(part)
			{
			}

			protected override bool UseDeepFetchHintForCusClassPartPivot
			{
				get { return true; }
			}
		}
	}
}
