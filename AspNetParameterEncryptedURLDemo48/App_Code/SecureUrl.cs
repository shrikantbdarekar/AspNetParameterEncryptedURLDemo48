using System;
using System.Collections.Specialized;
using System.Web;

public static class SecureUrl
{
    public static string Create(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;

        Uri uri;

        if (!Uri.TryCreate(
            url,
            UriKind.RelativeOrAbsolute,
            out uri))
        {
            return url;
        }

        // Don't process absolute external URLs
        if (uri.IsAbsoluteUri &&
            !string.Equals(
                uri.Host,
                HttpContext.Current.Request.Url.Host,
                StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        string path = uri.IsAbsoluteUri
            ? uri.AbsolutePath
            : url.Split('?')[0];

        string query = uri.IsAbsoluteUri
            ? uri.Query
            : GetQueryString(url);

        string fragment = uri.IsAbsoluteUri
            ? uri.Fragment
            : GetFragment(url);

        if (string.IsNullOrEmpty(query))
            return url;

        NameValueCollection parameters =
            HttpUtility.ParseQueryString(
                query.TrimStart('?'));

        if (parameters.Count == 0)
            return url;

        // Already secured
        if (!string.IsNullOrEmpty(
            parameters[UrlSecurity.TokenParameter]))
        {
            return url;
        }

        string token =
            UrlSecurity.EncryptParameters(parameters);

        string result =
            path +
            "?" +
            UrlSecurity.TokenParameter +
            "=" +
            HttpUtility.UrlEncode(token);

        if (!string.IsNullOrEmpty(fragment))
            result += fragment;

        return result;
    }

    private static string GetQueryString(string url)
    {
        int question = url.IndexOf('?');

        if (question < 0)
            return string.Empty;

        int hash = url.IndexOf('#');

        if (hash >= 0 && hash > question)
        {
            return url.Substring(
                question,
                hash - question);
        }

        return url.Substring(question);
    }

    private static string GetFragment(string url)
    {
        int hash = url.IndexOf('#');

        if (hash < 0)
            return string.Empty;

        return url.Substring(hash);
    }
}