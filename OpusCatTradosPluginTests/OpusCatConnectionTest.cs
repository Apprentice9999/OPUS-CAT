﻿using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpusCatTranslationProvider;

namespace OpusCatTradosPluginTests
{
    [TestClass]
    public class OpusCatConnectionTest
    {
        [TestMethod]
        public void PreOrderTest()
        {
            var connection = new OpusCatMtServiceConnection();

            var preOrderStatus = connection.PreOrderBatch(
                "localhost", "8500",
                new List<string>() { "This is a test" }, "en", "fi", "");

            Assert.AreEqual(HttpStatusCode.OK, preOrderStatus);
        }

        [TestMethod]
        public void ListLanguagePairsTest()
        {
            var connection = new OpusCatMtServiceConnection();

            var languagePairs = connection.ListSupportedLanguages(
                "localhost", "8500");

            Assert.IsTrue(languagePairs.Count > 0);
        }

        [TestMethod]
        public void CheckModelStatusTest()
        {
            var connection = new OpusCatMtServiceConnection();

            var modelStatus = connection.CheckModelStatus(
                "localhost", "8500","en","fi","");

            Assert.IsTrue(modelStatus != null);
        }
    }
}
