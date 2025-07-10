using System;
using System.ComponentModel.DataAnnotations;

namespace Enterprise.Services.ServiceHost
{
	public class JobParentInfo
	{
		public Guid ParentId { get; set; }

		[Required(ErrorMessage = "ParentTableCode is required.")]
		[MinLength(2, ErrorMessage = "ParentTableCode must be at least 2 characters long.")]
		public string ParentTableCode { get; set; } = string.Empty;
	}
}
