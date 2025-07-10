using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	sealed class ElectronicHouseBillDataObjectWriter : DataObjectWriter<HouseBill, UniversalShipment>
	{
		public ElectronicHouseBillDataObjectWriter(IDataWritingManager writeManager, IDocument document = null)
			: base(writeManager)
		{
			this.document = document;
		}

		readonly IDocument document;

		protected override UniversalShipment PopulateDataObject(HouseBill houseBill)
		{
			var uxmlShipment = new UniversalShipment(writeManager.WriterStrategy);

			uxmlShipment.DataContext = houseBill
				.CreateUXmlDataContext()
				.AddDataProvider()
				.AddUserBranchAndDepartment();

			uxmlShipment.ElectronicBillOfLadingVersion = houseBill.ElectronicBillOfLadingVersion;
			uxmlShipment.WayBillNumber = houseBill.HouseBillNumber;
			uxmlShipment.BillTerms = houseBill.BillTerms?.ToUXmlCodeDescriptionPair();
			uxmlShipment.BillType = houseBill.BillType?.ToUXmlCodeDescriptionPair();

			PopulateAddresses(houseBill, uxmlShipment);
			PopulateAttachedDocuments(houseBill, uxmlShipment);
			PopulateAddInfos(houseBill, uxmlShipment);

			return uxmlShipment;
		}

		#region Addresses

		void PopulateAddresses(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			var billOfLadingBillType = houseBill.BillType?.Code;

			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!houseBill.ElectronicBillOfLadingShipper.IsEmpty())
				{
					var organizationAddress = houseBill.ElectronicBillOfLadingShipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy, GetRegistrationNumbers(houseBill.ElectronicBillOfLadingShipper));
					addresses.Add(organizationAddress);
				}

				if (billOfLadingBillType.HasValue && Core.Constants.BillOfLadingBillType.Codes.Straight == billOfLadingBillType.Value && !houseBill.ElectronicBillOfLadingConsignee.IsEmpty())
				{
					var organizationAddress = houseBill.ElectronicBillOfLadingConsignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy, GetRegistrationNumbers(houseBill.ElectronicBillOfLadingConsignee));
					addresses.Add(organizationAddress);
				}

				if (!houseBill.CurrentUser.IsEmpty())
				{
					var organizationAddress = houseBill.CurrentUser.ToUXmlOrganizationAddress(nameof(houseBill.CurrentUser), writeManager.WriterStrategy, GetRegistrationNumbers(houseBill.CurrentUser));
					addresses.Add(organizationAddress);
				}

				if (!houseBill.Holder.IsEmpty())
				{
					var organizationAddress = houseBill.Holder.ToUXmlOrganizationAddress(nameof(DocAddressType.Holder), writeManager.WriterStrategy, GetRegistrationNumbers(houseBill.Holder));
					addresses.Add(organizationAddress);
				}

				if (!houseBill.SurrenderParty.IsEmpty())
				{
					addresses.Add(houseBill.SurrenderParty.ToUXmlOrganizationAddress(nameof(DocAddressType.SurrenderParty), writeManager.WriterStrategy, GetRegistrationNumbers(houseBill.SurrenderParty)));
				}

				if (billOfLadingBillType.HasValue && Core.Constants.BillOfLadingBillType.Codes.ToOrder == billOfLadingBillType.Value && !houseBill.ElectronicBillOfLadingToOrder.IsEmpty())
				{
					addresses.Add(houseBill.ElectronicBillOfLadingToOrder.ToUXmlOrganizationAddress(nameof(DocAddressType.ToOrder), writeManager.WriterStrategy));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		IEnumerable<UniversalDataBuss.DataObjects.Universal.RegistrationNumber> GetRegistrationNumbers(IAddress address)
		{
			var regNumbers = address.RegistrationNumbers.Select(x => x.ToUXmlRegistrationNumber()).ToList();

			if (!address.HeaderIdentifier.IsEmpty)
			{
				regNumbers.Add(new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					Type = new RegistrationNumberType
					{
						Code = "OHP"
					},
					Value = address.HeaderIdentifier.ToString()
				});
			}

			if (!address.AddressIdentifier.IsEmpty)
			{
				regNumbers.Add(new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					Type = new RegistrationNumberType
					{
						Code = "OAP"
					},
					Value = address.AddressIdentifier.ToString()
				});
			}

			return regNumbers;
		}

		#endregion

		#region AttachedDocuments

		void PopulateAttachedDocuments(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			var fileAttributes = new DataObjectWriterHelper.FileAttributes()
			{
				Name = $"House Bill ({houseBill.HouseBillNumber})",
				Description = (NoResString)"Original Bill Of Lading",
				Code = "OBL",
				IsPublished = false
			};

			DataObjectWriterHelper.AppendPopulatePDFAttachedDocument(uxmlShipment, document, fileAttributes, applyDraftWatermark: true);
		}

		#endregion

		#region AddInfos

		void PopulateAddInfos(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				AddAddInfo(addInfos, "AmendmentRequestID", houseBill.AmendmentRequestID);

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
	}
}
