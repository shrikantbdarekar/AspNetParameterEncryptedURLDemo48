# ASP.NET Encrypted URL Parameters

A drop-in security library for **ASP.NET Web Forms (.NET Framework 4.8)** that encrypts query-string parameters into a single tamper-proof token. Sensitive data like IDs, emails, and amounts never appear in plain text in the browser URL.

## The Problem

Query strings are visible in the browser address bar, server logs, proxy logs, and Referer headers:

```
Patient.aspx?PatientID=12345&SSN=123-45-6789
```

Anyone who sees this URL can modify the parameters and replay the request (IDOR attacks).

## The Solution

Replace plain query strings with a single encrypted `token`:

```
Patient.aspx?token=t92hz_GhdoTPVF8-REq92C9mUjEYjIjbyscpkDAeA9d...
```

- Parameters are encrypted with **AES-256-CBC** and signed with **HMAC-SHA256**
- Tokens are **tamper-proof** -- any modification invalidates the signature
- Tokens are **unreadable** -- parameters are not exposed in the URL
- Existing code continues using `Request.QueryString["X"]` unchanged

## How It Works

```
  Page renders                  User clicks link             Next request arrives
  ─────────────                 ───────────────              ────────────────────
  HyperLink.NavigateUrl         Browser navigates to         SecurePage.OnPreLoad
  = "Patient.aspx?id=5"         "Patient.aspx?token=..."     decrypts token,
                                                             rewrites query string

  SecureControlProcessor       Encryption applied            Request.QueryString["id"]
  encrypts on OnPreRender       automatically by pipeline    == "5" (transparent)
```

### Outgoing (Encryption)

`SecurePage.OnPreRender` calls `SecureControlProcessor.Process()` which walks the control tree and encrypts:

| Control Type | Property Encrypted |
|---|---|
| `<asp:HyperLink>` | `NavigateUrl` |
| `<a runat="server">` | `HRef` |

### Incoming (Decryption)

`SecurePage.OnPreLoad` reads the `token` query parameter, decrypts it, and rewrites the request via `Context.RewritePath()` so `Request.QueryString["paramName"]` works normally.

## Core Classes

| Class | Purpose |
|---|---|
| `SecurePage` | Base page class. Handles decrypt on load, encrypt on render. |
| `SecureControlProcessor` | Walks control tree, encrypts HyperLink/HtmlAnchor URLs. |
| `SecureUrl` | Encrypts a URL string, preserving path and fragment. |
| `SecureUrlHelper` | High-level helpers for JS, HTML, and raw URLs. |
| `UrlSecurity` | AES encryption/decryption, HMAC validation, token format. |

## Quick Start

### 1. Inherit from SecurePage

```csharp
// Before
public partial class PatientDetail : Page

// After
public partial class PatientDetail : SecurePage
```

### 2. Use HyperLink controls (auto-encrypted)

```aspx
<asp:HyperLink runat="server"
    NavigateUrl="Patient.aspx?PatientID=123&VisitID=456"
    Text="View Patient" />
```

Rendered HTML automatically becomes:

```html
<a href="Patient.aspx?token=t92hz_GhdoTPVF8-...">View Patient</a>
```

### 3. Read parameters normally

```csharp
protected void Page_Load(object sender, EventArgs e)
{
    string patientId = Request.QueryString["PatientID"]; // "123"
    string visitId = Request.QueryString["VisitID"];     // "456"
}
```

No decryption code needed -- `SecurePage` handles it transparently.

## Scenarios Not Auto-Encrypted

For URLs outside HyperLink/HtmlAnchor controls, use `SecureUrlHelper`:

### JavaScript (`window.location`, `window.open`)

```csharp
// In code-behind -- store encrypted URL in HiddenField
hfUrl.Value = SecureUrlHelper.Secure(
    "Patient.aspx?PatientID=123&Name=John");

// In markup -- read from HiddenField client-side
<script>
    window.location.href = document.getElementById('<%= hfUrl.ClientID %>').value;
</script>
```

> **Note:** `SecureUrlHelper.JavaScriptUrl()` wraps the URL in quotes as a JS string literal. Use `SecureUrlHelper.Secure()` when storing in HiddenFields.

