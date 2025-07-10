using System.IO;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	sealed class OrderNumberOwnersRefSpecificFieldSubGroupTest : TestCase
	{
		public void TestGetSubQuery_OrCategories()
		{
			var query = new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And);
			var groupQuery = new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And);
			query.Children.Add(groupQuery);
			var redQuery = new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And);
			redQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.StartsWith, "A"), FilterOrCategory.Red, JoinCondition.Or));
			redQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.DoesNotStartWith, "B"), FilterOrCategory.Red, JoinCondition.Or));
			groupQuery.Children.Add(redQuery);
			groupQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.StartsWith, "C"), FilterOrCategory.None, JoinCondition.And));
			groupQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.StartsWith, "D"), FilterOrCategory.None, JoinCondition.And));
			var greenQuery = new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And);
			greenQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.StartsWith, "E"), FilterOrCategory.Green, JoinCondition.Or));
			greenQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.StartsWith, "F"), FilterOrCategory.Green, JoinCondition.Or));
			groupQuery.Children.Add(greenQuery);
			groupQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.StartsWith, "G"), FilterOrCategory.None, JoinCondition.And));
			var blueQuery = new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And);
			blueQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.StartsWith, "H"), FilterOrCategory.Blue, JoinCondition.Or));
			groupQuery.Children.Add(blueQuery);
			groupQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.DoesNotStartWith, "I"), FilterOrCategory.None, JoinCondition.And));
			groupQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.DoesNotStartWith, "J"), FilterOrCategory.None, JoinCondition.And));
			var brownQuery = new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And);
			brownQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.DoesNotStartWith, "J"), FilterOrCategory.Brown, JoinCondition.Or));
			brownQuery.Children.Add(new QueryBlueprintPart(new ZQuery(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.DoesNotStartWith, "K"), FilterOrCategory.Brown, JoinCondition.Or));
			groupQuery.Children.Add(brownQuery);

			var resourceStream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.Module.Test.BaseJobDeclaration.OrderNumberOwnersRefSpecificFieldSubGroupTest_OrCategories.sql");
			using (var streamReader = new StreamReader(resourceStream))
			{
				var expectedSql = streamReader.ReadToEnd();
				AssertMultilineASCIIEquals(expectedSql, subGroup.GetSubQuery(query).Construct().LiteralTextSqlFormatted);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			subGroup = new OrderNumberOwnersRefSpecificFieldSubGroup();
		}

		OrderNumberOwnersRefSpecificFieldSubGroup subGroup;
	}
}
