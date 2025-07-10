using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	abstract class DataFormatTestCase<TDataFormat, T> : TestCase
		where T : struct
		where TDataFormat : DataFormat<T>
	{
		public void TestFormatting()
		{
			foreach (FormattingSample pair in GetSamples())
			{
				TDataFormat format = CreateDataFormat();
				FormattingResult formattingResult = new FormattingResult();
				AssertEquals(pair.ExpectedResult, format.Format(pair.Sample, formattingResult));
				AssertEquals(pair.ExpectedMessage.IsEmpty, formattingResult.IsFormattedCorrectly);
				if (pair.ExpectedMessage.IsEmpty)
				{
					AssertNull("Error Message", formattingResult.ErrorMessage);
				}
				else
				{
					AssertEquals(pair.ExpectedMessage, formattingResult.ErrorMessage);
				}
			}
		}

		protected abstract FormattingSample[] GetSamples();
		protected abstract TDataFormat CreateDataFormat();

		protected class FormattingSample
		{
			public FormattingSample(T sample, ZString expectedResult)
				: this(sample, expectedResult, ZString.Empty)
			{
			}

			public FormattingSample(T sample, ZString expectedResult, ZString expectedMessage)
			{
				Sample = sample;
				ExpectedResult = expectedResult;
				ExpectedMessage = expectedMessage;
			}

			public T Sample { get; set; }
			public ZString ExpectedResult { get; set; }
			public ZString ExpectedMessage { get; set; }
		}
	}
}
