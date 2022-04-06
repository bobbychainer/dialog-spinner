#define DISALLOW_NULL_EQUATION_TERMS


namespace TypeChecker
{

    /// <summary>
    /// Stores information that a Solver can use to solve a system of type
    /// equations.
    /// </summary>
    public abstract class TypeConstraint
    {
        /// <inheritdoc/>
        new public abstract string ToString();

        /// <summary>
        /// Simplifies this constraint, producing either a <see
        /// cref="TypeEqualityConstraint"/> or a <see
        /// cref="DisjunctionConstraint"/>.
        /// </summary>
        /// <remarks>
        /// If this method returns null, it represents that the constraint has
        /// determined that it is a tautology (e.g. 'T0 == T0'), and does not need
        /// further evaluation.
        /// </remarks>
        /// <param name="subst">A <see cref="Substitution"/> that the constraint can
        /// use to help decide how to simplify.</param>
        /// <returns>A TypeEqualityConstraint or a DisjunctionConstraint that
        /// represents a simplified version of this constraint, or null.</returns>
        public abstract TypeConstraint Simplify(Substitution subst);
    }
}
