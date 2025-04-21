using System;
using System.Linq.Expressions;


/// <summary>
/// borrowd from 
/// https://enterprisecraftsmanship.com/posts/specification-pattern-c-implementation/
/// </summary>
namespace Quantum.Specification
{
    public abstract class Specification<T>
    {
        public abstract Expression<Func<T, bool>> Condition();

        public bool IsItVerify(T userAccessRightStatus)
        {
            var predicate = Condition().Compile();
            return predicate(userAccessRightStatus);
        }

        public Specification<T> And(Specification<T> specification)
            => new AndSpecification<T>(this, specification);

        public Specification<T> Or(Specification<T> specification)
            => new OrSpecification<T>(this, specification);

        public class AndSpecification<T> : Specification<T>
        {
            private readonly Specification<T> _left;
            private readonly Specification<T> _right;

            public AndSpecification(Specification<T> left, Specification<T> right)
            {
                _right = right;
                _left = left;
            }

            public override Expression<Func<T, bool>> Condition()
            {
                Expression<Func<T, bool>> leftExpression = _left.Condition();
                Expression<Func<T, bool>> rightExpression = _right.Condition();


                var param = Expression.Parameter(typeof(T), "x");
                var body = Expression.AndAlso(
                        Expression.Invoke(leftExpression, param),
                        Expression.Invoke(rightExpression, param)
                    );
                var lambda = Expression.Lambda<Func<T, bool>>(body, param);
                return lambda;

                var paramExpr = Expression.Parameter(typeof(T));
                var exprBody = Expression.AndAlso(leftExpression.Body, rightExpression.Body);

                //return Expression.Lambda<Func<T, bool>>(exprBody,Expression.Parameter(typeof(T)));


                //Expression<Func<T, bool>> result = 
                //    Expression.<Func<T, bool>>(Expression.Or(leftExpression.Body, rightExpression.Body), 
                //                        leftExpression.Parameters);

                //return result;


                var parameterReplacer = new ParameterReplacer(paramExpr);

                exprBody = (BinaryExpression)parameterReplacer.Visit(exprBody);
                var finalExpr = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);

                return finalExpr;
            }
        }

        public class OrSpecification<T> : Specification<T>
        {
            private readonly Specification<T> _left;
            private readonly Specification<T> _right;

            public OrSpecification(Specification<T> left, Specification<T> right)
            {
                _right = right;
                _left = left;
            }

            public override Expression<Func<T, bool>> Condition()
            {
                var leftExpression = _left.Condition();
                var rightExpression = _right.Condition();
                var paramExpr = Expression.Parameter(typeof(T));

                var exprBody = Expression
                    .OrElse(leftExpression.Body, rightExpression.Body);

                exprBody = (BinaryExpression)new ParameterReplacer(paramExpr).Visit(exprBody);

                var finalExpr = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);

                return finalExpr;
            }
        }
    }
}