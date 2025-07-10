using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class TestCaseWithDummyOrgMatchApproval : TestCaseWithFactory
	{
		protected OrgHeader CreateSimilarOrgMatch()
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = "Organisation CompanyName ltd";
			result.MainAddress.OA_Address1 = "Street";
			result.MainAddress.OA_Address2 = "Street2";
			result.MainAddress.OA_City = "City";
			result.MainAddress.OA_State = "State";
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(OrgMatchApproval.Schema.TableName);

			DummyParent = Factory.New<DummyBusinessObjectAutoLogged>();
			Loader = new OrgMatchApproval.Loader(Factory);
			DummyMatchApproval = (DummyOrgMatchApproval)Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType);
		}

		protected DummyBusinessObjectAutoLogged DummyParent;
		protected DummyOrgMatchApproval DummyMatchApproval;
		protected OrgMatchApproval.Loader Loader;

		#region Test Classes

		protected class DummyBusinessObjectAutoLogged : DummyEnterpriseBusinessObject
		{
			public DummyBusinessObjectAutoLogged(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override EnterpriseBusinessObject.AutologState AutoLoggingState => EnterpriseBusinessObject.AutologState.AutoLogged;
		}

		#endregion
	}
}
