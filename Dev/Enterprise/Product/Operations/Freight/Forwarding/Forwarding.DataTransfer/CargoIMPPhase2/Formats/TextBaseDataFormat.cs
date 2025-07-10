using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	abstract class TextBaseDataFormat : DataFormat<ZString>
	{
		public TextBaseDataFormat(int min, int max, string validCharacters)
		{
			this.min = min;
			this.max = max;
			this.validCharacters = validCharacters;
		}

		protected readonly int min;
		protected readonly int max;
		protected readonly string validCharacters;

		public virtual ZString Preprocess(ZString data)
		{
			return data;
		}

		public abstract string TypeErrorDescription { get; }

		public override ZString Format(ZString data, FormattingResult formattingResult)
		{
			data = Preprocess(data);

			if (!data.IsEmpty && !Regex.IsMatch(data, "^[" + validCharacters + "]{" + min + "," + max + "}$"))
			{
				if (data.Length > max)
				{
					data = data.Left(max);
				}

				data = Regex.Replace(data, "[^" + validCharacters + "]", "");
				if (formattingResult != null)
				{
					formattingResult.IsFormattedCorrectly = false;
					if (min != max)
					{
						formattingResult.ErrorMessage = Res.GetString("64155cb9-e594-4ec8-91b3-690020e76f55", "Input data must consist of '{0}' with length from {1} to {2}", TypeErrorDescription, min, max);
					}
					else
					{
						formattingResult.ErrorMessage = Res.GetString("e6d45845-dcbe-46e4-a31e-2704005a0551", "Input data must consist of {1} '{0}'", TypeErrorDescription, min);
					}
				}
			}

			return data;
		}
	}
}
