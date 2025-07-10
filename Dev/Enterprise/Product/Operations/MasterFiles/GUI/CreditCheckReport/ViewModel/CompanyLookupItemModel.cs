using System.Collections.Generic;
using System.Linq;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class CompanyLookupItemModel : ModelBase<CompanyLookupItemModel>
	{
		public CompanyLookupItemModel(ResponseCompanyItem companyItem)
		{
			CompanyItem = companyItem;
		}

		public bool Selected
		{
			get => selected;
			set
			{
				selected = value;
				NotifyPropertyChanged();
			}
		}
		bool selected;

		public IEnumerable<Identifier> Identifiers => CompanyItem.Identifiers;

		public string DUNS => GetIdentifier(IdentifierType.DUNS);

		public string ABN => GetIdentifier(IdentifierType.ABN);

		public string ACN => GetIdentifier(IdentifierType.ACN);

		public string CompanyItemName => CompanyItem.Name;

		public string CompanyItemAddress => CompanyItem.Address;

		public string CompanyItemScore => ResourceStringHelper.GetCompanyItemScore(CompanyItem.Score);

		public ResponseCompanyItem CompanyItem { get; }

		string GetIdentifier(IdentifierType type) => Identifiers?.FirstOrDefault(t => t.Type == type)?.ID;
	}
}
