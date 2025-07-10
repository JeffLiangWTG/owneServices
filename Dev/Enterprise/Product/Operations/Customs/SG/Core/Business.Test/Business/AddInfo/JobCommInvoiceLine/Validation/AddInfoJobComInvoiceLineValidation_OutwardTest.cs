using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class AddInfoJobComInvoiceLineValidation_OutwardTest : AddInfoJobComInvoiceLineValidation_CUSDECTest
	{
		public void TestCategoryCode()
		{
			AssertStrategicValue(InvoiceLine.SG_CategoryCodeInfo);
		}

		public void TestEndUseCode1()
		{
			AssertStrategicValue(InvoiceLine.SG_EndUseCode1Info);
		}

		public void TestEndUseCode2()
		{
			AssertStrategicValue(InvoiceLine.SG_EndUseCode2Info);
		}

		public void TestEndUseCode3()
		{
			AssertStrategicValue(InvoiceLine.SG_EndUseCode3Info);
		}

		public void TestEndUseDescription()
		{
			AssertStrategicValue(InvoiceLine.SG_EndUseDescriptionInfo);
		}

		void AssertStrategicValue(ZPropertyInfo info)
		{
			InvoiceLine.SG_IsStrategic = true;
			info.Value = ZString.Empty;
			AssertHasMessageErrorContaining(info, "You have not entered a value.");
			info.Value = new ZString("TST");
			AssertNoMessageErrorContaining(info, "You have not entered a value.");
			InvoiceLine.SG_IsStrategic = false;
			info.Value = new ZString("TST");
			AssertNoMessageErrorContaining(info, "You have not entered a value.");
			AssertEquals(true, info.HasWarnings());
			info.Value = ZString.Empty;
			AssertEquals(false, info.HasWarnings());
			AssertNoMessageErrorContaining(info, "You have not entered a value.");
		}
	}
}
