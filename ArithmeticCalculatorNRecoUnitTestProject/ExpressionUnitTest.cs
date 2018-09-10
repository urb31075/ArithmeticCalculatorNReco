// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionUnitTest.cs" company="urb31075">
//  All Right Reserved 
// </copyright>
// <summary>
//   Defines the Zopa type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ArithmeticCalculatorNRecoUnitTestProject
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NReco.Linq;

    /// <summary>
    /// The expression unit test.
    /// </summary>
    [TestClass]
    public class ExpressionUnitTest
    {

        /// <summary>
        /// The func test 4.
        /// </summary>
        [TestMethod]
        public void FuncTestNReco4()
        {
            var lambdaParser = new LambdaParser();
            var varContext = new Dictionary<string, object>
                   {
                       ["A"] = 10,
                       ["B"] = 12
                   };
            var result = lambdaParser.Eval("A * (B + 10)", varContext);
            Console.WriteLine(result);
        }

        /// <summary>
        /// The eval cache perf.
        /// </summary>
        [TestMethod]
        public void EvalCachePerfNReco()
        {
            decimal result = decimal.MinValue;
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 10000; i++)
            {
                var varContext = new Dictionary<string, object>
                    {
                        ["a"] = 55,
                        ["b"] = 2
                    };
                var lambdaParser = new LambdaParser();
                result = (decimal)lambdaParser.Eval("(a*2 + 100)/b", varContext);
            }

            sw.Stop();
            Console.WriteLine("10000 iterations: {0}", sw.Elapsed);
            Assert.AreEqual(105M, result);
        }

        /// <summary>
        /// The parse perf.
        /// </summary>
        [TestMethod]
        public void ParsePerfNReco()
        {
            var paramList = new List<string>();
            var lambdaParser = new LambdaParser();
            var exp = "(a*2 + 100)/b + suka + testObj.Dupel((d + s)*(x + y))";
            var parseResult = lambdaParser.Parse(exp);
            this.ParamFind(parseResult, ref paramList);
            paramList.ForEach(Console.WriteLine);

            var varContext = new Dictionary<string, object> { ["testObj"] = new TestClass() };
            foreach (var prm in paramList)
            {
                if (prm.Contains("testObj"))
                {
                    continue;
                }

                varContext.Add(prm, 10);
            }

            var result = (decimal)lambdaParser.Eval(exp, varContext);
            Console.WriteLine(result);
        }

        /// <summary>
        /// The method call.
        /// </summary>
        [TestMethod]
        public void MethodCall()
        {
            MethodInfo method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            Assert.IsNotNull(method);
            var target = Expression.Parameter(typeof(string), "x");
            var methodArg = Expression.Parameter(typeof(string), "y");
            Expression[] methodArgs = { methodArg };

            var call = Expression.Call(target, method, methodArgs);
            var lambdaParameters = new[] { target, methodArg };
            var lambda = Expression.Lambda<Func<string, string, bool>>(call, lambdaParameters);
            var compiled = lambda.Compile();
            Console.WriteLine(compiled("123456", "789"));
            Console.WriteLine(compiled("123456", "123"));
        }

        [TestMethod]
        public void LambdaCall()
        {
            Expression<Func<string, string, bool>> lambda = (x, y) => x.StartsWith(y);
            var compiled = lambda.Compile();
            Console.WriteLine(compiled("123456", "789"));
            Console.WriteLine(compiled("123456", "123"));
        }

        /// <summary>
        /// The expr.
        /// </summary>
        [TestMethod]
        public void Expr0()
        {
            var first = Expression.Constant(2);
            var second = Expression.Constant(3);
            var add = Expression.Add(first, second);
            Console.WriteLine(add);
            var lambd = Expression.Lambda(add);
            Console.WriteLine(lambd);
            var comp1 = (Func<int>)lambd.Compile();
            var comp2 = Expression.Lambda<Func<int>>(add).Compile();
            Console.WriteLine("{0}", comp1());
            Console.WriteLine("{0}", comp2());
        }

        /// <summary>
        /// The expr 1.
        /// </summary>
        [TestMethod]
        public void Expr1()
        {
            Expression<Func<int, int, int>> ret = (x, y) => x + y;
            var comp = ret.Compile();
            Console.WriteLine("{0}", comp(2, 5));
        }

        /// <summary>
        /// The func test.
        /// </summary>
        [TestMethod]
        public void FuncTest0()
        {
            var n = Expression.Parameter(typeof(int), "n");
            var expr = Expression.Lambda<Func<int, int>>(Expression.Add(n, Expression.Constant(1)), n);
            Func<int, int> func = expr.Compile();
            for (int i = 0; i < 10; ++i)
            {
                Console.WriteLine(func(i));
            }
        }

        /// <summary>
        /// The func test 1.
        /// </summary>
        [TestMethod]
        public void FuncTest1()
        {
            const string Exp = @"(Person.Age * Person.Weight + 10)";
            var p = Expression.Parameter(typeof(Person), "Person");
            var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] { p }, null, Exp);
            var bob = new Person { Name = "Bob", Age = 30, Weight = 90, FavouriteDay = new DateTime(2000, 1, 1) };
            var result = e.Compile().DynamicInvoke(bob);
            Console.WriteLine(result);
        }

        /// <summary>
        /// The func test 2.
        /// </summary>
        [TestMethod]
        public void FuncTest2()
        {
            const string Exp = @"(A + B + 10 + Math.Sqrt(100))";
            var paramName = new[] { Expression.Parameter(typeof(int), "A"), Expression.Parameter(typeof(int), "B") };

            var paramValue = new int[2];
            paramValue[0] = 10;
            paramValue[1] = 20;

            var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(paramName, null, Exp, 1, 6);
            var result = e.Compile().DynamicInvoke(1, 6);
            Console.WriteLine(result);

            /*Func<int, int, double> func = (Func<int, int, double>)e.Compile(); //.DynamicInvoke();//  (10, 20);
            var x = func(10, 20);
            Console.WriteLine(x);*/
        }

        /// <summary>
        /// The func test 3.
        /// </summary>
        [TestMethod]
        public void FuncTest3()
        {
            const string Exp = @"(A +  B + C + @0 + @1 + 10 + Math.Sqrt(100))";

            var pn = new[]
                         {
                             Expression.Parameter(typeof(int), "A"), Expression.Parameter(typeof(int), "B"),
                             Expression.Parameter(typeof(int), "C")
                         };
            var pv = new[] { 10, 20, 30 };

            var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(pn, typeof(double), Exp, 1, 6);
            double result = double.NaN;
            switch (pn.Length)
            {
                case 0:
                    result = (double)e.Compile().DynamicInvoke();
                    break;
                case 1:
                    result = (double)e.Compile().DynamicInvoke(pv[0]);
                    break;
                case 2:
                    result = (double)e.Compile().DynamicInvoke(pv[0], pv[1]);
                    break;
                case 3:
                    result = (double)e.Compile().DynamicInvoke(pv[0], pv[1], pv[2]);
                    break;
            }

            Console.WriteLine(result);
        }

        /// <summary>
        /// The param find.
        /// </summary>
        /// <param name="exp">
        /// The exp.
        /// </param>
        /// <param name="paramList">
        /// The param list.
        /// </param>
        private void ParamFind(Expression exp, ref List<string> paramList)
        {
            var done = false;
            var binResult = exp as BinaryExpression;
            if (binResult != null)
            {
                done = true;
                this.ParamFind(binResult.Left, ref paramList);
                this.ParamFind(binResult.Right, ref paramList);
            }

            var paramResult = exp as ParameterExpression;
            if (paramResult != null)
            {
                done = true;
                paramList.Add(paramResult.Name);
            }

            var callResult = exp as MethodCallExpression;
            if (callResult != null)
            {
                done = true;
                foreach (var arg in callResult.Arguments)
                {
                    this.ParamFind(arg, ref paramList);
                }
            }

            var arrayResult = exp as NewArrayExpression;
            if (arrayResult != null)
            {
                done = true;
                foreach (var arg in arrayResult.Expressions)
                {
                    this.ParamFind(arg, ref paramList);
                }
            }

            var constResult = exp as ConstantExpression;
            if (constResult != null)
            {
                done = true;
            }

            if (!done)
            {
                throw new Exception("Не обработанный Expression " + exp.NodeType);
            }
        }

        /// <summary>
        /// The multiadder.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int Multiadder(params int[] parameters)
        {
            return parameters.Sum();
        }

        public void TestMultiadder()
        {
            Console.WriteLine(this.Multiadder(1, 2, 3));
            Console.WriteLine(this.Multiadder(1, 2, 3, 4, 5));
        }

        static void PrintConvertedValue<TInput, TOutput>(TInput input, Converter<TInput, TOutput> converter)
        {
            var container = new List<TOutput>
                                {
                                    converter(input),
                                    converter(input)
                                };
            foreach (var cnt in container)
            {
                Console.WriteLine(cnt);
            }
        }

        /// <summary>
        /// The dupel.
        /// </summary>
        /// <param name="p1">
        /// The p 1.
        /// </param>
        /// <param name="p2">
        /// The p 2.
        /// </param>
        /// <param name="p3">
        /// The p 3.
        /// </param>
        /// <typeparam name="T1">
        /// </typeparam>
        /// <typeparam name="T2">
        /// </typeparam>
        /// <typeparam name="T3">
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string Dupel<T1, T2, T3>(T1 p1, T2 p2, T3 p3)
        {
            return $"{p1.GetType()} {p2.GetType()} {p3.GetType()}";
        }

        /// <summary>
        /// The test multiadder.
        /// </summary>
        [TestMethod]
        public void TestPrintConvertedValue()
        {
            Console.WriteLine(Dupel(1, 3M, "sdf"));
            PrintConvertedValue("this is string", x => x.Length);
            PrintConvertedValue(3, x => "x = " + x.ToString());
            PrintConvertedValue(10M, x => $"dec = {x}");
        }

        /// <summary>
        /// The test class.
        /// </summary>
        public class TestClass
        {
            /// <summary>
            /// The dupel.
            /// </summary>
            /// <param name="s">
            /// The s.
            /// </param>
            /// <returns>
            /// The <see cref="decimal"/>.
            /// </returns>
            public decimal Dupel(decimal s)
            {
                return 5 * s;
            }
        }

        /// <summary>
        /// The person.
        /// </summary>
        public class Person
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the age.
            /// </summary>
            public int Age { get; set; }

            /// <summary>
            /// Gets or sets the weight.
            /// </summary>
            public int Weight { get; set; }

            /// <summary>
            /// Gets or sets the favourite day.
            /// </summary>
            public DateTime FavouriteDay { get; set; }
        }
    }
}
