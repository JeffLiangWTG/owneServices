using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class DetentionAdviceDeliveryLookups : ZLookups
	{
		public DetentionAdviceDeliveryLookups(DetentionAdviceDelivery parent)
			: base(parent) { }

		public CodeDescriptionPairList Modes
		{
			get { return modes ?? (modes = new DetentionAdviceDeliveryMode()); }
		}
		CodeDescriptionPairList modes;

		public CodeDescriptionPairList Printers
		{
			get
			{
				if (printers == null)
				{
					ZQuery filter = new ZQuery(StmPrintQueueSchema.SQ_AllowPrinting, true);

					StmPrintQueueCollection printQueues = new StmPrintQueueCollection(Parent.CurrentFactory, filter);
					printQueues.Load();
					printers = printQueues.GetOnlinePrinterNames();
				}

				return printers;
			}
		}
		CodeDescriptionPairList printers;

		public GlbGroupCollection Groups
		{
			get { return new GlbGroupCollection(Parent.CurrentFactory); }
		}

		#region Implementation

		new DetentionAdviceDelivery Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DetentionAdviceDelivery)base.Parent; }
		}

		#endregion
	}
}
