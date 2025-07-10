using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DocumentImportCargoLabelTest : BaseFreightTest
	{
		public void TestIncludeConsignee()
		{
			DocumentImportCargoLabel.IncludeConsignee = true;
			AssertEquals(true, DocumentImportCargoLabel.IncludeConsignee);

			DocumentImportCargoLabel.IncludeConsignee = false;
			AssertEquals(false, DocumentImportCargoLabel.IncludeConsignee);
		}

		public void TestIncludeConsignor()
		{
			DocumentImportCargoLabel.IncludeConsignor = true;
			AssertEquals(true, DocumentImportCargoLabel.IncludeConsignor);

			DocumentImportCargoLabel.IncludeConsignor = false;
			AssertEquals(false, DocumentImportCargoLabel.IncludeConsignor);
		}

		public void TestIncludeHouseBill()
		{
			DocumentImportCargoLabel.IncludeHouseBill = true;
			AssertEquals(true, DocumentImportCargoLabel.IncludeHouseBill);

			DocumentImportCargoLabel.IncludeHouseBill = false;
			AssertEquals(false, DocumentImportCargoLabel.IncludeHouseBill);
		}

		public void TestIncludeCFSName()
		{
			DocumentImportCargoLabel.IncludeCFSName = true;
			AssertEquals(true, DocumentImportCargoLabel.IncludeCFSName);

			DocumentImportCargoLabel.IncludeCFSName = false;
			AssertEquals(false, DocumentImportCargoLabel.IncludeCFSName);
		}

		public void TestNoOfLabelsToPrint()
		{
			AssertEquals(7, DocumentImportCargoLabel.NoOfLabelsToPrint); //initial			 
			DocumentImportCargoLabel.NoOfLabelsToPrint = 3;
			AssertEquals(3, DocumentImportCargoLabel.NoOfLabelsToPrint);
			DocumentImportCargoLabel.NoOfLabelsToPrint = 0;
			AssertEquals(true, DocumentImportCargoLabel.NoOfLabelsToPrintInfo.HasErrors());
			DocumentImportCargoLabel.NoOfLabelsToPrint = 8;
			AssertEquals(true, DocumentImportCargoLabel.NoOfLabelsToPrintInfo.HasWarnings());
			DocumentImportCargoLabel.NoOfLabelsToPrint = 6;
			AssertEquals(true, DocumentImportCargoLabel.NoOfLabelsToPrintInfo.HasWarnings());
			DocumentImportCargoLabel.NoOfLabelsToPrint = 7;
			AssertEquals(false, DocumentImportCargoLabel.NoOfLabelsToPrintInfo.HasWarnings());
		}

		public void TestNoOfLabelsToPrintEnabled()
		{
			DocumentImportCargoLabel.IncludeConsignee = false;
			DocumentImportCargoLabel.IncludeConsignor = false;
			DocumentImportCargoLabel.IncludeHouseBill = false;
			DocumentImportCargoLabel.IncludeCFSName = false;
			AssertEquals(false, DocumentImportCargoLabel.NoOfLabelsToPrintInfo.ReadOnly);

			DocumentImportCargoLabel.IncludeCFSName = true;
			AssertEquals(false, DocumentImportCargoLabel.NoOfLabelsToPrintInfo.ReadOnly);

			DocumentImportCargoLabel.IncludeConsignee = true;
			AssertEquals(true, DocumentImportCargoLabel.NoOfLabelsToPrintInfo.ReadOnly);

			DocumentImportCargoLabel.IncludeConsignee = false;
			DocumentImportCargoLabel.IncludeConsignor = true;
			AssertEquals(true, DocumentImportCargoLabel.NoOfLabelsToPrintInfo.ReadOnly);

			DocumentImportCargoLabel.IncludeConsignor = false;
			DocumentImportCargoLabel.IncludeHouseBill = true;
			AssertEquals(true, DocumentImportCargoLabel.NoOfLabelsToPrintInfo.ReadOnly);

			DocumentImportCargoLabel.IncludeHouseBill = false;
			AssertEquals(false, DocumentImportCargoLabel.NoOfLabelsToPrintInfo.ReadOnly);
		}

		#region Implementation

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

		#region DocumentImportCargoLabel

		DocumentImportCargoLabel DocumentImportCargoLabel
		{
			get
			{
				if (fDocumentImportCargoLabel == null)
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_OuterPacks = 7;

					fDocumentImportCargoLabel = new DocumentImportCargoLabel(shipment);
				}
				return fDocumentImportCargoLabel;
			}
		}
		DocumentImportCargoLabel fDocumentImportCargoLabel;

		#endregion

		ZString CurrentCompanyCountryCode;
		ZString CurrentBranchPort;

		#endregion
	}
}
