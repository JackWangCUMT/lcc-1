﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcc.TypeSystem;

namespace lcc.AST {

    /// <summary>
    /// The base class for all expression.
    /// </summary>
    public abstract class ASTExpr : ASTStatement {

        public override bool Equals(object obj) {
            return obj is ASTExpr;
        }

        public bool Equals(ASTExpr expr) {
            return true;
        }

        public override int GetHashCode() {
            return GetLine();
        }

        public virtual T TypeCheck(ASTEnv env) {
            throw new NotImplementedException();
        }
    }
}
