using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business.Testing.Workflow.ProcessTasks
{
	abstract class WorkflowItemCollectionViewSecurityTest : ProcessTaskBaseCollectionViewTest<WorkflowItemCollectionView>
	{
		protected void AssertAllowNewSecurity<THeader>(Func<THeader, WorkflowItemCollectionView> collectionViewFunc, string securityCode, bool securityIsAllowed) where THeader : BusinessObject
		{
			AssertSecurity(collectionViewFunc, view => view.AllowNew, securityCode, securityIsAllowed);
		}

		protected void AssertAllowRemoveSecurity<THeader>(Func<THeader, WorkflowItemCollectionView> collectionViewFunc, string securityCode, bool securityIsAllowed) where THeader : BusinessObject
		{
			AssertSecurity(collectionViewFunc, view => view.AllowRemove, securityCode, securityIsAllowed);
		}

		void AssertSecurity<THeader>(Func<THeader, WorkflowItemCollectionView> collectionViewFunc, Func<WorkflowItemCollectionView, bool> allowFunc, string securityCode, bool securityIsAllowed) where THeader : BusinessObject
		{
			// Arrange
			var header = Factory.NewWithValidTestData<THeader>();
			var collectionView = collectionViewFunc(header);
			var element = collectionView.AddNew();
			var controller = ObjectFactory.Get<IControllerFactory>().Create(element.ParentControllerID);
			using (var module = ObjectFactory.Get<IModuleFactory>().Create(controller.ModuleID))
			{
				var security = Env.Security.FindOrCreateWorkflowItemCheckpoint((SecurityCheckpoint)module.SecurityCheckpoint, securityCode);
				security.IsAllowed = securityIsAllowed;
			}

			// Act
			var result = allowFunc(collectionView);

			// Assert
			AssertEquals(securityIsAllowed, result);
		}

		protected void AssertDefaultAllowNewSecurity<THeader>(Func<THeader, WorkflowItemCollectionView> collectionViewFunc) where THeader : BusinessObject
		{
			AssertDefaultSecurity(collectionViewFunc, view => view.AllowNew);
		}

		protected void AssertDefaultAllowRemoveSecurity<THeader>(Func<THeader, WorkflowItemCollectionView> collectionViewFunc) where THeader : BusinessObject
		{
			AssertDefaultSecurity(collectionViewFunc, view => view.AllowRemove);
		}

		void AssertDefaultSecurity<THeader>(Func<THeader, WorkflowItemCollectionView> collectionViewFunc, Func<WorkflowItemCollectionView, bool> allowFunc) where THeader : BusinessObject
		{
			// Arrange
			var header = Factory.NewWithValidTestData<THeader>();
			var collectionView = collectionViewFunc(header);

			// Act
			var result = allowFunc(collectionView);

			// Assert
			AssertEquals(true, result);
		}
	}
}
