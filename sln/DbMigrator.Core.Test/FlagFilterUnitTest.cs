using System;
using System.Collections;
using System.Linq;
using DbMigrator.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbMigrator.Core.Test
{
    [TestClass]
    public class FlagFilterUnitTest
    {

        

        #region complex 1

        [TestMethod]
        public void ComplexFilter1_1()
        {
            var filter = new FlagFilter("!test&(v1|v2)");
        }

        [TestMethod]
        public void ComplexFilter1_2()
        {
            var filter = new FlagFilter("!test&((v1)|(v2))");
        }

        [TestMethod]
        public void ComplexFilter1_3()
        {
            var filter = new FlagFilter("!(test)&((v1)|(v2))");
        }

        public void Complex1BateryTest(FlagFilter filter)
        {
            //v1 or v2, never in test
            Assert.IsFalse(filter.Test(new string[] { "test" }));
            Assert.IsFalse(filter.Test(new string[] { "test", "v1" }));
            Assert.IsFalse(filter.Test(new string[] { "v3" }));
            Assert.IsTrue(filter.Test(new string[] { "v1" }));
            Assert.IsTrue(filter.Test(new string[] { "v2","production" }));
        }
        #endregion

        #region complex 2

        [TestMethod]
        public void ComplexFilter2_1()
        {
            var filter = new FlagFilter("(meat|vegetable|fruit)&!rot&!muchSugar&!muchCarbo");
        }

        //[TestMethod]
        //public void ComplexFilter2_2()
        //{
        //    var filter = new FlagFilter("!test&((v1)|(v2))");
        //}

        //[TestMethod]
        //public void ComplexFilter2_3()
        //{
        //    var filter = new FlagFilter("!(test)&((v1)|(v2))");
        //}

        public void Complex2BateryTest(FlagFilter filter)
        {
            //meat, vegetable or fruit. never with muchSugar or muchCarbo. never rot
            Assert.IsFalse(filter.Test(new string[] { "rock" }));
            Assert.IsTrue(filter.Test(new string[] { "meat" }));
            Assert.IsFalse(filter.Test(new string[] { "meat", "rot" }));
            Assert.IsFalse(filter.Test(new string[] { "fruit", "muchSugar" }));
            Assert.IsTrue(filter.Test(new string[] { "fruit","meat","vegetable" }));
            Assert.IsFalse(filter.Test(new string[] { "fruit", "meat", "vegetable","rot" }));
        }
        #endregion

        #region carne only

        public void CarneBateryTest(FlagFilter filter)
        {
            Assert.IsFalse(filter.Test(new string[] { }));
            Assert.IsTrue(filter.Test(new string[] { "vegetal", "carne" }));
            Assert.IsTrue(filter.Test(new string[] { "carne" }));
            Assert.IsFalse(filter.Test(new string[] { "vegetal" }));
            Assert.IsFalse(filter.Test(new string[] { "vegetal", "tomate", "cebola" }));
        }

        [TestMethod]
        public void CarneFilter()
        {
            var filter = new FlagFilter("carne");
        }

        [TestMethod]
        public void CarneFilterWithWhiteSpaces()
        {
            var filter = new FlagFilter(" carne ");
            CarneBateryTest(filter);
        }

        [TestMethod]
        public void CarneFilterWithParenthesis1()
        {
            var filter = new FlagFilter("(carne)");
            CarneBateryTest(filter);
        }

        [TestMethod]
        public void CarneFilterWithParenthesis2()
        {
            var filter = new FlagFilter(" ( carne ) ");
            CarneBateryTest(filter);
        }

        [TestMethod]
        public void CarneFilterWithParenthesis3()
        {
            var filter = new FlagFilter("( ( ( carne ) ) )");
            CarneBateryTest(filter);
        }

        #endregion

        #region carne or vegetal

        public void CarneOrVegetalBateryTest(FlagFilter filter)
        {
            Assert.IsFalse(filter.Test(new string[] { }));
            Assert.IsTrue(filter.Test(new string[] { "vegetal", "carne" }));
            Assert.IsTrue(filter.Test(new string[] { "carne" }));
            Assert.IsTrue(filter.Test(new string[] { "vegetal" }));
            Assert.IsTrue(filter.Test(new string[] { "vegetal", "tomate", "cebola" }));
            Assert.IsFalse(filter.Test(new string[] { "tomate", "cebola" }));
        }

        [TestMethod]
        public void CarneOrVegetalFilter1()
        {
            var filter = new FlagFilter("carne|vegetal");
            CarneOrVegetalBateryTest(filter);
        }

        [TestMethod]
        public void CarneOrVegetalFilter2()
        {
            var filter = new FlagFilter(" carne | vegetal ");
            CarneOrVegetalBateryTest(filter);
        }

        [TestMethod]
        public void CarneOrVegetalFilter3()
        {
            var filter = new FlagFilter(" ( carne | vegetal ) ");
            CarneOrVegetalBateryTest(filter);
        }

        [TestMethod]
        public void CarneOrVegetalFilter4()
        {
            var filter = new FlagFilter(" ( ( carne | vegetal ) ) ");
            CarneOrVegetalBateryTest(filter);
        }

        #endregion

        #region InvalidExpressions

        [TestMethod]
        public void InvalidExpression1()
        {
            string[] invalidExpressions = new string[] {
                "!!tag1",
                "&tag1",
                "tag1&&tag2",
                "tag1||tag2",
                "|(tag1)", 
                "tag|", 
                "tag&", 
                "tag!",
                "(|tag)",
                "tag&(&tag)"
            };
            foreach (var item in invalidExpressions)
            {
                try
                {
                    var filter = new FlagFilter(item);
                    Assert.Fail(item);
                }
                catch (AssertFailedException ex)
                {
                    throw ex;
                }
                catch (ArgumentException ex)
                {
                    //this is ok
                }
                catch(Exception ex)
                {
                    //this is unexpected
                    Assert.Fail(string.Format("Unexpected exception type ({0}) for expression '{1}'.", ex.Message, item));
                }
            }
        }

        #endregion

        #region NotCarneBattery

        public void NotCarneBateryTest(FlagFilter filter)
        {
            var result = !(new string[] { }).Contains("carne");
            Assert.IsTrue(filter.Test(new string[] { }), "Empty");
            Assert.IsFalse(filter.Test(new string[] { "vegetal", "carne" }), "vegetal,carne");
            Assert.IsFalse(filter.Test(new string[] { "carne" }), "carne");
            Assert.IsTrue(filter.Test(new string[] { "vegetal" }), "vegetal");
            Assert.IsTrue(filter.Test(new string[] { "vegetal", "tomate", "cebola" }), "vegetal,tomate,cebola");
            Assert.IsTrue(filter.Test(new string[] { "tomate", "cebola" }), "tomate,cebola");
        }

        [TestMethod]
        public void NotCarneFilter1()
        {
            var filter = new FlagFilter("!carne");
            NotCarneBateryTest(filter);
        }

        [TestMethod]
        public void NotCarneFilter2()
        {
            var filter = new FlagFilter(" ! carne ");
            NotCarneBateryTest(filter);
        }

        [TestMethod]
        public void NotCarneFilter3()
        {
            var filter = new FlagFilter(" ! ( carne ) ");
            NotCarneBateryTest(filter);
        }

        [TestMethod]
        public void NotCarneFilter4()
        {
            var filter = new FlagFilter(" ( ! ( carne ) ) ");
            NotCarneBateryTest(filter);
        }

        [TestMethod]
        public void NotCarneFilter5()
        {
            var filter = new FlagFilter(" (( ! ( carne ) )) ");
            NotCarneBateryTest(filter);
        }

        #endregion
    }
}
