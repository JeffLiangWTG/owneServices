using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using OrgSupplierPartCollection = Enterprise.MasterFiles.Business.OrgSupplierPartCollection;

namespace Enterprise.eTail.Business
{
	public class HVLVItemLineLookups : AutoHVLVItemLineLookups
	{
		public HVLVItemLineLookups(AutoHVLVItemLine parent) : base(parent)
		{
		}

		public new HVLVItemLine Parent => (HVLVItemLine)base.Parent;

		public OrgSupplierPartCollection Products
		{
			get
			{
				var eTailer = Parent.ParentItem?.ETailer;
				var collection = new OrgSupplierPartCollection(Factory, eTailer, null, true);

				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "FilterCondition", (ZString)ImporterSuplierFilterConditions.Codes.Loose, false));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property2", eTailer?.PK, false));

				return collection;
			}
		}

		public CodeDescriptionPairList WeightUnitList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public IBaseClassificationCollection<BaseCusClassification> ClassificationList => new BaseClassificationCollection<BaseCusClassification>(Factory);
	}
}
