using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(GlbStaff), "OneOffEntitlements")]
	public class GlbStaffOneOffEntitlement : AutoGlbStaffOneOffEntitlement
	{
		public GlbStaffOneOffEntitlement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Staff")]
		public override ZGuid GSO_GS_Staff { get => base.GSO_GS_Staff; set => base.GSO_GS_Staff = value; }

		public virtual GlbStaff Staff
		{
			get { return Factory.Load<GlbStaff>(GSO_GS_Staff); }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			GSO_PaymentType = "BON";
			GSO_ExpiryDate = ZDate.Empty;
			GSO_ClawbackDate = ZDate.Empty;
			GSO_AchievedDate = ZDate.Empty;
		}
#endif
	}
}
