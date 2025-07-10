using Enterprise.Customs.Common.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusContainerLookups : Customs.Business.CusContainerLookups
	{
		public CusContainerLookups(CusContainer parent)
			: base(parent)
		{
		}

		public CusContainer Container
		{
			get { return Parent; }
		}

		protected new CusContainer Parent
		{
			get { return (CusContainer)base.Parent; }
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get { return Factory.GetCachedValue<ImportMessageStatusList>(); }
		}
	}
}
