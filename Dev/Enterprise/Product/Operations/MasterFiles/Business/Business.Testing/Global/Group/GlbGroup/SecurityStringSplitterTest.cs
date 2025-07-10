using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SecurityStringSplitter))]
	public abstract class SecurityStringSplitterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSecurityRightParts()
		{
			GlbGroupSecurity sec = new GlbGroupSecurity();
			AssertSecurityRightParts(sec, "", "", "", "");

			sec.SecurityRight = "aaa";
			AssertSecurityRightParts(sec, "aaa", "", "", "");

			sec.SecurityRight = "    aaa  ";
			AssertSecurityRightParts(sec, "aaa", "", "", "");

			sec.SecurityRight = "    aaa  ->  bbb";
			AssertSecurityRightParts(sec, "aaa", "bbb", "", "");

			sec.SecurityRight = "    aaa  ->bbb->ccc";
			AssertSecurityRightParts(sec, "aaa", "bbb", "ccc", "");

			sec.SecurityRight = "    aaa  -> bbb      -> ccc -> qqq > eee ->dddd ";
			AssertSecurityRightParts(sec, "aaa", "bbb", "ccc", "qqq > eee ->dddd");
		}

		void AssertSecurityRightParts(GlbGroupSecurity security, string part1, string part2, string part3, string part4)
		{
			AssertEquals(security.SecurityRightPart1, part1);
			AssertEquals(security.SecurityRightPart2, part2);
			AssertEquals(security.SecurityRightPart3, part3);
			AssertEquals(security.SecurityRightPart4, part4);
		}
	}
}
