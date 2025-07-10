using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.ZArchitecture.Business;
using WTG.Foundation.FrameworkExtensions;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers
{
	[GlowTicketAuthentication]
	public sealed class NotesController : ApiController
	{
		[Route("api/notetypes/custom/{moduleId}")]
		[HttpGet]
		public IHttpActionResult GetCustomNoteTypes(string moduleId, string languageCode = Core.SharedConstants.Languages.English)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var noteTypes = CustomNotesProvider.Instance.CustomNoteTypesForModuleAndAllCountries(moduleId);
				return Json(GetNoteTypesInCollection(noteTypes)
					.Select(noteType => ToNoteTypeInfo(noteType, languageCode)));
			}
		}

		[Route("api/notetypes/info")]
		[HttpPost]
		public IHttpActionResult GetNoteTypesInfo([FromBody] string[] noteTypeNames, string languageCode = Core.SharedConstants.Languages.English)
		{
			if (noteTypeNames == null || noteTypeNames.Length == 0)
			{
				return BadRequest();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var noteTypes = new List<PredefinedNoteType>();
				foreach (var name in noteTypeNames)
				{
					var noteType = PredefinedNoteTypes.Instance.NoteTypeByDescription(name, includeCustomNotesInSearch: false);
					if (noteType != null)
					{
						noteTypes.Add(noteType);
					}
				}

				return Json(noteTypes.Select(noteType => ToNoteTypeInfo(noteType, languageCode)));
			}
		}

		[Route("api/notetypes/exists")]
		[HttpGet]
		public IHttpActionResult NoteTypesExist(string noteTypeName)
		{
			if (noteTypeName.IsNullOrEmpty())
			{
				return BadRequest();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var noteType = PredefinedNoteTypes.Instance.NoteTypeByDescription(noteTypeName, includeCustomNotesInSearch: true);
				return Json(noteType != null);
			}
		}

		static NoteTypeInfo ToNoteTypeInfo(PredefinedNoteType noteType, string languageCode)
			=> new NoteTypeInfo
			{
				Name = noteType.Code,
				DefaultVisibility = noteType.DefaultVisibility.ToString(),
				Description = noteType.MultilingualDescription.ToString(languageCode),
				IsTextOnly = noteType.IsTextOnly,
			};

		static IEnumerable<PredefinedNoteType> GetNoteTypesInCollection(NoteTypeCollection noteTypes)
		{
			foreach (PredefinedNoteType item in noteTypes)
			{
				yield return item;
			}
		}

		sealed class NoteTypeInfo
		{
			public string Name { get; set; }
			public string Description { get; set; }
			public string DefaultVisibility { get; set; }
			public bool IsTextOnly { get; set; }
		}
	}
}
