using System.IO;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing
{
	sealed class ConfigurationExtensionTest : TestCaseWithFactory
	{
		public void TestToXml()
		{
			var defaultValueOfPort = new FTP().Port;

			var configuration = new Configuration()
			{
				Name = "HI",
				Group = new GroupCollection
				{
					new Group
					{
						Type = "BOB", Reference = "BUILDER"
					},
					new Group
					{
						Items = new object[]
						{
							new FTP
							{
								Server = "10.0.0.1",
								Port = 8080,
							}
						}
					},
					new Group
					{
						Items = new object[]
						{
							new FTP
							{
								Server = "127.0.0.1",
							}
						}
					}
				}
			};

#if NETFRAMEWORK
			var expectedXml = $@"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""HI"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""BOB"" Reference=""BUILDER"" />
  <Group>
    <FTP>
      <Server>10.0.0.1</Server>
      <Port>8080</Port>
    </FTP>
  </Group>
  <Group>
    <FTP>
      <Server>127.0.0.1</Server>
      <Port>{defaultValueOfPort}</Port>
    </FTP>
  </Group>
</Configuration>";
#else
			var expectedXml = "<Configuration xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"HI\" xmlns=\"http://www.wisetechglobal.com/Schemas/Configuration\">\r\n  <Group Type=\"BOB\" Reference=\"BUILDER\" />\r\n  <Group>\r\n    <FTP>\r\n      <Server>10.0.0.1</Server>\r\n      <Port>8080</Port>\r\n    </FTP>\r\n  </Group>\r\n  <Group>\r\n    <FTP>\r\n      <Server>127.0.0.1</Server>\r\n      <Port>21</Port>\r\n    </FTP>\r\n  </Group>\r\n</Configuration>";
#endif

			var actualXml = configuration.ToXml().Trim();

			AssertContains(expectedXml, actualXml);
		}

		public void TestDeserializeToConfiguration()
		{
			var configuration = new Configuration()
			{
				Name = "HI",
				Group = new GroupCollection() { new Group() { Type = "BOB", Reference = "BUILDER" } }
			};
			using (var reader = new StringReader(configuration.ToXml()))
			{
				var configuration2 = reader.DeserializeToConfiguration();
				AssertEquals("HI", configuration2.Name);
				AssertEquals(1, configuration2.Group.Count);
				var group = configuration.Group[0];
				AssertEquals("BOB", group.Type);
				AssertEquals("BUILDER", group.Reference);
			}
		}
	}
}
