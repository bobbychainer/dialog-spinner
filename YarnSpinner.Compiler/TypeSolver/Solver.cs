// #define VERBOSE_SOLVER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yarn;

namespace TypeChecker
{
    /// <summary>
    /// Contains methods for solving systems of type equations.
    /// </summary>
    /// <remarks>
    /// This class contains the central algorithms used for solving the system
    /// of type equations produced by <see
    /// cref="Yarn.Compiler.TypeCheckerListener"/>.
    /// </remarks>
    internal static class Solver
    {
        [Conditional("VERBOSE_SOLVER")]
        private static void VerboseLog(int depth, string message)
        {
            System.Console.Write(new string('.', depth));
            System.Console.Write(" ");
            System.Console.WriteLine(message);
        }
        /// <summary>
        /// Incrementally solves a collection of type constraints, and produces
        /// a substitution.
        /// </summary>
        /// <param name="typeConstraints">The collection of type constraints to
        /// solve.</param>
        /// <param name="knownTypes">The list of types known to the
        /// solver.</param>
        /// <param name="diagnostics">The list of diagnostics. If this method
        /// returns false, the list will be added to.</param>
        /// <param name="substitution">The Substitution containing a partial
        /// solution to the overall type problem. If this method returns true,
        /// <paramref name="substitution"/> is updated with the solution;
        /// otherwise, <paramref name="substitution"/> is unmodified.</param>
        /// <returns><see langword="true"/> if the type constraints could be
        /// solved; <see langword="false"/> otherwise.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when
        /// <paramref name="typeConstraints"/> contains a type of constraint
        /// that could not be handled.</exception>
        internal static bool TrySolve(IEnumerable<TypeConstraint> typeConstraints, IEnumerable<TypeBase> knownTypes, List<Yarn.Compiler.Diagnostic> diagnostics, ref Substitution substitution)
        {

            substitution ??= new Substitution();

            if (typeConstraints.Count() == 0)
            {
                // Nothing to do.
                return true;
            }

            // Take our collection of type constraints, and simplify them.
            // Produce a conjunction of the result.
            var formula = new ConjunctionConstraint(new HashSet<TypeConstraint>(typeConstraints.WithoutTautologies())).Simplify(substitution, knownTypes);

            // Next, convert this conjunction into disjunctive normal form (i.e.
            // an 'OR of ANDs'). If any term in this disjunction resolves, then
            // that's our solution for the provided constraints.
            var disjunction = new DisjunctionConstraint(ToDisjunctiveNormalForm(formula).Simplify(substitution, knownTypes));

            // The collection of possible solutions that could have for this
            // type problem, given typeConstraints. If we have more than one,
            // pick the most specific one. If we have more than one equally
            // specific one, then the type solution is ambiguous, and that's a
            // compile error.
            List<Substitution> successfulSubstitutions = new List<Substitution>();

            foreach (var candidate in disjunction)
            {
                VerboseLog(0, $"Trying {candidate}");

                // Start by cloning the current solution - we'll update it as we
                // go.
                var subst = substitution.Clone();

                // At least one of the terms in the disjunction must be true in
                // order to find a solution.

                if (candidate is TrueConstraint)
                {
                    // The term is true by definition.
                    successfulSubstitutions.Add(subst);
                }
                else if (candidate is FalseConstraint)
                {
                    // The term is false by definition.
                    continue;
                }
                else if (!(candidate is ConjunctionConstraint conjunction))
                {
                    throw new InvalidOperationException($"Internal error: Didn't expect to see a {candidate.GetType()}: {candidate.ToString()}");
                }
                else
                {
                    bool isFailed = false;

                    // Evaluate each constraint in the clause.
                    foreach (var subterm in conjunction)
                    {
                        VerboseLog(1, $"Solving {subterm.ToString()}");
                        if (subterm is TypeEqualityConstraint equalityConstraint)
                        {
                            // Attempt to unify this equality with our current
                            // substitution.
                            if (!TryUnify(equalityConstraint.Left, equalityConstraint.Right, subst))
                            {
                                // Unification failed, so this solution doesn't
                                // work.
                                isFailed = true;
                                break;
                            }
                        }
                        else if (subterm is TrueConstraint)
                        {
                            // True constraints don't affect our solution.
                        }
                        else if (subterm is FalseConstraint)
                        {
                            // False constraints represent an immediate failure.
                            isFailed = true;
                            break;
                        }
                        else
                        {
                            // We didn't expect to see any other kind of
                            // constraint.
                            throw new InvalidOperationException($"Internal error: Didn't expect to see a {candidate.GetType()}: {candidate.ToString()}");
                        }
                    }

                    if (!isFailed)
                    {
                        // This solution works! Add this to the list of
                        // solutions.
                        successfulSubstitutions.Add(subst);
                    }
                }
            }

            if (successfulSubstitutions.Count == 1)
            {
                // We have precisely one successful solution! This must be it.
                substitution = successfulSubstitutions.Single();
                return true;
            }
            else if (successfulSubstitutions.Count > 1)
            {
                // Attempt to merge the solutions into the most specific one.

                if (TryMergeSubsitutions(successfulSubstitutions, out substitution))
                {
                    return true;
                }
                else
                {
                    // We have most than one successful substitution. The
                    // resolved type is ambiguous!
                    foreach (var constraint in typeConstraints)
                    {
                        diagnostics.Add(new Yarn.Compiler.Diagnostic(constraint.SourceFileName, constraint.SourceRange, "The type of this expression is ambiguous."));
                    }
                    return false;
                }
            }
            else
            {
                // Nothing we tried worked! 
                return false;
            }

        }

