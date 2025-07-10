using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.Freight.Forwarding.Business.ResString;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobOrderLineValidation : AutoJobOrderLineValidation
	{
		public JobOrderLineValidation(AutoJobOrderLine parent)
			: base(parent)
		{
		}

		new OrderLine Parent
		{
			get { return (OrderLine)base.Parent; }
		}

		protected QtyReceivedEqualsQtyOrderedValidation QtyReceivedEqualsQtyOrderedValidation
		{
			get { return fQtyReceivedEqualsQtyOrderedValidation ?? (fQtyReceivedEqualsQtyOrderedValidation = new QtyReceivedEqualsQtyOrderedValidation(Parent)); }
		}

		QtyReceivedEqualsQtyOrderedValidation fQtyReceivedEqualsQtyOrderedValidation;

		protected override void CheckJO_ContainerNumber()
		{
			base.CheckJO_ContainerNumber();

			ListValidation.ErrorIfInvalidCode(Parent.JO_ContainerNumberInfo, Parent.ContainerNumbersList);

			if (!Parent.JO_ContainerNumber.IsEmpty)
			{
				ContainerNumberValidation.WarnIfInvalid(Parent.JO_ContainerNumberInfo);

				if (HasItems(Parent.ContainersOnAllDeliveries))
				{
					Parent.JO_ContainerNumberInfo.AddError(Res.GetString("dbb4fef8-5e5e-45ef-a8eb-ae08b3571f35", "You cannot specify a container here as this order line has container allocations in the order line containers area. Please double-click on the order-line and specify the order lines against the relevant deliveries."));
				}
			}

			if (Parent.Order != null && Parent.Order.Shipment != null && Parent.Order.Shipment.Consols.Count > 0 && !ContainerExistsOnConsol())
			{
				Parent.JO_ContainerNumberInfo.AddWarning(Res.GetString("ef001259-1068-4c4b-9649-fdf66614acee", "The specified container does not exist on the attached shipment's consols."));
			}
		}

		protected override void CheckJO_UnderQuantityPercentageLimit()
		{
			base.CheckJO_UnderQuantityPercentageLimit();

			if (!Parent.JO_UnderQuantityPercentageLimitInfo.HasErrors())
			{
				var underQuantityPercentageLimit = Parent.JO_UnderQuantityPercentageLimit;
				if (underQuantityPercentageLimit < 0 || underQuantityPercentageLimit > 100)
				{
					Parent.JO_UnderQuantityPercentageLimitInfo.AddError(Res.GetString("25561924-2253-4074-8E2A-06AD3780A4C9", "Please enter a valid value for Quantity Allowable Under. Value should be from 0-100."));
				}
			}
		}

		protected override void CheckJO_OverQuantityPercentageLimit()
		{
			base.CheckJO_OverQuantityPercentageLimit();

			if (!Parent.JO_OverQuantityPercentageLimitInfo.HasErrors())
			{
				var overQuantityPercentageLimit = Parent.JO_OverQuantityPercentageLimit;
				if (overQuantityPercentageLimit < 0)
				{
					Parent.JO_OverQuantityPercentageLimitInfo.AddError(Res.GetString("8577BDCF-5B4B-466C-9942-8A1309BCB6A1", "Please enter a valid value for Quantity Allowable Over. Value should be greater than 0."));
				}
			}
		}

		bool ContainerExistsOnConsol()
		{
			if (Parent.Order != null && Parent.Order.Shipment != null)
			{
				foreach (CommonConsol consol in Parent.Order.Shipment.Consols)
				{
					foreach (CommonContainer container in consol.Containers)
					{
						if (container.JC_ContainerNum == Parent.JO_ContainerNumber)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		bool HasItems(IEnumerable enumerable)
		{
			foreach (object item in enumerable)
			{
				return true;
			}
			return false;
		}

		protected override void CheckJO_QtyInvoiced()
		{
			base.CheckJO_QtyInvoiced();

			if (Parent.JO_QtyInvoiced != Parent.JO_Calc_TotalQtyInvoiced)
			{
				Parent.JO_QtyInvoicedInfo.AddWarning(Res.GetString("0e49ac7a-227b-48ba-be54-38513e43c4e7", "Quantity Invoiced does not equal the total Quantity Invoiced specified on each container."));
			}

			ValidateJO_QtyReceived();
		}

		protected override void CheckJO_Quantity()
		{
			base.CheckJO_Quantity();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JO_QuantityInfo, 0);
			ValidateJO_QtyReceived();
		}

		protected override void CheckJO_QtyReceived()
		{
			base.CheckJO_QtyReceived();

			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JO_QtyReceivedInfo, 0);
			QtyReceivedEqualsQtyOrderedValidation.CheckQtyReceivedEqualsQtyOrdered();

			if (Parent.Deliveries.Count > 0 && Parent.JO_QtyReceived != Parent.JO_Calc_TotalQtyReceived)
			{
				Parent.JO_QtyReceivedInfo.AddWarning(Res.GetString("a3903e47-a97f-4ba6-9381-355c606c8df4", "Quantity Received doesn't add up to Quantity Allocated on all delivery lines (should be {0})",
					Parent.JO_Calc_TotalQtyReceived));
			}
		}

		protected override void CheckJO_InnerPacks()
		{
			base.CheckJO_InnerPacks();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JO_InnerPacksInfo, 0);
		}

		protected override void CheckJO_OuterPacks()
		{
			base.CheckJO_OuterPacks();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JO_OuterPacksInfo, 0);
		}

		protected override void CheckJO_ItemPrice()
		{
			base.CheckJO_ItemPrice();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JO_ItemPriceInfo, 0);
		}

		protected override void CheckJO_ActualWeight()
		{
			base.CheckJO_ActualWeight();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JO_ActualWeightInfo, 0);
		}

		protected override void CheckJO_ActualVolume()
		{
			base.CheckJO_ActualVolume();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JO_ActualVolumeInfo, 0);
		}

		protected override void CheckJO_ContainerPackingOrder()
		{
			base.CheckJO_ContainerPackingOrder();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JO_ContainerPackingOrderInfo, 0);
		}

		protected override void CheckJO_InnerPacksUQ()
		{
			base.CheckJO_InnerPacksUQ();
			ListValidation.ErrorIfInvalidCode(Parent.JO_InnerPacksUQInfo, Parent.Lookups.PackTypes);
		}

		protected override void CheckJO_OuterPacksUQ()
		{
			base.CheckJO_OuterPacksUQ();
			ListValidation.ErrorIfInvalidCode(Parent.JO_OuterPacksUQInfo, Parent.Lookups.PackTypes);
		}

		protected override void CheckJO_F3_NKPackType()
		{
			base.CheckJO_F3_NKPackType();
			ListValidation.ErrorIfInvalidCode(Parent.JO_F3_NKPackTypeInfo, Parent.JO_F3_NKPackType_List);
		}

		protected override void CheckJO_RN_NKCountryOfOrigin()
		{
			base.CheckJO_RN_NKCountryOfOrigin();
			ListValidation.ErrorIfInvalidCode(Parent.JO_RN_NKCountryOfOriginInfo, Parent.Lookups.CountryOfOrigins);
		}

		protected override void CheckJO_UnitOfWeight()
		{
			base.CheckJO_UnitOfWeight();
			ListValidation.ErrorIfInvalidCode(Parent.JO_UnitOfWeightInfo, Parent.WeightUnit_List);
		}

		protected override void CheckJO_UnitOfVolume()
		{
			base.CheckJO_UnitOfVolume();
			ListValidation.ErrorIfInvalidCode(Parent.JO_UnitOfVolumeInfo, Parent.VolumeUnit_List);
		}

		protected override void CheckJO_LineNo()
		{
			base.CheckJO_LineNo();

			if (Parent.JO_LineNo <= 0)
			{
				Parent.JO_LineNoInfo.AddError(Res.GetString("e0000b49-07aa-4e11-9451-6a63f817563e", "Line Number must be greater than or equal to 1."));
			}

			if (IsDuplicateLine)
			{
				Parent.JO_LineNoInfo.AddError(DuplicateOrderLineMessage);
			}
		}

		protected override void CheckJO_SubLineNo()
		{
			base.CheckJO_SubLineNo();

			if (IsDuplicateLine)
			{
				Parent.JO_SubLineNoInfo.AddError(DuplicateOrderLineMessage);
			}
		}

		protected override void CheckJO_ShipmentWindowEnd()
		{
			base.CheckJO_ShipmentWindowEnd();

			if (Parent.JO_ShipmentWindowEnd < Parent.JO_ShipmentWindowStart)
			{
				Parent.JO_ShipmentWindowEndInfo.AddError(Res.GetString("56b1995b-468d-7732-f960-2d69ca9f0573", "Ship window start date must be earlier than or equal to ship window end date."));
			}
		}

		protected override void CheckJO_ShipmentWindowStart()
		{
			base.CheckJO_ShipmentWindowStart();

			if (Parent.JO_ShipmentWindowEnd < Parent.JO_ShipmentWindowStart)
			{
				Parent.JO_ShipmentWindowStartInfo.AddError(Res.GetString("b3a9f969-cee3-9e1e-e2e0-83cc11b08102", "Ship window start date must be earlier than or equal to ship window end date."));
			}
		}

		static readonly Regex regexValidationPatternWhenEnableOrderLineReferenceMatching = new Regex(@"^[0-9A-Z@(){}_#\-\!\%\^&\*\""'<>=;:\+\.\/\$\?\|\\[\]~]{1,35}$");

		static readonly Regex regexValidationPatternWhenDisableOrderLineReferenceMatching = new Regex(@"^[0-9A-Z@(){}_#\-\!\%\^&\*\""'<>=;:\+\.\/\$\?\|\\[\]]{1,35}$");

		protected override void CheckJO_LineReference()
		{
			base.CheckJO_LineReference();

			if (IsDuplicateLineReference)
			{
				Parent.JO_LineReferenceInfo.AddError(DuplicateLineReferenceMessage);
			}

			if (!string.IsNullOrEmpty(Parent.JO_LineReference))
			{
				if (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.Value)
				{
					if (!regexValidationPatternWhenEnableOrderLineReferenceMatching.IsMatch(Parent.JO_LineReference))
					{
						Parent.JO_LineReferenceInfo.AddError(Res.GetString("1923b28d-2cf4-4da3-a126-d87acfbbb9a8", "Must be upper-case and alphanumeric: Space( ) Comma(,) and Back-tick(`) are not allowed."));
					}
				}
				else
				{
					if (!regexValidationPatternWhenDisableOrderLineReferenceMatching.IsMatch(Parent.JO_LineReference))
					{
						Parent.JO_LineReferenceInfo.AddError(Res.GetString("7f603fe3-d598-47ce-badc-9714cb816985", "Must be upper-case and alphanumeric: Space( ) Comma(,) Back-tick(`) and Tilde(~) are not allowed."));
					}
				}
			}
		}

		bool IsDuplicateLine => DuplicateLineExists(
			new ZQuery(JobOrderLineSchema.JO_JD, SQLComparisonOperator.Equal, Parent.JO_JD)
				.AddToFilter(JobOrderLineSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK)
				.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_LineNo, SQLComparisonOperator.Equal, Parent.JO_LineNo)
				.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_SubLineNo, SQLComparisonOperator.Equal, Parent.JO_SubLineNo)
		);

		bool IsDuplicateLineReference => Parent.JO_LineReference != string.Empty
			&& DuplicateLineExists(
				new ZQuery(JobOrderLineSchema.JO_JD, SQLComparisonOperator.Equal, Parent.JO_JD)
					.AddToFilter(JobOrderLineSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK)
					.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_LineReference, SQLComparisonOperator.Equal, Parent.JO_LineReference)
			);

		bool DuplicateLineExists(ZQuery duplicateLineQuery)
		{
			if (Parent.Order == null || !Parent.Order.IsInDatabase)
			{
				duplicateLineQuery.FetchOnlyFromLocalCache = true;
			}

			return Parent.Factory.LoadTop1<OrderLine>(duplicateLineQuery) != null;
		}

		static string DuplicateOrderLineMessage => Res.GetString("c6ed8ed8-7dde-48cf-bec7-102cdd8817ab", "A Line with this Order No / Sub-line No already exists on this Order");

		static string DuplicateLineReferenceMessage => Res.GetString("a4ea9936-4efe-41b7-a86a-472c914f2bd7", "A Line with this Line Reference already exists on this Order");

		protected override void CheckJO_Partno()
		{
			base.CheckJO_Partno();
			ListValidation.WarnIfInvalidCode(Parent.JO_PartnoInfo, Parent.JO_Partno_List, GetPartnoValidationMessage());
		}

		protected virtual MultilingualString GetPartnoValidationMessage()
		{
			return ResString.GetMultilingualString("1f6d0917-1e68-4d2d-add2-38f740d8483f", "You have not entered an existing product number that is related to the supplier or buyer.");
		}

		protected override void CheckJO_INCO()
		{
			base.CheckJO_INCO();
			ListValidation.ErrorIfInvalidCode(Parent.JO_INCOInfo, Parent.JO_INCO_List);
			IncotermValidation.Instance.WarningIfExpired(Parent.JO_INCOInfo);
		}

		#region Custom Label Mandatory Validation

		protected CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}
		CustomLabelPropertyValidation customLabelPropertyValidation;

		protected override void CheckJO_CustomAttrib1()
		{
			base.CheckJO_CustomAttrib1();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomAttrib1Info);
		}

		protected override void CheckJO_CustomAttrib2()
		{
			base.CheckJO_CustomAttrib2();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomAttrib2Info);
		}

		protected override void CheckJO_CustomAttrib3()
		{
			base.CheckJO_CustomAttrib3();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomAttrib3Info);
		}

		protected override void CheckJO_CustomAttrib4()
		{
			base.CheckJO_CustomAttrib4();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomAttrib4Info);
		}

		protected override void CheckJO_CustomAttrib5()
		{
			base.CheckJO_CustomAttrib5();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomAttrib5Info);
		}

		protected override void CheckJO_CustomAttrib6()
		{
			base.CheckJO_CustomAttrib6();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomAttrib6Info);
		}

		protected override void CheckJO_CustomTextBlob1()
		{
			base.CheckJO_CustomTextBlob1();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomTextBlob1Info);
		}

		protected override void CheckJO_CustomDecimal1()
		{
			base.CheckJO_CustomDecimal1();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomDecimal1Info);
		}

		protected override void CheckJO_CustomDecimal2()
		{
			base.CheckJO_CustomDecimal2();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomDecimal2Info);
		}

		protected override void CheckJO_CustomDecimal3()
		{
			base.CheckJO_CustomDecimal3();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomDecimal3Info);
		}

		protected override void CheckJO_CustomDecimal4()
		{
			base.CheckJO_CustomDecimal4();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomDecimal4Info);
		}

		protected override void CheckJO_CustomDecimal5()
		{
			base.CheckJO_CustomDecimal5();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomDecimal5Info);
		}

		protected override void CheckJO_CustomDate1()
		{
			base.CheckJO_CustomDate1();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomDate1Info);
		}

		protected override void CheckJO_CustomDate2()
		{
			base.CheckJO_CustomDate2();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomDate2Info);
		}

		protected override void CheckJO_CustomDate3()
		{
			base.CheckJO_CustomDate3();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomDate3Info);
		}

		protected override void CheckJO_CustomDate4()
		{
			base.CheckJO_CustomDate4();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomDate4Info);
		}

		protected override void CheckJO_CustomDate5()
		{
			base.CheckJO_CustomDate5();
			CustomLabelPropertyValidation.Validate(new OrderLine.CustomLabelsProvider(Parent.Order), Parent.JO_CustomDate5Info);
		}

		protected override void CheckJO_PartAttrib1()
		{
			base.CheckJO_PartAttrib1();
			if (Parent.Order != null)
			{
				OrgSupplierPart part = new OrgSupplierPart.Loader(Parent.Factory).Load(Parent.JO_Partno, Parent.Order.Buyer, Parent.Order.Supplier);
				PartAttributeValidation.CheckAttribute(Parent.Order.Buyer, part, Parent.JO_PartAttrib1Info, 1);
			}
		}

		protected override void CheckJO_PartAttrib2()
		{
			base.CheckJO_PartAttrib2();
			if (Parent.Order != null)
			{
				OrgSupplierPart part = new OrgSupplierPart.Loader(Parent.Factory).Load(Parent.JO_Partno, Parent.Order.Buyer, Parent.Order.Supplier);
				PartAttributeValidation.CheckAttribute(Parent.Order.Buyer, part, Parent.JO_PartAttrib2Info, 2);
			}
		}

		protected override void CheckJO_PartAttrib3()
		{
			base.CheckJO_PartAttrib3();
			if (Parent.Order != null)
			{
				OrgSupplierPart part = new OrgSupplierPart.Loader(Parent.Factory).Load(Parent.JO_Partno, Parent.Order.Buyer, Parent.Order.Supplier);
				PartAttributeValidation.CheckAttribute(Parent.Order.Buyer, part, Parent.JO_PartAttrib3Info, 3);
			}
		}

		PartAttributeValidation PartAttributeValidation
		{
			get
			{
				if (fPartAttributeValidation == null)
				{
					fPartAttributeValidation = new PartAttributeValidation();
				}
				return fPartAttributeValidation;
			}
		}
		PartAttributeValidation fPartAttributeValidation;

		#endregion
	}
}
