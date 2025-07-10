
namespace Enterprise.DataTransfer.Xml
{
	public interface IValueObject
	{
		bool IsSpecified { get; }
		bool ShouldCreateElementForEmptyValue { get; set; }
	}
}
