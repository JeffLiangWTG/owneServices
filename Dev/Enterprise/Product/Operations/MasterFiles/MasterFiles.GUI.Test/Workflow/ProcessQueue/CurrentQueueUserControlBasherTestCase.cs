using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	abstract class CurrentQueueUserControlBasherTestCase : BasherTest
	{
		protected abstract BusinessObject BusinessEntity { get; }

		protected virtual string BindToPrefix
		{
			get { return ""; }
		}
	}
}
