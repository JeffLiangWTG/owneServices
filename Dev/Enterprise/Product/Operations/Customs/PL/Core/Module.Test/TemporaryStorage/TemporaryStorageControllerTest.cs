using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Module.Testing;

[TestedType(typeof(TemporaryStorageController))]
class TemporaryStorageControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
{
	protected BusinessObject GetBusinessObjectHeader()
	{
		var header = CusTempStorageJobHeader.New(Factory);
		Factory.Save();
		return header;
	}

	public override Type ControllerToBashType
	{
		get { return typeof(TemporaryStorageController); }
	}

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.TemporaryStorage;

	protected override string CountryCode => Core.Constants.CountryCodes.Poland;
}
