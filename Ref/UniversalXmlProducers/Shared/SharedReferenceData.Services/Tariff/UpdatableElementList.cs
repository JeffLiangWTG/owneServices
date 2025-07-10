using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff
{
	public class UpdatableElementList<T> where T : ITariffModel
	{
		public IEnumerable<T> Values => models.Values.Where(m => m.OpType != MetaInfoOpTypes.Deleted);

		public bool ProcessUpdate(T model, XElement element, StringBuilder errorCollector = null, string source = null)
		{
			var result = false;
			if (!string.IsNullOrEmpty(model.HJID))
			{
				if (models.TryGetValue(model.HJID, out var existingModel))
				{
					if (existingModel.OpType != MetaInfoOpTypes.Deleted)
					{
						result = existingModel.ProcessUpdate(element);
					}
					else if (model.OpType != MetaInfoOpTypes.Deleted)
					{
						errorCollector?.Append(CultureInfo.InvariantCulture, $"Handling {model.OpType} for deleted element {model.GetType()} HJID {model.HJID} OpDate {model.OpDate} may be invalid");
						if (model.ProcessUpdate(element) && model.IsValidForCreate(errorCollector, source))
						{
							models[model.HJID] = model;
							result = true;
						}
					}
				}
				else
				{
					if (model.ProcessUpdate(element) && model.IsValidForCreate(errorCollector, source))
					{
						Add(model);
						result = true;
					}
				}
			}
			return result;
		}

		public void CopyFrom<TC>(UpdatableElementList<TC> other) where TC : T, ICopyable<T>
		{
			models.Clear();
			Add(other.models.Select(m => m.Value.Copy()));
		}

		public void Add(IEnumerable<T> values)
		{
			foreach (var value in values)
			{
				Add(value);
			}
		}

		void Add(T model)
		{
			models.Add(model.HJID, model);
		}

		internal void Clear() => models.Clear();
		internal T this[string key] { get => models[key]; set => models[key] = value; }
		internal bool TryGetValue(string key, out T value) => models.TryGetValue(key, out value);

		readonly Dictionary<string, T> models = new Dictionary<string, T>();
	}
}
