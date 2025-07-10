namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class TestValueObjectCollection : ValueObjectCollection
	{
		public void Add(TestValueObject item)
		{
			List.Add(item);
		}
	}
}
