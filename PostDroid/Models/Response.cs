using System.Collections.Generic;

namespace SuperPostDroidPunk.Models
{
    public class Response : AddDateTime
    {
        public Response()
        {
            Headers = new List<Param>();
            Params = new List<Param>();
            FormData = new List<Param>();
            FormUrlEncoded = new List<Param>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string HttpMethod { get; set; }

        public string Url { get; set; }

        public Core.AuthorizationType AuthorizationType { get; set; }

        public string AuthUserName { get; set; }

        public string AuthPassword { get; set; }

        public string AuthBearerToken { get; set; }

        public List<Param> Headers { get; set; }

        public List<Param> Params { get; set; }

        public Core.BodyType BodyType { get; set; }

        public List<Param> FormData { get; set; }

        public List<Param> FormUrlEncoded { get; set; }

        public string RawBody { get; set; }

        public string Raw { get; set; }

        public string Json { get; set; }

        public string Xml { get; set; }

        public string Notes { get; set; }
    }
}
