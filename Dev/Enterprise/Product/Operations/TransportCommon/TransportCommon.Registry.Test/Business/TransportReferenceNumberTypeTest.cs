using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(TransportReferenceNumberType))]
	public class TransportReferenceNumberTypeTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestCodeLengthDefaultValue()
		{
			TransportReferenceNumberType transportReferenceNumberType = new TransportReferenceNumberType();
			AssertEquals(3, transportReferenceNumberType.CodeMaxLength);
		}

		public void TestSerializeEmptyEntity()
		{
			TransportReferenceNumberType transportReferenceNumberType = new TransportReferenceNumberType();

			byte[] serializedValue = RegistryBusinessObjectTemplateTestCase.Serialize(transportReferenceNumberType);

			AssertEquals("<?xml version=\"1.0\" encoding=\"utf-16\"?><TransportReferenceNumberType><Code /><Description /><IsUnique>Y</IsUnique></TransportReferenceNumberType>",
				Encoding.Unicode.GetString(serializedValue).Trim());

			TransportReferenceNumberType deserializedTransportReferenceNumberType = RegistryBusinessObjectTemplateTestCase.Deserialize<TransportReferenceNumberType>(serializedValue);

			AssertEquals(ZString.Empty, deserializedTransportReferenceNumberType.Code);
			AssertEquals(string.Empty, deserializedTransportReferenceNumberType.Description);
			AssertEquals(true, deserializedTransportReferenceNumberType.IsUnique);
		}

		public void TestSerializeFullyPopulatedEntity()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "HRN";
			transportReferenceNumberType.Description = (NoResString)"HORNED";
			transportReferenceNumberType.SystemDefined = true;

			byte[] serializedValue = RegistryBusinessObjectTemplateTestCase.Serialize(transportReferenceNumberType);

			AssertEquals("<?xml version=\"1.0\" encoding=\"utf-16\"?><TransportReferenceNumberType><Code>HRN</Code><Description>HORNED</Description><IsUnique>Y</IsUnique></TransportReferenceNumberType>",
				Encoding.Unicode.GetString(serializedValue).Trim());

			var deserializedTransportReferenceNumberType = RegistryBusinessObjectTemplateTestCase.Deserialize<TransportReferenceNumberType>(serializedValue);

			AssertEquals("HRN", deserializedTransportReferenceNumberType.Code);
			AssertEquals("HORNED", deserializedTransportReferenceNumberType.Description);
			AssertEquals(true, deserializedTransportReferenceNumberType.IsUnique);
			AssertEquals("Should not serialize 'SystemDefined' as it may change in future.", false, deserializedTransportReferenceNumberType.SystemDefined);
		}

		public void TestCloneKeepsDescriptionAsResString()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "XXX";
			transportReferenceNumberType.Description = ResString.GetMultilingualString("test", "Test");
			var clone = (TransportReferenceNumberType)transportReferenceNumberType.Clone(null, null);
			AssertType(typeof(ResourceString), clone.Description);
		}

		public void TestClone_SystemDefined()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.SystemDefined = true;
			var clone = (TransportReferenceNumberType)transportReferenceNumberType.Clone(null, null);
			Assert(clone.SystemDefined);
		}

		public void TestSystemDefinedHSBReadOnlyMembers()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "HSB";
			transportReferenceNumberType.Description = (NoResString)"House Bill";
			transportReferenceNumberType.IsUnique = true;
			transportReferenceNumberType.SystemDefined = true;

			Assert("Code should be read only because it is system defined", transportReferenceNumberType.CodeInfo.ReadOnly);
			Assert("Description should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert("IsUnique should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.IsUniqueInfo.ReadOnly);
		}

		public void TestSystemDefinedMABReadOnlyMembers()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "MAB";
			transportReferenceNumberType.Description = (NoResString)"Master Bill";
			transportReferenceNumberType.IsUnique = false;
			transportReferenceNumberType.SystemDefined = true;

			Assert("Code should be read only because it is system defined", transportReferenceNumberType.CodeInfo.ReadOnly);
			Assert("Description should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert("IsUnique should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.IsUniqueInfo.ReadOnly);
		}

		public void TestSystemDefinedORDReadOnlyMembers()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "ORD";
			transportReferenceNumberType.Description = (NoResString)"Order Number";
			transportReferenceNumberType.IsUnique = false;
			transportReferenceNumberType.SystemDefined = true;

			Assert("Code should be read only because it is system defined", transportReferenceNumberType.CodeInfo.ReadOnly);
			Assert("Description should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert("IsUnique should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.IsUniqueInfo.ReadOnly);
		}

		public void TestSystemDefinedCLRReadOnlyMembers()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "CLR";
			transportReferenceNumberType.Description = (NoResString)"Client Reference Number";
			transportReferenceNumberType.IsUnique = true;
			transportReferenceNumberType.SystemDefined = true;

			Assert("Code should be read only because it is system defined", transportReferenceNumberType.CodeInfo.ReadOnly);
			Assert("Description should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert("IsUnique should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.IsUniqueInfo.ReadOnly);
		}

		public void TestSystemDefinedBPRReadOnlyMembers()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "BPR";
			transportReferenceNumberType.Description = (NoResString)"Booking Party Reference";
			transportReferenceNumberType.IsUnique = true;
			transportReferenceNumberType.SystemDefined = true;

			Assert("Code should be read only because it is system defined", transportReferenceNumberType.CodeInfo.ReadOnly);
			Assert("Description should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert("IsUnique should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.IsUniqueInfo.ReadOnly);
		}

		public void TestSystemDefinedTRFReadOnlyMembers()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "TRF";
			transportReferenceNumberType.Description = (NoResString)"Transport Reference Number";
			transportReferenceNumberType.IsUnique = true;
			transportReferenceNumberType.SystemDefined = true;

			Assert("Code should be read only because it is system defined", transportReferenceNumberType.CodeInfo.ReadOnly);
			Assert("Description should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert("IsUnique should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.IsUniqueInfo.ReadOnly);
		}

		public void TestSystemDefinedCBKReadOnlyMembers()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "CBK";
			transportReferenceNumberType.Description = (NoResString)"Carrier Booking Number";
			transportReferenceNumberType.IsUnique = true;
			transportReferenceNumberType.SystemDefined = true;

			Assert("Code should be read only because it is system defined", transportReferenceNumberType.CodeInfo.ReadOnly);
			Assert("Description should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert("IsUnique should be read only because its code is listed in SystemDefinedCodesMakingAllFieldsReadOnly", transportReferenceNumberType.IsUniqueInfo.ReadOnly);
		}

		public void TestSystemDefinedOtherCodeReadOnlyMembers()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "XXX";
			transportReferenceNumberType.Description = (NoResString)"Other Reference Number Type";
			transportReferenceNumberType.IsUnique = true;
			transportReferenceNumberType.SystemDefined = true;

			Assert("Code should be read only because it is system defined", transportReferenceNumberType.CodeInfo.ReadOnly);
			Assert("Description should not be read only because its code is not listed in SystemDefinedCodesMakingAllFieldsReadOnly", !transportReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert("IsUnique should not be read only because its code is not listed in SystemDefinedCodesMakingAllFieldsReadOnly", !transportReferenceNumberType.IsUniqueInfo.ReadOnly);
		}

		public void TestNonSystemDefinedOtherCodeReadOnlyMembers()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = "XXX";
			transportReferenceNumberType.Description = (NoResString)"Other Reference Number Type";
			transportReferenceNumberType.IsUnique = true;
			transportReferenceNumberType.SystemDefined = false;

			Assert("Code should not be read only because it is not system defined", !transportReferenceNumberType.CodeInfo.ReadOnly);
			Assert("Description should not be read only because it is not system defined", !transportReferenceNumberType.DescriptionInfo.ReadOnly);
			Assert("IsUnique should not be read only because it is not system defined", !transportReferenceNumberType.IsUniqueInfo.ReadOnly);
		}

		public void TestSystemDefined()
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			AssertEquals("Should not be system defined by default.", false, transportReferenceNumberType.SystemDefined);

			transportReferenceNumberType.SystemDefined = true;
			AssertEquals("Setter should set value.", true, transportReferenceNumberType.SystemDefined);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new TransportReferenceNumberType();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransportReferenceNumberType();
		}
	}
}
