using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class NonPersistentCusContainer : NonPersistentBusinessObject, IObsoleteValidation, IContainer
	{
		#region Schema
		public class Schema : BaseCusContainer.Schema
		{
			public const string ContainerNumber = "ContainerNumber";
			public const string IsForInvoiceLine = "IsForInvoiceLine";
			public const string Seal = "Seal";
			public const string Mode = "Mode";
			public const string OwnerCountry = "OwnerCountry";
			public const string ContainerWeight = "ContainerWeight";
			public const string GrossWeightInKG = "GrossWeightInKG";
			public const string NetWeightInKG = "NetWeightInKG";
			public const string SplitValue = "SplitValue";
			public const string SplitValueCurrency = "SplitValueCurrency";
			public const string PackQty = "PackQty";
		}

		#endregion

		public NonPersistentCusContainer(BaseJobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory)
		{
			this.InvoiceLine = invoiceLine;
		}
		protected readonly BaseJobComInvoiceLine InvoiceLine;

		protected override void AddToFactoryCache()
		{
			//DO NOT Allow memory to be held up by factory
		}

		protected sealed override void OnFactorySaved(bool saveSucceeded)
		{
		}

		protected sealed override void OnFactorySaving()
		{
		}

		public sealed override void OnSaved(bool saveSucceeded)
		{
		}

		public sealed override void OnSaving()
		{
		}

		#region related objects

		BaseCusContainer fContainer;
		public BaseCusContainer Container
		{
			get
			{
				if (fContainer == null || fContainer.IsDeleted)
				{
					return null;
				}
				else
				{
					return fContainer;
				}
			}
			set
			{
				fContainer = value;
				var pivot = Pivot;
				if (!IsValidationSuspended)
				{
					ValidateIsForInvoiceLine(pivot);
				}
				UpdatePivotProxyPropertiesReadOnly();
			}
		}

		public CusContainerInvoiceLinePivot Pivot
		{
			get { return Container != null ? InvoiceLine.ContainersPivot.GetRelatedPivot(Container) : null; }
		}

		#endregion

		#region New property

		/// <summary>
		/// Always in local currency
		/// </summary>
		public ZDecimal CustomsValue
		{
			get
			{
				ZDecimal result = 0m;
				if (InvoiceLine != null && Pivot != null && InvoiceLine.JI_LinePrice != 0m)
				{
					result = InvoiceLine.JI_CustomsValue * Pivot.C2_SplitValue / InvoiceLine.JI_LinePrice;
				}
				return result;
			}
		}

		#endregion

		#region Properties

		#region ContainerNumber
		public ZString ContainerNumber
		{
			get { return Container == null ? ZString.Empty : Container.CO_ContainerNumber; }
		}

		public ZPropertyInfo ContainerNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerNumber); }
		}
		#endregion

		#region IsForInvoiceLine

		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(IsForInvoiceLine_ReadOnly))]
		public virtual ZBool IsForInvoiceLine
		{
			get { return Pivot != null; }
			set
			{
				if (IsForInvoiceLine != value)
				{
					CusContainerInvoiceLinePivot pivot = InvoiceLine.ToggleLinkageWithContainer(Container, value);
					UpdatePivotProxyPropertiesReadOnly();
					if (!IsValidationSuspended)
					{
						ValidateIsForInvoiceLine(pivot);
					}
					IsForInvoiceLineInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsForInvoiceLineInfo
		{
			get { return GetZPropertyInfo(Schema.IsForInvoiceLine); }
		}

		protected ZBool IsForInvoiceLine_ReadOnly => InvoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking;

		#endregion

		#region Seal

		public ZString Seal
		{
			get { return Container == null ? ZString.Empty : Container.CO_Seal; }
		}

		public ZPropertyInfo SealInfo
		{
			get { return GetZPropertyInfo(Schema.Seal); }
		}

		#endregion

		#region ContainerWeight

		public ZDecimal ContainerWeight
		{
			get { return Container == null ? ZDecimal.Zero : Container.CO_Weight; }
		}

		public ZPropertyInfo ContainerWeightInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerWeight); }
		}

		#endregion

		#region GrossWeightInKG

		[ReadOnlyMember(nameof(IsNotForInvoiceLine))]
		public ZDecimal GrossWeightInKG
		{
			get { return Pivot == null ? ZDecimal.Zero : Pivot.C2_GrossWeight; }
			set
			{
				CusContainerInvoiceLinePivot pivot = this.Pivot;
				if (pivot != null)
				{
					pivot.C2_GrossWeight = value;
					if (!IsValidationSuspended)
					{
						ValidateGrossWeightInKG();
					}
				}
				else
				{
					ErrorReporter.ReportOnce(NoPivotAndCantAssignValue, NoPivotAndCantAssignValue);
				}
				GrossWeightInKGInfo.RefreshBinding();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "error report")]
		internal const string NoPivotAndCantAssignValue = "There is no pivot at this point. Please tick IsForInvoiceLine first";

		public ZPropertyInfo GrossWeightInKGInfo
		{
			get { return GetZPropertyInfo(Schema.GrossWeightInKG); }
		}

		#endregion

		#region NetWeightInKG

		[ReadOnlyMember(nameof(IsNotForInvoiceLine))]
		public ZDecimal NetWeightInKG
		{
			get { return Pivot == null ? ZDecimal.Zero : Pivot.C2_NetWeight; }
			set
			{
				if (Pivot != null)
				{
					Pivot.C2_NetWeight = value;
					if (!IsValidationSuspended)
					{
						ValidateNetWeightInKG();
					}
				}
				else
				{
					ErrorReporter.ReportOnce(NoPivotAndCantAssignValue, NoPivotAndCantAssignValue);
				}
				NetWeightInKGInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NetWeightInKGInfo
		{
			get { return GetZPropertyInfo(Schema.NetWeightInKG); }
		}

		#endregion

		#region SplitValue

		[ReadOnlyMember(nameof(IsNotForInvoiceLine))]
		public ZDecimal SplitValue
		{
			get { return Pivot == null ? ZDecimal.Zero : Pivot.C2_SplitValue; }
			set
			{
				if (Pivot != null)
				{
					Pivot.C2_SplitValue = value;
					if (!IsValidationSuspended)
					{
						ValidateSplitValue();
					}
				}
				else
				{
					ErrorReporter.ReportOnce(NoPivotAndCantAssignValue, NoPivotAndCantAssignValue);
				}
				SplitValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SplitValueInfo
		{
			get { return GetZPropertyInfo(Schema.SplitValue); }
		}

		public ZGuid SplitValueCurrency
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(InvoiceLine.InvoiceHeader.Factory, InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency);
				return currency != null ? currency.PK : ZGuid.Empty;
			}
		}

		public ZPropertyInfo SplitValueCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.SplitValueCurrency); }
		}

		public RefCurrencyCollection CurrencyList
		{
			get { return new RefCurrencyCollection(InvoiceLine.Factory); }
		}

		#endregion

		#region PackQty

		[ReadOnlyMember(nameof(IsNotForInvoiceLine))]
		public ZInt PackQty
		{
			get { return Pivot == null ? ZInt.Zero : Pivot.C2_PackQty; }
			set
			{
				if (Pivot != null)
				{
					Pivot.C2_PackQty = value;
					if (!IsValidationSuspended)
					{
						ValidatePackQty();
					}
				}
				else
				{
					ErrorReporter.ReportOnce(NoPivotAndCantAssignValue, NoPivotAndCantAssignValue);
				}
				PackQtyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PackQtyInfo
		{
			get { return GetZPropertyInfo(Schema.PackQty); }
		}

		#endregion

		#region Mode

		public ZString Mode
		{
			get { return Container == null ? ZString.Empty : Container.CO_FCL_LCL_AIR; }
		}

		public ZPropertyInfo ModeInfo
		{
			get { return GetZPropertyInfo(Schema.Mode); }
		}

		#endregion

		#region Owner Country

		public ZString OwnerCountry
		{
			get { return Container == null ? ZString.Empty : Container.CO_RN_NKOwnerCountry; }
		}

		public ZPropertyInfo OwnerCountryInfo
		{
			get { return GetZPropertyInfo(Schema.OwnerCountry); }
		}

		#endregion

		protected bool IsNotForInvoiceLine
		{
			get { return !IsForInvoiceLine; }
		}

		#endregion

		#region Validation

		void ValidateIsForInvoiceLine(CusContainerInvoiceLinePivot pivot)
		{
			IsForInvoiceLineInfo.ClearAllNotifications();
			if (pivot != null)
			{
				pivot.Validation.ValidateC2_CO();
				IsForInvoiceLineInfo.AddAllNotificationsFrom(pivot.C2_COInfo);
			}
			InvoiceLine.Validation.ValidateJI_ContainerMode();
			ValidateGrossWeightInKG();
			ValidateNetWeightInKG();
			ValidatePackQty();
			ValidateSplitValue();
		}

		void ValidateNetWeightInKG()
		{
			NetWeightInKGInfo.ClearAllNotifications();
			var pivot = Pivot;
			if (pivot != null)
			{
				pivot.Validation.ValidateC2_NetWeight();
				NetWeightInKGInfo.AddAllNotificationsFrom(pivot.C2_NetWeightInfo);
			}
		}

		void ValidateGrossWeightInKG()
		{
			GrossWeightInKGInfo.ClearAllNotifications();
			var pivot = Pivot;
			if (pivot != null)
			{
				pivot.Validation.ValidateC2_GrossWeight();
				GrossWeightInKGInfo.AddAllNotificationsFrom(pivot.C2_GrossWeightInfo);
			}
		}

		void ValidateSplitValue()
		{
			SplitValueInfo.ClearAllNotifications();
			var pivot = Pivot;
			if (pivot != null)
			{
				pivot.Validation.ValidateC2_SplitValue();
				SplitValueInfo.AddAllNotificationsFrom(pivot.C2_SplitValueInfo);
			}
		}

		void ValidatePackQty()
		{
			PackQtyInfo.ClearAllNotifications();
			var pivot = Pivot;
			if (pivot != null)
			{
				pivot.Validation.ValidateC2_PackQty();
				PackQtyInfo.AddAllNotificationsFrom(pivot.C2_PackQtyInfo);
			}
		}

		#endregion

		void UpdatePivotProxyPropertiesReadOnly()
		{
			GrossWeightInKGInfo.RefreshBinding();
			NetWeightInKGInfo.RefreshBinding();
			SplitValueInfo.RefreshBinding();
			PackQtyInfo.RefreshBinding();
		}
	}
}
