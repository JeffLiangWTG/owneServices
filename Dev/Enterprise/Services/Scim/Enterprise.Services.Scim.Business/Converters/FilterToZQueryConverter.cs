using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.Scim.Models;
using Enterprise.ZArchitecture.Schema;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Parser.Exceptions;
using SimpleIdServer.Scim.Parser.Expressions;
using SimpleIdServer.Scim.Parser.Operators;

namespace Enterprise.Services.Scim.Business
{
	public class FilterToZQueryConverter
	{
		public ZDBOnlyQuery ConvertToZQuery<T>(SCIMExpression expression) where T : BusinessObject
		{
			var main = new ZDBOnlyQuery(typeof(T));

			QueryType queryType;
			if (typeof(T) == typeof(GlbStaff))
			{
				queryType = QueryType.Staff;
				main.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);
				main.AddToFilter(GlbStaffSchema.GS_IsResource, false);
				main.AddToFilter(GlbStaffSchema.GS_IsRobot, false);
				main.AddToFilter(GlbStaffSchema.GS_IsDevice, false);
			}
			else if (typeof(T) == typeof(GlbGroup))
			{
				queryType = QueryType.Group;
			}
			else
			{
				throw new SCIMSchemaNotFoundException();
			}

			if (expression != null)
			{
				main.AddToFilter(GetQuery(expression, queryType));
			}

			return main;
		}

		ZQuery GetQuery(SCIMExpression expression, QueryType queryType, string parent = "")
		{
			var sub = new ZQuery();

			if (expression is SCIMLogicalExpression)
			{
				var logicalExpression = (SCIMLogicalExpression)expression;

				if (logicalExpression.LeftExpression is SCIMLogicalExpression)
				{
					AddLogicalExpression(parent, sub, logicalExpression.LeftExpression, logicalExpression.LogicalOperator, queryType);
				}

				if (logicalExpression.LeftExpression is SCIMComparisonExpression)
				{
					var comparison = (SCIMComparisonExpression)logicalExpression.LeftExpression;
					AddComparisonExpression(comparison, sub, parent, queryType);
				}

				if (logicalExpression.LeftExpression is SCIMComplexAttributeExpression)
				{
					AddComplexExpression(sub, (SCIMComplexAttributeExpression)logicalExpression.LeftExpression, GetJoinCondition(logicalExpression.LogicalOperator), queryType);
				}

				if (logicalExpression.RightExpression is SCIMLogicalExpression)
				{
					AddLogicalExpression(parent, sub, logicalExpression.RightExpression, logicalExpression.LogicalOperator, queryType);
				}

				if (logicalExpression.RightExpression is SCIMComparisonExpression)
				{
					var comparison = (SCIMComparisonExpression)logicalExpression.RightExpression;
					AddComparisonExpression(comparison, sub, GetJoinCondition(logicalExpression.LogicalOperator), parent, queryType);
				}

				if (logicalExpression.RightExpression is SCIMComplexAttributeExpression)
				{
					AddComplexExpression(sub, (SCIMComplexAttributeExpression)logicalExpression.RightExpression, GetJoinCondition(logicalExpression.LogicalOperator), queryType);
				}
			}
			else if (expression is SCIMComplexAttributeExpression)
			{
				AddComplexExpression(sub, (SCIMComplexAttributeExpression)expression, queryType);
			}
			else if (expression is SCIMComparisonExpression)
			{
				var comparison = (SCIMComparisonExpression)expression;
				AddComparisonExpression(comparison, sub, parent, queryType);
			}

			return sub;
		}

		void AddComplexExpression(ZQuery sub, SCIMComplexAttributeExpression expression, QueryType queryType)
		{
			AddComplexExpression(sub, expression, JoinCondition.And, queryType);
		}

		void AddComplexExpression(ZQuery sub, SCIMComplexAttributeExpression expression, JoinCondition joinCondition, QueryType queryType)
		{
			sub.AddToFilter(GetQuery(expression.GroupingFilter, queryType, expression.Name), joinCondition);
		}

		void AddLogicalExpression(string parent, ZQuery sub, SCIMExpression expression, SCIMLogicalOperators operators, QueryType queryType)
		{
			sub.AddToFilter(GetQuery(expression, queryType, parent), GetJoinCondition(operators));
		}

		void AddComparisonExpression(SCIMComparisonExpression comparison, ZQuery sub, string parent, QueryType queryType)
		{
			AddComparisonExpression(comparison, sub, JoinCondition.And, parent, queryType);
		}

