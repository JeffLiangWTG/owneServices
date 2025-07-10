using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public enum MergeOption
	{
		Merge,
		MergeAll
	}

	public class ForwardingPackLineMerger : BusinessObjectMerger<ForwardingPackLine>
	{
		public ForwardingPackLineMerger(IEnumerable<ForwardingPackLine> sourceList, MergeOption mergeOption, ForwardingConsol consol)
			: base(sourceList)
		{
			Argument.NotNull(consol, "Consol cannot be null");

			this.mergeOption = mergeOption;
			this.consol = consol;
		}

		readonly MergeOption mergeOption;
		readonly ForwardingConsol consol;

		#region Merge

		public override void DoMerge()
		{
			if (mergeOption == MergeOption.Merge)
			{
				Merge(SourceList);
			}
			else
			{
				MergeAll();
			}
		}

		void Merge(IEnumerable<ForwardingPackLine> sourcePackLines)
		{
			ForwardingPackLine firstPackLine = null;
			CommonContainer container = null;

			var packLinesArrary = sourcePackLines.ToArray();
			foreach (var packLine in packLinesArrary)
			{
				if (firstPackLine == null)
				{
					firstPackLine = packLine;
					container = packLine.GetContainer(consol);
				}
				else if (packLine != null)
				{
					MergePackLines(packLine, firstPackLine, container);
				}
			}

			MergeProducts(packLinesArrary, firstPackLine);
		}

		void MergeAll()
		{
			GroupAndMergePackLines(SourceList.ToList());
		}

		void MergeProducts(ForwardingPackLine[] packLines, ForwardingPackLine newPackLine)
		{
			var products = packLines.SelectMany(c => c.Products);
			var helper = new PackProductMerger(products, newPackLine);
			helper.DoMerge();
		}

		void GroupAndMergePackLines(List<ForwardingPackLine> sourcePackLines)
		{
			var firstPackLine = sourcePackLines.FirstOrDefault();
			if (firstPackLine != null)
			{
				var count = firstPackLine.UNDGs.Count;
				if (count == 0 || count == 1)
				{
					var samePackLines = sourcePackLines
					.Where(c => BelongToTheSameContainer(c, firstPackLine)
								&& HaveSameValuesInComparableColumns(c, firstPackLine)
								&& HaveSameDgItemCountAndValues(c, firstPackLine, count))
					.ToList();

					foreach (var packLine in samePackLines)
					{
						sourcePackLines.Remove(packLine);
					}

					if (samePackLines.Count > 1)
					{
						Merge(samePackLines);
					}
				}

				GroupAndMergePackLines(sourcePackLines);
			}
		}

		bool HaveSameDgItemCountAndValues(ForwardingPackLine source, ForwardingPackLine target, int targetCount)
		{
			if (source.PK == target.PK)
			{
				return true;
			}

			var result = source.UNDGs.Count == targetCount;
			if (result && targetCount == 1)
			{
				var sourceDgItem = source.UNDGs.First();
				var targetDgItem = target.UNDGs.First();

				result = UNDGDataCompareSchemas.All(c => Equals(sourceDgItem[c], targetDgItem[c]));

				if (result)
				{
					result = sourceDgItem.DI_DGWeight.IsEmpty
							&& sourceDgItem.DI_DGVolume.IsEmpty
							&& sourceDgItem.DI_PackageCount.IsEmpty;
				}
			}

			return result;
		}

		bool BelongToTheSameContainer(ForwardingPackLine source, ForwardingPackLine target)
		{
			return (source.PK == target.PK)
				|| (source.GetContainer(consol) == null && target.GetContainer(consol) == null)
				|| (source.GetContainer(consol)?.PK == target.GetContainer(consol)?.PK);
		}

		void MergePackLines(ForwardingPackLine source, ForwardingPackLine target, CommonContainer container)
		{
			target.JL_Outturn += source.JL_Outturn;
			target.JL_PackageCount += source.JL_PackageCount;
			target.JL_Pillaged += source.JL_Pillaged;
			target.JL_Damaged += source.JL_Damaged;
			target.JL_LoadingMeters += source.JL_LoadingMeters;

			target.JL_OutturnedWeight += Core.Constants.Weight.Convert(source.JL_OutturnedWeight, source.JL_OutturnWeightUQ, target.JL_OutturnWeightUQ);
			target.JL_ActualWeight += Core.Constants.Weight.Convert(source.JL_ActualWeight, source.JL_ActualWeightUQ, target.JL_ActualWeightUQ);

			target.JL_OutturnedVolume += Core.Constants.Volume.Convert(source.JL_OutturnedVolume, source.JL_OutturnVolumeUQ, target.JL_OutturnVolumeUQ);
			target.JL_ActualVolume += Core.Constants.Volume.Convert(source.JL_ActualVolume, source.JL_ActualVolumeUQ, target.JL_ActualVolumeUQ);

			source.UNDGs.RemoveAllFromRelationship();
			source.UNDGs.DeleteAll();

			foreach (var location in source.PackLocations.Cast<PackLocation>().ToArray())
			{
				source.PackLocations.Remove(location);
				target.PackLocations.Add(location);
			}

			if (container != null)
			{
				container.RemovePackLine(source);
			}

			source.Delete();
		}

		#endregion

		#region Columns

		protected override IEnumerable<SchemaColumn> GetComparableColumnsCore()
		{
			return new SchemaColumn[]
			{
				JobPackLinesSchema.JL_CustomAttrib1,
				JobPackLinesSchema.JL_CustomAttrib2,
				JobPackLinesSchema.JL_CustomAttrib3,
				JobPackLinesSchema.JL_CustomAttrib4,
				JobPackLinesSchema.JL_CustomDate1,
				JobPackLinesSchema.JL_CustomDate2,
				JobPackLinesSchema.JL_CustomDecimal1,
				JobPackLinesSchema.JL_CustomDecimal2,
				JobPackLinesSchema.JL_CustomFlag1,
				JobPackLinesSchema.JL_CustomFlag2,
				JobPackLinesSchema.JL_Description,
				JobPackLinesSchema.JL_DetailedDescription,
				JobPackLinesSchema.JL_EndItemNo,
				JobPackLinesSchema.JL_ImportRefNumber,
				JobPackLinesSchema.JL_ExportRefNumber,
				JobPackLinesSchema.JL_F3_NKPackType,
				JobPackLinesSchema.JL_FreightMode,
				JobPackLinesSchema.JL_HarmonisedCode,
				JobPackLinesSchema.JL_Height,
				JobPackLinesSchema.JL_ItemNo,
				JobPackLinesSchema.JL_JS,
				JobPackLinesSchema.JL_JSL_BookingLine,
				JobPackLinesSchema.JL_RC_ContainerType,
				JobPackLinesSchema.JL_Length,
				JobPackLinesSchema.JL_LinePrice,
				JobPackLinesSchema.JL_MarksAndNumbers,
				JobPackLinesSchema.JL_OriginTransitWarehouseStatus,
				JobPackLinesSchema.JL_OutturnComment,
				JobPackLinesSchema.JL_OutturnedHeight,
				JobPackLinesSchema.JL_OutturnedLength,
				JobPackLinesSchema.JL_OutturnedWidth,
				JobPackLinesSchema.JL_RefNumber,
				JobPackLinesSchema.JL_RequiredTemperatureMaximum,
				JobPackLinesSchema.JL_RequiredTemperatureMinimum,
				JobPackLinesSchema.JL_RequiredTemperatureUnit,
				JobPackLinesSchema.JL_RequiresTemperatureControl,
				JobPackLinesSchema.JL_RH_NKCommodityCode,
				JobPackLinesSchema.JL_RN_NKOrigin,
				JobPackLinesSchema.JL_UnitOfDimension,
				JobPackLinesSchema.JL_VehicleColor,
				JobPackLinesSchema.JL_VehicleMake,
				JobPackLinesSchema.JL_VehicleModel,
				JobPackLinesSchema.JL_VehicleNumberOfDoors,
				JobPackLinesSchema.JL_VehicleTransmission,
				JobPackLinesSchema.JL_VehicleYear,
				JobPackLinesSchema.JL_Width,
				JobPackLinesSchema.JL_DepartureTransitWarehouseExcluded
			};
		}

		protected override IEnumerable<SchemaColumn> GetIgnoredColumnsCore()
		{
			return new SchemaColumn[]
			{
					JobPackLinesSchema.PK,
					JobPackLinesSchema.JL_JL_OuterPackLine,
					JobPackLinesSchema.JL_IsValid,
					JobPackLinesSchema.JL_Outturn,
					JobPackLinesSchema.JL_PackageCount,
					JobPackLinesSchema.JL_PackLineId,
					JobPackLinesSchema.JL_Pillaged,
					JobPackLinesSchema.JL_Damaged,
					JobPackLinesSchema.JL_LoadingMeters,
					JobPackLinesSchema.JL_ActualWeight,
					JobPackLinesSchema.JL_ActualVolume,
					JobPackLinesSchema.JL_OutturnedWeight,
					JobPackLinesSchema.JL_OutturnedVolume,
					JobPackLinesSchema.JL_ActualWeightUQ,
					JobPackLinesSchema.JL_ActualVolumeUQ,
					JobPackLinesSchema.JL_IsHighRisk,
					JobPackLinesSchema.JL_ContainerPackingOrder,
					JobPackLinesSchema.JL_LastKnownTransitWarehouseStatus,
					JobPackLinesSchema.JL_LastKnownTransitWarehouseStatusDateTime,
					JobPackLinesSchema.JL_OA_LastKnownTransitWarehouseAddress,
					JobPackLinesSchema.JL_SystemCreateTimeUtc,
					JobPackLinesSchema.JL_SystemCreateUser,
					JobPackLinesSchema.JL_SystemLastEditTimeUtc,
					JobPackLinesSchema.JL_SystemLastEditUser
			};
		}

		IEnumerable<SchemaColumn> UNDGDataCompareSchemas
		{
			get
			{
				return undgDataCompareSchemas ?? (undgDataCompareSchemas = UNDGDataItemSchema
					.All
					.Cast<SchemaColumn>()
					.Where(c => !c.IsPKColumn && c.Name != UNDGDataItemSchema.DI_ParentID.Name));
			}
		}
		IEnumerable<SchemaColumn> undgDataCompareSchemas;

		#endregion

		#region Check

		public override string CheckMerger()
		{
			if (mergeOption == MergeOption.Merge)
			{
				if (SourceList.Count() <= 1)
				{
					return Res.GetString("165b52d4-a8cf-4728-bbff-9767830f9d10", "Please choose at least two pack lines!");
				}

				var shipments = SourceList.Select(c => c.JL_JS).Distinct();
				if (shipments.Count() != 1)
				{
					return Res.GetString("a6f37016-9aea-470e-834b-47d7a8928d8c", "Unable to merge pack lines from different shipments.");
				}

				var fistPackLine = SourceList.First();
				var count = fistPackLine.UNDGs.Count;
				if (!SourceList.All(c => HaveSameDgItemCountAndValues(c, fistPackLine, count)))
				{
					return Res.GetString("58d77302-88e9-4e19-86ca-5e897a0a697d", "Pack lines containing Dangerous Goods items can be merged if they have only one DG item, all parameters are similar, and Volume, Weight and Package count are zero.");
				}

				return GetDifferentColumnValuesError();
			}

			return string.Empty;
		}

		#endregion
	}
}
