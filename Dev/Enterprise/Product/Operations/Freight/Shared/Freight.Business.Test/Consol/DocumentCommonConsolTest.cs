using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DocumentCommonConsolTest : BaseFreightTest
	{
		public void TestIncludeConsignee()
		{
			DocumentCommonConsol.IncludeConsignee = true;
			AssertEquals(true, DocumentCommonConsol.IncludeConsignee);

			DocumentCommonConsol.IncludeConsignee = false;
			AssertEquals(false, DocumentCommonConsol.IncludeConsignee);
		}

		public void TestIncludeConsignor()
		{
			DocumentCommonConsol.IncludeConsignor = true;
			AssertEquals(true, DocumentCommonConsol.IncludeConsignor);

			DocumentCommonConsol.IncludeConsignor = false;
			AssertEquals(false, DocumentCommonConsol.IncludeConsignor);
		}

		public void TestIncludeCustomsBroker()
		{
			DocumentCommonConsol.IncludeCustomsBroker = true;
			AssertEquals(true, DocumentCommonConsol.IncludeCustomsBroker);

			DocumentCommonConsol.IncludeCustomsBroker = false;
			AssertEquals(false, DocumentCommonConsol.IncludeCustomsBroker);
		}

		public void TestIncludeAllShipments()
		{
			DocumentCommonConsol.IncludeAllShipments = true;
			AssertEquals(true, DocumentCommonConsol.IncludeAllShipments);

			DocumentCommonConsol.IncludeAllShipments = false;
			AssertEquals(false, DocumentCommonConsol.IncludeAllShipments);
		}

		public void TestIncludePacked()
		{
			DocumentCommonConsol.IncludePacked = true;
			AssertEquals(true, DocumentCommonConsol.IncludePacked);

			DocumentCommonConsol.IncludePacked = false;
			AssertEquals(false, DocumentCommonConsol.IncludePacked);
		}

		public void TestIncludeUnPacked()
		{
			DocumentCommonConsol.IncludeUnPacked = true;
			AssertEquals(true, DocumentCommonConsol.IncludeUnPacked);

			DocumentCommonConsol.IncludeUnPacked = false;
			AssertEquals(false, DocumentCommonConsol.IncludeUnPacked);
		}

		public void TestSetDefaultsFromDataContext()
		{
			DocumentCommonConsol.SetDefaultsFromDataContext(Constants.DataContext.LoadListDocument);
			AssertEquals("Defaults: IncludeConsignor should be ", ZBool.True, DocumentCommonConsol.IncludeConsignor);
			AssertEquals("Defaults: IncludeConsignee should be ", ZBool.True, DocumentCommonConsol.IncludeConsignee);
			AssertEquals("Defaults: IncludeAllShipments should be ", ZBool.True, DocumentCommonConsol.IncludeAllShipments);
		}

		#region Implementation

		#region Consol

		CommonConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<CommonConsol>();
					fConsol.JK_TransportMode = Constants.TransportModes.Air;
					fConsol.JK_MasterBillNum = "08112345678";

					Transport transport = Consol.Transports[0];
					transport.JW_RL_NKLoadPort = "AUSYD";

					fConsol.Shipments.AddNew();
					fConsol.Shipments[0].JS_OuterPacks = 7;
				}
				return fConsol;
			}
		}
		CommonConsol fConsol;

		#endregion

		#region DocumentCommonConsol

		DocumentCommonConsol DocumentCommonConsol
		{
			get
			{
				if (fDocumentCommonConsol == null)
				{
					fDocumentCommonConsol = new DocumentCommonConsol(Consol, Constants.DataContext.LoadListConsol);
				}

				return fDocumentCommonConsol;
			}
		}
		DocumentCommonConsol fDocumentCommonConsol;

		#endregion

		protected override void SetUp()
		{
			CurrentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CurrentBranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CurrentCompanyCountryCode;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = CurrentBranchPort;
			base.TearDown();
		}

		ZString CurrentCompanyCountryCode;
		ZString CurrentBranchPort;

		#endregion
	}
}
