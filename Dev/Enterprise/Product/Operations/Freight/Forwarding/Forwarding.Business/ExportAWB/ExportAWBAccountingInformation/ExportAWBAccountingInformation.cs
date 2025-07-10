using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DependentBusinessObject(typeof(ConsolExportAWBHeader), "AWBAccountingInformations")]
	public class ExportAWBAccountingInformation : Forwarding.AWB.Business.ExportAWBAccountingInformation
	{
		public ExportAWBAccountingInformation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			originalEA_EH = EA_EH;
		}

		public override bool IsSavedByFactory
		{
			get
			{
				ExportAWBHeader.SaveMode saveMode = OriginalMaster != null ? OriginalMaster.FactorySaveMode : ExportAWBHeader.SaveMode.Normal;
				switch (saveMode)
				{
					case ExportAWBHeader.SaveMode.Normal:
						return base.IsSavedByFactory;
					case ExportAWBHeader.SaveMode.Forced:
						return true;
					default:
						return false;
				}
			}
		}

		ExportAWBHeader OriginalMaster
		{
			get { return Factory.Load<ExportAWBHeader>(IsDeleted || EA_EH.IsEmpty ? originalEA_EH : EA_EH); }
		}

		public override ZGuid EA_EH
		{
			get { return base.EA_EH; }
			set
			{
				base.EA_EH = value;
				if (!value.IsEmpty)
				{
					originalEA_EH = value;
				}
			}
		}
		ZGuid originalEA_EH;

		public bool IsItalianRegistrationCode
		{
			get { return EA_InformationID == ExportAWBAccountingInformationLookups.IssuedByIVA || EA_InformationID == ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA; }
		}

		protected override Forwarding.AWB.Business.ExportAWBAccountingInformationLookups GetNewLookups()
		{
			return new ExportAWBAccountingInformationLookups(this);
		}

		public override bool IsSkippedOnMessaging
		{
			get { return IsItalianRegistrationCode; }
		}
	}
}
