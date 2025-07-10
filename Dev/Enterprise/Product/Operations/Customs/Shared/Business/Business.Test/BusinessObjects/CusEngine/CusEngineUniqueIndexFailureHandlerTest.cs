using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

[TestedType(typeof(CusEngineUniqueIndexFailureHandler))]
public class CusEngineUniqueIndexFailureHandlerTest : TestCaseWithFactory
{
	public void TestHandledUniqueIndexNames()
	{
		var engine = Factory.New<CusEngine>();
		var handler = new CusEngineUniqueIndexFailureHandler(engine);
		var name = handler.HandledUniqueIndexNames.Single();
		AssertEquals("HandledUniqueIndexName", "NR_UX__CEG_ParentID", name);
	}

	public void TestNotifyUserAndAttemptToResolve()
	{
		Factory.RefreshEnabled = false;

		var company = Factory.New<GlbCompany>();
		company.GC_Code = "CTR";
		company.GC_RN_NKCountryCode = "TR";
		var branch = Factory.New<GlbBranch>();
		branch.GB_GC = company.PK;
		branch.GB_Code = "BTR";

		var jobDeclaration = Factory.New<BaseJobDeclaration>();
		jobDeclaration.JE_GC = company.PK;
		jobDeclaration.JE_GB = branch.PK;

		var parent = Factory.NewWithValidTestData<CusVehicleForTesting>();
		parent.EngineRelationshipForTesting = EngineRelationshipType.One;
		parent.CVH_ParentTableCode = jobDeclaration.TablePrefix;
		parent.CVH_ParentID = jobDeclaration.PK;

		var engine1 = Factory.New<CusEngine>();
		engine1.CEG_ParentTableCode = parent.TablePrefix;
		engine1.CEG_ParentID = parent.PK;
		engine1.CEG_EngineNumber = "ENGINE1";
		Factory.Save();

		var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
		var engine2 = factory2.New<CusEngine>();
		engine2.CEG_ParentTableCode = parent.TablePrefix;
		engine2.CEG_ParentID = parent.PK;
		engine2.CEG_EngineNumber = "ENGINE2";

		try
		{
			factory2.Save();
		}
		catch (ZSaveException exception)
		{
			ZExceptionReporting.HandleSaveException(exception);
		}

		var existingEngine = factory2.Load<CusEngine>(engine1.PK);
		var expectedMessage = $"While you were working, Engine details have been entered for '{parent.HumanReadableName}' by another user ({existingEngine.CEG_SystemLastEditUser + " @ " + existingEngine.CEG_SystemLastEditTimeUtc}). Your changes have been merged, please review your changes and save again.";
		AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
	}
}
