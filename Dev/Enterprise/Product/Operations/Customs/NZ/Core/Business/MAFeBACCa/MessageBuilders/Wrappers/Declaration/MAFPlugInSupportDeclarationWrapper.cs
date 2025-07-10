namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.DocumentEngineCore.DocumentSupport;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.ZArchitecture.Business;

	public partial class MAFPlugInSupportDeclarationWrapper : IMAFPlugInSupport
	{
		public MAFPlugInSupportDeclarationWrapper(JobDeclaration declaration)
		{
			this.declaration = declaration;
			Argument.NotNull(declaration, "declaration");
		}

		#region Implementation of IMAFPlugInSupport

		Logs IMAFPlugInSupport.Logs
		{
			get { return declaration.Logs; }
		}

		bool IMAFPlugInSupport.IsPlugInNew
		{
			get { return false; }
		}

		bool IMAFPlugInSupport.PlugInVisible
		{
			get { return declaration.IsImport && !declaration.IsTSWDeclaration; }
		}

		bool IMAFPlugInSupport.CargoTypeVisible
		{
			get { return declaration.IsSea; }
		}

		event EventHandler IMAFPlugInSupport.PlugInVisibilityDataChanged
		{
			add
			{
				declaration.JE_MessageTypeInfo.ValueChanged += value;
				declaration.JE_ApplicationCodeInfo.ValueChanged += value;
			}

			remove
			{
				declaration.JE_MessageTypeInfo.ValueChanged -= value;
				declaration.JE_ApplicationCodeInfo.ValueChanged -= value;
			}
		}

		IDocumentEvents IMAFPlugInSupport.DocumentEvents
		{
			get { return declaration.Shipment == null ? declaration.DocumentSupporter : declaration.Shipment.DocumentSupporter; }
		}

		#endregion

		#region Implementation of IMAFMessagingSource

		BusinessObject IMAFMessagingSource.Master
		{
			get { return declaration; }
		}

		IMAFOrganisation IMAFMessagingSource.Broker
		{
			get
			{
				return MAFOrganisationWrapper.GetMAFOrganisation(
					NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant(),
					GlbCompany.CurrentCompany,
					declaration.Branch ?? GlbBranch.CurrentBranch,
					GlbStaff.CurrentUser);
			}
		}

		ZString IMAFMessagingSource.OriginCountry
		{
			get { return declaration.JE_RL_NKOrigin.Left(2); }
		}

		IEnumerable<ZString> IMAFMessagingSource.DischargePorts
		{
			get { yield return declaration.JE_RL_NKPortOfArrival; }
		}

		IEnumerable<ZString> IMAFMessagingSource.Destinations
		{
			get { yield return declaration.JE_RL_NKFinalDestination; }
		}

		ZString IMAFMessagingSource.ShipName
		{
			get { return declaration.IsSea ? declaration.JE_VesselName : ZString.Empty; }
		}

		ZString IMAFMessagingSource.VoyageNumber
		{
			get { return declaration.IsSea ? declaration.JE_VoyageFlightNo : ZString.Empty; }
		}

		ZString IMAFMessagingSource.ShippingCompany
		{
			get { return declaration.IsSea && declaration.ShippingLine != null ? declaration.ShippingLine.OH_FullNameTruncated : ZString.Empty; }
		}

		ZDateTime IMAFMessagingSource.VoyageArrivalDate
		{
			get { return declaration.IsSea ? declaration.JE_DateOfArrival : ZDateTime.Empty; }
		}

		ZString IMAFMessagingSource.FlightNumber
		{
			get { return declaration.IsAir ? declaration.JE_VoyageFlightNo : ZString.Empty; }
		}

		ZDateTime IMAFMessagingSource.FlightArrivalDate
		{
			get { return declaration.IsAir ? declaration.JE_DateOfArrival : ZDateTime.Empty; }
		}

		IEnumerable<ZString> IMAFMessagingSource.BillOfLadingNumbers
		{
			get { return from Bill bill in declaration.Bills where bill.IsMasterBill select bill.CU_MasterBill; }
		}

		IEnumerable<ZString> IMAFMessagingSource.SubBillOfLadingNumbers
		{
			get { return from Bill bill in declaration.Bills where !bill.IsMasterBill select bill.CU_BillNum; }
		}

		IEnumerable<IMAFContainer> IMAFMessagingSource.Containers
		{
			get
			{
				return from CusContainer container in declaration.CusContainers
					   select (IMAFContainer)new ContainerData
					   {
						   ContainerNumber = container.CO_ContainerNumber,
						   ContainerType = container.CO_MAF_ContainerType,
						   IsFullContainer = container.IsFullContainer
					   };
			}
		}

		ZString IMAFMessagingSource.ConsignmentDescription
		{
			get { return declaration.JE_GoodsDescription; }
		}

		IEnumerable<IMAFCommodity> IMAFMessagingSource.Commodities
		{
			get
			{
				return declaration.JE_MessageSubType == JobMessageSubTypeList.Codes.WriteOff ? new[] { new MAFCommodityDeclarationWrapper(declaration) }
					: from CusEntryLine entryLine in declaration.CusEntryHeader.MergedLines select (IMAFCommodity)new MAFCommodityEntryLineWrapper(entryLine);
			}
		}

		ZInt IMAFMessagingSource.CustomsEntryNumber
		{
			get
			{
				int entryNumber;
				var entryHeader = declaration.CusEntryHeader;
				return int.TryParse(entryHeader.EntryNumber, out entryNumber)
					   && !(entryHeader is Declaration.ECIWriteOff.Manifesting.CusEntryHeader)
						? entryNumber : 0;
			}
		}

		bool IMAFMessagingSource.IsECIWriteoff
		{
			get { return declaration.IsECIWriteoff; }
		}

		ZString IMAFMessagingSource.ClientReferenceCoverSheet
		{
			get { return declaration.JE_DeclarationReference + (declaration.JE_OwnerRef.IsEmpty ? "" : " / " + declaration.JE_OwnerRef); }
		}

		IMAFOrganisation IMAFMessagingSource.TransitionalFacility
		{
			get
			{
				return MAFOrganisationWrapper.GetMAFOrganisation(declaration.DepotDocAddress, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility)
					   ?? MAFOrganisationWrapper.GetMAFOrganisation(declaration.ImporterDeliveryAddress, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility);
			}
		}

		IMAFOrganisation IMAFMessagingSource.TreatmentProvider
		{
			get { return MAFOrganisationWrapper.GetMAFOrganisation(declaration.TreatmentProviderDocAddress); }
		}

		#endregion

		#region Implementation of IMAFMessagingFallback

		ZString IMAFMessagingFallback.ProcessingOffice
		{
			get { return declaration.JE_RL_NKPortOfArrival; }
		}

		ZString IMAFMessagingFallback.ConsignmentType
		{
			get
			{
				return declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.PrivateImportTransactionFee) != null
						? ConsignmentTypeList.Codes.PrivateCargo : ConsignmentTypeList.Codes.CommercialCargo;
			}
		}

		ZString IMAFMessagingFallback.CargoType
		{
			get { return declaration.IsSea ? CargoTypeList.GetDefaultCargoType(declaration.CusContainers) : string.Empty; }
		}

		ZString IMAFMessagingFallback.MeasurementUQ
		{
			get
			{
				var packUnit = MeasurementUQList.TranslateFromCustomsPackageTypeCode(declaration.JE_TotalNoOfPacksPackType);
				return ((IMAFMessagingFallback)this).MeasurementValue > 0 ? packUnit : string.Empty;
			}
		}

		ZInt IMAFMessagingFallback.MeasurementValue
		{
			get { return declaration.JE_TotalNoOfPacks; }
		}

		IMAFAccountDetails IMAFMessagingFallback.AccountDetails
		{
			get
			{
				var orgs = new Dictionary<string, OrgHeader> { { "Importer", declaration.Importer } };
				if (declaration.Branch != null)
				{
					orgs["Branch Organization"] = declaration.Branch.OrgProxy;
				}

				return MAFAccountDetailsWrapper.GetAccountDetails(orgs);
			}
		}

		IMAFOrganisation IMAFMessagingFallback.Importer
		{
			get { return MAFOrganisationWrapper.GetMAFOrganisation(declaration.Importer, OrgCusCode.CodeTypes.CustomsClientCode, ContactType.Consignee); }
		}

		IMAFOrganisation IMAFMessagingFallback.Exporter
		{
			get { return MAFOrganisationWrapper.GetMAFOrganisation(declaration.Supplier, OrgCusCode.CodeTypes.SupplierCode, ContactType.Consignor); }
		}

		#endregion

		#region Implementation of IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return declaration.GetDocManagerInfo(); }
		}

		#endregion

		#region Implementation of IHaveNZAddInfo

		NZAddInfo IHaveNZAddInfo.AddInfo
		{
			get { return ((IHaveNZAddInfo)declaration).AddInfo; }
		}

		#endregion

		#region Implementation of IJobNumber

		string IJobNumber.JobNumber
		{
			get { return ((IJobNumber)declaration).JobNumber; }
		}

		#endregion

		#region ContainerData

		class ContainerData : IMAFContainer
		{
			public ZString ContainerNumber { get; set; }
			public ZString ContainerType { get; set; }
			public ZBool IsFullContainer { get; set; }
		}

		#endregion

		readonly JobDeclaration declaration;
	}
}
