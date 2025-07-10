using System;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.HRM.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.HRM.Testing
{
	[TestedType(typeof(ReviewProcess))]
	class ReviewProcessTest : EnterpriseBusinessObjectTestCase
	{
		[UseSnapshotProtection]
		public void TestCanCreateWithValidStatus()
		{
			var rp = Factory.NewWithValidTestData<ReviewProcess>();

			rp.RPR_Name = "Who to fire";
			rp.RPR_EffectiveDate = ZDate.Today;
			rp.RPR_SubmissionDate = ZDateTimeOffset.Today;
			rp.RPR_PrimaryHierarchy = "DRM";
			rp.RPR_SystemCreateUser = "E";
			rp.RPR_SystemLastEditUser = "E";

			var f = Factory.New<StmModuleFilter>();
			rp.RPR_S9_EmployeesInReview = f.PK;

			rp.RPR_Status = "UNS";
			AssertNoExceptionThrown("UNS code should be valid", Factory.Save);

			rp.RPR_Status = "BLD";
			AssertNoExceptionThrown("BLD code should be valid", Factory.Save);

			rp.RPR_Status = "WRK";
			AssertNoExceptionThrown("WRK code should be valid", Factory.Save);

			rp.RPR_Status = "CLS";
			AssertNoExceptionThrown("CLS code should be valid", Factory.Save);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestCannotCreateWithInvalidStatus()
		{
			var f = Factory.New<StmModuleFilter>();
			Factory.Save();

			var rp = Factory.NewWithValidTestData<ReviewProcess>();

			rp.RPR_Name = "Who to fire";
			rp.RPR_EffectiveDate = ZDate.Today;
			rp.RPR_SubmissionDate = ZDateTimeOffset.Today;
			rp.RPR_PrimaryHierarchy = "DRM";
			rp.RPR_SystemCreateUser = "E";
			rp.RPR_SystemLastEditUser = "E";

			rp.RPR_S9_EmployeesInReview = f.PK;

			rp.RPR_Status = "PEE";
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "The INSERT statement conflicted with the CHECK constraint \"Constraint_RPR_Status\"", true), "PEE code should be invalid");

			rp.RPR_Status = "BL";
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "The INSERT statement conflicted with the CHECK constraint \"Constraint_RPR_Status\"", true), "BL code should be invalid");
		}

		[UseSnapshotProtection]
		public void TestCreateWithValidTypes()
		{
			var f = Factory.New<StmModuleFilter>();
			Factory.Save();

			var rp = Factory.New<ReviewProcess>();

			rp.RPR_Name = "Dummy Review Process";
			rp.RPR_Type = "REM";
			rp.RPR_ConfigType = "C";
			rp.RPR_EffectiveDate = ZDate.Today;
			rp.RPR_SubmissionDate = ZDateTimeOffset.Today;
			rp.RPR_RX_NKCurrency = "AUD";
			rp.RPR_PrimaryHierarchy = "DRM";
			rp.RPR_SystemCreateUser = "~BP";
			rp.RPR_SystemLastEditUser = "~BP";

			rp.RPR_S9_EmployeesInReview = f.PK;

			AssertNoExceptionThrown("REM code should be valid", Factory.Save);
			rp.RPR_GC_Company = Guid.Empty;
			rp.RPR_Type = "PER";
			rp.RPR_ConfigType = "";
			rp.RPR_RX_NKCurrency = "";

			AssertNoExceptionThrown("PER code should be valid", Factory.Save);

			rp.RPR_Type = "CLA";

			AssertNoExceptionThrown("CLA code should be valid", Factory.Save);

			rp.RPR_Type = "RPL";

			AssertNoExceptionThrown("RPL code should be valid", Factory.Save);

			rp.RPR_Type = "C&P";

			AssertNoExceptionThrown("C&P code should be valid", Factory.Save);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestCannotCreateWithInvalidType()
		{
			var f = Factory.New<StmModuleFilter>();
			Factory.Save();

			var rp = Factory.New<ReviewProcess>();

			rp.RPR_Name = "Dummy Review Process";
			rp.RPR_Type = "PER";
			rp.RPR_ConfigType = string.Empty;
			rp.RPR_EffectiveDate = ZDate.Today;
			rp.RPR_SubmissionDate = ZDateTimeOffset.Today;
			rp.RPR_PrimaryHierarchy = "DRM";
			rp.RPR_SystemCreateUser = "E";
			rp.RPR_SystemLastEditUser = "E";

			rp.RPR_S9_EmployeesInReview = f.PK;

			rp.RPR_Type = "PEE";
			rp.RPR_GC_Company = Guid.Empty;
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "The INSERT statement conflicted with the CHECK constraint \"Constraint_RPR_Type\"", true), "PEE code should be invalid");

			rp.RPR_Type = "BL";
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "The INSERT statement conflicted with the CHECK constraint \"Constraint_RPR_Type\"", true), "BL code should be invalid");
		}
	}
}
