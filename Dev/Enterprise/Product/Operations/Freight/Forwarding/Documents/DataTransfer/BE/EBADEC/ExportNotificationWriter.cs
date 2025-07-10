using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.BE
{
	sealed class ExportNotificationWriter : DataObjectWriter<ExportNotification, UniversalShipment>
	{
		public ExportNotificationWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(ExportNotification exportNotification)
		{
			var uxmlShipment = CreateUniveralShipment(exportNotification);

			uxmlShipment.PlaceOfReceipt = exportNotification.PortOfDestination.ToUXmlUnloco();
			uxmlShipment.PortOfLoading = exportNotification.PortOfOrigin.ToUXmlUnloco();
			uxmlShipment.ContainerMode = exportNotification.ContainerMode?.ToUXmlContainerMode();
			uxmlShipment.ShipmentType = exportNotification.ShipmentType?.ToUXmlCodeDescriptionPair();
			uxmlShipment.BookingConfirmationReference = exportNotification.BookingReference;

			PopulateAdditionalReferences(exportNotification, uxmlShipment);
			PopulateAddInfos(exportNotification, uxmlShipment);
			PopulateAddresses(exportNotification, uxmlShipment);
			PopulateSubShipments(exportNotification, uxmlShipment);

			return uxmlShipment;
		}

		#region CreateShipment

		UniversalShipment CreateUniveralShipment(ExportNotification exportNotification)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalShipment.DataContext = exportNotification
				.CreateUXmlDataContext()
				.AddDataProvider()
				.AddUserBranchAndDepartment();

			return universalShipment;
		}

		UniversalShipment CreateUniversalSubShipment(ExportNotification exportNotification, ZString mrn)
		{
			var universalSubShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalSubShipment.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
				{
					Type = "MRN",
					Key = mrn
				}
			};

			return universalSubShipment;
		}

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(ExportNotification exportNotification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(exportNotification));
				return additionalReferences.Any() ? additionalReferences : null;
			});
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(ExportNotification exportNotification)
		{
			if (!exportNotification.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = exportNotification.ConsolNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		#endregion

		#region PopulateAddInfos

		void PopulateAddInfos(ExportNotification exportNotification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				AddAddInfo(addInfos, "TransportToTerminalType_Code", exportNotification.TransportModeToTerminal?.Code ?? ZString.Empty);
				AddAddInfo(addInfos, "Terminal_Code", exportNotification.Terminal);
				AddAddInfo(addInfos, "OperationalPort_Code", exportNotification.PortOfOrigin?.Code ?? ZString.Empty);
				AddAddInfo(addInfos, "OperationalPort_Name", exportNotification.PortOfOrigin?.Name ?? ZString.Empty);
				AddAddInfo(addInfos, "VesselType_Code", exportNotification.VesselType);
				AddAddInfo(addInfos, "Is_FerryTerminal", exportNotification.IsFerryTerminal ? "Y" : "N");

				if (addInfos.Any())
				{
					addInfos.Add(new AddInfo { Key = DocDataConstants.AddinfoTypes.FormVersion, Value = "1.0.0" }); // Programmatic version number
				}

				return addInfos.Any() ? addInfos : null;
			});
		}

		void PopulateAddInfosSubShipment(DocumentVisualizer.DocDataObjects.ICodeDescription customsDocumentCode, ZString customsOfficeCode, UniversalShipment uxmlSubShipment)
		{
			uxmlSubShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				AddAddInfo(addInfos, "Entry_DocumentCode", customsDocumentCode.Code);
				AddAddInfo(addInfos, "Customs_OfficeCode", customsOfficeCode);

				if (addInfos.Any())
				{
					addInfos.Add(new AddInfo { Key = DocDataConstants.AddinfoTypes.FormVersion, Value = "1.0.0" }); // Programmatic version number
				}

				return addInfos.Any() ? addInfos : null;
			});
		}

		void AddAddInfo(List<AddInfo> addinfos, ZString key, ZString value)
		{
			if (!value.IsEmpty)
			{
				addinfos.Add(new AddInfo
				{
					Key = key,
					Value = value
				});
			}
		}

		#endregion

		#region PopulateAddresses

		void PopulateAddresses(ExportNotification exportNotification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!exportNotification.DepartureCTOAddress.IsEmpty())
				{
					addresses.Add(exportNotification.DepartureCTOAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCTOAddress), writeManager.WriterStrategy));
				}

				if (!exportNotification.SendingForwarderAddress.IsEmpty())
				{
					addresses.Add(exportNotification.SendingForwarderAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy));
				}

				if (!exportNotification.CurrentUser.IsEmpty())
				{
					addresses.Add(exportNotification.CurrentUser.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(exportNotification.SendingPartyCode)));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		IEnumerable<UniversalDataBuss.DataObjects.Universal.RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => x != null && !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}

		#endregion

		#region PopulateSubShipments

		void PopulateSubShipments(ExportNotification exportNotification, UniversalShipment uxmlShipment)
		{
			var subShipmentsList = new DataObjectList<UniversalShipment>();

			var subShipments = new List<SubShipment>();
			foreach (var packLine in exportNotification.PackLines)
			{
				foreach (var mrn in packLine.MovementReferenceNumbers)
				{
					var subShipment = new SubShipment(packLine.ContainerNumber, packLine.VIN, mrn.MRN, mrn.CustomsDocumentCode, mrn.CustomsOfficeCode);
					subShipments.Add(subShipment);
				}
			}

			var subShipmentsGroupedByMRN = subShipments.Where(subShipment => !subShipment.MRN.IsEmpty).GroupBy(subShipment => subShipment.MRN);
			foreach (var mrnGroup in subShipmentsGroupedByMRN)
			{
				var uxmlSubShipment = CreateUniversalSubShipment(exportNotification, mrnGroup.FirstOrDefault().MRN);
				subShipmentsList.Add(uxmlSubShipment);

				var uxmlPackingLineList = new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>();
				var packingLineNumber = 1;
				foreach (var packingLine in mrnGroup)
				{
					var universalPackingLine = new UniversalDataBuss.DataObjects.Universal.PackingLine(writeManager.WriterStrategy);
					universalPackingLine.ExportReferenceNumber = packingLine.MRN;
					if (!packingLine.ContainerNumber.IsEmpty)
					{
						universalPackingLine.ContainerNumber = packingLine.ContainerNumber;
					}
					if (!packingLine.VIN.IsEmpty)
					{
						universalPackingLine.ReferenceNumber = packingLine.VIN;
					}

					if (packingLineNumber == 1)
					{
						PopulateAddInfosSubShipment(packingLine.CustomsDocumentCode, packingLine.CustomsOfficeCode, uxmlSubShipment);
					}

					uxmlPackingLineList.Add(universalPackingLine);

					packingLineNumber++;
				}
				uxmlSubShipment.SetPackingLineCollection(() => uxmlPackingLineList);
			}

			uxmlShipment.SetSubShipmentCollection(() => subShipmentsList.Any() ? subShipmentsList : null);
		}

		class SubShipment
		{
			public SubShipment(ZString containerNumber, ZString vin, ZString mrn, DocumentVisualizer.DocDataObjects.ICodeDescription customsDocumentCode, ZString customsOfficeCode)
			{
				this.ContainerNumber = containerNumber;
				this.VIN = vin;
				this.MRN = mrn;
				this.CustomsDocumentCode = customsDocumentCode;
				this.CustomsOfficeCode = customsOfficeCode;
			}

			#region ContainerNumber

			public ZString ContainerNumber
			{
				get => containerNumber;
				set => containerNumber = value;
			}
			ZString containerNumber;

			#endregion

			#region VIN

			public ZString VIN
			{
				get => vin;
				set => vin = value;
			}
			ZString vin;

			#endregion

			#region MRN

			public ZString MRN
			{
				get => mrn;
				set => mrn = value;
			}
			ZString mrn;

			#endregion

			#region CustomsDocumentCode

			public DocumentVisualizer.DocDataObjects.ICodeDescription CustomsDocumentCode
			{
				get => customsDocumentCode;
				set => customsDocumentCode = value;
			}
			DocumentVisualizer.DocDataObjects.ICodeDescription customsDocumentCode;

			#endregion

			#region CustomsOfficeCode

			public ZString CustomsOfficeCode
			{
				get => customsOfficeCode;
				set => customsOfficeCode = value;
			}
			ZString customsOfficeCode;

			#endregion
		}

		#endregion
	}
}
