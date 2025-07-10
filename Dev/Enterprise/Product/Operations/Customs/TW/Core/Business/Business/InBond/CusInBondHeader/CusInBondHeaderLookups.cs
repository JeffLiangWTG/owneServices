using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondHeaderLookups : Customs.Business.CusInBondHeaderLookups
	{
		public CusInBondHeaderLookups(CusInBondHeader parent)
			: base(parent)
		{
		}

		protected new CusInBondHeader Parent => (CusInBondHeader)base.Parent;

		public ConsigneeCollection ImporterList => new ConsigneeCollection(Factory);

		public ForwarderCollection ForwarderList => new ForwarderCollection(Factory);

		public CodeDescriptionPairList TransportModeCodes => Factory.GetCachedValue("Enterprise.Customs.TW.Business.InBondTransportModeCodes", () =>
		{
			var result = new UntranslatableCodeDescriptionPairList((NoResString)"InBond Transport Mode Codes");
			result.AddRange(new InBondTransportModeCodes());
			return result;
		});

		public RefVesselCollection Vessels => new RefVesselCollection(Factory);

		public TWGlbStaffCollection Staffs => new TWGlbStaffCollection(new BusinessObjectFactory());

		public new ShippingProviderCollection Carriers => new ShippingProviderCollection(Factory);

		public ICodeDescriptionPairList CustomsOfficeList => TWRefCusCodeListTypes.GetCustomsOfficeList(Factory);

		public CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue("Enterprise.Customs.TW.Business.EntryStatusCodeList", () =>
		{
			var list = new UntranslatableCodeDescriptionPairList((NoResString)"Entry Status Code List");
			list.AddRange(new EntryStatusCodeList());
			return list;
		});

		public CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<TWMessageStatusCodeList>();

		public CodeDescriptionPairList BoxNumberList => RegistryHelper.GetBoxNumberList(Factory);
	}
}
