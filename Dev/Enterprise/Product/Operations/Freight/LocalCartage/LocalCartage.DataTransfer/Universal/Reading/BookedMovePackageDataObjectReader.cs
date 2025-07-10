using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	public class BookedMovePackageDataObjectReader : DataObjectReader<PackingLine, CommonBookedCtgMove>
	{
		public BookedMovePackageDataObjectReader(PackingLine packageDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CommonCartage cartage, Dictionary<ZGuid, ZInt> linksDictionary)
			: base(packageDataObject, logger, factory)
		{
			this.linksDictionary = linksDictionary;
			this.cartage = cartage;
		}

		readonly Dictionary<ZGuid, ZInt> linksDictionary;
		readonly CommonCartage cartage;

		protected override CommonBookedCtgMove GetExistingBusinessObject()
		{
			return null;
		}

		protected override CommonBookedCtgMove GetNewBusinessObject()
		{
			var move = cartage.LooseBookedMoves.AddNew();
			move.EW_DisplayOrder = Convert.ToInt16(cartage.BookedMovesCollection.Count);
			move.DefaultAddresses();
			return move;
		}

		protected override void PopulateBusinessObject(CommonBookedCtgMove targetBO)
		{
			SetValue(targetBO, JobBookedCtgMoveSchema.EW_DimUnit, dataObject.LengthUnit);
			SetValue(targetBO, JobBookedCtgMoveSchema.EW_BookedLength, dataObject.Length);
			SetValue(targetBO, JobBookedCtgMoveSchema.EW_BookedWidth, dataObject.Width);
			SetValue(targetBO, JobBookedCtgMoveSchema.EW_BookedHeight, dataObject.Height);

			SetValue(targetBO, JobBookedCtgMoveSchema.EW_F3_NKPackType, dataObject.PackType);
			SetValue(targetBO, JobBookedCtgMoveSchema.EW_BookedPackCount, dataObject.PackQty);

			SetValue(targetBO, JobBookedCtgMoveSchema.EW_VolumeUQ, dataObject.VolumeUnit);
			SetValue(targetBO, JobBookedCtgMoveSchema.EW_BookedVolume, dataObject.Volume);
			SetValue(targetBO, JobBookedCtgMoveSchema.EW_WeightUQ, dataObject.WeightUnit);
			SetValue(targetBO, JobBookedCtgMoveSchema.EW_BookedWeight, dataObject.Weight);

			if (dataObject.Link.HasValue)
			{
				linksDictionary.Add(targetBO.PK, dataObject.Link.Value);
			}

			if (dataObject.UNDGCollection != null)
			{
				foreach (var undgDataObject in dataObject.UNDGCollection)
				{
					var undg = new UNDGDataObjectReader(undgDataObject, logger, factory).ReadIntoBusinessObject();
					targetBO.UNDGs.Add(undg);
				}
			}

			if (dataObject.AdditionalServiceCollection != null)
			{
				foreach (var additionalServiceDataObject in dataObject.AdditionalServiceCollection)
				{
					targetBO.Services.Add(new AdditionalServiceDataObjectReader(additionalServiceDataObject, logger, factory, targetBO).ReadIntoBusinessObject());
				}
			}
		}
	}
}
