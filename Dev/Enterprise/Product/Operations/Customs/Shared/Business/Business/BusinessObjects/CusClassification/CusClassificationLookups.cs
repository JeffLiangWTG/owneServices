//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR ITS INHERITED CLASS (it will break AutoCusClassification)
//
//    This class should be used for overriding collections in AutoCusClassificationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusClassificationLookups : AutoCusClassificationLookups
	{
		public CusClassificationLookups(AutoCusClassification parent) : base(parent)
		{
		}

		public GlbStaffCollection Staffs
		{
			get
			{
				if (fStaffs == null)
				{
					fStaffs = new GlbStaffCollection(Factory);
				}
				return fStaffs;
			}
		}
		GlbStaffCollection fStaffs;

		#region Classification collection

		BaseClassificationCollection<BaseCusClassification> fClassifications;
		public BaseClassificationCollection<BaseCusClassification> Classifications
		{
			get
			{
				if (fClassifications == null)
				{
					fClassifications = new BaseClassificationCollection<BaseCusClassification>(Factory, new ZQuery(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				}
				return fClassifications;
			}
		}

		#endregion
	}
}
