using System;

namespace CargoWise.eHub.Products.CACustoms.DocImg.BT.Helpers
{
	[Serializable]
	public class HttpResponse
	{
		public HttpResponse(string code, string description, bool shouldRetry = false)
		{
			Code = code;
			Description = description;
            _shouldRetry = shouldRetry;

        }
		public string Code { get; private set; }
		public string Description { get; private set; }

        private bool _shouldRetry ;
		public bool ShouldRetry
        {
            get
            {
                switch (Code)
                {
                    case "401":
                    case "404":
                    case "408":
                    case "423":
                    case "429":
                    case "451":
                        return true;
                    default:
                        if (Code.Length == 3 && Code.StartsWith("5"))
                            return true;
                        break;
                }
                return _shouldRetry;
            }
        }
    }
}
