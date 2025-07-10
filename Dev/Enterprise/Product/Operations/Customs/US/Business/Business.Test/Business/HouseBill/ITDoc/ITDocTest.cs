using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ITDoc))]
	sealed class ITDocTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ITDoc>
	{
		public void TestSettingOpenAreaUpdatesITDocData()
		{
			var itDoc = Factory.New<ITDoc>();
			itDoc.US_7512OpenArea = "7512 Document open area";
			AssertEquals("7512 Document open area", itDoc.US_7512OpenArea);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var factory = NewFactory();
			var bO = (ITDoc)GetNewBusinessObjectForDeleteTest(factory);
			bO.US_7512OpenArea = "Some value";
			bO.Factory.Save();
			AssertEquals("BO.IsDeleted", false, bO.IsDeleted);
			var separateFactory = NewFactory();
			AssertNotNull("The BizO is saved and should have been persisted", separateFactory.Load(bO.GetType(), bO.PK));
			if (CanPersistedObjectBeDeleted)
			{
				bO.Delete();
				try
				{
					bO.Factory.Save();
				}
				catch (ZSaveException ex)
				{
					if (ex.ToString().Contains("conflicted with the REFERENCE constraint"))
					{
						ErrorReporter.ReportOnce("Tables were last saved in the following order:\r\n");
					}

					throw;
				}

				separateFactory = NewFactory();
				AssertNull("The BizO should have been deleted from DB", separateFactory.Load(bO.GetType(), bO.PK));
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObjectForDeleteTest(factory) as ITDoc;
			result.B7_AddInfoData = "ITDoc";
			result.US_7512OpenArea = "Some value";
			return result;
		}

		protected override IEnumerable<ITDoc> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var result = factory.New<ITDoc>();
			result.B7_ParentID = declaration.PK;
			result.B7_ParentTableCode = declaration.TablePrefix;
			result.US_7512OpenArea = "SD";
			yield return result;
		}
	}
}
