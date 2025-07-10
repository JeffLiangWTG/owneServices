using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusBrokerStaffLookups : ZLookups
	{
		public CusBrokerStaffLookups(CusBrokerStaff parent)
			 : base(parent)
		{
		}

		protected new CusBrokerStaff Parent => (CusBrokerStaff)base.Parent;

		protected override BusinessObjectFactory Factory => Parent.Factory;

		public CodeDescriptionPairList MailboxList => Parent.BrokerStaff?.GetCustomsProfile() ?? new CodeDescriptionPairList();

		public TWGlbStaffCollection BrokerStaffList => new TWGlbStaffCollection(Factory);
	}
}
