using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Customs.TW.DataTransfer.Constants;
using EZC = Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class TWDataObjectWriterHelper : UniversalDataObjectWriterHelper
	{
		public TWDataObjectWriterHelper(BusinessObjectFactory factory) : base(factory, Core.Constants.CountryCodes.Taiwan)
		{
		}

		public override ZString? GetReferencedEntityDescriptionForCusCodeData(CusCodeData cusCodeData)
		{
			return (cusCodeData as DeclarationDuplicate)?.Description;
		}

		protected override IAdditionalAddInfoGroupCollectionDataObjectWriter GetAdditionalAddInfoGroupCollectionSupportForCore(BusinessObject bizObj, IDataWritingManager writeManager)
		{
			IAdditionalAddInfoGroupCollectionDataObjectWriter addInfoGroupCollectionWriter = null;
			if (bizObj != null)
			{
				if (bizObj is JobDeclaration declaration)
				{
					addInfoGroupCollectionWriter = new TWJobDeclarationAdditionalAddInfoGroupCollectionDataObjectWriter(declaration);
				}
				else if (bizObj is JobComInvoiceLine invoiceLine)
				{
					addInfoGroupCollectionWriter = new TWInvoiceLineAdditionalAddInfoGroupCollectionDataObjectWriter(invoiceLine, this);
				}
			}
			return addInfoGroupCollectionWriter;
		}

		protected override IEnumerable<CustomsReference> GetAdditionalCustomsReferenceDataForCore(BusinessObject bizObj, IDataWritingManager writeManager, string dataContext)
		{
			var result = new List<CustomsReference>();
			var baseResult = base.GetAdditionalCustomsReferenceDataForCore(bizObj, writeManager, dataContext);
			if (baseResult != null)
			{
				result.AddRange(baseResult);
			}

			var declaration = bizObj as JobDeclaration;
			if (declaration != null)
			{
				PopulateJobDeclarationCustomsReferences(result, declaration);
			}

			var invoiceLine = bizObj as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				PopulateJobComInvoiceLineCustomsReferences(result, invoiceLine);
			}
			return result;
		}

		void PopulateJobDeclarationCustomsReferences(List<CustomsReference> result, JobDeclaration declaration)
		{
			foreach (var reservedField in declaration.ReservedFields.Cast<JobDeclarationReservedField>())
			{
				PopulateReservedField(result, reservedField);
			}
		}

		void PopulateJobComInvoiceLineCustomsReferences(List<CustomsReference> result, JobComInvoiceLine invoiceLine)
		{
			foreach (var reservedField in invoiceLine.ReservedFields.Cast<JobComInvoiceLineReservedField>())
			{
				PopulateReservedField(result, reservedField);
			}

			var assignedNumbers = invoiceLine.AssignedJobComInvLineRefsCollection.Cast<AssignedJobComInvLineRefs>().Select(@ref => @ref.JG_ReferenceNumber);
			foreach (var assignedNumber in assignedNumbers)
			{
				result.Add(new CustomsReference
				{
					Type = assignedNumberType,
					Reference = assignedNumber
				});
			}

			foreach (var foodData in invoiceLine.FoodDataCollection.Cast<FoodData>())
			{
				result.Add(new CustomsReference
				{
					Type = ingredientContentType,
					Reference = foodData.CY_Data,
					ReferencedEntityDescription = foodData.Content.ToString()
				});
			}

			foreach (var chassis in invoiceLine.ChassisJobComInvLineRefsCollection.Cast<ChassisJobComInvLineRefs>())
			{
				result.Add(new CustomsReference
				{
					Type = carChassisNumberType,
					Reference = chassis.JG_ReferenceNumber
				});
			}

			var storageAndShippingConditionJobComInvLineRefs = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.Cast<StorageAndShippingConditionJobComInvLineRefs>().Select(@ref => @ref.JG_ReferenceNumber);
			foreach (var storageAndShippingCondition in storageAndShippingConditionJobComInvLineRefs)
			{
				result.Add(new CustomsReference
				{
					Type = storageAndShippingConditionType,
					Reference = storageAndShippingCondition
				});
			}
		}

		void PopulateReservedField(List<CustomsReference> result, ReservedField reservedField)
		{
			result.Add(new CustomsReference
			{
				Type = customsDeclarationReservedField,
				Reference = reservedField.CY_Code,
				ReferencedEntityDescription = reservedField.CY_Data
			});
		}

		public ZInt? AllocateControllingMessageHeaderLink(ZGuid sourcePK)
		{
			ZInt? result = null;
			if (sourcePK.IsValid)
			{
				if (controllingMessageLinkMap.ContainsKey(sourcePK))
				{
					result = controllingMessageLinkMap[sourcePK];
				}
				else
				{
					result = controllingMessageLinkMap.Count + 1;
					controllingMessageLinkMap[sourcePK] = (int)result;
				}
			}
			return result;
		}
		readonly Dictionary<ZGuid, int> controllingMessageLinkMap = new Dictionary<ZGuid, int>();

		public ZString GetAllocatedControllingMessageHeaderLink(ZGuid sourcePK)
		{
			var result = ZString.Empty;
			if (sourcePK.IsValid && controllingMessageLinkMap.ContainsKey(sourcePK))
			{
				result = controllingMessageLinkMap[sourcePK].ToString(CultureInfo.InvariantCulture);
			}
			return result;
		}

		public override WayBillType GetWayBillType(ZString billType)
		{
			return billType == Business.BillTypeList.Codes.ContainerNote ? new WayBillType() { Code = Constants.WayBillTypeCode.ContainerNote, Description = Constants.WayBillTypeCode.ContainerNoteDescription } : base.GetWayBillType(billType);
		}

		readonly CodeDescriptionPair carChassisNumberType = new CodeDescriptionPair() { Code = AddInfoGroupTypeCodes.CarChassisNumberType, Description = (EZC.NoResString)"Car Chassis Number" };
		readonly CodeDescriptionPair customsDeclarationReservedField = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.ReservedField, Description = (EZC.NoResString)"Declaration Reserved Field" };
		readonly CodeDescriptionPair assignedNumberType = new CodeDescriptionPair() { Code = JobComInvLineRefsType.Codes.AssignedNumber, Description = "AssignedNumberType" };
		readonly CodeDescriptionPair ingredientContentType = new CodeDescriptionPair { Code = CusCodeDataTypeList.Codes.Food, Description = "IngredientContent" };
		readonly CodeDescriptionPair storageAndShippingConditionType = new CodeDescriptionPair() { Code = JobComInvLineRefsType.Codes.StorageAndShippingCondition, Description = JobComInvLineRefsType.Descriptions.StorageAndShippingCondition };
	}
}
