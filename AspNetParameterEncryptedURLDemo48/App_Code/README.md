# URL Security Helper – ASP.NET Web Forms / .NET Framework 4.8

This helper covers URL cases that are not automatically handled by SecurePage/SecureControlProcessor:

- Response.Redirect()
- JavaScript URLs
- AJAX URLs
- Raw `<a href="">`
- HTML generated as literal strings

## Basic rule

Use `SecureUrlHelper.Secure(...)` whenever a URL is constructed outside an ASP.NET server control.

Examples:

```csharp
Response.Redirect(SecureUrlHelper.Secure(
    "Patient.aspx?PatientID=" + patientId
));
```

```csharp
string url = SecureUrlHelper.Secure(
    "Patient.aspx?PatientID=" + patientId
);

ClientScript.RegisterStartupScript(
    GetType(),
    "openPatient",
    "window.location.href=" + HttpUtility.JavaScriptStringEncode(url, true) + ";",
    true
);
```

For AJAX, secure the URL on the server and return it to the client. Do not try to encrypt a URL independently in JavaScript.

For raw HTML, use `SecureUrlHelper.Secure(...)` before inserting the URL into the HTML.

IMPORTANT:
- This helper assumes the `UrlSecurity.cs` and `SecureUrl.cs` classes from the main implementation are already installed.
- Encryption protects confidentiality/integrity of URL parameters; it does not replace authorization.
- Never put encryption keys in JavaScript.
- For a HIMS, authorization must still be checked after the parameters are decrypted.
