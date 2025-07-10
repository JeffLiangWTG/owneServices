using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class OrgSupplierPart : Customs.Business.OrgSupplierPart, Integration.Customs.SG.IOrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new OrgSupplierPartLookups Lookups => (OrgSupplierPartLookups)base.Lookups;

		public new OrgSupplierPartValidation Validation => (OrgSupplierPartValidation)base.Validation;

		public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)base.PivotsForBinding;

		public ZString ExportClassificationLookup
		{
			get
			{
				var exportClassCodes = GetExpClassCodesFromAllPivots().Take(2).ToArray();
				return exportClassCodes.Length == 0 ? "" : exportClassCodes.Length > 1 ? MultipleValues : exportClassCodes[0].ToString();
			}
		}

		[MaxLength(Schema.OP_BrandMaxLength)]
		public override ZString OP_Brand
		{
			get { return base.OP_Brand; }
			set { base.OP_Brand = value; }
		}

		[MaxLength(Schema.OP_ModelMaxLength)]
		public override ZString OP_Model
		{
			get { return base.OP_Model; }
			set { base.OP_Model = value; }
		}

		public ZString ImportClassificationLookup
		{
			get
			{
				var importClassCodes = GetImpClassCodesFromAllPivots().Take(2).ToArray();
				return importClassCodes.Length == 0 ? "" : importClassCodes.Length > 1 ? MultipleValues : importClassCodes[0].ToString();
			}
		}

		protected override MasterFiles.Business.OrgSupplierPartValidation GetNewValidation() => new OrgSupplierPartValidation(this);

		protected override MasterFiles.Business.OrgSupplierPartLookups GetNewLookups() => new OrgSupplierPartLookups(this);

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.Singapore);

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<Classification>(this, Core.Constants.CountryCodes.Singapore);

		IEnumerable<ZString> GetImpClassCodesFromAllPivots()
		{
			foreach (var pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Singapore))
			{
				if (pivot.Classification != null && pivot.IsImportClassification)
				{
					yield return pivot.Classification.CC_LookupCode;
				}
			}
		}

		IEnumerable<ZString> GetExpClassCodesFromAllPivots()
		{
			foreach (var pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Singapore))
			{
				if (pivot.Classification != null && pivot.IsExportClassification)
				{
					yield return pivot.Classification.CC_LookupCode;
				}
			}
		}
	}
}
