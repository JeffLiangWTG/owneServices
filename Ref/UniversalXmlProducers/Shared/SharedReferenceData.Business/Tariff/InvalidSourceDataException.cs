using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff
{
	public class InvalidSourceDataException : Exception
	{
		public InvalidSourceDataException()
		{
		}

		public InvalidSourceDataException(string message) : base(message)
		{
		}

		public InvalidSourceDataException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public InvalidSourceDataException(string dataProvider, string team, IEnumerable<string> errors = null)
			: base($"One or more errors were identified with the source data, this may need to be raised with {dataProvider}. Please assign this issue to {team}.")
		{
			Errors = errors;
		}

		public IEnumerable<string> Errors { get; set; }

		public override string Message => base.Message + ErrorsWithNewLineBeforeEachEntry;

		string ErrorsWithNewLineBeforeEachEntry
		{
			get
			{
				var errors = Errors != null ? string.Join(Environment.NewLine, Errors) : null;
				return string.IsNullOrEmpty(errors) ? string.Empty : Environment.NewLine + errors;
			}
		}
	}
}
