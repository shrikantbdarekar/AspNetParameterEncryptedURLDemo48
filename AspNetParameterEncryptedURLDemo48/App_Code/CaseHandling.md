# URL Security – Case Handling Guide

## A. Response.Redirect()

### Before

```csharp
Response.Redirect(
    "Patient.aspx?PatientID=" + patientId);
```

### After

```csharp
Response.Redirect(
    SecureUrlHelper.Secure(
        "Patient.aspx?PatientID=" + patientId));
```

Recommended for new code:

```csharp
Response.Redirect(
    SecureUrlHelper.Secure(
        "Patient.aspx",
        "PatientID", patientId.ToString(),
        "VisitID", visitId.ToString()),
    false);

Context.ApplicationInstance.CompleteRequest();
```

---

## B. JavaScript URL

Never do encryption in browser JavaScript.

### Before

```javascript
window.location.href =
    "Patient.aspx?PatientID=123";
```

### Server-generated secure URL

```csharp
string url =
    SecureUrlHelper.JavaScriptUrl(
        "Patient.aspx?PatientID=" + patientId);
```

Then:

```csharp
string script =
    "window.location.href=" + url + ";";
```

This produces a JavaScript-safe string literal.

---

## C. AJAX URL

The browser should receive an already secured URL.

### Server

```csharp
string url =
    SecureUrlHelper.Secure(
        "PatientData.aspx?PatientID=" + patientId);
```

Return the URL in your JSON:

```json
{
    "url": "PatientData.aspx?token=..."
}
```

### JavaScript

```javascript
$.ajax({
    url: response.url,
    type: "GET"
});
```

Do not expose AES/HMAC keys to JavaScript.

---

## D. Raw `<a href="">`

### Before

```csharp
string html =
    "<a href=\"Patient.aspx?PatientID=" +
    patientId +
    "\">Open</a>";
```

### After

```csharp
string url =
    SecureUrlHelper.HtmlAttributeUrl(
        "Patient.aspx?PatientID=" + patientId);

string html =
    "<a href=\"" +
    url +
    "\">Open</a>";
```

---

## E. Literal HTML

Any URL inserted into generated HTML should be:

1. Secured
2. HTML attribute encoded

Use:

```csharp
SecureUrlHelper.HtmlAttributeUrl(url)
```

Do not simply do:

```csharp
HttpUtility.HtmlEncode(url)
```

because that only encodes the URL; it does not secure the query parameters.

---

## F. URL inside JavaScript embedded in HTML

Use:

```csharp
SecureUrlHelper.JavaScriptUrl(url)
```

Example:

```csharp
string url =
    SecureUrlHelper.JavaScriptUrl(
        "Patient.aspx?PatientID=" + patientId);

string html =
    "<button onclick=\"window.location.href=" +
    url +
    "\">Open</button>";
```

---

## G. URLs in data attributes

### Server

```csharp
string url =
    SecureUrlHelper.HtmlAttributeUrl(
        "Patient.aspx?PatientID=" + patientId);

string html =
    "<button data-url=\"" +
    url +
    "\">Open</button>";
```

### JavaScript

```javascript
location.href = button.dataset.url;
```

---

## H. URLs returned from WebMethod / PageMethods

Secure them on the server:

```csharp
[System.Web.Services.WebMethod]
public static string GetPatientUrl(int patientId)
{
    return SecureUrlHelper.Secure(
        "Patient.aspx?PatientID=" +
        patientId);
}
```

Client:

```javascript
PageMethods.GetPatientUrl(
    patientId,
    function(url) {
        window.location.href = url;
    }
);
```

---

## I. Important: AJAX data URLs

If the AJAX request itself contains:

```text
PatientData.aspx?PatientID=123
```

secure it:

```csharp
string url =
    SecureUrlHelper.Secure(
        "PatientData.aspx?PatientID=" + patientId);
```

But if the request is POST JSON such as:

```json
{
    "patientId": 123
}
```

this URL-security mechanism does not apply. You should use normal request validation, authentication, authorization, HTTPS, and server-side validation.

---

## J. Do not double-encrypt

`SecureUrl.Create()` already checks whether the URL contains:

```text
token=...
```

So calling:

```csharp
SecureUrlHelper.Secure(
    SecureUrlHelper.Secure(url))
```

should not create another encrypted token.

Still, avoid unnecessary repeated processing.

---

## K. External URLs

The helper is intended for internal application URLs.

Do not encrypt:

```text
https://google.com/...
https://maps.google.com/...
```

The helper leaves external absolute URLs unchanged.

---

## L. Authorization is still required

Encrypted:

```text
Patient.aspx?token=...
```

does NOT mean the logged-in user is allowed to access that patient.

After decryption:

```csharp
string patientId =
    Request.QueryString["PatientID"];
```

your application must still verify:

```csharp
UserCanAccessPatient(patientId)
```

before displaying the record.

---

## M. Search your existing project

For migration, search your solution for:

```text
Response.Redirect(
NavigateUrl=
href=
window.location
window.open(
location.href
$.ajax(
$.get(
$.post(
ajax(
PageMethods.
RegisterStartupScript(
RegisterClientScriptBlock(
"<a
"<button
data-url=
```

These are the main places to review for URLs that will not be covered by automatic server-control processing.
