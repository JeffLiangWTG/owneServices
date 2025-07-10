using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class CFSLoadListConsumerType : JobInvoicingConsumerType
	{
		public CFSLoadListConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.LoadListConsol; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<CFS.ICFSLoadListConsol>(); }
		}

		public override bool IsTransportModeSupported
		{
			get { return true; }
		}

		public override bool IsDirectionSupported
		{
			get { return true; }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCFS; }
		}
		public override bool SupportsWiseRates
		{
			get { return true; }
		}
	}
}
