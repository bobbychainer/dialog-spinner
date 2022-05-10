namespace Yarn.Compiler
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a range of text in a multi-line string.
    /// </summary>
    [System.Serializable]
    public class Range
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> class, given
        /// start and end information.
        /// </summary>
        /// <param name="startLine">The zero-indexed line number of the start of
        /// the range.</param>
        /// <param name="startCharacter">The zero-indexed character number of
        /// the start of the range.</param>
        /// <param name="endLine">The zero-indexed line number of the end of the
        /// range.</param>
        /// <param name="endCharacter">The zero-indexed character number of the
        /// end of the range.</param>
        public Range(int startLine, int startCharacter, int endLine, int endCharacter)
        {
            this.Start = new Position { Line = startLine, Character = startCharacter };
            this.End = new Position { Line = endLine, Character = endCharacter };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> class.
        /// </summary>
        /// <remarks>
        /// The <see cref="Start"/> and <see cref="End"/> positions will both be
        /// set to have a line and character index of -1.
        /// </remarks>
        public Range()
        {
            this.Start = new Position();
            this.End = new Position();
        }

        /// <summary>
        /// Gets or sets the start position of this range.
        /// </summary>
        public Position Start { get; set; } = new Position();

        /// <summary>
        /// Gets or sets the end position of this range.
        /// </summary>
        public Position End { get; set; } = new Position();

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Range range &&
                   EqualityComparer<Position>.Default.Equals(this.Start, range.Start) &&
                   EqualityComparer<Position>.Default.Equals(this.End, range.End);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1676728671;
            hashCode = (hashCode * -1521134295) + EqualityComparer<Position>.Default.GetHashCode(this.Start);
            hashCode = (hashCode * -1521134295) + EqualityComparer<Position>.Default.GetHashCode(this.End);
            return hashCode;
        }
    }

    /// <summary>
    /// Represents a position in a multi-line string.
    /// </summary>
    [System.Serializable]
    public class Position
    {
        /// <summary>
        /// Gets or sets the zero-indexed line of this position.
        /// </summary>
        public int Line { get; set; } = -1;

        /// <summary>
        /// Gets or sets the zero-indexed character number of this position.
        /// </summary>
        public int Character { get; set; } = -1;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Position position &&
                   this.Line == position.Line &&
                   this.Character == position.Character;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1927683087;
            hashCode = (hashCode * -1521134295) + this.Line.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.Character.GetHashCode();
            return hashCode;
        }
    }

    public class DeferredTypeDiagnostic
    {
        public string Name { get; internal set; }
        public Diagnostic diagnostic { get; set; }
        public static DeferredTypeDiagnostic CreateDeferredTypeDiagnostic(string name, Diagnostic diagnostic)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            return new DeferredTypeDiagnostic
            {
                Name = name,
                diagnostic = diagnostic,
            };
        }
    }
    
    [Serializable]
    public class Declaration
    {
        /// <summary>
        /// Gets the name of this Declaration.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Declaration"/> class,
        /// using the given name, type and default value.
        /// </summary>
        /// <param name="name">The name of the new declaration.</param>
        /// <param name="type">The type of the declaration.</param>
        /// <param name="defaultValue">The default value of the
        /// declaration. This must be a string, a number (integer or
        /// floating-point), or boolean value.</param>
        /// <param name="description">The description of the new
        /// declaration.</param>
        /// <returns>A new instance of the <see cref="Declaration"/>
        /// class.</returns>
        public static Declaration CreateVariable(string name, TypeChecker.IIType type, IConvertible defaultValue, string description = null)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            if (defaultValue is null)
            {
                throw new ArgumentNullException(nameof(defaultValue));
            }

            // We're all good to create the new declaration.
            return new Declaration
            {
                Name = name,
                DefaultValue = defaultValue,
                Type = type,
                Description = description,
            };
        }

        /// <summary>
        /// Gets the default value of this <see cref="Declaration"/>, if no
        /// value has been specified in code or is available from a <see
        /// cref="Dialogue"/>'s <see cref="IVariableStorage"/>.
        /// </summary>
        public IConvertible DefaultValue { get; internal set; }

        /// <summary>
        /// Gets a string describing the purpose of this <see
        /// cref="Declaration"/>.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the name of the file in which this Declaration was found.
        /// </summary>
        /// <remarks>
        /// If this <see cref="Declaration"/> was not found in a Yarn
        /// source file, this will be <see cref="ExternalDeclaration"/>.
        /// </remarks>
        public string SourceFileName { get; internal set; }

        /// <summary>
        /// Gets the name of the node in which this Declaration was found.
        /// </summary>
        /// <remarks>
        /// If this <see cref="Declaration"/> was not found in a Yarn
        /// source file, this will be <see langword="null"/>.
        /// </remarks>
        public string SourceNodeName { get; internal set; }

        /// <summary>
        /// Gets the line number at which this Declaration was found in the
        /// source file.
        /// </summary>
        /// <remarks>
        /// If this <see cref="Declaration"/> was not found in a Yarn
        /// source file, this will be -1.
        /// </remarks>
        public int SourceFileLine => this.Range.Start.Line;

        /// <summary>
        /// Gets a value indicating whether get or sets a value indicating
        /// whether this Declaration was implicitly inferred from usage.
        /// </summary>
        /// <value>If <see langword="true"/>, this Declaration was implicitly
        /// inferred from usage. If <see langword="false"/>, this Declaration
        /// appears in the source code.</value>
        public bool IsImplicit { get; internal set; }

        /// <summary>
        /// Gets the type of the variable, as represented by an object that
        /// implements <see cref="IType"/>.
        /// </summary>
        public IType Type { get; internal set; }

        /// <summary>
        /// The string used for <see cref="SourceFileName"/> if the
        /// Declaration was found outside of a Yarn source file.
        /// </summary>
        public const string ExternalDeclaration = "(External)";

        /// <summary>
        /// Gets the range of text at which this declaration occurs.
        /// </summary>
        /// <remarks>
        /// This range refers to the declaration of the symbol itself, and not
        /// any syntax surrounding it. For example, the declaration
        /// <c>&lt;&lt;declare $x = 1&gt;&gt;</c> would have a Range referring
        /// to the <c>$x</c> symbol.
        /// </remarks>
        public Range Range { get; internal set; } = new Range();

        /// <inheritdoc/>
        public override string ToString()
        {
            string result = $"{this.Name} : {this.Type} = {this.DefaultValue}";

            if (string.IsNullOrEmpty(this.Description))
            {
                return result;
            }
            else
            {
                return result + $" (\"{this.Description}\")";
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Declaration otherDecl))
            {
                return false;
            }

            return this.Name == otherDecl.Name &&
                this.Type == otherDecl.Type &&
                this.DefaultValue == otherDecl.DefaultValue &&
                this.Description == otherDecl.Description;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            
            return this.Name.GetHashCode()
                ^ this.Type.GetHashCode()
                ^ this.DefaultValue.GetHashCode()
                ^ (this.Description ?? string.Empty).GetHashCode();
        }

    }
}
