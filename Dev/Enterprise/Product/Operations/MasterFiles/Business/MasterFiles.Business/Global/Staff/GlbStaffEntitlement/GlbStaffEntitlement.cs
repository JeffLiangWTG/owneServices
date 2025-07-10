using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(GlbStaffRemuneration), "Entitlements")]
	public class GlbStaffEntitlement : AutoGlbStaffEntitlement
	{
		public GlbStaffEntitlement(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[RelatedBusinessObject("Remuneration")]
		public override ZGuid GSI_GSR_Remuneration { get => base.GSI_GSR_Remuneration; set => base.GSI_GSR_Remuneration = value; }

		public virtual GlbStaffRemuneration Remuneration
		{
			get { return (GlbStaffRemuneration)Factory.Load(typeof(GlbStaffRemuneration), GSI_GSR_Remuneration); }
		}
	}
}
