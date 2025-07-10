using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[CodeAlive("ICR is WIP development")]
	public class ICREntryLineWrapper : IICRConsignmentItem
	{
		public ICREntryLineWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, "EntryLine cannot be null");
		}

		readonly CusEntryLine entryLine;

		#region IICRConsignmentItem Implementation

		public ZShort SequenceNumber
		{
			get { return entryLine.CL_LineNumber; }
		}

		public ZBool IsEmptyContainer => false;

		public ZString GoodsDescription
		{
			get { return entryLine.GoodsDescription; }
		}

		public ZString IdentityNumber
		{
			get { return ZString.Empty; }   //TODO: IdentityNumber
		}

		public ZDecimal Value
		{
			get { return entryLine.VFDWholeNZD; }
		}

		public ZString Currency
		{
			get { return Core.Constants.CurrencyCodes.NewZealand; } //TODO: ItemCurrency
		}

		public ZString IdentityType
		{
			get { return ZString.Empty; }
		}

		public IEnumerable<IClassification> Classifications
		{
			get
			{
				if (!entryLine.CL_AdValoremTariff.IsEmpty)
				{
					yield return new ICRClassification(entryLine.CL_AdValoremTariff, ClassificationTypeList.Codes.HS);
				}

				if (!entryLine.ConcessionCode.IsEmpty)
				{
					yield return new ICRClassification(entryLine.ConcessionCode, ClassificationTypeList.Codes.CV);
				}

				//SSO = "United Nations Dangerous Goods List";
				if (!entryLine.UNDGNo.IsEmpty)
				{
					yield return new ICRClassification(entryLine.UNDGNo, ClassificationTypeList.Codes.SSO);
				}

				////SSQ = "International Code of Zoological Nomenclature";
				////SSR = "International Code of Nomenclature for Cultivated Plants"
				//foreach (CommodityLine commodityLine in entryLine.CommodityLines)
				//{
				//	if (commodityLine.NZ_ClassificationType == ClassificationTypeList.Codes.SSQ ||
				//		commodityLine.NZ_ClassificationType == ClassificationTypeList.Codes.SSR)
				//	{
				//		var classification = new Classificaton(commodityLine.NZ_Classification, commodityLine.NZ_ClassificationType, 0);
				//		yield return (IClassification)classification;
				//	}
				//}
			}
		}

		public ZBool SendFlashpointTemp => entryLine.RandomLine.UNDGs.Count > 0;

		public ZDecimal FlashpointTempInCelsius
		{
			get
			{
				var result = 0m;
				if (!entryLine.UNDGNo.IsEmpty)
				{
					result = entryLine.RandomLine.UNDGs[0].DI_DGFlashPoint;
				}

				return result;
			}
		}

		public ITemperatureRequirements Temperatures
		{
			get
			{
				ITemperatureRequirements result = null;
				if (entryLine.RandomLine.JI_TemperatureDetailsToBeSent)
				{
					result = new TemperatureRequirements(entryLine.RandomLine.JI_StorageTemp, entryLine.RandomLine.JI_MinTemp, entryLine.RandomLine.JI_MaxTemp);
				}

				return result;
			}
		}

		public ZDecimal GrossWeightInKg
		{
			get { return entryLine.EffectiveGrossWeight.InKilograms; }
		}

		public ZString GoodsOriginCountry
		{
			get { return entryLine.CountryOfOrigin; }
		}

		public ZInt PackageQty
		{
			get { return entryLine.InvoiceQuantity.ToZInt(); }
		}

		public ZString PackageType
		{
			get { return entryLine.InvoiceUQ; }
		}

		public ZString ContainerNumber  //TODO: line container number - Customs only want 1 container number here??
		{
			get
			{
				var result = ZString.Empty;
				if (entryLine.RandomLine.ContainersForInvoiceLinesForBindingOnly.Count > 0)
				{
					foreach (Customs.Business.NonPersistentCusContainer lineContainer in entryLine.RandomLine.ContainersForInvoiceLinesForBindingOnly)
					{
						if (lineContainer.IsForInvoiceLine)
						{
							result = lineContainer.ContainerNumber;
							break;
						}
					}
				}
				return result;
			}
		}

		public ZString MPIApprovedSystemNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Temperature Requirements

		class TemperatureRequirements : ITemperatureRequirements
		{
			public TemperatureRequirements(ZDecimal store, ZDecimal min, ZDecimal max)
			{
				storageTemp = store;
				minStorageTemp = min;
				maxStorageTemp = max;
			}

			readonly ZDecimal storageTemp;
			readonly ZDecimal minStorageTemp;
			readonly ZDecimal maxStorageTemp;
			const string tempUnit = "CEL";

			public ZDecimal StorageTemp
			{
				get { return storageTemp; }
			}

			public ZString StorageTempUnit
			{
				get { return tempUnit; }
			}

			public ZDecimal MinStorageTemp
			{
				get { return minStorageTemp; }
			}

			public ZString MinStorageTempUnit
			{
				get { return tempUnit; }
			}

			public ZDecimal MaxStorageTemp
			{
				get { return maxStorageTemp; }
			}

			public ZString MaxStorageTempUnit
			{
				get { return tempUnit; }
			}
		}

		#endregion
	}
}
