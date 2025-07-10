using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CertificateTypeCollection))]
	sealed class CertificateTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CertificateTypeCollection>
	{
		public void TestFind()
		{
			CertificateType type = Collection.AddNew();
			type.SetupValues("AAA", "A 1 desc", false, false, false, AlertTypeList.Codes.ErrorAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B 1 desc", false, false, false, AlertTypeList.Codes.ErrorAlert);

			type = Collection.AddNew();
			type.SetupValues("AAA", "A 2 desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B 2 desc", false, false, false, AlertTypeList.Codes.NoAlert);

			AssertCertificateType("AAA", "A 1 desc", false, false, false, AlertTypeList.Codes.ErrorAlert, Collection.Find("AAA"));
			AssertCertificateType("BBB", "B 1 desc", false, false, false, AlertTypeList.Codes.ErrorAlert, Collection.Find("BBB"));
		}

		public void TestIsDuplicated()
		{
			CertificateType type = Collection.AddNew();
			type.SetupValues("AAA", "A 1 desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("AAA", "A 2 desc", false, false, false, AlertTypeList.Codes.ErrorAlert);

			AssertEquals(true, Collection.IsDuplicated("AAA"));
			AssertEquals(false, Collection.IsDuplicated("BBB"));
		}

		public void TestIsMandatory()
		{
			CertificateType type = Collection.AddNew();
			type.SetupValues("AAA", "A 1 desc", true, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("AAA", "A 2 desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B 1 desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B 2 desc", true, false, false, AlertTypeList.Codes.NoAlert);

			AssertEquals(true, Collection.IsMandatory("AAA"));
			AssertEquals(false, Collection.IsMandatory("BBB"));
		}

		public void TestIsUnique()
		{
			CertificateType type = Collection.AddNew();
			type.SetupValues("AAA", "A 1 desc", false, true, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("AAA", "A 2 desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B 1 desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B 2 desc", false, true, false, AlertTypeList.Codes.NoAlert);

			AssertEquals(true, Collection.IsUnique("AAA"));
			AssertEquals(false, Collection.IsUnique("BBB"));
		}

		public void TestIsSystem()
		{
			CertificateType type = Collection.AddNew();
			type.SetupValues("AAA", "A 1 desc", false, false, true, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("AAA", "A 2 desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B 1 desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B 2 desc", false, false, true, AlertTypeList.Codes.NoAlert);

			AssertEquals(true, Collection.IsSystem("AAA"));
			AssertEquals(false, Collection.IsSystem("BBB"));
		}

		#region IXmlSerializable Members

		public void TestSerialisation()
		{
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(CertificateTypeCollection));

			CertificateTypeCollection collection1 = new CertificateTypeCollection();
			CertificateType type = collection1.AddNew();
			type.SetupValues("AAA", "A desc", false, false, false, AlertTypeList.Codes.ErrorAlert);
			type = collection1.AddNew();
			type.SetupValues("BBB", "B desc", true, true, true, AlertTypeList.Codes.ErrorAlert);

			string xml;
			using (StringWriter stream = new StringWriter())
			{
				serialiser.Serialize(stream, collection1);
				xml = stream.ToString();
			}

			const string expectedXml =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>\n" +
				"<CertificateTypeCollection>\n" +
				"  <CertificateType>\n" +
				"    <Code>AAA</Code>\n" +
				"    <Description>A desc</Description>\n" +
				"    <IsSystem>N</IsSystem>\n" +
				"    <IsMandatory>N</IsMandatory>\n" +
				"    <IsUnique>N</IsUnique>\n" +
				"    <AlertType>ERR</AlertType>\n" +
				"  </CertificateType>\n" +
				"  <CertificateType>\n" +
				"    <Code>BBB</Code>\n" +
				"    <Description>B desc</Description>\n" +
				"    <IsSystem>Y</IsSystem>\n" +
				"    <IsMandatory>Y</IsMandatory>\n" +
				"    <IsUnique>Y</IsUnique>\n" +
				"    <AlertType>ERR</AlertType>\n" +
				"  </CertificateType>\n" +
				"</CertificateTypeCollection>\n" +
				"";

			AssertMultilineASCIIEquals("", expectedXml, xml);

			CertificateTypeCollection collection2;
			using (StringReader stream = new StringReader(xml))
			{
				collection2 = (CertificateTypeCollection)serialiser.Deserialize(stream);
			}

			for (int i = 0; i < collection1.Count; i++)
			{
				AssertEquals("Code", collection1[i].Code, collection2[i].Code);
				AssertEquals("Description", collection1[i].Description, collection2[i].Description);
				AssertEquals("IsMandatory", collection1[i].IsMandatory, collection2[i].IsMandatory);
				AssertEquals("IsUnique", collection1[i].IsUnique, collection2[i].IsUnique);
				AssertEquals("IsSystem", collection1[i].IsSystem, collection2[i].IsSystem);
				AssertEquals("AlertType", collection1[i].AlertType, collection2[i].AlertType);
			}
		}

		#endregion

		#region ICodeDescriptionPairList Members

		public void TestContainsCode()
		{
			CertificateType type = Collection.AddNew();
			type.SetupValues("AAA", "A desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B desc", false, false, false, AlertTypeList.Codes.NoAlert);

			AssertEquals(true, ((ICodeDescriptionPairList)Collection).ContainsCode("BBB"));
			AssertEquals(false, ((ICodeDescriptionPairList)Collection).ContainsCode("CCC"));
		}

		public void TestGetDescriptionFromCode()
		{
			CertificateType type = Collection.AddNew();
			type.SetupValues("AAA", "A desc", false, false, false, AlertTypeList.Codes.NoAlert);

			type = Collection.AddNew();
			type.SetupValues("BBB", "B desc", false, false, false, AlertTypeList.Codes.NoAlert);

			AssertEquals("A desc", ((ICodeDescriptionPairList)Collection).GetDescriptionFromCode("AAA"));
			AssertEquals("B desc", ((ICodeDescriptionPairList)Collection).GetDescriptionFromCode("BBB"));
		}

		#endregion

		#region Implementation

		void AssertCertificateType(ZString code, ZString description, ZBool isMandatory, ZBool isUnique, ZBool isSystem, ZString alertType, CertificateType type)
		{
			AssertEquals("Code", code, type.Code);
			AssertEquals("Description", description, type.Description);
			AssertEquals("IsMandatory", isMandatory, type.IsMandatory);
			AssertEquals("IsUnique", isUnique, type.IsUnique);
			AssertEquals("IsSystem", isSystem, type.IsSystem);
			AssertEquals("AlertType", alertType, type.AlertType);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CertificateTypeCollection GetCollectionToTest()
		{
			return new CertificateTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CertificateType(Collection);
		}

		#endregion
	}
}
