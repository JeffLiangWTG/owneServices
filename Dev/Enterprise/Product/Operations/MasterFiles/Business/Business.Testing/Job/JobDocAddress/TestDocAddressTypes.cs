using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestDocAddressTypes : TestCaseWithFactory
	{
		public void TestGetDescription()
		{
			AssertEquals("Consignee Documentary Address", DocAddressTypes.GetDescription(Factory, DocAddressType.ConsigneeDocumentaryAddress));
		}

		public void TestGetCode()
		{
			AssertEquals("Precondition", DocAddressTypes.Codes.ConsigneeDocumentaryAddress, DocAddressTypes.GetCode(Factory, DocAddressType.ConsigneeDocumentaryAddress));
			AssertEquals("Correct value added to cache", DocAddressTypes.Codes.ConsigneeDocumentaryAddress, DocAddressTypes.GetCode(Factory, DocAddressType.ConsigneeDocumentaryAddress));
		}

		public void TestCodes()
		{
			var codesList = new List<ZString>();
			foreach (DocAddressType docAddress in Enum.GetValues(typeof(DocAddressType)))
			{
				codesList.Add(DocAddressTypes.GetCode(Factory, docAddress));
			}

			AssertContainsExactElementsInAnyOrder(codesList, DocAddressTypes.GetCodes(Factory));
		}

		public void TestGetCodeAndDescription()
		{
			AssertEquals("CED", DocAddressTypes.Codes.ConsigneeDocumentaryAddress);
			AssertEquals("Consignee Documentary Address", DocAddressTypes.Descriptions.ConsigneeDocumentaryAddress);
		}

		public void TestGetPair()
		{
			AssertEquals("CED", DocAddressTypes.Codes.ConsigneeDocumentaryAddress);
			AssertEquals("Consignee Documentary Address", DocAddressTypes.Descriptions.ConsigneeDocumentaryAddress);
			CodeDescriptionPair pair = DocAddressTypes.GetPair(Factory, DocAddressType.ConsigneeDocumentaryAddress);
			AssertEquals("CED", pair.Code);
			AssertEquals("Consignee Documentary Address", pair.Description);
		}

		public void TestGetTypeFromCode()
		{
			foreach (DocAddressType docAddress in Enum.GetValues(typeof(DocAddressType)))
			{
				string code = DocAddressTypes.GetCode(Factory, docAddress);
				AssertEquals(docAddress, DocAddressTypes.GetDocAddressTypeFromCode(Factory, code));
			}
		}

		public void TestNonDuplicatedCode()
		{
			ArrayList codes = new ArrayList();
			foreach (DocAddressType docAddress in Enum.GetValues(typeof(DocAddressType)))
			{
				ZString code = DocAddressTypes.GetCode(Factory, docAddress);
				AssertCollectionNotContains("The DocAddress code " + code + " is duplicated.", code, codes);
				codes.Add(code);

				AssertEquals("The DocAddress code for enum " + docAddress + " is empty.", false, code.IsEmpty);
				if (docAddress != DocAddressType.None)
				{
					ZString description = DocAddressTypes.GetPair(Factory, docAddress).Description;
					AssertEquals("The DocAddress description for enum " + docAddress + " is empty.", false, description.IsEmpty);
				}
			}
		}
	}
}
