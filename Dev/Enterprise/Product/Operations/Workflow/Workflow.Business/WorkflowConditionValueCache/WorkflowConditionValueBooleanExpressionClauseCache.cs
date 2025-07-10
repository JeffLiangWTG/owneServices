using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Macros;
using CargoWise.Types;

namespace Enterprise.Workflow.Business
{
	class WorkflowConditionValueBooleanExpressionClauseCache : IWorkflowConditionValueBooleanExpressionClauseCache
	{
		readonly Dictionary<(Guid Identifier, ZString Condition), IMacroBooleanExpressionClause> booleanExpressionClauseMap = new Dictionary<(Guid, ZString), IMacroBooleanExpressionClause>();

		readonly ReaderWriterLockSlim splitConditionsReaderWriterLock = new ReaderWriterLockSlim();

		public IMacroBooleanExpressionClause GetBooleanExpressionClauseCached(Guid identifier, ZString condition, Func<ZString, IMacroBooleanExpressionClause> splitFunction)
		{
			if (condition.IsEmpty)
			{
				return new EmptyConditionClause();
			}

			var cacheKey = (Identifier: identifier, Condition: condition);

			try
			{
				splitConditionsReaderWriterLock.EnterReadLock();
				if (!booleanExpressionClauseMap.TryGetValue(cacheKey, out var result))
				{
					splitConditionsReaderWriterLock.ExitReadLock();
					splitConditionsReaderWriterLock.EnterWriteLock();
					if (!booleanExpressionClauseMap.TryGetValue(cacheKey, out result))
					{
						booleanExpressionClauseMap.Add(cacheKey, result = splitFunction(condition));
					}
				}

				return result;
			}
			finally
			{
				if (splitConditionsReaderWriterLock.IsReadLockHeld)
				{
					splitConditionsReaderWriterLock.ExitReadLock();
				}
				else if (splitConditionsReaderWriterLock.IsWriteLockHeld)
				{
					splitConditionsReaderWriterLock.ExitWriteLock();
				}
			}
		}
	}
}
