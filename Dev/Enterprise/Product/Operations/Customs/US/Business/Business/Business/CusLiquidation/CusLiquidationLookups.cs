//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusLiquidationLookups
//
//    This class should be used for overriding collections in AutoCusLiquidationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusLiquidationLookups : AutoCusLiquidationLookups
	{
		public CusLiquidationLookups(AutoCusLiquidation parent) : base(parent)
		{
		}

		new CusLiquidation Parent => (CusLiquidation)base.Parent;

		public CodeDescriptionPairList ExtensionSuspensionCodeList
		{
			get
			{
				return Factory.GetCachedValue("ExtensionSuspensionCodeList" + ZDateTime.Today.ToShortDateString(), delegate
				{
					return new ExtensionSuspensionCodeList(Factory);
				});
			}
		}

		public ChangeLiquidationReasonCodeList ChangeLiquidationReasonCodeList => Factory.GetCachedValue<ChangeLiquidationReasonCodeList>();

		public LiquidationTypeCodeList LiquidationTypeCodeList => Factory.GetCachedValue<LiquidationTypeCodeList>();

		public CodeDescriptionPairList EntryTypeList => GetEntryTypeList(Factory);

		public static CodeDescriptionPairList GetEntryTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("LiquidationNoticeEntryTypeList", delegate
				{
					var result = new EntryTypeList();
					result.RemoveInBondEntryTypes();
					result.RemoveDrawbackSummaryEntryTypes();
					result.Sort();
					return result;
				});
		}

		public JobDeclarationCollection JobDeclarationList => new JobDeclarationCollection(Factory, Parent.Company.PK);
	}
}
