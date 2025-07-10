using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	class AddInfoLoopTest : TestCaseWithFactory
	{
		public void TestLoopingInitialising()
		{
			var invoiceLine = Factory.New<InvoiceLineWithLoopingAddInfo>();
			invoiceLine.JI_AddInfo = "String=Test	 *Decimal=123.12345";
			invoiceLine.JI_NAddInfo = "NString=测试";
			Factory.Save();

			CombineAssertions(() =>
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedInvoiceLine = newFactory.Load<InvoiceLineWithLoopingAddInfo>(invoiceLine.PK);
				AssertEquals(123.12345m, reloadedInvoiceLine.AddInfo.UZ_Decimal);
				AssertEquals("Test", reloadedInvoiceLine.AddInfo.UZ_String);
				AssertEquals("测试", reloadedInvoiceLine.AddInfo.UZ_NString);
				reloadedInvoiceLine.AddInfo.UZ_String = "SecondTest	 ";
				reloadedInvoiceLine.AddInfo.UZ_NString = "再来一次	 ";
				AssertEquals("123.12345SecondTest", reloadedInvoiceLine.AddInfo.UZ_String);
				AssertEquals("123.12345再来一次", reloadedInvoiceLine.AddInfo.UZ_NString);
				newFactory.Save();
				AssertEquals("Decimal=123.12345*String=123.12345SecondTest*NString=123.12345再来一次", reloadedInvoiceLine.JI_AddInfo);
				AssertEquals("Field 'Decimal' is not allowed to set with non western European characters.", "NString=123.12345再来一次", reloadedInvoiceLine.JI_NAddInfo);
			});
		}

		class TestAddInfoOfLoopInitialising : TestAddInfo
		{
			public TestAddInfoOfLoopInitialising(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
			{
			}

			public override ZString UZ_String
			{
				get { return base.UZ_String; }
				set
				{
					base.UZ_String = (Parent as InvoiceLineWithLoopingAddInfo).AddInfo.UZ_Decimal.ToString() + value;
				}
			}

			public override ZString UZ_NString
			{
				get { return base.UZ_NString; }
				set
				{
					base.UZ_NString = (Parent as InvoiceLineWithLoopingAddInfo).AddInfo.UZ_Decimal.ToString() + value;
				}
			}
		}

		class InvoiceLineWithLoopingAddInfo : BaseJobComInvoiceLine, IAddInfoManager, INAddInfoSupporter
		{
			public InvoiceLineWithLoopingAddInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				var testheader = factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
				JI_JZ = testheader.PK;
			}

			public TestAddInfoOfLoopInitialising AddInfo
			{
				get
				{
					if (addInfo == null)
					{
						addInfo = new TestAddInfoOfLoopInitialising(JI_AddInfoInfo);
						RegisterEditableChildObject(addInfo);
					}
					return addInfo;
				}
			}
			TestAddInfoOfLoopInitialising addInfo;

			#region IAddInfoManager Members

			IAddInfo IAddInfoManager.AddInfo
			{
				get { return AddInfo; }
			}

			#endregion

			ZPropertyInfoString INAddInfoSupporter.NAddInfoProperty => JI_NAddInfoInfo as ZPropertyInfoString;
		}
	}
}
