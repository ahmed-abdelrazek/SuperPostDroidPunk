using System.Collections.Generic;

namespace SuperPostDroidPunk.Models
{
    public class ResponsesList : AddDateTime
    {
        public ResponsesList()
        {
            Responses = new List<Response>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public List<Response> Responses { get; set; }

        public string Notes { get; set; }
    }
}
