using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using LiteDB;
using ReactiveUI;
using SuperPostDroidPunk.Core;
using SuperPostDroidPunk.Models;
using System.Collections.ObjectModel;

namespace SuperPostDroidPunk.ViewModels
{
    public class CollectionNodeViewModel : ViewModelBase
    {
        private int id;
        private string name;
        private int? parentId;
        private string method;
        private string url;
        private Path icon;
        private string notes;
        private bool isSelected;
        private bool isExpanded;
        private bool isFolder;
        private bool isModified;
        private ObservableCollection<CollectionNodeViewModel> children;

        public CollectionNodeViewModel()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Icon = new Path();
            });

            Children = new ObservableCollection<CollectionNodeViewModel>();
        }

        public int Id
        {
            get => id;
            set
            {
                this.RaiseAndSetIfChanged(ref id, value);
                if (id > 0)
                {
                    LoadChildNodes(id);
                }
            }
        }

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    this.RaiseAndSetIfChanged(ref name, value);
                    IsModified = true;
                }
            }
        }

        public int? ParentId { get => parentId; set => this.RaiseAndSetIfChanged(ref parentId, value); }

        public string Method { get => method; set => this.RaiseAndSetIfChanged(ref method, value); }

        public string Url { get => url; set => url = this.RaiseAndSetIfChanged(ref url, value); }

        public Path Icon { get => icon; set => this.RaiseAndSetIfChanged(ref icon, value); }

        public string Notes
        {
            get => notes;
            set
            {
                if (notes != value)
                {
                    this.RaiseAndSetIfChanged(ref notes, value);
                    IsModified = true;
                }
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set => this.RaiseAndSetIfChanged(ref isSelected, value);
        }

        public bool IsExpanded { get => isExpanded; set => this.RaiseAndSetIfChanged(ref isExpanded, value); }

        public bool IsFolder
        {
            get => isFolder;
            set
            {
                this.RaiseAndSetIfChanged(ref isFolder, value);
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (isFolder)
                    {
                        Icon.Data = Geometry.Parse("M22,4H14L12,2H6A2,2 0 0,0 4,4V16A2,2 0 0,0 6,18H22A2,2 0 0,0 24,16V6A2,2 0 0,0 22,4M2,6H0V11H0V20A2,2 0 0,0 2,22H20V20H2V6Z");
                        Icon.Fill = new SolidColorBrush(Color.Parse("#FFFF00"));
                    }
                    else
                    {
                        Icon.Data = Geometry.Parse("M13,9H18.5L13,3.5V9M6,2H14L20,8V20A2,2 0 0,1 18,22H6C4.89,22 4,21.1 4,20V4C4,2.89 4.89,2 6,2M15,18V16H6V18H15M18,14V12H6V14H18Z");
                        Icon.Fill = new SolidColorBrush(Color.Parse("#E5E5E5"));
                    }
                });
            }
        }

        public bool IsModified { get => isModified; set => this.RaiseAndSetIfChanged(ref isModified, value); }

        public bool ChildrenLoaded { get; set; }

        public ObservableCollection<CollectionNodeViewModel> Children { get => children; set => this.RaiseAndSetIfChanged(ref children, value); }

        public void LoadChildNodes(int id)
        {
            if (ChildrenLoaded) return;

            using var db = new LiteDatabase(DbConfig.ConnectionString);

            var newChildren = db.GetCollection<ResponsesList>(DbConfig.HistoryCollection).FindById(id);

            Children.Clear();
            foreach (var folderChild in newChildren.SubList)
            {
                Children.Add(new CollectionNodeViewModel
                {
                    Id = folderChild.Id,
                    Name = folderChild.Name,
                    Notes = folderChild.Notes,
                    IsFolder = true,
                    parentId = Id
                });
            }

            foreach (var ResponseChild in newChildren.Responses)
            {
                Children.Add(new CollectionNodeViewModel
                {
                    Id = ResponseChild.Id,
                    Name = ResponseChild.Name,
                    Method = ResponseChild.HttpMethod,
                    url = ResponseChild.Url,
                    Notes = ResponseChild.Notes,
                    IsFolder = false,
                    parentId = Id
                });
            }

            ChildrenLoaded = true;
        }
    }
}
