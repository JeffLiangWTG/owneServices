using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartBarcodeLookups : AutoOrgSupplierPartBarcodeLookups
	{
		public OrgSupplierPartBarcodeLookups(AutoOrgSupplierPartBarcode parent)
			: base(parent)
		{
		}

		#region ProductUQList

		public CodeDescriptionPairList ProductUQList
		{
			get
			{
				return Factory.GetCachedValue("OrgSupplierPartBarcodeLookups|ProductUQList",
					delegate
					{
						return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits();
					});
			}
		}

		#endregion
	}
}
