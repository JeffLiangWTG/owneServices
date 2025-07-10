using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class CusEntryLineFeeProviderTest : TestCaseWithFactory
	{
		public void TestTaxesMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				using (TRCustomsDataRegistry.Instance.TRTestingSystem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					using (TRCustomsDataRegistry.Instance.SendTRDeclarationWithComma.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
						var taxes = declaration.Taxes.ToArray();

						CombineAssertions("Tax Test 1 | Test = False | SendTRDeclarationWithComma = False", () =>
						{
							AssertEquals("LineNo", 1, taxes[0].LineNo);
							AssertEquals("Code", "10", taxes[0].Code);
							AssertEquals("Quantity", 620.76m, taxes[0].Quantity);
							AssertEquals("Ratio", "33.59", taxes[0].Ratio);
							AssertEquals("PaymentType", "P", taxes[0].PaymentType);
							AssertEquals("TaxAssessment", 1848.04m, taxes[0].TaxAssessment);
						});

						CombineAssertions("Tax Test 2 | Test = False | SendTRDeclarationWithComma = False", () =>
						{
							AssertEquals("LineNo", 1, taxes[1].LineNo);
							AssertEquals("Code", "40", taxes[1].Code);
							AssertEquals("Quantity", 100m, taxes[1].Quantity);
							AssertEquals("Ratio", "10", taxes[1].Ratio);
							AssertEquals("PaymentType", "C", taxes[1].PaymentType);
							AssertEquals("TaxAssessment", 1000m, taxes[1].TaxAssessment);
						});
					}

					using (TRCustomsDataRegistry.Instance.SendTRDeclarationWithComma.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
						var taxes = declaration.Taxes.ToArray();

						CombineAssertions("Tax Test 3 | Test = False | SendTRDeclarationWithComma = True", () =>
						{
							AssertEquals("Ratio", "33.59", taxes[0].Ratio);
						});
					}
				}

				using (TRCustomsDataRegistry.Instance.TRTestingSystem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					using (TRCustomsDataRegistry.Instance.SendTRDeclarationWithComma.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
						var taxes = declaration.Taxes.ToArray();

						CombineAssertions("Tax Test 4 | Test = True | SendTRDeclarationWithComma = False", () =>
						{
							AssertEquals("Ratio", "33.59", taxes[0].Ratio);
						});
					}

					using (TRCustomsDataRegistry.Instance.SendTRDeclarationWithComma.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
						var taxes = declaration.Taxes.ToArray();

						CombineAssertions("Tax Test 5 | Test = True | SendTRDeclarationWithComma = True", () =>
						{
							AssertEquals("Ratio", "33,59", taxes[0].Ratio);
						});
					}
				}
			}
		}
	}
}
