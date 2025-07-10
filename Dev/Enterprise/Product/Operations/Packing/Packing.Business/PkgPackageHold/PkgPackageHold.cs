using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public class PkgPackageHold : AutoPkgPackageHold
	{
		public PkgPackageHold(BusinessObjectFactory factory, DataRow row)
			   : base(factory, row) { }

		#region Related Entities

		#region Package

		public PkgPackage Package
		{
			get { return Factory.Load<PkgPackage>(KHR_KP_Package); }
		}

		#endregion

		#endregion

		#region Properties

		#region K0_KP_Package

		[RelatedBusinessObject("Package")]
		public override ZGuid KHR_KP_Package
		{
			get { return base.KHR_KP_Package; }
			set { base.KHR_KP_Package = value; }
		}

		#endregion

		#endregion
	}
}
