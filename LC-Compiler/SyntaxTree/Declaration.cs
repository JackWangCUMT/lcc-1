﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcc.TypeSystem;

namespace lcc.SyntaxTree {

    public sealed class Declaration : Stmt, IEquatable<Declaration> {

        public Declaration(
            DeclSpecs specifiers,
            IEnumerable<STInitDeclarator> declarators
            ) {
            this.specifiers = specifiers;
            this.declarators = declarators;
        }

        /// <summary>
        /// Check if this is a typedef declaration.
        /// </summary>
        /// <returns></returns>
        public bool IsTypedef => specifiers.storage == STStoreSpec.Kind.TYPEDEF;

        /// <summary>
        /// Return the names of the all the direct declarators.
        /// </summary>
        public IEnumerable<string> DeclNames => from declarator in declarators select declarator.declarator.direct.Name;

        public override Position Pos => specifiers.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as Declaration);
        }

        public bool Equals(Declaration x) {
            return x != null && x.specifiers.Equals(specifiers)
                && x.declarators.SequenceEqual(declarators);
        }

        public override int GetHashCode() {
            return specifiers.GetHashCode();
        }

        public readonly DeclSpecs specifiers;
        public readonly IEnumerable<STInitDeclarator> declarators;
        
        public AST.Node ToAst(Env env) {

            T type = specifiers.GetT(env);

        }
    }

    public sealed class DeclSpecs : Node, IEquatable<DeclSpecs> {

        public DeclSpecs(
            IEnumerable<DeclSpec> all,
            STStoreSpec.Kind storage,
            IEnumerable<TypeSpec.Kind> keys,
            STFuncSpec.Kind function = STFuncSpec.Kind.NONE
            ) {
            this.storage = storage;
            this.keys = keys;
            this.function = function;
            qualifiers = GetQualifiers(from s in all.OfType<TypeQual>() select s.kind);
            pos = all.First().Pos;
        }

        public DeclSpecs(
            IEnumerable<DeclSpec> all,
            STStoreSpec.Kind storage,
            TypeUserSpec specifier,
            STFuncSpec.Kind function = STFuncSpec.Kind.NONE
            ) {
            this.storage = storage;
            this.function = function;
            this.specifier = specifier;
            qualifiers = GetQualifiers(from s in all.OfType<TypeQual>() select s.kind);
            pos = all.First().Pos;
        }

        public bool Equals(DeclSpecs x) {
            return x != null && x.pos.Equals(pos)
                && x.storage == storage
                && x.qualifiers.Equals(qualifiers)
                && keys == null ? x.keys == null : keys.SequenceEqual(x.keys)
                && NullableEquals(x.specifier, specifier)
                && x.function == function;
        }

        public override bool Equals(object obj) {
            return Equals(obj as DeclSpecs);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public override Position Pos => pos;

        /// <summary>
        /// At most one storage specifier.
        /// </summary>
        public readonly STStoreSpec.Kind storage;

        /// <summary>
        /// All the qualifiers.
        /// </summary>
        public readonly TQualifiers qualifiers;

        /// <summary>
        /// At most one function specifier (inline).
        /// </summary>
        public readonly STFuncSpec.Kind function;

        /// <summary>
        /// All the type specifiers EXCEPT struct, union, enum, typedef.
        /// </summary>
        public readonly IEnumerable<TypeSpec.Kind> keys;

        /// <summary>
        /// All the struct, union, enum, typedef specifier.
        /// </summary>
        public readonly TypeUserSpec specifier;

        /// <summary>
        /// Get the qualified type as LValue.
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public T GetT(Env env) {
            return GetTUnqualified(env).Qualify(qualifiers, T.LR.L, T.Store.NONE);
        }

        public bool IsTypeDef => storage == STStoreSpec.Kind.TYPEDEF;
        public T.Store Storage => GetStore(storage);

        /// <summary>
        /// Evaluate the type specifiers and get the unqualified type.
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        private TUnqualified GetTUnqualified(Env env) {
            if (keys != null) {
                // This is built-in type.
                if (dict.ContainsKey(keys)) return dict[keys];
                else throw new Error(pos, string.Format("Unkown type: {0}", keys.Aggregate("", (str, key) => str + " " + key)));
            } else if (specifier != null) {
                // This is a user-defined type.
                return specifier.GetTUnqualified(env);
            } else {
                // Error!
                throw new Error(pos, "At least one type specifier should be given.");
            }
        }

        /// <summary>
        /// Evaluate the type qualifiers.
        /// </summary>
        /// <returns></returns>
        public static TQualifiers GetQualifiers(IEnumerable<TypeQual.Kind> qualifiers) {
            var tuple = new Tuple<bool, bool, bool>(
                qualifiers.Contains(TypeQual.Kind.CONST),
                qualifiers.Contains(TypeQual.Kind.RESTRICT), 
                qualifiers.Contains(TypeQual.Kind.VOLATILE));
            return TQualifiers.dict[tuple];
        }

        public static T.Store GetStore(STStoreSpec.Kind storage) {
            switch (storage) {
                case STStoreSpec.Kind.NONE: return T.Store.NONE;
                case STStoreSpec.Kind.EXTERN: return T.Store.EXTERN;
                case STStoreSpec.Kind.REGISTER: return T.Store.REGISTER;
                case STStoreSpec.Kind.STATIC: return T.Store.STATIC;
                case STStoreSpec.Kind.AUTO: return T.Store.AUTO;
                default:
                    throw new ArgumentException("Unknown storage specifier.");
            }
        }

        private readonly Position pos;

        #region TypeSpecifier Map
        private class ListComparer : IEqualityComparer<IEnumerable<TypeSpec.Kind>> {
            public bool Equals(IEnumerable<TypeSpec.Kind> x, IEnumerable<TypeSpec.Kind> y) {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(IEnumerable<TypeSpec.Kind> x) {
                int hash = 0;
                foreach (var s in x) {
                    hash |= s.GetHashCode();
                }
                return hash;
            }
        }

        private static Dictionary<IEnumerable<TypeSpec.Kind>, TUnqualified> dict =
            new Dictionary<IEnumerable<TypeSpec.Kind>, TUnqualified>(new ListComparer()) {
                {
                    // void
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.VOID
                    } orderby s ascending select s,
                    TVoid.Instance
                },
                {
                    // char
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.CHAR
                    } orderby s ascending select s,
                    TChar.Instance
                },
                {
                    // signed char
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SIGNED,
                        TypeSpec.Kind.CHAR
                    } orderby s ascending select s,
                    TSChar.Instance
                },
                {
                    // unsigned char
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.UNSIGNED,
                        TypeSpec.Kind.CHAR
                    } orderby s ascending select s,
                    TUChar.Instance
                },
                {
                    // short
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SHORT
                    } orderby s ascending select s,
                    TShort.Instance
                },
                {
                    // signed short
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SIGNED,
                        TypeSpec.Kind.SHORT
                    } orderby s ascending select s,
                    TShort.Instance
                },
                {
                    // short int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SHORT,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TShort.Instance
                },
                {
                    // signed short int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SIGNED,
                        TypeSpec.Kind.SHORT,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TShort.Instance
                },
                {
                    // unsigned short
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.UNSIGNED,
                        TypeSpec.Kind.SHORT
                    } orderby s ascending select s,
                    TUShort.Instance
                },
                {
                    // unsigned short int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.UNSIGNED,
                        TypeSpec.Kind.SHORT,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TUShort.Instance
                },
                {
                    // int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TInt.Instance
                },
                {
                    // signed
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SIGNED
                    } orderby s ascending select s,
                    TInt.Instance
                },
                {
                    // signed int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SIGNED,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TInt.Instance
                },
                {
                    // unsigned
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.UNSIGNED
                    } orderby s ascending select s,
                    TUInt.Instance
                },
                {
                    // unsigned int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.UNSIGNED,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TUInt.Instance
                },
                {
                    // long
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.LONG
                    } orderby s ascending select s,
                    TLong.Instance
                },
                {
                    // signed long
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SIGNED,
                        TypeSpec.Kind.LONG
                    } orderby s ascending select s,
                    TLong.Instance
                },
                {
                    // long int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TLong.Instance
                },
                {
                    // signed long int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SIGNED,
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TLong.Instance
                },
                {
                    // unsigned long
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.UNSIGNED,
                        TypeSpec.Kind.LONG
                    } orderby s ascending select s,
                    TULong.Instance
                },
                {
                    // unsigned long int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.UNSIGNED,
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TULong.Instance
                },
                {
                    // long long
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.LONG
                    } orderby s ascending select s,
                    TLLong.Instance
                },
                {
                    // signed long long
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SIGNED,
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.LONG
                    } orderby s ascending select s,
                    TLLong.Instance
                },
                {
                    // long long int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TLLong.Instance
                },
                {
                    // signed long long int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.SIGNED,
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TLLong.Instance
                },
                {
                    // unsigned long long
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.UNSIGNED,
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.LONG
                    } orderby s ascending select s,
                    TULLong.Instance
                },
                {
                    // unsigned long long int
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.UNSIGNED,
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.INT
                    } orderby s ascending select s,
                    TULLong.Instance
                },
                {
                    // float
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.FLOAT
                    } orderby s ascending select s,
                    TFloat.Instance
                },
                {
                    // double
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.DOUBLE
                    } orderby s ascending select s,
                    TDouble.Instance
                },
                {
                    // long double
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.LONG,
                        TypeSpec.Kind.DOUBLE
                    } orderby s ascending select s,
                    TLDouble.Instance
                },
                {
                    // double
                    from s in new List<TypeSpec.Kind> {
                        TypeSpec.Kind.BOOL
                    } orderby s ascending select s,
                    TBool.Instance
                },
            };
        #endregion
    }

    public sealed class STInitDeclarator : Node, IEquatable<STInitDeclarator> {

        public STInitDeclarator(Declarator declarator, STInitializer initializer = null) {
            this.declarator = declarator;
            this.initializer = initializer;
        }

        public bool Equals(STInitDeclarator x) {
            return x != null && x.declarator.Equals(declarator) && NullableEquals(x.initializer, initializer);
        }

        public override bool Equals(object obj) {
            return Equals(obj as STInitDeclarator);
        }

        public override int GetHashCode() {
            return declarator.GetHashCode();
        }

        public override Position Pos => declarator.Pos;

        public readonly Declarator declarator;
        public readonly STInitializer initializer;
    }

    public abstract class DeclSpec : Node { }

    public sealed class STStoreSpec : DeclSpec, IEquatable<STStoreSpec> {

        public enum Kind {
            NONE,           // Represent no storage-specifier
            TYPEDEF,
            EXTERN,
            STATIC,
            AUTO,
            REGISTER
        }

        public STStoreSpec(int line, Kind type) {
            this.pos = new Position { line = line };
            this.kind = type;
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as STStoreSpec);
        }

        public bool Equals(STStoreSpec x) {
            return x != null && x.pos.Equals(pos) && x.kind == kind;
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public readonly Kind kind;
        public readonly Position pos;
    }

    public abstract class TypeSpecQual : DeclSpec { }

    public abstract class TypeSpec : TypeSpecQual {

        public enum Kind {
            VOID,
            CHAR,
            SHORT,
            INT,
            LONG,
            FLOAT,
            DOUBLE,
            SIGNED,
            UNSIGNED,
            BOOL,
            COMPLEX,
            STRUCT,
            UNION,
            ENUM,
            TYPEDEF
        }

        public TypeSpec(Kind kind) {
            this.kind = kind;
        }

        public readonly Kind kind;
    }

    public sealed class STTypeKeySpec : TypeSpec, IEquatable<STTypeKeySpec> {

        public STTypeKeySpec(int line, Kind kind) : base(kind) {
            this.pos = new Position { line = line };
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as STTypeKeySpec);
        }

        public bool Equals(STTypeKeySpec x) {
            return x != null && x.pos.Equals(pos) && x.kind == kind;
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public readonly Position pos;
    }

    /// <summary>
    /// Represents a user-defined type specifier.
    /// </summary>
    public abstract class TypeUserSpec : TypeSpec {
        public TypeUserSpec(Kind kind) : base(kind) { }
        public abstract TUnqualified GetTUnqualified(Env env);
    }

    public sealed class StructUnionSpec : TypeUserSpec, IEquatable<StructUnionSpec> {

        public StructUnionSpec(
            int line,
            Id identifier,
            IEnumerable<StructDeclaration> declarations,
            Kind kind
            ) : base(kind) {
            if (kind != Kind.STRUCT && kind != Kind.UNION)
                throw new ArgumentException("This must be either struct or union!");
            pos = new Position { line = line };
            this.identifier = identifier;
            this.declarations = declarations;
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as StructUnionSpec);
        }

        public bool Equals(StructUnionSpec x) {
            return x != null
                && NullableEquals(x.identifier, identifier)
                && NullableEquals(x.declarations, declarations);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        /// <summary>
        /// Evaluate the struct or union specifier and get the unqualified type.
        /// 
        /// struct/union tag
        ///     - check if the tag has been declaraed in the current scope
        ///     - yes -> return the type
        ///     - no  -> make this an incomplete type in the current scope
        ///     
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public override TUnqualified GetTUnqualified(Env env) {

            Func<TagEntry, bool> IsSameType = (tagEntry) => {
                if (kind == Kind.STRUCT) return tagEntry.type.IsStruct;
                else return tagEntry.type.IsUnion;
            };

            if (declarations == null) {
                // struct/union tag
                var tag = identifier.name;
                
                // Check the tag in all scopes.
                var tagEntry = env.GetTag(tag, false);
                if (tagEntry != null) {
                    // This tag has been declaraed, check if this is the same type.
                    if (IsSameType(tagEntry.Value))
                        return tagEntry.Value.type;
                    else
                        throw new ErrDeclareTagAsDifferentType(pos, tag, tagEntry.Value.node.Pos, tagEntry.Value.type);
                } else {
                    // This tag has not been declaraed, make it an incomplete type.
                    TStructUnion type = kind == Kind.STRUCT ? new TStruct(tag) as TStructUnion : new TUnion(tag);
                    env.AddTag(identifier.name, type, this);
                    return type;
                }
            } else {
                TStructUnion type;
                TagEntry entry;

                if (identifier == null) {
                    // struct/union { ... }
                    // should declare a unique new type.
                    type = kind == Kind.STRUCT ? new TStruct() as TStructUnion : new TUnion();
                    entry = env.AddTag(type.tag, type, this);
                } else {
                    // struct/union tag { ... }
                    // Check if this tag has been declared in the current scope.
                    var tag = identifier.name;
                    var tmp = env.GetTag(tag, true);
                    if (tmp != null) {
                        // This tag has been declaraed, check if this is the same type
                        if (!IsSameType(tmp.Value))
                            throw new ErrDeclareTagAsDifferentType(pos, tag, tmp.Value.node.Pos, tmp.Value.type);
                        // Check if this type is already complete.
                        if (tmp.Value.type.IsComplete)
                            throw new ErrRedefineTag(pos, tag, tmp.Value.node.Pos);
                        entry = tmp.Value;
                        type = entry.type as TStructUnion;
                    } else {
                        type = kind == Kind.STRUCT ? new TStruct(tag) as TStructUnion : new TUnion(tag);
                        entry = env.AddTag(type.tag, type, this);
                    }
                }

                // Push a new scope.
                env.PushScope(ScopeKind.BLOCK);

                // Get all the fields.
                var fields = declarations.Aggregate(
                    Enumerable.Empty<Tuple<string, T>>(),
                    (acc, decl) => acc.Concat(decl.GetFields(env)));

                // Complete the definition of the struct/union with all these structs.
                if (kind == Kind.STRUCT) {
                    type.DefStruct(fields);
                } else {
                    type.DefUnion(fields);
                }

                // Update the entry and set the node to this syntax tree node.
                entry.node = this;

                // Pop a new scope.
                env.PopScope();

                return type;
            }
        }

        private readonly Position pos;

        /// <summary>
        /// Nullable.
        /// </summary>
        public readonly Id identifier;
        /// <summary>
        /// Nullable.
        /// </summary>
        public readonly IEnumerable<StructDeclaration> declarations;
    }

    public sealed class StructDeclaration : Node, IEquatable<StructDeclaration> {

        public StructDeclaration(
            DeclSpecs specifiers,
            IEnumerable<StructDeclarator> declarators
            ) {
            this.specifiers = specifiers;
            this.declarators = declarators;
        }

        public override Position Pos => specifiers.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as StructDeclaration);
        }

        public bool Equals(StructDeclaration x) {
            return x != null
                && x.specifiers.Equals(specifiers)
                && x.declarators.SequenceEqual(declarators);
        }

        public override int GetHashCode() {
            return specifiers.GetHashCode();
        }

        /// <summary>
        /// Evaluate the fields and get all the fields.
        /// </summary>
        /// <param name="env"> Environment. </param>
        /// <returns> A list of tuples, containing the field name and its type. </returns>
        public IEnumerable<Tuple<string, T>> GetFields(Env env) {

            // First get the type.
            T type = specifiers.GetT(env);

            // Get all the fields.
            return declarators.Aggregate(
                new LinkedList<Tuple<string, T>>(),
                (acc, decl) => {
                    acc.AddLast(decl.GetField(env, type));
                    return acc;
                });
        }

        public readonly DeclSpecs specifiers;
        public readonly IEnumerable<StructDeclarator> declarators;
    }

    /// <summary>
    /// struct-declarator
    ///     : declarator
    ///     | declarator_opt : constant-expression
    ///     ;
    /// </summary>
    public sealed class StructDeclarator : Node, IEquatable<StructDeclarator> {

        public StructDeclarator(Declarator declarator, Expr expr) {
            this.declarator = declarator;
            this.expr = expr;
        }

        public override Position Pos => declarator == null ? expr.Pos : declarator.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as StructDeclarator);
        }

        public bool Equals(StructDeclarator x) {
            return x != null
                && NullableEquals(x.declarator, declarator)
                && NullableEquals(x.expr, expr);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        /// <summary>
        /// Get the field and the type.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Tuple<string, T> GetField(Env env, T type) {


            if (declarator != null) {

            }

            // Whether a bit field.
            if (expr == null) {

            } else {

            }

            throw new NotImplementedException();
        }

        public readonly Declarator declarator;
        public readonly Expr expr;
    }

    public sealed class STEnumSpec : TypeUserSpec, IEquatable<STEnumSpec> {

        public STEnumSpec(int line, Id id, IEnumerable<STEnum> enums)
            : base(Kind.ENUM) {
            pos = new Position { line = line };
            this.id = id;
            this.enums = enums;
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as STEnumSpec);
        }

        public bool Equals(STEnumSpec x) {
            return x != null
                && NullableEquals(x.id, id)
                && NullableEquals(x.enums, enums);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public override TUnqualified GetTUnqualified(Env env) {
            throw new NotImplementedException();
        }

        //public override TUnqualified GetTUnqualified(ASTEnv env) {
        //    if (enums == null) {
        //        // enum identifier
        //    } else {

        //        // If the type has a tag, check if redeclared.
        //        if (id != null && env.ContainsSymbolInCurrentScope(id.name)) {

        //        }

        //        // Make the new enum type.
        //        TypeEnum type = id == null ? new TypeEnum() : new TypeEnum(id.name);

        //        // Make the complete type for all the enums.
        //        T constType = new lcc.Type.Type(type, new lcc.Type.Type.Qualifier(true, false, false));

        //        foreach (var enumerator in enums) {

        //            ASTid id = enumerator.id;

        //            // First check if the name has been defined.
        //            if (env.ContainsSymbolInCurrentScope(id.name)) {
        //                Utility.TCErrRedecl(env.GetDeclaration(id.name), id);
        //            }

        //            // Check if the enumverator has an initializer.
        //            if (enumerator.expr != null) {

        //            }

        //            // Add every enumerator into the environment.
        //            env.AddSymbol(id.name, type, id.line);
        //        }

        //        return type;
        //    }
        //}

        private readonly Position pos;
        public readonly Id id;
        public readonly IEnumerable<STEnum> enums;
    }

    public sealed class STEnum : Node, IEquatable<STEnum> {

        public STEnum(Id id, Expr expr) {
            this.id = id;
            this.expr = expr;
        }

        public override Position Pos => id.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as STEnum);
        }

        public bool Equals(STEnum x) {
            return x != null
                && x.id.Equals(id)
                && NullableEquals(expr, x.expr);
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }

        public readonly Id id;
        public readonly Expr expr;
    }

    public sealed class TypeQual : TypeSpecQual, IEquatable<TypeQual> {

        public enum Kind {
            CONST,
            RESTRICT,
            VOLATILE
        }

        public TypeQual(int line, Kind kind) {
            this.pos = new Position { line = line };
            this.kind = kind;
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as TypeQual);
        }

        public bool Equals(TypeQual x) {
            return x != null && x.pos.Equals(pos) && x.kind == kind;
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public readonly Kind kind;
        private readonly Position pos;
    }

    public sealed class STFuncSpec : DeclSpec, IEquatable<STFuncSpec> {

        public enum Kind {
            NONE,
            INLINE
        }

        public STFuncSpec(int line, Kind kind) {
            this.pos = new Position { line = line };
            this.kind = kind;
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as STFuncSpec);
        }

        public bool Equals(STFuncSpec x) {
            return x != null && x.pos.Equals(pos) && x.kind == kind;
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public readonly Kind kind;
        private readonly Position pos;

    }

    public sealed class Declarator : Node, IEquatable<Declarator> {

        public Declarator(IEnumerable<Ptr> pointers, DirDeclarator direct) {
            this.pointers = pointers;
            this.direct = direct;
        }

        public bool Equals(Declarator x) {
            return x != null && x.pointers.SequenceEqual(pointers) && x.direct.Equals(direct);
        }

        public override bool Equals(object obj) {
            return Equals(obj as Declarator);
        }

        public override int GetHashCode() {
            return direct.GetHashCode();
        }

        /// <summary>
        /// Get the identifier and the type of the declarator.
        /// </summary>
        /// <param name="env"> Environment </param>
        /// <param name="type"> Type specified by declaration specifiers. </param>
        /// <returns></returns>
        public Tuple<string, T, IEnumerable<Tuple<string, T>>> Declare(Env env, T type, IEnumerable<Tuple<string, T>> ps = null) {

            // Add the pointer derivation.
            foreach (var pointer in pointers) {
                type = pointer.GetPtr(type);
            }

            // Get the declaration from direct declarator.
            return direct.Declare(env, type, ps);
        }

        public override Position Pos => direct.Pos;

        /// <summary>
        /// Zero or more pointers.
        /// </summary>
        public IEnumerable<Ptr> pointers;
        public DirDeclarator direct;
    }

    public abstract class DirDeclarator : Node {
        public abstract string Name { get; }
        public abstract Tuple<string, T, IEnumerable<Tuple<string, T>>> Declare(Env env, T type, IEnumerable<Tuple<string, T>> ps);
    }

    public sealed class IdDeclarator : DirDeclarator, IEquatable<IdDeclarator> {

        public IdDeclarator(Id id) {
            this.id = id;
        }

        public override string Name => id.name;
        public override Position Pos => id.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as IdDeclarator);
        }

        public bool Equals(IdDeclarator i) {
            return i != null && i.id.Equals(id);
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }

        public override Tuple<string, T, IEnumerable<Tuple<string, T>>> Declare(Env env, T type, IEnumerable<Tuple<string, T>> ps) {
            return new Tuple<string, T, IEnumerable<Tuple<string, T>>>(id.name, type, ps);
        }

        public readonly Id id;
    }

    public sealed class ParDeclarator : DirDeclarator, IEquatable<ParDeclarator> {

        public ParDeclarator(Declarator declarator) {
            this.declarator = declarator;
        }

        public override string Name => declarator.direct.Name;
        public override Position Pos => declarator.Pos;

        public bool Equals(ParDeclarator x) {
            return x != null && x.declarator.Equals(declarator);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ParDeclarator);
        }

        public override int GetHashCode() {
            return declarator.GetHashCode();
        }
        public override Tuple<string, T, IEnumerable<Tuple<string, T>>> Declare(Env env, T type, IEnumerable<Tuple<string, T>> ps) {
            return declarator.Declare(env, type, ps);
        }

        public readonly Declarator declarator;
    }

    public sealed class FuncDeclarator : DirDeclarator, IEquatable<FuncDeclarator> {

        public FuncDeclarator(
            DirDeclarator direct,
            IEnumerable<Param> parameters,
            bool isEllipis
            ) {
            this.direct = direct;
            this.parameters = parameters;
            this.isEllipis = isEllipis;
        }

        public FuncDeclarator(DirDeclarator direct, IEnumerable<Id> identifiers) {
            this.direct = direct;
            this.identifiers = identifiers;
        }

        public override string Name => direct.Name;

        public override Position Pos => direct.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as FuncDeclarator);
        }

        public bool Equals(FuncDeclarator x) {
            return x != null && x.direct.Equals(direct)
                && NullableEquals(x.parameters, parameters)
                && x.isEllipis == isEllipis
                && NullableEquals(x.identifiers, identifiers);
        }

        public override int GetHashCode() {
            return direct.GetHashCode();
        }

        /// <summary>
        /// Get the name and the type of the function, as well as the parameter names.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override Tuple<string, T, IEnumerable<Tuple<string, T>>> Declare(Env env, T type, IEnumerable<Tuple<string, T>> ps) {
            if (parameters == null) {
                // This is an identifier list.
                if (identifiers.Count() == 0) {
                    return direct.Declare(env, type.Func(Enumerable.Empty<T>(), false), Enumerable.Empty<Tuple<string, T>>());
                } else if (env.IsFuncDef) {
                    throw new Error(Pos, "sorry identifier list in a function definition is not supported yet.");
                } else {
                    throw new Error(Pos, "an identifier list in a function declarator that is not part of a definition of that function shall be empty.");
                }
            } else {
                var result = GetT(env, type, parameters, isEllipis, Pos);
                return direct.Declare(env, result.Item1, result.Item2);
            }
        }

        /// <summary>
        /// Get the type of the function and the name of the parameters.
        /// 
        /// Constraints:
        ///     1. If the declarator is part of function definition, all the parameter should have name.
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <param name="isEllipis"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Tuple<T, IEnumerable<Tuple<string, T>>> GetT(Env env, T type, IEnumerable<Param> parameters, bool isEllipis, Position pos) {

            // A function declarator shall not specify a return type that is a function type or an array type.
            if (type.IsFunc) {
                throw new Error(pos,
                    string.Format("A function declaration shall not specify a return type that is a function type: {0}.", type));
            }
            if (type.IsArray) {
                throw new Error(pos,
                    string.Format("A function declaration shall not specify a return type that is an array type: {0}.", type));
            }

            // Start evaluate the parameters.
            env.PushScope(ScopeKind.PARAM);
            LinkedList<Tuple<string, T>> ps = new LinkedList<Tuple<string, T>>();

            // Special case for (void).
            if (parameters.Count() == 1) {
                var param = parameters.First().Declare(env);
                if (env.IsFuncDef && param.Item1 == null && !param.Item2.nake.Equals(TVoid.Instance)) {
                    throw new Error(parameters.First().Pos, "parameter for function definition should have a name.");
                }
                if (!param.Item2.nake.Equals(TVoid.Instance)) {
                    ps.AddLast(param);
                }
            } else {
                // Evaluate each parameter.
                foreach (var parameter in parameters) {
                    var param = parameter.Declare(env);
                    if (env.IsFuncDef && param.Item1 == null) {
                        throw new Error(parameter.Pos, "parameter for function definition should have a name.");
                    }
                    ps.AddLast(param);
                }
            }

            env.PopScope();

            return new Tuple<T, IEnumerable<Tuple<string, T>>>(type.Func(from p in ps select p.Item2, isEllipis), ps);
        }

        public readonly DirDeclarator direct;

        // ( parameter-type-list )
        // Maybe null.
        public readonly IEnumerable<Param> parameters;
        public readonly bool isEllipis;

        // ( identifier-list_opt )
        // Maybe null or empty.
        public readonly IEnumerable<Id> identifiers;
    }

    public sealed class ArrDeclarator : DirDeclarator, IEquatable<ArrDeclarator> {

        public ArrDeclarator(
            DirDeclarator direct,
            IEnumerable<TypeQual> qualifiers,
            Expr expr,
            bool isStatic
            ) {
            this.direct = direct;
            this.qualifiers = qualifiers;
            this.expr = expr;
            this.isStatic = isStatic;
            this.isStar = false;
        }

        public ArrDeclarator(DirDeclarator direct, IEnumerable<TypeQual> qualifiers) {
            this.direct = direct;
            this.qualifiers = qualifiers;
            this.expr = null;
            this.isStar = true;
            this.isStatic = false;
        }

        public override string Name => direct.Name;

        public bool Equals(ArrDeclarator x) {
            return x != null && x.direct.Equals(direct)
                && x.qualifiers.SequenceEqual(qualifiers)
                && x.isStatic == isStatic && x.isStar == isStar
                && NullableEquals(x.expr, expr);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ArrDeclarator);
        }

        public override int GetHashCode() {
            return direct.GetHashCode();
        }

        /// <summary>
        /// Array declarator.
        /// For now only support complete array and incomplete array.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override Tuple<string, T, IEnumerable<Tuple<string, T>>> Declare(Env env, T type, IEnumerable<Tuple<string, T>> ps) {
            return direct.Declare(env, Declare(env, type, expr, Pos), null);
        }

        public static T Declare(Env env, T type, Expr expr, Position pos) {
            // The element type shall not be an incomplete type.
            if (!type.IsComplete) {
                throw new Error(pos, string.Format("array has incomplete element type: {0}.", type));
            }

            // The element type shall not be a function type.
            if (type.IsFunc) {
                throw new Error(pos, string.Format("use function type as array element: {0}.", type));
            }

            // Check the expression is const integer expression.
            if (expr != null) {
                AST.ConstIntExpr ast = expr.ToAST(env) as AST.ConstIntExpr;
                if (ast == null) {
                    throw new Error(pos, "use non-constant expression as array length.");
                }
                if (ast.value <= 0) {
                    throw new Error(pos, "array length should be greater than zero.");
                }
                if (ast.value > TInt.Instance.MAX) {
                    throw new Error(pos, string.Format("sorry we can't handle such long array: {0}.", ast.value));
                }
                return type.Arr((int)ast.value);
            } else {
                // Set this as incomplete array.
                return type.IArr();
            }
        }

        public override Position Pos => direct.Pos;

        public readonly DirDeclarator direct;

        public readonly Expr expr;
        public readonly IEnumerable<TypeQual> qualifiers;
        public readonly bool isStatic;
        public readonly bool isStar;
    }

    public sealed class Ptr : Node, IEquatable<Ptr> {

        public Ptr(int line, IEnumerable<TypeQual> qualifiers) {
            this.pos = new Position { line = line };
            this.qualifiers = DeclSpecs.GetQualifiers(qualifiers.Select(_ => _.kind));
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as Ptr);
        }

        public bool Equals(Ptr x) {
            return x != null && x.pos.Equals(pos) && x.qualifiers.Equals(qualifiers);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public T GetPtr(T type) {
            return type.Ptr(qualifiers);
        }

        private readonly Position pos;
        public readonly TQualifiers qualifiers;
    }

    public sealed class Param : Node, IEquatable<Param> {

        public Param(DeclSpecs specifiers, Declarator declarator) {
            this.specifiers = specifiers;
            this.declarator = declarator;
        }

        public Param(DeclSpecs specifiers, AbsDeclarator absDeclarator = null) {
            this.specifiers = specifiers;
            this.absDeclarator = absDeclarator;
        }

        public override Position Pos => specifiers.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as Param);
        }

        public bool Equals(Param x) {
            return x != null && specifiers.Equals(x.specifiers) && NullableEquals(x.declarator, declarator)
                && NullableEquals(x.absDeclarator, absDeclarator);
        }

        public override int GetHashCode() {
            return specifiers.GetHashCode();
        }

        /// <summary>
        /// Get the parameter name and type.
        /// If this is an abstract declarator, the parameter name is set to null.
        /// 
        /// Constraints:
        ///     1. The only storage-class specifier that shall occur in a parameter declaration is register.
        ///     2. A declaration of a parameter as "array of type" shall be adjusted to "qualified pointer to type".
        ///     3. A declaration of a parameter as "function returning type" shall be adjusted to "pointer to function returning type".
        ///     4. All the parameter names shall be different.
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public Tuple<string, T> Declare(Env env) {

            // Turn on the env.IsFuncParam switch.
            env.IsFuncParam = true;

            // 1. The only storage-class specifier that shall occur in a parameter declaration is register.
            if (specifiers.storage != STStoreSpec.Kind.NONE && specifiers.storage != STStoreSpec.Kind.REGISTER) {
                throw new Error(Pos, string.Format("Illegal storage-class specifier in a parameter declaration: {0}", specifiers.storage));
            }

            T type = specifiers.GetT(env);
            string name = null;

            if (declarator != null) {
                // parameter-declaration: declaration-sepcifiers declarator
                var declaration = declarator.Declare(env, type);
                name = declaration.Item1;
                type = declaration.Item2;

                // 4. All the parameter name shall be different.
                var entry = env.GetSymbol(name, true);
                if (env.GetSymbol(name, true) != null) {
                    throw new ErrRedefineSymbol(Pos, name, entry.Pos);
                }

            } else if (absDeclarator != null) {
                // parameter-declaration: declaration-specifiers abstract-declarator
                type = absDeclarator.Declare(env, type);
            } 

            // 2. A declaration of a parameter as "array of type" shall be adjusted to "qualified pointer to type".
            if (type.IsArray) {
                type = (type.nake as TArray).element.Ptr(type.qualifiers, T.LR.L, specifiers.Storage);
            }

            // 3. A declaration of a parameter as "function returning type" shall be adjusted to "pointer to function returning type".
            if (type.IsFunc) {
                type = type.Ptr();
            }

            // Push the name into the environment.
            if (name != null) {
                env.AddParam(name, type, this);
            }

            // Turn off the env.IsFuncParam switch.
            env.IsFuncParam = false;

            return new Tuple<string, T>(name, type);
        }

        public readonly DeclSpecs specifiers;
        public readonly Declarator declarator;
        public readonly AbsDeclarator absDeclarator;
    }

    public sealed class TypeName : Node, IEquatable<TypeName> {

        public TypeName(DeclSpecs specifiers, AbsDeclarator declarator = null) {
            this.specifiers = specifiers;
            this.declarator = declarator;
        }

        public bool Equals(TypeName x) {
            return x != null && x.specifiers.Equals(specifiers) && NullableEquals(declarator, x.declarator);
        }

        public override bool Equals(object obj) {
            return Equals(obj as TypeName);
        }

        public override int GetHashCode() {
            return specifiers.GetHashCode();
        }

        public T GetT(Env env) {
            T type = specifiers.GetT(env);
            return declarator == null ? type : declarator.Declare(env, type);
        }

        public override Position Pos => specifiers.Pos;

        public readonly DeclSpecs specifiers;
        public readonly AbsDeclarator declarator;
    }

    public sealed class AbsDeclarator : Node, IEquatable<AbsDeclarator> {

        public AbsDeclarator(IEnumerable<Ptr> pointers, AbsDirDeclarator direct) {
            this.pointers = pointers;
            this.direct = direct;
        }

        public bool Equals(AbsDeclarator x) {
            return x != null && x.pointers.SequenceEqual(pointers) && NullableEquals(x.direct, direct);
        }

        public override bool Equals(object obj) {
            return Equals(obj as AbsDeclarator);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public T Declare(Env env, T type) {

            // Add the pointer derivation.
            foreach (var ptr in pointers) {
                type = ptr.GetPtr(type);
            }

            return direct != null ? direct.Declare(env, type) : type;
        }

        public override Position Pos => direct == null ? pointers.First().Pos : direct.Pos;

        public IEnumerable<Ptr> pointers;

        /// <summary>
        /// Nullable.
        /// </summary>
        public AbsDirDeclarator direct;
    }

    public abstract class AbsDirDeclarator : Node {
        public abstract T Declare(Env env, T type);
    }

    public sealed class AbsDirDeclaratorNil : AbsDirDeclarator {
        public AbsDirDeclaratorNil(int line) {
            pos = new Position { line = line };
        }

        public bool Equals(AbsDirDeclaratorNil x) {
            return x != null && x.pos.Equals(pos);
        }

        public override bool Equals(object obj) {
            return Equals(obj as AbsDirDeclaratorNil);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public override T Declare(Env env, T type) {
            return type;
        }

        public override Position Pos => pos;
        private readonly Position pos;
    }

    public sealed class AbsParDeclarator : AbsDirDeclarator, IEquatable<AbsParDeclarator> {

        public AbsParDeclarator(AbsDeclarator declarator) {
            this.declarator = declarator;
        }

        public bool Equals(AbsParDeclarator x) {
            return x != null && x.declarator.Equals(declarator);
        }

        public override bool Equals(object obj) {
            return Equals(obj as AbsParDeclarator);
        }

        public override int GetHashCode() {
            return declarator.GetHashCode();
        }

        public override T Declare(Env env, T type) {
            return declarator.Declare(env, type);
        }

        public override Position Pos => declarator.Pos;

        public readonly AbsDeclarator declarator;
    }

    public sealed class AbsArrDeclarator : AbsDirDeclarator, IEquatable<AbsArrDeclarator> {

        public AbsArrDeclarator(
            AbsDirDeclarator direct,
            IEnumerable<TypeQual> qualifiers,
            Expr expr,
            bool isStatic
            ) {
            this.direct = direct;
            this.qualifiers = qualifiers;
            this.expr = expr;
            this.isStatic = isStatic;
            this.isStar = false;
        }

        public AbsArrDeclarator(AbsDirDeclarator direct) {
            this.direct = direct;
            this.qualifiers = new List<TypeQual>();
            this.expr = null;
            this.isStar = true;
            this.isStatic = false;
        }

        public bool Equals(AbsArrDeclarator x) {
            return x != null && NullableEquals(x.direct, direct)
                && x.qualifiers.SequenceEqual(qualifiers)
                && x.isStatic == isStatic && x.isStar == isStar
                && NullableEquals(x.expr, expr);
        }

        public override bool Equals(object obj) {
            return Equals(obj as AbsArrDeclarator);
        }

        public override int GetHashCode() {
            return direct.GetHashCode();
        }

        public override T Declare(Env env, T type) {
            return ArrDeclarator.Declare(env, type, expr, Pos);
        }

        public override Position Pos => direct.Pos;

        public readonly AbsDirDeclarator direct;

        public readonly Expr expr;
        public readonly IEnumerable<TypeQual> qualifiers;
        public readonly bool isStatic;
        public readonly bool isStar;
    }

    public sealed class AbsFuncDeclarator : AbsDirDeclarator, IEquatable<AbsFuncDeclarator> {

        public AbsFuncDeclarator(
            AbsDirDeclarator direct,
            IEnumerable<Param> parameters,
            bool isEllipis
            ) {
            this.direct = direct;
            this.parameters = parameters;
            this.isEllipis = isEllipis;
        }

        public override Position Pos => direct.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as AbsFuncDeclarator);
        }

        public bool Equals(AbsFuncDeclarator x) {
            return x != null && NullableEquals(x.direct, direct)
                && NullableEquals(x.parameters, parameters)
                && x.isEllipis == isEllipis;
        }

        public override int GetHashCode() {
            return direct.GetHashCode();
        }

        /// <summary>
        /// Get the function derivation.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override T Declare(Env env, T type) {

            if (parameters == null) {
                return direct.Declare(env, type.Func(Enumerable.Empty<T>(), false));
            } else {
                type = FuncDeclarator.GetT(env, type, parameters, isEllipis, Pos).Item1;
                return direct.Declare(env, type);
            }

        }

        public readonly AbsDirDeclarator direct;

        // ( parameter-type-list )
        public readonly IEnumerable<Param> parameters;
        public readonly bool isEllipis;
    }

    public sealed class STTypedefName : TypeUserSpec, IEquatable<STTypedefName> {

        public STTypedefName(Id identifier) : base(Kind.TYPEDEF) {
            name = identifier.name;
            pos = identifier.Pos;
        }

        public bool Equals(STTypedefName x) {
            return x != null && x.name.Equals(name) && x.pos.Equals(pos);
        }

        public override bool Equals(object obj) {
            return Equals(obj as STTypedefName);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public override TUnqualified GetTUnqualified(Env env) {
            throw new NotImplementedException();
        }

        public override Position Pos => pos;

        public readonly string name;
        private readonly Position pos;
    }

    /// <summary>
    /// initializer
    ///     : assignment-expression
    ///     | { initializer-list }
    ///     | { initializer-list , }
    ///     ;
    /// </summary>
    public sealed class STInitializer : Node, IEquatable<STInitializer> {

        /// <summary>
        /// initializer : assignment-expression;
        /// </summary>
        /// <param name="expr"></param>
        public STInitializer(Expr expr) { this.expr = expr; }
        public STInitializer(IEnumerable<STInitItem> items) { this.items = items; }

        public bool Equals(STInitializer x) {
            return x != null
                && NullableEquals(x.expr, expr)
                && NullableEquals(x.items, items);
        }

        public override bool Equals(object obj) {
            return Equals(obj as STInitializer);
        }

        public override int GetHashCode() {
            return expr != null ? expr.GetHashCode() : items.First().GetHashCode();
        }

        public override Position Pos => expr != null ? expr.Pos : items.First().Pos;

        public readonly Expr expr;
        public readonly IEnumerable<STInitItem> items;
    }

    /// <summary>
    /// init-item : designation_opt initializer;
    /// </summary>
    public sealed class STInitItem : Node, IEquatable<STInitItem> {

        public STInitItem(STInitializer initializer, IEnumerable<STDesignator> designators = null) {
            this.initializer = initializer;
            this.designators = designators;
        }

        public bool Equals(STInitItem x) {
            return x != null
                && x.initializer.Equals(initializer)
                && NullableEquals(x.designators, designators);
        }

        public override bool Equals(object obj) {
            return Equals(obj as STInitItem);
        }

        public override int GetHashCode() {
            return initializer.GetHashCode();
        }

        public override Position Pos => designators == null ? initializer.Pos : designators.First().Pos;

        public readonly STInitializer initializer;
        public readonly IEnumerable<STDesignator> designators;
    }

    /// <summary>
    /// [ constant-expression ]
    /// .identfier
    /// </summary>
    public sealed class STDesignator : Node, IEquatable<STDesignator> {

        public STDesignator(Expr expr) { this.expr = expr; }
        public STDesignator(Id id) { this.id = id; }

        public override Position Pos => expr != null ? expr.Pos : id.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as STDesignator);
        }

        public bool Equals(STDesignator x) {
            return x != null
                && NullableEquals(x.expr, expr)
                && NullableEquals(x.id, id);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public readonly Expr expr;
        public readonly Id id;
    }

}
