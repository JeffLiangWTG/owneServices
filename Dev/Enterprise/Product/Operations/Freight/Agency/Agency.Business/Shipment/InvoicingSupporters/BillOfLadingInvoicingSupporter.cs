using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class BillOfLadingInvoicingSupporter : AgencyShipmentInvoicingSupporter
	{
		public BillOfLadingInvoicingSupporter(BillOfLading parent)
			: base(parent)
		{
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.AgencyBillOfLadingAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.AgencyBillOfLadingJobInvoicing;
		}

		protected override SecurityCheckpoint GetEditSecurityCheckpointCore()
		{
			return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.AgencyBillOfLadingJobInvoicing, SecurityCore.AllowInvAmendmentsDays);
		}

		protected override ZString EditSecurityMessageCore
		{
			get
			{
				ZString message = ZString.Empty;
				if (EditSecurityLockCore)
				{
					message =
						Res.GetString("db495f95-ea67-4d33-8443-39dbf910b209", "You do not have security rights to make adjustments/posting of costs & charges to this job because this billing job exceeds the number of days ({0}) the changes are allowed (as set in the Registry:", AgencyRegistry.Instance.AllowInvoiceAmendmentsDays.Value) + " "
						+ ((IRegistryItemInternals)AgencyRegistry.Instance.AllowInvoiceAmendmentsDays).Location + "). " + (char)13 + (char)10 +
						Res.GetString("fe79e6fd-1589-45aa-a53b-7248c1d3e22b", "Contact the accounts department or a user who has security rights to override this restriction.") + (char)13 + (char)10 + (char)13 + (char)10 +
						Res.GetString("5d87033d-7e58-41ba-873f-69e4527f3335", "Please enter user name & password to save/post this change.");
				}

				return message;
			}
		}

		protected override bool EditSecurityLockCore
		{
			get
			{
				bool objLock =
					(!EditSecurityCutOffCore.IsEmpty)
					&& (EditSecurityCutOffCore < ZDateTime.Today)
					&& (EditSecurityCheckpoint != Env.Security.None)
					&& (!EditSecurityCheckpoint.IsAllowed);
				return objLock;
			}
		}

		ZDateTime EditSecurityCutOffCore
		{
			get
			{
				ZDateTime date = ZDateTime.Empty;
				if (AgencyRegistry.Instance.AllowInvoiceAmendmentsDays.Value > 0 &&
					Shipment.Sailing != null && Shipment.Sailing.Voyage != null)
				{
					if (IsExport)
					{
						foreach (VoyageOrigin origin in Shipment.Sailing.Voyage.Origins)
						{
							if (ImportExportHelper.IsBranchCountry(origin.JA_RL_NKPortOfLoading))
							{
								if (origin.JA_A_DEP.IsEmpty)
								{
									date = ZDateTime.Empty;
									break;
								}
								if (date.IsEmpty || origin.JA_A_DEP > date)
								{
									date = origin.JA_A_DEP;
								}
							}
						}
					}
					else
					{
						foreach (VoyageDestination destination in Shipment.Sailing.Voyage.Destinations)
						{
							if (ImportExportHelper.IsBranchCountry(destination.JB_RL_NKPortOfDischarge))
							{
								if (destination.JB_A_ARV.IsEmpty)
								{
									date = ZDateTime.Empty;
									break;
								}
								if (date.IsEmpty || destination.JB_A_ARV > date)
								{
									date = destination.JB_A_ARV;
								}
							}
						}
					}

					if (!date.IsEmpty)
					{
						date = date.AddDays((int)AgencyRegistry.Instance.AllowInvoiceAmendmentsDays.Value);
					}
				}
				return date;
			}
		}
	}
}


