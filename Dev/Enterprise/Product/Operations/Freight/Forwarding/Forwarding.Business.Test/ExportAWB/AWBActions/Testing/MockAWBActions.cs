using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class MockAWBActions : AWBActions
	{
		public MockAWBActions(ActionsModeType actionsMode, BusinessObjectFactory factory) : base(actionsMode, factory)
		{
		}

		public override ZString ParentName => throw new NotImplementedException();

		protected override ExportAWBHeader AWB => awb;
		ExportAWBHeader awb;

		public void SetAWB(ExportAWBHeader awb)
		{
			this.awb = awb;
		}

		protected override IDocumentSupportable DocumentSupportable => throw new NotImplementedException();

		protected override ZInt PackagesNumber => throw new NotImplementedException();

		public ZString DocumentSize { get; set; }
		public ZInt LabelStartRange { get; set; }
		public ZInt MAWBLabelStartRange { get; set; }
		public ZInt LabelEndRange { get; set; }
		public ZInt LabelTotalPacks { get; set; }
		public ZInt MAWBLabelTotalPacks { get; set; }

		protected override void SetDocumentSettingsCore()
		{
			AWB.DocumentSize = DocumentSize;
			AWB.LabelStartRange = LabelStartRange;
			AWB.MAWBLabelStartRange = MAWBLabelStartRange;
			AWB.LabelEndRange = LabelEndRange;
			AWB.LabelTotalPacks = LabelTotalPacks;
			AWB.MAWBLabelTotalPacks = MAWBLabelTotalPacks;
			AWB.PrintOptionalInformation = PrintOptionalInformation;
		}
	}
}
