using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class PackageLookups : CusDecHouseContainerPackLookups
	{
		public PackageLookups(Package package)
			: base(package)
		{
		}

		new Package Parent
		{
			get { return (Package)base.Parent; }
		}

		public BillCollection BillList
		{
			get { return Parent.Declaration.Bills; }
		}

		public ICusContainerCollection<BaseCusContainer> ContainerList => Parent.Declaration.CusContainers;

		public CodeDescriptionPairList PackTypeList
		{
			get { return Factory.GetCachedValue<ShippingOrPackingingUnitList>(); }
		}
	}
}
