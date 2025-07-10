using System.IO;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(CMMFlags))]
	internal class CMMFlagsTest : RegistryBusinessObjectTemplateTestCase<CMMFlags>
	{
		public void TestSerialisation()
		{
			const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" + "<CMMFlags>\r\n" + "  <YardGateIn>Y</YardGateIn>\r\n" + "  <YardGateOut>N</YardGateOut>\r\n" + "  <DepotGateIn>N</DepotGateIn>\r\n" + "  <DepotGateOut>Y</DepotGateOut>\r\n" + "  <WharfGateIn>Y</WharfGateIn>\r\n" + "  <WharfGateOut>N</WharfGateOut>\r\n" + "  <Load>N</Load>\r\n" + "  <Discharge>Y</Discharge>\r\n" + "</CMMFlags>\r\n" + "";
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(CMMFlags));
			using (StringWriter writer = new StringWriter())
			{
				CMMFlags flags = new CMMFlags();
				flags.YardGateIn = true;
				flags.DepotGateOut = true;
				flags.WharfGateIn = true;
				flags.Discharge = true;
				serialiser.Serialize(writer, flags);
				writer.Flush();
				AssertMultilineASCIIEquals("", xml, writer.ToString());
			}

			using (StringReader reader = new StringReader(xml))
			{
				CMMFlags flags = (CMMFlags)serialiser.Deserialize(reader);
				CombineAssertions(delegate
				{
					AssertEquals("YardGateIn", true, flags.YardGateIn);
					AssertEquals("YardGateOut", false, flags.YardGateOut);
					AssertEquals("DepotGateIn", false, flags.DepotGateIn);
					AssertEquals("DepotGateOut", true, flags.DepotGateOut);
					AssertEquals("WharfGateIn", true, flags.WharfGateIn);
					AssertEquals("WharfGateOut", false, flags.WharfGateOut);
					AssertEquals("Load", false, flags.Load);
					AssertEquals("Discharge", true, flags.Discharge);
				});
			}
		}

		#region Implementation
		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override CMMFlags GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override CMMFlags GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		CMMFlags NewPopulatedBusinessObject()
		{
			CMMFlags result = new CMMFlags();
			result.WharfGateIn = true;
			result.Load = true;
			return result;
		}
		#endregion
	}
}
