using System.IO;
using System.Text;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class XMLElementExtensionsTest : TestCaseWithFactory
	{
		public void TestGetSingleNode()
		{
			const string xml = "<abc><def>1</def><child><grandchild>2</grandchild><grandchild>3</grandchild></child></abc>";
			using (var reader = new StringReader(xml))
			{
				var node = reader.GetSingleNode("/abc");
				AssertEquals("<def>1</def><child><grandchild>2</grandchild><grandchild>3</grandchild></child>", node.InnerXml);
			}
			using (var reader = new StringReader(xml))
			{
				var node = reader.GetSingleNode("/abc/def");
				AssertEquals("1", node.InnerXml);
			}
			using (var reader = new StringReader(xml))
			{
				var node = reader.GetSingleNode("/abc/child/grandchild");
				AssertEquals("2", node.InnerXml);
			}
			using (var reader = new StringReader(xml))
			{
				AssertNull(reader.GetSingleNode("/abc/child2"));
			}
			const string xmlWithNamespace = @"<env:abc xmlns:env=""http://www.w3.org/2003/05/soap-envelope"">
	<a:def xmlns:a=""http://www.abc.org"">4</a:def>
	<env:def>1</env:def>
	<env:child>
		<env:grandchild>2</env:grandchild>
		<env:grandchild>3</env:grandchild>
	</env:child>
</env:abc>";
			CombineAssertions(() =>
			{
				using (var reader = new StringReader(xmlWithNamespace))
				{
					var node = reader.GetSingleNode("/env:abc/a:def", ("env", "http://www.w3.org/2003/05/soap-envelope"), ("a", "http://www.abc.org"));
					AssertEquals("4", node.InnerXml);
				}
				using (var reader = new StringReader(xmlWithNamespace))
				{
					var node = reader.GetSingleNode("/env:abc/env:def", ("env", "http://www.w3.org/2003/05/soap-envelope"), ("a", "http://www.abc.org"));
					AssertEquals("1", node.InnerXml);
				}
			});
		}

		public void TestGetNodeValue()
		{
			foreach (var includeChildrenOfChildrenInSearch in new[] { true, false })
			{
				var element = GetXElement(includeChildrenOfChildrenInSearch ? SampleXML2 : SampleXML);
				var nodeValue = element.GetNodeValue(ZStringField, includeChildrenOfChildrenInSearch);
				AssertEquals("DL", nodeValue);
			}
		}

		public void TestGetAttributeValue()
		{
			var element = GetXElement(SampleXML3);
			var attributeValue = element.GetAttributeValue(ZDecimalField);
			AssertEquals("100", attributeValue);
		}

		public void TestTryToSetValueFromXElelment()
		{
			var element = GetXElement(SampleXML);
			element.TryToSetValueFromXElelment<ZString>(ZStringField, v => AssertEquals("DL", v));
			element.TryToSetValueFromXElelment<ZDecimal>(ZDecimalField, v => AssertEquals(24M, v));
			element.TryToSetValueFromXElelment<ZDateTime>(ZDateField, v => AssertEquals(new ZDateTime(2014, 11, 9, 0, 0, 0), v));
			element.TryToSetValueFromXElelment<ZGuid>(ZGuidField, v => AssertEquals(new ZGuid("40000000-0000-0000-0000-000000000000"), v));
			element.TryToSetValueFromXElelment<ZBool>(ZBoolField, v => AssertEquals(true, v));
			element.TryToSetValueFromXElelment<ZByte>(ZByteField, v => AssertEquals((ZByte)14, v));
		}

		public void TestTryToSetValueFromXAttribute()
		{
			var element = GetXElement(SampleXML3);
			element.TryToSetValueFromXAttribute<ZString>(ZStringField, v => AssertEquals("DL", v));
			element.TryToSetValueFromXAttribute<ZDecimal>(ZDecimalField, v => AssertEquals(100M, v));
			element.TryToSetValueFromXAttribute<ZDateTime>(ZDateField, v => AssertEquals(new ZDateTime(2014, 11, 11, 0, 0, 0), v));
			element.TryToSetValueFromXAttribute<ZGuid>(ZGuidField, v => AssertEquals(new ZGuid("40000000-0000-0000-0000-000000000000"), v));
			element.TryToSetValueFromXAttribute<ZBool>(ZBoolField, v => AssertEquals(true, v));
			element.TryToSetValueFromXAttribute<ZByte>(ZByteField, v => AssertEquals((ZByte)14, v));
		}

		XElement GetXElement(string xml)
		{
			XElement element = null;
			using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
			{
				element = XElement.Load(stream);
			}
			return element;
		}

		const string SampleXML = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><TransactionPendingAllocationApprovalDetails><MaxAmountToApprove>100</MaxAmountToApprove><TransactionDate>2014-11-09T00:00:00</TransactionDate><PostDate>2014-11-11T00:00:00</PostDate><TransactionNumber>INV12</TransactionNumber><CreditorPK>00000000-0000-0000-0000-000000000000</CreditorPK><DueDate>2014-11-16T00:00:00</DueDate><CurrencyCode>USD</CurrencyCode><ExRate>2</ExRate><OSExTaxAmount>100</OSExTaxAmount><OSTaxAmount>12</OSTaxAmount><LocalExTaxAmount>200</LocalExTaxAmount><LocalTaxAmount>24</LocalTaxAmount><Description>Some text</Description><BranchPK>10000000-0000-0000-0000-000000000000</BranchPK><DepartmentPK>20000000-0000-0000-0000-000000000000</DepartmentPK><AddressPK>30000000-0000-0000-0000-000000000000</AddressPK><ContactPK>40000000-0000-0000-0000-000000000000</ContactPK><NumberOfSupportingDocuments>14</NumberOfSupportingDocuments><SourceXML IsCrossLedgerImport=\"Y\"><XML>&lt;xml&gt;some xml&lt;/xml&gt;</XML></SourceXML><PlaceOfSupply>DL</PlaceOfSupply><PlaceOfSupplyType>STA</PlaceOfSupplyType><SampleBoolField>Y</SampleBoolField></TransactionPendingAllocationApprovalDetails>";
		const string SampleXML2 = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><TransactionPendingAllocationApprovalDetails><MaxAmountToApprove>100</MaxAmountToApprove><Details><TransactionDate>2014-11-09T00:00:00</TransactionDate><PostDate>2014-11-11T00:00:00</PostDate><TransactionNumber>INV12</TransactionNumber><CreditorPK>00000000-0000-0000-0000-000000000000</CreditorPK><DueDate>2014-11-16T00:00:00</DueDate><CurrencyCode>USD</CurrencyCode><ExRate>2</ExRate><OSExTaxAmount>100</OSExTaxAmount><OSTaxAmount>12</OSTaxAmount><LocalExTaxAmount>200</LocalExTaxAmount><LocalTaxAmount>24</LocalTaxAmount><Description>Some text</Description><BranchPK>10000000-0000-0000-0000-000000000000</BranchPK><DepartmentPK>20000000-0000-0000-0000-000000000000</DepartmentPK><AddressPK>30000000-0000-0000-0000-000000000000</AddressPK><ContactPK>40000000-0000-0000-0000-000000000000</ContactPK><NumberOfSupportingDocuments>14</NumberOfSupportingDocuments><SourceXML IsCrossLedgerImport=\"Y\"><XML>&lt;xml&gt;some xml&lt;/xml&gt;</XML></SourceXML><PlaceOfSupply>DL</PlaceOfSupply><PlaceOfSupplyType>STA</PlaceOfSupplyType><SampleBoolField>Y</SampleBoolField></Details></TransactionPendingAllocationApprovalDetails>";
		const string SampleXML3 = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><TransactionData PlaceOfSupply=\"DL\" LocalTaxAmount=\"100\" TransactionDate=\"2014-11-11T00:00:00\" ContactPK=\"40000000-0000-0000-0000-000000000000\" SampleBoolField=\"Y\" NumberOfSupportingDocuments=\"14\">No Children</TransactionData>";
		const string ZStringField = "PlaceOfSupply";
		const string ZDecimalField = "LocalTaxAmount";
		const string ZDateField = "TransactionDate";
		const string ZGuidField = "ContactPK";
		const string ZBoolField = "SampleBoolField";
		const string ZByteField = "NumberOfSupportingDocuments";
	}
}
