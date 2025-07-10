using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.TW.Business.IncoTermsCodeDescriptionPairList;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent)
			: base(parent)
		{
		}

		public CusEntryHeader EntryHeader
		{
			get { return Parent; }
		}

		protected new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		public RefCurrencyCollection CurrencyList
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public CodeDescriptionPairList IncoTermList => Factory.GetCachedValue("CusEntryHeaderLookups|IncoTermList", () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(IncoTerms.CostInsuranceAndFreight, Descriptions.CostInsuranceAndFreight);
			list.AddPair(IncoTerms.CostAndFreight, Descriptions.CostAndFreight);
			list.AddPair(IncoTerms.FreeOnBoard, Descriptions.FreeOnBoard);
			list.AddPair(IncoTerms.CostAndInsurance, Descriptions.CostAndInsurance);
			list.AddPair(IncoTerms.FreeAlongsideShip, Descriptions.FreeAlongsideShip);
			list.AddPair(IncoTerms.ExWorks, Descriptions.ExWorks);
			return list;
		});
	}
}
