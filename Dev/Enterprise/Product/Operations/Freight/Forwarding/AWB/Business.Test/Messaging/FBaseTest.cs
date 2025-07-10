using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Integration.AWB;
using Enterprise.Messaging.Business.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	public abstract class FBaseTest : CargoIMPTest
	{
		protected ExportAWBHeader ConsolAWBHeader;

		protected override void SetUp()
		{
			ConsolAWBHeader = CreateTemplateAWBHeader();
		}

		protected abstract ExportAWBHeader CreateTemplateAWBHeader();

		protected abstract ExportAWBHeader GetNewFWBHeader();

		#region AWBQuantityDetail

		public void TestAWBQuantityDetail_NumberOfPieces()
		{
			var header = GetNewFWBHeader();
			header.EH_ShippingLoadAndCount = 10;

			header.AWBRateLines.AddNew().ER_NoOfPiecesOrRCP = "1";
			header.AWBRateLines.AddNew().ER_NoOfPiecesOrRCP = "2";
			header.AWBRateLines.AddNew().ER_NoOfPiecesOrRCP = "3";
			header.AWBRateLines[0].ER_WeightInLBsOrKGs = Constants.AWB.RateLineUQ.Kilos;

			AssertEquals("Precondition", 10, header.EH_ShippingLoadAndCount);
			AssertEquals("Precondition", 6, header.EH_TotalNoOfPieces);

			var fwbDetails = new FWBMessageDetails(header);

			ElementList elementList = new FBaseForTesting(fwbDetails).AWBQuantityDetail;
			AssertEquals("AWBQuantityDetail should use EH_TotalNoOfPieces", "/T6K0", elementList.ToString());
		}

		#endregion

		#region Class for testing

		class FBaseForTesting : FBase
		{
			public FBaseForTesting(IFWBMessageDetailsProvider fwbDetailsProvider)
				: base(fwbDetailsProvider)
			{
			}

			public new ElementList AWBQuantityDetail
			{
				get { return base.AWBQuantityDetail; }
			}

			public override ZString StandardMessageIdentifier
			{
				get { throw new System.NotImplementedException(); }
			}

			protected override ZString MessageTypeVersionNumber
			{
				get { throw new System.NotImplementedException(); }
			}

			protected override void ConstructMessage()
			{
				throw new System.NotImplementedException();
			}

			protected override CharType OCIMUFormatType
			{
				get { throw new System.NotImplementedException(); }
			}

			protected override CharType OCIMDFormatType
			{
				get { throw new System.NotImplementedException(); }
			}
		}

		#endregion
	}
}
