using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.Business.Testing
{
	public class OutturnableDummy : DummyBusinessObject, IOutturnableLine
	{
		public OutturnableDummy(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString UnderbondHumanReadableName
		{
			get { return "asd"; }
		}

		public BusinessObject LinkedBusinessObject
		{
			get { return this; }
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return "WTO"; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return 5; }
		}
	}
}
