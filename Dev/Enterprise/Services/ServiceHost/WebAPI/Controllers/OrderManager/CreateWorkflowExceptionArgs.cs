using System;
using System.Runtime.Serialization;

namespace Enterprise.Services.ServiceHost
{
	[DataContract]
	public class CreateWorkflowExceptionArgs
	{
		[DataMember(Name = "parentPK")]
		public Guid ParentPK { get; set; }

		[DataMember(Name = "parentTableCode")]
		public string ParentTableCode { get; set; }

		[DataMember(Name = "exceptionTypePK")]
		public Guid ExceptionTypePK { get; set; }

		[DataMember(Name = "exceptionTimeUTC")]
		public string ExceptionTimeUTC { get; set; }

		[DataMember(Name = "exceptionPublished")]
		public bool ExceptionPublished { get; set; }

		[DataMember(Name = "exceptionCausePK")]
		public Guid ExceptionCausePK { get; set; }

		[DataMember(Name = "exceptionStaffPK")]
		public Guid ExceptionStaffPK { get; set; }

		[DataMember(Name = "exceptionStaffGroupPK")]
		public Guid ExceptionStaffGroupPK { get; set; }

		[DataMember(Name = "exceptionNotes")]
		public string ExceptionNotes { get; set; }

		[DataMember(Name = "exceptionDescription")]
		public string ExceptionDescription { get; set; }
	}
}
