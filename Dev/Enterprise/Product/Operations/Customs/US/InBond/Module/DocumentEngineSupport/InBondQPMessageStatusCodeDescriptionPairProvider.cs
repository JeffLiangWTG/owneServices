using static Enterprise.Integration.Customs.US.InBond;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Module
{
	public class InBondQPMessageStatusCodeDescriptionPairProvider : DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider, IInBondQPMessageStatusCodeDescriptionPairProvider
	{
		#region ICodeDescriptionPairListProvider
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return FilterBO.Lookups.InbondQPMessageStatusListForFilter;
		}
		#endregion

		CusInBondHeaderFilterStripBusinessObject FilterBO
		{
			get
			{
				if (filterBO == null)
				{
					filterBO = new CusInBondHeaderFilterStripBusinessObject();
				}
				return filterBO;
			}
		}
		CusInBondHeaderFilterStripBusinessObject filterBO;
	}
}
