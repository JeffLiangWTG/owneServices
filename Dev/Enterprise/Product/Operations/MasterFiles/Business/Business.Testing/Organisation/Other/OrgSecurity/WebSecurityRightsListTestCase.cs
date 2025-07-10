using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class WebSecurityRightsListTestCase : TestCaseWithFactory
	{
		public void TestSecurityRightsMaxLengthCorrect()
		{
			Assert(true);

			foreach (WebSecurityRight securityRight in GetNewList())
			{
				Guid guid;
				if (!Guid.TryParse(securityRight.Code, out guid))
				{
					Assert(string.Format("Security right code has max length which is <= that of OrgSecurity.OX_SecurityItemName: {0}", securityRight.Code), securityRight.Code.Length <= OrgSecuritySchema.OX_SecurityItemName.MaxLength);
				}
			}
		}

		protected abstract IWebSecurityRightProvider GetNewList();
	}
}
