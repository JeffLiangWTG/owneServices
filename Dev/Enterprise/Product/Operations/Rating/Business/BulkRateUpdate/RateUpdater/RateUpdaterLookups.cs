using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class RateUpdaterLookups : ZLookups
	{
		public RateUpdaterLookups(RateUpdater parent)
			: base(parent) { }

		public CodeDescriptionPairList Printers
		{
			get { return printers ?? (printers = new DocDeliveryPrintDetails(Factory).PrinterNames); }
		}
		CodeDescriptionPairList printers;

		#region Implementation

		protected new RateUpdater Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (RateUpdater)base.Parent; }
		}

		#endregion
	}
}

