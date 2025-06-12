using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Hawking.RuleEngine
{
	public static class Criterion
	{
		public class Condition : ICriterion
		{
			Delegate CompiledExpression;
			MethodInfo ExpressionInvoker;

			public string Expression { get; set; }

			public bool Evaluate(FactsProvider factsProvider, ILogger logger)
			{
                logger.LogDebug("Condition Start. Expression = '{0}'", Expression);

				if (CompiledExpression == null)
				{
					lock (this)
					{
						if (CompiledExpression == null)
						{
							CompiledExpression = DynamicExpression.ParseLambda(factsProvider.Facts.GetType(), typeof(bool), Expression).Compile();
							ExpressionInvoker = CompiledExpression.GetType().GetMethod("Invoke");
						}
					}
				}

                bool result = (bool)ExpressionInvoker.Invoke(CompiledExpression, new object[] { factsProvider.Facts });

                logger.LogDebug("Condition End. Result = {0}", result);

                return result;
			}
		}

		public class CompoundCondition : Collection<ICriterion>, ICriterion
		{
			public new IList<ICriterion> Items => base.Items;
		    public LogicalOperator LogicalOperation { get; set; }

			public bool Evaluate(FactsProvider factsProvider, ILogger logger)
			{
                logger.LogDebug("Compound Condition Start. Logical Operation = {0}", LogicalOperation);
                bool result = false;

				switch (LogicalOperation)
				{
					case LogicalOperator.Or:
						result = Items.Any(i => i.Evaluate(factsProvider, logger));
                        break;
					case LogicalOperator.And:
						result = Items.All(i => i.Evaluate(factsProvider, logger));
                        break;
				}

                logger.LogDebug("Compound Condition End. Result = {0}", result);
                return result;
			}

			public enum LogicalOperator
			{
				Or,
				And
			}
		}

		public class ExternalMethod : ICriterion
		{
			public string ExternalType { get; set; }
			public string Method { get; set; }
			public List<Value> ParameterValues { get; set; }

			MethodInfo externalMethod;
			object externalInstance;

			public ExternalMethod()
			{
				ParameterValues = new List<Value>();
			}

			public bool Evaluate(FactsProvider factsProvider, ILogger logger)
			{
                logger.LogDebug("External Method Condition Start. Method = " + Method);

				var parameters = ParameterValues.Select(p => p.Evaluate(factsProvider).Contents).ToArray();

				if (externalMethod == null)
				{
					lock (this)
					{
						if (externalMethod == null)
						{
							var type = Type.GetType(ExternalType);
							externalMethod = type.GetMethod(Method, parameters.Select(p => p.GetType()).ToArray());
							externalInstance = externalMethod.IsStatic ? null : Activator.CreateInstance(type);
						}
					}
				}

                bool result = (bool)externalMethod.Invoke(externalInstance, parameters);


                logger.LogDebug("External Method Condition End. Result = {0}", result);
                return result;
			}
		}
	}
}
