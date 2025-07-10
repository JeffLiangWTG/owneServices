using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	[CodeAlive("ICR is WIP development - Currently disabled - ICR from ASYCUDA Manifest Header is still under development and to be determined - Dean / BKG.")]
	public class ICRManifestHeaderWrapper : IInwardCargoReport
	{
		public ICRManifestHeaderWrapper(AsycudaManifestHeader manifest, IAdditionalInformation additionalInformation)
		{
			this.manifest = Argument.NotNull(manifest, "manifest cannot be null");
			this.additionalInformation = additionalInformation;
		}

		readonly AsycudaManifestHeader manifest;
		readonly IAdditionalInformation additionalInformation;

		ZString IInwardCargoReport.SenderReferenceNumber => manifest.AMA_JobReference;

		ZString IInwardCargoReport.TSWReferenceNumber => manifest.AMA_MasterBill;

		ZBool IInwardCargoReport.IsSea => manifest.IsSea;

		ZBool IInwardCargoReport.IsCarrierCargoReport => false;

		ZString IInwardCargoReport.CraftName => manifest.AMA_VesselName;

		ZString IInwardCargoReport.LloydsNo => manifest.Vessel?.RV_LloydsNumber ?? ZString.Empty;

		ZString IInwardCargoReport.VoyageNo => manifest.AMA_Voyage;

		ZString IInwardCargoReport.FlightNo => manifest.AMA_Voyage;

		ZDateTime IInwardCargoReport.ArrivalDate => manifest.AMA_E_ARV;

		ZString IInwardCargoReport.PortOfArrival => manifest.AMA_RL_NKPortOfFirstArrival;

		IOrganisationSimple IInwardCargoReport.Carrier => carrier ?? (carrier = OrgHeaderWrapper.New(manifest.Carrier?.Header));
		IOrganisationSimple carrier;

		IDeclarant IInwardCargoReport.Declarant => declarant ?? (declarant = new TSWGlbStaffWrapper(GlbStaff.CurrentUser));
		IDeclarant declarant;

		IAdditionalInformation IInwardCargoReport.AdditionalInformation => additionalInformation;

		ZBool IInwardCargoReport.UseInterfaceSequenceNumber => ZBool.False;

		IEnumerable<IICRConsignment> IInwardCargoReport.Consignments
		{
			get
			{
				foreach (AsycudaBill bill in manifest.Bills)
				{
					yield return new ICRBillWrapper(bill);
				}
			}
		}

		ZString IInwardCargoReport.MPIAccountDetails => ZString.Empty;

		IEnumerable<ITSWAttachment> IInwardCargoReport.SupportingDocuments => Array.Empty<ITSWAttachment>();
	}
}
