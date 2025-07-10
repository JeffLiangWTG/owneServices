using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class RefMessagingBussPackageInfo : AutoRefMessagingBussPackageInfo
	{
		public RefMessagingBussPackageInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("RefMessagingBussPackageInfo|ZMP_PackageName", Caption = "Package Name")]
		public override ZString ZMP_PackageName => base.ZMP_PackageName;
	}
}
