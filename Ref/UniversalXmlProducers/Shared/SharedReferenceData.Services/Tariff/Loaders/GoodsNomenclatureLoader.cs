using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public class GoodsNomenclatureLoader : XmlElementReader<GoodsNomenclature>, IProcessorLoader
	{
		protected override string ParentElement => "findGoodsNomenclatureByDatesResponse";
		protected override string ElementName => "GoodsNomenclature";

		protected override bool IsValidElement(XElement element)
		{
			return element.Element("metainfo")?.Element("opType")?.Value == MetaInfoOpTypes.Updated ||
				(element.Elements("goodsNomenclatureDescriptionPeriod").Any() && element.Elements("goodsNomenclatureIndents").Any());
		}

		protected override bool ProcessElementCore(GoodsNomenclature model, XElement element)
		{
			if (model.OpType == MetaInfoOpTypes.Updated)
			{
				return UpdateModel(model, element);
			}
			return base.ProcessElementCore(model, element);
		}

		bool UpdateModel(GoodsNomenclature model, XElement element)
		{
			var hasData = false;

			if (LastModelDoesExist(model.HJID, out var lastModel))
			{
				model.ProcessUpdate(element);
				if (ValidateLastModel(lastModel, model))
				{
					hasData = true;
					lastModel.OpDate = model.OpDate;
					if (element.Element("validityEndDate") != null)
					{
						lastModel.EndDate = model.EndDate;
					}
					if (element.Element("validityStartDate") != null)
					{
						lastModel.StartDate = model.StartDate;
					}
					lastModel.UpdateDescriptionAndIndents(element);

					if (lastModel.EndDate != null)
					{
						var successorElements = element.Elements("goodsNomenclatureSuccessor");
						foreach (var successor in successorElements)
						{
							ProcessSuccessor(successor, lastModel, model);
						}
					}
				}
			}

			return hasData;
		}

		bool LastModelDoesExist(string hjid, out GoodsNomenclature lastModel)
		{
			var valid = Models.TryGetValue(hjid, out var last);
			lastModel = last as GoodsNomenclature;
			return valid && lastModel != null;
		}

		bool ValidateLastModel(GoodsNomenclature lastModel, GoodsNomenclature model)
		{
			var valid = true;
			if (lastModel.OpDate >= model.OpDate)
			{
				valid = false;
			}
			else if (lastModel.OpType != MetaInfoOpTypes.Created)
			{
				var msg = Invariant($"{Source}:Update for model {model.HJID} ({model.ItemId} {model.ProductLineSuffix}) found OpType {lastModel.OpType}, OpDate={lastModel.OpDate}");
				ErrorCollector.AppendLine(msg);
				valid = false;
			}
			else if (lastModel.ItemId != model.ItemId || lastModel.ProductLineSuffix != model.ProductLineSuffix)
			{
				var msg = Invariant($"{Source}:Update for model {model.HJID} ({model.ItemId} {model.ProductLineSuffix}) does not match existing {lastModel.HJID} ({lastModel.ItemId} {model.ProductLineSuffix}) - this update has been ignored");
				ErrorCollector.AppendLine(msg);
				valid = false;
			}

			return valid;
		}

		void ProcessSuccessor(XElement successor, GoodsNomenclature model, GoodsNomenclature lastModel)
		{
			var hjid = successor.Element("hjid")?.Value;
			var absorbedGoodsNomenclatureItemId = successor.Element("absorbedGoodsNomenclatureItemId")?.Value;
			var absorbedProductlineSuffix = successor.Element("absorbedProductlineSuffix")?.Value;
			var opType = successor.Element("metainfo")?.Element("opType")?.Value ?? "";

			if (opType == MetaInfoOpTypes.Created)
			{
				var existing = FindLatestGoodsNomenclature(absorbedGoodsNomenclatureItemId, absorbedProductlineSuffix);
				if (existing != null)
				{
					if (existing.EndDate == null && existing.StartDate < lastModel.CalcEndDate)
					{
						existing.EndDate = model.EndDate;
						var newModel = new GoodsNomenclature
						{
							HJID = hjid,
							ItemId = absorbedGoodsNomenclatureItemId,
							ProductLineSuffix = absorbedProductlineSuffix,
							StartDate = lastModel.CalcEndDate.AddMinutes(1),
							Description = existing.Description,
							Indent = existing.Indent
						};
						LoadMetaInfo(newModel, successor);
						Models[hjid] = newModel;
					}
				}
			}
		}

		GoodsNomenclature FindLatestGoodsNomenclature(string itemId, string productlineSuffix)
		{
			GoodsNomenclature found = null;
			foreach (var model in Models.Values)
			{
				if (model.ItemId == itemId && model.ProductLineSuffix == productlineSuffix)
				{
					found = found == null ? model : (GoodsNomenclature)model.GetLatest(found);
				}
			}
			return found;
		}

		protected override GoodsNomenclature CreateMinimumModelFromXElement(XElement element)
		{
			return new GoodsNomenclature
			{
				ItemId = element.Element("goodsNomenclatureItemId").Value,
			};
		}
	}
}
