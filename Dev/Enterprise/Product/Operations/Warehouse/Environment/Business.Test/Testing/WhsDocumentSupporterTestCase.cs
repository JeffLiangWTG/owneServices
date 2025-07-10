using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class WhsDocumentSupporterTestCase : DocumentSupporterTest
	{
		#region Document Supporter Tests

		public abstract void TestBusinessContext();
		public abstract void TestGetContactOrganisation();

		public virtual void TestSupportedDataContexts()
		{
			AssertEquals("DataContext is Supported", true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext)));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertNotEquals("Warehouse document supporters should use securities.", Env.Security.None, DocSupporter.CustomisationSecurityCheckpoint);
			AssertEquals(ExpectedCustomisationSecurityCheckpoint, DocSupporter.CustomisationSecurityCheckpoint);
		}

		protected abstract ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint { get; }

		public virtual void TestGetDocBusinessObjects()
		{
			IBODocDataProvider[] docBizoList = DocSupporter.GetDocumentWrappers(DataContext, null);
			AssertEquals(1, docBizoList.Length);
			AssertNotNull(docBizoList[0]);
			AssertEquals(docBizoList[0].ParentBusinessObject, BusinessObject);
			// can't assert datacontext because its not exposed
		}

		public virtual void TestGetBODocDataProvidersNotFoundMessageReturnOneReason()
		{
			foreach (var dataContextAndMessage in SupportedDataContextAndNotFoundMessageReasonPairs)
			{
				var dataContext = dataContextAndMessage.Item1;
				var reasonMessage = dataContextAndMessage.Item2;
				var notFoundMessage = DocSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(dataContext.ToString()), null);
				if (DocSupporter.ShowReasonForNotPrinting(dataContext, null))
				{
					AssertEquals($"Should returns a reason when attempt to print without underlying data. (DataContext: {dataContext})", reasonMessage, notFoundMessage);
				}
				else
				{
					Assert(true);
				}
			}
			Assert(true);
		}

		protected virtual IEnumerable<Tuple<Enterprise.Core.Constants.DataContext, string>> SupportedDataContextAndNotFoundMessageReasonPairs => Enumerable.Empty<Tuple<Enterprise.Core.Constants.DataContext, string>>();

		public void TestShowReasonForNotPrinting()
		{
			AssertEquals("ShowReasonForNotPrinting", GetExpectedShowReasonForNotPrinting(), DocSupporter.ShowReasonForNotPrinting(DataContext, null));
		}

		protected virtual bool GetExpectedShowReasonForNotPrinting()
		{
			return true;
		}

		#endregion

		#region Implementation

		protected abstract Enterprise.Core.Constants.DataContext DataContext
		{
			get;
		}

		protected abstract BusinessObject GetNewBusinessObject();

		BusinessObject businessObject;
		protected BusinessObject BusinessObject
		{
			get { return businessObject ?? (businessObject = GetNewBusinessObject()); }
			set { businessObject = value; }
		}

		DocumentSupporter docSupporter;
		protected DocumentSupporter DocSupporter
		{
			get { return docSupporter ?? (docSupporter = ((IDocumentSupportable)BusinessObject).DocumentSupporter); }
			set { docSupporter = value; }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return (IDocumentSupportable)BusinessObject;
		}

		WhsTestHelperFunctionsEnv helper;
		protected WhsTestHelperFunctionsEnv Helper
		{
			get
			{
				if (helper == null)
				{
					helper = GetNewTestHelperFunctions();
				}
				return helper;
			}
		}

		protected virtual WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsEnv(Factory);
		}

		TestNotificationBuffer notify;
		protected TestNotificationBuffer Notify
		{
			get
			{
				if (notify == null)
				{
					notify = new TestNotificationBuffer();
				}
				return notify;
			}
		}

		#endregion
	}
}
