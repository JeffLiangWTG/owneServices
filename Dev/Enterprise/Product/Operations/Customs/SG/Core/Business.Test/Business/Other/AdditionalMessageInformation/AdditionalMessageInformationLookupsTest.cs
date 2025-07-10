using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AdditionalMessageInformationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRefundCodeList()
		{
			Assert(AdditionalMessageInformationLookups.RefundCodeList.Count > 0);
		}

		public void TestUpdateIndicatorList()
		{
			Assert(AdditionalMessageInformationLookups.UpdateIndicatorList.Count > 0);
			Assert(!AdditionalMessageInformationLookups.UpdateIndicatorList.ContainsCode(SGConstants.UpdateIndicators.CNL));
		}

		public void TestCancellationCodeList()
		{
			Assert(AdditionalMessageInformationLookups.CancellationCodeList.Count > 0);
		}

		public void TestBrokersCodeList()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbStaff staff = factory.New<GlbStaff>();
			var wrapper = SGGlbStaffWrapper.Get(staff);
			wrapper.Tradenetv4Password.GP_UserID = "mail1";
			wrapper.Tradenetv4Password.GP_CurrentPassword = "0KKwb6ydAlmfdLUP+jgqwUGsR6GLqfWCNrg9I0zCnfI=";
			staff.GS_Code = "TST";
			staff.GS_LoginName = "test1";
			GlbStaff staff2 = factory.New<GlbStaff>();
			staff2.GS_Code = "TSS";
			staff2.GS_LoginName = "test2";
			factory.Save();
			Assert(AdditionalMessageInformationLookups.Brokers.Count == 1);
		}

		#region Implementation
		AdditionalMessageInformation AdditionalMessageInformation
		{
			get
			{
				return additionalMessageInformation ?? (additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory));
			}
		}

		AdditionalMessageInformation additionalMessageInformation;
		AdditionalMessageInformationLookups AdditionalMessageInformationLookups
		{
			get
			{
				return AdditionalMessageInformation.Lookups;
			}
		}
		#endregion
	}
}
