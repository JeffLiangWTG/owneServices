using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class BrazilImportAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public BrazilImportAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		public bool IsApplicable()
		{
			return Parent.IsImportToBrazil;
		}

		protected override void CheckEH_AsAgreed1st()
		{
			base.CheckEH_AsAgreed1st();

			if (Parent.EH_AsAgreed1st != Core.Constants.AWB.AsAgreedTypes.Codes.None)
			{
				Parent.EH_AsAgreed1stInfo.AddError(BrazilImportAsAgreedNotAllowedMessage);
			}
		}

		protected override void CheckEH_AsAgreed2nd()
		{
			base.CheckEH_AsAgreed2nd();

			if (Parent.EH_AsAgreed2nd != Core.Constants.AWB.AsAgreedTypes.Codes.None)
			{
				Parent.EH_AsAgreed2ndInfo.AddError(BrazilImportAsAgreedNotAllowedMessage);
			}
		}

		protected string BrazilImportAsAgreedNotAllowedMessage =>
			Parent is ShipmentExportAWBHeader
			? Res.GetString("86d03361-3261-4c7e-b501-995a6d363e29", "As Agreed cannot be selected on HAWB for Imports to Brazil")
			: Res.GetString("f24057a7-3f32-41d0-bdab-62a57b8a0ab9", "As Agreed cannot be selected on Master Bill for Imports to Brazil");
	}
}
