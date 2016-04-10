﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lcc.TypeSystem {
    public sealed class TFunc : TUnqualified {

        public TFunc(T ret, IEnumerable<T> parameters) {
            this.ret = ret;
            this.parameters = parameters;
        }

        public override bool IsFunc => true;
        public override bool IsComplete => true;
        public override int Size { get { throw new InvalidOperationException("Can't take sizeof func designator!"); } } 

        public override TUnqualified Composite(TUnqualified other) {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) {
            return Equals(obj as TFunc);
        }

        public bool Equals(TFunc t) {
            return t != null && t.ret.Equals(ret) && t.parameters.SequenceEqual(parameters);
        }

        public override int GetHashCode() {
            return ret.GetHashCode();
        }

        public override string ToString() {
            string paramsStr = "";
            foreach (var param in parameters) {
                paramsStr = string.Format("{0}{1}, ", paramsStr, param);
            }
            return string.Format("(({1}) -> {0})", ret, paramsStr);
        }

        public readonly T ret;
        public readonly IEnumerable<T> parameters;
    }
}
