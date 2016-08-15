using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppInventory.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryUserMachineSyncAppInventoryTests.Controllers;
using System.Web.Mvc;

namespace AppInventory.Controllers.AppInventoryTests
{
    [TestClass()]
    public class AppCIControllerTests
    {
        [TestMethod()]
        public void IndexTest()
        {
            var appCI = new AppCIController();
            var result = appCI.Index(string.Empty, string.Empty, string.Empty, null, null, null, string.Empty, null, string.Empty, string.Empty, string.Empty, null, string.Empty, null, null) as ActionResult;
            Assert.AreEqual("Index", result);
        }

        [TestMethod()]
        public void DetailsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UsersTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void EditTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void EditTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteConfirmedTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ChangeLogTest()
        {
            Assert.Fail();
        }
    }
}
