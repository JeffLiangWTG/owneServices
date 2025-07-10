using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;
using Bill = Enterprise.Customs.AU.Declaration.Business.Bill;
using JobDeclaration = Enterprise.Customs.AU.Declaration.Business.JobDeclaration;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(AUCusDecHouseBillColumnProvider))]
	sealed class AUCusDecHouseBillColumnProviderTest : BaseCusDecHouseBillColumnProviderTest
	{
		public override void TestColumnKeys()
		{
			base.TestColumnKeys();

			SetupAUDeclaration_Post();
			base.TestColumnKeys();

			SetupAUDeclaration_PartShipConsignmentReferenceRelevant();
			base.TestColumnKeys();
		}

		public override void TestDefaultColumns()
		{
			base.TestDefaultColumns();

			SetupAUDeclaration_Post();
			base.TestDefaultColumns();

			SetupAUDeclaration_PartShipConsignmentReferenceRelevant();
			base.TestDefaultColumns();
		}

		public override void TestRequiredColumns()
		{
			base.TestRequiredColumns();

			SetupAUDeclaration_Post();
			base.TestRequiredColumns();

			SetupAUDeclaration_PartShipConsignmentReferenceRelevant();
			base.TestRequiredColumns();
		}

		public override void TestUniqueColumns()
		{
			base.TestUniqueColumns();

			SetupAUDeclaration_Post();
			base.TestUniqueColumns();

			SetupAUDeclaration_PartShipConsignmentReferenceRelevant();
			base.TestUniqueColumns();
		}

		protected override void SetupCountrySpecificColumns()
		{
			if (TestDeclaration.IsPost)
			{
				AddDefaultsColumn(new ZTextEditColumn("Parcel Post Number", nameof(Bill.CU_BillNum)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.BillNum });
			}
			else
			{
				base.SetupCountrySpecificColumns();
			}

			if (TestDeclaration.IsPartShipConsignmentReferenceRelevant)
			{
				AddDefaultsColumn(new ZTextEditColumn("Consign. Ref. No.", nameof(Bill.CU_fPartShipConsignmentReference)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ConsignRefNo });
			}
		}

		JobDeclaration TestDeclaration { get; set; }

		protected override GridColumnProvider GetNewTestProvider()
		{
			if (TestDeclaration == null)
			{
				SetupAUDeclaration();
			}

			return new AUCusDecHouseBillColumnProvider(TestDeclaration);
		}

		void SetupAUDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				TestDeclaration = Factory.NewWithValidTestData<JobDeclaration>();

				Assert("Precondition", !TestDeclaration.IsPost);
				Assert("Precondition", !TestDeclaration.IsPartShipConsignmentReferenceRelevant);
				SetupNewProvider();
			}
		}

		void SetupAUDeclaration_Post()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				TestDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				TestDeclaration.JE_TransportMode = Constants.TransportModes.Mail;

				Assert("Precondition", TestDeclaration.IsPost);
				Assert("Precondition", !TestDeclaration.IsPartShipConsignmentReferenceRelevant);
				SetupNewProvider();
			}
		}

		void SetupAUDeclaration_PartShipConsignmentReferenceRelevant()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				TestDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				TestDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				TestDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;

				Assert("Precondition", !TestDeclaration.IsPost);
				Assert("Precondition", TestDeclaration.IsPartShipConsignmentReferenceRelevant);
				SetupNewProvider();
			}
		}
	}
}
