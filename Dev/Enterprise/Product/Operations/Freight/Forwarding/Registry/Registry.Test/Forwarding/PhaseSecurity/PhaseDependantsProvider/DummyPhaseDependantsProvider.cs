using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	public class DummyPhaseDependantsProvider : PhaseDependantsProvider
	{
		protected override Type ParentType
		{
			get { return typeof(DummyBusinessObject); }
		}
	}
}