        private static bool TryMergeSubsitutions(IEnumerable<Substitution> substitutions, out Substitution result)
        {
            // Merge multiple dicts 
            var mergedDictionary = new Dictionary<TypeVariable, HashSet<IType>>();

            foreach (var subst in substitutions)
            {
                foreach (var kv in subst)
                {
                    if (!mergedDictionary.TryGetValue(kv.Key, out HashSet<IType> list))
                    {
                        list = new HashSet<IType>();
                        mergedDictionary[kv.Key] = list;
                    }

                    list.Add(kv.Value);
                }
            }

            result = new Substitution();

            foreach (var kv in mergedDictionary)
            {
                if (kv.Value.Count == 0)
                {
                    // No substitutions for this type variable?? We don't expect
                    // this to happen.
                    throw new InvalidOperationException($"Expected {kv.Key} to have at least one substitution");
                }
                else if (kv.Value.Count == 1)
                {
                    // Precisely one substitution for this type variable. Use
                    // it.
                    result[kv.Key] = kv.Value.Single();
                }
                else
                {
                    // Multiple possible solutions for this type variable. Pick
                    // the most specific one. If there are multiple solutions
                    // that are equally specific, then the type solution is
                    // ambiguous and we fail.
                    var bestCandidates = kv.Value.GroupBy(candidate =>
                    {
                        if (candidate is TypeBase type)
                        {
                            return type.TypeDepth;
                        }
                        else
                        {
                            return int.MaxValue;
                        }
                    }).OrderByDescending(group => group.Key).First();

                    if (bestCandidates.Count() == 1)
                    {
                        result[kv.Key] = bestCandidates.Single();
                    }
                    else
                    {
                        // More than one option is the best candidate, so the
                        // solution is ambiguous!
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to unify <paramref name="x"/> with <paramref name="y"/>,
        /// updating <paramref name="subst"/>.
        /// </summary>
        /// <remarks>
        /// <para>This method modifies <paramref name="subst"/> in place.</para>
        /// <para>
        /// If unifying <paramref name="x"/> with <paramref name="y"/> fails,
        /// subst is returned unmodified.
        /// </para>
        /// </remarks>
        /// <param name="x">The first term to unify.</param>
        /// <param name="y">The second term to unify.</param>
        /// <param name="subst">The <see cref="Substitution"/> to use and
        /// update.</param>
        /// <returns>True if the unification worked; false otherwise.</returns>
        internal static bool TryUnify(IType x, IType y, Substitution subst)
        {
            if (x.Equals(y))
            {
                // If the two terms are already identical, no unification is
                // necessary. 
                return true;
            }

            if (x is TypeVariable varX)
            {
                // If x is a variable, unify it with y.
                return UnifyVariable(varX, y, subst);
            }

            if (y is TypeVariable varY)
            {
                // Otherwise, if y is a variable, unify it with x.
                return UnifyVariable(varY, x, subst);
            }

            if (x is FunctionType xFunc && y is FunctionType yFunc)
            {
                // If they're both function applications, attempt to unify them.

                // We cannot unify two function applications if they don't have
                // the same number of parameters.
                if (xFunc.Parameters.Count() != yFunc.Parameters.Count())
                {
                    return false;
                }

                // For each argument in the function applications, unify them.
                for (int i = 0; i < xFunc.Parameters.Count(); i++)
                {
                    if (TryUnify(xFunc.Parameters.ElementAt(i), yFunc.Parameters.ElementAt(i), subst) == false)
                    {
                        return false;
                    }
                }

                // Unify the return types, too.
                return TryUnify(xFunc.ReturnType, yFunc.ReturnType, subst);

                // All done.
            }
            return false;
        }

        /// <summary>
        /// Unifies a variable with a term, by creating a substitution from the
        /// variable to the value represented by the term, taking into account
        /// the existing subsitution.
        /// </summary>
        /// <param name="var">The variable to unify.</param>
        /// <param name="term">The term to unify.</param>
        /// <param name="subst">The current substitution.</param>
        /// <returns>The updated substitution.</returns>
        private static bool UnifyVariable(TypeVariable var, IType term, Substitution subst)
        {
            // If we already have a unifier for var, then unify term with it.
            if (subst.ContainsKey(var))
            {
                return TryUnify(subst[var], term, subst);
            }

            // If term is a variable, and we have a unifier for it, then unify
            // var with whatever we've already unified term with.
            if (term is TypeVariable xVar && subst.ContainsKey(xVar))
            {
                return TryUnify(var, subst[xVar], subst);
            }

            // If term contains var in it, we cannot unify it, because that
            // would result in a cycle.
            if (OccursCheck(var, term, subst))
            {
                return false;
            }

            // var is not yet in subst, and we can't simplify term. Extend
            // 'subst'.
            subst.Add(var, term);

            return true;
        }

        /// <summary>
        /// Recursively determines if <paramref name="var"/> exists as a
        /// sub-term in <paramref name="term"/>, taking into account any
        /// existing substitutions in <paramref name="subst"/>.
        /// </summary>
        /// <remarks>
        /// This function is used to prevent the creation of cyclical
        /// substitutions (i.e. <c>X: Y, Y: X</c>).
        /// </remarks>
        /// <param name="var">The variable to test for use inside <paramref
        /// name="term"/>.</param>
        /// <param name="term">The term to test to see if <paramref name="var"/>
        /// is used inside it.</param>
        /// <param name="subst">The current substitution.</param>
        /// <returns><see langword="true"/> if <paramref name="var"/> exists in
        /// <paramref name="term"/>, <see langword="false"/>
        /// otherwise.</returns>
        private static bool OccursCheck(TypeVariable var, IType term, Substitution subst)
        {
            // Does the variable 'var' occur anywhere inside 'term'?

            // Variables in 'term' are looked up in 'subst' and the check is
            // applied recursively.

            if (var.Equals(term))
            {
                // If var is term, then var exists in term by definition.
                return true;
            }
            else if (term is TypeVariable termVar && subst.ContainsKey(termVar))
            {
                // If term is itself a variable, and we already have a
                // substitution for it, then check var against whatever we're
                // substituting term for.
                return OccursCheck(var, subst[termVar], subst);
            }
            else if (term is FunctionType app)
            {
                // If term is a function application, then check var against
                // each of the function's arguments, and its return type.
                foreach (var arg in app.Parameters)
                {
                    if (OccursCheck(var, arg, subst))
                    {
                        return true;
                    }
                }
                return OccursCheck(var, app.ReturnType, subst);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a constraint into disjunctive normal form (that is, an 'OR
        /// of ANDs'.)
        /// </summary>
        /// <remarks>
        /// Converting a conjunction containing disjunctions produces an
        /// exponential number of new constraints, which can result in extremely
        /// large memory and time cost. Accordingly, <paramref name="input"/>
        /// should be kept as simple as possible.
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DisjunctionConstraint ToDisjunctiveNormalForm(TypeConstraint input)
        {
            IEnumerable<TypeConstraint> ExpandDisjunctions(TypeConstraint constraint)
            {
                // Recursively expand any disjunctions in our children.
                var expandedProducts = constraint.Children.Select(a => ExpandDisjunctions(a)).CartesianProduct().ToList();

                // If this is itself a disjunction, the terms we're returning
                // are the arguments itself.
                if (constraint is DisjunctionConstraint)
                {
                    return expandedProducts.SelectMany(a => a).Distinct();
                }
                else if (constraint is ConjunctionConstraint)
                {
                    // Otherwise, for every combination of our arguments, return
                    // a new compound with that combination of arguments.
                    return expandedProducts.Select(product => new ConjunctionConstraint(product.SelectMany(p => ExpandConjunctions(p)).ToArray()));
                }
                else
                {
                    return new[] { constraint };
                }
            }

            IEnumerable<TypeConstraint> ExpandConjunctions(TypeConstraint constraint)
            {
                if (constraint is ConjunctionConstraint conjunction)
                {
                    // Recursively expand any child conjunctions
                    return conjunction.Children.SelectMany(arg => ExpandConjunctions(arg)).ToList();
                }
                else
                {
                    // Otherwise, this is not an 'and', so just return this.
                    return new[] { constraint };
                }
            }

            return new DisjunctionConstraint(ExpandDisjunctions(input));
        }
    }
}
