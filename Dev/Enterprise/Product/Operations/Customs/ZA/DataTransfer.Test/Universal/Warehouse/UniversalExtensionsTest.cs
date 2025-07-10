using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class UniversalExtensionsTest : TestCaseWithFactory
	{
		public void TestGetInvoiceLineAddInfosApplicableForInwardWarehousing()
		{
			var list = Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing();
			AssertEquals("List should be cached to the factory", list, Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing());
			var checkedList = new List<string>(list);
			var missingFields = new List<string>();
			foreach (var expectedField in ExpectedList.Select(x => x.Substring(3)))
			{
				if (checkedList.Contains(expectedField))
				{
					checkedList.Remove(expectedField);
				}
				else
				{
					missingFields.Add(expectedField);
				}
			}

			var failedMessage = new ZStringBuilder();
			if (missingFields.Count > 0)
			{
				failedMessage.Append("The following expected fields are missing from the list; if they are no longer valid then please remove them from the expected list:");
				missingFields.ForEach(x => failedMessage.Append(x));
				failedMessage.AppendLine();
			}

			if (checkedList.Count > 0)
			{
				failedMessage.Append("The following new fields are not in the expected list; if they are valid then please add them to the expected list and also if they are numeric fields that need to be apportioned correctly then add them to Enterprise.Customs.US.Busines.InventorySelectionHeader.FieldsNeedToApplyRatio:");
				checkedList.ForEach(x => failedMessage.Append(x));
			}

			Assert(failedMessage.ToStringWithNewLineBetweenAppends(), failedMessage.IsEmpty);
		}

		public static string[] ExpectedList
		{
			get
			{
				return new string[]
				{
					JobComInvoiceLine.Schema.JI_EngineNumber,
					JobComInvoiceLine.Schema.JI_ROOCert,
					JobComInvoiceLine.Schema.JI_VIN,
					JobComInvoiceLine.Schema.JI_NewUsed
				};
			}
		}
	}
}
