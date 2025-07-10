using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(RelatedOrgPartyScreeningStatus))]
	public class RelatedOrgPartyScreeningStatusTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var obj = Factory.NewWithValidTestData<RelatedOrgPartyScreeningStatus>();
			obj.PJ_ParentTableCode = "JE";
			return obj;
		}

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
			BusinessObjectFactory factory = NewFactory();
			BusinessObject newBusinessObject;
			if (!IsDeleteSupported())
			{
				newBusinessObject = GetNewBusinessObject();
				try
				{
					newBusinessObject.Delete();
					AssertNotNullOrEmpty("Deleting is not supported on " + newBusinessObject.GetType().FullName + " but no developer error is raised", ErrorReporter.LastMessageReported);
					return;
				}
				catch (NotSupportedException)
				{
					Assert(condition: true);
					return;
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}

			newBusinessObject = GetNewBusinessObjectForDeleteTest(factory);
			newBusinessObject.Factory.Save();
			AssertEquals("BO.IsDeleted", expected: false, newBusinessObject.IsDeleted);
			BusinessObjectFactory businessObjectFactory = NewFactory();
			AssertNotNull("The BizO is saved and should have been persisted", businessObjectFactory.Load(newBusinessObject.GetType(), newBusinessObject.PK));
			if (CanPersistedObjectBeDeleted)
			{
				newBusinessObject.Delete();
				if (newBusinessObject.TableName != "StmALog")
				{
					try
					{
						newBusinessObject.Factory.Save();
					}
					catch (ZSaveException ex2)
					{
						if (ex2.ToString().Contains("conflicted with the REFERENCE constraint"))
						{
							ErrorReporter.ReportOnce("Tables were last saved in the following order:\r\n" + string.Join(",", ZSaver.LastTableSaveOrder));
						}

						throw;
					}

					businessObjectFactory = NewFactory();
					AssertNull("The BizO should have been deleted from DB", businessObjectFactory.Load(newBusinessObject.GetType(), newBusinessObject.PK));
				}
			}

			ErrorReporter.Clear();
		}
	}
}
