﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using lcc.SyntaxTree;
using lcc.Token;
using lcc.Parser;
using Parserc;

namespace LC_CompilerTests {
    public partial class ParserTests {

        [TestMethod]
        public void LCCParserLabeledStatement() {
            var tests = new Dictionary<string, STLabeled> {
                {
                    "FOO: x = x + 2;",
                    new STLabeled(
                        new Id(new T_IDENTIFIER(1, "FOO")),
                        new STAssignExpr(
                            new Id(new T_IDENTIFIER(1, "x")),
                            new BiExpr(
                                new Id(new T_IDENTIFIER(1, "x")),
                                new ConstInt(new T_CONST_INT(1, "2", 10)),
                                BiExpr.Op.PLUS),
                            STAssignExpr.Op.ASSIGN))
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.LabeledStatement().End(), test.Value);
            }
        }

        [TestMethod]
        public void LCCParserCaseStatement() {
            var tests = new Dictionary<string, STCase> {
                {
                    "case 1 : a;",
                    new STCase(
                        new ConstInt(new T_CONST_INT(1, "1", 10)),
                        new Id(new T_IDENTIFIER(1, "a")))
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.CaseStatement().End(), test.Value);
            }
        }

        [TestMethod]
        public void LCCParserDefaultStatement() {
            var tests = new Dictionary<string, STDefault> {
                {
                    "default: a;",
                    new STDefault(new Id(new T_IDENTIFIER(1, "a")))
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.DefaultStatement().End(), test.Value);
            }
        }

        [TestMethod]
        public void LCCParserCompoundStatement() {
            var tests = new Dictionary<string, CompoundStmt> {
                {
                    @"
{
    a;
    int x;
}",
                    new CompoundStmt(
                        new List<Stmt> {
                            new Id(new T_IDENTIFIER(3, "a")),
                            new Declaration(
                                new DeclSpecs(
                                    new List<DeclSpec> {
                                        new TypeKeySpec(4, TypeSpec.Kind.INT)
                                    },
                                    STStoreSpec.Kind.NONE,
                                    new List<TypeSpec.Kind> {
                                        TypeSpec.Kind.INT
                                    }),
                                new List<InitDeclarator> {
                                    new InitDeclarator(
                                        new Declarator(
                                            new List<Ptr>(),
                                            new IdDeclarator(new Id(new T_IDENTIFIER(4, "x")))))
                                }),
                        })
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.CompoundStatement(), test.Value);
            }
        }

        [TestMethod]
        public void LCCParserExpressionStatement() {
            var tests = new Dictionary<string, Stmt> {
                {
                    ";",
                    new STVoidStmt(1)
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.ExpressionStatement(), test.Value);
            }
        }

        [TestMethod]
        public void LCCParserIfStatement() {
            var tests = new Dictionary<string, STIf> {
                {
                    @"
if (i < length)
    x++;",
                    new STIf(
                        2,
                        new BiExpr(
                            new Id(new T_IDENTIFIER(2, "i")),
                            new Id(new T_IDENTIFIER(2, "length")),
                            BiExpr.Op.LT),
                        new PostStep(
                            new Id(new T_IDENTIFIER(3, "x")),
                            PostStep.Kind.INC),
                        null)
                },
                {
                    @"
if (i < length)
    x++;
else
    x--;",
                    new STIf(
                        2,
                        new BiExpr(
                            new Id(new T_IDENTIFIER(2, "i")),
                            new Id(new T_IDENTIFIER(2, "length")),
                            BiExpr.Op.LT),
                        new PostStep(
                            new Id(new T_IDENTIFIER(3, "x")),
                            PostStep.Kind.INC),
                        new PostStep(
                            new Id(new T_IDENTIFIER(5, "x")),
                            PostStep.Kind.DEC))
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.IfStatement(), test.Value);
            }
        }

