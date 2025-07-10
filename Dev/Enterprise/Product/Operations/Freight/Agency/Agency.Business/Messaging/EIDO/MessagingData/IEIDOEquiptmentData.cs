using System;
using System.Collections.Generic;

namespace Enterprise.Freight.Agency.Business
{
	public interface IEIDOEquiptmentData
	{
		string ContainerNumber { get; }
		string ContainerISOCode { get; }
		List<string> SealNumbers { get; }

		string GoodsDescription { get; }

		string HandlingInstructions { get; }
		string IMDGClassCode { get; }
		string IMDGClass { get; }

		/// <summary>
		/// NAD CR - EmptyEquiptment Return Party
		/// </summary>
		IEIDOOrganisation EmptyReturn { get; }

		DateTime? EmptyReturnBy { get; }

		decimal GrossKilograms { get; }

		bool IsEmpty { get; }
	}
}
