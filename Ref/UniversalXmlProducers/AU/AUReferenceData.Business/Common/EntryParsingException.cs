using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class EntryParsingException : Exception
	{
		public EntryParsingException()
		{
		}

		public EntryParsingException(Exception ex) : base(ex.Message, ex)
		{
		}

		public EntryParsingException(string message) : base(message)
		{ }

		public EntryParsingException(string message, Exception ex) : base(message, ex)
		{ }
	}
}
