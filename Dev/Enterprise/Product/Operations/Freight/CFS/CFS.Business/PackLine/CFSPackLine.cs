using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSPackLine : PackLine, Integration.CFS.ICFSPackLine, ICartageLooseCargo
	{
		#region Schema

		public new abstract class Schema : PackLine.Schema
		{
			public const string JL_Calc_FullGatePassID = "JL_Calc_FullGatePassID";
			public const string JL_Calc_OutturnUndelivered = "JL_OutturnUndelivered";
		}

		#endregion

		public CFSPackLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Validation

		public new CFSPackLineValidation Validation
		{
			get { return new CFSPackLineValidation(this); }
		}

		protected override JobPackLinesValidation GetNewValidation()
		{
			return new CFSPackLineValidation(this);
		}

		#endregion

		#region Related Business Objects

		public new CFSContainerManyToManyCollection Containers
		{
			get { return (CFSContainerManyToManyCollection)base.Containers; }
		}

		public new CFSShipment Shipment
		{
			get { return (CFSShipment)base.Shipment; }
		}

		[ChildEditable(true)]
		public new CFSPackLocationCollection PackLocations
		{
			get { return (CFSPackLocationCollection)base.PackLocations; }
		}

		protected override CommonContainerManyToManyCollection GetNewContainersCollection()
		{
			return new CFSContainerManyToManyCollection(this);
		}

		protected override CommonShipment GetParentShipment()
		{
			return Factory.Load<CFSShipment>(JL_JS);
		}

		protected override CommonContainer LoadContainer(ZGuid containerPK)
		{
			return Factory.Load<CFSContainer>(containerPK);
		}

		protected override PackLocationCollection GetNewPackLocationCollection()
		{
			return new CFSPackLocationCollection(this, Factory);
		}

		#endregion

		public void SetPackageDetailsReadOnly(bool readOnly)
		{
			JL_ActualVolume_ReadOnly = readOnly;
			JL_ActualVolumeUQ_ReadOnly = readOnly;
			JL_ActualWeight_ReadOnly = readOnly;
			JL_ActualWeightUQ_ReadOnly = readOnly;
			JL_Description_ReadOnly = readOnly;
			JL_HarmonisedCode_ReadOnly = readOnly;
			JL_Height_ReadOnly = readOnly;
			JL_ItemNo_ReadOnly = readOnly;
			JL_Length_ReadOnly = readOnly;
			JL_LinePrice_ReadOnly = readOnly;
			JL_PackageCount_ReadOnly = readOnly;
			JL_F3_NKPackType_ReadOnly = readOnly;
			JL_RefNumber_ReadOnly = readOnly;
			JL_UnitOfDimension_ReadOnly = readOnly;
			JL_Width_ReadOnly = readOnly;

			JL_ActualVolumeInfo.RefreshBinding();
			JL_ActualVolumeUQInfo.RefreshBinding();
			JL_ActualWeightInfo.RefreshBinding();
			JL_ActualWeightUQInfo.RefreshBinding();
			JL_DescriptionInfo.RefreshBinding();
			JL_HarmonisedCodeInfo.RefreshBinding();
			JL_HeightInfo.RefreshBinding();
			JL_ItemNoInfo.RefreshBinding();
			JL_LengthInfo.RefreshBinding();
			JL_LinePriceInfo.RefreshBinding();
			JL_PackageCountInfo.RefreshBinding();
			JL_F3_NKPackTypeInfo.RefreshBinding();
			JL_RefNumberInfo.RefreshBinding();
			JL_UnitOfDimensionInfo.RefreshBinding();
			JL_WidthInfo.RefreshBinding();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.JL_FreightMode = "OUT";
		}

		public ZDateTime ArrivalAtCFS
		{
			get
			{
				ZDateTime arrivalDate = ZDateTime.Empty;

				foreach (var confirm in OriginCFSArrivalConfirms)
				{
					var divot = confirm.GetDivot(this);
					if (divot.J8_PackagesDelivered > 0 && !confirm.EU_PickupDeliveryTime.IsEmpty)
					{
						arrivalDate = confirm.EU_PickupDeliveryTime;
						break;
					}
				}

				return arrivalDate;
			}
		}

		#region New Properties

		public virtual bool IsFullyDelivered
		{
			get
			{
				bool result = false;
				if (UseOutturn)
				{
					result = JL_Outturn > 0 && JL_Outturn == PackagesConfirmed_DispatchedFromDestinationCFS;
				}
				else
				{
					result = JL_PackageCount > 0 && JL_PackageCount == PackagesConfirmed_DispatchedFromDestinationCFS;
				}

				return result;
			}
		}

		public ZString FirstImportContainerNum
		{
			get
			{
				ZString result = JL_Calc_ContainerNum;
				return result;
			}
		}

		public ZInt JL_OutturnUndelivered
		{
			get { return JL_Outturn - PackagesConfirmed_DispatchedFromDestinationCFS; }
		}

		public ZPropertyInfo JL_OutturnUndeliveredInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_OutturnUndelivered); }
		}

		protected override bool AreContainerLegsReadOnly
		{
			get { return false; }
		}

		public ZString CustomsStatusDescription
		{
			get
			{
				ZString result = ZString.Empty;
				IPackLineStatusProvider packLineStatusProvider = (IPackLineStatusProvider)Activator.CreateInstance(ObjectFactory.GetType<IPackLineStatusProvider>());
				IPackLineStatus packLineStatus = packLineStatusProvider.GetPackLineStatus(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				if (packLineStatus != null)
				{
					result = packLineStatus.GetCustomsStatusDescription(Shipment != null && Shipment.ArrivalConsol != null ? Shipment.ArrivalConsol.JK_MasterBillNum : ZString.Empty, JL_JS_HouseBill, FirstImportContainerNum);
				}
				return result;
			}
		}

		#endregion

		#region Property Overrides

		[ReadOnlyMember(nameof(IsNotOutturned))]
		public override ZDecimal JL_OutturnedHeight
		{
			get { return base.JL_OutturnedHeight; }
			set { base.JL_OutturnedHeight = value; }
		}

		[ReadOnlyMember(nameof(IsNotOutturned))]
		public override ZDecimal JL_OutturnedLength
		{
			get { return base.JL_OutturnedLength; }
			set { base.JL_OutturnedLength = value; }
		}

		[ReadOnlyMember(nameof(IsNotOutturned))]
		public override ZDecimal JL_OutturnedVolume
		{
			get { return base.JL_OutturnedVolume; }
			set { base.JL_OutturnedVolume = value; }
		}

		[ReadOnlyMember(nameof(IsNotOutturned))]
		public override ZDecimal JL_OutturnedWeight
		{
			get { return base.JL_OutturnedWeight; }
			set { base.JL_OutturnedWeight = value; }
		}

		[ReadOnlyMember(nameof(IsNotOutturned))]
		public override ZDecimal JL_OutturnedWidth
		{
			get { return base.JL_OutturnedWidth; }
			set { base.JL_OutturnedWidth = value; }
		}

		protected bool IsNotOutturned
		{
			get { return !IsOutturned; }
		}

		#endregion

		#region ReadOnly flags

		protected bool JL_ActualVolume_ReadOnly { get; set; }
		protected virtual bool JL_ActualVolumeUQ_ReadOnly { get; set; }
		protected bool JL_ActualWeight_ReadOnly { get; set; }
		protected virtual bool JL_ActualWeightUQ_ReadOnly { get; set; }
		protected bool JL_Description_ReadOnly { get; set; }
		protected bool JL_HarmonisedCode_ReadOnly { get; set; }
		protected bool JL_Height_ReadOnly { get; set; }
		protected bool JL_ItemNo_ReadOnly { get; set; }
		protected bool JL_Length_ReadOnly { get; set; }
		protected bool JL_LinePrice_ReadOnly { get; set; }
		protected bool JL_PackageCount_ReadOnly { get; set; }
		protected bool JL_F3_NKPackType_ReadOnly { get; set; }
		protected bool JL_RefNumber_ReadOnly { get; set; }
		protected virtual bool JL_UnitOfDimension_ReadOnly { get; set; }
		protected bool JL_Width_ReadOnly { get; set; }

		#endregion

		#region ICartageLooseCargo Members

		ZString ICartageLooseCargo.BookedPackType
		{
			get { return JL_F3_NKPackType; }
		}

		ZInt ICartageLooseCargo.BookedPackages
		{
			get { return JL_PackageCount; }
		}

		ZDecimal ICartageLooseCargo.BookedVolume
		{
			get { return JL_ActualVolume; }
		}

		ZString ICartageLooseCargo.BookedVolumeUnit
		{
			get { return JL_ActualVolumeUQ; }
		}

		ZDecimal ICartageLooseCargo.BookedWeight
		{
			get { return JL_ActualWeight; }
		}

		ZString ICartageLooseCargo.BookedWeightUnit
		{
			get { return JL_ActualWeightUQ; }
		}

		ZDecimal ICartageLooseCargo.BookedHeight
		{
			get { return JL_Height; }
		}

		ZDecimal ICartageLooseCargo.BookedWidth
		{
			get { return JL_Width; }
		}

		ZDecimal ICartageLooseCargo.BookedLength
		{
			get { return JL_Length; }
		}

		ZString ICartageLooseCargo.BookedDimensionUnit
		{
			get { return JL_UnitOfDimension; }
		}

		IReadOnlyCollection<UNDGDataItem> ICartageLooseCargo.DangerousGoods
		{
			get { return UNDGs.ToArray(); }
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CFSPackLineFetchStrategy(this);
		}

		#endregion
	}
}
