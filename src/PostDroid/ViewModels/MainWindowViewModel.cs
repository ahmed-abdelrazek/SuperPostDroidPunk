using Avalonia.Controls.Notifications;
using DynamicData;
using Flurl;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using SuperPostDroidPunk.Core;
using SuperPostDroidPunk.Extensions;
using SuperPostDroidPunk.Helpers;
using SuperPostDroidPunk.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SuperPostDroidPunk.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool isSavingHistory;
        private bool isDeletingHistory;
        private bool isSending;
        private bool isErrorInSaveHistoryExist;
        private bool isErrorInDeleteHistoryExist;
        private bool isErrorInRequestExist;
        private bool isErrorInResponseExist;

        private string _url;
        private bool _isSaveInHistory;
        private string _selectedMethod;
        private string _responseBodyJson;
        private string _responseBodyXml;
        private string _responseBodyRaw;
        private AuthorizationType _selectedAuthType;
        private string _authBearer;
        private string _authUsername;
        private string _authPassword;
        private bool _isAuthBasic;
        private bool _isAuthBearer;
        private string _requestBody;
        private ObservableCollection<Param> _headers;
        private ObservableCollection<Param> _params;
        private ObservableCollection<AuthorizationType> _authorizationTypes;
        private ObservableCollection<Response> _history;
        private ObservableCollection<ResponsesList> _historyCollection;
        private Response _selectedHistory;
        private IManagedNotificationManager _notificationManager;
        private ObservableCollection<CollectionNodeViewModel> _tree;

        public ObservableCollection<string> HttpMethods { get; set; }

        public string SelectedMethod { get => _selectedMethod; set => this.RaiseAndSetIfChanged(ref _selectedMethod, value); }

        [Required(AllowEmptyStrings = false)]
        [RegularExpression(@"^http(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$", ErrorMessage = "Please enter a valid URL.")]
        public string Url { get => _url; set => this.RaiseAndSetIfChanged(ref _url, value); }

        public bool IsSaveInHistory { get => _isSaveInHistory; set => this.RaiseAndSetIfChanged(ref _isSaveInHistory, value); }

        public ObservableCollection<Param> Headers { get => _headers; set => this.RaiseAndSetIfChanged(ref _headers, value); }

        public ObservableCollection<Param> Params { get => _params; set => this.RaiseAndSetIfChanged(ref _params, value); }

        public ObservableCollection<AuthorizationType> AuthorizationTypes { get => _authorizationTypes; set => this.RaiseAndSetIfChanged(ref _authorizationTypes, value); }

        public AuthorizationType SelectedAuthType
        {
            get => _selectedAuthType;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedAuthType, value);
                switch (_selectedAuthType)
                {
                    case AuthorizationType.Basic:
                        {
                            IsAuthBasic = true;
                            IsAuthBearer = false;
                            break;
                        }
                    case AuthorizationType.Bearer:
                        {
                            IsAuthBasic = false;
                            IsAuthBearer = true;
                            break;
                        }
                    default:
                        {
                            IsAuthBasic = false;
                            IsAuthBearer = false;
                            break;
                        }
                }
            }
        }

        public string AuthBearer { get => _authBearer; set => this.RaiseAndSetIfChanged(ref _authBearer, value); }

        public string AuthUsername { get => _authUsername; set => this.RaiseAndSetIfChanged(ref _authUsername, value); }

        public string AuthPassword { get => _authPassword; set => this.RaiseAndSetIfChanged(ref _authPassword, value); }

        public bool IsAuthBasic { get => _isAuthBasic; set => this.RaiseAndSetIfChanged(ref _isAuthBasic, value); }

        public bool IsAuthBearer { get => _isAuthBearer; set => this.RaiseAndSetIfChanged(ref _isAuthBearer, value); }

        public string RequestBody { get => _requestBody; set => this.RaiseAndSetIfChanged(ref _requestBody, value); }

        public string ResponseBodyJson { get => _responseBodyJson; set => this.RaiseAndSetIfChanged(ref _responseBodyJson, value); }

        public string ResponseBodyXml { get => _responseBodyXml; set => this.RaiseAndSetIfChanged(ref _responseBodyXml, value); }

        public string ResponseBodyRaw { get => _responseBodyRaw; set => this.RaiseAndSetIfChanged(ref _responseBodyRaw, value); }

        public ObservableCollection<Response> History { get => _history; set => this.RaiseAndSetIfChanged(ref _history, value); }

        public ObservableCollection<ResponsesList> HistoryCollection { get => _historyCollection; set => this.RaiseAndSetIfChanged(ref _historyCollection, value); }

        public ObservableCollection<CollectionNodeViewModel> Tree { get => _tree; set => this.RaiseAndSetIfChanged(ref _tree, value); }

        public Response SelectedHistory
        {
            get => _selectedHistory;
            set
            {
                if (_selectedHistory != value)
                {
                    this.RaiseAndSetIfChanged(ref _selectedHistory, value);
                    SelectedMethod = _selectedHistory.HttpMethod;
                    Params = new ObservableCollection<Param>();
                    foreach (var item in _selectedHistory.Params)
                    {
                        Params.Add(item);
                    }
                    SelectedAuthType = _selectedHistory.AuthorizationType;
                    AuthUsername = _selectedHistory.AuthUserName;
                    AuthPassword = _selectedHistory.AuthPassword;
                    AuthBearer = _selectedHistory.AuthBearerToken;
                    RequestBody = _selectedHistory.RequestRawBody;
                    ResponseBodyJson = _selectedHistory.Json;
                    ResponseBodyXml = _selectedHistory.Xml;
                    ResponseBodyRaw = _selectedHistory.Raw;
                    Url = _selectedHistory.Url;
                    IsSaveInHistory = false;
                }
            }
        }

        public IManagedNotificationManager NotificationManager
        {
            get { return _notificationManager; }
            set { this.RaiseAndSetIfChanged(ref _notificationManager, value); }
        }

        public ReactiveCommand<Unit, Unit> SaveModifiedResponses { get; }
        public ReactiveCommand<Unit, Unit> CopyResponseToCollection { get; }
        public ReactiveCommand<Unit, Unit> DeleteSelectedResponses { get; }
        public ReactiveCommand<string, Unit> SendRequest { get; }
        public ReactiveCommand<Unit, Unit> AddNewParam { get; }

        public MainWindowViewModel(IManagedNotificationManager notificationManager)
        {
            Headers = new ObservableCollection<Param>();
            Params = new ObservableCollection<Param>
            {
                new Param()
            };

            // Load all the UI data from async method for faster startups
            Task.Run(() => LoadAsync());

            SaveModifiedResponses = ReactiveCommand.Create(DoSaveModifiedResponses);
            CopyResponseToCollection = ReactiveCommand.Create(DoCopyResponseToCollection);
            DeleteSelectedResponses = ReactiveCommand.Create(DoDeleteSelectedResponses);

            SendRequest = ReactiveCommand.Create<string>(DoSendRequest);
            AddNewParam = ReactiveCommand.Create(DoAddNewParam);

            _notificationManager = notificationManager;
        }

        async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                // Load all Http request methods and choose the first one as the default normally should be GET
                try
                {
                    HttpMethods = new ObservableCollection<string>(typeof(HttpMethod).GetProperties().Where(x => x.IsStatic()).Select(x => x.Name));
                    SelectedMethod = HttpMethods.First();
                }
                catch
                {
                    HttpMethods = new ObservableCollection<string>(typeof(HttpMethod).GetProperties().Where(x => x.IsStatic()).Select(x => x.Name));
                    SelectedMethod = HttpMethods.FirstOrDefault();
                }

                AuthorizationTypes = new ObservableCollection<AuthorizationType>(Enum.GetValues(typeof(AuthorizationType)).Cast<AuthorizationType>());
                SelectedAuthType = AuthorizationTypes.FirstOrDefault();

                using (var db = new LiteDatabase(DbConfig.ConnectionString))
                {
                    History = new ObservableCollection<Response>(db.GetCollection<Response>(DbConfig.ResponseCollection).FindAll().OrderByDescending(x => x.ModifiedAt));
                    HistoryCollection = new ObservableCollection<ResponsesList>(db.GetCollection<ResponsesList>(DbConfig.HistoryCollection).Find(x => x.ParentId == 0).OrderByDescending(x => x.ModifiedAt));
                    //db.GetCollection<ResponsesList>(DbConfig.HistoryCollection).
                    //Insert(new ResponsesList
                    //{
                    //    Name = "Root Node",
                    //    CreateAt = DateTime.Now,
                    //    Responses = new ObservableCollection<Response>
                    //    {
                    //        new Response
                    //        {
                    //            Url = "fdsfsd.com",
                    //            HttpMethod = "Get",
                    //            CreateAt = DateTime.Now
                    //        }
                    //    }
                    //});
                }

                Tree = new ObservableCollection<CollectionNodeViewModel>();

                foreach (var item in HistoryCollection)
                {
                    Tree.Add(new CollectionNodeViewModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Notes = item.Notes,
                        IsFolder = true,
                    });
                }
            });
        }

        private async void DoSendRequest(string url)
        {
            if (!isSending && CheckUrlValid(url))
            {
                isSending = true;
                ResponseBodyJson = ResponseBodyXml = ResponseBodyRaw = string.Empty;

                HttpClient client = new HttpClient();

                var newResponse = new Response
                {
                    Url = Url,
                    HttpMethod = SelectedMethod
                };

                // Paramaters
                if (Params.Count > 0)
                {
                    var headerParams = new Dictionary<string, string>();
                    ParamsListToDictionaryHelper.ListToDictionary(Params, headerParams);

                    foreach (var item in Params)
                    {
                        newResponse.Params.Add(item);
                    }

                    url = url.SetQueryParams(headerParams);
                }

                // Authorization
                newResponse.AuthorizationType = SelectedAuthType;
                switch (SelectedAuthType)
                {
                    case AuthorizationType.Basic:
                        {
                            newResponse.AuthUserName = AuthUsername;
                            newResponse.AuthPassword = AuthPassword;
                            client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue(AuthorizationType.Basic.ToString(),
                            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{AuthUsername}:{AuthPassword}")));
                            break;
                        }
                    case AuthorizationType.Bearer:
                        {
                            newResponse.AuthBearerToken = AuthBearer;
                            client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue(AuthorizationType.Bearer.ToString(), AuthBearer);
                            break;
                        }
                }

                // Body
                var httpContent = new StringContent(string.Empty);
                if (!string.IsNullOrWhiteSpace(RequestBody))
                {
                    newResponse.RequestBodyType = BodyType.Json;
                    newResponse.RequestRawBody = RequestBody;

                    // Serialize the body into a JSON String
                    var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(RequestBody));
                    // Wrap our JSON inside a StringContent which then can be used by the HttpClient class to be send with Post, Put or Patch
                    httpContent = new StringContent(stringPayload, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
                }

                string responseBody = string.Empty;

                try
                {
                    // Http Request Methods
                    switch (SelectedMethod)
                    {
                        case "Get":
                            {
                                HttpResponseMessage response = await client.GetAsync(url);
                                response.EnsureSuccessStatusCode();
                                responseBody = await response.Content.ReadAsStringAsync();
                                break;
                            }
                        case "Post":
                            {
                                HttpResponseMessage response = await client.PostAsync(url, httpContent);
                                response.EnsureSuccessStatusCode();
                                responseBody = await response.Content.ReadAsStringAsync();
                                break;
                            }
                        case "Put":
                            {
                                HttpResponseMessage response = await client.PutAsync(url, httpContent);
                                response.EnsureSuccessStatusCode();
                                responseBody = await response.Content.ReadAsStringAsync();
                                break;
                            }
                        case "Delete":
                            {
                                HttpResponseMessage response = await client.DeleteAsync(url);
                                response.EnsureSuccessStatusCode();
                                responseBody = await response.Content.ReadAsStringAsync();
                                break;
                            }
                        case "PATCH":
                            {
                                HttpResponseMessage response = await client.PatchAsync(url, httpContent);
                                response.EnsureSuccessStatusCode();
                                responseBody = await response.Content.ReadAsStringAsync();
                                break;
                            }
                        case "HEAD":
                            {
                                break;
                            }
                        case "CONNECT":
                            {
                                break;
                            }
                        case "OPTIONS":
                            {
                                break;
                            }
                        case "TRACE":
                            {
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    isErrorInRequestExist = true;
                    ResponseBodyRaw = ex.Message;

                    NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", "There is a problem with your request, please check out Raw tab.", NotificationType.Error));
                }

                await Task.Run(() =>
                {
                    // Check if the response is json then Try to format it as one
                    bool isValidJson = responseBody.IsValidJson(out JToken outJson);
                    try
                    {
                        if (isValidJson)
                        {
                            ResponseBodyJson = outJson.ToString(Newtonsoft.Json.Formatting.Indented);
                            newResponse.Json = ResponseBodyJson;
                        }
                        else
                        {
                            ResponseBodyJson = "No valid Json returned, check XML or Raw";
                        }
                    }
                    catch (Exception ex)
                    {
                        ResponseBodyJson = $"Please look at Raw. {Environment.NewLine} {ex.Message}";
                        isErrorInResponseExist = true;
                    }

                    // check if the response is xml or get it from json then Try to format it as XML
                    try
                    {
                        if (responseBody.IsValidXML(out XmlDocument outXml))
                        {
                            ResponseBodyXml = outXml.AsString();
                            newResponse.Xml = ResponseBodyXml;
                        }
                        else if (isValidJson)
                        {
                            ResponseBodyXml = XmlExtensions.DeserializeXmlNode(responseBody, "root", "array").AsString();
                            newResponse.Xml = ResponseBodyXml;
                        }
                        else
                        {
                            ResponseBodyXml = "No valid Xml returned, check Json or Raw";
                        }
                    }
                    catch (Exception ex)
                    {
                        ResponseBodyXml = $"Please look at Raw. {Environment.NewLine} {ex.Message}";
                        isErrorInResponseExist = true;
                    }
                });

                // show response or error as is
                try
                {
                    ResponseBodyRaw = responseBody;
                    newResponse.Raw = ResponseBodyRaw;
                }
                catch (Exception ex)
                {
                    ResponseBodyXml = $"There is a problem with your request. {Environment.NewLine} {ex.Message}";
                    isErrorInResponseExist = true;
                }

                // Insert the new request into the database and the history list
                try
                {
                    if (IsSaveInHistory)
                    {
                        using var db = new LiteDatabase(DbConfig.ConnectionString);
                        var col = db.GetCollection<Response>(DbConfig.ResponseCollection);
                        newResponse.ModifiedAt = DateTime.Now;
                        col.Insert(newResponse);

                        History.Add(newResponse);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    if (!isErrorInRequestExist)
                    {
                        if (isErrorInResponseExist)
                        {
                            NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Warning", "Sent the request but there are some errors.", NotificationType.Warning));
                        }
                        else
                        {
                            NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Done", "Sent a request and got a response back.", NotificationType.Success));
                        }
                    }
                    isSending = false;
                }
            }
            else
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Info", "Already sending a request or there a problem with your URL.", NotificationType.Information));
            }
        }

        /// <summary>
        /// Get all items in the history and save the Modified Ones
        /// </summary>
        private async void DoSaveModifiedResponses()
        {
            if (!isSavingHistory)
            {
                isSavingHistory = true;
                try
                {
                    using var db = new LiteDatabase(DbConfig.ConnectionString);
                    var col = db.GetCollection<Response>(DbConfig.ResponseCollection);

                    await Task.Run(() =>
                    {
                        foreach (var item in History.Where(x => x.IsModified == true))
                        {
                            item.IsModified = false;
                            col.Update(item);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    isErrorInSaveHistoryExist = true;
                }
                finally
                {
                    if (isErrorInSaveHistoryExist)
                    {
                        NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Warning", "Saved Some but can't save others so we got error.", NotificationType.Warning));
                    }
                    else
                    {
                        NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Done", "Everything is saved now.", NotificationType.Success));
                    }
                    isSavingHistory = false;
                    isErrorInSaveHistoryExist = false;
                }
            }
        }

        /// <summary>
        /// Adds selected Responses to a Collection in the collection tab
        /// </summary>
        private void DoCopyResponseToCollection()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the selected Responses from the history list
        /// </summary>
        private async void DoDeleteSelectedResponses()
        {
            if (!isDeletingHistory)
            {
                isDeletingHistory = true;
                try
                {
                    using var db = new LiteDatabase(DbConfig.ConnectionString);
                    var col = db.GetCollection<Response>(DbConfig.ResponseCollection);

                    var toBeDeleted = History.Where(x => x.IsSelected == true);

                    await Task.Run(() =>
                    {
                        foreach (var item in toBeDeleted)
                        {
                            col.Delete(item.Id);
                        }
                    });
                    History.RemoveMany(toBeDeleted);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    isErrorInDeleteHistoryExist = true;
                }
                finally
                {
                    if (isErrorInDeleteHistoryExist)
                    {
                        NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Warning", "Delete Some but can't delete others so we got error.", NotificationType.Warning));
                    }
                    else
                    {
                        NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Done", "Everything is deleted now.", NotificationType.Success));
                    }
                    isDeletingHistory = false;
                    isErrorInDeleteHistoryExist = false;
                }
            }
        }

        /// <summary>
        /// Param add button
        /// </summary>
        private void DoAddNewParam()
        {
            Params.Add(new Param { IsSelected = true });
        }

        /// <summary>
        /// checks if the user url is vaild
        /// </summary>
        /// <param name="url">url as string</param>
        /// <returns>true is real url</returns>
        private static bool CheckUrlValid(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            try
            {
                Uri uriResult = new Uri(url);
                return Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
