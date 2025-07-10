using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccInvMsg)]
	public class AccInvMsgCollection : ActiveBusinessObjectCollection<AccInvMsg>, Integration.IAccInvMsgCollection
	{
		public AccInvMsgCollection(BusinessObjectFactory factory)
			: this(factory, ZString.Empty)
		{
		}

		public AccInvMsgCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public AccInvMsgCollection(BusinessObjectFactory factory, ZString countryCode) : base(factory)
		{
			this.countryCode = countryCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : countryCode;
		}

		readonly ZString countryCode;

		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter().AddToFilter(AccInvMsgSchema.A9_RN_NKCountryCode, this.countryCode);
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
