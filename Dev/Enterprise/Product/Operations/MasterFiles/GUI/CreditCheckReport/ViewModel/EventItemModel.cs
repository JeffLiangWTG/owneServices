using System.Drawing;
using System.Globalization;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class EventItemModel : ModelBase<EventItemModel>
	{
		public EventItemModel(CreditEvent model)
		{
			Model = model;
		}

		public CreditEvent Model { get; }

		public string EventDate => Model.EventDate.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture);

		public string EventDescription => ResourceStringHelper.GetCreditEventDescription(Model.Type);

		public Image EventIcon => ImageBitmapHelper.GetCreditEventIconInfo(Model.Type).Icon;
	}
}
