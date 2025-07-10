using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(GlbStaffWrapper))]
sealed class PLGlbStaffWrapperTest : MasterFiles.Business.Testing.GlbStaffWrapperTest<GlbStaffWrapper>
{
	public void TestIsNCTSPhase5()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Should be true", true, Wrapper.IsNCTSPhase5);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase4, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, true))
			{
				AssertEquals("Should be false", false, Wrapper.IsNCTSPhase5);
			}
		});
	}

	public void TestSeapId()
	{
		var seapId = Wrapper.SeapId;
		CombineAssertions(() =>
		{
			AssertEquals("Staff", Staff.PK, seapId.GP_GS);
			AssertEquals("Company", GlbCompany.CurrentCompany.PK, seapId.GP_GC);
		});
	}

	public void TestCommunicationChannel()
	{
		var communicationChannel = Wrapper.CommunicationChannel;
		CombineAssertions(() =>
		{
			AssertEquals("Stadff", Staff.PK, communicationChannel.GP_GS);
			AssertEquals("Company", GlbCompany.CurrentCompany.PK, communicationChannel.GP_GC);
		});
	}

	protected override GlbStaffWrapper CreateNewWrapper(GlbStaff staff)
	{
		return GlbStaffWrapper.Get(staff);
	}
}
