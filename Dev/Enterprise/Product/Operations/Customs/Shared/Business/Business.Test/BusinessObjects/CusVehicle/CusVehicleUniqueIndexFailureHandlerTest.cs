using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusVehicleUniqueIndexFailureHandler))]
	public class CusVehicleUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestHandledUniqueIndexNames()
		{
			var vehicle = Factory.New<CusVehicle>();
			var handler = new CusVehicleUniqueIndexFailureHandler(vehicle);
			var name = handler.HandledUniqueIndexNames.Single();
			AssertEquals("HandledUniqueIndexName", "NR_UX__CVH_ParentID", name);
		}

		public void TestNotifyUserAndAttemptToResolve()
		{
			Factory.RefreshEnabled = false;

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CES";
			company.GC_RN_NKCountryCode = "PL";
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "BES";

			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			var parent = jobDeclaration
				.JobComInvoiceGroupHeaders[0]
				.JobComInvoiceHeaders.AddNew()
				.InvoiceLines.AddNew();
			jobDeclaration.JE_GC = company.PK;
			jobDeclaration.JE_GB = branch.PK;

			Factory.Save();

			var vehicle1 = Factory.New<CusVehicle>();
			vehicle1.CVH_ParentTableCode = parent.TablePrefix;
			vehicle1.CVH_ParentID = parent.PK;
			vehicle1.CVH_VehicleIdentificationNumber = "VIN1";
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var vehicle2 = factory2.New<CusVehicle>();
			vehicle2.CVH_ParentTableCode = parent.TablePrefix;
			vehicle2.CVH_ParentID = parent.PK;
			vehicle2.CVH_VehicleIdentificationNumber = "VIN2";

			try
			{
				factory2.Save();
			}
			catch (ZSaveException exception)
			{
				ZExceptionReporting.HandleSaveException(exception);
			}

			var existingVehicle = factory2.Load<CusVehicle>(vehicle1.PK);
			var expectedMessage = $"While you were working, Vehicle details have been entered for '{parent.HumanReadableName}' by another user ({existingVehicle.CVH_SystemLastEditUser + " @ " + existingVehicle.CVH_SystemLastEditTimeUtc}). Your changes have been merged, please review your changes and save again.";
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
