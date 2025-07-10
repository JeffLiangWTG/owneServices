namespace Enterprise.eTail.Business
{
	public static class HVLVBookingHeaderTestLoadlistCreator
	{
		public static HVLVOriginLoadList CreateLoadList(HVLVBookingHeader bookingHeader, bool shouldCreateOuterPackage)
		{
			var loadList = bookingHeader.Factory.New<HVLVOriginLoadList>();
			var items = bookingHeader.AllItems;
			foreach (var item in items)
			{
				item.HVI_HVL_LoadList = loadList.PK;
			}

			if (shouldCreateOuterPackage)
			{
				var outerPackage = bookingHeader.Factory.New<HVLVOuterPackage>();
				outerPackage.HVO_HVL_LoadList = loadList.PK;
				foreach (var item in items)
				{
					item.HVI_HVO_OuterPackage = outerPackage.PK;
				}
			}

			bookingHeader.Factory.Save();
			return loadList;
		}
	}
}
