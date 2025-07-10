using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public interface IViewStmNumsCollection<out T> : IActiveBusinessObjectCollection<T> where T : ViewStmNums
	{
		new T this[int index] { get; }
		void RefreshFromDb();
		T TryGetStmNums(string fountainCode, string prefix);
		INumberFountainProxy TryGetNumberFountain(string numberFountainCode, string prefix);
	}

	public abstract class ViewStmNumsCollection<T> : ActiveBusinessObjectCollection<T>, IViewStmNumsCollection<T> where T : ViewStmNums
	{
		protected ViewStmNumsCollection(BusinessObject owner, string namePrefix)
			: base(owner.Factory, owner, new ZQuery(ViewStmNumsSchema.SN_Name, SQLComparisonOperator.StartsWith, namePrefix), ViewStmNumsSchema.SN_Owner)
		{
			Owner = owner;
		}
		protected readonly BusinessObject Owner;

		protected override bool AllowNew => false;

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.SN_Owner = Owner.PK;
		}

		public T TryGetStmNums(string fountainCode, string prefix)
		{
			return this.FirstOrDefault(viewStmNum => viewStmNum.SN_Type == fountainCode && prefix == viewStmNum.SN_Prefix);
		}

		public INumberFountainProxy TryGetNumberFountain(string numberFountainCode, string prefix)
		{
			var matchedStmNum = TryGetStmNums(numberFountainCode, prefix);
			return matchedStmNum?.TryGetNumberFountain();
		}
	}
}
