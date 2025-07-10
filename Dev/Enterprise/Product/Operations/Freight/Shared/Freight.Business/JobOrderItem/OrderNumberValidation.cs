using System.Collections;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public static class OrderNumberValidation
	{
		public static void ErrorIfOrderNumberNotValid(ZPropertyInfo orderNumberProperty)
		{
			IList allowedChars = "!@#$%^&*()_+{}|:\"<>?-=[]\\;'/.~".ToCharArray();
			foreach (char character in orderNumberProperty.Value.ToString())
			{
				if (!char.IsLetterOrDigit(character) && !allowedChars.Contains(character))
				{
					orderNumberProperty.AddError(Res.GetString("7670bd90-c78f-4158-b356-bd437ac2cc7f", "Must enter alpha-numeric characters only"));
					break;
				}
			}
		}
	}
}
