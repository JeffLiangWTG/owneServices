using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CustomsNumberViewStmNumsSetting))]
	sealed class CustomsNumberViewStmNumsSettingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var settingMock = new Mock<CustomsNumberViewStmNumsSetting>(new object[] { GlbCompany.CurrentCompany, (ZString)"CEN" });
			settingMock.CallBase = true;
			var setting = settingMock.Object;
			AssertEquals("IsInDatabase", true, setting.IsInDatabase);
			AssertEquals("TablePrefix", GlbCompanySchema.Constants.Prefix, setting.TablePrefix);
			AssertEquals("SupportsClone()", false, setting.SupportsClone());
			AssertEquals("PK", GlbCompany.CurrentCompany.PK, setting.PK);
			AssertEquals("GetPrefix()", ZString.Empty, setting.GetPrefix());
			AssertNull("DefaultTypeRangeMax()", setting.DefaultTypeRangeMax());
			AssertEquals("RequiredDigit()", 8, setting.RequiredDigit());
			AssertEquals("CanRollover()", false, setting.CanRollover());
			AssertEquals("AllowDuplicate()", true, setting.AllowDuplicate());
			AssertEquals("GetThresholdRunOutWarning()", ZLong.Zero, setting.GetThresholdRunOutWarning());

			settingMock.Protected().Setup<ZString>("GetPrefixCore").Returns("HI");
			AssertEquals("GetPrefix()", "HI", setting.GetPrefix());

			settingMock.Protected().Setup<ZLong?>("DefaultTypeRangeMaxCore").Returns(new ZLong(999999));
			AssertEquals("DefaultTypeRangeMax()", 999999L, setting.DefaultTypeRangeMax());

			settingMock.Protected().Setup<int>("RequiredDigitCore").Returns(4);
			AssertEquals("RequiredDigit()", 4, setting.RequiredDigit());

			settingMock.Protected().Setup<ZLong>("GetThresholdRunOutWarningCore").Returns(new ZLong(200));
			AssertEquals("GetThresholdRunOutWarning()", 200L, setting.GetThresholdRunOutWarning());

			settingMock.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CustomsNumberViewStmNumsSetting(GlbCompany.CurrentCompany, "CEN");
		}
	}
}
