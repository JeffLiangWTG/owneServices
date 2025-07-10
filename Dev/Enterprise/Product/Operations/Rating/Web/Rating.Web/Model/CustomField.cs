namespace Enterprise.Rating.Web.Model
{
	public class CustomField
	{
		public CustomField() { }

		public CustomField(string name, string value) : base()
		{
			Name = name;
			Value = value;
		}

		public string Name { get; set; }

		public string Value { get; set; }
	}
}
