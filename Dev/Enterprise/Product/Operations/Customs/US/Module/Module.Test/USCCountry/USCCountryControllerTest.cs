using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCCountryController))]
	sealed class USCCountryControllerTest : ZControllerBasherTest
	{
		[ExpectException(typeof(ModuleGuiNotSupportedException))]
		public void TestNotSupportedGetForm()
		{
			var myController = new CountryControllerForTest();
			myController.myGetForm(null);
		}

		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestNewForm()
		{
			Assert(true);
		}

		public override void TestViewForm()
		{
			Assert(true);
		}

		public override Type ControllerToBashType => typeof(USCCountryController);

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.USCCountry;

		sealed class CountryControllerForTest : USCCountryController
		{
			public IZForm myGetForm(IBusiness businessEntity) => GetForm(businessEntity);
		}
	}
}
