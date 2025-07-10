using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportCommon.Module.Testing
{
	public abstract class DtbTransportFilterControlTest<TTransport, TTransportFilterBizO> : TestCaseWithFactory
			where TTransport : DtbTransport
			where TTransportFilterBizO : DtbTransportFilterBusinessObject<TTransport>, new()
	{
		#region TestGetNewFilterStripControl

		public void TestGetNewFilterStripControl()
		{
			using (var form = new ZForm())
			{
				var transports = GetTransportCollection();
				var filterBO = new TTransportFilterBizO();
				var filterControl = GetFilterControl(transports, filterBO);

				form.Controls.Add(filterControl);
				form.Show();

				filterControl.AddNewFilterStrip();
				Assert("Must return WorkflowFilterStrip so that workflow filter strips may be selected", filterControl.LastFilterStripType.IsSubclassOf(typeof(WorkflowFilterStrip)));
			}
		}

		protected abstract DtbTransportCollection<TTransport> GetTransportCollection();
		protected abstract DtbTransportFilterControl<TTransport> GetFilterControl(DtbTransportCollection<TTransport> gridCollection, TTransportFilterBizO filterBizO);

		#endregion
	}
}
