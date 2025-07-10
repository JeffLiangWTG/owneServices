using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business
{
	public class CusInBondCargoDescSynchroniser : BusinessObjectSynchroniser
	{
		public CusInBondCargoDescSynchroniser(CusInBondCargoDesc destination, PackLine source)
			: base(destination, source)
		{
		}

		public new CusInBondCargoDesc Destination
		{
			get { return (CusInBondCargoDesc)base.Destination; }
		}

		public new PackLine Source
		{
			get { return (PackLine)base.Source; }
		}

		public CommonShipment ShipmentSource
		{
			get { return Source.Shipment; }
		}

		#region Implementation

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.BY_FormattedHarmonisedTariffInfo, Source.JL_HarmonisedCodeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BY_GrossWeightInfo, Source.JL_ActualWeightInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BY_GrossWeightUnitInfo, Source.JL_ActualWeightUQInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BY_RN_NKCountryOfOriginInfo, Source.JL_RN_NKOriginInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BY_MonetaryValueInfo, () => Source.JL_LinePrice.Round(0), () => new[] { Source.JL_LinePriceInfo }));

			var descriptionSynchroniser = new FieldSynchroniser(Destination.BY_DescriptionInfo, Source.JL_DescriptionInfo);
			descriptionSynchroniser.Format += DescriptionSynchroniser_Format;
			Synchronisers.Add(descriptionSynchroniser);

			var marksAndNumbersSynchroniser = new FieldSynchroniser(Destination.BY_MarksAndNumbersInfo, GetMarksAndNumbers, GetInfosAffectingMarksAndNumbers);
			marksAndNumbersSynchroniser.Format += MarksAndNumbersSynchroniser_Format;
			Synchronisers.Add(marksAndNumbersSynchroniser);

			Synchronisers.Add(new FieldSynchroniser(Destination.BY_PieceCountInfo, Source.JL_PackageCountInfo));

			Synchronisers.Add(new FieldSynchroniser(Destination.BY_ManifestUnitCodeInfo, GetConvertedManifestUnitCode, GetManifestUnitCodeInfo));
		}

		protected virtual IZType GetConvertedManifestUnitCode()
		{
			return new PackageTypeMapping().GetPackageType(Source.JL_F3_NKPackType);
		}

		protected IEnumerable<ZPropertyInfo> GetManifestUnitCodeInfo()
		{
			yield return Source.JL_F3_NKPackTypeInfo;
		}

		protected virtual IZType GetMarksAndNumbers()
		{
			var result = Source.JL_MarksAndNumbers;
			if (result.IsEmpty)
			{
				var shipmentSource = ShipmentSource;
				if (shipmentSource != null)
				{
					result = shipmentSource.JS_MarksAndNumbers;
				}
			}
			return result.Left(CusInBondCargoDesc.Schema.BY_MarksAndNumbersMaxLength);
		}

		protected virtual IEnumerable<ZPropertyInfo> GetInfosAffectingMarksAndNumbers()
		{
			yield return Source.JL_MarksAndNumbersInfo;
			var shipmentSource = ShipmentSource;
			if (shipmentSource != null)
			{
				yield return shipmentSource.JS_MarksAndNumbersInfo;
			}
		}

		protected void DescriptionSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(ZString))
			{
				e.Value = ((ZString)e.Value).Left(CusInBondCargoDesc.Schema.BY_DescriptionMaxLength);
			}
		}

		protected void MarksAndNumbersSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(ZString))
			{
				e.Value = ((ZString)e.Value).Left(CusInBondCargoDesc.Schema.BY_MarksAndNumbersMaxLength);
			}
		}

		#endregion
	}
}
