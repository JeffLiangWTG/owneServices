using System;
using System.Drawing;
using System.Runtime.Caching;
using CargoWise.Common.Cache;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI.RateChooser
{
	public static class RateChooserImageRepository
	{
		public static Image ErrorIcon =>
			MemoryCache.Default.GetOrAdd
			(
				"RateChooserImageRepository.ErrorIcon",
				() => Icons.GetIcon(IconTypes.Error).ToImage(),
				new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMinutes(10) }
			);

		public static Image WarningIcon =>
			MemoryCache.Default.GetOrAdd
			(
				"RateChooserImageRepository.WarningIcon",
				() => Icons.GetIcon(IconTypes.Warning).ToImage(),
				new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMinutes(10) }
			);
	}
}
