using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UniqueDocumentCollectionView))]
	sealed class UniqueDocumentCollectionViewTest : ActiveBusinessObjectCollectionTestCase<UniqueDocumentCollectionView>
	{
		public void TestPreventAutoDeliveryNotInCollection()
		{
			var uniqueDocuments = new UniqueDocumentCollectionView(Factory);
			AssertEquals("Pre-condition: uniqueDocuments.Count", 0, uniqueDocuments.Count);

			var menuItem = CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Cartage/", ContactType.LocalTransport, "");

			menuItem.SU_PreventAutoDelivery = ZBool.False;
			AssertEquals("uniqueDocuments.Count", 1, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Cartage Advice : Customs : Cartage/ : TRN");

			menuItem.SU_PreventAutoDelivery = ZBool.True;
			AssertEquals("uniqueDocuments.Count", 0, uniqueDocuments.Count);

			menuItem.SU_PreventAutoDelivery = ZBool.False;
			AssertEquals("uniqueDocuments.Count", 1, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Cartage Advice : Customs : Cartage/ : TRN");
		}

		public void TestNoContactTypeNotInCollection()
		{
			var uniqueDocuments = new UniqueDocumentCollectionView(Factory);
			AssertEquals("Pre-condition: uniqueDocuments.Count", 0, uniqueDocuments.Count);

			var menuItem = CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Cartage/", ContactType.LocalTransport, "");
			AssertEquals("uniqueDocuments.Count", 1, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Cartage Advice : Customs : Cartage/ : TRN");

			menuItem.SU_ContactType = ZString.Empty;
			AssertEquals("uniqueDocuments.Count", 0, uniqueDocuments.Count);

			menuItem.SU_ContactType = ContactType.Consignor.Code;
			AssertEquals("uniqueDocuments.Count", 1, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Cartage Advice : Customs : Cartage/ : CNR");

			menuItem.SU_ContactType = ContactType.NoContactType.Code;
			AssertEquals("uniqueDocuments.Count", 0, uniqueDocuments.Count);
		}

		public void TestDocumentsInTheCollectionHaveUniqueCode()
		{
			var uniqueDocuments = new UniqueDocumentCollectionView(Factory);
			AssertEquals("Pre-condition: uniqueDocuments.Count", 0, uniqueDocuments.Count);

			CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Cartage/", ContactType.LocalTransport, "\"<MessageTypeForDocumentFilter>\" == \"EXP\"");
			AssertEquals("uniqueDocuments.Count", 1, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Cartage Advice : Customs : Cartage/ : TRN");

			CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Cartage/", ContactType.LocalTransport, "\"<MessageTypeForDocumentFilter>\" != \"EXP\"");
			AssertEquals("uniqueDocuments.Count", 1, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Cartage Advice : Customs : Cartage/ : TRN");

			CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Legacy Documents/Cartage/", ContactType.LocalTransport, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");
			AssertEquals("uniqueDocuments.Count", 2, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Cartage Advice : Customs : Legacy Documents/Cartage/ : TRN");

			CreateStmMenuItem(BusinessContext.Shipment, "Pre-Alert", "Legacy Documents/Arrival/", ContactType.Consignee, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");
			AssertEquals("uniqueDocuments.Count", 3, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Pre-Alert : Shipment : Legacy Documents/Arrival/ : CNE");

			CreateStmMenuItem(BusinessContext.Shipment, "Pre-Alert", "Arrival/", ContactType.Consignee, "");
			AssertEquals("uniqueDocuments.Count", 4, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Pre-Alert : Shipment : Arrival/ : CNE");
		}

		public void TestDocumentsInTheCollectionMatchBusinessContext()
		{
			var uniqueDocuments = new UniqueDocumentCollectionView(Factory, null, nameof(BusinessContext.Shipment));
			AssertEquals("Pre-condition: uniqueDocuments.Count", 0, uniqueDocuments.Count);

			CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Cartage/", ContactType.LocalTransport, "\"<MessageTypeForDocumentFilter>\" != \"EXP\"");
			AssertEquals("uniqueDocuments.Count", 0, uniqueDocuments.Count);

			CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Legacy Documents/Cartage/", ContactType.LocalTransport, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");
			AssertEquals("uniqueDocuments.Count", 0, uniqueDocuments.Count);

			CreateStmMenuItem(BusinessContext.Shipment, "Pre-Alert", "Legacy Documents/Arrival/", ContactType.Consignee, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");
			AssertEquals("uniqueDocuments.Count", 1, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Pre-Alert : Shipment : Legacy Documents/Arrival/ : CNE");

			CreateStmMenuItem(BusinessContext.Shipment, "Pre-Alert", "Arrival/", ContactType.Consignee, ZString.Empty);
			AssertEquals("uniqueDocuments.Count", 2, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Pre-Alert : Shipment : Arrival/ : CNE");

			uniqueDocuments = new UniqueDocumentCollectionView(Factory, null, string.Empty);
			AssertEquals("uniqueDocuments.Count", 4, uniqueDocuments.Count);

			uniqueDocuments = new UniqueDocumentCollectionView(Factory, null, null);
			AssertEquals("uniqueDocuments.Count", 4, uniqueDocuments.Count);
		}

		public void TestDocumentsInTheCollectionMatchApplicableCommands()
		{
			var uniqueDocuments = new UniqueDocumentCollectionView(Factory);
			AssertEquals("Pre-condition: uniqueDocuments.Count", 0, uniqueDocuments.Count);

			var item1 = CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Cartage/", ContactType.LocalTransport, "\"<MessageTypeForDocumentFilter>\" != \"EXP\"");
			var item2 = CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Legacy Documents/Cartage/", ContactType.LocalTransport, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");
			var item3 = CreateStmMenuItem(BusinessContext.Shipment, "Pre-Alert", "Legacy Documents/Arrival/", ContactType.Consignee, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");
			var item4 = CreateStmMenuItem(BusinessContext.Shipment, "Pre-Alert", "Arrival/", ContactType.Consignee, ZString.Empty);
			AssertEquals("uniqueDocuments.Count", 4, uniqueDocuments.Count);

			uniqueDocuments = new UniqueDocumentCollectionView(Factory, null, null);
			AssertEquals("uniqueDocuments.Count", 4, uniqueDocuments.Count);

			uniqueDocuments = new UniqueDocumentCollectionView(Factory, new List<ZGuid>(), null);
			AssertEquals("uniqueDocuments.Count", 4, uniqueDocuments.Count);

			uniqueDocuments = new UniqueDocumentCollectionView(Factory, new List<ZGuid> { item1.PK, item3.PK }, null);
			AssertEquals("uniqueDocuments.Count", 2, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Cartage Advice : Customs : Cartage/ : TRN");
			AssertUniqueDocumentsContains(uniqueDocuments, "Pre-Alert : Shipment : Legacy Documents/Arrival/ : CNE");

			uniqueDocuments = new UniqueDocumentCollectionView(Factory, new List<ZGuid> { item2.PK }, null);
			AssertEquals("uniqueDocuments.Count", 1, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Cartage Advice : Customs : Legacy Documents/Cartage/ : TRN");

			uniqueDocuments = new UniqueDocumentCollectionView(Factory, new List<ZGuid> { item2.PK }, nameof(BusinessContext.Customs));
			AssertEquals("uniqueDocuments.Count", 1, uniqueDocuments.Count);
			AssertUniqueDocumentsContains(uniqueDocuments, "Cartage Advice : Customs : Legacy Documents/Cartage/ : TRN");

			uniqueDocuments = new UniqueDocumentCollectionView(Factory, new List<ZGuid> { item2.PK }, nameof(BusinessContext.Shipment));
			AssertEquals("uniqueDocuments.Count", 0, uniqueDocuments.Count);
		}

		public void TestApplicableCommandPKs_ShouldUseTableValuedParameters()
		{
			var item1 = CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Cartage/", ContactType.LocalTransport, "\"<MessageTypeForDocumentFilter>\" != \"EXP\"");
			var item2 = CreateStmMenuItem(BusinessContext.Customs, "Cartage Advice", "Legacy Documents/Cartage/", ContactType.LocalTransport, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");
			var item3 = CreateStmMenuItem(BusinessContext.Shipment, "Pre-Alert", "Legacy Documents/Arrival/", ContactType.Consignee, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");
			var item4 = CreateStmMenuItem(BusinessContext.Shipment, "Pre-Alert", "Arrival/", ContactType.Consignee, ZString.Empty);

			using (Db.Connection.TrackExecutedCommands())
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");
					var uniqueDocuments = new UniqueDocumentCollectionView(Factory, new List<ZGuid> { item1.PK, item3.PK }, null);
					AssertEquals("uniqueDocuments.Count", 2, uniqueDocuments.Count);

					var executedCommand = Db.Connection.ExecutedCommands.FirstOrDefault(c => c.Contains("FROM dbo.StmMenuItem"));
					AssertNotNull(executedCommand);
					AssertContains("Should use TVPs", "(SU_PK in (SELECT Value FROM", executedCommand);
					Assert("Should not use explicit values", !Regex.IsMatch(executedCommand, @"\(SU_PK in \((@(.*?),)*?@(.*?)\)"));
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable("StmMenuDocumentConfigItem");
			TestCaseHelper.ClearTable("StmMenuDocumentConfig");
			TestCaseHelper.ClearTable(StmMenuTemplatePivot.Schema.TableName);
			TestCaseHelper.ClearTable(StmMenuMenuPivot.Schema.TableName);
			TestCaseHelper.ClearTable(StmMenuEDocs.Schema.TableName);
			TestCaseHelper.ClearTable(StmMenuItem.Schema.TableName);
		}

		void AssertUniqueDocumentsContains(UniqueDocumentCollectionView uniqueDocuments, ZString uniqueCode)
		{
			var count = 0;

			foreach (var menuItem in uniqueDocuments)
			{
				if (menuItem.MenuItemUniqueCode.EqualsIgnoringCase(uniqueCode))
				{
					count++;
				}
			}

			AssertEquals(string.Format("The collection must only contain one of [{0}].", uniqueCode), 1, count);
		}

		StmMenuItem CreateStmMenuItem(BusinessContext businessContext, ZString menuName, ZString menuPath, ContactType contactType, ZString filterList)
		{
			var result = Factory.New<StmMenuItem>();

			result.SU_BusinessContext = businessContext.ToString();
			result.SU_MenuName = menuName;
			result.SU_MenuPath = menuPath;
			result.SU_ContactType = contactType.Code;
			result.SU_FilterList = filterList;

			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = base.GetNewElementToAddToTheCollection() as StmMenuItem;
			result.SU_MenuName = ZGuid.NewZGuid().ToString();
			result.SU_PreventAutoDelivery = ZBool.False;
			result.SU_ContactType = ContactType.All.Code;

			return result;
		}

		#endregion
	}
}
