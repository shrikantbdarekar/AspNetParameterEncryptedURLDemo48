using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;

public class SecurePage : Page
{
    private const string ProcessingKey =
        "__URL_SECURITY_PROCESSED";

    protected override void OnPreLoad(EventArgs e)
    {
        ProcessIncomingRequest();

        base.OnPreLoad(e);
    }

    protected override void OnPreRender(EventArgs e)
    {
        SecureControlProcessor.Process(this);

        base.OnPreRender(e);
    }

    private void ProcessIncomingRequest()
    {
        if (Context.Items[ProcessingKey] != null)
            return;

        Context.Items[ProcessingKey] = true;

        string token =
            Request.QueryString[
                UrlSecurity.TokenParameter];

        if (string.IsNullOrWhiteSpace(token))
            return;

        try
        {
            NameValueCollection parameters =
                UrlSecurity.DecryptParameters(token);

            if (parameters.Count == 0)
                throw new Exception(
                    "URL token contains no parameters.");

            string queryString =
                BuildQueryString(parameters);

            string path =
                Request.AppRelativeCurrentExecutionFilePath;

            string pathInfo =
                Request.PathInfo;

            // Rewrite request internally so existing code
            // can continue using Request.QueryString["X"]
            Context.RewritePath(
                path,
                pathInfo,
                queryString,
                false);
        }
        catch
        {
            HandleInvalidToken();
        }
    }

    private string BuildQueryString_Old(
        NameValueCollection parameters)
    {
        return HttpUtility.UrlEncode(
            parameters.ToString());
    }

    private string BuildQueryString(
    NameValueCollection parameters)
    {
        System.Text.StringBuilder query =
            new System.Text.StringBuilder();

        foreach (string key in parameters.AllKeys)
        {
            if (string.IsNullOrEmpty(key))
                continue;

            string[] values =
                parameters.GetValues(key);

            if (values == null)
                continue;

            foreach (string value in values)
            {
                if (query.Length > 0)
                    query.Append("&");

                query.Append(
                    HttpUtility.UrlEncode(key));

                query.Append("=");

                query.Append(
                    HttpUtility.UrlEncode(
                        value ?? string.Empty));
            }
        }

        return query.ToString();
    }
    private void HandleInvalidToken()
    {
        Response.Clear();
        Response.StatusCode = 400;
        Response.StatusDescription =
            "Invalid URL security token";

        Response.End();
    }
}