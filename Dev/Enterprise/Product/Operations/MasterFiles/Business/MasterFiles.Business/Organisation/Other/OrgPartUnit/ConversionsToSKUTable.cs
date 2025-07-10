using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Contains valid conversions from different quantitative pack types to stock keeping units for a product.	
	/// </summary>
	public class ConversionsToSKUTable : IEnumerable<ConversionToSKU>
	{
		public ConversionsToSKUTable(OrgSupplierPart product, bool allowFractionalConversions = false)
		{
			Product = Argument.NotNull(product, nameof(product));
			ConversionMap = new Dictionary<string, ConversionToSKU>(StringComparer.InvariantCultureIgnoreCase);
			AllowFractionalConversions = allowFractionalConversions;
		}

		protected OrgSupplierPart Product { get; }
		Dictionary<string, ConversionToSKU> ConversionMap { get; }
		bool AllowFractionalConversions { get; }

		#region Indexer

		public ConversionToSKU this[string packType]
		{
			get
			{
				BuildTableIfNeeded();
				ConversionMap.TryGetValue(packType, out var conversion);
				return conversion;
			}
		}

		#endregion

		#region BuildTableIfNeeded

		/// <summary>
		/// If you expose new public method or property in this class, make sure this method is called inside it
		/// </summary>
		void BuildTableIfNeeded()
		{
			if (!IsBuilt)
			{
				ConversionNotifications = new List<ConversionNotification>();
				var packTypes = new Queue<string>();
				var uomTypes = Product.Lookups.PackTypes.ToDictionary(p => p.F3_Code, p => p.F3_UOMType);

				ConversionMap.Add(Product.OP_StockKeepingUnit, new ConversionToSKU(Product.OP_StockKeepingUnit, GetUOMType(Product.OP_StockKeepingUnit, 1m, uomTypes)));
				AddConversionsFrom(Product, Product.OP_StockKeepingUnit, packTypes, uomTypes);
				AddConversionsTo(Product, Product.OP_StockKeepingUnit, packTypes, uomTypes);

				while (packTypes.Count > 0)
				{
					var packType = packTypes.Dequeue();
					AddConversionsFrom(Product, packType, packTypes, uomTypes);
					AddConversionsTo(Product, packType, packTypes, uomTypes);
				}

				FindAndRemoveInvalidConversionPaths();
			}
		}

		/// <summary>
		/// As we build the table we may discover invalid conversion paths at the end of the process. This can render a VALID conversion path as INVALID.
		/// For example:
		///  A -> C -> D, and is valid so far
		///  A -> B -> C = 12, and is valid so far
		///  A -> X -> C = 13, rendering the previous conversion to C as invalid (because C = 12 AND C = 13), and the very first conversion uses A -> C, 
		/// and so it too is now rendered invalid.		
		/// </summary>
		void FindAndRemoveInvalidConversionPaths()
		{
			var conversionValues = ConversionMap.Values.ToArray();
			var invalidPackTypes = conversionValues.Where(c => c.IsInvalid).Select(c => c.PackType.ToString()).ToArray();

			foreach (var seemsToBeValidConversion in conversionValues.Where(c => !c.IsInvalid))
			{
				if (invalidPackTypes.Any(p => seemsToBeValidConversion.PackTypesFromConversionPath.Contains(p)))
				{
					seemsToBeValidConversion.IsInvalid = true;
				}
			}
			MarkAdditionalInvalidConversionPaths(conversionValues);

			foreach (var conversion in conversionValues)
			{
				if (conversion.IsInvalid || (!AllowFractionalConversions && !IsWholeNumber(conversion.QtySKU)))
				{
					ConversionMap.Remove(conversion.PackType);
				}
			}
		}

		protected virtual void MarkAdditionalInvalidConversionPaths(IEnumerable<ConversionToSKU> conversions)
		{
		}

		bool IsBuilt => ConversionMap.Count > 0;

		#endregion

		#region Errors and Warnings

		public bool HasErrors
		{
			get { return NotificationsForPackTypes(NotificationTypes.Error).Any(); }
		}

		public IEnumerable<string> ErrorsForPackTypes(IEnumerable<string> packTypes)
		{
			return NotificationsForPackTypes(packTypes, NotificationTypes.Error);
		}

		public IEnumerable<string> WarningsForPackTypes(IEnumerable<string> packTypes)
		{
			return NotificationsForPackTypes(packTypes, NotificationTypes.Warning);
		}

		IEnumerable<string> NotificationsForPackTypes(IEnumerable<string> packTypes, NotificationTypes notificationType)
		{
			return NotificationsForPackTypes(notificationType).Where(e => e.RelatedPackTypes.Intersect(packTypes, StringComparer.OrdinalIgnoreCase).Any()).Select(e => e.NotificationMessage);
		}

		IEnumerable<ConversionNotification> NotificationsForPackTypes(NotificationTypes notificationType)
		{
			BuildTableIfNeeded();
			return ConversionNotifications.Where(e => e.NotificationType == notificationType);
		}

		public bool HasWarnings
		{
			get { return NotificationsForPackTypes(NotificationTypes.Warning).Any(); }
		}

		void AddNotification(string errorText, NotificationTypes notificationType, IEnumerable<string> relatedPackTypes)
		{
			if (!NotificationsForPackTypes(notificationType).Any(e => e.NotificationMessage == errorText))
			{
				ConversionNotifications.Add(new ConversionNotification(errorText, notificationType, relatedPackTypes));
			}
		}

		#endregion

		#region ConversionNotifications

		class ConversionNotification
		{
			public ConversionNotification(string message, NotificationTypes notificationType, IEnumerable<string> relatedPackTypes)
			{
				NotificationMessage = Argument.NotNullOrEmpty(message, nameof(message));
				NotificationType = notificationType;
				RelatedPackTypes = relatedPackTypes;
			}

			public readonly string NotificationMessage;
			public readonly NotificationTypes NotificationType;
			public readonly IEnumerable<string> RelatedPackTypes;
		}

		List<ConversionNotification> ConversionNotifications;

		#endregion

		#region AddConversionsFrom

		void AddConversionsFrom(OrgSupplierPart product, string targetPackType, Queue<string> packTypes, IReadOnlyDictionary<ZString, ZString> uomTypes)
		{
			foreach (var conversion in GetPositiveConversionsFrom(product, targetPackType))
			{
				if (this[conversion.OF_PackType] != null)
				{
					var qtySku = this[targetPackType].QtySKU * conversion.OF_QuantityInParent;
					var row = new ConversionToSKU(conversion.OF_ParentPackType, GetUOMType(conversion.OF_ParentPackType, qtySku, uomTypes), qtySku, this[conversion.OF_PackType].ConversionPath);
					if (!row.ContainsLoopForThePackType)
					{
						EvaluateConversion(row, packTypes);
					}
				}
			}
		}

		#endregion

		#region AddConversionsTo

		void AddConversionsTo(OrgSupplierPart product, string targetPackType, Queue<string> packTypes, IReadOnlyDictionary<ZString, ZString> uomTypes)
		{
			foreach (var conversion in GetPositiveConversionsTo(product, targetPackType))
			{
				if (this[conversion.OF_ParentPackType] != null)
				{
					var qtySku = this[targetPackType].QtySKU / conversion.OF_QuantityInParent;
					var row = new ConversionToSKU(conversion.OF_PackType, GetUOMType(conversion.OF_PackType, qtySku, uomTypes), qtySku, this[conversion.OF_ParentPackType].ConversionPath);
					if (!row.ContainsLoopForThePackType)
					{
						EvaluateConversion(row, packTypes);
					}
				}
			}
		}

		#endregion

		#region GetUOMType

		protected virtual ZString GetUOMType(string packType, decimal qtySku, IReadOnlyDictionary<ZString, ZString> uomTypes)
		{
			uomTypes.TryGetValue(packType.ToUpper(), out var uomType);
			return uomType;
		}

		#endregion

		#region GetPositiveConversionsFrom

		IEnumerable<OrgPartUnit> GetPositiveConversionsFrom(OrgSupplierPart product, string targetPackType)
		{
			return product.PartUnits.Cast<OrgPartUnit>()
				.Where(x => x.OF_PackType.EqualsIgnoringCase(targetPackType) && x.OF_QuantityInParent > 0);
		}

		#endregion

		#region GetPositiveConversionsTo

		IEnumerable<OrgPartUnit> GetPositiveConversionsTo(OrgSupplierPart product, string targetPackType)
		{
			return product.PartUnits.Cast<OrgPartUnit>()
				.Where(x => x.OF_ParentPackType.EqualsIgnoringCase(targetPackType) && x.OF_QuantityInParent > 0);
		}

		#endregion

		#region EvaluateConversion

		void EvaluateConversion(ConversionToSKU conv, Queue<string> packTypes)
		{
			if (IsQuantitativePackType(conv.PackType))
			{
				if (!IsWholeNumber(conv.QtySKU))
				{
					AddNotification(Res.GetString("bb491764-eca9-4bd0-a30b-beafc9fe30e4", "Converting {0} will result in an item that is a fraction of SKU ({1}). {2} cannot be picked in quantities of {1}.", conv.ConversionPath, conv.QtySKU.ToString("0.###"), Product.OP_PartNum), NotificationTypes.Warning, new string[] { conv.PackType });
				}

				if (!ConversionMap.TryGetValue(conv.PackType, out var conversion))
				{
					ConversionMap.Add(conv.PackType, conv);
					packTypes.Enqueue(conv.PackType);
				}
				else if (conversion.QtySKU != conv.QtySKU) // if new route does not have the same qty (eg A -> X = 10, A -> B -> X = 11), this is an error.
				{
					var partsOfThePath = conv.PackTypesFromConversionPath.Union(conversion.PackTypesFromConversionPath).ToList();
					AddNotification(Res.GetString("d360a479-0cb5-46b4-8f5b-42fed71f39c2", "Inconsistent conversion paths. Path {0} creates {1} SKUs, path {2} creates {3} SKUs.",
						conv.ConversionPath, conv.QtySKU.ToString("#.#"), conversion.ConversionPath, conversion.QtySKU.ToString("#.#")), NotificationTypes.Error, partsOfThePath);

					foreach (var invalidPackType in partsOfThePath.Where(p => !Product.OP_StockKeepingUnit.EqualsIgnoringCase(p)))
					{
						this[invalidPackType].IsInvalid = true;
					}
				}
			}
		}

		static bool IsWholeNumber(decimal number) => number % 1 == 0;

		static bool IsQuantitativePackType(ZString packType)
		{
			return
				!Constants.Volume.ContainsCode(packType) &&
				!Constants.Weight.ContainsCode(packType);
		}

		#endregion

		#region IEnumerator<ConversionToSKU> implementation

		IEnumerator<ConversionToSKU> IEnumerable<ConversionToSKU>.GetEnumerator()
		{
			if (!IsBuilt)
			{
				BuildTableIfNeeded();
			}
			return ConversionMap.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ConversionToSKU>)this).GetEnumerator();

		#endregion
	}
}
