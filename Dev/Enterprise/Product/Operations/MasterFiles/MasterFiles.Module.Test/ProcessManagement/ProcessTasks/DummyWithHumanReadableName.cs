using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Module.Testing
{
	public sealed class DummyWithHumanReadableName : DummyBusinessObject
	{
		public DummyWithHumanReadableName(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Z0_Description;
	}
}
