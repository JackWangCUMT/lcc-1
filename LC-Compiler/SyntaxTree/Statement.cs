﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lcc.AST;
using lcc.TypeSystem;

namespace lcc.SyntaxTree {
    public abstract class Stmt : Node {
        public virtual AST.Stmt ToAST(Env env) {
            throw new NotImplementedException();
        }
    }

    public sealed class Labeled : Stmt, IEquatable<Labeled> {

        public Labeled(Id id, Stmt statement) {
            this.id = id;
            this.stmt = statement;
        }

        public override Position Pos => id.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as Labeled);
        }

        public bool Equals(Labeled x) {
            return x != null
                && x.id.Equals(id)
                && x.stmt.Equals(stmt);
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }

        public override AST.Stmt ToAST(Env env) {
            if (Id.IsReservedIdentifier(id.symbol)) throw new EReservedIdentifier(Pos, id.symbol);
            if (env.GetLable(id.symbol) != null) throw new ERedfineLabel(Pos, id.symbol);
            string transformed = env.AddLabel(id.symbol);
            return new AST.Labeled(transformed, stmt.ToAST(env));
        }

        public readonly Id id;
        public readonly Stmt stmt;
    }

    public sealed class Case : Stmt, IEquatable<Case> {

        public Case(Expr expr, Stmt statement) {
            this.expr = expr;
            this.stmt = statement;
        }

        public override Position Pos => expr.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as Case);
        }

        public bool Equals(Case x) {
            return x != null
                && x.expr.Equals(expr)
                && x.stmt.Equals(stmt);
        }

        public override int GetHashCode() {
            return expr.GetHashCode();
        }

        public override AST.Stmt ToAST(Env env) {
            /// Check the switch statement.
            Switch sw = env.GetSwitch();
            if (sw == null) {
                throw new Error(Pos, "default statement not in switch statement");
            }

            /// The expression of a case should be constant integer expression.
            AST.ConstIntExpr c = expr.GetASTExpr(env) as AST.ConstIntExpr;
            if (c == null) {
                throw new Error(Pos, "the expression of a case should be constant integer expression");
            }

            /// No two of the case constant expressions shall have the same value.
            /// TODO: The conversion.
            foreach (var e in sw.cases) {
                if (c.value == e.Item2.value) {
                    throw new Error(Pos, string.Format("duplicate value {0} in case", c.value));
                }
            }

            string label = env.AllocCaseLabel();

            sw.cases.AddLast(new Tuple<string, ConstIntExpr>(label, c));

            AST.Stmt s = stmt.ToAST(env);

            return new AST.Labeled(label, s);
        }

        public readonly Expr expr;
        public readonly Stmt stmt;
    }

    public sealed class Default : Stmt, IEquatable<Default> {

        public Default(Stmt statement) {
            this.stmt = statement;
        }

        public override Position Pos => stmt.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as Default);
        }

        public bool Equals(Default x) {
            return x != null && x.stmt.Equals(stmt);
        }

        public override int GetHashCode() {
            return stmt.GetHashCode();
        }

        public override AST.Stmt ToAST(Env env) {

            /// Check the switch statement.
            Switch sw = env.GetSwitch();
            if (sw == null) {
                throw new Error(Pos, "default statement not in switch statement");
            }

            /// At most one default statement in a switch.
            if (sw.defaultLabel != null) {
                throw new Error(Pos, "at most one default label in a switch statement");
            }

            string label = env.AllocDefaultLabel();
            sw.defaultLabel = label;

            AST.Stmt s = stmt.ToAST(env);

            return new AST.Labeled(label, s);
        }

        public readonly Stmt stmt;
    }

    public sealed class CompoundStmt : Stmt, IEquatable<CompoundStmt> {

        public CompoundStmt(IEnumerable<Stmt> stmts) {
            this.stmts = stmts;
        }

        public override Position Pos => stmts.First().Pos;

        public override bool Equals(object obj) {
            return Equals(obj as CompoundStmt);
        }

        public bool Equals(CompoundStmt x) {
            return x != null && x.stmts.SequenceEqual(stmts);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public override AST.Stmt ToAST(Env env) {
            env.PushScope(ScopeKind.BLOCK);
            LinkedList<AST.Stmt> results = new LinkedList<AST.Stmt>();
            foreach (var stmt in stmts) {
                results.AddLast(stmt.ToAST(env));
            }
            env.PopScope();
            return new AST.CompoundStmt(results);
        }

        //public IEnumerable<AST.Stmt> FuncBody(Env env, T type, IEnumerable<)

        public readonly IEnumerable<Stmt> stmts;
    }

    public sealed class If : Stmt, IEquatable<If> {

        public If(
            int line,
            Expr expr,
            Stmt then,
            Stmt other
            ) {
            this.pos = new Position { line = line };
            this.expr = expr;
            this.then = then;
            this.other = other;
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as If);
        }

        public bool Equals(If x) {
            return x != null
                && x.pos.Equals(pos)
                && x.expr.Equals(expr)
                && x.then.Equals(then)
                && x.other == null ? other == null : x.other.Equals(other);
        }

        public override int GetHashCode() {
            return expr.GetHashCode();
        }

        /// <summary>
        /// A selection statement is a block whose scope is a strict subset
        /// of the scope of its enclosing block.
        /// 
        /// The controlling expression of an if statement shall have scalar type.
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public override AST.Stmt ToAST(Env env) {
            env.PushScope(ScopeKind.BLOCK);
            AST.Expr e = expr.GetASTExpr(env);
            if (!e.Type.IsScalar) {
                throw new ETypeError(Pos, string.Format("expecting scalar type, given {0}", e.Type));
            }

            AST.Stmt t = then.ToAST(env);
            AST.Stmt o = other != null ? other.ToAST(env) : null;

            env.PopScope();
            return new AST.If(e, t, o);
        }

        private readonly Position pos;
        public readonly Expr expr;
        public readonly Stmt then;
        public readonly Stmt other;
    }

    public abstract class Breakable : Stmt {
        public string breakLabel;
    }

    public sealed class Switch : Breakable, IEquatable<Switch> {

        public Switch(
            int line,
            Expr expr,
            Stmt statement
            ) {
            this.pos = new Position { line = line };
            this.expr = expr;
            this.stmt = statement;
            this.cases = new LinkedList<Tuple<string, ConstIntExpr>>();
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as Switch);
        }

        public bool Equals(Switch x) {
            return x != null
                && x.pos.Equals(pos)
                && x.expr.Equals(expr)
                && x.stmt.Equals(stmt);
        }

        public override int GetHashCode() {
            return expr.GetHashCode();
        }

        public override AST.Stmt ToAST(Env env) {

            /// The controlling expression shall have integer type.
            e = expr.GetASTExpr(env);
            if (!e.Type.IsInteger) {
                throw new ETypeError(Pos, "the controlling expression of switch statement shall have integer type");
            }

            /// Integer promotions are performed on the controlling expression.
            e = e.IntPromote();

            env.PushSwitch(this);

            /// Semantic check the statment.
            AST.Stmt s = stmt.ToAST(env);

            env.PopBreakable();

            return new AST.Switch(breakLabel, cases, defaultLabel, e, s);
        }

        /// <summary>
        /// For case and default statement.
        /// </summary>
        public LinkedList<Tuple<string, AST.ConstIntExpr>> cases;
        public string defaultLabel;
        public AST.Expr e {
            get;
            private set;
        }

        public readonly Expr expr;
        public readonly Stmt stmt;

        private readonly Position pos;
    }

    public abstract class Loop : Breakable {
        public string continueLabel;
    }

    public sealed class While : Loop, IEquatable<While> {

        public While(Expr expr, Stmt statement) {
            this.expr = expr;
            this.statement = statement;
        }

        public override Position Pos => expr.Pos;

        public override bool Equals(object obj) {
            return Equals(obj as While);
        }

        public bool Equals(While x) {
            return x != null
                && x.expr.Equals(expr)
                && x.statement.Equals(statement);
        }

        public override int GetHashCode() {
            return expr.GetHashCode() | statement.GetHashCode();
        }

        public readonly Expr expr;
        public readonly Stmt statement;
    }

    public sealed class Do : Loop, IEquatable<Do> {

        public Do(Expr expr, Stmt statement) {
            this.expr = expr;
            this.statement = statement;
        }

        public override Position Pos => expr.Pos;

        public override bool Equals(object obj) {
            Do x = obj as Do;
            return Equals(x);
        }

        public bool Equals(Do x) {
            return x != null
                && x.expr.Equals(expr)
                && x.statement.Equals(statement);
        }

        public override int GetHashCode() {
            return expr.GetHashCode() | statement.GetHashCode();
        }

        public readonly Expr expr;
        public readonly Stmt statement;
    }

    public sealed class For : Loop, IEquatable<For> {

        public For(
            int line,
            Expr init,
            Expr pred,
            Expr iter,
            Stmt statement) {
            this.pos = new Position { line = line };
            this.init = init;
            this.pred = pred;
            this.iter = iter;
            this.statement = statement;
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as For);
        }

        public bool Equals(For x) {
            return x != null
                && x.pos.Equals(pos)
                && NullableEquals(x.init, init)
                && NullableEquals(x.pred, pred)
                && NullableEquals(x.iter, iter)
                && x.statement.Equals(statement);
        }

        public override int GetHashCode() {
            return statement.GetHashCode();
        }

        private readonly Position pos;
        public readonly Expr init;
        public readonly Expr pred;
        public readonly Expr iter;
        public readonly Stmt statement;
    }

    public sealed class Continue : Stmt, IEquatable<Continue> {

        public Continue(
            int line
            ) {
            this.pos = new Position { line = line };
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as Continue);
        }

        public bool Equals(Continue x) {
            return x != null && x.pos.Equals(pos);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        private readonly Position pos;
    }

    public sealed class Break : Stmt, IEquatable<Break> {

        public Break(
            int line
            ) {
            this.pos = new Position { line = line };
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as Break);
        }

        public bool Equals(Break x) {
            return x != null && x.pos.Equals(pos);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        private readonly Position pos;
    }

    public sealed class Return : Stmt, IEquatable<Return> {

        public Return(
            int line,
            Expr expr
            ) {
            this.pos = new Position { line = line };
            this.expr = expr;
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as Return);
        }

        public bool Equals(Return x) {
            return x != null && x.pos.Equals(pos)
                && x.expr == null ? expr == null : x.expr.Equals(expr);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        private readonly Position pos;
        public readonly Expr expr;
    }

    public sealed class Goto : Stmt, IEquatable<Goto> {

        public Goto(
            int line,
            Id label
            ) {
            this.pos = new Position { line = line };
            this.label = label;
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as Goto);
        }

        public bool Equals(Goto x) {
            return x != null && x.pos.Equals(pos)
                && x.label.Equals(label);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        private readonly Position pos;
        public readonly Id label;
    }

    public sealed class VoidStmt : Stmt {

        public VoidStmt(int line) {
            this.pos = new Position { line = line };
        }

        public override Position Pos => pos;

        public override bool Equals(object obj) {
            return Equals(obj as VoidStmt);
        }

        public bool Equals(VoidStmt x) {
            return x != null && x.pos.Equals(pos);
        }

        public override int GetHashCode() {
            return Pos.GetHashCode();
        }

        public override AST.Stmt ToAST(Env env) {
            return AST.VoidStmt.Instance;
        }

        private readonly Position pos;
    }
}
