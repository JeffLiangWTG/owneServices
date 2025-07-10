using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business.Declaration;

public class GuaranteeBondDetailCollection : GuaranteeForEntryInstructionCollection
{
	public GuaranteeBondDetailCollection(CusEntryInstruction cei)
		: base(cei)
	{
		entryInstruction = cei;
	}
	readonly CusEntryInstruction entryInstruction;

	public new GuaranteeBondDetail this[int index] => (GuaranteeBondDetail)Elements[index];

	protected override BusinessObject AddNewCore() => AddNew(typeof(GuaranteeBondDetail));

	public new GuaranteeBondDetail AddNew() => (GuaranteeBondDetail)base.AddNew();

	protected new GuaranteeBondDetail AddNew(Type type) => (GuaranteeBondDetail)base.AddNew(type);

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		GuaranteeBondDetail guaranteeForEntryInstruction = child as GuaranteeBondDetail;
		if ((object)guaranteeForEntryInstruction != null)
		{
			guaranteeForEntryInstruction.PW_ApplicationCode = GuaranteeBondDetail.Schema.ApplicationCode;
			guaranteeForEntryInstruction.PW_ActivityCode = GuaranteeBondDetail.Schema.ActivityCode;
			guaranteeForEntryInstruction.PW_RN_NKCountryOfIssue = Core.Constants.CountryCodes.Poland;
			guaranteeForEntryInstruction.PW_RX_NKCurrency = Core.Constants.CurrencyCodes.Poland;
			guaranteeForEntryInstruction.PW_BondAmount = 0;
		}
	}

	protected override void OnCountChanged(CollectionCountChangedEventArgs e)
	{
		base.OnCountChanged(e);
		if (!IsLoading)
		{
			entryInstruction?.SetGuaranteeAmountIfNeeded();
		}
	}

	protected override ZQuery CreateRelationshipFilter()
	{
		var query = base.CreateRelationshipFilter();
		query.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, SQLComparisonOperator.Equal, GuaranteeBondDetail.Schema.ApplicationCode);
		query.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, SQLComparisonOperator.Equal, CusEntryInstructionSchema.Constants.Prefix);
		return query;
	}
}
