//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCustomLabelsLookups
//
//    This class should be used for overriding collections in AutoOrgCustomLabelsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCustomLabelsLookups : AutoOrgCustomLabelsLookups
	{
		public OrgCustomLabelsLookups(AutoOrgCustomLabels parent) : base(parent)
		{
		}
		public CodeDescriptionPairList OT_FieldName_List
		{
			get
			{
				CodeDescriptionPairList result = null;
				switch (Parent.OT_Type)
				{
					case OrgConstants.CustomLabelType.Form:
						result = new CodeDescriptionPairList(OLookUpEditType.CustomLabels);
						break;
					case OrgConstants.CustomLabelType.Document:
						result = new CodeDescriptionPairList(OLookUpEditType.CustomDocuments);
						result.AddRange(new LandedCostingCustomDocumentLabelsList()); //common ones

						var countryCode = ZString.Empty;
						var orgHeader = Parent.ParentOrg;
						if (orgHeader != null)
						{
							countryCode = GlbBranch.FindControllingBranchWithFallBackToAnyCompanyIfOnlyOne(orgHeader)?.GB_RN_NKCountryCode ?? orgHeader.CountryCode;
							if (countryCode.IsEmpty)
							{
								countryCode = orgHeader.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
							}
						}
						var companyPK = ZGuid.Empty;
						if (countryCode.IsEmpty)
						{
							countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							companyPK = GlbCompany.CurrentCompany.PK;
						}
						else if (countryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
						{
							companyPK = GlbCompany.CurrentCompany.PK;
						}

						switch (countryCode)
						{
							case Constants.CountryCodes.Australia:
								result.AddRange(new AULandedCostingCustomDocumentLabelsList());
								break;
							case Core.Constants.CountryCodes.SouthAfrica:
								result.AddRange(new ZALandedCostingCustomDocumentLabelsList());
								result.AddRange(new ZACustomsWorksheetCustomDocumentLabelsList());
								break;
							case Core.Constants.CountryCodes.NewZealand:
								result.AddRange(new NZLandedCostingCustomDocumentLabelsList());
								break;

							case Core.Constants.CountryCodes.UnitedStates:
								result.AddRange(new USLandedCostingCustomDocumentLabelsList());
								break;
						}

						var customsChargeLCItemSettings = ObjectFactory.Get<ICustomsChargeLCItemSettingsProvider>("ICustomsChargeLCItemSettingsProvider").GetCustomsChargeLCItemSettings(countryCode, null, companyPK);
						if (customsChargeLCItemSettings != null)
						{
							foreach (var setting in customsChargeLCItemSettings.Where(x => !x.DocumentCustomLabelCode.IsEmpty))
							{
								result.AddPairIfNotExist("LandedCostingDocument." + setting.DocumentCustomLabelCode, setting.Description);
							}
						}

						result.Sort();
						break;
					default:
						result = new CodeDescriptionPairList();
						break;
				}

				return result;
			}
		}

		#region Implementation

		new OrgCustomLabels Parent
		{
			get { return (OrgCustomLabels)base.Parent; }
		}

		#endregion
	}
}
