using System;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	abstract class BaseTWMessageBuilderTest<DataSource, SerializableClassType> : MessageBuilderTest
	{
		[ExpectNoExceptions]
		public virtual void TestSerializeToMessageString()
		{
			var expectedString = TWMessageBuilder.SerializeToMessageString(Declaration, FunctionCode);
			if (!FunctionCode.IsEmpty)
			{
				AssertXMLContains(ZString.Format("<FunctionCode>{0}</FunctionCode>", FunctionCode), expectedString);
			}
			AsserContainsXmlValueByType(expectedString, Declaration, typeof(DataSource));
		}

		protected virtual ZString FunctionCode => "9";

		protected abstract ITWMessageBuilder CreateNewMessageBuilder();

		protected abstract DataSource CreateDataSource();

		protected Type DeclarationType => typeof(SerializableClassType);

		protected ITWMessageBuilder TWMessageBuilder
		{
			get
			{
				if (fITWMessageBuilder == null)
				{
					fITWMessageBuilder = CreateNewMessageBuilder();
				}
				return fITWMessageBuilder;
			}
		}

		ITWMessageBuilder fITWMessageBuilder;

		protected DataSource Declaration
		{
			get
			{
				if (fDataSource == null)
				{
					fDataSource = CreateDataSource();
				}
				return fDataSource;
			}
		}

		DataSource fDataSource;
	}
}
