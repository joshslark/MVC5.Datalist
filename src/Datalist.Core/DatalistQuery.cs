using System;
using System.Linq;
using System.Linq.Expressions;

namespace Datalist
{
    public static class DatalistQuery
    {
        public static Boolean IsOrdered(IQueryable models)
        {
            DatalistExpressionVisitor expression = new DatalistExpressionVisitor();
            expression.Visit(models.Expression);

            return expression.Ordered;
        }

        private class DatalistExpressionVisitor : ExpressionVisitor
        {
            public Boolean Ordered { get; private set; }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType != typeof(Queryable))
                    return base.VisitMethodCall(node);

                if (!node.Method.Name.StartsWith("OrderBy"))
                    return base.VisitMethodCall(node);

                Ordered = true;

                return node;
            }
        }
    }
}
