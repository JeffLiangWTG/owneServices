using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusStorageDocPivotUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestConflictResolution()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var declaration = Factory.New<BaseJobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var invoice = declaration.Invoices.AddNew();
			declaration.DocManagerInfo.Save();
			Factory.Save();

			var pivot = CreatePivot(Factory, invoice, "TY1", eDoc.UniqueKey);
			var declarationInAnotherFactory = anotherFactory.Load<BaseJobDeclaration>(declaration.PK);
			var invoiceInAnotherFactory = declarationInAnotherFactory.Invoices[0];
			var pivotInAnotherFactory = CreatePivot(anotherFactory, invoiceInAnotherFactory, "TY1", declarationInAnotherFactory.DocManagerInfo.AllEDocs.GetFromUniqueKey(eDoc.UniqueKey.ToGuid()).UniqueKey);

			Factory.Save();

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					anotherFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Message to notify user", $@"The type 'TY1' eDoc ({eDoc.UniqueKey}) has already been linked to Invoice by another user. Your changes have been merged, please review your changes and save again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Pivot 2 should be deleted", true, pivotInAnotherFactory.IsDeleted);

				anotherFactory.Save();

				var query = new ZQuery();
				query.AddToFilter(CusStorageDocPivotSchema.CSD_ParentID, invoice.PK);
				query.AddToFilter(CusStorageDocPivotSchema.CSD_DocType, "TY1");
				query.AddToFilter(CusStorageDocPivotSchema.CSD_StorageDocReference, eDoc.UniqueKey);
				AssertEquals("Should be using the existing Pivot now", pivot.PK, anotherFactory.Load<BaseCusStorageDocPivot>(query).Single().PK);
			});
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CusStorageDocPivotUniqueIndexFailureHandler(null));
		}

		public void TestHandledUniqueIndexNames()
		{
			AssertEquals(CusStorageDocPivotSchema.Constants.Indexes.NR_UC__CSD_ParentTableCode_CSD_ParentID_CSD_DocType_CSD_StorageDocReference, new CusStorageDocPivotUniqueIndexFailureHandler(Factory.New<CusStorageDocPivotForTest>()).HandledUniqueIndexNames.Single());
		}

		static CusStorageDocPivotForTest CreatePivot(BusinessObjectFactory factory, BaseJobComInvoiceHeader invoice, string docType, ZGuid reference)
		{
			var result = factory.New<CusStorageDocPivotForTest>();
			result.CSD_ParentID = invoice.PK;
			result.CSD_ParentTableCode = invoice.TablePrefix;
			result.CSD_DocType = docType;
			result.CSD_StorageDocReference = reference;
			return result;
		}
	}
}
