using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Web.WebService;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	abstract class BaseWebServiceTest<T> : WebServiceWithFactoryTest<T>
				where T : BaseWebService, new()
	{
		#region Setup 

		OrgHeader fCurrentOrg;
		protected OrgHeader CurrentOrg
		{
			get
			{
				if (fCurrentOrg == null)
				{
					fCurrentOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
					fCurrentOrg.OH_IsActive = true;
				}
				return fCurrentOrg;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			WebService.MessageHeader = new WebTrackerSOAPHeader();
			WebService.MessageHeader.CompanyCode = CurrentOrg.OH_Code;
		}

		#endregion
	}
}
