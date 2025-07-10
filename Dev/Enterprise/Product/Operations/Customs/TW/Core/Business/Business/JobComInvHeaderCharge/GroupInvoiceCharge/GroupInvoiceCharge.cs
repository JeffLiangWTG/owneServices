using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	public partial class GroupInvoiceCharge : AutoGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZBool AllowNonWesternEuropeanCharacterForChargeDescription => true;

		[ResourceStringData("Enterprise.Customs.TW.Business.GroupInvoiceCharge|J7_IsStatisticalValueApplicable", Caption = "Incl. in FOB")]
		public override ZBool J7_IsStatisticalValueApplicable { get => base.J7_IsStatisticalValueApplicable; set => base.J7_IsStatisticalValueApplicable = value; }
	}
}
