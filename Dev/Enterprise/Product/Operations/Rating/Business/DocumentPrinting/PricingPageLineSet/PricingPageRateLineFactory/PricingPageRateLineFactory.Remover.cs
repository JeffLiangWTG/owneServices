using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public partial class PricingPageRateLineFactory
	{
		sealed class Remover : SimpleOverriddenRateLinesRemover
		{
			public Remover(RateEntry parentEntry, ZString containerCode)
				: base(parentEntry.Criteria)
			{
				this.parentEntry = parentEntry;
				this.isFreight = parentEntry.IsFreightEntry();
				this.containerCode = containerCode;
			}

			internal override LinkedList<BaseRateLineComparer> GetComparers()
			{
				var result = base.GetComparers();
				result.AddAfter(result.Find(new ContainerTypeColumnComparer()), new ContainerClassComparer(containerCode));

				return result;
			}

			internal override bool IsComparable(FastLine line, FastLine possibleOverride)
			{
				if (!base.IsComparable(line, possibleOverride))
				{
					return false;
				}

				var entry = line.ParentRateEntry;
				var possibleOverrideEntry = possibleOverride.ParentRateEntry;

				if (isFreight)
				{
					if (possibleOverrideEntry.TI_OH_Supplier != entry.TI_OH_Supplier && (!possibleOverrideEntry.IsFreightEntry() || !entry.IsFreightEntry()))
					{
						return false;
					}

					if (distinguishingColumns == null)
					{
						distinguishingColumns = DistinguishingColumnsForDocumentGrouping;
					}

					foreach (var distinguishingColumn in distinguishingColumns)
					{
						if (!IsComparableByColumn(entry, possibleOverrideEntry, distinguishingColumn))
						{
							return false;
						}
					}
				}

				if (line.Calculator.MessageType != possibleOverride.Calculator.MessageType || line.Calculator.MessageSubType != possibleOverride.Calculator.MessageSubType)
				{
					return false;
				}

				var parentOrigin = parentEntry.Origin();
				var parentDestination = parentEntry.Destination();

				return (parentOrigin == null || GetLineEffectiveOrigin(parentOrigin, line) == GetLineEffectiveOrigin(parentOrigin, possibleOverride))
					&& (parentDestination == null || GetLineEffectiveDestination(parentDestination, line) == GetLineEffectiveDestination(parentDestination, possibleOverride));
			}

			bool IsComparableByColumn(IRateEntry entry, IRateEntry possibleOverride, SchemaColumn column)
			{
				var boEntry = RateEntry.GetBO(entry);
				var boPossibleOverride = RateEntry.GetBO(possibleOverride);

				if (!((IZType)parentEntry[column]).IsEmpty)
				{
					return true;
				}
				else if (object.Equals(boEntry[column], boPossibleOverride[column]))
				{
					return true;
				}
				else if (((IZType)boPossibleOverride[column]).IsEmpty)
				{
					return HasHigherPrecedence(boPossibleOverride.Parent, boEntry.Parent);
				}
				else if (((IZType)boEntry[column]).IsEmpty)
				{
					return HasHigherPrecedence(boEntry.Parent, boPossibleOverride.Parent);
				}
				else
				{
					return false;
				}
			}

			/// <summary>
			/// Returns if the first has higher precedence than the second one.
			/// Precedence: Quote > Client Rate > Company Tariff
			/// </summary>
			static bool HasHigherPrecedence(RatingHeader firstHeader, RatingHeader secondHeader)
			{
				if (firstHeader.TH_RateType == secondHeader.TH_RateType)
				{
					return false;
				}
				else if (firstHeader.IsQuote())
				{
					return true;
				}
				else if (secondHeader.IsQuote())
				{
					return false;
				}
				else if (firstHeader.IsClientRate())
				{
					return true;
				}
				else
				{
					return false;
				}
			}

			static string GetLineEffectiveOrigin(ILocation parentOrigin, FastLine line)
				=> line == null || line.ParentRateEntry == null || !line.ParentRateEntry.IsOriginEntry() || !parentOrigin.CompletelyCovers(line.ParentRateEntry.Origin())
					? parentOrigin.Description
					: line.ParentRateEntry.Origin().Description;

			static string GetLineEffectiveDestination(ILocation parentDestination, FastLine line)
				=> line == null || line.ParentRateEntry == null || !line.ParentRateEntry.IsDestinationEntry() || !parentDestination.CompletelyCovers(line.ParentRateEntry.Destination())
					? parentDestination.Description
					: line.ParentRateEntry.Destination().Description;

			SchemaColumn[] distinguishingColumns;
			readonly bool isFreight;
			readonly RateEntry parentEntry;
			readonly ZString containerCode;
		}
	}
}
