using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(ClientRatesController))]
	public class ClientRatesControllerTest : RatingControllerTest<ClientRatesController, ClientRate>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ClientRates;
		}

		protected override SecurityCheckpoint DefaultCheckPointForView
		{
			get { return Env.Security.ClientRatesView; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForEdit
		{
			get { return Env.Security.ClientRatesEdit; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForDelete
		{
			get { return Env.Security.ClientRatesDelete; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForCopy
		{
			get { return Env.Security.ClientRatesCopy; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForNew
		{
			get { return Env.Security.ClientRatesNew; }
		}

		protected override CRMSecurity DefaultCRMSecurity
		{
			get
			{
				return Env.Security.ClientRatesCRMSecurity;
			}
		}

		protected override RatingHeader GetRatingHeader()
		{
			return Helper.NewClientRate(Helper.NewOrgHeader());
		}

		protected override RatingHeader GetGlobalRatingHeader()
		{
			return Helper.NewGlobalClientRate(Helper.NewOrgHeader());
		}

		public void TestCheckPointsForView()
		{
			AssertSecurityCheckPointsForView(Env.Security.GlobalClientRatesView);
		}

		public void TestCheckPointsForNew()
		{
			AssertSecurityCheckPointsForNew(Env.Security.GlobalClientRatesNew);
		}

		public void TestCheckPointsForEdit()
		{
			AssertSecurityCheckPointsForEdit(Env.Security.GlobalClientRatesEdit);
		}

		public void TestCheckPointsForDelete()
		{
			AssertSecurityCheckPointsForDelete(Env.Security.GlobalClientRatesDelete);
		}

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<ClientRate>();
			bizObjWithoutAccess.Header.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			CRMSecurityProviderTest<ClientRate>.AssertController(new ClientRatesController(), bizObjWithoutAccess, Env.Security.ClientRatesCRMSecurity);
		}

		#endregion
	}
}
