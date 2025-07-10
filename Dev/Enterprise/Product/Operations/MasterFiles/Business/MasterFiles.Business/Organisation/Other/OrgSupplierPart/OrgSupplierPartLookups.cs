using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartLookups : AutoOrgSupplierPartLookups
	{
		public OrgSupplierPartLookups(AutoOrgSupplierPart parent) : base(parent)
		{
		}

		#region Organisations
		public OrgHeaderCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrgHeaderCollection(Factory);
				}
				return fOrganisations;
			}
		}
		OrgHeaderCollection fOrganisations;
		#endregion

		#region WeightedCostCurrencyList
		public RefCurrencyCollection WeightedCostCurrencyList
		{
			get
			{
				if (fWeightedCostCurrencyList == null)
				{
					fWeightedCostCurrencyList = new RefCurrencyCollection(Factory);
				}
				return fWeightedCostCurrencyList;
			}
		}
		RefCurrencyCollection fWeightedCostCurrencyList;
		#endregion

		#region OP_ProductUQ_List

		public CodeDescriptionPairList OP_ProductUQ_List
		{
			get { return Factory.GetCachedValue("OrgSupplierPartLookups|OP_ProductUQ_List", () => new OrgSupplierPart.StockUnitPairList(Factory)); }
		}

		#endregion

		#region OP_WeightUQ_List
		public CodeDescriptionPairList OP_WeightUQ_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}
		#endregion

		#region OP_CubicUQ_List
		public CodeDescriptionPairList OP_CubicUQ_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}
		#endregion

		#region OP_MeasureUQ_List
		public CodeDescriptionPairList OP_MeasureUQ_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Length); }
		}
		#endregion

		#region NMFCList

		public virtual RefNMFCCollection NMFCList
		{
			get { return new RefNMFCCollection(Factory, new ZQuery(RefNMFCSchema.FN_IsActive, ZBool.True)); }
		}

		#endregion
	}
}
