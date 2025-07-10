using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class CompanyLookupModel
	{
		public CompanyLookupModel(IEnumerable<ResponseCompanyItem> companyItems)
		{
			foreach (var companyItem in companyItems)
			{
				CompanyLookupItemModels.Add(new CompanyLookupItemModel(companyItem));
			}

			if (CompanyLookupItemModels.Any())
			{
				CompanyLookupItemModels.FirstOrDefault().Selected = true;
			}
		}

		public string Confirm => ResourceStringHelper.Confirm;

		public string Cancel => ResourceStringHelper.Cancel;

		public string Title => ResourceStringHelper.Title;

		public ObservableCollection<CompanyLookupItemModel> CompanyLookupItemModels { get; set; } = new ObservableCollection<CompanyLookupItemModel>();
	}
}
