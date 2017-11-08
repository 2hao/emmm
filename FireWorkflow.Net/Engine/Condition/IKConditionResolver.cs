using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireWorkflow.Net.Engine.Condition
{
    public class IKConditionResolver : IConditionResolver
    {
        public bool resolveBooleanExpression(Dictionary<string, object> vars, string elExpression)
        {
            List<Variable> variables = new List<Variable>();
            String[] keys = vars.keySet().toArray(new String[vars.size()]);
            for (int i = 0; keys != null && i < keys.length; i++)
            {
                String key = keys[i];
                variables.add(Variable.createVariable(key, vars.get(key)));
            }

            Object result = ExpressionEvaluator.evaluate(elExpression, variables);

            Boolean b = (Boolean)result;
            return b.booleanValue();
        }
    }
}
