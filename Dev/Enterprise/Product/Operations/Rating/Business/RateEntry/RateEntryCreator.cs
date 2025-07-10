using System;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateEntryCreator : IBusinessObjectCreationSource
	{
		RateEntryCreator(string sourceCode)
		{
			CreationSourceCode = sourceCode;
		}

		public string CreationSourceCode { get; }

		/// <summary>
		/// Pushes this IBusinessObjectCreationSource as the top-most
		/// creator source in the IBusinessObjectCreationSourceStack singleton
		/// </summary>
		/// <returns>
		/// A disposable that must be disposed once this creation source
		/// goes out of scope
		/// </returns>
		public IDisposable Push()
		{
			var stack = ObjectFactory.Get<IBusinessObjectCreationSourceStack>();
			return stack.PushCreationSource(this);
		}

		#region Factory methods

		public static RateEntryCreator CreateFromQuotation() => new RateEntryCreator(Sources.FromQuotation);
		//note: CreateFromNativeXML is implemented with a magic string in the RateInterceptor.cs
		//note: CreateFromTACTImport is implemented in TACTImporter.cs by directly accessing Sources.FromTACT
		//note: CreateFromADAWImport is implemented in GlowCollectionImporter.cs by directly using "ADW"

		#endregion

		#region Constants

		public static class Sources
		{
			public const string FromQuotation = "QUO";

			public const string FromTACT = "TCT";

			public const string FromNativeXML = "XML";

			public const string FromADAW = "ADW";

			public const string Manual = "";
		}

		#endregion
	}
}
