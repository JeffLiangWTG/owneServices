using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public static class DefaultRatingXmlSerializer
	{
		public static string Serialize(object objectToSerialize)
		{
			var businessObject = objectToSerialize as IBusiness;

			return businessObject != null
				? (string)businessObject.HumanReadableName
				: objectToSerialize.ToString();
		}
	}
}
