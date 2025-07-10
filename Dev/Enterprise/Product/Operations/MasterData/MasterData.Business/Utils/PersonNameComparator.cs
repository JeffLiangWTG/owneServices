namespace Enterprise.MasterData.Business
{
	public static class PersonNameComparator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Dummy Contact FullName")]
		public static bool IsDummyContact(string personName)
		{
			if (string.IsNullOrEmpty(personName))
			{
				return false;
			}

			var name = personName.Trim().ToUpper();
			return name == "DUMMY CONTACT TO SUPPRESS DOCS";
		}
	}
}
