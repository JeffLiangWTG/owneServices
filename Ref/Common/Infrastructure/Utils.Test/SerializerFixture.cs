using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class SerializerFixture
	{
		[Test]
		public void Serialize()
		{
			var keyData = new KeyData { Name = "Name1", Value = "Value1" };
			var serializeResults = Serializer.Serialize(keyData);
			Assert.NotNull(serializeResults);
			Assert.AreEqual(33, serializeResults.Length);
		}

		[Test]
		public void Deserialize()
		{
			var keyData = new KeyData { Name = "Name1", Value = "Value1" };
			var byteArray = Serializer.Serialize(keyData);
			var obj = Serializer.Deserialize<KeyData>(byteArray);
			Assert.NotNull(obj);
			Assert.AreEqual("Name1", obj.Name);
			Assert.AreEqual("Value1", obj.Value);
		}
	}
}
