#define DISALLOW_NULL_EQUATION_TERMS

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TypeChecker
{
    /// <summary>
    /// A conjunction constraint is a logical 'and' that resolves if all of its
    /// child constraints resolve.
    /// </summary>
    internal class ConjunctionConstraint : TypeConstraint, IEnumerable<TypeConstraint>
    {
        public IEnumerable<TypeConstraint> Constraints { get; private set; }

        /// <inheritdoc/>
        public override IEnumerable<TypeVariable> AllVariables => Constraints.SelectMany(c => c.AllVariables).Distinct();

        public ConjunctionConstraint(params TypeConstraint[] constraints)
        {
            Constraints = constraints;
        }

        public ConjunctionConstraint(IEnumerable<TypeConstraint> constraints)
        {
            if (constraints.Count() == 0) {
                throw new System.ArgumentException($"{nameof(ConjunctionConstraint)} received no terms");
            }
            Constraints = constraints;
        }

        public override IEnumerable<string> GetFailureMessages(Substitution subst)
        {
            return this.Constraints.SelectMany(c => c.GetFailureMessages(subst));
        }


        public override string ToString() => string.Join(" ∧ ", Constraints.Select(t => $"({t.ToString()})"));

        public IEnumerator<TypeConstraint> GetEnumerator()
        {
            return Constraints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Constraints).GetEnumerator();
        }

        public override TypeConstraint Simplify(Substitution subst, IEnumerable<Yarn.TypeBase> knownTypes)
        {
            if (this.IsTautological) {
                // If we are a tautology, simplify to true
                return new TrueConstraint(this);
            } else {
                if (this.Children.Any(c => c is FalseConstraint)) {
                    // If any of our children are known to be false, then this
                    // constraint simplifies to false
                    return new FalseConstraint(this);
                }
            }
            var conjunctionConstraint = new ConjunctionConstraint(
                Constraints.Distinct()
                           .Select(c => c.Simplify(subst, knownTypes))
                           .Where(t => t.GetType() != typeof(TrueConstraint)).ToList()
                           );

            conjunctionConstraint.SourceExpression = this.SourceExpression;
            conjunctionConstraint.SourceContext = this.SourceContext;
            conjunctionConstraint.SourceFileName = this.SourceFileName;
            conjunctionConstraint.SourceRange = this.SourceRange;
            return conjunctionConstraint;
        }

        public override IEnumerable<TypeConstraint> DescendantsAndSelf => Constraints.SelectMany(c => c.DescendantsAndSelf).Prepend(this);

        public override IEnumerable<TypeConstraint> Children => Constraints;

        // A conjunction is tautological if it it has children and they are all
        // tautological
        public override bool IsTautological => Children.Any() && Children.All(c => c.IsTautological);
    }
}
