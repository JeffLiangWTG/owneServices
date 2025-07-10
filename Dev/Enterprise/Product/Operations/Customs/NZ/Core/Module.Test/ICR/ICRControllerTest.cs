using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(ICRController))]
	sealed class ICRControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = Factory.NewWithValidTestData<CusEntryNumber>();
			result.CE_ParentTable = "CusEntryHeader";
			Factory.Save();

			return result;
		}

		public override Type ControllerToBashType
		{
			get
			{
				return typeof(ICRController);
			}
		}

		protected override string CountryCode
		{
			get
			{
				return Core.Constants.CountryCodes.NewZealand;
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.NZ.InwardCargoReport;
		}
	}
}
