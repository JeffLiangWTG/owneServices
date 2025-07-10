using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	#region Entitlement Codes Enum

	public enum EntitlementCodes
	{
		Carrier,
		Agent
	}

	#endregion

	[DependentBusinessObject(typeof(ShipmentExportAWBHeader), "AWBOtherCharges")]
	public class ExportAWBOtherCharges : Forwarding.AWB.Business.ExportAWBOtherCharges
	{
		public ExportAWBOtherCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			originalEO_EH = EO_EH;
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
			get { return Factory.Load<ExportAWBHeader>(IsDeleted || EO_EH.IsEmpty ? originalEO_EH : EO_EH); }
		}

		public void RefreshMaster()
		{
			if (Master != null)
			{
				Master.RefreshOtherChargesData();
			}
		}

		protected new ExportAWBHeader Master
		{
			get { return Factory.Load<ExportAWBHeader>(EO_EH); }
		}

		public override ZGuid EO_EH
		{
			get { return base.EO_EH; }
			set
			{
				base.EO_EH = value;
				if (!value.IsEmpty)
				{
					originalEO_EH = value;
				}
			}
		}

		ZGuid originalEO_EH;
	}
}
