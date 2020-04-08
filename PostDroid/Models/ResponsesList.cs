using System.Collections.ObjectModel;

namespace SuperPostDroidPunk.Models
{
    public class ResponsesList : AddDateTime
    {
        public ResponsesList()
        {
            Responses = new ObservableCollection<Response>();
            SubList = new ObservableCollection<ResponsesList>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ObservableCollection<Response> Responses { get; set; }

        public virtual ObservableCollection<ResponsesList> SubList { get; set; }

        public string Notes { get; set; }
    }
}
