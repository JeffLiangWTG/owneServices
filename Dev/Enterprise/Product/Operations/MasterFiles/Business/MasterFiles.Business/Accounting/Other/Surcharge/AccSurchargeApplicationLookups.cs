//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccSurchargeApplicationLookups
//
//    This class should be used for overriding collections in AutoAccSurchargeApplicationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccSurchargeApplicationLookups : AutoAccSurchargeApplicationLookups
	{
		public AccSurchargeApplicationLookups(AutoAccSurchargeApplication parent) : base(parent)
		{
		}

		public new AccSurchargeApplication Parent
		{
			get { return (AccSurchargeApplication)base.Parent; }
		}

		#region Job Types

		public CodeDescriptionPairList JobTypes
		{
			get
			{
				CodeDescriptionPairList fJobTypes = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
				fJobTypes.Insert(0, new AllJobsConsumerType());
				CodeDescriptionPair njrPair = new CodeDescriptionPair(AccountingMasterFilesConstants.JobTypes.NonJobRelated, Res.GetString("2f5fd274-5576-47da-bee7-8abf585a5158", "Non-Job"));
				fJobTypes.InsertInSortOrder(njrPair);

				return fJobTypes;
			}
		}

		#endregion

		#region Supply Types
		public CodeDescriptionPairList SupplyTypes
		{
			get
			{
				if (supplyTypes == null)
				{
					var companyPK = Parent.ASP_GC_Company.IsEmpty ? Guid.Empty : Parent.ASP_GC_Company.ToGuid();
					var supplyTypeClassificationCodesList = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
					supplyTypes = supplyTypeClassificationCodesList.GetActiveCodeDescriptionPairList();
				}
				return supplyTypes;
			}
		}

		CodeDescriptionPairList supplyTypes;
		#endregion

		#region Locations
		public CodeDescriptionPairList Locations
		{
			get
			{
				return AccountingTaxLocations.GetLocations(Factory, false);
			}
		}
		#endregion

		#region OrganisationCategoryList

		public CodeDescriptionPairList OrganisationCategoryList
		{
			get
			{
				if (fOrganisationCategoryList == null)
				{
					fOrganisationCategoryList = new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory);
					fOrganisationCategoryList.Insert(0, new CodeDescriptionPair(AccSurchargeApplication.ALL, CategoryAdditionalDescriptions.All));
				}
				return fOrganisationCategoryList;
			}
		}

		CodeDescriptionPairList fOrganisationCategoryList;

		public static class CategoryAdditionalDescriptions
		{
			public static string All
			{
				get { return Res.GetString("88cf5ba0-d0b5-49f3-aea7-1ee1c773893b", "All Categories"); }
			}
		}
		#endregion

		#region SurchargeCodes
		public CodeDescriptionPairList SurchargeCodes
		{
			get
			{
				if (surchargeCodes == null)
				{
					surchargeCodes = new CodeDescriptionPairList();
					var fChargeCodes = new AccSurchargeConfigurationCollection(Factory, Parent.ASP_GC_Company);
					fChargeCodes.Load();
					foreach (var surchargeConfiguration in fChargeCodes.OfType<AccSurchargeConfiguration>())
					{
						if (!surchargeCodes.ContainsCode(surchargeConfiguration.ASC_Code))
						{
							surchargeCodes.AddPair(surchargeConfiguration.ASC_Code, surchargeConfiguration.ASC_Description);
						}
					}
				}
				return surchargeCodes;
			}
		}

		CodeDescriptionPairList surchargeCodes;
		#endregion

		public override AccTaxRateCollection TaxRates
		{
			get
			{
				return new VATAccTaxRateCollection(Factory, Parent.CountryCode);
			}
		}
	}
}
