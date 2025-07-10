using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class CommonConfirmDivot : AutoJobTransportLegPackLineDivot, IGoods, ICanDelete, IDefaultNumberOfDecimalsSupporterWithSchemaColumn
	{
		public CommonConfirmDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			//_stackTrace = System.Environment.StackTrace;
		}
		//public readonly string _stackTrace;

		#region Schema

		public new abstract class Schema : AutoJobTransportLegPackLineDivot.Schema
		{
			public const string PackLineWeight = "PackLineWeight";
			public const string PackLineWeightUnit = "PackLineWeightUnit";
			public const string PackLineVolume = "PackLineVolume";
			public const string PackLineVolumeUnit = "PackLineVolumeUnit";
		}

		#endregion

		#region Business Object Overrides

		public override bool IsSavedByFactory
		{
			get
			{
				if (!IsInDatabase
					&& !IsDeleted
					&& (Confirm == null
					|| (!Confirm.IsInDatabase && Confirm.IsEmpty && Confirm.Divots.All(d => d.IsEmpty))))
				{
					return false;
				}

				return base.IsSavedByFactory;
			}
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || (Confirm != null && Confirm.ReadOnly); }
			set { base.ReadOnly = value; }
		}

		public override void Delete()
		{
			try
			{
				if (!isDeleting)
				{
					isDeleting = true;

					if (Confirm != null && !Confirm.IsDeleted)
					{
						if (Confirm.Divots.Count == 1)
						{
							Confirm.Delete();
						}
						else if (Confirm.Divots.Count > 1)
						{
							Confirm.DivotRemovalLogs.Add(new CommonPickupDeliveryConfirm.DivotRemovalLog(J8_JL, System.Environment.StackTrace));
						}
					}

					base.Delete();
				}
			}
			finally
			{
				isDeleting = false;
			}
		}
		bool isDeleting;

		#endregion

		#region Related Business Objects

		#region PackLine

		[BusinessObjectTestExclude()]
		[RelatedBusinessObject("PackLine")]
		public override ZGuid J8_JL
		{
			get { return base.J8_JL; }
			set
			{
				var oldValue = J8_JL;
				base.J8_JL = value;

				var shipment = PackLine?.Shipment;
				if (shipment != null && shipment.JS_IsBooking && !shipment.JS_IsCFSRegistered && !shipment.JS_IsForwardRegistered)
				{
					ErrorReporter.ReportOnce(
					"{E79655E3-1DCA-42c6-BFEF-F227420A9A9E}",
					"Attempting to attach a Confirmation to a Booking (PK: '" + shipment.ToString() + "')"
					);
				}

				if (oldValue.IsValid && oldValue != value)
				{
					var notification = FormattableString.Invariant($"CommonConfirmDivot: '{PK}' is being relinked from packline: '{oldValue}' to '{value}'");
					if (Confirm == null)
					{
						ErrorReporter.ReportOnce(notification);
					}
					else
					{
						Confirm.DebugLog.AppendLine(notification);
						Confirm.DebugLog.AppendLine(System.Environment.StackTrace);
					}
				}
			}
		}

		public Type PackLineType { get; set; }

		public PackLine PackLine
		{
			get { return J8_JL.IsEmpty ? null : (PackLine)Factory.Load(PackLineType ?? typeof(PackLine), J8_JL); }
		}

		#endregion

		#region Confirm

		public CommonPickupDeliveryConfirm Confirm
		{
			get
			{
				var result = Factory.Load<CommonPickupDeliveryConfirm>(J8_EU_PickupDeliverConfirm);
				if (result != null && result.PackLineType == null)
				{
					result.PackLineType = PackLineType;
				}

				return result;
			}
		}

		public override ZGuid J8_EU_PickupDeliverConfirm
		{
			get => base.J8_EU_PickupDeliverConfirm;
			set
			{
				var oldValue = base.J8_EU_PickupDeliverConfirm;
				var notification = FormattableString.Invariant($"CommonConfirmDivot: '{PK}' is being relinked from confirm: '{oldValue}' to '{value}'");

				if (oldValue.IsValid && oldValue != value)
				{
					Confirm?.DebugLog.AppendLine(notification);
					Confirm?.DebugLog.AppendLine(System.Environment.StackTrace);
					CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship.debugLogMessage += notification;
					CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship.debugLogMessage += System.Environment.NewLine;
					CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship.debugLogMessage += System.Environment.StackTrace;
					CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship.debugLogMessage += System.Environment.NewLine;
				}

				base.J8_EU_PickupDeliverConfirm = value;
			}
		}

		#endregion

		#endregion

		#region Property Overrides

		#region J8_PackagesDelivered

		public override ZInt J8_PackagesDelivered
		{
			get { return base.J8_PackagesDelivered; }
			set
			{
				if (!IsDeleted)
				{
					if (J8_PackagesDelivered != value)
					{
						base.J8_PackagesDelivered = value;

						decimal percentage = 0;
						if (PackLinePackages > 0 && value > 0)
						{
							percentage = (new decimal(J8_PackagesDelivered) / new decimal(PackLinePackages));
						}

						J8_DeliveryWeight = percentage * PackLineWeight;
						J8_DeliveryVolume = percentage * PackLineVolume;

						if (PackLine != null)
						{
							PackLine.Shipment.RefreshDeliveryBinding();
						}
					}
				}
			}
		}

		#endregion

		#region J8_DeliveryVolume

		public override ZDecimal J8_DeliveryVolume
		{
			get { return base.J8_DeliveryVolume; }
			set { base.J8_DeliveryVolume = this.GetRoundedValue(JobTransportLegPackLineDivotSchema.J8_DeliveryVolume, J8_DeliveryVolumeInfo, value); }
		}

		#endregion

		#region J8_DeliveryWeight

		public override ZDecimal J8_DeliveryWeight
		{
			get { return base.J8_DeliveryWeight; }
			set { base.J8_DeliveryWeight = this.GetRoundedValue(JobTransportLegPackLineDivotSchema.J8_DeliveryWeight, J8_DeliveryWeightInfo, value); }
		}

		#endregion

		#endregion

		#region New Properties

		#region PackLineUNDGs

		public ZString PackLineUNDGs
		{
			get { return PackLine != null ? GetPackLineUNDGs() : ZString.Empty; }
		}

		ZString GetPackLineUNDGs()
		{
			ZString result = ZString.Empty;

			foreach (UNDGDataItem undg in PackLine.UNDGs)
			{
				if (undg.Substance != null)
				{
					result += undg.Substance.DG_Code + ";";
				}
			}

			return result.SubstringSafe(0, result.Length - 1);
		}

		#endregion

		#region PackLineUnitOfDimension

		public ZString PackLineUnitOfDimension
		{
			get { return PackLine != null ? PackLine.JL_UnitOfDimension : ZString.Empty; }
		}

		#endregion

		#region PackLinePackageType

		public ZString PackLinePackageType
		{
			get { return PackLine != null ? PackLine.JL_F3_NKPackType : ZString.Empty; }
		}

		#endregion

		#region PackLineWidth

		public ZDecimal PackLineWidth
		{
			get { return (PackLine != null) ? PackLine.JL_Width : 0; }
		}

		#endregion

		#region PackLineHeight

		public ZDecimal PackLineHeight
		{
			get { return (PackLine != null) ? PackLine.JL_Height : 0; }
		}

		#endregion

		#region PackLineLength

		public ZDecimal PackLineLength
		{
			get { return (PackLine != null) ? PackLine.JL_Length : 0; }
		}

		#endregion

		#region PackLinePackages

		public ZInt PackLinePackages
		{
			get { return PackLine != null ? PackLine.PackagesToDeliver : ZInt.Zero; }
		}

		#endregion

		#region PackLineWeight

		public ZDecimal PackLineWeight
		{
			get
			{
				var result = (PackLine != null) ? PackLine.JL_Calc_WeightToDeliver : 0;
				return this.GetRoundedValue(PackLineWeightInfo, result);
			}
		}

		public ZPropertyInfo PackLineWeightInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(CommonConfirmDivot.Schema.PackLineWeight); }
		}

		#endregion

		#region PackLineWeightUnit

		public ZString PackLineWeightUnit
		{
			get { return (PackLine != null) ? PackLine.JL_ActualWeightUQ : ZString.Empty; }
		}

		#endregion

		#region PackLineVolume

		public ZDecimal PackLineVolume
		{
			get
			{
				var result = (PackLine != null) ? PackLine.JL_Calc_VolumeToDeliver : 0;
				return this.GetRoundedValue(PackLineVolumeInfo, result);
			}
		}

		public ZPropertyInfo PackLineVolumeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(CommonConfirmDivot.Schema.PackLineVolume); }
		}

		#endregion

		#region PackLineVolumeUnit

		public ZString PackLineVolumeUnit
		{
			get { return (PackLine != null) ? PackLine.JL_ActualVolumeUQ : ZString.Empty; }
		}

		#endregion

		#region IsEmpty

		public bool IsEmpty
		{
			get
			{
				return J8_PackagesDelivered.IsEmpty
					&& J8_DeliveryWeight.IsEmpty
					&& J8_DeliveryVolume.IsEmpty;
			}
		}

		#endregion

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !(PackLine != null && PackLine.Shipment != null); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("7b4b42f7-4d99-4c07-a7f1-5ba6382e0627", "This Pack Line Confirmation cannot be removed. If this Pack Line is not part of this Confirmation, set Packs Delivered to 0 (Zero)."); }
		}

		#endregion

		#region GetNewValidation

		protected override JobTransportLegPackLineDivotValidation GetNewValidation()
		{
			return Confirm != null && Confirm.IsConfirmHiddenOnAllShipments ? base.GetNewValidation() : new CommonConfirmDivotValidation(this);
		}

		#endregion

		#region IGoods Members

		ZInt IGoods.BookedPackages
		{
			get { return PackLinePackages; }
		}

		ZDecimal IGoods.BookedWeight
		{
			get { return PackLineWeight; }
		}

		ZDecimal IGoods.BookedVolume
		{
			get { return PackLineVolume; }
		}

		ZInt IGoods.DeliveredPackages
		{
			get { return J8_PackagesDelivered; }
		}

		ZDecimal IGoods.DeliveredWeight
		{
			get { return J8_DeliveryWeight; }
		}

		ZDecimal IGoods.DeliveredVolume
		{
			get { return J8_DeliveryVolume; }
		}

		ZString IGoods.PackagesUnit
		{
			get { return PackLine != null ? PackLine.JL_F3_NKPackType : ZString.Empty; }
		}

		ZString IGoods.WeightUnit
		{
			get { return PackLine != null ? PackLine.PackLineWeightUnit : ZString.Empty; }
		}

		ZString IGoods.VolumeUnit
		{
			get { return PackLine != null ? PackLine.PackLineVolumeUnit : ZString.Empty; }
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				IDefaultNumberOfDecimalsSupporter parent = Confirm;
				var result = (parent != null) ? parent.TransportMode : ZString.Empty;

				return result;
			}
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.J8_DeliveryWeight:
					unitOfMeasure = Core.Constants.Weight.Kilograms;
					break;

				case Schema.J8_DeliveryVolume:
					unitOfMeasure = Core.Constants.Volume.CubicMetres;
					break;

				case Schema.PackLineWeight:
					unitOfMeasure = PackLineWeightUnit;
					break;

				case Schema.PackLineVolume:
					unitOfMeasure = PackLineVolumeUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZDecimal IDefaultNumberOfDecimalsSupporterWithSchemaColumn.GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			this.SetRoundedValue(JobTransportLegPackLineDivotSchema.J8_DeliveryWeight, J8_DeliveryWeightInfo);
			this.SetRoundedValue(JobTransportLegPackLineDivotSchema.J8_DeliveryVolume, J8_DeliveryVolumeInfo);
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		#endregion

		#region OnSaving

		// WI00748370 - Remove error logging to resolve performance issues in CS01594069
		//public override void OnSaving()
		//{
		//	base.OnSaving();

		//	var query = new ZQuery();
		//	query.AddToFilter(JobTransportLegPackLineDivotSchema.J8_JL, J8_JL);
		//	query.AddToFilter(JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, J8_EU_PickupDeliverConfirm);

		//	var containersWithDuplicatedNumbers = Factory.Load<CommonConfirmDivot>(query);
		//	var doWeHaveRepeatedRows = containersWithDuplicatedNumbers.Any(c => !c.PK.Equals(PK) && c.J8_JL.Equals(J8_JL) && c.J8_EU_PickupDeliverConfirm.Equals(J8_EU_PickupDeliverConfirm));
		//	if (doWeHaveRepeatedRows)
		//	{
		//		var repeatedRows = containersWithDuplicatedNumbers.Where(c => !c.PK.Equals(PK) && c.J8_JL.Equals(J8_JL) && c.J8_EU_PickupDeliverConfirm.Equals(J8_EU_PickupDeliverConfirm));
		//		var creationStacks = new ZStringBuilder();
		//		creationStacks
		//				.AppendLine($"The creation stack of main commonConfirmDivot(PK: {PK}), J8_JL:{J8_JL}, J8_EU_PickupDeliverConfirm:{J8_EU_PickupDeliverConfirm}, IsInDatabase:{IsInDatabase}, IsDeleted:{IsDeleted}, J8_SystemCreateTimeUtc:{J8_SystemCreateTimeUtc}, J8_SystemCreateUser:{J8_SystemCreateUser}")
		//				.AppendLine("\r\n")
		//				.AppendLine(_stackTrace)
		//				.AppendLine("\r\n");
		//		foreach (var commonConfirmDivot in repeatedRows)
		//		{
		//			creationStacks
		//				.AppendLine($"The creation stack of duplicate commonConfirmDivot(PK: {commonConfirmDivot.PK}), J8_JL:{commonConfirmDivot.J8_JL}, J8_EU_PickupDeliverConfirm:{commonConfirmDivot.J8_EU_PickupDeliverConfirm}, IsInDatabase:{commonConfirmDivot.IsInDatabase}, IsDeleted:{IsDeleted}, J8_SystemCreateTimeUtc:{commonConfirmDivot.J8_SystemCreateTimeUtc}, J8_SystemCreateUser:{commonConfirmDivot.J8_SystemCreateUser}")
		//				.AppendLine("\r\n")
		//				.AppendLine(commonConfirmDivot._stackTrace)
		//				.AppendLine("\r\n");
		//		}
		//		ErrorReporter.ReportOnce("The combination of J8_JL and J8_EU_PickupDeliverConfirm repeatedly save.", creationStacks.ToString());
		//	}
		//}

		#endregion
	}
}
