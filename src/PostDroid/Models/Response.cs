using System.Collections.ObjectModel;

namespace SuperPostDroidPunk.Models
{
    public class Response : AddDateTime
    {
        private string name;
        private string notes;

        public Response()
        {
            Headers = new ObservableCollection<Param>();
            Params = new ObservableCollection<Param>();
            FormData = new ObservableCollection<Param>();
            FormUrlEncoded = new ObservableCollection<Param>();
        }

        public int Id { get; set; }

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    IsModified = true;
                }
            }
        }

        public string? HttpMethod { get; set; }

        public string Url { get; set; }

        public Core.AuthorizationType AuthorizationType { get; set; }

        public string AuthUserName { get; set; }

        public string AuthPassword { get; set; }

        public string AuthBearerToken { get; set; }

        public ObservableCollection<Param> Headers { get; set; }

        public ObservableCollection<Param> Params { get; set; }

        public Core.BodyType RequestBodyType { get; set; }

        public ObservableCollection<Param> FormData { get; set; }

        public ObservableCollection<Param> FormUrlEncoded { get; set; }

        public string RequestRawBody { get; set; }

        public string Raw { get; set; }

        public string Json { get; set; }

        public string Xml { get; set; }

        public string Notes
        {
            get => notes;
            set
            {
                if (notes != value)
                {
                    notes = value;
                    IsModified = true;
                }
            }
        }

        public bool IsSelected { get; set; }

        public bool IsModified { get; set; }
    }
}
