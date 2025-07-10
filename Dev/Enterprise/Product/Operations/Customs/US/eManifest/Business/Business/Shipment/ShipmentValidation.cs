using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	public class ShipmentValidation : CusInBondBillValidation
	{
		public ShipmentValidation(Shipment parent)
			: base(parent)
		{
		}

		new Shipment Parent
		{
			get { return (Shipment)base.Parent; }
		}

		#region CheckB0_BoardedQuantity

		protected override void CheckB0_BoardedQuantity()
		{
			base.CheckB0_BoardedQuantity();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.B0_BoardedQuantityInfo, "Boarded Quantity");
			if (Parent.IsSplit)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_BoardedQuantityInfo);
			}

			if (!Parent.B0_ManifestQty.IsEmpty && !Parent.B0_BoardedQuantityInfo.HasNotifications())
			{
				if (Parent.B0_BoardedQuantity >= Parent.B0_ManifestQty)
				{
					const string message = "The boarded quantity should be less than the shipment quantity.\r\nLeave zero if same as the shipment quantity.";
					Parent.B0_BoardedQuantityInfo.AddMessageError(message);
				}
			}
		}

		#endregion

		#region CheckB0_DateOfExport

		protected override void CheckB0_DateOfExport()
		{
			base.CheckB0_DateOfExport();
			if (IsNotReferenceMandatoryOnly && Parent.B0_ShipmentType == ShipmentTypes.Codes.GoodsAstray && Parent.B0_DateOfExport.IsEmpty)
			{
				Parent.B0_DateOfExportInfo.AddMessageError("Export Date is required for Goods Astray.");
			}
		}

		bool IsNotReferenceMandatoryOnly
		{
			get { return !Parent.IsSplit && Parent.B0_ReleaseStatus != ShipmentEntryStatusList.Codes.LodgedWithOtherTrip; }
		}

		#endregion

		#region CheckB0_Firms

		protected override void CheckB0_Firms()
		{
			base.CheckB0_Firms();
			ListValidation.MessageErrorIfInvalidCode(Parent.B0_FirmsInfo);
		}

		#endregion

		#region CheckB0_ManifestQty

		protected override void CheckB0_ManifestQty()
		{
			base.CheckB0_ManifestQty();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.B0_ManifestQtyInfo, "Shipment Quantity");
			if (!Parent.B0_ManifestQtyInfo.HasNotifications() && (Parent.Commodities.Count > 0 || Parent.ValidateAllHasBeenRun))
			{
				var totalQuantity = (from commodity in Parent.Commodities select (int)commodity.BY_PieceCount).Sum();
				if (Parent.B0_ManifestQty != totalQuantity)
				{
					Parent.B0_ManifestQtyInfo.AddMessageError(string.Format(@"The shipment quantity should equal to the total packages of all commodities,
but the balance between the values makes up {0} {1}.", Parent.B0_ManifestQty - totalQuantity, Parent.B0_ManifestUQ));
				}
			}
			Parent.Validation.ValidateB0_BoardedQuantity();
		}

		#endregion

		#region CheckB0_ManifestUQ

		protected override void CheckB0_ManifestUQ()
		{
			base.CheckB0_ManifestUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.B0_ManifestUQInfo);
			MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.B0_ManifestUQInfo, Parent.B0_ManifestQtyInfo);
		}

		#endregion

		#region CheckB0_MasterBillNumber

		protected override void CheckB0_MasterBillNumber()
		{
			base.CheckB0_MasterBillNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_MasterBillNumberInfo);
			var trip = Parent.Trip;
			if (trip != null)
			{
				if (Parent.B0_ReleaseStatus == EntryStatusList.Codes.Cancelled && trip.BH_ReleaseStatus != EntryStatusList.Codes.Cancelled)
				{
					Parent.B0_MasterBillNumberInfo.AddWarning("This shipment has been deleted from Customs file and can be deleted from the system.");
				}

				var billNumber = Parent.B0_MasterBillNumber;
				if (!billNumber.IsEmpty)
				{
					var eta = trip.BH_ETA;
					if (eta.IsValid)
					{
						var tripIsFromHVLVShipment = Parent.Factory.GetCachedValue(string.Format("TripIsFromHVLVShipment|{0}", trip.PK), () =>
						{
							return trip.Logs.Find(log => !log.SL_IsCancelled
													&& log.SL_SE_NKEvent == Events.TransferredCode
													&& log.Parameters.TryGetValue(EventReferenceParameters.Codes.Type, out var type)
													&& type == Constants.ShipmentTypes.HighVolumeLowValue).Any();
						});

						if (!tripIsFromHVLVShipment)
						{
							var query = new ZDBOnlyQuery(typeof(Trip));
							query.AddToFilter(CusInBondHeaderSchema.PK, SQLComparisonOperator.NotEqual, trip.PK);
							query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, Common.CusInBondApplicationCodeList.Codes.eManifest);
							query.AddToFilter(CusInBondHeaderSchema.BH_ImportTransportMode, TransportModes.Codes.Road);
							query.AddToFilter(CusInBondHeaderSchema.BH_ETA, SQLComparisonOperator.LessThanOrEqualTo, eta);
							query.AddToFilter(CusInBondHeaderSchema.BH_ETA, SQLComparisonOperator.GreaterThanOrEqualTo, eta.AddYears(-3));

							var subQuery = new ZDBOnlySubQuery(typeof(Shipment), CusInBondBillSchema.B0_BH, CusInBondHeaderSchema.PK);
							subQuery.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, billNumber);
							query.AddSubQuery(subQuery, JoinCondition.And);
							var trips = Parent.Factory.Load<Trip>(query);
							if (trips.Length > 0)
							{
								Parent.B0_MasterBillNumberInfo.AddMessageError(string.Format("House bill number found on {0}.", string.Join(", ", trips.Select(x => x.BH_JobReference).OrderBy(x => x))));
							}
						}
					}

					if (trip.ShipmentMasterBillExists(Parent, billNumber))
					{
						Parent.B0_MasterBillNumberInfo.AddMessageError("House Bill number must be unique.");
					}
				}
			}
		}

		#endregion

		#region CheckB0_PortOfLadingKCode

		protected override void CheckB0_PortOfLadingKCode()
		{
			base.CheckB0_PortOfLadingKCode();
			if (IsNotReferenceMandatoryOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B0_PortOfLadingKCodeInfo, Parent.Lookups.ScheduleKPortCodes);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_PortOfLadingKCodeInfo, Parent.Lookups.ScheduleKPortCodes);
			}
		}

		#endregion

		#region CheckB0_RL_NKPortOfLading

		protected override void CheckB0_RL_NKPortOfLading()
		{
			base.CheckB0_RL_NKPortOfLading();
			ListValidation.MessageErrorIfInvalidCode(Parent.B0_RL_NKPortOfLadingInfo);
		}

		#endregion

		#region CheckB0_ServiceType

		protected override void CheckB0_ServiceType()
		{
			base.CheckB0_ServiceType();
			ListValidation.MessageErrorIfInvalidCode(Parent.B0_ServiceTypeInfo);
		}

		#endregion

		#region CheckB0_ShipmentType

		protected override void CheckB0_ShipmentType()
		{
			base.CheckB0_ShipmentType();
			if (Parent.B0_ReleaseStatus == ShipmentEntryStatusList.Codes.LodgedWithOtherTrip)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_ShipmentTypeInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B0_ShipmentTypeInfo);

				if (Parent.ValidateAllHasBeenRun)
				{
					ValidateAtLeastOneCommodityEntered(Parent.B0_ShipmentTypeInfo);
				}
			}
		}

		void ValidateAtLeastOneCommodityEntered(ZPropertyInfo notificationInfo)
		{
			if (IsNotReferenceMandatoryOnly && !(Parent.Commodities.Count > 0))
			{
				notificationInfo.AddMessageError("At least one commodity must be entered.");
			}
		}

		#endregion

		#region CheckB0_Volume

		protected override void CheckB0_Volume()
		{
			base.CheckB0_Volume();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.B0_VolumeInfo, Parent.B0_VolumeInfo.HumanReadableName);
		}

		#endregion

		#region CheckB0_VolumeUQ

		protected override void CheckB0_VolumeUQ()
		{
			base.CheckB0_VolumeUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.B0_VolumeUQInfo);
			MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.B0_VolumeUQInfo, Parent.B0_VolumeInfo);
		}

		#endregion

		#region CheckB0_Weight

		protected override void CheckB0_Weight()
		{
			base.CheckB0_Weight();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.B0_WeightInfo, Parent.B0_WeightInfo.HumanReadableName);
			var weightValue = Parent.B0_Weight;
			var weightUQ = Parent.B0_WeightUQ;
			var weight = new ZWeight(weightValue, weightUQ);
			if (weightValue >= 0 && weight.IsValid && (Parent.Commodities.Count > 0 || Parent.ValidateAllHasBeenRun))
			{
				var totalCommoditiesWeight = Parent.Commodities.Sum(x => Constants.Weight.ConvertSafe(x.BY_GrossWeight, x.BY_GrossWeightUnit, weightUQ));
				if (weightValue != totalCommoditiesWeight)
				{
					Parent.B0_WeightInfo.AddMessageError(Res.GetString("A9563D62-D397-4B7F-8D6C-267DE5B56831", "The gross weight should equal to the total weight of all commodities, but the balance between the values makes up {0} {1}.", weightValue - totalCommoditiesWeight, weightUQ));
				}
			}
		}

		#endregion

		#region CheckB0_WeightUQ

		protected override void CheckB0_WeightUQ()
		{
			base.CheckB0_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.B0_WeightUQInfo);
			MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.B0_WeightUQInfo, Parent.B0_WeightInfo);
		}

		#endregion

		#region CheckB0_WasOutOfUSFor45DaysOrLess

		protected override void CheckB0_WasOutOfUSFor45DaysOrLess()
		{
			base.CheckB0_WasOutOfUSFor45DaysOrLess();
			if (IsNotReferenceMandatoryOnly && Parent.B0_ShipmentType == ShipmentTypes.Codes.GoodsAstray && !Parent.B0_WasOutOfUSFor45DaysOrLess)
			{
				Parent.B0_WasOutOfUSFor45DaysOrLessInfo.AddMessageError(
					"For Goods Astray, you have to declare that the goods have not left either your or the foreign countries'" +
					" customs service control while in the foreign country and returned within 45 days or less since the date of exportation.");
			}
		}

		#endregion

		#region CheckB0_IssuerSCAC

		protected override void CheckB0_IssuerSCAC()
		{
			base.CheckB0_IssuerSCAC();

			if (!Parent.B0_IsLodged)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_IssuerSCACInfo, Parent.Lookups.SCACCarrierCodes);

				if (Parent.B0_IssuerSCAC.IsEmpty)
				{
					Parent.B0_IssuerSCACInfo.AddMessageError(Res.GetString("C3312272-9D04-46A4-A60F-CFAB4403EA41", "The issuer's SCAC is required to make the message to send to customs office valid, otherwise the message will be rejected as an error with missing SCAC."));
				}
			}
		}

		#endregion

		#region CheckB0_GoodsValue

		protected override void CheckB0_GoodsValue()
		{
			base.CheckB0_GoodsValue();

			if (Parent.TotalCommoditiesValue != Parent.B0_GoodsValue)
			{
				Parent.B0_GoodsValueInfo.AddMessageError(
					Res.GetString(
						"CD1CECD3-337B-4C23-9179-E1ED8435B13C",
						"The shipment value should equal to the total commodities value. but the balance between the values makes up {0} {1}.",
						Parent.B0_GoodsValue - Parent.TotalCommoditiesValue,
						Parent.B0_RX_NKGoodsValueCurrency));
			}
		}

		#endregion

		#region CheckB0_RN_NKCountryOfExport

		protected override void CheckB0_RN_NKCountryOfExport()
		{
			base.CheckB0_RN_NKCountryOfExport();
			ListValidation.MessageErrorIfInvalidCode(Parent.B0_RN_NKCountryOfExportInfo);
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			Parent.ValidateAllHasBeenRun = true;
			base.ValidateAll();
		}

		#endregion
	}
}
