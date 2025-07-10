using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(FoodDataCollection))]
	sealed class FoodDataCollectionTest : CusCodeDataCollectionTest<FoodData>
	{
		protected override CusCodeDataCollection<FoodData> GetCusCodeDataCollection()
		{
			return new FoodDataCollection(JobComInvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<FoodData>();
			result.CY_ParentID = JobComInvoiceLine.PK;
			result.CY_ParentTableCode = JobComInvoiceLine.TablePrefix;
			return result;
		}

		JobComInvoiceLine JobComInvoiceLine
		{
			get
			{
				if (fJobComInvoiceLine == null)
				{
					var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
					var jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CD" });
					jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
					fJobComInvoiceLine = jobDeclartion.Invoices.AddNew().JobComInvoiceLines.AddNew();
					ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(fJobComInvoiceLine, "CD", true);
				}

				return fJobComInvoiceLine;
			}
		}

		JobComInvoiceLine fJobComInvoiceLine;
	}
}