		void AddComparisonExpression(SCIMComparisonExpression comparison, ZQuery sub, JoinCondition joinCondition, string parent, QueryType queryType)
		{
			var name = comparison.LeftExpression.Name;

			if (ShouldSkipFilter(name, comparison.Value, parent))
			{
				return;
			}

			var child = comparison.LeftExpression.Child;

			while (child != null)
			{
				name += $".{child.Name}";
				child = child.Child;
			}

			SchemaColumn column;

			switch (queryType)
			{
				case QueryType.Staff:
					{
						if (name.Equals(AttributeNames.HomeBranch))
						{
							AddSubQueryFKToCode(comparison, sub, joinCondition, typeof(GlbBranch), GlbStaffSchema.GS_GB_HomeBranch, GlbBranchSchema.GB_Code);
							return;
						}
						else if (name.Equals(AttributeNames.HomeDepartment))
						{
							AddSubQueryFKToCode(comparison, sub, joinCondition, typeof(GlbDepartment), GlbStaffSchema.GS_GE_HomeDepartment, GlbDepartmentSchema.GE_Code);
							return;
						}
						else
						{
							column = StaffColumnHelper.GetStaffColumn(name, parent);
							break;
						}
					}
				case QueryType.Group:
					{
						if (parent.Equals(AttributeNames.Members))
						{
							var main = new ZDBOnlyQuery(typeof(GlbGroup));

							var staffGroupsSub = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GG);
							staffGroupsSub.AddToFilter(GlbGroupLinkSchema.GK_GS, GetSQLComparisonOperator(comparison.ComparisonOperator), GetValueForColumn(GlbGroupLinkSchema.GK_GS, comparison.Value));

							var groupGroupsSub = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupSchema.GG_GG_ParentGroup);
							groupGroupsSub.AddToFilter(GlbGroupSchema.PK, GetSQLComparisonOperator(comparison.ComparisonOperator), GetValueForColumn(GlbGroupSchema.PK, comparison.Value));

							main.AddSubQuery(staffGroupsSub, JoinCondition.And);
							main.AddSubQuery(groupGroupsSub, JoinCondition.Or);

							sub.AddToFilter(main, joinCondition);
							return;
						}
						else
						{
							column = GroupColumnHelper.GetGroupColumn(name);
							break;
						}
					}
				default:
					throw new SCIMNoTargetException("Unknown query type.");
			}

			if (column == null)
			{
				throw new SCIMFilterException($"Filtering is not supported for the column: [{comparison.LeftExpression.Name}].");
			}

			sub.AddToFilter(joinCondition, column, GetSQLComparisonOperator(comparison.ComparisonOperator), GetValueForColumn(column, comparison.Value));
		}

		void AddSubQueryFKToCode(SCIMComparisonExpression comparison, ZQuery sub, JoinCondition joinCondition, Type subType, SchemaGuidColumn fkColumn, SchemaStringColumn codeColumn)
		{
			var main = new ZDBOnlyQuery(typeof(GlbStaff));
			var comparisonOperator = GetSQLComparisonOperator(comparison.ComparisonOperator);

			var isNotIn = false;
			if (comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				isNotIn = true;
				comparisonOperator = SQLComparisonOperator.Equal;
			}
			else if (comparisonOperator == SQLComparisonOperator.NotContains)
			{
				isNotIn = true;
				comparisonOperator = SQLComparisonOperator.Contains;
			}
			else if (comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				isNotIn = true;
				comparisonOperator = SQLComparisonOperator.StartsWith;
			}

			var subSubQuery = new ZDBOnlySubQuery(subType, fkColumn, isNotIn);

			subSubQuery.AddToFilter(codeColumn, comparisonOperator, comparison.Value);

			main.AddSubQuery(subSubQuery, JoinCondition.And);
			if (isNotIn)
			{
				main.AddToFilter(JoinCondition.Or, fkColumn, null);
			}

			sub.AddToFilter(main, joinCondition);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		bool ShouldSkipFilter(string expressionName, string value, string parent)
		{
			if (string.IsNullOrEmpty(parent))
			{
				return false;
			}

			if (!expressionName.ToLower().Equals("type"))
			{
				return false;
			}

			if (!value.ToLower().Equals("work"))
			{
				return false;
			}

			return true;
		}

		object GetValueForColumn(SchemaColumn column, string value)
		{
			if (column is SchemaGuidColumn)
			{
				return new ZGuid(value);
			}
			else if (column is SchemaDateColumn || column is SchemaDateTimeColumn)
			{
				if (ZDateTime.TryParseISO8601Date(value, out ZDateTime datetime))
				{
					return datetime;
				}
				else
				{
					if (ZDateTimeOffset.TryParse(value, out ZDateTimeOffset datetimeOffset))
					{
						return datetimeOffset.ToLocalZDateTime();
					}
				}

				return ZDateTime.Empty;
			}

			return value;
		}

		SQLComparisonOperator GetSQLComparisonOperator(SCIMComparisonOperators operators)
		{
			switch (operators)
			{
				case SCIMComparisonOperators.EQ:
					return SQLComparisonOperator.Equal;
				case SCIMComparisonOperators.NE:
					return SQLComparisonOperator.NotEqual;
				case SCIMComparisonOperators.LT:
					return SQLComparisonOperator.LessThan;
				case SCIMComparisonOperators.GE:
					return SQLComparisonOperator.GreaterThanOrEqualTo;
				case SCIMComparisonOperators.GT:
					return SQLComparisonOperator.GreaterThan;
				case SCIMComparisonOperators.LE:
					return SQLComparisonOperator.LessThanOrEqualTo;
				case SCIMComparisonOperators.SW:
					return SQLComparisonOperator.StartsWith;
				case SCIMComparisonOperators.EW:
					return SQLComparisonOperator.EndsWith;
				case SCIMComparisonOperators.CO:
					return SQLComparisonOperator.Contains;
				default:
					throw new SCIMFilterException($"Unknown operator: [{operators}].");
			}
		}

		JoinCondition GetJoinCondition(SCIMLogicalOperators operators)
		{
			switch (operators)
			{
				case SCIMLogicalOperators.AND:
					return JoinCondition.And;
				case SCIMLogicalOperators.OR:
					return JoinCondition.Or;
				default:
					throw new SCIMFilterException($"Unknown logical operator: [{operators}].");
			}
		}

		enum QueryType
		{
			Staff,
			Group
		}
	}
}
