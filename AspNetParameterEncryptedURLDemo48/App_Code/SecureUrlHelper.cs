using System;
using System.Web;

/// <summary>
/// Helper for URLs that are not automatically handled by
/// SecureControlProcessor / SecurePage.
///
/// ASP.NET Web Forms / .NET Framework 4.8
/// </summary>
public static class SecureUrlHelper
{
    /// <summary>
    /// Encrypts the query string of an internal URL.
    /// If there is no query string, the original URL is returned.
    /// External absolute URLs are returned unchanged.
    /// </summary>
    public static string Secure(string url)
    {
        return SecureUrl.Create(url);
    }

    /// <summary>
    /// Creates a secure URL from a page and query-string values.
    ///
    /// Example:
    /// Secure("Patient.aspx", "PatientID", "123", "VisitID", "456")
    /// </summary>
    public static string Secure(
        string page,
        params string[] keyValuePairs)
    {
        if (string.IsNullOrWhiteSpace(page))
            throw new ArgumentNullException("page");

        if (keyValuePairs == null ||
            keyValuePairs.Length % 2 != 0)
        {
            throw new ArgumentException(
                "keyValuePairs must contain key/value pairs.");
        }

        var query = HttpUtility.ParseQueryString(string.Empty);

        for (int i = 0; i < keyValuePairs.Length; i += 2)
        {
            string key = keyValuePairs[i];
            string value = keyValuePairs[i + 1];

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(
                    "Query-string key cannot be empty.");

            query[key] = value ?? string.Empty;
        }

        string queryString = query.ToString();

        if (string.IsNullOrEmpty(queryString))
            return page;

        return SecureUrl.Create(
            page + "?" + queryString);
    }

    /// <summary>
    /// Returns a JavaScript-safe string literal containing the secure URL.
    /// Useful when writing window.location, window.open, etc.
    ///
    /// Example:
    /// string jsUrl = SecureUrlHelper.JavaScriptUrl(
    ///     "Patient.aspx?PatientID=123");
    /// </summary>
    public static string JavaScriptUrl(string url)
    {
        string secureUrl = Secure(url);

        return HttpUtility.JavaScriptStringEncode(
            secureUrl,
            true);
    }

    /// <summary>
    /// Returns an HTML attribute-safe secure URL.
    ///
    /// Example:
    /// string htmlUrl = SecureUrlHelper.HtmlAttributeUrl(
    ///     "Patient.aspx?PatientID=123");
    /// </summary>
    public static string HtmlAttributeUrl(string url)
    {
        string secureUrl = Secure(url);

        return HttpUtility.HtmlAttributeEncode(
            secureUrl);
    }

    /// <summary>
    /// Creates an HTML href attribute.
    ///
    /// Example output:
    /// href="Patient.aspx?token=..."
    /// </summary>
    public static string HtmlHref(string url)
    {
        return "href=\"" +
               HtmlAttributeUrl(url) +
               "\"";
    }
}
