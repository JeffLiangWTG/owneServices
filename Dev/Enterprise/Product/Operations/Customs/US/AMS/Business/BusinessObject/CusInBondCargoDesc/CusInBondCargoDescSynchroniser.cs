using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondCargoDescSynchroniser : Customs.Business.CusInBondCargoDescSynchroniser
	{
		readonly PackLine linkedOuterPackLine;

		public CusInBondCargoDescSynchroniser(CusInBondCargoDesc destination, PackLine source) : base(destination, source)
		{
			linkedOuterPackLine = source;
		}

		public CusInBondCargoDescSynchroniser(CusInBondCargoDesc destination, PackLine source, PackLine linkedPackLine) : base(destination, source)
		{
			linkedOuterPackLine = linkedPackLine;
		}

		public new CusInBondCargoDesc Destination
		{
			get { return (CusInBondCargoDesc)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			if (Source.IsOuterPackType)
			{
				base.HookSynchronisers();
			}
			else if (linkedOuterPackLine != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_FormattedHarmonisedTariffInfo, linkedOuterPackLine.JL_HarmonisedCodeInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_MonetaryValueInfo, () => linkedOuterPackLine.JL_LinePrice.Round(0), () => new[] { linkedOuterPackLine.JL_LinePriceInfo }));

				Synchronisers.Add(new FieldSynchroniser(Destination.BY_GrossWeightInfo, Source.JL_ActualWeightInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_GrossWeightUnitInfo, Source.JL_ActualWeightUQInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_PieceCountInfo, Source.JL_PackageCountInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_ManifestUnitCodeInfo, GetConvertedManifestUnitCode, GetManifestUnitCodeInfo));

				Synchronisers.Add(GetGoodsDescriptionSynchroniser());
				Synchronisers.Add(GetMarksAndNumbersSynchroniser());

				Synchronisers.Add(new FieldSynchroniser(Destination.BY_RN_NKCountryOfOriginInfo, linkedOuterPackLine.JL_RN_NKOriginInfo));
			}
		}

		protected override IZType GetConvertedManifestUnitCode()
		{
			var result = ZString.Empty;
			var packType = Source.JL_F3_NKPackType;
			if (!packType.IsEmpty)
			{
				var refPacks = CusRefPacksHelper.LoadFilteredRefPacks(Destination.Factory, Core.Constants.CountryCodes.UnitedStates, RPTypeList.Codes.AMSManifest, packType);
				if (refPacks.Count > 0)
				{
					var manifestUnitList = Destination.Lookups.ManifestUnitList;
					result = refPacks
						.Select(y => y.RP_CustomsPack.Left(3))
						.FirstOrDefault(x => manifestUnitList.ContainsCode(x));
				}
				if (result.IsEmpty)
				{
					result = new PackageTypeMapping().GetPackageType(packType);
				}
			}
			return result;
		}

		FieldSynchroniser GetGoodsDescriptionSynchroniser()
		{
			var synchroniser = new FieldSynchroniser(Destination.BY_DescriptionInfo, GetGoodsDescription, GetInfosAffectingGoodsDescription);
			synchroniser.Format += base.DescriptionSynchroniser_Format;
			return synchroniser;
		}

		IZType GetGoodsDescription()
		{
			if (Source.IsOuterPackType)
			{
				return Source.JL_Description;
			}
			return Source.JL_Description.IsEmpty ? linkedOuterPackLine.JL_Description : Source.JL_Description;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingGoodsDescription()
		{
			yield return Source.JL_DescriptionInfo;
		}

		FieldSynchroniser GetMarksAndNumbersSynchroniser()
		{
			var synchroniser = new FieldSynchroniser(Destination.BY_MarksAndNumbersInfo, GetMarksAndNumbers, GetInfosAffectingMarksAndNumbers);
			synchroniser.Format += MarksAndNumbersSynchroniser_Format;
			return synchroniser;
		}

		protected override IZType GetMarksAndNumbers()
		{
			var result = linkedOuterPackLine.JL_MarksAndNumbers;
			if (result.IsEmpty)
			{
				var shipmentSource = ShipmentSource;
				if (shipmentSource != null)
				{
					result = shipmentSource.JS_MarksAndNumbers;
				}
			}
			return result;
		}

		protected override IEnumerable<ZPropertyInfo> GetInfosAffectingMarksAndNumbers()
		{
			yield return linkedOuterPackLine.JL_MarksAndNumbersInfo;
			var shipmentSource = ShipmentSource;
			if (shipmentSource != null)
			{
				yield return shipmentSource.JS_MarksAndNumbersInfo;
			}
		}
	}
}

