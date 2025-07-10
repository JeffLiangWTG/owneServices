using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class CalculateGreenhouseGasEmissionsActionMethod : OperationalActionMethod
	{
		public CalculateGreenhouseGasEmissionsActionMethod() : base(new Guid("E53B3B55-28A4-4802-832C-4B70F6F6B063"))
		{
		}

		public override string Name
		{
			get { return Res.GetString("9A2DBAC3-DB75-4436-810D-5E2B04E0A676", "Calculate Greenhouse Gas Emissions"); }
		}

		public override string Description
		{
			get { return Res.GetString("21507AF6-659D-46EA-9128-26609AF25B3E", "Calculate the greenhouse gas emissions for selected shipments."); }
		}

		public override bool HasControl
		{
			get { return false; }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CalculateGreenhouseGasEmissionsApplicator();
		}

		public override bool RunWithoutResultLogging
		{
			get { return true; }
		}
	}
}
