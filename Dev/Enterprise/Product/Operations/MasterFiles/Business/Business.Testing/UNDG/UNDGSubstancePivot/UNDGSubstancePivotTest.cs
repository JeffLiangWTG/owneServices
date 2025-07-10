using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGSubstancePivot))]
	sealed class UNDGSubstancePivotTest : EnterpriseBusinessObjectTestCase
	{
		public void Test_Add_UNDGSubstance_Using_ManyToManyComplexRelationship()
		{
			var undgDataItem1 = Factory.NewWithValidTestData<UNDGDataItem>();
			var relationship = SubstanceRelationship.New(undgDataItem1);
			var collection = new ActiveBusinessObjectCollection<UNDGSubstance>(Factory, relationship);

			var standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance1.DG_Standard = standard;
			undgSubstance1.DG_UNNO = "2910";

			collection.Add(undgSubstance1);
			Factory.Save();

			var substancePivot = undgDataItem1.UNDGSubstancePivotCollection.FirstOrDefault();
			AssertNotNull(substancePivot);
		}

		class SubstanceRelationship : ManyToManyComplexRelationship
		{
			public static SubstanceRelationship New(UNDGDataItem master)
			{
				var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
				var pivotTableFKsToMaster = new List<SchemaColumn>
				{
				schemaResolver.GetSchemaColumn(UNDGSubstancePivot.Schema.DP_ParentId, UNDGSubstancePivot.Schema.TableName),
				};

				var masterTableFKsToPivots = new List<SchemaColumn>
				{
				schemaResolver.GetSchemaColumn(UNDGDataItem.Schema.PK, UNDGDataItem.Schema.TableName),
				};

				var pivotTableFKsToElements = new List<SchemaColumn>
				{
				schemaResolver.GetSchemaColumn(UNDGSubstancePivot.Schema.DP_UNNO, UNDGSubstancePivot.Schema.TableName),
				schemaResolver.GetSchemaColumn(UNDGSubstancePivot.Schema.DP_Standard, UNDGSubstancePivot.Schema.TableName),
				schemaResolver.GetSchemaColumn(UNDGSubstancePivot.Schema.DP_Variant, UNDGSubstancePivot.Schema.TableName),
				};

				var elementTableFKsToPivots = new List<SchemaColumn>
				{
				schemaResolver.GetSchemaColumn(UNDGSubstance.Schema.DG_UNNO, UNDGSubstance.Schema.TableName),
				schemaResolver.GetSchemaColumn(UNDGSubstance.Schema.DG_Standard, UNDGSubstance.Schema.TableName),
				schemaResolver.GetSchemaColumn(UNDGSubstance.Schema.DG_Variant, UNDGSubstance.Schema.TableName),
				};

				var pivotTableRelationshipColumn = schemaResolver.GetSchemaColumn(UNDGSubstancePivot.Schema.DP_ParentTableCode, UNDGSubstancePivot.Schema.TableName);
				var pivotTableRelationshipValue = "DI";

				var relationship = new SubstanceRelationship(master, pivotTableFKsToMaster, masterTableFKsToPivots, pivotTableFKsToElements, elementTableFKsToPivots, pivotTableRelationshipColumn, pivotTableRelationshipValue);
				return relationship;
			}

			SubstanceRelationship(UNDGDataItem master, List<SchemaColumn> pivotTableFKsToMaster, List<SchemaColumn> masterTableFKsToPivots,
				List<SchemaColumn> pivotTableFKsToElements, List<SchemaColumn> elementTableFKsToPivots,
				SchemaColumn pivotTableRelationshipColumn, object pivotTableRelationshipValue)
				: base(master, typeof(UNDGSubstance), typeof(UNDGSubstancePivot), new ZQuery(), pivotTableFKsToMaster, masterTableFKsToPivots, pivotTableFKsToElements, elementTableFKsToPivots,
					  pivotTableRelationshipColumn, pivotTableRelationshipValue) { }
		}

		public void TestUNDGSubstancePivot_ShouldRefuseToSaveData_WithInvalidParentTableCode()
		{
			var pivot = Factory.NewWithValidTestData<UNDGSubstancePivot>();

			var aValidParentTableCode = UNDGDataItemSchema.Constants.Prefix;
			pivot.DP_ParentTableCode = aValidParentTableCode;
			AssertNoExceptionThrown("Valid data should be allowed to be saved.", () => Factory.Save());

			var anInvalidParentTableCode = UNDGSubstancePivotSchema.Constants.Prefix;
			pivot.DP_ParentTableCode = anInvalidParentTableCode;
			AssertExceptionThrown<ZSaveException>("Invalid data should not be allowed to be saved.", () => Factory.Save());
		}
	}
}
