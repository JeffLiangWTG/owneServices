using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Business
{
	public class MessageSendingObjectProperty
	{
		public MessageSendingObjectProperty(ZString propertyName, bool ismandatory = false, int columnWidth = 200, ResourceStringData resourceString = null, bool isVisible = true)
		{
			PropertyName = propertyName;
			IsMandatory = ismandatory;
			ColumnWidth = columnWidth;
			ResourceString = resourceString;
			IsVisible = isVisible;
		}

		public ZString PropertyName { get; }
		public ZBool IsMandatory { get; }
		public ZBool IsVisible { get; }
		public ZInt ColumnWidth { get; }
		public ResourceStringData ResourceString { get; }
	}
}
