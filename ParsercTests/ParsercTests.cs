﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parserc;
using Parserc.PChar;
using static Parserc.Parserc;
using Parserc.Examples;

namespace ParsercTests {
    [TestClass]
    public class ParsercTests {

        [TestMethod]
        public void parserc_recursive() {
            Func<int, int> fib = Recursion.Y<int, int>(
                f => n => n > 1 ? f(n - 1) + f(n - 2) : n
                );
            Func<int, int> fact = Recursion.Y<int, int>(
                f => n => n > 1 ? n * f(n - 1) : n
                );
            Assert.AreEqual(8, fib(6));
            Assert.AreEqual(720, fact(6));
        }


        [TestMethod]
        public void parserc_item() {
            Parser<char, char> parser = Item<char>();
            string src = "abcdef90886/,,';";
            ITokenStream<char> tokens = new CharStream(src);
            for (int i = 0; i < src.Length; ++i) {
                var result = parser(tokens);
                Assert.AreEqual(result.Count, 1);
                Assert.AreEqual(result[0].value, src[i]);
                tokens = result[0].remain;
            }
        }

        [TestMethod]
        public void parserc_result() {
            Parser<char, char> parser = Result<char, char>('o');
            string src = "abcdef90886/,,';";
            ITokenStream<char> tokens = new CharStream(src);
            for (int i = 0; i < src.Length; ++i) {
                var result = parser(tokens);
                Assert.AreEqual(result.Count, 1);
                Assert.AreEqual(result[0].value, 'o');
                Assert.AreEqual(result[0].remain.Copy().Next(), 'a');
                tokens = result[0].remain;
            }
        }

        [TestMethod]
        public void parserc_zero() {
            Parser<char, char> parser = Zero<char, char>();
            string src = "abcdef90886/,,';";
            ITokenStream<char> tokens = new CharStream(src);
            for (int i = 0; i < src.Length; ++i) {
                var result = parser(tokens);
                Assert.AreEqual(result.Count, 0);
            }
        }

        [TestMethod]
        public void parserc_sat() {
            Parser<char, char> parser = Sat<char>(c => c == 'b');
            string src = "bbbabc";
            ITokenStream<char> tokens = new CharStream(src);
            for (int i = 0; i < src.Length; ++i, tokens.Next()) {
                var result = parser(tokens);
                if (src[i] != 'b') {
                    Assert.AreEqual(result.Count, 0);
                } else {
                    Assert.AreEqual(result.Count, 1);
                    Assert.AreEqual(result[0].value, 'b');
                }
            }
        }

        [TestMethod]
        public void parserc_bind() {
            Parser<char, char[]> parser =
                CharParser.Lower().Bind(x => { return
                CharParser.Lower().Bind(y => { return
                Result<char, char[]>(new char[] { x, y }); });});
            string src = "abc";
            ITokenStream<char> tokens = new CharStream(src);
            var result = parser(tokens);
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].value.Length, 2);
            Assert.AreEqual(result[0].value[0], 'a');
            Assert.AreEqual(result[0].value[1], 'b');
            Assert.AreEqual(result[0].remain.Next(), 'c');
        }

        [TestMethod]
        public void parserc_word() {
            Parser<char, string> parser = CharParser.Word();
            string src = "abc";
            ITokenStream<char> tokens = new CharStream(src);
            var result = parser(tokens);
            Assert.AreEqual(result.Count, 4);
            Assert.AreEqual(result[0].value, "abc");
            Assert.AreEqual(result[1].value, "ab");
            Assert.AreEqual(result[2].value, "a");
            Assert.AreEqual(result[3].value, "");
        }

        [TestMethod]
        public void parserc_natural() {
            Parser<char, uint> parser = CharParser.Natural();
            string src = "629abc";
            ITokenStream<char> tokens = new CharStream(src);
            var result = parser(tokens);
            Assert.AreEqual(result.Count, 3);
            Assert.AreEqual(result[0].value, 629u);
            Assert.AreEqual(result[1].value, 62u);
            Assert.AreEqual(result[2].value, 6u);
        }

        [TestMethod]
        public void parserc_integer() {
            Parser<char, int> parser = CharParser.Integer();
            string src = "-629abc";
            ITokenStream<char> tokens = new CharStream(src);
            var result = parser(tokens);
            Assert.AreEqual(result.Count, 3);
            Assert.AreEqual(result[0].value, -629);
            Assert.AreEqual(result[1].value, -62);
            Assert.AreEqual(result[2].value, -6);
        }

        [TestMethod]
        public void parserc_integer_list() {
            Parser<char, LinkedList<int>> parser = CharParser.IntegerList();
            string src = "{1,2,3,4,5}";
            ITokenStream<char> tokens = new CharStream(src);
            var result = parser(tokens);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(5, result[0].value.Count);
            {
                int i = 1;
                foreach (int r in result[0].value) {
                    Assert.AreEqual(i, r);
                    i++;
                }
            }

            string empty = "{}";
            tokens = new CharStream(empty);
            result = parser(tokens);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].value.Count);
        }

        [TestMethod]
        public void parserc_arithmetic_legal() {

            Dictionary<string, int> srcs = new Dictionary<string, int> {
                { "1+2", 3 },
                { "4^5", 1024 },
                { "1-5", -4 },
                { "1--5", 6 },
                { "(1+1)^10", 1024 },
                { "1+2^3", 9 },
                { "1+2+3", 6 }
            };

            foreach (var test in srcs) {
                Assert.AreEqual(test.Value, Arithmetic.Eval(test.Key));
            }
        }
    }
}