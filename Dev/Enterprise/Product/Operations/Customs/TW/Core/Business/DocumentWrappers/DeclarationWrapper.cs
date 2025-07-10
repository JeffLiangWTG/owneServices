using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.TW.Business
{
	public class DeclarationWrapper : DocumentWrapper
	{
		DeclarationWrapper(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap) : base(jobDeclaration, factoryToWrap)
		{
			DeclarationBO = jobDeclaration ?? factoryToWrap.GetNull<JobDeclaration>();
		}

		public readonly JobDeclaration DeclarationBO;

		public static DeclarationWrapper New(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
		{
			return new DeclarationWrapper(jobDeclaration, factoryToWrap);
		}

		#region Field

		public ZString PortOfOriginName => DocumentWrapperHelper.GetOriginProperNameWithCountry(DeclarationBO, " - ");

		public ZString FinalDestinationName => DocumentWrapperHelper.GetFinalDestinationProperNameWithCountry(DeclarationBO, " - ");

		public ZString Transportation
		{
			get
			{
				var result = ZString.Empty;
				switch (DeclarationBO.JE_TransportMode)
				{
					case TransportTypeList.Codes.Sea:
						result = DeclarationBO.JE_VesselName;
						break;
					case TransportTypeList.Codes.Air:
						result = Enterprise.DocumentWrappers.GenericWrappers.TransportModeList.Descriptions.Airfreight.ToString().ToUpper();
						break;
				}
				return result;
			}
		}

		public ZString GoodsDescription => DeclarationBO.JE_GoodsDescription;

		public ZString PackageDescription => DeclarationBO.CusEntryInstruction.CEI_PackageDescription;

		#endregion

		public DocJobDocAddress SupplierJobDocAddress => supplierJobDocAddress ??= DocJobDocAddress.New(DeclarationBO.SupplierDocumentaryAddress, Factory);
		DocJobDocAddress supplierJobDocAddress;

		public DocJobDocAddress ImporterJobDocAddress => importerJobDocAddress ??= DocJobDocAddress.New(DeclarationBO.ImporterDocumentaryAddress, Factory);
		DocJobDocAddress importerJobDocAddress;

		public DocumentaryAddressDetailsWrapper SupplierAddressData
		{
			get
			{
				if (supplierAddressData == null)
				{
					var supplierDocumentaryAddress = DeclarationBO.SupplierDocumentaryAddress;
					if (supplierDocumentaryAddress != null && supplierDocumentaryAddress.Address != null)
					{
						supplierAddressData = new ExportExporterDocumentaryAddressDetailsWrapper(DeclarationBO.Supplier, supplierDocumentaryAddress);
					}
				}
				return supplierAddressData;
			}
		}
		DocumentaryAddressDetailsWrapper supplierAddressData;

		public DocumentaryAddressDetailsWrapper ImporterAddressData
		{
			get
			{
				if (importerAddressData == null)
				{
					var importerDocumentaryAddress = DeclarationBO.ImporterDocumentaryAddress;
					if (importerDocumentaryAddress != null && importerDocumentaryAddress.Address != null)
					{
						importerAddressData = new ExportBuyerDocumentaryAddressDetailsWrapper(DeclarationBO.Importer, importerDocumentaryAddress);
					}
				}
				return importerAddressData;
			}
		}
		DocumentaryAddressDetailsWrapper importerAddressData;

		public DocOrganisation Notify => notify ??= DocOrganisation.New(DeclarationBO.NotifyParty, Factory);
		DocOrganisation notify;

		public IPartyDetails NotifyPartyDetails
		{
			get
			{
				if (notifyParty == null)
				{
					var notifyPartyOrg = DeclarationBO.NotifyParty;
					if (notifyPartyOrg != null)
					{
						notifyParty = new PartyDetailsWrapper("", "", "", notifyPartyOrg.MainAddress);
					}
				}
				return notifyParty;
			}
		}

		IPartyDetails notifyParty;
	}
}
