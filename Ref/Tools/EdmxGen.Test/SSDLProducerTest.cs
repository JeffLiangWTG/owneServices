using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EdmxGen.Test
{
	[TestFixture]
	public class SSDLProducerTest
	{
		[Test]
		public void HasSetCorrectSSDLFile()
		{
			var ssdlProducer = new SsdlProducer(tablesConfigMock.Object, IgnoredTables.Safe);
			ssdlProducer.SetSSDLFile(new string[] { ssdlFilePath, "anotherone.any", "onemore.mor" });

			Assert.AreEqual(ssdlProducer.SSDLFilePath, ssdlFilePath);
		}

		[Test]
		public void IsIgnoringTablesSafe()
		{
			tablesConfigMock.Setup(x => x.GetTablesConfigurations()).Returns(new Dictionary<string, ColumnConfiguration[]>());

			var ssdlProducer = new SsdlProducer(tablesConfigMock.Object, IgnoredTables.Safe);
			ssdlProducer.SetSSDLFile(new string[] { ssdlFilePath });
			var document = ssdlProducer.LoadDocument();

			var rootElement = document.Elements().First();
			var entityContainer = rootElement.Elements().First();

			foreach (var tblToIgnore in IgnoredTables.Safe)
			{
				Assert.True(!entityContainer.Elements().Any(x => x.FirstAttribute.Value == tblToIgnore));
			}
		}

		[Test]
		public void IsIgnoringTablesStaging()
		{
			tablesConfigMock.Setup(x => x.GetTablesConfigurations()).Returns(new Dictionary<string, ColumnConfiguration[]>());

			var ssdlProducer = new SsdlProducer(tablesConfigMock.Object, IgnoredTables.Staging);
			ssdlProducer.SetSSDLFile(new string[] { ssdlFilePath });
			var document = ssdlProducer.LoadDocument();

			var rootElement = document.Elements().First();
			var entityContainer = rootElement.Elements().First();

			foreach (var tblToIgnore in IgnoredTables.Safe)
			{
				Assert.True(!entityContainer.Elements().Any(x => x.FirstAttribute.Value == tblToIgnore));
			}
		}

		[Test]
		public void LogicalRelationshipConfiguration()
		{
			var tblConfiguration = new Dictionary<string, ColumnConfiguration[]>
			{
				{
					"UNDGAttributeZZ",
					new ColumnConfiguration[]
					{
						new ColumnConfiguration { Column = "DAZ_ParentPK", LogicalRelationships = new LogicalRelationship[] {
							new LogicalRelationship { Table = "UNDGSubstanceADR", ReferencedTable = "UNDGAttributeZZ", ReferencedColumn = "DAZ_ParentPK", IsNullable = true },
							new LogicalRelationship { Table = "UNDGSubstanceRID", ReferencedTable = "UNDGAttributeZZ", ReferencedColumn = "DAZ_ParentPK", IsNullable = true }
						} }
					}
				}
			};

			tablesConfigMock.Setup(x => x.GetTablesConfigurations()).Returns(tblConfiguration);

			var ssdlProducer = new SsdlProducer(tablesConfigMock.Object, IgnoredTables.Staging);
			ssdlProducer.SetSSDLFile(new string[] { ssdlFilePath });
			ssdlProducer.LoadDocument();

			tblConfiguration = tablesConfigMock.Object.GetTablesConfigurations();

			var tableConfig = tblConfiguration["UNDGAttributeZZ"];
			Assert.That(tableConfig, Is.Not.Null);
			Assert.That(tableConfig[0].LogicalRelationships, Is.Not.Null);
			Assert.That(tableConfig[0].LogicalRelationships.First().Column, Is.EqualTo("ADR_PK"));
			Assert.That(tableConfig[0].LogicalRelationships.First().GetAssociationName(), Is.EqualTo("LFK_UNDGSubstanceADR_UNDGAttributeZZ"));

			Assert.That(tableConfig[0].LogicalRelationships.Last().Column, Is.EqualTo("RID_PK"));
			Assert.That(tableConfig[0].LogicalRelationships.Last().GetAssociationName(), Is.EqualTo("LFK_UNDGSubstanceRID_UNDGAttributeZZ"));
		}

		[Test]
		public void LogicalRelationshipDocumentContent()
		{
			var tblConfiguration = new Dictionary<string, ColumnConfiguration[]>
			{
				{
					"UNDGAttributeZZ",
					new ColumnConfiguration[]
					{
						new ColumnConfiguration { Column = "DAZ_ParentPK", LogicalRelationships = new LogicalRelationship[] {
							new LogicalRelationship { Table = "UNDGSubstanceADR", ReferencedTable = "UNDGAttributeZZ", ReferencedColumn = "DAZ_ParentPK", IsNullable = true },
							new LogicalRelationship { Table = "UNDGSubstanceRID", ReferencedTable = "UNDGAttributeZZ", ReferencedColumn = "DAZ_ParentPK", IsNullable = true }
						} }
					}
				}
			};

			tablesConfigMock.Setup(x => x.GetTablesConfigurations()).Returns(tblConfiguration);

			var ssdlProducer = new SsdlProducer(tablesConfigMock.Object, IgnoredTables.Safe);
			ssdlProducer.SetSSDLFile(new string[] { ssdlFilePath });
			var document = ssdlProducer.LoadDocument();
			tblConfiguration = tablesConfigMock.Object.GetTablesConfigurations();

			var rootElement = document.Elements().First();

			foreach (var logicalRelationship in tblConfiguration["UNDGAttributeZZ"][0].LogicalRelationships)
			{
				var associationSetNode = rootElement.Elements().FirstOrDefault().Elements().FirstOrDefault(x => x.Name.LocalName == Constants.Elements.AssociationSet && x.FirstAttribute.Value == logicalRelationship.GetAssociationName());
				var associationNode = rootElement.Elements().FirstOrDefault(x => x.Name.LocalName == Constants.Elements.Association && x.FirstAttribute.Value == logicalRelationship.GetAssociationName());

				Assert.That(associationSetNode.ToString(), Does.Contain($@"<AssociationSet Name=""{logicalRelationship.GetAssociationName()}"" Association=""Odyssey_RefDb_Ent_ZZModel.Store.{logicalRelationship.GetAssociationName()}"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"">
  <End Role=""{logicalRelationship.Table}"" EntitySet=""{logicalRelationship.Table}"" />
  <End Role=""{logicalRelationship.ReferencedTable}"" EntitySet=""{logicalRelationship.ReferencedTable}"" />
</AssociationSet>"));

				Assert.That(associationNode.ToString(), Does.Contain($@"<Association Name=""{logicalRelationship.GetAssociationName()}"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"">
  <End Role=""{logicalRelationship.Table}"" Type=""Odyssey_RefDb_Ent_ZZModel.Store.{logicalRelationship.Table}"" Multiplicity=""0..1"" />
  <End Role=""{logicalRelationship.ReferencedTable}"" Type=""Odyssey_RefDb_Ent_ZZModel.Store.{logicalRelationship.ReferencedTable}"" Multiplicity=""*"" />
  <ReferentialConstraint>
    <Principal Role=""{logicalRelationship.Table}"">
      <PropertyRef Name=""{logicalRelationship.Column}"" />
    </Principal>
    <Dependent Role=""{logicalRelationship.ReferencedTable}"">
      <PropertyRef Name=""{logicalRelationship.ReferencedColumn}"" />
    </Dependent>
  </ReferentialConstraint>
</Association>"));
			}
		}

		[Test]
		public void IsConfiguringTablesProperties()
		{
			var tblConfiguration = new Dictionary<string, ColumnConfiguration[]>
			{
				{
					"RefCusApplicability",
					new ColumnConfiguration[]
			{
				new ColumnConfiguration { Column = "ZZT_DataSetPK", Configuration = "StoreGeneratedPattern=Computed" },
				new ColumnConfiguration { Column = "ZZT_DataSetCode", Configuration = "StoreGeneratedPattern=Computed" }
			}
				},
				{
					"RefCusCondition",
					new ColumnConfiguration[]
			{
				new ColumnConfiguration { Column = "ZX1_DataSetPK", Configuration = "StoreGeneratedPattern=Computed" },
				new ColumnConfiguration { Column = "ZX1_DataSetCode", Configuration = "StoreGeneratedPattern=Computed" }
			}
				},
				{
					"RefCusConditionValue",
					new ColumnConfiguration[]
			{
				new ColumnConfiguration { Column = "ZX3_DataSetPK", Configuration = "StoreGeneratedPattern=Computed" },
				new ColumnConfiguration { Column = "ZX3_DataSetCode", Configuration = "StoreGeneratedPattern=Computed" }
			}
				},
				{
					"RefCusExcludedTradeGroup",
					new ColumnConfiguration[]
			{
				new ColumnConfiguration { Column = "ZZC_DataSetPK", Configuration = "StoreGeneratedPattern=Computed" },
				new ColumnConfiguration { Column = "ZZC_DataSetCode", Configuration = "StoreGeneratedPattern=Computed" }
			}
				},
				{
					"RefCusRate",
					new ColumnConfiguration[]
			{
				new ColumnConfiguration { Column = "ZZ2_DataSetPK", Configuration = "StoreGeneratedPattern=Computed" },
				new ColumnConfiguration { Column = "ZZ2_DataSetCode", Configuration = "StoreGeneratedPattern=Computed" }
			}
				},
				{
					"RefCusRateAttribute",
					new ColumnConfiguration[]
			{
				new ColumnConfiguration { Column = "ZZJ_DataSetPK", Configuration = "StoreGeneratedPattern=Computed" },
				new ColumnConfiguration { Column = "ZZJ_DataSetCode", Configuration = "StoreGeneratedPattern=Computed" }
			}
				},
				{
					"RefCusTariffAttribute",
					new ColumnConfiguration[]
			{
				new ColumnConfiguration { Column = "ZZ3_DataSetPK", Configuration = "StoreGeneratedPattern=Computed" },
				new ColumnConfiguration { Column = "ZZ3_DataSetCode", Configuration = "StoreGeneratedPattern=Computed" }
			}
				},
				{
					"RefCusTariffUOM",
					new ColumnConfiguration[]
			{
				new ColumnConfiguration { Column = "ZZ8_DataSetPK", Configuration = "StoreGeneratedPattern=Computed" },
				new ColumnConfiguration { Column = "ZZ8_DataSetCode", Configuration = "StoreGeneratedPattern=Computed" }
			}
				},
				{
					"RefCusVATApplicability",
					new ColumnConfiguration[]
			{
				new ColumnConfiguration { Column = "ZX5_DataSetPK", Configuration = "StoreGeneratedPattern=Computed" },
				new ColumnConfiguration { Column = "ZX5_DataSetCode", Configuration = "StoreGeneratedPattern=Computed" }
			}
				}
			};

			tablesConfigMock.Setup(x => x.GetTablesConfigurations()).Returns(tblConfiguration);

			var ssdlProducer = new SsdlProducer(tablesConfigMock.Object, IgnoredTables.Safe);
			ssdlProducer.SetSSDLFile(new string[] { ssdlFilePath });
			var document = ssdlProducer.LoadDocument();
			tblConfiguration = tablesConfigMock.Object.GetTablesConfigurations();

			var rootElement = document.Elements().First();

			foreach (var tblConfig in tblConfiguration)
			{
				Assert.True(tblConfig.Key != null);
				var tblElement = rootElement.Elements().FirstOrDefault(o => o.Name.LocalName == Constants.Elements.EntityType && o.FirstAttribute.Value == tblConfig.Key);
				Assert.True(tblElement != null);
				foreach (var clnConfig in tblConfig.Value)
				{
					var clnElement = tblElement.Elements().FirstOrDefault(o => o.Name.LocalName == Constants.Elements.Property && o.FirstAttribute.Value == clnConfig.Column);
					var clnConfigurationValue = clnConfig.GetConfigurationValue();

					Assert.True(clnElement.Attributes().Any(x => x.Name.LocalName == clnConfigurationValue.Item1 && x.Value == clnConfigurationValue.Item2));
				}
			}
		}

		[SetUp]
		public void SetUp()
		{
			ssdlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"testfiles\", "Test.ssdl");
			tablesConfigMock = new Mock<ITablesConfig>();
		}

		string ssdlFilePath;
		Mock<ITablesConfig> tablesConfigMock;
	}
}
