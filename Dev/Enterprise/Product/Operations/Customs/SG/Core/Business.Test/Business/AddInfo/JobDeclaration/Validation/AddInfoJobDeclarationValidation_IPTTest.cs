using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobDeclarationValidation_IPTTest : AddInfoCUSDECValidationTest
	{
		public void TestIsSeaStore()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			AddInfoJobDeclaration.SG_IsSeaStore = false;
			AssertEquals(false, Declaration.SG_IsSeaStoreInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			AssertEquals(true, Declaration.SG_IsSeaStoreInfo.HasMessageErrors());
		}

		public void TestRemovalStartDate()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			AssertEquals(false, Declaration.SG_RemovalStartDateInfo.HasWarning("Start Date.\r\nFor blanket imports, specify the Start Date"));
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.SG_RemovalStartDate = ZDateTime.Empty;
			AssertEquals("BKT declaration requires Start Date", true, Declaration.SG_RemovalStartDateInfo.HasWarning("Start Date.\r\nFor blanket imports, specify the Start Date"));
			Declaration.SG_RemovalStartDate = ZDateTime.Today;
			AssertEquals(false, Declaration.SG_RemovalStartDateInfo.HasWarning("Start Date.\r\nFor blanket imports, specify the Start Date"));
		}

		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.IPT;
			}
		}
	}
}
