using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	class UNDGDataItemSynchroniser : BaseSynchroniser
	{
		internal UNDGDataItemSynchroniser(UNDGDataItem destination, UNDGDataItem source, UNDGDataItemCollectionSynchroniser parentSynchroniserCollection)
		{
			this.destination = Argument.NotNull(destination, "UNDGDataItem destination");
			sources = new List<UNDGDataItem>(new[] { Argument.NotNull(source, "UNDGDataItem source") });
			infos = new Dictionary<ZPropertyInfo, IZType>();
			this.parentSynchroniserCollection = Argument.NotNull(parentSynchroniserCollection, "UNDGDataItemCollectionSynchroniser parentSynchroniserCollection");
		}

		internal bool IsEmpty
		{
			get { return destination.IsDeleted || !Sources.Any(); }
		}

		internal bool ContainsSource(UNDGDataItem source)
		{
			return Sources.Contains(Argument.NotNull(source, "UNDGDataItem source"));
		}

		internal void AddSource(UNDGDataItem source)
		{
			if (!ContainsSource(source))
			{
				sources.Add(source);
				HookEvents(source);
			}
		}

		internal void RemoveSource(UNDGDataItem source)
		{
			if (source != null && sources.Contains(Argument.NotNull(source, "UNDGDataItem source")))
			{
				sources.Remove(source);
				UnHookEvents(source);
				if (sources.Count == 0)
				{
					ForceSynchronise();
				}
			}
		}

		void HookEvents(UNDGDataItem source)
		{
			if (!source.IsDeleted)
			{
				HookInfo(source.DI_DGInfo);
				HookInfo(source.DI_OC_DGContactInfo);
				HookInfo(source.DI_DGFlashPointInfo);
				HookInfo(source.DI_TechnicalNameInfo);
			}
		}

		void UnHookEvents(UNDGDataItem source)
		{
			UnHookInfo(source.DI_DGInfo);
			UnHookInfo(source.DI_OC_DGContactInfo);
			UnHookInfo(source.DI_DGFlashPointInfo);
			UnHookInfo(source.DI_TechnicalNameInfo);
		}

		void HookInfo(ZPropertyInfo info)
		{
			UnHookInfo(info);
			info.ValueChanged += Info_ValueChanged;
			infos.Add(info, info.Value);
		}

		void UnHookInfo(ZPropertyInfo info)
		{
			info.ValueChanged -= Info_ValueChanged;
			if (infos.ContainsKey(info))
			{
				infos.Remove(info);
			}
		}

		void Info_ValueChanged(object sender, System.EventArgs e)
		{
			if (e is InfoEventArgs ve)
			{
				var wrapped = ve.Info as ZWrappedPropertyInfo;
				var info = (wrapped == null) ? ve.Info : wrapped.InnerInfo;
				var undg = info.BizObj as UNDGDataItem;
				if (infos.TryGetValue(info, out IZType lastValue) && !ve.Info.Value.Equals(lastValue))
				{
					var synchroniser = parentSynchroniserCollection.GetOtherSynchroniser(this, undg);
					if (synchroniser != null)
					{
						RemoveSource(undg);
						synchroniser.AddSource(undg);
					}
					else
					{
						var countTop2 = Sources.Take(2).Count();
						if (countTop2 == 1)
						{
							destination[info.Name] = info.Value;
							infos[info] = info.Value;
						}
						else if (countTop2 > 0)
						{
							RemoveSource(undg);
							synchroniser = parentSynchroniserCollection.AddUNDGSynchroniserIfNotExists(undg);
							synchroniser.Synchronise();
						}
					}
					if (!Sources.Any())
					{
						parentSynchroniserCollection.RemoveSynchroniser(this);
					}
				}
			}
		}

		protected override void ForceSynchronise()
		{
			if (IsEnabled)
			{
				var subsPK = ZGuid.Empty;
				var flashPoints = ZDecimal.Zero;
				var contact = ZGuid.Empty;
				var technicalName = ZString.Empty;
				if (Sources.Any())
				{
					var source = Sources.First();
					subsPK = source.DI_DG;
					flashPoints = source.DI_DGFlashPoint;
					contact = source.DI_OC_DGContact;
					technicalName = source.DI_TechnicalName;
				}
				destination.DI_DG = subsPK;
				destination.DI_DGFlashPoint = flashPoints;
				destination.DI_OC_DGContact = contact;
				destination.DI_TechnicalName = technicalName;
			}
		}

		protected override void UnHookEvents()
		{
			foreach (var source in sources)
			{
				UnHookEvents(source);
			}
		}

		protected override void HookEvents()
		{
			foreach (var source in sources)
			{
				HookEvents(source);
			}
		}

		protected override void OnDetectEnabledChanged()
		{
		}

		readonly Dictionary<ZPropertyInfo, IZType> infos;
		readonly UNDGDataItemCollectionSynchroniser parentSynchroniserCollection;
		internal readonly UNDGDataItem destination;
		internal IEnumerable<UNDGDataItem> Sources
		{
			get
			{
				foreach (var source in sources)
				{
					if (!source.IsDeleted)
					{
						yield return source;
					}
				}
			}
		}
		readonly List<UNDGDataItem> sources;
	}
}
