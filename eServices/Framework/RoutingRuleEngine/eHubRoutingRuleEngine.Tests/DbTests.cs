using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Common.Logging.Simple;
using eServices.BuildTools.SqlDeploy;
using eServices.eHubDataModel.eHubTransactions;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace eServices.eHubRoutingRuleEngine.Tests
{
    [Property("DAT:CapabilityRequirements", "SQL")]
    public class DbTests
    {
        private eHubTransactionsContext eHubDbContext;
        private eHubRoutingRule[] eHubRoutingRules;
        private eHubRoutingRuleFact[] eHubRoutingRuleFacts;
        private eHubServiceProvider[] eHubServiceProviders;
        private eHubServiceProviderRequiredRegistration[] eHubServiceProviderRequiredRegistrations;
        private eHubRegistrationType[] eHubRegistrationTypes;
        private eHubClient[] eHubClients;
        private eHubClientRegistration[] eHubClientRegistrations;
        private Dictionary<string, DeploymentInfo> deploymentInfos;

        private const string eHubTransactionsConnectionString = "Data Source=localhost;Initial Catalog=eHubTransactions;Integrated Security=True;Encrypt=False;";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var eHubTransactionsJson = JsonObject.Parse(Resources.eHubTransactions);
            eHubRoutingRules = JsonSerializer.Deserialize<eHubRoutingRule[]>(eHubTransactionsJson["eHubRoutingRules"].ToString());
            eHubRoutingRuleFacts = JsonSerializer.Deserialize<eHubRoutingRuleFact[]>(eHubTransactionsJson["eHubRoutingRuleFacts"].ToString());
            eHubServiceProviders = JsonSerializer.Deserialize<eHubServiceProvider[]>(eHubTransactionsJson["eHubServiceProviders"].ToString());
            eHubServiceProviderRequiredRegistrations = JsonSerializer.Deserialize<eHubServiceProviderRequiredRegistration[]>(eHubTransactionsJson["eHubServiceProviderRequiredRegistrations"].ToString());
            eHubRegistrationTypes = JsonSerializer.Deserialize<eHubRegistrationType[]>(eHubTransactionsJson["eHubRegistrationTypes"].ToString());
            eHubClients = JsonSerializer.Deserialize<eHubClient[]>(eHubTransactionsJson["eHubClients"].ToString());
            eHubClientRegistrations = JsonSerializer.Deserialize<eHubClientRegistration[]>(eHubTransactionsJson["eHubClientRegistrations"].ToString());

            var deploymentWorkingDir = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\..\Dependencies\eServices.eHubDatabase");
            deploymentInfos = DeploymentTasks.GetDefaultDeploymentInfos(deploymentWorkingDir, TestContext.Progress.WriteLine);

#if !DEBUG
            DeploymentTasks.DropDatabase(deploymentInfos["eHubTransactions"]);
#endif

            DeploymentTasks.DeployDatabase(deploymentInfos["eHubTransactions"]);
        }

#if !DEBUG
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DeploymentTasks.DropDatabase(deploymentInfos["eHubTransactions"]);
        }
