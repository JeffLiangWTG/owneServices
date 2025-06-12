using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Tests.Fakes;
using Common.Logging;
using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using System.Collections.Specialized;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
    [TestClass]
    public class TransformationSetTests : BaseControllerTest<TransformationSetController>
    {
        [TestMethod]
        public void TransformationSet_TransformationTypes_Add()
        {
            var logger = new TestLogger();
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;

            var tsPK = new Guid("00000000-0000-0000-0000-000000000000");
            var ttPK1 = new Guid("11111111-1111-1111-1111-111111111111");
            var ttPK2 = new Guid("22222222-2222-2222-2222-222222222222");

            context.eHubTransformationSets.AddObject(new eHubTransformationSet { TS_PK = tsPK });
            context.eHubTransformationTypes.AddObject(new eHubTransformationType { TT_PK = ttPK1, TT_TransformationType = "" });
            context.eHubTransformationTypes.AddObject(new eHubTransformationType { TT_PK = ttPK2, TT_TransformationType = "" });

            var formValues = new FormCollection();
            formValues["TransformationSet.TS_Name"] = "";
            formValues["CNameSender"] = "";
            formValues["CNameRecipient"] = "";
            formValues["TId"] = "";
            formValues["TName"] = "";
            formValues["TransformationSet.TS_BillSender"] = "false";
            formValues["TransformationSet.TS_BillRecipient"] = "false";
            formValues["CNameBillOther"] = "";
            formValues["TransformationSet.TS_BillingNumMessagesIncluded"] = "";
            formValues["TransformationSet.TS_BillingFee"] = "";
            formValues["MId0"] = ttPK1.ToString();
            formValues["MId1"] = ttPK2.ToString();
            controller.ValueProvider = formValues.ToValueProvider();

            var result = EditTest(tsPK, formValues, logger);

            CollectionAssert.AreEqual(new List<Tuple<Guid, byte, Guid>>
            { 
                new Tuple<Guid, byte, Guid>(tsPK, 0, ttPK1),
                new Tuple<Guid, byte, Guid>(tsPK, 1, ttPK2),
            }, context.eHubTransformationMappings.Select(m => new Tuple<Guid, byte, Guid>(m.TM_TS_PK, m.TM_Order, m.TM_TT_PK)).ToList());
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

        [TestMethod]
        public void TransformationSet_TransformationTypes_Update()
        {
            var logger = new TestLogger();
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;

            var tsPK = new Guid("00000000-0000-0000-0000-000000000000");
            var ttPK1 = new Guid("11111111-1111-1111-1111-111111111111");
            var ttPK2 = new Guid("22222222-2222-2222-2222-222222222222");
            var ttPK3 = new Guid("33333333-3333-3333-3333-333333333333");
            var ttPK4 = new Guid("44444444-4444-4444-4444-444444444444");
            var ttPK5 = new Guid("55555555-5555-5555-5555-555555555555");

            context.eHubTransformationSets.AddObject(new eHubTransformationSet { TS_PK = tsPK });
            context.eHubTransformationTypes.AddObject(new eHubTransformationType { TT_PK = ttPK1, TT_TransformationType = "" });
            context.eHubTransformationTypes.AddObject(new eHubTransformationType { TT_PK = ttPK2, TT_TransformationType = "" });
            context.eHubTransformationTypes.AddObject(new eHubTransformationType { TT_PK = ttPK3, TT_TransformationType = "" });
            context.eHubTransformationTypes.AddObject(new eHubTransformationType { TT_PK = ttPK4, TT_TransformationType = "" });
            context.eHubTransformationTypes.AddObject(new eHubTransformationType { TT_PK = ttPK5, TT_TransformationType = "" });
            context.eHubTransformationMappings.AddObject(new eHubTransformationMapping { TM_TS_PK = tsPK, TM_Order = 0, TM_TT_PK = ttPK1 });
            context.eHubTransformationMappings.AddObject(new eHubTransformationMapping { TM_TS_PK = tsPK, TM_Order = 1, TM_TT_PK = ttPK2 });
            context.eHubTransformationMappings.AddObject(new eHubTransformationMapping { TM_TS_PK = tsPK, TM_Order = 2, TM_TT_PK = ttPK3 });
            context.eHubTransformationMappings.AddObject(new eHubTransformationMapping { TM_TS_PK = tsPK, TM_Order = 3, TM_TT_PK = ttPK4 });

            var formValues = new FormCollection();
            formValues["TransformationSet.TS_Name"] = "";
            formValues["CNameSender"] = "";
            formValues["CNameRecipient"] = "";
            formValues["TId"] = "";
            formValues["TName"] = "";
            formValues["TransformationSet.TS_BillSender"] = "false";
            formValues["TransformationSet.TS_BillRecipient"] = "false";
            formValues["CNameBillOther"] = "";
            formValues["TransformationSet.TS_BillingNumMessagesIncluded"] = "";
            formValues["TransformationSet.TS_BillingFee"] = "";
            formValues["MId2"] = ttPK2.ToString();
            formValues["MId4"] = ttPK4.ToString();
            formValues["MId5"] = ttPK5.ToString();
            controller.ValueProvider = formValues.ToValueProvider();

            var result = EditTest(tsPK, formValues, logger);

            CollectionAssert.AreEqual(new List<Tuple<Guid, byte, Guid>>
            { 
                new Tuple<Guid, byte, Guid>(tsPK, 0, ttPK2),
                new Tuple<Guid, byte, Guid>(tsPK, 1, ttPK4),
                new Tuple<Guid, byte, Guid>(tsPK, 2, ttPK5),
            }, context.eHubTransformationMappings.Select(m => new Tuple<Guid, byte, Guid>(m.TM_TS_PK, m.TM_Order, m.TM_TT_PK)).ToList());
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

        [TestMethod]
        public void TransformationSet_Source_Add()
        {
            var tsPK = new Guid("00000000-0000-0000-0000-000000000000");
            var dtPK1 = new Guid("11111111-1111-1111-1111-111111111111");

            var logger = new TestLogger();
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;

            var ts = new eHubTransformationSet { TS_PK = tsPK };
            context.eHubTransformationSets.AddObject(ts);

            var formValues = new FormCollection();
            formValues["TransformationSet.TS_Name"] = "";
            formValues["CNameSender"] = "";
            formValues["CNameRecipient"] = "";
            formValues["TId"] = dtPK1.ToString();
            formValues["TName"] = "http://messagetype";
            formValues["TransformationSet.TS_BillSender"] = "false";
            formValues["TransformationSet.TS_BillRecipient"] = "false";
            formValues["CNameBillOther"] = "";
            formValues["TransformationSet.TS_BillingNumMessagesIncluded"] = "";
            formValues["TransformationSet.TS_BillingFee"] = "";
            controller.ValueProvider = formValues.ToValueProvider();

            var result = EditTest(tsPK, formValues, logger);

            Assert.AreEqual(dtPK1, ts.TS_DT_Source);
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

        [TestMethod]
        public void TransformationSet_Source_Delete()
        {
            var tsPK = new Guid("00000000-0000-0000-0000-000000000000");
            var dtPK1 = new Guid("11111111-1111-1111-1111-111111111111");

            var logger = new TestLogger();
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;

            var ts = new eHubTransformationSet { TS_PK = tsPK, TS_DT_Source = dtPK1 };
            context.eHubTransformationSets.AddObject(ts);

            var formValues = new FormCollection();
            formValues["TransformationSet.TS_Name"] = "";
            formValues["CNameSender"] = "";
            formValues["CNameRecipient"] = "";
            formValues["TId"] = "";
            formValues["TName"] = "";
            formValues["TransformationSet.TS_BillSender"] = "false";
            formValues["TransformationSet.TS_BillRecipient"] = "false";
            formValues["CNameBillOther"] = "";
            formValues["TransformationSet.TS_BillingNumMessagesIncluded"] = "";
            formValues["TransformationSet.TS_BillingFee"] = "";
            controller.ValueProvider = formValues.ToValueProvider();

            var result = EditTest(tsPK, formValues, logger);

            Assert.IsNull(ts.TS_DT_Source);
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

        protected ActionResult EditTest(Guid id, FormCollection formValues, ILog logger)
        {
            controller.logger = logger;
            return controller.Edit(id, formValues);
        }
    }
}
