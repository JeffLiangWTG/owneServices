using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	static class CustomsDeliveryOrderDefaulter
	{
		public static void Defaulter(JobDeclaration declaration, DeliveryOrderHeader header)
		{
			header.US_PreviousITDate = declaration.US_ITDate;

			if (declaration.JE_PrimaryITNumber != JobDeclaration.Constants.Multiple)
			{
				header.US_PreviousITNo = declaration.JE_PrimaryITNumber;
			}

			JobDocAddress deliveryAddress = declaration.ImporterDeliveryAddress;
			if (deliveryAddress != null && !deliveryAddress.IsEmpty)
			{
				BusinessObjectCloneArgs args = new BusinessObjectCloneArgs(new string[] { JobDocAddress.Schema.E2_ParentTableCode, JobDocAddress.Schema.E2_ParentID, JobDocAddress.Schema.E2_AddressSequence, JobDocAddress.Schema.E2_AddressType });
				header.ForDeliveryToAddress.CopyPersistentValuesFrom(deliveryAddress, args);
			}

			OrgHeader deliveryCartageCompany = declaration.DeliveryOrPickupCartageCo;
			if (deliveryCartageCompany != null)
			{
				header.US_OH_InlandCarrier = deliveryCartageCompany.PK;
			}
			DefaultBillDetails(declaration, header);
			DefaultContainerDetails(declaration, header);
			DefaultOrderHazmat(declaration, header);
			if (!declaration.JE_OwnerRef.IsEmpty)
			{
				header.US_OrderReference = declaration.JE_OwnerRef;
			}
			else if (declaration.DocsAndCartage.OrderItems.Count > 0)
			{
				header.US_OrderReference = declaration.DocsAndCartage.OrderItems[0].JT_OrderReference;
			}

			header.US_UI_NKCarriersLocalAgent = declaration.US_UI_NKCarrierSCAC;
			ZStringBuilder deliveryInstructionBuilder = new ZStringBuilder();
			foreach (StmNote deliveryInstructionNote in declaration.NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, true))
			{
				deliveryInstructionBuilder.Append(deliveryInstructionNote.ST_NoteDataAsText);
			}
			header.US_DeliveryInstructions = deliveryInstructionBuilder.ToStringWithNewLineBetweenAppends();
		}

		public static void DefaultOrderLine(JobDeclaration declaration, DeliveryOrderHeader header)
		{
			DeliveryOrderLine orderLine = header.DeliveryOrderLines.AddNew();
			if (orderLine.US_GoodsDescription.IsEmpty)
			{
				orderLine.US_GoodsDescription = declaration.JE_GoodsDescriptionDetailed.Left(USDeliveryOrderLineAddInfo.Schema.US_GoodsDescriptionMaxLength);
			}
			if (orderLine.US_NoOfPackages.IsEmpty)
			{
				orderLine.US_NoOfPackages = declaration.JE_TotalNoOfPacks;
				orderLine.US_PackageType = declaration.JE_TotalNoOfPacksPackType;
			}
			if (orderLine.US_WeightInKilograms.IsEmpty && Core.Constants.Weight.ContainsCode(declaration.JE_TotalWeightUnit))
			{
				orderLine.US_WeightInKilograms = Core.Constants.Weight.Convert(declaration.JE_TotalWeight, declaration.JE_TotalWeightUnit, Core.Constants.Weight.Kilograms);
			}
		}

		static void DefaultOrderHazmat(JobDeclaration declaration, DeliveryOrderHeader header)
		{
			var dictionary = new Dictionary<ZString, KeyValuePair<ZString, ZString>>();
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				if (invoiceLine.UNDGs.Count > 0)
				{
					var undg = invoiceLine.UNDGs[0];
					var subs = undg.Substance;
					if (subs != null)
					{
						ZString key = subs.DG_Code + undg.DI_OC_DGContact.ToStringKey();
						if (!dictionary.ContainsKey(key))
						{
							var contact = undg.DGContact;
							var emergencyContact = contact == null ? ZString.Empty : contact.PhoneFallbackToOrganisation;
							dictionary.Add(key, new KeyValuePair<ZString, ZString>(subs.DG_Code, emergencyContact));
						}
					}
				}
			}
			foreach (var value in dictionary.Values)
			{
				var hazmat = header.DeliveryOrderHazmats.AddNew();
				hazmat.US_UNNumber = value.Key;
				hazmat.US_EmergencyContactNmber = value.Value;
			}
		}

		static void DefaultContainerDetails(JobDeclaration declaration, DeliveryOrderHeader header)
		{
			if (declaration.ContainersRequired)
			{
				header.DeliveryOrderContainers.CopyContainersIfMissing(declaration.CusContainers.ToArray<CusContainer>());
			}
		}

		static void DefaultBillDetails(JobDeclaration declaration, DeliveryOrderHeader header)
		{
			Bill masterBill = declaration.PrimaryMasterBill;
			if (masterBill != null)
			{
				AddOrderBill(header, masterBill, !declaration.IsAir);
			}

			Bill houseBill = declaration.PrimaryHouseBill;
			if (houseBill != null)
			{
				AddOrderBill(header, houseBill, true);
			}

			foreach (Bill bill in declaration.Bills)
			{
				if (bill != declaration.PrimaryHouseBill && bill != declaration.PrimaryMasterBill)
				{
					AddOrderBill(header, bill, !(declaration.IsAir && bill.IsMasterBill));
				}
			}
		}

		static void AddOrderBill(DeliveryOrderHeader header, Bill bill, bool addSCAC)
		{
			DeliveryOrderBill orderBill = header.DeliveryOrderBills.AddNew();
			ZString data = addSCAC ? (ZString)(bill.US_UI_NKBillIssuerSCAC + bill.CU_BillNum) : bill.CU_BillNum;

			orderBill.CY_Data = data;
			orderBill.CY_Code = bill.CU_BillType;
		}
	}
}