#endif

        [SetUp]
        public void Setup()
        {
            eHubDbContext = new eHubTransactionsContext(eHubTransactionsConnectionString);
            eHubDbContext.BeginTransaction();
            RoutingRuleFactory.ruleCache.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            eHubDbContext?.Dispose();
        }

        [Test]
        public void TestNonExistentClientShouldBeCached()
        {
            var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(DbTests));
            var ruleFactory = new RoutingRuleFactory(eHubDbContext, logger);

            _ = ruleFactory.GetForReading("AAAAAAAAA");

            Assert.That(RoutingRuleFactory.ruleCache.TryGetValue("AAAAAAAAA", out var result), Is.True);
            Assert.That(result.Rule, Is.Null);
        }

        [Test]
        public void TestRuleLockedWhenReadingFromDatabase()
        {
            LoadData();
            var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(DbTests));
            var ruleFactory = new RoutingRuleFactory(eHubDbContext, logger);

            var rule1 = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            eHubDbContext.ExecuteSqlCommand("UPDATE eHubRoutingRule SET RR_Group_Ordering = RR_Group_Ordering + 1 WHERE RR_PK = '97C31418-0425-4B0F-9C4B-1F739A02C319'");
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());
            var rule2 = ruleFactory.GetForReading("SHIPPING_INSTRUCTION");

            Assert.That(rule2.Timestamp, Is.GreaterThan(rule1.Timestamp));
        }

        [Test]
        public void TestRuleLockedWhenThereIsNoCache()
        {
            using (var eHubDbContext2 = new eHubTransactionsContext(eHubTransactionsConnectionString))
            using (var transaction2 = eHubDbContext2.BeginTransaction())
            {
                var rr = new eHubRoutingRule();
                var cc = new eHubClient { CC_ID = "LOCK_TEST", CC_RR = rr.RR_PK, CC_OwnerCategory = "Service", CC_SystemCategory = "Third Party" };
                eHubDbContext2.eHubRoutingRules.Add(rr);
                eHubDbContext2.eHubClients.Add(cc);
                eHubDbContext2.SaveChanges();
                transaction2.Commit();

                try
                {
                    var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(DbTests));
                    var ruleFactory = new RoutingRuleFactory(eHubDbContext, logger);
                    var rule1 = ruleFactory.GetForReading("LOCK_TEST");
                    Assert.That(rule1, Is.Not.Null);

                    RoutingRuleFactory.ruleCache.Clear();
                    using (var connection = new SqlConnection(eHubTransactionsConnectionString))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        using (var command = new SqlCommand($"UPDATE eHubRoutingRule SET RR_Group_Ordering = RR_Group_Ordering + 1 WHERE RR_PK = '{rr.RR_PK}'", connection, transaction))
                        {
                            command.ExecuteNonQuery();
                            var rule2 = ruleFactory.GetForReading("LOCK_TEST");
                            Assert.That(rule2, Is.Null);
                        }
                    }
                }
                finally
                {
                    eHubDbContext2.eHubClients.Remove(cc);
                    eHubDbContext2.eHubRoutingRules.Remove(rr);
                    eHubDbContext2.SaveChanges();
                }
            }
        }

        [Test]
        public void TestRule_WhiteListCaching()
        {
            LoadData();
            var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(DbTests));
            var ruleFactory = new RoutingRuleFactory(eHubDbContext, logger);

            var originalSplitRule = ruleFactory.GetForReading("OCM_Splitting", TimeSpan.Zero, TimeSpan.Zero);
            var whiteListCondition = eHubDbContext.eHubRoutingRules.Find(new Guid("5B051E4E-8180-4B02-8836-BBE8871B4737"));
            var updatedExpression = "[@DocumentName,Equal,CarrierBooking]&&[@ContainerCount,NotEqual,0]&&[@ContainerCount,NotEqual,1]";
            whiteListCondition.RR_Condition_Expression = updatedExpression;
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());
            var newSplitRuleGroup = ruleFactory.GetForReading("OCM_Splitting");

            Assert.Multiple(() =>
            {
                Assert.That(originalSplitRule.Timestamp, Is.LessThan(newSplitRuleGroup.Timestamp));
                Assert.That(((Group)((Rule)newSplitRuleGroup).SubRules.First().FailedSubRule).SubRules, Has.One.With.Property("Expression").EqualTo(updatedExpression));
            });
        }

        [Test]
        public void TestRule_BlackListCaching()
        {
            LoadData();
            var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(DbTests));
            var ruleFactory = new RoutingRuleFactory(eHubDbContext, logger);

            var originalSplitRule = ruleFactory.GetForReading("OCM_Splitting", TimeSpan.Zero, TimeSpan.Zero);
            var blackListCondition = eHubDbContext.eHubRoutingRules.Find(new Guid("F54C0D07-6375-4572-A4AF-2A5C156E8E4A"));
            var updatedExpression = "[@Port,Equal,CNNBB]&&[@Purpose,Equal,WTH]";
            blackListCondition.RR_Condition_Expression = updatedExpression;
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());
            var newSplitRuleGroup = ruleFactory.GetForReading("OCM_Splitting");

            Assert.Multiple(() =>
            {
                Assert.That(originalSplitRule.Timestamp, Is.LessThan(newSplitRuleGroup.Timestamp));
                Assert.That(((Group)((Rule)newSplitRuleGroup).SubRules.First()).SubRules, Has.One.With.Property("Expression").EqualTo(updatedExpression));
            });
        }

        [Test]
        public void TestRuleCache_ConditionTrigger_AddUpdateDelete()
        {
            LoadData();
            var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(DbTests));
            var ruleFactory = new RoutingRuleFactory(eHubDbContext, logger);

            var ruleOriginal = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var subRuleCountOriginal = ruleOriginal.FindGroupRule("DefaultCarrier").SubRules.Count;

            var ruleForUpdating = ruleFactory.GetForEditing("SHIPPING_INSTRUCTION");

            // Add
            var newRule = new Condition();
            ruleForUpdating.FindGroupRule("DefaultCarrier").SubRules.Insert(0, newRule);
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());

            var ruleAfterAdd = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var subRuleCountAfterAdd = ruleAfterAdd.FindGroupRule("DefaultCarrier").SubRules.Count;

            Assert.That(subRuleCountAfterAdd, Is.EqualTo(subRuleCountOriginal + 1));
            Assert.That(ruleAfterAdd.Timestamp, Is.GreaterThan(ruleOriginal.Timestamp));

            // Update
            newRule.Expression = "[@SCAC,Equal,DUMMY1]";
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());

            var ruleAfterUpdate = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var subRuleCountAfterUpdate = ruleAfterUpdate.FindGroupRule("DefaultCarrier").SubRules.Count;

            Assert.That(subRuleCountAfterUpdate, Is.EqualTo(subRuleCountAfterAdd));
            Assert.That(ruleAfterUpdate.Timestamp, Is.GreaterThan(ruleAfterAdd.Timestamp));

            // Delete
            ruleForUpdating.FindGroupRule("DefaultCarrier").SubRules.RemoveAt(0);
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());

            var ruleAfterDelete = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var subRuleCountAfterDelete = ruleAfterDelete.FindGroupRule("DefaultCarrier").SubRules.Count;

            Assert.That(subRuleCountAfterDelete, Is.EqualTo(subRuleCountAfterUpdate - 1));
            Assert.That(ruleAfterDelete.Timestamp, Is.GreaterThan(ruleAfterUpdate.Timestamp));
        }

        [Test]
        public void TestRuleCache_FactTrigger_AddUpdateDelete()
        {
            LoadData();
            var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(DbTests));
            var ruleFactory = new RoutingRuleFactory(eHubDbContext, logger);

            var ruleOriginal = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var factCountOriginal = ruleOriginal.Facts.Count;

            var ruleForUpdating = ruleFactory.GetForEditing("SHIPPING_INSTRUCTION");

            // Add
            var newFact = new Fact { Name = "TEST_FACT", Type = "PROPERTY" };
            ruleForUpdating.Facts.Add(newFact);
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());

            var ruleAfterAdd = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var factCountAfterAdd = ruleAfterAdd.Facts.Count;

            Assert.That(factCountAfterAdd, Is.EqualTo(factCountOriginal + 1));
            Assert.That(ruleAfterAdd.Timestamp, Is.GreaterThan(ruleOriginal.Timestamp));

            // Update
            newFact.Query = "ActionPurpose2";
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());

            var ruleAfterUpdate = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var factCountAfterUpdate = ruleAfterUpdate.Facts.Count;

            Assert.That(factCountAfterUpdate, Is.EqualTo(factCountAfterAdd));
            Assert.That(ruleAfterUpdate.Timestamp, Is.GreaterThan(ruleAfterAdd.Timestamp));

            // Delete
            ruleForUpdating.Facts.Remove(newFact);
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());

            var ruleAfterDelete = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var factCountAfterDelete = ruleAfterDelete.Facts.Count;

            Assert.That(factCountAfterDelete, Is.EqualTo(factCountAfterUpdate - 1));
            Assert.That(ruleAfterDelete.Timestamp, Is.GreaterThan(ruleAfterUpdate.Timestamp));
        }

        [Test]
        public void TestRuleCache_ServiceProviderTrigger_Update()
        {
            LoadData();
            var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(DbTests));
            var ruleFactory = new RoutingRuleFactory(eHubDbContext, logger);

            var ruleOriginal = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var serviceProviderCountOriginal = ruleOriginal.ServiceProviders.Count;

            // Update
            var serviceProvider = eHubDbContext.eHubServiceProviders.Find(new Guid("38982008-7ABE-42DD-B1F0-02E4B3068AF9"));
            serviceProvider.eHubClient_Provider = new eHubClient { CC_ID = "NEW_PROVIDER", CC_OwnerCategory = "Service", CC_SystemCategory = "Third Party" };
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());

            var ruleAfterUpdate = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var serviceProviderCountAfterUpdate = ruleAfterUpdate.ServiceProviders.Count;

            Assert.That(serviceProviderCountAfterUpdate, Is.EqualTo(serviceProviderCountOriginal));
            Assert.That(ruleAfterUpdate.Timestamp, Is.GreaterThan(ruleOriginal.Timestamp));
        }

        [Test]
        public void TestRuleCache_RequiredRegistrationTrigger_AddUpdateDelete()
        {
            LoadData();
            var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(DbTests));
            var ruleFactory = new RoutingRuleFactory(eHubDbContext, logger);

            var ruleOriginal = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var requiredRegistrationCountOriginal = ruleOriginal.ServiceProviders.Sum(sp => sp.RequiredRegistrations.Count);
            var testRequiredRegistration = eHubDbContext.eHubServiceProviderRequiredRegistrations
                .Find(new Guid("38982008-7ABE-42DD-B1F0-02E4B3068AF9"), new Guid("EE998059-D01D-4ACD-8E35-5DE3A270CE3E"));

            // Update
            testRequiredRegistration.SX_LookupFactName = "DestinationParty";
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());

            var ruleAfterUpdate = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var requiredRegistrationCountAfterUpdate = ruleAfterUpdate.ServiceProviders.Sum(sp => sp.RequiredRegistrations.Count);

            Assert.That(requiredRegistrationCountAfterUpdate, Is.EqualTo(requiredRegistrationCountOriginal));
            Assert.That(ruleAfterUpdate.Timestamp, Is.GreaterThan(ruleOriginal.Timestamp));

            // Delete
            eHubDbContext.eHubServiceProviderRequiredRegistrations.Remove(testRequiredRegistration);
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());

            var ruleAfterDelete = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var requiredRegistrationCountAfterDelete = ruleAfterDelete.ServiceProviders.Sum(sp => sp.RequiredRegistrations.Count);

            Assert.That(requiredRegistrationCountAfterDelete, Is.EqualTo(requiredRegistrationCountOriginal - 1));
            Assert.That(ruleAfterDelete.Timestamp, Is.GreaterThan(ruleAfterUpdate.Timestamp));

            // Add
            eHubDbContext.eHubServiceProviderRequiredRegistrations.Add(testRequiredRegistration);
            eHubDbContext.SaveChanges();
            eHubDbContext.ChangeTracker.Entries().ToList().ForEach(r => eHubDbContext.Entry(r.Entity).Reload());

            var ruleAfterAdd = ruleFactory.GetForReading("SHIPPING_INSTRUCTION", TimeSpan.Zero, TimeSpan.Zero);
            var requiredRegistrationCountAfterAdd = ruleAfterAdd.ServiceProviders.Sum(sp => sp.RequiredRegistrations.Count);

            Assert.That(requiredRegistrationCountAfterAdd, Is.EqualTo(requiredRegistrationCountOriginal));
            Assert.That(ruleAfterAdd.Timestamp, Is.GreaterThan(ruleAfterDelete.Timestamp));
        }

        private void LoadData()
        {
            eHubDbContext.eHubRoutingRules.AddRange(eHubRoutingRules);
            eHubDbContext.eHubRoutingRuleFacts.AddRange(eHubRoutingRuleFacts);
            eHubDbContext.eHubServiceProviders.AddRange(eHubServiceProviders);
            eHubDbContext.eHubServiceProviderRequiredRegistrations.AddRange(eHubServiceProviderRequiredRegistrations);
            eHubDbContext.eHubRegistrationTypes.AddRange(eHubRegistrationTypes);
            eHubDbContext.eHubClients.AddRange(eHubClients);
            eHubDbContext.eHubClientRegistrations.AddRange(eHubClientRegistrations);
            eHubDbContext.SaveChanges();
        }

        const string shippingInstructionRule = "D74CF7D3-6F85-4F06-AFC7-EDBEA8BBB7FC";
    }
}