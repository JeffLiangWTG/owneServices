using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class CommercialInvoiceControllerTest : ZControllerBasherTest
	{
		[RequiresSTA]
		public virtual void TestFormReturnedIsOfRightType()
		{
			AssertControllerNotNull();
			if (ExpectedFormType != null)
			{
				try
				{
					IZForm formFromGetFormOnNew = Controller.ShowNewForm();
					AssertEquals("Check to make sure your Controller is properly registered in ZModules, and that GetForm() is working properly.\r\n\r\nformFromGetFormOnNew.GetType()", ExpectedFormType, formFromGetFormOnNew.GetType());
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		public void TestControllerReturnedFromSpecifiedControllerIDIsOfTheRightType()
		{
			AssertControllerNotNull();
			if (ExpectedControllerType != null)
			{
				try
				{
					AssertEquals("Check to make sure your Controller is properly registered in ZModules.\r\n\r\nController.GetType()", ExpectedControllerType, Controller.GetType());
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		protected sealed override ControllerID GetControllerID()
		{
			return ControllerIDs.CommercialInvoice;
		}

		protected sealed override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var invoice = GetNewInvoiceHeader();
			Factory.Save();
			return invoice;
		}

		protected abstract BaseJobComInvoiceHeader GetNewInvoiceHeader();
		protected abstract Type ExpectedFormType { get; }

		protected Type ExpectedControllerType => TestedTypeHelper.GetTestedType(GetType());
	}
}
