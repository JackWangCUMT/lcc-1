﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lcc.AST {
    public abstract class ASTNode {
        protected ASTNode() { }

        public abstract int GetLine();
    }
}
