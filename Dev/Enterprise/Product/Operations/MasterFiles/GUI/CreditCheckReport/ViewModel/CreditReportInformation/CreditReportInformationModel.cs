using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.MasterFiles.Business;
using WTG.ROPE.Model;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.GUI
{
	[CodeAlive("CreditReportInformationWindow, deleted in WI00756942, see respective WI's e-doc for details.")]
	public class CreditReportInformationModel : ModelBase<CreditReportInformationModel>
	{
		public CreditReportInformationModel(OrgHeader header, CreditReportExtractedInfo extractedInfo)
		{
			BuildAddressViewModels(header, extractedInfo.AddressInfos);
			BuildRelatedBrandOrCompanyNameViewModels(extractedInfo.RelatedBrandOrCompanyNames);
			BuildRegistrationNumberViewModels(header, extractedInfo.RegistrationNums);
			BuildWebsiteViewModels(header, extractedInfo.WebsiteUrl);
		}

		public ObservableCollection<AddressModel> AddressViewModels { get; } = new ObservableCollection<AddressModel>();

		public ObservableCollection<BrandsModel> RelatedBrandOrCompanyNameViewModels { get; } = new ObservableCollection<BrandsModel>();

		public ObservableCollection<RegistrationNumberModel> RegistrationNumberViewModels { get; } = new ObservableCollection<RegistrationNumberModel>();

		public ObservableCollection<WebsiteModel> WebsiteViewModels { get; } = new ObservableCollection<WebsiteModel>();

		#region Caption

		public string FormTitle => ResourceStringHelper.CreditReportInformationFormTitle;

		public string CreditReportInformationCaption => ResourceStringHelper.CreditReportInformationCaption;

		public string AddressesCaption => ResourceStringHelper.Addresses;

		public string BrandsCaption => ResourceStringHelper.BrandsAndCompanyNames;

		public string RegistrationNumberCaption => ResourceStringHelper.RegistrationNumber;

		public string WebsiteCaption => ResourceStringHelper.Website;

		public string MergeActionHeader => ResourceStringHelper.Action;

		public string Address1Header => ResourceStringHelper.Address1;

		public string CityHeader => ResourceStringHelper.City;

		public string MatchAddressHeader => ResourceStringHelper.MatchAddress;

		public string NewBrandNameHeader => ResourceStringHelper.NewBrandName;

		public string NewRegistrationNumberHeader => ResourceStringHelper.NewRegistrationNumber;

		public string CurrentRegistrationNumberHeader => ResourceStringHelper.CurrentRegistrationNumber;

		public string RegistrationNumberType => ResourceStringHelper.RegistrationNumberType;

		public string PrimaryHeader => ResourceStringHelper.Primary;

		public string NewWebsiteHeader => ResourceStringHelper.NewWebsite;

		public string CurrentWebsiteHeader => ResourceStringHelper.CurrentWebsite;

		public string SkipCaption => ResourceStringHelper.SkipCaption;

		public string UpdateOrganizationCaption => ResourceStringHelper.UpdateOrganizationCaption;

		#endregion

		#region Implementation

		void BuildAddressViewModels(OrgHeader header, IEnumerable<CreditReportExtractAddress> extractedInfoAddressInfos)
		{
			foreach (var addressInfo in extractedInfoAddressInfos)
			{
				if (!string.IsNullOrEmpty(addressInfo.Address))
				{
					AddressViewModels.Add(new AddressModel(addressInfo, FindMatchAddress(addressInfo, header), header.Addresses.Cast<OrgAddress>().ToList()));
				}
			}
		}

		OrgAddress FindMatchAddress(CreditReportExtractAddress extractedAddress, OrgHeader header)
		{
			OrgAddress result = null;
			foreach (var address in header.Addresses.Cast<OrgAddress>())
			{
				var addressToMatch = string.IsNullOrEmpty(address.Address2.ToString())
					? address.Address1.ToString()
					: address.Address1 + ", " + address.Address2;
				var oldAddressForComparison = AddressLineFuzzyMatch.GetStringForFuzzyComparing(addressToMatch);
				var newAddressForComparison = AddressLineFuzzyMatch.GetStringForFuzzyComparing(extractedAddress.Address);

				if (!string.IsNullOrEmpty(oldAddressForComparison) && oldAddressForComparison.CompareTo(newAddressForComparison) == 0)
				{
					result = address;
					break;
				}
			}

			return result;
		}

		void BuildRelatedBrandOrCompanyNameViewModels(IEnumerable<string> relatedBrandOrCompanyNames)
		{
			if (relatedBrandOrCompanyNames != null)
			{
				foreach (var brandName in relatedBrandOrCompanyNames)
				{
					RelatedBrandOrCompanyNameViewModels.Add(new BrandsModel(brandName));
				}
			}
		}

		void BuildRegistrationNumberViewModels(OrgHeader header, IEnumerable<CreditReportExtractRegistrationNumber> registrationNumbers)
		{
			foreach (var regNo in registrationNumbers)
			{
				var dbCodeType = CreditReportHelper.MapCodeType(regNo.Type);
				var currentRegistryNumber = header.CustomsCodes.GetCustomsRegNo(dbCodeType);
				var newRegistrationNumber = regNo.Number;
				if (currentRegistryNumber != newRegistrationNumber)
				{
					RegistrationNumberViewModels.Add(new RegistrationNumberModel(currentRegistryNumber, newRegistrationNumber, regNo.Type));
				}
			}
		}

		void BuildWebsiteViewModels(OrgHeader header, string websiteUrl)
		{
			if (!string.IsNullOrEmpty(websiteUrl))
			{
				WebsiteViewModels.Add(new WebsiteModel(header.MainWebURL.PU_URL, websiteUrl));
			}
		}

		#endregion
	}
}
