using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	class ContainerLoadPlanSOGroupingBuilder
	{
		public ContainerLoadPlanSOGrouping Build(string bookingNumber, IEnumerable<PackLine> packingLines)
		{
			if (packingLines == null || !packingLines.Any())
			{
				return null;
			}

			var context = new CommonContext(packingLines.First().Factory);

			var unitOfWeight = Core.Constants.Weight.Kilograms;
			var unitOfVolume = Core.Constants.Volume.CubicMetres;

			var group = new ContainerLoadPlanSOGrouping(bookingNumber);
			group.SONumber = bookingNumber;
			group.MarksAndNumbers = GetMarksAndNumbers(packingLines);
			group.GoodsDescription = GetGoodsDescription(packingLines);
			group.Quantity = packingLines.Sum(x => x.JL_PackageCount);
			group.PackageType = GetPackageType(packingLines);

			var weight = packingLines.Sum(x => Core.Constants.Weight.Convert(x.JL_ActualWeight, x.JL_ActualWeightUQ, unitOfWeight));
			group.Weight = new Measurement()
			{
				Value = weight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			var volume = packingLines.Sum(x => Core.Constants.Volume.Convert(x.JL_ActualVolume, x.JL_ActualVolumeUQ, unitOfVolume));
			group.Volume = new Measurement()
			{
				Value = volume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = unitOfVolume
				}
			};

			var undgBuilder = new DangerousGoodBuilder();
			group.DangerousGoods = packingLines
				.SelectMany(x => x.UNDGs)
				.Select(x => undgBuilder.Build(x, context)).ToArray();

			group.DangerousGoodsDescription = string.Join(System.Environment.NewLine, group.DangerousGoods.Select(d => d.ToString()));

			return group;
		}

		ZString GetMarksAndNumbers(IEnumerable<PackLine> packLines)
		{
			var group = packLines
				.GroupBy(x => x.JL_MarksAndNumbers.IsEmpty
				? x.Shipment?.JS_MarksAndNumbers ?? ZString.Empty
				: x.JL_MarksAndNumbers);

			return group.Count() == 1 ? group.First().Key : new ZString(string.Join(", ", group.Select(x => x.Key)));
		}

		CodeDescription GetPackageType(IEnumerable<PackLine> packLines)
		{
			var group = packLines.GroupBy(x => x.JL_F3_NKPackType);

			return new CodeDescription(packLines.First().Lookups.PackTypes)
			{
				Code = group.Count() == 1
					? group.First().Key
					: (ZString)Core.Constants.PkgUnit.Package
			};
		}

		string GetGoodsDescription(IEnumerable<PackLine> packLines)
		{
			var pack = packLines.First();

			return new[]
			{
				pack.JL_DetailedDescription,
				pack.JL_Description,
				pack.Shipment?.DetailedGoodsDescriptionNoteText ?? ZString.Empty,
				pack.Shipment?.JS_GoodsDescription ?? ZString.Empty
			}.FirstOrDefault(desc => !desc.IsEmpty);
		}
	}
}
