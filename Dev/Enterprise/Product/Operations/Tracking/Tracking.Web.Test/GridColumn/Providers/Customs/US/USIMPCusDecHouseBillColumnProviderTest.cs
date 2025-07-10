using System.Collections.Generic;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;
using Bill = Enterprise.Customs.US.Business.Bill;
using JobDeclaration = Enterprise.Customs.US.Business.JobDeclaration;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(USIMPCusDecHouseBillColumnProvider))]
	sealed class USIMPCusDecHouseBillColumnProviderTest : BaseCusDecHouseBillColumnProviderTest
	{
		public override void TestColumnKeys()
		{
			base.TestColumnKeys();

			SetupUSIMPDeclaration_SplitDetailsRelevant();
			base.TestColumnKeys();
		}

		public override void TestDefaultColumns()
		{
			base.TestDefaultColumns();

			SetupUSIMPDeclaration_SplitDetailsRelevant();
			base.TestDefaultColumns();
		}

		public override void TestRequiredColumns()
		{
			base.TestRequiredColumns();

			SetupUSIMPDeclaration_SplitDetailsRelevant();
			base.TestRequiredColumns();
		}

		public override void TestUniqueColumns()
		{
			base.TestUniqueColumns();

			SetupUSIMPDeclaration_SplitDetailsRelevant();
			base.TestUniqueColumns();
		}

		protected override void SetupCountrySpecificColumns()
		{
			base.SetupCountrySpecificColumns();
			AddColumn(new ZCodeFindBoxColumn("Bill Issuer SCAC", nameof(Bill.US_UI_NKBillIssuerSCAC), "Lookups+USCarrierList") { ColumnKey = WebTracker.Grids.CusDecHouseBills.BillIssuerSCAC });
			AddColumn(new ZTextEditColumn("ISF Bill Status", nameof(Bill.ISFBillStatus)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ISFBillStatus });
			AddColumn(new ZTextEditColumn("ISF Bill Status Description", nameof(Bill.ISFBillStatusDescription)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ISFBillStatusDescription });
			AddDefaultsColumn(new ZTextEditColumn("IT Number", nameof(Bill.ITNumber)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ITNumber });

			if (TestDeclaration.AreSplitDetailsRelevant)
			{
				AddColumn(new ZCheckBoxColumn("Is Split", nameof(Bill.US_SESplitShip)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ITNumber });
			}
		}

		protected override bool ExpectHBLIssueDateColumn => false;

		protected override List<object> GetUnsortableColumnKeys()
		{
			var result = base.GetUnsortableColumnKeys();

			result.Add(WebTracker.Grids.CusDecHouseBills.BillIssuerSCAC);

			return result;
		}

		JobDeclaration TestDeclaration { get; set; }

		protected override GridColumnProvider GetNewTestProvider()
		{
			if (TestDeclaration == null)
			{
				SetupUSIMPDeclaration();
			}

			return new USIMPCusDecHouseBillColumnProvider(TestDeclaration);
		}

		void SetupUSIMPDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				TestDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				TestDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				TestDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;

				Assert("Precondition", TestDeclaration.IsImport);
				Assert("Precondition", !TestDeclaration.AreSplitDetailsRelevant);
			}
		}

		void SetupUSIMPDeclaration_SplitDetailsRelevant()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				TestDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				TestDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				TestDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
				TestDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				TestDeclaration.US_EnableENS = true;
				TestDeclaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

				Assert("Precondition", TestDeclaration.IsImport);
				Assert("Precondition", TestDeclaration.AreSplitDetailsRelevant);
			}
		}
	}
}
