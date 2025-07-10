using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Container = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using PackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.BE
{
	static class BEExtensions
	{
		#region ToUXmlContainer

		public static Container ToUXmlContainer(this DGNContainer source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var container = new Container(writerStrategy);
			container.ContainerNumber = source.Number;
			container.PalletCount = source.PackCount;
			container.NonOperatingReefer = source.IsNonOperativeReefer;

			return container;
		}

		public static Container ToUXmlContainer(this CertifiedPickupContainer source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var container = new Container(writerStrategy);
			container.ContainerNumber = source.Number;
			container.ContainerImportDORelease = source.ReleaseIdentification;
			container.NonOperatingReefer = source.IsNonOperativeReefer;

			container.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				if (!source.Action.ObjectValue.IsEmpty)
				{
					addInfos.Add(new AddInfo
					{
						Key = AddInfoKeyAction,
						Value = source.Action.ObjectValue.ToString()
					});

					if (!source.Reason.IsEmpty)
					{
						addInfos.Add(new AddInfo
						{
							Key = AddInfoKeyReason,
							Value = source.Reason
						});
					}

					if (!source.ReleaseFromName.IsEmpty)
					{
						addInfos.Add(new AddInfo
						{
							Key = AddInfoReleaseFromPartyName,
							Value = source.ReleaseFromName
						});
					}

					if (!source.ReleaseFromId.IsEmpty)
					{
						addInfos.Add(new AddInfo
						{
							Key = AddInfoReleaseFromPartyIdType,
							Value = source.ReleaseFromId
						});
					}

					if (!source.ReleaseFromCode.IsEmpty)
					{
						addInfos.Add(new AddInfo
						{
							Key = AddInfoReleaseFromPartyIdCode,
							Value = source.ReleaseFromCode
						});
					}
				}

				return addInfos;
			});

			return container;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const value")]
		public const string AddInfoKeyAction = "Action";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const value")]
		public const string AddInfoKeyReason = "Reason";
		public const string AddInfoReleaseFromPartyName = "ReleaseFromPartyName"; // const value
		public const string AddInfoReleaseFromPartyIdType = "ReleaseFromPartyIdType"; // const value
		public const string AddInfoReleaseFromPartyIdCode = "ReleaseFromPartyIdCode"; // const value

		#endregion

		#region ToUXmlPackingLine

		public static PackingLine ToUXmlPackingLine(this DGNPackingLine source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var packingLine = new PackingLine(writerStrategy);
			packingLine.ContainerNumber = source.ContainerNumber;
			packingLine.GoodsDescription = source.GoodsDescription;
			packingLine.PackQty = new ZLong(source.Quantity);
			packingLine.PackType = source.PackageType?.ToUXmlPackType();
			packingLine.Volume = source.Volume?.Value;
			packingLine.VolumeUnit = source.Volume?.Unit.ToUXmlUnitOfVolume();
			packingLine.Weight = source.Weight?.Value;
			packingLine.WeightUnit = source.Weight?.Unit.ToUXmlUnitOfWeight();
			packingLine.SetUNDGCollection(() => source.DangerousGoods?.Select(d => ToUXmlUNDG(d, writerStrategy)).ToList());

			return packingLine;
		}

		#endregion

		#region ToUXmlUNDG

		static UNDG ToUXmlUNDG(DGNDangerousGood source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var undg = new UNDG(writerStrategy);
			undg.UNDGCode = source.Unno;
			undg.Standard = source.RegulationStandard;
			undg.ProperShippingName = source.ProperShippingName;
			undg.TechicalName = source.TechnicalName;
			undg.PackQty = source.Quantity;
			undg.PackedInLimitedQuantity = source.PackedInLimitedQuantity;
			undg.IMOClass = source.IMOClass;
			undg.PackingGroup = source.PackingGroup;
			undg.SubLabel1 = source.SubLabel1;
			undg.SubLabel2 = source.SubLabel2;
			undg.Weight = source.Weight?.Value;
			undg.State = new UNDGStateConverter().ToEnumValue(source.State);
			undg.WeightUQ = new UnitOfWeight { Code = source.Weight?.Unit?.Code, Description = source.Weight?.Unit?.Description };
			undg.Volume = source.Volume?.Value;
			undg.VolumeUQ = new UnitOfVolume { Code = source.Volume?.Unit?.Code, Description = source.Volume?.Unit?.Description };
			undg.PackType = new PackageType { Code = source.PackageType?.Code, Description = source.PackageType?.Description };
			undg.MarinePollutant = new UNDGMarinePollutant { Code = source.MarinePollutant?.Code, Description = source.MarinePollutant?.Description };
			undg.NetExplosiveWeight = source.NetExplosiveWeight?.Value;
			undg.NetExplosiveWeightUQ = new UnitOfWeight { Code = source.NetExplosiveWeight?.Unit?.Code, Description = source.NetExplosiveWeight?.Unit?.Description };
			undg.Radioactivity = source.Radioactivity?.Value;
			undg.RadioactivityUQ = new UnitOfRadioactivity { Code = source.Radioactivity?.Unit?.Code, Description = source.Radioactivity?.Unit?.Description };
			undg.RadioactiveTransportIndex = source.RadioactiveTransportIndex;
			undg.RadioactiveCriticalitySafetyIndex = source.RadioactiveCriticalitySafetyIndex;
			undg.PackedInExceptedQuantity = source.PackedInExceptedQuantity;
			undg.EmergencyScheduleFire = source.EmergencyScheduleFire?.ToUXmlCodeDescriptionPair();
			undg.EmergencyScheduleSpillage = source.EmergencyScheduleSpillage?.ToUXmlCodeDescriptionPair();
			undg.MedicalFirstAidGuide = source.MedicalFirstAidGuide.IsEmpty
				? null
				: new CodeDescriptionPair4Char { Code = source.MedicalFirstAidGuide };

			if (source.FlashPoint != null)
			{
				undg.FlashPoint = source.FlashPoint?.Value.ToString();
			}

			return undg;
		}

		#endregion
	}
}
