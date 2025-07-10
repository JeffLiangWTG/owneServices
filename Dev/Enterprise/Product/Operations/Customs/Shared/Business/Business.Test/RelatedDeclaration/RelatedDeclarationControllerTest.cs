using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business.Testing
{
	public class RelatedDeclarationControllerTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertEquals(0, Declaration.RelatedDeclarations.Count);
			RelatedDeclarationHelperForTest.CreateNewRelated(Declaration);
			Assert(!FormPresenter.NewFormShown);
			Declaration.Factory.Save();
			RelatedDeclarationHelperForTest.CreateNewRelated(Declaration);
			Assert(FormPresenter.NewFormShown);
			AssertEquals(1, Declaration.RelatedDeclarations.Count);
			AssertEquals(ControllerIDs.Customs.JobDeclaration, FormPresenter.controllerId);
			AssertEquals(Declaration.GetType(), FormPresenter.businessObjectType);
		}

		public void TestEdit()
		{
			RelatedDeclarationHelperForTest.EditExisting(Declaration);
			Assert(FormPresenter.EditFormShown);
			AssertEquals(ControllerIDs.Customs.JobDeclaration, FormPresenter.controllerId);
			AssertEquals(declaration.GetType(), FormPresenter.businessObjectType);
		}

		public void TestView()
		{
			RelatedDeclarationHelperForTest.ViewExisting(Declaration);
			CombineAssertions(() =>
			{
				AssertEquals("ViewFormShown", true, FormPresenter.ViewFormShown);
				AssertEquals("controllerId", ControllerIDs.Customs.JobDeclaration, FormPresenter.controllerId);
				AssertEquals("businessObjectType", declaration.GetType(), FormPresenter.businessObjectType);
			});
		}

		#region Implementation

		RelatedDeclarationHelper RelatedDeclarationHelperForTest
		{
			get
			{
				if (relatedDeclarationHelperForTest == null)
				{
					relatedDeclarationHelperForTest = new RelatedDeclarationHelper(FormPresenter);
				}
				return relatedDeclarationHelperForTest;
			}
		}
		RelatedDeclarationHelper relatedDeclarationHelperForTest;

		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<BaseJobDeclaration>();
				}
				return declaration;
			}
		}
		BaseJobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			FormPresenter.NewFormShown = false;
			FormPresenter.EditFormShown = false;
			formPresenter.ViewFormShown = false;
			FormPresenter.controllerId = null;
			FormPresenter.businessObjectType = null;
		}

		ZFormPresenterForTest FormPresenter
		{
			get
			{
				if (formPresenter == null)
				{
					formPresenter = new ZFormPresenterForTest();
				}
				return formPresenter;
			}
		}
		ZFormPresenterForTest formPresenter;

		public class ZFormPresenterForTest : IFormPresenter
		{
			public ZFormPresenterForTest()
			{
				controllerId = null;
				businessObjectType = null;
			}

			public ControllerID controllerId;
			public Type businessObjectType;
			public bool NewFormShown;
			public bool EditFormShown;
			public bool ViewFormShown;

			#region FormPresenter Members

			public void ShowError(string title, string caption)
			{
			}

			public void ShowNew(ControllerID controllerId, BusinessObject bo)
			{
				NewFormShown = true;
				this.controllerId = controllerId;
				businessObjectType = bo.GetType();
				bo.Factory.Save();
			}

			public void ShowEdit(ControllerID controllerId, BusinessObject bo)
			{
				EditFormShown = true;
				this.controllerId = controllerId;
				businessObjectType = bo.GetType();
			}

			public void ShowView(ControllerID controllerId, BusinessObject bo)
			{
				ViewFormShown = true;
				this.controllerId = controllerId;
				businessObjectType = bo.GetType();
			}

			#endregion
		}

		#endregion

	}
}