### AJAX (`fetch`, `XMLHttpRequest`)

```csharp
// In code-behind
hfAjaxUrl.Value = SecureUrlHelper.Secure(
    "api/PatientData.aspx?PatientID=123&ajax=true");
```

```javascript
// Client-side
fetch(document.getElementById('<%= hfAjaxUrl.ClientID %>').value)
    .then(r => r.text())
    .then(html => document.getElementById('result').innerHTML = html);
```

### Literal HTML (`<a href>` in string)

```csharp
string html = "<a href=\"" +
    SecureUrlHelper.HtmlAttributeUrl("Patient.aspx?PatientID=123") +
    "\">Open Patient</a>";
```

### Building URLs from code

```csharp
string url = SecureUrlHelper.Secure(
    "Patient.aspx",
    "PatientID", "123",
    "VisitID", "456");
// => "Patient.aspx?token=..."
```

## Configuration

Encryption keys in `Web.config`:

```xml
<appSettings>
    <add key="UrlSecurity.EncryptionKey"
         value="3/ljDp65WwLgdv/TPq+7xK5kplacaqLlRS8TRrR+kSs=" />
    <add key="UrlSecurity.ValidationKey"
         value="rcCxym3VpN5XQ0zsF9iYsHO5qZxokalNLKwxJsV5CUBbHQlQn41b2kBINAJw5ApHG89Uhzx5PI5GI6XhiiW9CA==" />
</appSettings>
```

| Key | Requirements |
|---|---|
| `UrlSecurity.EncryptionKey` | Exactly 32 bytes (256-bit), Base64-encoded |
| `UrlSecurity.ValidationKey` | At least 32 bytes, Base64-encoded (HMAC-SHA256) |

Generate new keys:

```csharp
byte[] encKey = new byte[32];
byte[] valKey = new byte[64];
using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
{
    rng.GetBytes(encKey);
    rng.GetBytes(valKey);
}
string enc = Convert.ToBase64String(encKey);
string val = Convert.ToBase64String(valKey);
```

## Token Format

```
Base64Url( IV[16] + AES-CBC-Ciphertext[...] + HMAC-SHA256[32] )
```

- **IV** -- random 16 bytes per token (unique even for identical parameters)
- **Ciphertext** -- AES-256-CBC with PKCS7 padding
- **HMAC** -- SHA-256 over IV + ciphertext, using validation key
- **Encoding** -- Base64 URL-safe (`+` -> `-`, `/` -> `_`, no padding)

## Security Features

- **AES-256-CBC encryption** -- parameters are not readable in the URL
- **HMAC-SHA256 validation** -- any tampering invalidates the token
- **Random IV** -- identical parameters produce different tokens each time
- **Fixed-time comparison** -- HMAC comparison is resistant to timing attacks
- **External URL protection** -- URLs pointing to other hosts are left unchanged
- **Already-secured detection** -- tokens are not double-encrypted
- **Invalid token handling** -- returns HTTP 400 with descriptive error

## Project Structure

```
App_Code/
    SecurePage.cs              Base page class (decrypt + encrypt)
    SecureControlProcessor.cs  Auto-encrypts HyperLink/HtmlAnchor
    SecureUrl.cs               URL encryption logic
    SecureUrlHelper.cs         High-level helpers (JS, HTML, Secure)
    UrlSecurity.cs             AES/HMAC crypto, token encode/decode
    UsageExamples.cs           Copy-paste code samples

Default.aspx                  Demo page with 10 HyperLinks + JS/AJAX/HTML
DecryptViewer.aspx            Displays decrypted parameter values
```

## Running the Demo

1. Open in Visual Studio 2022
2. Set IIS Express HTTPS port to 44358 (or update `Web.config`)
3. Build and run (`F5`)
4. Click any link on the home page to see encrypted URL in action
5. DecryptViewer.aspx shows the decrypted parameters in a table

## Requirements

- .NET Framework 4.8
- Visual Studio 2022 (or MSBuild 17+)
- IIS Express (for HTTPS)

## License

MIT
