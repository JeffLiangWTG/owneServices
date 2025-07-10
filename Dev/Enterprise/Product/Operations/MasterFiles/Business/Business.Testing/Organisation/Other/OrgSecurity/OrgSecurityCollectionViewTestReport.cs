using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityCollectionView))]
	class OrgSecurityCollectionViewTestReport : BusinessObjectCollectionViewTestCase<OrgSecurityCollectionView>
	{
		protected new OrgSecurityCollectionView Collection
		{
			get { return base.Collection; }
		}

		protected override OrgSecurityCollectionView GetCollectionToTest()
		{
			var securityCollection = new OrgSecurityCollection(Factory);
			var view = new OrgSecurityCollectionView(securityCollection);
			return view;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "RepTest";
			report.SU_MenuName = "Test Report";
			report.SU_MenuType = "WEB";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgSecurity = Factory.New<OrgSecurity>();
			orgSecurity.OX_SecurityItemName = string.Empty;
			orgSecurity.OX_OH = org.PK;
			orgSecurity.OX_SU = report.PK;
			orgSecurity.OX_Granted = true;

			return orgSecurity;
		}
	}
}
