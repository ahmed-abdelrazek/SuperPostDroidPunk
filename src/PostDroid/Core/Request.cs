using System.ComponentModel.DataAnnotations;

namespace SuperPostDroidPunk.Core
{
    public enum AuthorizationType : byte
    {
        [Display(Name = "No Auth")]
        None,
        [Display(Name = "Basic Auth")]
        Basic,
        [Display(Name = "Bearer Token")]
        Bearer
    }

    public enum BodyType : byte
    {
        [Display(Name = "none")]
        None,
        [Display(Name = "form-data")]
        FormData,
        [Display(Name = "x-www-form-urlencoded")]
        XWwwFormUrlEncoded,
        [Display(Name = "json")]
        Json,
        [Display(Name = "xml")]
        XML,
        [Display(Name = "text")]
        Text,
    }

    public enum HTTPRequestMethod : byte
    {
        /// <summary>
        /// The GET method requests a representation of the specified resource.
        /// </summary>
        [Display(Name = "GET")]
        get,
        /// <summary>
        /// The HEAD method asks for a response identical to that of a GET request, but without the response body.
        /// </summary>
        [Display(Name = "HEAD")]
        head,
        /// <summary>
        /// The POST method is used to submit an entity to the specified resource, often causing a change in state or side effects on the server.
        /// </summary>
        [Display(Name = "POST")]
        post,
        /// <summary>
        /// The PUT method replaces all current representations of the target resource with the request payload.
        /// </summary>
        [Display(Name = "PUT")]
        put,
        /// <summary>
        /// The DELETE method deletes the specified resource.
        /// </summary>
        [Display(Name = "DELETE")]
        delete,
        /// <summary>
        /// The CONNECT method establishes a tunnel to the server identified by the target resource.
        /// </summary>
        [Display(Name = "CONNECT")]
        connect,
        /// <summary>
        /// The OPTIONS method is used to describe the communication options for the target resource.
        /// </summary>
        [Display(Name = "OPTIONS")]
        options,
        /// <summary>
        /// The TRACE method performs a message loop-back test along the path to the target resource.
        /// </summary>
        [Display(Name = "TRACE")]
        trace,
        /// <summary>
        /// The PATCH method is used to apply partial modifications to a resource.
        /// </summary>
        [Display(Name = "PATCH")]
        patch
    }
}
