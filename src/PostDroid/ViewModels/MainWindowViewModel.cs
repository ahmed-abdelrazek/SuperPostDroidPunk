using Avalonia.Media;
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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private bool _isNotificationVisible;
        private SolidColorBrush _notificationBrush;
        private string _notificationMessage;
        private ObservableCollection<CollectionNodeViewModel> _tree;

        public ObservableCollection<string> HttpMethods { get; set; }

        public string SelectedMethod { get => _selectedMethod; set => this.RaiseAndSetIfChanged(ref _selectedMethod, value); }

        public string Url { get => _url; set => this.RaiseAndSetIfChanged(ref _url, value); }

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
                }
            }
        }

        public bool IsNotificationVisible
        {
            get => _isNotificationVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isNotificationVisible, value);
                if (_isNotificationVisible)
                {
                    new Thread(() =>
                    {
                        Thread.Sleep(5000);
                        IsNotificationVisible = false;
                    }).Start();
                }
            }
        }

        public SolidColorBrush NotificationBrush { get => _notificationBrush; set => this.RaiseAndSetIfChanged(ref _notificationBrush, value); }

        public string NotificationMessage { get => _notificationMessage; set => this.RaiseAndSetIfChanged(ref _notificationMessage, value); }

        public ReactiveCommand<Unit, Unit> SaveModifiedResponses { get; }
        public ReactiveCommand<Unit, Unit> CopyResponseToCollection { get; }
        public ReactiveCommand<Unit, Unit> DeleteSelectedResponses { get; }
        public ReactiveCommand<string, Unit> SendRequest { get; }
        public ReactiveCommand<Unit, Unit> AddNewParam { get; }


        public MainWindowViewModel()
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
                    HistoryCollection = new ObservableCollection<ResponsesList>(db.GetCollection<ResponsesList>(DbConfig.HistoryCollection).FindAll().OrderByDescending(x => x.ModifiedAt));
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

                    NotificationBrush = new SolidColorBrush(Color.Parse("#BD202C"), 0.75);
                    NotificationMessage = "Errors with Request see Raw.";
                }

                // Try to format the response as Json
                try
                {
                    ResponseBodyJson = JToken.Parse(responseBody).ToString(Formatting.Indented);
                    newResponse.Json = ResponseBodyJson;
                }
                catch (Exception ex)
                {
                    ResponseBodyJson = $"Please look at Raw. {Environment.NewLine} {ex.Message}";
                    isErrorInResponseExist = true;
                }

                // Try to format the response as XML
                try
                {
                    ResponseBodyXml = XmlExtensions.DeserializeXmlNode(responseBody, "root", "array").AsString();
                    newResponse.Xml = ResponseBodyXml;
                }
                catch (Exception ex)
                {
                    ResponseBodyXml = $"Please look at Raw. {Environment.NewLine} {ex.Message}";
                    isErrorInResponseExist = true;
                }

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
                    using var db = new LiteDatabase(DbConfig.ConnectionString);
                    var col = db.GetCollection<Response>(DbConfig.ResponseCollection);
                    newResponse.ModifiedAt = DateTime.Now;
                    col.Insert(newResponse);

                    History.Add(newResponse);
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
                            NotificationBrush = new SolidColorBrush(Color.Parse("#FDB328"), 0.75);
                            NotificationMessage = "Sent But with errors see Raw.";
                        }
                        else
                        {
                            NotificationBrush = new SolidColorBrush(Color.Parse("#1F9E45"), 0.75);
                            NotificationMessage = "Sent and came home.";
                        }
                    }
                    IsNotificationVisible = true;
                    isSending = false;
                }
            }
            else
            {
                NotificationBrush = new SolidColorBrush(Color.Parse("#BD202C"), 0.75);
                NotificationMessage = "Already sending or Url is empty/invalid.";
                IsNotificationVisible = true;
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
                        NotificationBrush = new SolidColorBrush(Color.Parse("#FDB328"), 0.75);
                        NotificationMessage = "Saved Some but got error.";
                    }
                    else
                    {
                        NotificationBrush = new SolidColorBrush(Color.Parse("#1F9E45"), 0.75);
                        NotificationMessage = "Saving Done.";
                    }
                    IsNotificationVisible = true;
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
                        NotificationBrush = new SolidColorBrush(Color.Parse("#FDB328"), 0.75);
                        NotificationMessage = "Deleted Some but got error.";
                    }
                    else
                    {
                        NotificationBrush = new SolidColorBrush(Color.Parse("#1F9E45"), 0.75);
                        NotificationMessage = "Deleteing Done.";
                    }
                    IsNotificationVisible = true;
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