        [TestMethod]
        public void LCCParserSwitchStatement() {
            var tests = new Dictionary<string, STSwitch> {
                {
                    @"
switch (x) {
case 0: a;
default: c;
}",
                    new STSwitch(
                        2,
                        new Id(new T_IDENTIFIER(2, "x")),
                        new CompoundStmt(new LinkedList<Stmt>(new List<Stmt> {
                            new STCase(
                                new ConstInt(new T_CONST_INT(3, "0", 8)),
                                new Id(new T_IDENTIFIER(3, "a"))),
                            new STDefault(
                                new Id(new T_IDENTIFIER(4, "c"))) })))
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.SwitchStatement().End(), test.Value);
            }
        }

        [TestMethod]
        public void LCCParserWhileStatement() {
            var tests = new Dictionary<string, STWhile> {
                {
                    @"
while (x < length) {
    x++;
}",
                    new STWhile(
                        new BiExpr(
                            new Id(new T_IDENTIFIER(2, "x")),
                            new Id(new T_IDENTIFIER(2, "length")),
                            BiExpr.Op.LT),
                        new CompoundStmt(
                            new List<Stmt> {
                                new PostStep(
                                    new Id(new T_IDENTIFIER(3, "x")),
                                    PostStep.Kind.INC)
                            }))
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.WhileStatement(), test.Value);
            }
        }

        [TestMethod]
        public void LCCParserDoStatement() {
            var tests = new Dictionary<string, STDo> {
                {
                    @"
do {
    x++;
} while (x < length);",
                    new STDo(
                        new BiExpr(
                            new Id(new T_IDENTIFIER(4, "x")),
                            new Id(new T_IDENTIFIER(4, "length")),
                            BiExpr.Op.LT),
                        new CompoundStmt(
                            new List<Stmt> {
                                new PostStep(
                                    new Id(new T_IDENTIFIER(3, "x")),
                                    PostStep.Kind.INC)
                            }))
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.DoStatement(), test.Value);
            }
        }

        [TestMethod]
        public void LCCParserForStatement() {
            var tests = new Dictionary<string, STFor> {
                {
                    @"
for (i = 0; i < length; ++i) {
    x += i;
}",
                    new STFor(
                        2,
                        new STAssignExpr(
                            new Id(new T_IDENTIFIER(2, "i")),
                            new ConstInt(new T_CONST_INT(2, "0", 8)),
                            STAssignExpr.Op.ASSIGN),
                        new BiExpr(
                            new Id(new T_IDENTIFIER(2, "i")),
                            new Id(new T_IDENTIFIER(2, "length")),
                            BiExpr.Op.LT),
                        new PreStep(
                            new Id(new T_IDENTIFIER(2, "i")),
                            PreStep.Kind.INC),
                        new CompoundStmt(
                            new List<Stmt> {
                                new STAssignExpr(
                                    new Id(new T_IDENTIFIER(3, "x")),
                                    new Id(new T_IDENTIFIER(3, "i")),
                                    STAssignExpr.Op.PLUSEQ)
                            }))
                },
                {
                    @"
for (;1;) ;",
                    new STFor(
                        2,
                        null,
                        new ConstInt(new T_CONST_INT(2, "1", 10)),
                        null,
                        new STVoidStmt(2))
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.ForStatement(), test.Value);
            }
        }

        [TestMethod]
        public void LCCParserJumpStatement() {
            var tests = new Dictionary<string, Stmt> {
                {
                    "continue;",
                    new STContinue(1)
                },
                {
                    "break;",
                    new STBreak(1)
                },
                {
                    "goto foo;",
                    new STGoto(1, new Id(new T_IDENTIFIER(1, "foo")))
                },
                {
                    "return 0;",
                    new STReturn(1, new ConstInt(new T_CONST_INT(1, "0", 8)))
                }
            };

            foreach (var test in tests) {
                Aux(test.Key, Parser.JumpStatement(), test.Value);
            }
        }
    }
}