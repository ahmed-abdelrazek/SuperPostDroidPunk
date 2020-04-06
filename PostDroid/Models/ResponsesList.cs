using System.Collections.Generic;

namespace SuperPostDroidPunk.Models
{
    public class ResponsesList : AddDateTime
    {
        public ResponsesList()
        {
            SubList = new List<ResponsesList>();
            Responses = new List<Response>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public virtual List<ResponsesList> SubList { get; set; }

        public virtual List<Response> Responses { get; set; }

        public string Notes { get; set; }
    }
}
