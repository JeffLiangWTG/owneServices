using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Hawking.RuleEngine
{
    public class Decision : IEvaluator
    {
        public Decision(string expression, dynamic success, dynamic fail)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Success = success ?? throw new ArgumentNullException(nameof(success));
            Fail = fail ?? throw new ArgumentNullException(nameof(fail));
        }

        public string Expression { get; }
        public dynamic Success { get; }
        public dynamic Fail { get; }

        public ICollection<dynamic> Evaluate( facts)
        {
            if (CompiledExpression == null)
            {
                lock (this)
                {
                    if (CompiledExpression == null)
                    {
                        CompiledExpression = DynamicExpression.ParseLambda(facts.GetType(), typeof(bool), Expression).Compile();
                        ExpressionInvoker = CompiledExpression.GetType().GetMethod("Invoke");
                    }
                }
            }

            var result = (bool)ExpressionInvoker.Invoke(CompiledExpression, new object[] {facts});

            return new[] { result ? Success : Fail };
        }

        private MethodInfo ExpressionInvoker { get; set; }
        private object CompiledExpression { get; set; }
    }
}
