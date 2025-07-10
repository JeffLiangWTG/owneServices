namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	using Enterprise.Customs.Business.MultiLineAddInfos;

	class NZMAFFileCollection : CusAddInfoCollection<MAFFile>
	{
		public NZMAFFileCollection(MAFMessagingBO mafMessaging)
			: base(mafMessaging.PlugInSupport.Master)
		{
			this.mafMessaging = mafMessaging;
		}

		protected override void RemoveCollectionRelationshipsCore(CargoWise.EntityFramework.BusinessObject dependent, bool forDelete)
		{
			var mafFile = dependent as CusAddInfo<MAFFile>;
			if (mafFile != null)
			{
				mafFile.Data.MAFMessaging = null;
			}
			base.RemoveCollectionRelationshipsCore(dependent, forDelete);
		}

		protected override void SetCollectionRelationships(CargoWise.EntityFramework.BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var mafFile = dependent as CusAddInfo<MAFFile>;
			if (mafFile != null)
			{
				mafFile.Data.MAFMessaging = mafMessaging;
			}
		}

		readonly MAFMessagingBO mafMessaging;
	}
}
