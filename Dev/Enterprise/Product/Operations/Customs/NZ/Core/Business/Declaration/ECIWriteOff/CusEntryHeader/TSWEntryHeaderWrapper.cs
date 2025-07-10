using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff
{
	public class TSWEntryHeaderWrapper : DeclarationConsignmentWrapper, ICargoReportExport, IInwardCargoReport
	{
		public TSWEntryHeaderWrapper(CusEntryHeader entryHeader, IAdditionalInformation additionalInformation)
			: base(entryHeader == null ? null : entryHeader.Declaration)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "EntryHeader cannot be null");
			declaration = entryHeader.Declaration;
			this.additionalInformation = additionalInformation;
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly IAdditionalInformation additionalInformation;
		Manifesting.CusEntryHeader manifestEntryHeader;

		#region CRE

		#region ICargoReportExport Implementation

		ZBool ICargoReportExport.IsSea
		{
			get { return declaration.IsSea; }
		}

		ZBool ICargoReportExport.IsAir
		{
			get { return declaration.IsAir; }
		}

		ZBool ICargoReportExport.IsMail
		{
			get { return declaration.IsPost; }
		}

		ZBool ICargoReportExport.IsContainerised
		{
			get { return declaration.CusContainers.Count > 0; }
		}

		ZBool ICargoReportExport.HasEmptyContainersOnly
		{
			get { return declaration.HasContainersAndTheyreAllEmpty && declaration.InvoiceLines.Count == 0; }
		}

		ZString ICargoReportExport.SenderReferenceNumber
		{
			get { return entryHeader.CH_BGMReference.GetSenderReferenceNumber(); }
		}

		ZString ICargoReportExport.TSWReferenceNumber
		{
			get { return entryHeader.EntryNumber; }
		}

		IOrganisationSimple ICargoReportExport.Carrier
		{
			get { return OrgHeaderWrapper.New(declaration.ShippingLine); }
		}

		IAdditionalInformation ICargoReportExport.AdditionalInformation
		{
			get { return additionalInformation; }
		}

		ZString ICargoReportExport.CraftName
		{
			get { return declaration.JE_VesselName; }
		}

		ZString ICargoReportExport.LloydsNo
		{
			get { return declaration.Vessel != null ? declaration.Vessel.RV_LloydsNumber : ZString.Empty; }
		}

		ZString ICargoReportExport.VoyageNo
		{
			get { return declaration.JE_VoyageFlightNo.Left(8); }
		}

		ZString ICargoReportExport.FlightNo
		{
			get { return declaration.JE_VoyageFlightNo; }
		}

		ZDateTime ICargoReportExport.DepartureDate
		{
			get { return declaration.JE_ExportDate; }
		}

		ZBool ICargoReportExport.UseInterfaceSequenceNumber => false;

		IEnumerable<ICREConsignment> ICargoReportExport.Consignments
		{
			get
			{
				manifestEntryHeader = entryHeader as Manifesting.CusEntryHeader;
				if (manifestEntryHeader != null && additionalInformation != null && additionalInformation.SendManifest)
				{
					foreach (JobDeclaration declaration in manifestEntryHeader.Declarations)
					{
						yield return new DeclarationConsignmentWrapper(declaration);
					}
				}
				else
				{
					yield return this;
				}
			}
		}

		IDeclarant ICargoReportExport.Declarant
		{
			get { return new TSWGlbStaffWrapper(GlbStaff.CurrentUser); }
		}

		ZString ICargoReportExport.PortOfDeparture
		{
			get
			{
				return declaration.IsExport ? declaration.JE_RL_NKPortOfLoading : declaration.JE_RL_NKPortOfArrival;
			}
		}

		IEnumerable<ITSWAttachment> ICargoReportExport.SupportingDocuments => Array.Empty<ITSWAttachment>();

		#endregion

		#region Overrides

		protected override IEnumerable<ICREConsignmentItem> GetConsignmentItems
		{
			get
			{
				if (declaration.IsTSWEmptyContainerWriteOff || declaration.HasCusContainers)
				{
					int containerCount = 0;
					foreach (CusContainer container in declaration.CusContainers)   // A separate consignment item is required for each container
					{
						if (container.CO_FCL_LCL_AIR == Core.Constants.ContainerModes.Empty)
						{
							yield return new EmptyContainerConsignmentItemWrapper(container);
						}
						else
						{
							containerCount++;
							yield return new ContainerConsignmentItemWrapper(container, containerCount != 1);
						}
					}
				}
				else
				{
					if (declaration.InvoiceLines.Count > 0)
					{
						foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
						{
							yield return new CREJobComInvoiceLineWrapper(invoiceLine);   // line information like dangerous goods code or classification has been used for this entry
						}
					}
					else
					{
						yield return this;
					}
				}
			}
		}

		protected override IEnumerable<IICRConsignmentItem> GetICRConsignmentItems
		{
			get
			{
				if (!declaration.IsTSWEmptyContainerWriteOff && declaration.InvoiceLines.Count > 0)
				{
					foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
					{
						yield return new ICRJobComInvoiceLineWrapper(invoiceLine);   // line information has been used for this entry
					}
				}
				else if (declaration.HasCusContainers)
				{
					int containerCount = 0;
					foreach (CusContainer container in declaration.CusContainers)   // A separate consignment item is required for each container
					{
						if (container.CO_FCL_LCL_AIR == Core.Constants.ContainerModes.Empty)
						{
							yield return new EmptyContainerConsignmentItemWrapper(container);
						}
						else
						{
							containerCount++;
							yield return new ContainerConsignmentItemWrapper(container, containerCount != 1);
						}
					}
				}
				else
				{
					yield return this;
				}
			}
		}

		#endregion

		#endregion

		#region ICR

		#region IInwardCargoReport Implementation

		ZString IInwardCargoReport.SenderReferenceNumber
		{
			get { return entryHeader.CH_BGMReference.GetSenderReferenceNumber(); }
		}

		ZString IInwardCargoReport.TSWReferenceNumber
		{
			get { return entryHeader.EntryNumber; }
		}

		ZBool IInwardCargoReport.IsSea
		{
			get { return declaration.IsSea; }
		}

		ZBool IInwardCargoReport.IsCarrierCargoReport
		{
			get { return false; }   // TODO: implement this if ever required for stand-alone ICR from declaration.
		}

		ZString IInwardCargoReport.CraftName
		{
			get { return declaration.JE_VesselName; }
		}

		ZString IInwardCargoReport.LloydsNo
		{
			get { return declaration.Vessel != null ? declaration.Vessel.RV_LloydsNumber : ZString.Empty; }
		}

		ZString IInwardCargoReport.VoyageNo
		{
			get { return declaration.JE_VoyageFlightNo.Left(8); }
		}

		ZString IInwardCargoReport.FlightNo
		{
			get { return declaration.JE_VoyageFlightNo; }
		}

		ZDateTime IInwardCargoReport.ArrivalDate
		{
			get { return declaration.JE_DateOfArrival; }
		}

		ZString IInwardCargoReport.PortOfArrival
		{
			get { return declaration.JE_RL_NKPortOfArrival; }
		}

		IOrganisationSimple IInwardCargoReport.Carrier
		{
			get { return OrgHeaderWrapper.New(declaration.ShippingLine); }
		}

		ZBool IInwardCargoReport.UseInterfaceSequenceNumber => false;

		IEnumerable<IICRConsignment> IInwardCargoReport.Consignments
		{
			get
			{
				manifestEntryHeader = entryHeader as Manifesting.CusEntryHeader;
				if (manifestEntryHeader != null && additionalInformation != null && additionalInformation.SendManifest)
				{
					var declarationsForManifest = manifestEntryHeader.Declarations;
					declarationsForManifest.Sort(JobDeclaration.Schema.JE_DeclarationReference, System.ComponentModel.ListSortDirection.Ascending);
					foreach (JobDeclaration declaration in declarationsForManifest)
					{
						if (declaration.IsECIWriteoff)
						{
							yield return new DeclarationConsignmentWrapper(declaration);
						}
					}
				}
				else
				{
					yield return this;
				}
			}
		}

		IDeclarant IInwardCargoReport.Declarant
		{
			get { return new TSWGlbStaffWrapper(GlbStaff.CurrentUser); }
		}

		IAdditionalInformation IInwardCargoReport.AdditionalInformation
		{
			get { return additionalInformation; }
		}

		ZString IInwardCargoReport.MPIAccountDetails => ZString.Empty;

		IEnumerable<ITSWAttachment> IInwardCargoReport.SupportingDocuments => Array.Empty<ITSWAttachment>();

		#endregion
		#endregion
	}
}
