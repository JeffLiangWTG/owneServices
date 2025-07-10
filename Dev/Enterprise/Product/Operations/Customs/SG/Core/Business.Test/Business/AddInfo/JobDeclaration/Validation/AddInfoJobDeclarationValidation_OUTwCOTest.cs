using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobDeclarationValidation_OUTwCOTest : AddInfoJobDeclarationValidation_OUTTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.OUT;
			}
		}

		public void TestOutwardTransport()
		{
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			Validation.ValidateSG_OutwardTransportMode();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardTransportModeInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateSG_OutwardTransportMode();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardTransportModeInfo.HasMessageErrors());
		}
	}
}
