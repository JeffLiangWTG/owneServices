using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public abstract class RatingHeaderProcessTaskCollection<T, P> : ProcessTaskCollection
		where T : RatingHeader
		where P : RatingHeaderProcessTask<T>
	{
		protected RatingHeaderProcessTaskCollection(T ratingHeader)
			: base(ratingHeader)
		{
		}

		public new P this[int index]
		{
			get { return (P)Elements[index]; }
		}

		public new P AddNew()
		{
			return (P)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return GetNewCollectionCore(Parent);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, RatingHeaderSchema.Constants.Prefix);
			return result;
		}

		protected abstract ProcessTaskCollection GetNewCollectionCore(T parent);

		#region Implementation

		new T Parent
		{
			get { return (T)base.Parent; }
		}

		#endregion
	}
}

