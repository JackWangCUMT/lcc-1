﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using lcc.TypeSystem;

namespace LC_CompilerTests {
    [TestClass]
    public class TypeTest {

        [TestMethod]
        public void LCCIntPromote() {

            var tests = new Dictionary<TUnqualified, string> {
                {
                    TChar.Instance,
                    "int"
                },
                {
                    TSChar.Instance,
                    "int"
                },
                {
                    TUChar.Instance,
                    "int"
                },
                {
                    TShort.Instance,
                    "int"
                },
                {
                    TUShort.Instance,
                    "int"
                },
                {
                    TInt.Instance,
                    "int"
                },
                {
                    TUInt.Instance,
                    "unsigned int"
                },
                {
                    TLong.Instance,
                    "long"
                },
                {
                    TULong.Instance,
                    "unsigned long"
                },
                {
                    TLLong.Instance,
                    "long long"
                },
                {
                    TULLong.Instance,
                    "unsigned long long"
                },
                {
                    TSingle.Instance,
                    "float"
                },
                {
                    TDouble.Instance,
                    "double"
                },
                {
                    TBool.Instance,
                    "int"
                },
            };

            foreach (var test in tests) {
                System.Console.WriteLine(test.Key);
                Assert.AreEqual(test.Value, test.Key.IntPromote().ToString());
            }
        }

        [TestMethod]
        public void LCCTypeToChar() {

            var tests = new Dictionary<TUnqualified, string> {
                {
                    TChar.Instance,
                    "char"
                },
                {
                    TSChar.Instance,
                    "signed char"
                },
                {
                    TUChar.Instance,
                    "unsigned char"
                },
                {
                    TVoid.Instance,
                    "void"
                },
                {
                    TShort.Instance,
                    "short"
                },
                {
                    TUShort.Instance,
                    "unsigned short"
                },
                {
                    TInt.Instance,
                    "int"
                },
                {
                    TUInt.Instance,
                    "unsigned int"
                },
                {
                    TLong.Instance,
                    "long"
                },
                {
                    TULong.Instance,
                    "unsigned long"
                },
                {
                    TLLong.Instance,
                    "long long"
                },
                {
                    TULLong.Instance,
                    "unsigned long long"
                },
                {
                    TSingle.Instance,
                    "float"
                },
                {
                    TDouble.Instance,
                    "double"
                },
                {
                    TBool.Instance,
                    "_Bool"
                },
                {
                    new TCArr(TChar.Instance.None(), 3),
                    "(char)[3]"
                }
            };

            foreach (var test in tests) {
                System.Console.WriteLine(test.Key);
                Assert.AreEqual(test.Value, test.Key.ToString());
            }
        }
    }
}
